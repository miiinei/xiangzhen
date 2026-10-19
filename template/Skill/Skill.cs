using Godot;
using System;
using System.Collections.Generic;

public partial class Skill : Control
{
    private const string DefaultSkillName = "Skill";
    private const string CollisionDamageRangeHint = "0,20,1";
    private const float SkillIconSize = 64f;
    private const int KnockBackFromCaster = 1;
    private const int KnockBackFromTarget = 2;
    private const int UnlimitedStraightTravelRange = 100;
    private const float TilePreviewElevation = 0.18f;
    private const float ArrowElevation = 0.22f;
    private const float ArrowShaftLength = 0.42f;
    private const float ArrowHalfWidth = 0.34f;
    private const float ArrowTipLength = 0.5f;
    private const float ExplosionElevation = 0.28f;
    private const float ExplosionRadius = 0.55f;
    private const int ExplosionPointCount = 12;
    private const float ExplosionInnerRadius = 0.23f;
    private const int TrajectorySegmentCount = 24;
    private const float TrajectoryWidth = 0.08f;
    private const float TrajectoryHeightDistanceScale = 0.3f;
    private const float MinimumTrajectoryHeight = 0.6f;
    private const float MaximumTrajectoryHeight = 3f;
    private const float TrajectoryHeadLength = 0.5f;
    private const float TrajectoryHeadHalfWidth = 0.2f;
    private const float MaximumTrajectoryHeadFraction = 0.4f;
    private const float ParabolaHeightFactor = 4f;
    private static readonly NodePath CombatPreviewNodePath = "CombatPreview";
    private static readonly NodePath FrameNodePath = "Frame";
    private static readonly NodePath IconNodePath = "Frame/TextureRect";
    private static readonly Color EmptySlotColor = new(0.55f, 0.55f, 0.55f, 0.75f);
    private static readonly Color HighlightedSkillColor = new(1.4f, 1.2f, 0.6f, 1f);
    private static readonly Color NormalSkillColor = new(0.85f, 0.85f, 0.85f, 1f);
    private static readonly Color UsedSkillColor = new(0.45f, 0.45f, 0.45f, 0.75f);

    public enum ChainedTargetResult
    {
        NoChainedSkill,
        ChoosingChainedTarget,
        ResolvedPrimaryTarget
    }

    [Export]
    public Texture2D IconTexture { get; set; } = null;

    [Export]
    public string SkillName { get; set; } = DefaultSkillName;




    [Export]
    public int Damage { get; set; } = 0;

    [Export]
    public int Range { get; set; } = 0;

    [Export]
    public int KnockBack { get; set; } = 0;

    [Export(PropertyHint.Range, CollisionDamageRangeHint)]
    public int CollisionDamage { get; set; } = 1;

    [Export]
    public bool CanTargetMountain { get; set; } = false;

    [Export]
    public bool Straight { get; set; } = false;

    [Export]
    public bool Spread { get; set; } = false;

    [Export]
    public int SpreadDamage { get; set; } = 0;
    
    [Export]
    public bool Puncture { get; set; } = false;






    [Export]
    public PackedScene MultiTargetSkill { get; set; } = null;

    [Export]
    public bool HitNone { get; set; } = false;





    [Export]
    public bool IsEmptySlot { get; set; } = false;

    public bool IsUsed { get; set; } = false;
    public Piece Caster { get; set; }

    protected TextureRect Sprite;
    protected Panel Frame;
    private MeshInstance3D _combatPreview;
    private readonly HashSet<Thing> _previewThings = new();
    private readonly Dictionary<Thing, int> _previewDamageTotals = new();
    private Skill _chainedSkill;
    private Tile _primaryTarget;
    private Tile _originTile;

    public Skill TargetingSkill => _chainedSkill ?? this;
    public Tile TargetOriginTile => _chainedSkill != null ? _primaryTarget : Caster?.OccupiedTile;
    public bool IsChoosingChainedTarget => _chainedSkill != null;

    public bool CanTargetFrom(Tile originTile, Tile targetTile)
    {
        if (originTile == null || targetTile == null || originTile == targetTile) return false;
        if (targetTile.State == TileState.Mountain && !CanTargetMountain) return false;
        if (Range > 0 && Function.HexDistance(originTile.GridPosition, targetTile.GridPosition) > Range) return false;
        return !Straight || Function.TryGetHexStraightDirection(originTile.GridPosition, targetTile.GridPosition, out _);
    }

    // Predicts per-piece damage for an attack from originTile to targetTile, including
    // knockback collision damage to both the knocked piece and any piece it collides into.
    // Mirrors ShowCombatPreview so AI scoring stays consistent with what actually happens.
    public Dictionary<Piece, int> PredictDamage(Tile originTile, Tile targetTile)
    {
        var damage = new Dictionary<Piece, int>();
        if (originTile == null || targetTile == null || Caster?.WorldRef == null || !CanTargetFrom(originTile, targetTile)) return damage;

        foreach (var affectedTile in GetAffectedTiles(originTile, targetTile))
        {
            var hitPiece = affectedTile?.OccupiedPiece;
            if (hitPiece == null || hitPiece.IsDead) continue;

            int baseDamage = !Straight && affectedTile != targetTile ? SpreadDamage : Damage;
            int totalDamage = baseDamage;

            if (KnockBack != 0 && hitPiece.CanKnockBack && hitPiece.HP > baseDamage &&
                TryGetKnockBackDirection(originTile, hitPiece, targetTile.GridPosition, out var direction))
            {
                var destination = GetKnockBackDestination(hitPiece, direction);
                bool collides = destination == null || destination.State == TileState.Mountain || destination.OccupiedPiece != null;
                if (collides)
                {
                    totalDamage += CollisionDamage;
                    if (destination?.OccupiedPiece is Piece collisionPiece && collisionPiece != hitPiece)
                    {
                        AddPredictedDamage(damage, collisionPiece, CollisionDamage);
                    }
                }
            }

            AddPredictedDamage(damage, hitPiece, totalDamage);
        }

        return damage;
    }

    private static void AddPredictedDamage(Dictionary<Piece, int> damage, Piece piece, int amount)
    {
        if (piece == null || amount <= 0) return;
        damage.TryGetValue(piece, out int current);
        damage[piece] = current + amount;
    }

    public override void _Ready()
    {
        EnsureVisualTree();
        _combatPreview = GetNodeOrNull<MeshInstance3D>(CombatPreviewNodePath);

        // Configure input handling: have this Control receive GUI input
        MouseFilter = Control.MouseFilterEnum.Stop;
        if (Frame != null)
        {
            Frame.MouseFilter = Control.MouseFilterEnum.Ignore;
        }
        if (Sprite != null)
        {
            Sprite.MouseFilter = Control.MouseFilterEnum.Ignore;
        }

        if (IsEmptySlot)
        {
            if (Frame != null)
            {
                Frame.Modulate = EmptySlotColor;
            }

            if (Sprite != null)
            {
                Sprite.Texture = null;
            }
        }
        else
        {
            UpdateVisualState();
        }

        // Allow IconTexture to be null to represent an empty slot.
        // Do not auto-load a default texture here — leave Sprite.Texture null when IconTexture is null.
        if (Sprite != null)
        {
            Sprite.Texture = IconTexture;
            Sprite.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
            Sprite.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
            Sprite.Position = Vector2.Zero;
            Sprite.SizeFlagsHorizontal = SizeFlags.ExpandFill;
            Sprite.SizeFlagsVertical = SizeFlags.ExpandFill;
            Sprite.CustomMinimumSize = new Vector2(SkillIconSize, SkillIconSize);
            Sprite.SetSize(new Vector2(SkillIconSize, SkillIconSize));
        }

        CustomMinimumSize = new Vector2(SkillIconSize, SkillIconSize);
        SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter;
        SizeFlagsVertical = Control.SizeFlags.ShrinkCenter;
        SetSize(new Vector2(SkillIconSize, SkillIconSize));

        if (Frame != null)
        {
            Frame.CustomMinimumSize = new Vector2(SkillIconSize, SkillIconSize);
            Frame.SetSize(new Vector2(SkillIconSize, SkillIconSize));
        }
    }

    private void EnsureVisualTree()
    {
        // Expect `Frame` and `Frame/TextureRect` to be defined in the scene (skill.tscn).
        Frame = GetNodeOrNull<Panel>(FrameNodePath);
        Sprite = GetNodeOrNull<TextureRect>(IconNodePath);

        if (Frame == null)
        {
            GD.PrintErr("Skill visual nodes missing: ensure Skill.tscn contains a 'Frame' Panel with a 'TextureRect' child.");
        }
    }

    public void SetIcon(Texture2D texture)
    {
        IconTexture = texture;
        if (Sprite != null)
        {
            Sprite.Texture = texture;
        }
    }

    public event Action<Skill> Clicked;

    public override void _GuiInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton mb)
        {
            if (mb.Pressed && mb.ButtonIndex == MouseButton.Left)
            {
                Clicked?.Invoke(this);
            }
        }
    }

    public void SetHighlighted(bool highlighted)
    {
        if (Frame == null)
            return;

        if (highlighted)
        {
            Frame.Modulate = HighlightedSkillColor;
        }
        else
        {
            Frame.Modulate = NormalSkillColor;
        }
    }

    public void SetUsed(bool used)
    {
        IsUsed = used;
        UpdateVisualState();
    }

    public void UpdateVisualState()
    {
        if (IsEmptySlot)
        {
            return;
        }

        if (IsUsed)
        {
            Modulate = UsedSkillColor;
        }
        else
        {
            Modulate = Colors.White;
        }
    }

    public void ResetTurn()
    {
        SetUsed(false);
    }

    public virtual void Execute(Tile target)
    {
        ClearCombatPreview();
        if (target == null)
        {
            return;
        }

        if (!Straight)
        {
            ResolveTargetArea(target);
            return;
        }

        Tile originTile = _originTile ?? Caster?.OccupiedTile;
        if (originTile == null || Caster.WorldRef == null ||
            !Function.TryGetHexStraightDirection(originTile.GridPosition, target.GridPosition, out var direction))
        {
            return;
        }

        int range = GetStraightTravelRange(originTile, target);
        var punctureTargets = new List<Piece>();
        foreach (var gridPosition in Function.GetHexStraightLine(originTile.GridPosition, direction, range))
        {
            var tile = Caster.WorldRef.GetTile(gridPosition);
            if (tile == null)
            {
                break;
            }

            if (tile.State == TileState.Mountain)
            {
                if (!Puncture) break;
            }

            if (tile.OccupiedPiece != null)
            {
                if (!Puncture)
                {
                    ResolveHit(tile.OccupiedPiece, target.GridPosition, Damage);
                    return;
                }

                if (!punctureTargets.Contains(tile.OccupiedPiece))
                {
                    punctureTargets.Add(tile.OccupiedPiece);
                }
            }
        }

        foreach (var hitPiece in punctureTargets)
        {
            ResolveHit(hitPiece, target.GridPosition, Damage);
        }
    }

    private void ResolveTargetArea(Tile target)
    {
        ResolveHit(target.OccupiedPiece, target.GridPosition, Damage);

        if (Spread && Caster?.WorldRef != null)
        {
            foreach (var neighbor in Function.GetNeighborTiles(target, Caster.WorldRef.Tiles))
            {
                ResolveHit(neighbor.OccupiedPiece, target.GridPosition, SpreadDamage);
            }
        }
    }

    private void ResolveHit(Piece hitPiece, Vector2I targetPosition, int hitDamage)
    {
        if (hitPiece == null || hitPiece.IsDead) return;

        if (hitDamage > 0)
        {
            hitPiece.TakeDamage(hitDamage);
        }

        if (KnockBack == 0 || !hitPiece.CanKnockBack || hitPiece.IsDead || Caster?.OccupiedTile == null)
        {
            return;
        }

        Vector2I direction;
        if (KnockBack == KnockBackFromCaster)
        {
            Tile originTile = _originTile ?? Caster.OccupiedTile;
            if (!Function.TryGetHexDirectionToward(originTile.GridPosition, hitPiece.OccupiedTile.GridPosition, out direction)) return;
        }
        else if (KnockBack == KnockBackFromTarget)
        {
            if (!Function.TryGetHexDirectionToward(targetPosition, hitPiece.OccupiedTile.GridPosition, out direction)) return;
        }
        else
        {
            return;
        }

        hitPiece.KnockBack(direction, CollisionDamage);
    }

    public void ShowCombatPreview(Tile target)
    {
        ClearCombatPreview();
        Tile originTile = _originTile ?? Caster?.OccupiedTile;
        if (target == null || originTile == null || Caster.WorldRef == null || _combatPreview == null) return;

        _combatPreview.TopLevel = true;
        _combatPreview.GlobalTransform = Transform3D.Identity;
        var mesh = new ImmediateMesh();
        mesh.SurfaceBegin(Mesh.PrimitiveType.Triangles);

        if (!Straight)
        {
            AddTrajectoryArrow(mesh, originTile, target);
        }

        foreach (var affectedTile in GetAffectedTiles(originTile, target))
        {
            AddTileHex(mesh, affectedTile);

            var hitPiece = affectedTile.OccupiedPiece;
            if (hitPiece == null || hitPiece.IsDead) continue;

            int baseDamage = !Straight && affectedTile != target ? SpreadDamage : Damage;
            int predictedDamage = baseDamage;
            if (KnockBack != 0 && hitPiece.CanKnockBack && hitPiece.HP > baseDamage &&
                TryGetKnockBackDirection(originTile, hitPiece, target.GridPosition, out var direction))
            {
                var destination = GetKnockBackDestination(hitPiece, direction);
                bool collision = destination == null || destination.State == TileState.Mountain || destination.OccupiedPiece != null;
                if (collision)
                {
                    predictedDamage += CollisionDamage;
                    if (destination?.OccupiedPiece is Piece collisionPiece && collisionPiece != hitPiece)
                    {
                        AddPreviewDamage(collisionPiece, CollisionDamage);
                    }
                    Vector3 collisionPosition = destination?.GlobalPosition ?? hitPiece.GlobalPosition + AxialDirectionToWorld(direction);
                    AddExplosion(mesh, hitPiece.GlobalPosition, collisionPosition);
                }
                else
                {
                    AddKnockBackArrow(mesh, hitPiece.GlobalPosition, destination.GlobalPosition);
                }
            }

            AddPreviewDamage(hitPiece, predictedDamage);
        }

        mesh.SurfaceEnd();
        _combatPreview.Mesh = mesh;
        _combatPreview.Visible = true;
    }

    public void ClearCombatPreview()
    {
        _chainedSkill?.ClearCombatPreview();
        foreach (var thing in _previewThings)
        {
            thing?.ClearCombatDamagePreview(this);
        }
        _previewThings.Clear();
        _previewDamageTotals.Clear();

        if (_combatPreview != null)
        {
            _combatPreview.Visible = false;
            _combatPreview.Mesh = null;
        }
    }

    private List<Tile> GetAffectedTiles(Tile originTile, Tile target)
    {
        var affectedTiles = new List<Tile>();
        if (!Straight)
        {
            affectedTiles.Add(target);
            if (Spread)
            {
                affectedTiles.AddRange(Function.GetNeighborTiles(target, Caster.WorldRef.Tiles));
            }
            return affectedTiles;
        }

        if (!Function.TryGetHexStraightDirection(originTile.GridPosition, target.GridPosition, out var direction)) return affectedTiles;
        int range = GetStraightTravelRange(originTile, target);
        foreach (var gridPosition in Function.GetHexStraightLine(originTile.GridPosition, direction, range))
        {
            var tile = Caster.WorldRef.GetTile(gridPosition);
            if (tile == null) break;
            affectedTiles.Add(tile);
            if (!Puncture && (tile.State == TileState.Mountain || tile.OccupiedPiece != null)) break;
        }
        return affectedTiles;
    }

    private int GetStraightTravelRange(Tile originTile, Tile target)
    {
        int configuredRange = Range > 0 ? Range : UnlimitedStraightTravelRange;
        if (MultiTargetSkill == null || _originTile != null)
        {
            return configuredRange;
        }

        return Mathf.Min(
            configuredRange,
            Function.HexDistance(originTile.GridPosition, target.GridPosition)
        );
    }

    private void AddPreviewDamage(Thing thing, int damage)
    {
        if (thing == null || damage <= 0) return;
        _previewThings.Add(thing);
        _previewDamageTotals.TryGetValue(thing, out int currentDamage);
        int totalDamage = currentDamage + damage;
        _previewDamageTotals[thing] = totalDamage;
        thing.SetCombatDamagePreview(this, totalDamage);
    }

    private bool TryGetKnockBackDirection(Tile originTile, Piece hitPiece, Vector2I targetPosition, out Vector2I direction)
    {
        direction = Vector2I.Zero;
        if (KnockBack == KnockBackFromCaster)
        {
            return Function.TryGetHexDirectionToward(originTile.GridPosition, hitPiece.OccupiedTile.GridPosition, out direction);
        }
        if (KnockBack == KnockBackFromTarget)
        {
            return Function.TryGetHexDirectionToward(targetPosition, hitPiece.OccupiedTile.GridPosition, out direction);
        }
        return false;
    }

    public ChainedTargetResult BeginChainedTarget(Tile primaryTarget)
    {
        if (MultiTargetSkill == null || _chainedSkill != null || primaryTarget == null)
        {
            return ChainedTargetResult.NoChainedSkill;
        }

        if (HitNone && PrimaryTargetHitsSomething(primaryTarget))
        {
            Execute(primaryTarget);
            return ChainedTargetResult.ResolvedPrimaryTarget;
        }

        ShowCombatPreview(primaryTarget);
        _primaryTarget = primaryTarget;
        _chainedSkill = MultiTargetSkill.Instantiate<Skill>();
        _chainedSkill.Caster = Caster;
        _chainedSkill._originTile = primaryTarget;
        AddChild(_chainedSkill);
        _chainedSkill.Visible = false;
        return ChainedTargetResult.ChoosingChainedTarget;
    }

    private bool PrimaryTargetHitsSomething(Tile primaryTarget)
    {
        Tile originTile = _originTile ?? Caster?.OccupiedTile;
        foreach (var affectedTile in GetAffectedTiles(originTile, primaryTarget))
        {
            if (affectedTile.State == TileState.Mountain || affectedTile.OccupiedPiece != null)
            {
                return true;
            }
        }

        return false;
    }

    public void ShowTargetingCombatPreview(Tile target)
    {
        TargetingSkill.ShowCombatPreview(target);
    }

    public void ClearTargetingCombatPreview()
    {
        TargetingSkill.ClearOwnCombatPreview();
    }

    public void ExecuteTarget(Tile target)
    {
        if (_chainedSkill == null)
        {
            Execute(target);
            return;
        }

        Tile primaryTarget = _primaryTarget;
        Skill chainedSkill = _chainedSkill;
        ClearCombatPreview();
        chainedSkill.ClearCombatPreview();
        Execute(primaryTarget);
        chainedSkill.Execute(target);
        chainedSkill.QueueFree();
        _chainedSkill = null;
        _primaryTarget = null;
    }

    public void CancelChainedTarget()
    {
        if (!ReturnToPrimaryTargetStage())
        {
            ClearCombatPreview();
        }
    }

    public bool ReturnToPrimaryTargetStage()
    {
        if (_chainedSkill == null) return false;

        Skill chainedSkill = _chainedSkill;
        _chainedSkill = null;
        _primaryTarget = null;
        chainedSkill.ClearCombatPreview();
        chainedSkill.QueueFree();
        ClearCombatPreview();
        return true;
    }

    private void ClearOwnCombatPreview()
    {
        foreach (var thing in _previewThings)
        {
            thing?.ClearCombatDamagePreview(this);
        }
        _previewThings.Clear();
        _previewDamageTotals.Clear();

        if (_combatPreview != null)
        {
            _combatPreview.Visible = false;
            _combatPreview.Mesh = null;
        }
    }

    private Tile GetKnockBackDestination(Piece hitPiece, Vector2I direction)
    {
        var axial = Function.OffsetToAxial(hitPiece.OccupiedTile.GridPosition.X, hitPiece.OccupiedTile.GridPosition.Y);
        return Caster.WorldRef.GetTile(Function.AxialToOffset(axial.X + direction.X, axial.Y + direction.Y));
    }

    private static Vector3 AxialDirectionToWorld(Vector2I direction) => new(
        HexGeometry.SqrtThree * (direction.X + direction.Y * HexGeometry.Half),
        0f,
        HexGeometry.RowSpacing * direction.Y
    );

    private static void AddTrajectoryArrow(ImmediateMesh mesh, Tile originTile, Tile targetTile)
    {
        Vector3 origin = originTile.GlobalPosition + Vector3.Up * ArrowElevation;
        Vector3 destination = targetTile.GlobalPosition + Vector3.Up * ArrowElevation;
        Vector3 horizontalDirection = destination - origin;
        horizontalDirection.Y = 0f;
        float distance = horizontalDirection.Length();
        if (Mathf.IsZeroApprox(distance)) return;

        horizontalDirection /= distance;
        Vector3 side = new(-horizontalDirection.Z, 0f, horizontalDirection.X);
        Vector3 halfWidth = side * TrajectoryWidth * HexGeometry.Half;
        float height = Mathf.Clamp(distance * TrajectoryHeightDistanceScale, MinimumTrajectoryHeight, MaximumTrajectoryHeight);
        float shaftEnd = 1f - Mathf.Min(TrajectoryHeadLength / distance, MaximumTrajectoryHeadFraction);

        for (int index = 0; index < TrajectorySegmentCount; index++)
        {
            float startProgress = shaftEnd * index / TrajectorySegmentCount;
            float endProgress = shaftEnd * (index + 1) / TrajectorySegmentCount;
            Vector3 start = GetTrajectoryPoint(origin, destination, height, startProgress);
            Vector3 end = GetTrajectoryPoint(origin, destination, height, endProgress);
            mesh.SurfaceAddVertex(start + halfWidth);
            mesh.SurfaceAddVertex(start - halfWidth);
            mesh.SurfaceAddVertex(end - halfWidth);
            mesh.SurfaceAddVertex(start + halfWidth);
            mesh.SurfaceAddVertex(end - halfWidth);
            mesh.SurfaceAddVertex(end + halfWidth);
        }

        Vector3 headBase = GetTrajectoryPoint(origin, destination, height, shaftEnd);
        mesh.SurfaceAddVertex(headBase + side * TrajectoryHeadHalfWidth);
        mesh.SurfaceAddVertex(headBase - side * TrajectoryHeadHalfWidth);
        mesh.SurfaceAddVertex(destination);
    }

    private static Vector3 GetTrajectoryPoint(Vector3 origin, Vector3 destination, float height, float progress) =>
        origin.Lerp(destination, progress) + Vector3.Up * (ParabolaHeightFactor * height * progress * (1f - progress));

    private static void AddKnockBackArrow(ImmediateMesh mesh, Vector3 origin, Vector3 destination)
    {
        Vector3 direction = (destination - origin).Normalized();
        Vector3 side = new(-direction.Z, 0f, direction.X);
        Vector3 center = destination + Vector3.Up * ArrowElevation;
        Vector3 baseCenter = center - direction * ArrowShaftLength;
        mesh.SurfaceAddVertex(baseCenter + side * ArrowHalfWidth);
        mesh.SurfaceAddVertex(baseCenter - side * ArrowHalfWidth);
        mesh.SurfaceAddVertex(center + direction * ArrowTipLength);
    }

    private static void AddExplosion(ImmediateMesh mesh, Vector3 origin, Vector3 destination)
    {
        AddDisc(mesh, (origin + destination) * HexGeometry.Half + Vector3.Up * ExplosionElevation, ExplosionRadius, ExplosionPointCount, ExplosionInnerRadius);
    }

    private static void AddTileHex(ImmediateMesh mesh, Tile tile)
    {
        Vector2[] vertices = tile.GetHexagonVertices();
        Vector3 center = tile.ToGlobal(Vector3.Up * TilePreviewElevation);
        for (int index = 0; index < vertices.Length; index++)
        {
            Vector2 current = vertices[index];
            Vector2 next = vertices[(index + 1) % vertices.Length];
            mesh.SurfaceAddVertex(center);
            mesh.SurfaceAddVertex(tile.ToGlobal(new Vector3(current.X, TilePreviewElevation, current.Y)));
            mesh.SurfaceAddVertex(tile.ToGlobal(new Vector3(next.X, TilePreviewElevation, next.Y)));
        }
    }

    private static void AddDisc(ImmediateMesh mesh, Vector3 center, float outerRadius, int points, float innerRadius = 0f)
    {
        for (int index = 0; index < points; index++)
        {
            float radiusA = innerRadius > 0f && index % HexGeometry.OffsetParityDivisor != 0 ? innerRadius : outerRadius;
            float radiusB = innerRadius > 0f && (index + 1) % HexGeometry.OffsetParityDivisor != 0 ? innerRadius : outerRadius;
            float angleA = Mathf.Tau * index / points;
            float angleB = Mathf.Tau * (index + 1) / points;
            mesh.SurfaceAddVertex(center);
            mesh.SurfaceAddVertex(center + new Vector3(Mathf.Cos(angleA) * radiusA, 0f, Mathf.Sin(angleA) * radiusA));
            mesh.SurfaceAddVertex(center + new Vector3(Mathf.Cos(angleB) * radiusB, 0f, Mathf.Sin(angleB) * radiusB));
        }
    }
}
