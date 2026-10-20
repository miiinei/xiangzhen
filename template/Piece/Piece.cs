using Godot;
using System.Collections.Generic;
using System.Threading.Tasks;

public partial class Piece : Thing
{
    private const int DefaultMoveRange = 3;
    private const float DefaultPathPreviewWidth = 0.08f;
    private const string PathPreviewWidthRangeHint = "0.01,0.5,0.01";
    private const float MovementStepDuration = 0.15f;
    private const int MinimumPathPointCount = 2;
    private const float MinimumMiterDenominator = 0.25f;
    private const float MaximumMiterWidthMultiplier = 1.5f;
    private static readonly NodePath BodyMeshNodePath = "BodyMesh";
    private static readonly NodePath PathPreviewNodePath = "PathPreview";
    private static readonly Color SelectedPieceColor = new(1f, 0.8f, 0.22f);
    private static readonly Color DefaultPieceColor = new(0.85f, 0.45f, 0.18f);
    private static readonly Color SelectionEmissionColor = new(0.35f, 0.22f, 0.03f);

    private enum PieceState { Default, Move, Release }
    [Export] public PackedScene Skill1Scene { get; set; }
    [Export] public PackedScene Skill2Scene { get; set; }
    [Export] public PackedScene Skill3Scene { get; set; }
    [Export] public PackedScene Skill4Scene { get; set; }

    [Export] public PackedScene skillPanel { get; set; }
    public Skill Skill1 { get; set; }
    public Skill Skill2 { get; set; }
    public Skill Skill3 { get; set; }
    public Skill Skill4 { get; set; }
    [Export] public int MoveRange { get; set; } = DefaultMoveRange;
    [Export] public int[] MoveGroups { get; set; } = new[] { ObstacleGroups.Ground, ObstacleGroups.FlyingUnit };
    [Export(PropertyHint.Range, PathPreviewWidthRangeHint)] public float PathPreviewWidth { get; set; } = DefaultPathPreviewWidth;
    [Export] public Color DefaultBodyColor { get; set; } = DefaultPieceColor;
    public int RemainingMoveRange { get; set; }
    private PieceState _state;
    private SkillPanel _skillPanel;
    private MeshInstance3D _mesh;
    private MeshInstance3D _pathPreview;
    private Tile _hoveredTargetTile;
    private bool _isMoving;

    public override void _Ready()
    {
        if (Faction == Faction.Neutral) Faction = Faction.Player;
        base._Ready();
        RemainingMoveRange = MoveRange;
        EnsureSkills();
        _mesh = GetNodeOrNull<MeshInstance3D>(BodyMeshNodePath);
        if (_mesh != null) _mesh.MaterialOverride = CreateMaterial(false);
        _pathPreview = GetNodeOrNull<MeshInstance3D>(PathPreviewNodePath);
    }

    public override void _Notification(int what)
    {
        if (what != NotificationPredelete) return;

        // Skills outside the scene tree still belong to this piece.
        foreach (var skill in GetSkills())
        {
            if (GodotObject.IsInstanceValid(skill) && skill.GetParent() == null)
            {
                skill.Free();
            }
        }
    }

    public void EnsureSkills()
    {
        if (Skill1 == null && Skill1Scene != null) Skill1 = Skill1Scene.Instantiate<Skill>();
        if (Skill2 == null && Skill2Scene != null) Skill2 = Skill2Scene.Instantiate<Skill>();
        if (Skill3 == null && Skill3Scene != null) Skill3 = Skill3Scene.Instantiate<Skill>();
        if (Skill4 == null && Skill4Scene != null) Skill4 = Skill4Scene.Instantiate<Skill>();

        if (Skill1 != null) Skill1.Caster = this;
        if (Skill2 != null) Skill2.Caster = this;
        if (Skill3 != null) Skill3.Caster = this;
        if (Skill4 != null) Skill4.Caster = this;
    }

    protected Skill[] GetSkills() => new[] { Skill1, Skill2, Skill3, Skill4 };

    public virtual void Select()
    {
        if (IsDead || _isMoving) return;
        WorldRef.SetActivePiece(this);
        EnsureSkillPanel(skillPanel);
        _skillPanel?.ShowForPiece(this);
        SetSelected(true);
        SetState(PieceState.Move);
    }

    public void Deselect()
    {
        SetSelected(false);
        _skillPanel?.Hide();
        SetState(PieceState.Default);
        ClearPathPreview();
        WorldRef?.ClearActivePiece(this);
    }

    public void HandleBack()
    {
        if (IsDead || _isMoving) return;

        if (_state == PieceState.Release)
        {
            _skillPanel?.StepBackTargeting();
            return;
        }

        if (_state == PieceState.Move)
        {
            Deselect();
        }
    }

    public void HandleTileHover(Tile tile)
    {
        if (IsDead || _isMoving)
        {
            ClearPathPreview();
            return;
        }
        if (_state == PieceState.Release)
        {
            ShowSkillTargetPreview(tile);
            SetHoveredTargetOutline(CanReleaseSkillOnTile(_skillPanel?.ActiveSkill, tile) ? tile : null);
            return;
        }

        if (_state == PieceState.Move)
        {
            var path = FindPath(tile);
            ShowPathPreview(path);
            SetHoveredTargetOutline(path != null ? tile : null);
        }
    }
    public void HandleTileClick(Tile tile)
    {
        if (IsDead || _isMoving) return;
        if (_state == PieceState.Release) { _skillPanel?.OnTileClick(tile); return; }
        if (_state == PieceState.Default) { tile?.OccupiedPiece?.Select(); return; }
        if (tile == OccupiedTile) { Deselect(); return; }
        if (tile?.OccupiedPiece != null) { tile.OccupiedPiece.Select(); return; }
        var path = FindPath(tile);
        if (path == null) return;
        RemainingMoveRange -= path.Count - 1;
        ClearPathPreview();
        ClearRangeOverlays();
        _ = StartMoveAlongPath(path);
    }

    public override bool IsBusy => _state != PieceState.Default;

    public override void SetSelected(bool selected) { if (_mesh != null) _mesh.MaterialOverride = CreateMaterial(selected); OccupiedTile?.SetSelected(selected); }
    public override void ResetTurn()
    {
        RemainingMoveRange = MoveRange;
        Skill1?.ResetTurn();
        Skill2?.ResetTurn();
        Skill3?.ResetTurn();
        Skill4?.ResetTurn();
    }

    public override void Die()
    {
        SetSelected(false);
        ClearPathPreview();
        ClearRangeOverlays();
        _skillPanel?.QueueFree();
        _skillPanel = null;
        base.Die();
    }

    public bool CanIgnoreObstacleGroup(int obstacleGroup) => System.Array.IndexOf(MoveGroups, obstacleGroup) >= 0;

    public void KnockBack(Vector2I direction, int collisionDamage)
    {
        if (IsDead || WorldRef == null || OccupiedTile == null || direction == Vector2I.Zero) return;

        var axialPosition = Function.OffsetToAxial(OccupiedTile.GridPosition.X, OccupiedTile.GridPosition.Y);
        var destinationPosition = Function.AxialToOffset(axialPosition.X + direction.X, axialPosition.Y + direction.Y);
        var destination = WorldRef.GetTile(destinationPosition);
        if (destination == null || destination.State == TileState.Mountain)
        {
            TakeDamage(collisionDamage);
            return;
        }

        if (destination.OccupiedPiece != null)
        {
            destination.OccupiedPiece.TakeDamage(collisionDamage);
            TakeDamage(collisionDamage);
            return;
        }

        SetOccupiedTile(destination);
        Position = destination.Position + Vector3.Up * WorldGeometry.PieceElevation;
        RefreshHealthBarPosition();
    }

    public Task MoveAlongPathAsync(List<Tile> path) => StartMoveAlongPath(path);

    private Task StartMoveAlongPath(List<Tile> path)
    {
        if (_isMoving || path == null || path.Count < MinimumPathPointCount) return Task.CompletedTask;

        _isMoving = true;
        ClearPathPreview();
        return MoveAlongPathCoreAsync(path);
    }

    private async Task MoveAlongPathCoreAsync(List<Tile> path)
    {
        var startingTile = OccupiedTile;
        startingTile.SetSelected(false);
        OccupiedTile.OccupiedPiece = null;
        try
        {
            foreach (var tile in path.GetRange(1, path.Count - 1))
            {
                var tween = CreateTween();
                tween.TweenMethod(
                    Callable.From<Vector3>(SetMovementPosition),
                    Position,
                    tile.Position + Vector3.Up * WorldGeometry.PieceElevation,
                    MovementStepDuration
                );
                await ToSignal(tween, Tween.SignalName.Finished);
            }
            SetOccupiedTile(path[^1]);
            if (_state != PieceState.Default)
            {
                OccupiedTile.SetSelected(true);
            }
            if (_state == PieceState.Move)
            {
                ShowMoveRange();
            }
        }
        finally
        {
            _isMoving = false;
        }
    }

    private void ShowMoveRange() => ShowRangeBoundary(GetReachableTiles(), PreviewGeometry.RangeColor);

    private void ClearRangeOverlays()
    {
        if (WorldRef == null) return;
        Skill1?.ClearCombatPreview();
        Skill2?.ClearCombatPreview();
        Skill3?.ClearCombatPreview();
        Skill4?.ClearCombatPreview();
        foreach (var tile in WorldRef.Tiles.Values)
        {
            tile.SetRangeOverlay(false);
            tile.SetTargetPreview(false);
        }
        _hoveredTargetTile = null;
    }

    protected HashSet<Vector2I> GetReachableTiles()
    {
        var result = new HashSet<Vector2I>();
        if (WorldRef == null || OccupiedTile == null) return result;

        var queue = new Queue<Tile>();
        var distances = new Dictionary<Vector2I, int> { [OccupiedTile.GridPosition] = 0 };
        queue.Enqueue(OccupiedTile);

        while (queue.Count > 0)
        {
            var currentTile = queue.Dequeue();
            int nextDistance = distances[currentTile.GridPosition] + 1;
            if (nextDistance > RemainingMoveRange) continue;

            foreach (var neighbor in Function.GetNeighborTiles(currentTile, WorldRef.Tiles))
            {
                if (distances.ContainsKey(neighbor.GridPosition) || !CanTraverseTile(neighbor)) continue;
                distances[neighbor.GridPosition] = nextDistance;
                queue.Enqueue(neighbor);
                if (CanStopOnTile(neighbor)) result.Add(neighbor.GridPosition);
            }
        }
        return result;
    }

    protected List<Tile> FindPath(Tile destination)
    {
        if (destination == null || WorldRef == null || OccupiedTile == null || !CanStopOnTile(destination)) return null;

        var queue = new Queue<Tile>();
        var distances = new Dictionary<Vector2I, int> { [OccupiedTile.GridPosition] = 0 };
        var cameFrom = new Dictionary<Vector2I, Vector2I>();
        queue.Enqueue(OccupiedTile);

        while (queue.Count > 0)
        {
            var currentTile = queue.Dequeue();
            if (currentTile == destination) break;

            int nextDistance = distances[currentTile.GridPosition] + 1;
            if (nextDistance > RemainingMoveRange) continue;

            foreach (var neighbor in Function.GetNeighborTiles(currentTile, WorldRef.Tiles))
            {
                if (distances.ContainsKey(neighbor.GridPosition) || !CanTraverseTile(neighbor)) continue;
                distances[neighbor.GridPosition] = nextDistance;
                cameFrom[neighbor.GridPosition] = currentTile.GridPosition;
                queue.Enqueue(neighbor);
            }
        }

        if (!distances.ContainsKey(destination.GridPosition)) return null;

        var path = new List<Tile>();
        for (var currentPosition = destination.GridPosition; currentPosition != OccupiedTile.GridPosition; currentPosition = cameFrom[currentPosition])
        {
            path.Add(WorldRef.GetTile(currentPosition));
        }
        path.Add(OccupiedTile);
        path.Reverse();
        return path;
    }

    protected bool CanTraverseTile(Tile tile)
    {
        if (tile == null || !CanIgnoreObstacleGroup(tile.ObstacleGroup)) return false;
        return tile.OccupiedPiece == null || tile.OccupiedPiece == this || CanIgnoreObstacleGroup(tile.OccupiedPiece.ObstacleGroup);
    }

    protected bool CanStopOnTile(Tile tile) => CanTraverseTile(tile) && tile.OccupiedPiece == null;

    private void SetOccupiedTile(Tile destination)
    {
        if (destination == null || WorldRef == null) return;

        foreach (var tile in WorldRef.Tiles.Values)
        {
            if (tile.OccupiedPiece == this)
            {
                tile.OccupiedPiece = null;
                tile.SetSelected(false);
            }
        }

        OccupiedTile = destination;
        destination.OccupiedPiece = this;
        if (_state != PieceState.Default)
        {
            destination.SetSelected(true);
        }
        WorldRef.NotifyBoardStateChanged();
    }

    private void EnsureSkillPanel(PackedScene skillPanel)
    {
        if (_skillPanel != null || WorldRef?.UiLayer == null) return;
        if (skillPanel == null)
        {
            GD.PushError($"{Name}: skillPanel scene is not assigned.");
            return;
        }

        _skillPanel = skillPanel.Instantiate<SkillPanel>();

        _skillPanel.Name = SceneNames.SkillPanel(Name);
        _skillPanel.AnchorLeft = 1f;
        _skillPanel.AnchorTop = 0f;
        _skillPanel.AnchorRight = 1f;
        _skillPanel.AnchorBottom = 0f;
        _skillPanel.OffsetLeft = -_skillPanel.Size.X;
        _skillPanel.OffsetTop = 0f;
        _skillPanel.OffsetRight = 0f;
        _skillPanel.OffsetBottom = _skillPanel.Size.Y;
        _skillPanel.PanelClicked += Deselect;
        _skillPanel.ReleaseModeChanged += isReleaseMode => SetState(isReleaseMode ? PieceState.Release : PieceState.Move);
        _skillPanel.TargetingStageChanged += RefreshSkillTargetingRange;
        WorldRef.UiLayer.AddChild(_skillPanel);
    }

    private void SetState(PieceState state)
    {
        if (IsDead) return;
        _state = state;
        RefreshHealthBarPosition();
        ClearPathPreview();
        ClearRangeOverlays();
        if (state == PieceState.Move) ShowMoveRange();
        if (state == PieceState.Release) ShowSkillRange();
    }

    private void SetMovementPosition(Vector3 position)
    {
        Position = position;
        RefreshHealthBarPosition();
    }

    private void ShowSkillRange()
    {
        var rootSkill = _skillPanel?.ActiveSkill;
        var skill = rootSkill?.TargetingSkill;
        var originTile = rootSkill?.TargetOriginTile;
        if (skill == null || originTile == null || WorldRef == null) return;
        var selectableTiles = new HashSet<Vector2I>();
        foreach (var tile in WorldRef.Tiles.Values)
        {
            if (CanReleaseSkillOnTile(rootSkill, tile)) selectableTiles.Add(tile.GridPosition);
        }
        ShowRangeBoundary(selectableTiles, PreviewGeometry.RangeColor);
    }

    private bool CanReleaseSkillOnTile(Skill skill, Tile tile)
    {
        Skill targetingSkill = skill?.TargetingSkill;
        Tile originTile = skill?.TargetOriginTile;
        if (tile == null || originTile == null || targetingSkill == null) return false;
        if (tile == originTile) return false;
        if (tile.State == TileState.Mountain && !targetingSkill.CanTargetMountain) return false;
        if (targetingSkill.Straight && !Function.TryGetHexStraightDirection(originTile.GridPosition, tile.GridPosition, out _)) return false;
        return targetingSkill.Range == 0 || Function.HexDistance(originTile.GridPosition, tile.GridPosition) <= targetingSkill.Range;
    }

    private void ShowSkillTargetPreview(Tile targetTile)
    {
        var skill = _skillPanel?.ActiveSkill;
        if (!CanReleaseSkillOnTile(skill, targetTile))
        {
            skill?.ClearTargetingCombatPreview();
            return;
        }
        skill.ShowTargetingCombatPreview(targetTile);
    }

    private void RefreshSkillTargetingRange()
    {
        if (WorldRef == null || _state != PieceState.Release) return;
        foreach (var tile in WorldRef.Tiles.Values) tile.SetRangeOverlay(false);
        _hoveredTargetTile = null;
        ShowSkillRange();
    }

    private void ShowRangeBoundary(HashSet<Vector2I> selectableTiles, Color color)
    {
        if (WorldRef == null) return;
        foreach (var gridPosition in selectableTiles)
        {
            var visibleEdges = new bool[HexGeometry.EdgeCount];
            var axialPosition = Function.OffsetToAxial(gridPosition.X, gridPosition.Y);
            for (int edge = 0; edge < visibleEdges.Length; edge++)
            {
                var neighborAxial = axialPosition + Function.HexEdgeDirections[edge];
                var neighborPosition = Function.AxialToOffset(neighborAxial.X, neighborAxial.Y);
                visibleEdges[edge] = !selectableTiles.Contains(neighborPosition);
            }

            WorldRef.GetTile(gridPosition)?.SetRangeOutlineEdges(visibleEdges, color);
        }
    }

    private void SetHoveredTargetOutline(Tile tile)
    {
        if (_hoveredTargetTile == tile) return;

        _hoveredTargetTile = tile;
        RedrawRangeOutlines();
        _hoveredTargetTile?.SetRangeOutlineEdges(
            new[] { true, true, true, true, true, true },
            PreviewGeometry.RangeColor
        );
    }

    private void RedrawRangeOutlines()
    {
        if (WorldRef == null) return;
        foreach (var tile in WorldRef.Tiles.Values) tile.SetRangeOverlay(false);

        if (_state == PieceState.Move) ShowMoveRange();
        if (_state == PieceState.Release) ShowSkillRange();
    }

    private void ShowPathPreview(List<Tile> path)
    {
        if (path == null || path.Count < MinimumPathPointCount)
        {
            ClearPathPreview();
            return;
        }

        if (_pathPreview == null) return;

        var points = new Vector3[path.Count];
        var leftPoints = new Vector3[path.Count];
        var rightPoints = new Vector3[path.Count];
        for (int index = 0; index < path.Count; index++)
        {
            points[index] = ToLocal(path[index].GlobalPosition + Vector3.Up * PreviewGeometry.SurfaceElevation);
        }

        float halfWidth = PathPreviewWidth * HexGeometry.Half;
        for (int index = 0; index < points.Length; index++)
        {
            Vector3 offset;
            if (index == 0 || index == points.Length - 1)
            {
                Vector3 direction = index == 0
                    ? (points[1] - points[0]).Normalized()
                    : (points[^1] - points[^2]).Normalized();
                offset = new Vector3(-direction.Z, 0f, direction.X) * halfWidth;
            }
            else
            {
                Vector3 previousDirection = (points[index] - points[index - 1]).Normalized();
                Vector3 nextDirection = (points[index + 1] - points[index]).Normalized();
                Vector3 previousNormal = new(-previousDirection.Z, 0f, previousDirection.X);
                Vector3 nextNormal = new(-nextDirection.Z, 0f, nextDirection.X);
                Vector3 miter = (previousNormal + nextNormal).Normalized();
                float denominator = Mathf.Max(MinimumMiterDenominator, Mathf.Abs(miter.Dot(previousNormal)));
                offset = miter * Mathf.Min(halfWidth / denominator, PathPreviewWidth * MaximumMiterWidthMultiplier);
            }

            leftPoints[index] = points[index] + offset;
            rightPoints[index] = points[index] - offset;
        }

        var mesh = new ImmediateMesh();
        mesh.SurfaceBegin(Mesh.PrimitiveType.Triangles);
        for (int index = 0; index < path.Count - 1; index++)
        {
            mesh.SurfaceAddVertex(leftPoints[index]);
            mesh.SurfaceAddVertex(rightPoints[index]);
            mesh.SurfaceAddVertex(rightPoints[index + 1]);
            mesh.SurfaceAddVertex(leftPoints[index]);
            mesh.SurfaceAddVertex(rightPoints[index + 1]);
            mesh.SurfaceAddVertex(leftPoints[index + 1]);
        }
        mesh.SurfaceEnd();
        _pathPreview.Mesh = mesh;
        _pathPreview.Visible = true;
    }

    private void ClearPathPreview()
    {
        if (_pathPreview != null) _pathPreview.Visible = false;
    }

    private ShaderMaterial CreateMaterial(bool selected) => StylizedMaterials.Create(
        selected ? SelectedPieceColor : DefaultBodyColor,
        selected ? SelectionEmissionColor : default);
}