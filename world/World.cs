using Godot;
using System.Collections.Generic;
using System.Linq;

public partial class World : Node3D
{
    private const float BaseCameraSize = 20f;
    private const float DefaultHexRadius = 1f;
    private const int DefaultGridWidth = 9;
    private const int DefaultGridHeight = 7;
    private const float DefaultCameraMoveSpeed = 12f;
    private const float DefaultEdgeScrollDistance = 30f;
    private const float DefaultCameraZoomStep = 2f;
    private const float DefaultMinCameraDistance = 8f;
    private const float DefaultMaxCameraDistance = 40f;
    private const float CameraDragDistanceScale = 0.0025f;
    private static readonly Vector3 InitialCameraTarget = new(7f, 0f, 4f);
    private static readonly Vector2I[] InitialPieceSpawnPositions =
    {
        new(1, 1),
        new(4, 3),
        new(4, 5)
    };
    private static readonly Vector2I[] InitialEnemySpawnPositions =
    {
        new(7, 4),
        new(9, 6)
    };
    private static readonly HashSet<Vector2I> InitialMountainCells = new()
    {
        new(3, 0),
        new(3, 1),
        new(3, 2),
        new(2, 1)
    };
    private static readonly HashSet<Vector2I> InitialWaterCells = new()
    {
        new(3, 4),
        new(4, 4),
        new(5, 4)
    };
    private static readonly NodePath CameraNodePath = "Camera3D";
    private static readonly NodePath UiLayerNodePath = "UILayer";
    private static readonly NodePath EndTurnButtonNodePath = "EndTurnLayer/EndTurnButton";
    private static readonly NodePath BuildPanelNodePath = "UILayer/BuildPanel";

    [Export] public float HexRadius { get; set; } = DefaultHexRadius;
    [Export] public int GridWidth { get; set; } = DefaultGridWidth;
    [Export] public int GridHeight { get; set; } = DefaultGridHeight;
    [Export] public float CameraMoveSpeed { get; set; } = DefaultCameraMoveSpeed;
    [Export] public float EdgeScrollDistance { get; set; } = DefaultEdgeScrollDistance;
    [Export] public float CameraZoomStep { get; set; } = DefaultCameraZoomStep;
    [Export] public float MinCameraDistance { get; set; } = DefaultMinCameraDistance;
    [Export] public float MaxCameraDistance { get; set; } = DefaultMaxCameraDistance;

    [Export] public PackedScene PieceScene { get; set; }
    [Export] public PackedScene EnemyScene { get; set; }
    [Export] public PackedScene TileScene { get; set; }

    private readonly Dictionary<Vector2I, Tile> _tiles = new();
    private readonly List<Piece> _pieces = new();
    private readonly List<Enemy> _enemies = new();
    private List<Enemy> _enemyActionOrder = new();
    private Camera3D _camera;
    private CanvasLayer _uiLayer;
    private Button _endTurnButton;
    private BuildPanel _buildPanel;
    private Piece _activePiece;
    private Vector3 _cameraTarget = InitialCameraTarget;
    private bool _isPanningCamera;
    private Vector2 _lastMousePosition;
    private float _cameraDistance = BaseCameraSize;
    private readonly List<Thing> _things = new();
    private Thing _hoveredThing;
    private bool _enemyTurnInProgress;


    public IReadOnlyDictionary<Vector2I, Tile> Tiles => _tiles;
    public CanvasLayer UiLayer => _uiLayer;

    public bool TryProjectToScreen(Vector3 worldPosition, out Vector2 screenPosition)
    {
        screenPosition = Vector2.Zero;
        if (_camera == null || _camera.IsPositionBehind(worldPosition)) return false;

        screenPosition = _camera.UnprojectPosition(worldPosition);
        return true;
    }

    public override void _Ready()
    {
        CreateEnvironment();
        CreateGrid();
        CreateEndTurnButton();


        foreach (var spawnPosition in InitialPieceSpawnPositions)
        {
            SpawnPiece(spawnPosition, PieceScene);
        }

        foreach (var spawnPosition in InitialEnemySpawnPositions)
        {
            SpawnEnemy(spawnPosition, EnemyScene);
        }

        foreach (var enemy in _enemies)
        {
            enemy.PrepareInitialAttack();
        }
        RefreshEnemyActionOrder();
    }

    public override void _ExitTree()
    {
        _buildPanel?.SaveMap();
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventKey keyEvent && keyEvent.Keycode == Key.Alt)
        {
            SetEnemyActionOrderVisible(keyEvent.Pressed);
        }
    }

    public override void _Process(double delta)
    {
        Vector2 screenDirection = Vector2.Zero;
        if (Input.IsKeyPressed(Key.A) || Input.IsKeyPressed(Key.Left)) screenDirection.X -= 1f;
        if (Input.IsKeyPressed(Key.D) || Input.IsKeyPressed(Key.Right)) screenDirection.X += 1f;
        if (Input.IsKeyPressed(Key.W) || Input.IsKeyPressed(Key.Up)) screenDirection.Y -= 1f;
        if (Input.IsKeyPressed(Key.S) || Input.IsKeyPressed(Key.Down)) screenDirection.Y += 1f;

        if (!_isPanningCamera)
        {
            var viewport = GetViewport();
            Vector2 mousePosition = viewport.GetMousePosition();
            Vector2 viewportSize = viewport.GetVisibleRect().Size;
            if (mousePosition.X < EdgeScrollDistance) screenDirection.X -= 1f;
            if (mousePosition.X > viewportSize.X - EdgeScrollDistance) screenDirection.X += 1f;
            if (mousePosition.Y < EdgeScrollDistance) screenDirection.Y -= 1f;
            if (mousePosition.Y > viewportSize.Y - EdgeScrollDistance) screenDirection.Y += 1f;
        }

        if (screenDirection != Vector2.Zero)
        {
            float zoomSpeedScale = _camera.Size / BaseCameraSize;
            _cameraTarget += GetCameraPlaneDirection(screenDirection.Normalized()) * CameraMoveSpeed * zoomSpeedScale * (float)delta;
            UpdateCameraTransform();
        }
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventKey keyEvent && keyEvent.Pressed && !keyEvent.Echo && keyEvent.Keycode == Key.Escape)
        {
            _buildPanel?.CancelBuildMode();
            return;
        }

        if (@event is InputEventMouseButton mouseButton)
        {
            if (mouseButton.ButtonIndex == MouseButton.Middle)
            {
                _isPanningCamera = mouseButton.Pressed;
                _lastMousePosition = mouseButton.Position;
                return;
            }

            if (!mouseButton.Pressed) return;
            if (mouseButton.ButtonIndex == MouseButton.WheelUp) { SetCameraDistance(_cameraDistance - CameraZoomStep, mouseButton.Position); return; }
            if (mouseButton.ButtonIndex == MouseButton.WheelDown) { SetCameraDistance(_cameraDistance + CameraZoomStep, mouseButton.Position); return; }
            if (mouseButton.ButtonIndex == MouseButton.Right && _buildPanel?.IsBuildMode == true) { _buildPanel.CancelBuildMode(); return; }
            if (_enemyTurnInProgress) return;
            if (mouseButton.ButtonIndex == MouseButton.Right) { _activePiece?.HandleBack(); return; }
            if (mouseButton.ButtonIndex == MouseButton.Left)
            {
                var clickedTile = GetTileUnderMouse(mouseButton.Position);
                if (_buildPanel?.IsBuildMode == true) _buildPanel.TryBuild(clickedTile);
                else HandleTileClick(clickedTile);
            }
            return;
        }

        if (@event is InputEventMouseMotion mouseMotion)
        {
            if (_isPanningCamera)
            {
                Vector2 mouseDelta = mouseMotion.Position - _lastMousePosition;
                _lastMousePosition = mouseMotion.Position;
                _cameraTarget += GetCameraPlaneDirection(-mouseDelta.Normalized()) * mouseDelta.Length() * (_cameraDistance * CameraDragDistanceScale);
                UpdateCameraTransform();
            }

            var hoveredTile = GetTileUnderMouse(mouseMotion.Position);
            if (_buildPanel?.IsBuildMode == true)
            {
                if (Input.IsMouseButtonPressed(MouseButton.Left)) _buildPanel.TryBuild(hoveredTile);
            }
            else
            {
                _activePiece?.HandleTileHover(hoveredTile);
                UpdateHoveredThing(hoveredTile);
            }
        }
    }

    public void SetActivePiece(Piece piece)
    {
        if (_activePiece == piece) return;
        _activePiece?.Deselect();
        _activePiece = piece;
    }

    public void ClearActivePiece(Piece piece)
    {
        if (_activePiece == piece) _activePiece = null;
    }

    public void RegisterThing(Thing thing)
    {
        if (!_things.Contains(thing)) _things.Add(thing);
    }

    public void NotifyBoardStateChanged()
    {
        var enemies = new List<Enemy>(_enemies);
        foreach (var enemy in enemies)
        {
            enemy.RefreshPreparedAttackPreview();
        }
    }

    public void UnregisterThing(Thing thing)
    {
        _things.Remove(thing);
        if (_hoveredThing == thing) _hoveredThing = null;

        if (thing is Piece piece)
        {
            _pieces.Remove(piece);
            ClearActivePiece(piece);
        }
        if (thing is Enemy enemy)
        {
            _enemies.Remove(enemy);
        }
    }

    private void UpdateHoveredThing(Tile hoveredTile)
    {
        Thing closest = hoveredTile?.OccupiedPiece;

        if (_hoveredThing == closest) return;
        _hoveredThing?.SetHovered(false);
        _hoveredThing = closest;
        _hoveredThing?.SetHovered(true);
    }

    public Tile GetTile(Vector2I gridPosition) => _tiles.GetValueOrDefault(gridPosition);
    public static int HexDistance(Vector2I a, Vector2I b) => Function.HexDistance(a, b);
    public static bool TryGetHexStraightDirection(Vector2I startOffset, Vector2I targetOffset, out Vector2I directionAxial) => Function.TryGetHexStraightDirection(startOffset, targetOffset, out directionAxial);
    public static List<Vector2I> GetHexStraightLine(Vector2I startOffset, Vector2I directionAxial, int range) => Function.GetHexStraightLine(startOffset, directionAxial, range);

    private void CreateEnvironment()
    {
        _camera = GetNodeOrNull<Camera3D>(CameraNodePath);
        UpdateCameraTransform();
    }

    private void CreateGrid()
    {
        TileScene ??= GD.Load<PackedScene>(ScenePaths.Tile);
        if (TileScene == null)
        {
            GD.PushError("CreateGrid: TileScene is null!");
            return;
        }

        for (int row = 0; row < GridHeight; row++)
        for (int column = 0; column < GridWidth; column++)
        {
            var gridPosition = new Vector2I(column, row);
            var tile = TileScene.Instantiate<Tile>();
            tile.Name = SceneNames.Tile(column, row);
            tile.GridPosition = gridPosition;
            tile.HexRadius = HexRadius;
            tile.Position = GridToWorld(gridPosition);
            if (InitialMountainCells.Contains(gridPosition)) tile.State = TileState.Mountain;
            if (InitialWaterCells.Contains(gridPosition)) tile.State = TileState.Water;
            AddChild(tile);
            tile.WorldRef = this;
            _tiles[gridPosition] = tile;
        }
    }

private void SpawnPiece(Vector2I gridPosition, PackedScene pieceScene)
{
    // 1. 提前校验资源有效性
    if (pieceScene == null)
    {
        GD.PushError("SpawnPiece: PieceScene is null!");
        return;
    }

    // 2. 先获取并检查目标格子
    var tile = GetTile(gridPosition);
    if (tile == null || tile.OccupiedPiece != null)
        return;

    // 3. 通过验证后才实例化
    Piece piece = pieceScene.Instantiate<Piece>();

    // 4. 初始化并添加到场景
    piece.Name = SceneNames.Piece(gridPosition);
    piece.WorldRef = this;
    piece.OccupiedTile = tile;
    piece.Position = tile.Position + Vector3.Up * WorldGeometry.PieceElevation;
    AddChild(piece);

    // 5. 更新状态
    tile.OccupiedPiece = piece;
    _pieces.Add(piece);
}

    private void SpawnEnemy(Vector2I gridPosition, PackedScene enemyScene)
    {
        if (enemyScene == null)
        {
            GD.PushError("SpawnEnemy: EnemyScene is null!");
            return;
        }

        var tile = GetTile(gridPosition);
        if (tile == null || tile.OccupiedPiece != null) return;

        var enemy = enemyScene.Instantiate<Enemy>();
        enemy.Name = SceneNames.Enemy(gridPosition);
        enemy.WorldRef = this;
        enemy.OccupiedTile = tile;
        enemy.Position = tile.Position + Vector3.Up * WorldGeometry.PieceElevation;
        AddChild(enemy);
        tile.OccupiedPiece = enemy;
        _pieces.Add(enemy);
        _enemies.Add(enemy);
    }

    private void CreateEndTurnButton()
    {
        _uiLayer = GetNodeOrNull<CanvasLayer>(UiLayerNodePath);
        _endTurnButton = GetNodeOrNull<Button>(EndTurnButtonNodePath);
        _buildPanel = GetNodeOrNull<BuildPanel>(BuildPanelNodePath);
        _buildPanel?.Initialize(this);
        if (_endTurnButton != null) _endTurnButton.Pressed += EndTurn;
    }

    private async void EndTurn()
    {
        if (_enemyTurnInProgress) return;

        _enemyTurnInProgress = true;
        if (_endTurnButton != null) _endTurnButton.Disabled = true;
        _activePiece?.Deselect();
        try
        {
            var enemies = new List<Enemy>(_enemyActionOrder);
            foreach (var enemy in enemies)
            {
                if (!enemy.IsDead) enemy.ExecutePreparedAttack();
            }

            enemies = new List<Enemy>(_enemyActionOrder);
            foreach (var enemy in enemies)
            {
                if (!enemy.IsDead) await enemy.MoveAndPrepareAsync();
            }

            foreach (var piece in _pieces)
            {
                if (!piece.IsDead && piece.Faction == Faction.Player) piece.ResetTurn();
            }
            RefreshEnemyActionOrder();
        }
        finally
        {
            _enemyTurnInProgress = false;
            if (_endTurnButton != null) _endTurnButton.Disabled = false;
        }
    }

    private Tile GetTileUnderMouse(Vector2 screenPosition)
    {
        Vector3 origin = _camera.ProjectRayOrigin(screenPosition);
        Vector3 direction = _camera.ProjectRayNormal(screenPosition);
        if (Mathf.IsZeroApprox(direction.Y)) return null;
        float distance = -origin.Y / direction.Y;
        if (distance < 0f) return null;
        return GetTile(WorldToGrid(origin + direction * distance));
    }

    private Vector3 GridToWorld(Vector2I gridPosition) => new(HexRadius * HexGeometry.SqrtThree * (gridPosition.X + HexGeometry.Half * (gridPosition.Y & HexGeometry.OddRowMask)), 0f, HexRadius * HexGeometry.RowSpacing * gridPosition.Y);
    private Vector2I WorldToGrid(Vector3 worldPosition) => new(Mathf.RoundToInt(worldPosition.X / (HexRadius * HexGeometry.SqrtThree) - HexGeometry.Half * (Mathf.RoundToInt(worldPosition.Z / (HexRadius * HexGeometry.RowSpacing)) & HexGeometry.OddRowMask)), Mathf.RoundToInt(worldPosition.Z / (HexRadius * HexGeometry.RowSpacing)));
    private void HandleTileClick(Tile tile) { if (tile == null) return; if (_activePiece == null) tile.OccupiedPiece?.Select(); else _activePiece.HandleTileClick(tile); }
    private void SetCameraDistance(float distance, Vector2 screenPosition)
    {
        bool hasWorldPointBeforeZoom = TryGetGroundPosition(screenPosition, out Vector3 worldPointBeforeZoom);
        _cameraDistance = Mathf.Clamp(distance, MinCameraDistance, MaxCameraDistance);
        _camera.Size = _cameraDistance;
        UpdateCameraTransform();

        if (hasWorldPointBeforeZoom && TryGetGroundPosition(screenPosition, out Vector3 worldPointAfterZoom))
        {
            _cameraTarget += worldPointBeforeZoom - worldPointAfterZoom;
            UpdateCameraTransform();
        }
    }

    private bool TryGetGroundPosition(Vector2 screenPosition, out Vector3 groundPosition)
    {
        groundPosition = Vector3.Zero;
        Vector3 origin = _camera.ProjectRayOrigin(screenPosition);
        Vector3 direction = _camera.ProjectRayNormal(screenPosition);
        if (Mathf.IsZeroApprox(direction.Y)) return false;

        float distance = -origin.Y / direction.Y;
        if (distance < 0f) return false;
        groundPosition = origin + direction * distance;
        return true;
    }

    private void UpdateCameraTransform()
    {
        float isometricOffset = _cameraDistance / HexGeometry.SqrtThree;
        _camera.Position = _cameraTarget + new Vector3(isometricOffset, isometricOffset, isometricOffset);
        _camera.LookAt(_cameraTarget);
        RefreshThingHealthBars();
    }

    private void RefreshThingHealthBars()
    {
        foreach (var thing in _things)
        {
            thing?.RefreshHealthBarPosition();
            if (thing is Enemy enemy) enemy.RefreshActionOrderPosition();
        }
    }

    private void RefreshEnemyActionOrder()
    {
        var shuffledEnemies = new List<Enemy>();
        foreach (var enemy in _enemies)
        {
            if (!enemy.IsDead) shuffledEnemies.Add(enemy);
        }

        for (int index = shuffledEnemies.Count - 1; index > 0; index--)
        {
            int swapIndex = (int)GD.RandRange(0, index);
            (shuffledEnemies[index], shuffledEnemies[swapIndex]) = (shuffledEnemies[swapIndex], shuffledEnemies[index]);
        }

        _enemyActionOrder = shuffledEnemies.OrderByDescending(enemy => enemy.Speed).ToList();
        for (int index = 0; index < _enemyActionOrder.Count; index++)
        {
            _enemyActionOrder[index].SetActionOrder(index + 1);
        }
    }

    private void SetEnemyActionOrderVisible(bool visible)
    {
        foreach (var enemy in _enemyActionOrder)
        {
            enemy.SetActionOrderVisible(visible && !enemy.IsDead);
        }
    }

    private Vector3 GetCameraPlaneDirection(Vector2 screenDirection)
    {
        Vector3 right = _camera.GlobalTransform.Basis.X;
        Vector3 forward = -_camera.GlobalTransform.Basis.Z;
        right.Y = 0f;
        forward.Y = 0f;
        return (right.Normalized() * screenDirection.X - forward.Normalized() * screenDirection.Y).Normalized();
    }
}