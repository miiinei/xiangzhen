using Godot;
using System;
using System.Collections.Generic;

public partial class World : Node2D
{
    private enum WorldState
    {
        Default,
        Move,
        Release
    }

    [Export]
    public PackedScene PieceScene { get; set; }

    [Export]
    public PackedScene SkillPanelScene { get; set; }

    [Export]
    public float EdgeScrollDistance { get; set; } = 30f;

    [Export]
    public float EdgeScrollSpeed { get; set; } = 500f;

    [Export(PropertyHint.Range, "0.2,2.0,0.05")]
    public float MinZoom { get; set; } = 0.5f;

    [Export(PropertyHint.Range, "0.5,3.0,0.05")]
    public float MaxZoom { get; set; } = 2.0f;

    [Export]
    public float ZoomStep { get; set; } = 0.1f;

    [Export]
    public Color SkillRangeBorderColor { get; set; } = new(0.25f, 0.55f, 0.75f, 1f);

    [Export]
    public Color MoveRangeBorderColor { get; set; } = new(0.4f, 0.9f, 1f, 1f);

    private readonly Dictionary<Vector2I, Tile> _tiles = new();
    private readonly List<Line2D> _skillRangeLines = new();
    private readonly List<Line2D> _moveRangeLines = new();
    private static readonly Vector2I[] HexEdgeDirections =
    {
        new(0, 1),
        new(-1, 1),
        new(-1, 0),
        new(0, -1),
        new(1, -1),
        new(1, 0)
    };
    private Piece _selectedPiece;
    private Camera2D _camera;
    private CanvasLayer _uiLayer;
    private SkillPanel _skillPanel;
    private Button _endTurnButton;
    private bool _isDraggingCamera;
    private Vector2 _lastMousePosition;
    private Line2D _movePreviewLine;
    private Tile _hoveredMoveTargetTile;
    private WorldState _state = WorldState.Default;

    public override void _Ready()
    {
        EnsureCamera();
        EnsureSkillPanel();
        EnsureEndTurnButton();
        RegisterTiles();
        EnsureMovePreviewLine();

        if (PieceScene == null)
        {
            GD.PrintErr("PieceScene is null. Please assign the Piece.tscn in the World node.");
            return;
        }

        SpawnPiece(new Vector2I(1, 1), PieceScene);
        SpawnPiece(new Vector2I(4, 3), PieceScene);
        SpawnPiece(new Vector2I(4, 5), PieceScene);
    }

    public override void _Input(InputEvent @event)
    {
        if (_camera == null)
        {
            return;
        }

        if (@event is InputEventMouseButton mouseButton)
        {
            if (mouseButton.ButtonIndex == MouseButton.WheelUp)
            {
                ZoomCamera(1f + ZoomStep);
                return;
            }

            if (mouseButton.ButtonIndex == MouseButton.WheelDown)
            {
                ZoomCamera(1f - ZoomStep);
                return;
            }

            if (mouseButton.ButtonIndex == MouseButton.Middle)
            {
                _isDraggingCamera = mouseButton.Pressed;
                if (_isDraggingCamera)
                {
                    _lastMousePosition = GetViewport().GetMousePosition();
                }
            }
        }
        else if (@event is InputEventMouseMotion mouseMotion)
        {
            if (_isDraggingCamera)
            {
                var delta = mouseMotion.Position - _lastMousePosition;
                _lastMousePosition = mouseMotion.Position;
                _camera.Position -= delta / _camera.Zoom;
            }
        }
    }

    public override void _Process(double delta)
    {
        if (_camera == null)
        {
            return;
        }

        var viewport = GetViewport();
        if (viewport == null)
        {
            return;
        }

        var mousePosition = viewport.GetMousePosition();
        var screenSize = viewport.GetVisibleRect().Size;

        Vector2 direction = Vector2.Zero;

        if (mousePosition.X < EdgeScrollDistance)
        {
            direction.X = -1f;
        }
        else if (mousePosition.X > screenSize.X - EdgeScrollDistance)
        {
            direction.X = 1f;
        }

        if (mousePosition.Y < EdgeScrollDistance)
        {
            direction.Y = -1f;
        }
        else if (mousePosition.Y > screenSize.Y - EdgeScrollDistance)
        {
            direction.Y = 1f;
        }

        if (direction != Vector2.Zero)
        {
            _camera.Position += direction.Normalized() * EdgeScrollSpeed * (float)delta;
        }
    }

    private void EnsureCamera()
    {
        _camera = GetNodeOrNull<Camera2D>("Camera2D");
        if (_camera == null)
        {
            _camera = new Camera2D();
            _camera.Name = "Camera2D";
            AddChild(_camera);
            _camera.MakeCurrent();
        }

        _camera.Zoom = new Vector2(1f, 1f);
    }

    private void EnsureSkillPanel()
    {
        if (_skillPanel != null)
        {
            return;
        }

        if (SkillPanelScene == null)
        {
            SkillPanelScene = GD.Load<PackedScene>("res://template/UI/SkillPanel.tscn");
        }

        if (SkillPanelScene == null)
        {
            GD.PrintErr("SkillPanelScene is null. Please assign the SkillPanel.tscn in the World node.");
            return;
        }

        _uiLayer = new CanvasLayer();
        _uiLayer.Name = "UILayer";
        AddChild(_uiLayer);

        _skillPanel = SkillPanelScene.Instantiate<SkillPanel>();
        _skillPanel.Name = "SkillPanel";
        _skillPanel.Visible = false;
        _uiLayer.AddChild(_skillPanel);

        // clicking the skill panel cancels move state, but does not force close release state
        _skillPanel.PanelClicked += () =>
        {
            if (_state == WorldState.Move)
            {
                DeselectPiece();
            }
        };

        _skillPanel.ReleaseModeChanged += isReleaseMode =>
        {
            if (isReleaseMode)
            {
                SetState(WorldState.Release);
                ShowSkillRangePreview();
            }
            else
            {
                ClearSkillRangePreview();
                if (_selectedPiece != null)
                {
                    // After release finishes, always return to default mode.
                    DeselectPiece();
                }
                else
                {
                    SetState(WorldState.Default);
                }
            }
        };

        var screenSize = GetViewportRect().Size;
        var panelWidth = 480f;
        _skillPanel.Position = new Vector2(screenSize.X - panelWidth, 0f);
        _skillPanel.Size = new Vector2(panelWidth, screenSize.Y);
    }

    private void EnsureEndTurnButton()
    {
        if (_endTurnButton != null)
        {
            return;
        }

        if (_uiLayer == null)
        {
            _uiLayer = new CanvasLayer { Name = "UILayer" };
            AddChild(_uiLayer);
        }

        _endTurnButton = new Button
        {
            Name = "EndTurnButton",
            Text = "结束回合",
            CustomMinimumSize = new Vector2(140f, 48f)
        };

        _endTurnButton.AnchorLeft = 1f;
        _endTurnButton.AnchorTop = 1f;
        _endTurnButton.AnchorRight = 1f;
        _endTurnButton.AnchorBottom = 1f;
        _endTurnButton.OffsetLeft = -160f;
        _endTurnButton.OffsetTop = -64f;
        _endTurnButton.OffsetRight = -20f;
        _endTurnButton.OffsetBottom = -16f;

        var styleNormal = new StyleBoxFlat
        {
            BgColor = new Color(0.18f, 0.22f, 0.32f, 0.95f),
            BorderColor = new Color(0.4f, 0.65f, 0.9f, 1f),
            BorderWidthLeft = 2,
            BorderWidthTop = 2,
            BorderWidthRight = 2,
            BorderWidthBottom = 2,
            CornerRadiusTopLeft = 6,
            CornerRadiusTopRight = 6,
            CornerRadiusBottomLeft = 6,
            CornerRadiusBottomRight = 6
        };
        var styleHover = new StyleBoxFlat
        {
            BgColor = new Color(0.25f, 0.32f, 0.48f, 1f),
            BorderColor = new Color(0.6f, 0.85f, 1f, 1f),
            BorderWidthLeft = 2,
            BorderWidthTop = 2,
            BorderWidthRight = 2,
            BorderWidthBottom = 2,
            CornerRadiusTopLeft = 6,
            CornerRadiusTopRight = 6,
            CornerRadiusBottomLeft = 6,
            CornerRadiusBottomRight = 6
        };
        var stylePressed = new StyleBoxFlat
        {
            BgColor = new Color(0.12f, 0.15f, 0.22f, 1f),
            BorderColor = new Color(0.3f, 0.5f, 0.7f, 1f),
            BorderWidthLeft = 2,
            BorderWidthTop = 2,
            BorderWidthRight = 2,
            BorderWidthBottom = 2,
            CornerRadiusTopLeft = 6,
            CornerRadiusTopRight = 6,
            CornerRadiusBottomLeft = 6,
            CornerRadiusBottomRight = 6
        };

        _endTurnButton.AddThemeStyleboxOverride("normal", styleNormal);
        _endTurnButton.AddThemeStyleboxOverride("hover", styleHover);
        _endTurnButton.AddThemeStyleboxOverride("pressed", stylePressed);
        _endTurnButton.AddThemeColorOverride("font_color", Colors.White);
        _endTurnButton.AddThemeFontSizeOverride("font_size", 18);

        _endTurnButton.Pressed += EndTurn;
        _uiLayer.AddChild(_endTurnButton);
    }

    public void EndTurn()
    {
        foreach (var child in GetChildren())
        {
            if (child is Piece piece)
            {
                piece.ResetTurn();
            }
        }

        if (_selectedPiece != null)
        {
            _skillPanel?.ShowForPiece(_selectedPiece);
            if (_state == WorldState.Move)
            {
                ShowMoveRangePreview();
            }
        }
    }

    private void EnsureMovePreviewLine()
    {
        if (_movePreviewLine != null)
        {
            return;
        }

        _movePreviewLine = new Line2D();
        _movePreviewLine.Name = "MovePreviewLine";
        _movePreviewLine.Width = 4f;
        _movePreviewLine.DefaultColor = new Color(0.4f, 0.9f, 1f, 1f);
        _movePreviewLine.ZIndex = 200;
        _movePreviewLine.Visible = false;
        AddChild(_movePreviewLine);
    }

    private void ZoomCamera(float multiplier)
    {
        if (_camera == null)
        {
            return;
        }

        var newZoom = _camera.Zoom * multiplier;
        newZoom = new Vector2(
            Mathf.Clamp(newZoom.X, MinZoom, MaxZoom),
            Mathf.Clamp(newZoom.Y, MinZoom, MaxZoom)
        );

        _camera.Zoom = newZoom;
    }

    public void HandleTileHover(Tile hoveredTile)
    {
        if (_state == WorldState.Release)
        {
            _skillPanel?.OnTileHover(hoveredTile);
            return;
        }

        if (_state != WorldState.Move || _selectedPiece == null)
        {
            ClearMovePreview();
            ClearHoveredMoveTarget();
            return;
        }

        if (hoveredTile == _selectedPiece.OccupiedTile)
        {
            ClearMovePreview();
            ClearHoveredMoveTarget();
            return;
        }

        var path = GetBestMovePathToward(_selectedPiece.OccupiedTile, hoveredTile);
        if (path != null && path.Count > 1)
        {
            ShowMovePreview(path);
            var endTile = path[^1];
            SetHoveredMoveTarget(endTile);
            return;
        }

        ClearMovePreview();
        ClearHoveredMoveTarget();
    }

    public void HandleTileClick(Tile clickedTile)
    {
        if (_state == WorldState.Release)
        {
            _skillPanel?.OnTileClick(clickedTile);
            return;
        }

        if (_state == WorldState.Default)
        {
            if (clickedTile.OccupiedPiece != null)
            {
                SelectPiece(clickedTile.OccupiedPiece);
            }

            return;
        }

        if (_state != WorldState.Move || _selectedPiece == null)
        {
            return;
        }

        if (clickedTile == _selectedPiece.OccupiedTile)
        {
            DeselectPiece();
            return;
        }

        if (clickedTile.OccupiedPiece != null)
        {
            SelectPiece(clickedTile.OccupiedPiece);
            return;
        }

        var path = GetBestMovePathToward(_selectedPiece.OccupiedTile, clickedTile);
        if (path != null && path.Count > 1)
        {
            int steps = path.Count - 1;
            _selectedPiece.RemainingMoveRange = Math.Max(0, _selectedPiece.RemainingMoveRange - steps);
            ClearMovePreview();
            ClearMoveRangePreview();
            _ = _selectedPiece.MoveAlongPathAsync(path);
            DeselectPiece();
        }
    }

    private void RegisterTiles()
    {
        _tiles.Clear();

        foreach (Node child in GetChildren())
        {
            if (child is not Tile tile)
            {
                continue;
            }

            tile.WorldRef = this;
            tile.InputPickable = true;
            tile.Connect("input_event", new Callable(tile, nameof(Tile.OnTileInput)));
            _tiles[tile.GridPosition] = tile;
        }
    }

    private void SpawnPiece(Vector2I gridPosition, PackedScene pieceScene = null)
    {

        if (pieceScene == null)
        {
            return;
        }

        var tile = _tiles.GetValueOrDefault(gridPosition);
        if (tile == null || tile.OccupiedPiece != null)
        {
            return;
        }

        var piece = pieceScene.Instantiate<Piece>();
        piece.OccupiedTile = tile;
        tile.OccupiedPiece = piece;
        piece.Position = tile.Position;
        piece.WorldRef = this;

        AddChild(piece);
    }

    private void SelectPiece(Piece piece)
    {
        if (_selectedPiece != null)
        {
            _selectedPiece.SetSelected(false);
        }

        _selectedPiece = piece;
        _selectedPiece.SetSelected(true);
        if (_skillPanel != null)
        {
            _skillPanel.ShowForPiece(piece);
        }
        SetState(WorldState.Move);
        ClearMovePreview();
        ClearHoveredMoveTarget();
    }

    private void DeselectPiece()
    {
        if (_selectedPiece != null)
        {
            _selectedPiece.SetSelected(false);
        }

        _selectedPiece = null;
        SetState(WorldState.Default);
        ClearMovePreview();
        ClearHoveredMoveTarget();
    }

    // Skill activation is managed by SkillPanel; world tracks a coarse gameplay state.

    private void SetState(WorldState state)
    {
        if (_state == state)
        {
            return;
        }

        _state = state;
        RefreshUiForState();
    }

    private void RefreshUiForState()
    {
        if (_skillPanel != null)
        {
            _skillPanel.Visible = _state != WorldState.Default;
        }

        // Move preview should only exist in move state.
        if (_state != WorldState.Move)
        {
            ClearMovePreview();
            ClearHoveredMoveTarget();
            ClearMoveRangePreview();
        }
        else
        {
            ShowMoveRangePreview();
        }

        if (_state != WorldState.Release)
        {
            ClearSkillRangePreview();
        }
    }

    private void ShowMoveRangePreview()
    {
        ClearMoveRangePreview();

        if (_selectedPiece?.OccupiedTile == null || _selectedPiece.RemainingMoveRange <= 0)
        {
            return;
        }

        var reachablePositions = GetReachableMovePositions(_selectedPiece);
        if (reachablePositions.Count <= 1)
        {
            return;
        }

        Function.DrawRangeBoundary(this, reachablePositions, _tiles, _moveRangeLines, MoveRangeBorderColor, "MoveRangeLine", 145);
    }

    private HashSet<Vector2I> GetReachableMovePositions(Piece piece)
    {
        var positions = new HashSet<Vector2I>();
        if (piece?.OccupiedTile == null || piece.RemainingMoveRange <= 0)
        {
            return positions;
        }

        var start = piece.OccupiedTile;
        positions.Add(start.GridPosition);

        var queue = new Queue<Tile>();
        var visited = new HashSet<Vector2I> { start.GridPosition };
        var dist = new Dictionary<Vector2I, int> { [start.GridPosition] = 0 };
        queue.Enqueue(start);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            var currentDist = dist[current.GridPosition];

            foreach (var neighbor in Function.GetNeighborTiles(current, _tiles))
            {
                if (neighbor == null || !CanTraverseToTile(piece, current, neighbor, null))
                {
                    continue;
                }

                if (visited.Contains(neighbor.GridPosition))
                {
                    continue;
                }

                int nextDist = currentDist + 1;
                if (nextDist > piece.RemainingMoveRange)
                {
                    continue;
                }

                visited.Add(neighbor.GridPosition);
                dist[neighbor.GridPosition] = nextDist;
                queue.Enqueue(neighbor);

                if (neighbor.OccupiedPiece == null || neighbor.OccupiedPiece == piece)
                {
                    positions.Add(neighbor.GridPosition);
                }
            }
        }

        return positions;
    }

    private void ClearMoveRangePreview()
    {
        Function.ClearLines(_moveRangeLines);
    }

    private void ShowSkillRangePreview()
    {
        ClearSkillRangePreview();

        if (_selectedPiece?.OccupiedTile == null || _skillPanel?.ActiveSkill == null)
        {
            return;
        }

        var skill = _skillPanel.ActiveSkill;
        var centerPos = _selectedPiece.OccupiedTile.GridPosition;
        int range = skill.Range;

        var inRangePositions = new HashSet<Vector2I>();
        if (skill.Straight)
        {
            var centerAxial = Function.OffsetToAxial(centerPos.X, centerPos.Y);
            int maxStep = range > 0 ? range : 100;
            foreach (var dir in Function.HexEdgeDirections)
            {
                for (int step = 1; step <= maxStep; step++)
                {
                    var targetAxial = centerAxial + dir * step;
                    var offset = Function.AxialToOffset(targetAxial.X, targetAxial.Y);
                    if (_tiles.ContainsKey(offset))
                    {
                        inRangePositions.Add(offset);
                    }
                }
            }
        }
        else
        {
            foreach (var (gridPos, tile) in _tiles)
            {
                if (tile == null) continue;
                if (range > 0)
                {
                    if (Function.HexDistance(centerPos, gridPos) <= range)
                    {
                        inRangePositions.Add(gridPos);
                    }
                }
                else
                {
                    inRangePositions.Add(gridPos);
                }
            }
        }

        if (inRangePositions.Count == 0)
        {
            return;
        }

        Function.DrawRangeBoundary(this, inRangePositions, _tiles, _skillRangeLines, SkillRangeBorderColor, "SkillRangeLine", 150);
    }

    private void ClearSkillRangePreview()
    {
        Function.ClearLines(_skillRangeLines);
    }

    private void ShowMovePreview(List<Tile> path)
    {
        ClearMovePreview();

        if (_movePreviewLine == null)
        {
            EnsureMovePreviewLine();
        }

        var points = new List<Vector2>();
        foreach (var tile in path)
        {
            if (tile != null)
            {
                points.Add(tile.GlobalPosition);
            }
        }

        _movePreviewLine.Points = points.ToArray();
        _movePreviewLine.Visible = points.Count > 1;
    }

    private void ClearMovePreview()
    {
        if (_movePreviewLine != null)
        {
            _movePreviewLine.Visible = false;
            _movePreviewLine.Points = Array.Empty<Vector2>();
        }
    }

    private void SetHoveredMoveTarget(Tile tile)
    {
        if (tile == null)
        {
            ClearHoveredMoveTarget();
            return;
        }

        if (_hoveredMoveTargetTile != null && _hoveredMoveTargetTile != tile)
        {
            _hoveredMoveTargetTile.SetSelected(false);
        }

        _hoveredMoveTargetTile = tile;
        _hoveredMoveTargetTile.SetSelected(true);
    }

    private void ClearHoveredMoveTarget()
    {
        if (_hoveredMoveTargetTile != null)
        {
            _hoveredMoveTargetTile.SetSelected(false);
            _hoveredMoveTargetTile = null;
        }
    }

    private List<Tile> GetBestMovePathToward(Tile start, Tile goal)
    {
        if (start == null || goal == null)
        {
            return null;
        }

        if (start == goal)
        {
            return new List<Tile> { start };
        }

        if (_selectedPiece == null)
        {
            return null;
        }

        var maxSteps = _selectedPiece.RemainingMoveRange;
        if (maxSteps <= 0)
        {
            return null;
        }

        var queue = new Queue<Tile>();
        var visited = new HashSet<Vector2I> { start.GridPosition };
        var cameFrom = new Dictionary<Vector2I, Vector2I>();
        var distanceFromStart = new Dictionary<Vector2I, int> { [start.GridPosition] = 0 };

        queue.Enqueue(start);

        Tile bestTile = null;
        int bestDistanceToGoal = int.MaxValue;

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            var currentDistance = distanceFromStart[current.GridPosition];

            foreach (var neighbor in Function.GetNeighborTiles(current, _tiles))
            {
                if (neighbor == null || !CanTraverseToTile(_selectedPiece, current, neighbor, goal))
                {
                    continue;
                }

                if (neighbor == goal && goal.OccupiedPiece != null && goal.OccupiedPiece != _selectedPiece)
                {
                    bestTile = current;
                    bestDistanceToGoal = 0;
                    continue;
                }

                if (visited.Contains(neighbor.GridPosition))
                {
                    continue;
                }

                var nextDistance = currentDistance + 1;
                if (nextDistance > maxSteps)
                {
                    continue;
                }

                visited.Add(neighbor.GridPosition);
                cameFrom[neighbor.GridPosition] = current.GridPosition;
                distanceFromStart[neighbor.GridPosition] = nextDistance;
                queue.Enqueue(neighbor);

                if (neighbor.OccupiedPiece != null && neighbor.OccupiedPiece != _selectedPiece && neighbor != goal)
                {
                    continue;
                }

                var distanceToGoal = Function.HexDistance(neighbor.GridPosition, goal.GridPosition);
                if (distanceToGoal < bestDistanceToGoal)
                {
                    bestDistanceToGoal = distanceToGoal;
                    bestTile = neighbor;
                }
            }
        }

        if (bestTile == null)
        {
            return null;
        }

        var path = new List<Tile>();
        var cursor = bestTile.GridPosition;
        while (cursor != start.GridPosition)
        {
            path.Add(_tiles[cursor]);
            cursor = cameFrom[cursor];
        }

        path.Add(start);
        path.Reverse();
        return path;
    }

    private bool CanTraverseToTile(Piece movingPiece, Tile fromTile, Tile nextTile, Tile goal)
    {
        if (movingPiece == null || nextTile == null)
        {
            return false;
        }

        if (nextTile.ObstacleGroup != 0 && !movingPiece.CanIgnoreObstacleGroup(nextTile.ObstacleGroup))
        {
            return false;
        }

        if (nextTile.OccupiedPiece == null || nextTile.OccupiedPiece == movingPiece)
        {
            return true;
        }

        if (nextTile == goal)
        {
            return false;
        }

        return movingPiece.CanIgnoreObstacleGroup(nextTile.OccupiedPiece.ObstacleGroup);
    }

    public Tile GetTile(Vector2I gridPosition)
    {
        return _tiles.GetValueOrDefault(gridPosition);
    }

    // 兼容转发，确保其他脚本通过 World.xxx 调用依然正常
    public static Vector2I OffsetToAxial(int col, int row) => Function.OffsetToAxial(col, row);
    public static Vector2I AxialToOffset(int q, int r) => Function.AxialToOffset(q, r);
    public static int HexDistance(Vector2I a, Vector2I b) => Function.HexDistance(a, b);
    public static bool TryGetHexStraightDirection(Vector2I startOffset, Vector2I targetOffset, out Vector2I directionAxial) =>
        Function.TryGetHexStraightDirection(startOffset, targetOffset, out directionAxial);
    public static List<Vector2I> GetHexStraightLine(Vector2I startOffset, Vector2I directionAxial, int range) =>
        Function.GetHexStraightLine(startOffset, directionAxial, range);
}