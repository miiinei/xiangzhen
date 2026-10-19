using Godot;
using System.Collections.Generic;

public partial class Thing : Node3D
{
    private const int DefaultHealth = 3;
    private const string MaxHealthRangeHint = "1,20,1";
    private const string HealthRangeHint = "0,20,1";
    private const float DamagePreviewPulseDuration = 0.55f;
    private const float DamagePreviewPulseDelay = 0.55f;
    private const float HealthBarWidth = 96f;
    private const float HealthSegmentHeight = 6f;
    private const float HealthSegmentGap = 2f;
    private const float HealthBarFramePadding = 6f;
    private const float HealthBarInset = 3f;
    private const float HealthBarWorldElevation = 0.12f;
    private const float HealthBarScreenOffset = 8f;
    private const int HealthBarCornerRadius = 1;
    private static readonly Color DamagePreviewColor = new(1f, 0.25f, 0.2f, 0.25f);
    private static readonly Color HealthBarBackgroundColor = new(0.28f, 0.3f, 0.32f);
    private static readonly Color HealthySegmentColor = new(0.2f, 0.8f, 0.3f);
    private static readonly Color EmptySegmentColor = new(0.08f, 0.09f, 0.1f);
    private static readonly NodePath ModulateProperty = "modulate";

    [Export] public int ObstacleGroup { get; set; }
    [Export] public bool CanKnockBack { get; set; } = true;
    [Export] public Faction Faction { get; set; } = Faction.Neutral;
    private int _maxHP = DefaultHealth;

    [Export(PropertyHint.Range, MaxHealthRangeHint)]
    public int MaxHP
    {
        get => _maxHP;
        set
        {
            _maxHP = Mathf.Max(1, value);
            _hp = Mathf.Clamp(_hp, 0, _maxHP);
            if (IsNodeReady()) RebuildHealthBar();
        }
    }

    private int _hp = DefaultHealth;

    [Export(PropertyHint.Range, HealthRangeHint)]
    public int HP
    {
        get => _hp;
        set
        {
            _hp = Mathf.Clamp(value, 0, MaxHP);
            if (IsNodeReady())
            {
                UpdateHealthBar();
                RefreshHealthBarPosition();
            }
            if (_hp == 0 && IsNodeReady()) Die();
        }
    }

    public World WorldRef { get; set; }
    public Tile OccupiedTile { get; set; }
    public bool IsDead { get; private set; }
    private Control _healthBar;
    private bool _isHovered;
    private int _previewDamage;
    private Tween _damagePreviewTween;
    private readonly Dictionary<Skill, int> _combatPreviewDamage = new();

    // Non-default (e.g. moving/casting) pieces keep their health bar visible.
    public virtual bool IsBusy => false;

    public override void _Ready()
    {
        WorldRef ??= GetParentOrNull<World>();
        _hp = Mathf.Clamp(_hp, 0, MaxHP);
        WorldRef?.RegisterThing(this);
        CreateHealthBar();
        UpdateHealthBar();
        RefreshHealthBarPosition();
    }

    public virtual void SetSelected(bool selected) { }
    public virtual void ResetTurn() { }

    public void SetHovered(bool hovered)
    {
        _isHovered = hovered;
        RefreshHealthBarPosition();
    }

    public void SetCombatDamagePreview(Skill source, int damage)
    {
        if (source == null) return;
        _combatPreviewDamage[source] = Mathf.Max(0, damage);
        RefreshCombatDamagePreview();
        RefreshHealthBarPosition();
    }

    public void ClearCombatDamagePreview(Skill source)
    {
        if (source == null) return;
        _combatPreviewDamage.Remove(source);
        RefreshCombatDamagePreview();
        RefreshHealthBarPosition();
    }

    private void RefreshCombatDamagePreview()
    {
        _damagePreviewTween?.Kill();
        _damagePreviewTween = null;
        _previewDamage = 0;
        foreach (int damage in _combatPreviewDamage.Values) _previewDamage += damage;
        _previewDamage = Mathf.Clamp(_previewDamage, 0, HP);

        if (_healthBar != null)
        {
            foreach (Node child in _healthBar.GetChildren())
            {
                if (child is Panel segment) segment.Modulate = Colors.White;
            }
        }

        if (_previewDamage == 0 || _healthBar == null) return;

        _damagePreviewTween = CreateTween().SetLoops();
        _damagePreviewTween.SetParallel(true);
        int firstPreviewSegment = HP - _previewDamage;
        for (int index = firstPreviewSegment; index < HP; index++)
        {
            var segment = _healthBar.GetNodeOrNull<Panel>(SceneNames.HealthSegment(index));
            if (segment == null) continue;
            _damagePreviewTween.TweenProperty(segment, ModulateProperty, DamagePreviewColor, DamagePreviewPulseDuration)
                .SetTrans(Tween.TransitionType.Sine);
            _damagePreviewTween.TweenProperty(segment, ModulateProperty, Colors.White, DamagePreviewPulseDuration)
                .SetDelay(DamagePreviewPulseDelay)
                .SetTrans(Tween.TransitionType.Sine);
        }
    }

    public void ClearCombatDamagePreview()
    {
        _combatPreviewDamage.Clear();
        RefreshCombatDamagePreview();
    }

    public void TakeDamage(int damage)
    {
        if (damage <= 0 || HP <= 0) return;
        HP -= damage;
    }

    public virtual void Die()
    {
        if (IsDead)
        {
            return;
        }

        IsDead = true;
        if (OccupiedTile?.OccupiedPiece == this)
        {
            OccupiedTile.OccupiedPiece = null;
        }

        WorldRef?.UnregisterThing(this);
        WorldRef?.NotifyBoardStateChanged();
        _healthBar?.QueueFree();
        QueueFree();
    }

    private void CreateHealthBar()
    {
        if (WorldRef?.UiLayer == null) return;

        _healthBar = new Panel
        {
            Name = SceneNames.HealthBar(Name),
            Size = new Vector2(HealthBarWidth + HealthBarFramePadding, HealthSegmentHeight + HealthBarFramePadding),
            Visible = false,
            MouseFilter = Control.MouseFilterEnum.Ignore
        };
        _healthBar.AddThemeStyleboxOverride(GodotThemeKeys.Panel, CreateStyle(HealthBarBackgroundColor));
        WorldRef.UiLayer.AddChild(_healthBar);

        // Total bar width stays fixed; higher MaxHP just splits it into more/narrower segments.
        float segmentWidth = (HealthBarWidth - (MaxHP - 1) * HealthSegmentGap) / MaxHP;
        for (int index = 0; index < MaxHP; index++)
        {
            var segment = new Panel
            {
                Name = SceneNames.HealthSegment(index),
                Position = new Vector2(HealthBarInset + index * (segmentWidth + HealthSegmentGap), HealthBarInset),
                Size = new Vector2(segmentWidth, HealthSegmentHeight),
                MouseFilter = Control.MouseFilterEnum.Ignore
            };
            _healthBar.AddChild(segment);
        }
    }

    private void RebuildHealthBar()
    {
        if (_healthBar != null)
        {
            _healthBar.GetParent()?.RemoveChild(_healthBar);
            _healthBar.QueueFree();
            _healthBar = null;
        }

        CreateHealthBar();
        UpdateHealthBar();
        RefreshHealthBarPosition();
    }

    private void UpdateHealthBar()
    {
        if (_healthBar == null) return;

        for (int index = 0; index < MaxHP; index++)
        {
            var segment = _healthBar.GetNodeOrNull<Panel>(SceneNames.HealthSegment(index));
            if (segment != null)
            {
                segment.AddThemeStyleboxOverride(GodotThemeKeys.Panel, CreateStyle(index < HP ? HealthySegmentColor : EmptySegmentColor));
            }
        }
    }

    public void RefreshHealthBarPosition()
    {
        if (_healthBar == null || WorldRef == null) return;

        bool shouldShow = _isHovered || IsBusy || _previewDamage > 0;
        if (!shouldShow || !WorldRef.TryProjectToScreen(GlobalPosition + Vector3.Up * HealthBarWorldElevation, out Vector2 screenPosition))
        {
            _healthBar.Visible = false;
            return;
        }

        _healthBar.Visible = true;
        _healthBar.Position = screenPosition + new Vector2(-_healthBar.Size.X * HexGeometry.Half, HealthBarScreenOffset);
    }

    private static StyleBoxFlat CreateStyle(Color color) => new()
    {
        BgColor = color,
        CornerRadiusTopLeft = HealthBarCornerRadius,
        CornerRadiusTopRight = HealthBarCornerRadius,
        CornerRadiusBottomLeft = HealthBarCornerRadius,
        CornerRadiusBottomRight = HealthBarCornerRadius
    };
}