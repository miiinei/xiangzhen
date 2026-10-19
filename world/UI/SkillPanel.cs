using Godot;
using System;

public partial class SkillPanel : Control
{
    private const int PanelBorderWidth = 2;
    private const int SkillSeparation = 8;
    private const int SkillSlotCount = 4;
    private static readonly Color PanelBackgroundColor = new(0.12f, 0.12f, 0.12f, 1f);
    private static readonly Color PanelBorderColor = new(0.32f, 0.32f, 0.32f, 1f);
    private static readonly NodePath PanelNodePath = "Panel";
    private static readonly NodePath SkillContainerNodePath = "Panel/MarginContainer/VBoxContainer/SkillIconContainer";
    private static readonly NodePath TitleLabelNodePath = "Panel/MarginContainer/VBoxContainer/TitleLabel";

    public event Action PanelClicked;
    public event Action<bool> ReleaseModeChanged;
    public event Action TargetingStageChanged;
    private HBoxContainer _skillContainer;
    private Label _titleLabel;
    private PackedScene _defaultSkillScene;
    private Skill _activeSkill;
    private Tile _hoveredTile;
    private int _multiTargetCount;
    private Piece _currentPiece;
    private readonly System.Collections.Generic.List<Tile> _straightHighlightedTiles = new();

    public Skill ActiveSkill => _activeSkill;

    public override void _Ready()
    {
        var panel = GetNodeOrNull<Panel>(PanelNodePath);
        _skillContainer = GetNodeOrNull<HBoxContainer>(SkillContainerNodePath);
        _titleLabel = GetNodeOrNull<Label>(TitleLabelNodePath);

        if (panel != null)
        {
            var style = new StyleBoxFlat();
            style.BgColor = PanelBackgroundColor;
            style.BorderColor = PanelBorderColor;
            style.BorderWidthLeft = PanelBorderWidth;
            style.BorderWidthTop = PanelBorderWidth;
            style.BorderWidthRight = PanelBorderWidth;
            style.BorderWidthBottom = PanelBorderWidth;
            panel.AddThemeStyleboxOverride(GodotThemeKeys.Panel, style);
            panel.Modulate = Colors.White;
        }

        if (_skillContainer != null)
        {
            _skillContainer.AddThemeConstantOverride(GodotThemeKeys.Separation, SkillSeparation);
        }
        _defaultSkillScene = GD.Load<PackedScene>(ScenePaths.DefaultSkill);

        ClearSkills();
        Visible = false;
        UpdateTitle();

        // allow click on panel background to cancel move mode in World
        MouseFilter = MouseFilterEnum.Stop;
    }

    public override void _GuiInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton mb && mb.Pressed && mb.ButtonIndex == MouseButton.Left)
        {
            PanelClicked?.Invoke();
        }
    }

    public void ShowForPiece(Piece piece)
    {
        _currentPiece = piece;
        Visible = true;
        // switching piece cancels any active skill for previous piece
        ClearActiveSkill();
        UpdateSkills(piece);
        UpdateTitle(piece);
    }

    private void ClearSkills()
    {
        if (_skillContainer == null)
        {
            return;
        }

        foreach (Node child in _skillContainer.GetChildren())
        {
            if (child is Skill s)
            {
                s.Clicked -= OnSkillClicked;
                _skillContainer.RemoveChild(s);
                if (s.IsEmptySlot)
                {
                    s.QueueFree();
                }
            }
        }
    }

    private void UpdateSkills(Piece piece)
    {
        ClearSkills();

        if (_skillContainer == null || piece == null)
        {
            return;
        }

        piece.EnsureSkills();
        var skills = new[]
        {
            piece.Skill1,
            piece.Skill2,
            piece.Skill3,
            piece.Skill4
        };

        for (var i = 0; i < SkillSlotCount; i++)
        {
            var skill = i < skills.Length ? skills[i] : null;

            if (skill == null)
            {
                if (_defaultSkillScene != null)
                {
                    skill = _defaultSkillScene.Instantiate<Skill>();
                }
                else
                {
                    skill = new Skill();
                }
                skill.IsEmptySlot = true;
                skill.SkillName = string.Empty;
            }

            if (skill.GetParent() != null)
            {
                skill.GetParent().RemoveChild(skill);
            }

            skill.Name = !skill.IsEmptySlot
                ? skill.SkillName ?? SceneNames.SkillSlot(i + 1, false)
                : SceneNames.SkillSlot(i + 1, true);
            _skillContainer.AddChild(skill);
            skill.Clicked += OnSkillClicked;
            skill.UpdateVisualState();
        }
    }

    private void UpdateTitle(Piece piece = null)
    {
        if (_titleLabel == null)
        {
            return;
        }

        if (piece == null)
        {
            _titleLabel.Text = "技能";
            return;
        }

        _titleLabel.Text = piece.Name;
    }

    private void OnSkillClicked(Skill skill)
    {
        if (skill == null)
            return;

        // ignore empty slots and already used skills
        if (skill.IsEmptySlot || skill.IsUsed)
        {
            if (_activeSkill != null && _activeSkill == skill)
            {
                ClearActiveSkill();
            }
            return;
        }

        if (_activeSkill == skill)
        {
            // toggle off
            ClearActiveSkill();
            return;
        }

        // switch to new active skill
        SetActiveSkill(skill);
    }

    private void SetActiveSkill(Skill skill)
    {
        var wasInReleaseMode = _activeSkill != null;

        if (_activeSkill != null)
        {
            try
            {
                _activeSkill.SetHighlighted(false);
            }
            catch (ObjectDisposedException)
            {
                // already disposed, ignore
            }
        }

        _activeSkill = skill;
        _multiTargetCount = 0;
        if (_activeSkill != null)
        {
            try
            {
                _activeSkill.SetHighlighted(true);
            }
            catch (ObjectDisposedException)
            {
                // ignore
            }
        }

        if (_activeSkill != null)
        {
            ReleaseModeChanged?.Invoke(true);
        }
    }

    private void ClearActiveSkill()
    {
        var wasInReleaseMode = _activeSkill != null;

        if (_activeSkill != null)
        {
            _activeSkill.CancelChainedTarget();
            try
            {
                _activeSkill.SetHighlighted(false);
            }
            catch (ObjectDisposedException)
            {
                // already disposed
            }
        }

        _activeSkill = null;
        _multiTargetCount = 0;
        ClearStraightHighlights();

        // clear any hovered tile highlight when exiting release mode
        if (_hoveredTile != null)
        {
            try
            {
                _hoveredTile.SetSelected(false);
            }
            catch (ObjectDisposedException)
            {
                // ignore
            }
            _hoveredTile = null;
        }

        if (wasInReleaseMode)
        {
            ReleaseModeChanged?.Invoke(false);
        }
    }

    private void ClearStraightHighlights()
    {
        foreach (var t in _straightHighlightedTiles)
        {
            if (t != null)
            {
                try { t.SetRangeOverlay(false); } catch { }
            }
        }
        _straightHighlightedTiles.Clear();
    }

    private bool IsValidTarget(Tile tile)
    {
        if (_activeSkill == null || tile == null)
        {
            return false;
        }

        Skill targetingSkill = _activeSkill.TargetingSkill;
        Tile originTile = _activeSkill.TargetOriginTile;
        if (originTile == null)
        {
            return false;
        }

        if (tile.State == TileState.Mountain && !targetingSkill.CanTargetMountain)
        {
            return false;
        }

        if (_currentPiece?.OccupiedTile == null)
        {
            return false;
        }

        var startPos = originTile.GridPosition;

        if (targetingSkill.Straight)
        {
            if (!World.TryGetHexStraightDirection(startPos, tile.GridPosition, out _))
            {
                return false;
            }
        }

        if (targetingSkill.Range > 0)
        {
            int distance = World.HexDistance(startPos, tile.GridPosition);
            if (distance > targetingSkill.Range)
            {
                return false;
            }
        }

        return true;
    }

    public void OnTileHover(Tile tile)
    {
        // Piece owns all 3D targeting visuals.
    }

    public bool StepBackTargeting()
    {
        if (_activeSkill == null) return false;

        if (_activeSkill.ReturnToPrimaryTargetStage())
        {
            TargetingStageChanged?.Invoke();
            return true;
        }

        ClearActiveSkill();
        return true;
    }

    public void OnTileClick(Tile tile)
    {
        if (_activeSkill == null)
            return;

        if (!IsValidTarget(tile))
            return;

        var skillToUse = _activeSkill;

        if (!skillToUse.IsChoosingChainedTarget)
        {
            var chainedResult = skillToUse.BeginChainedTarget(tile);
            if (chainedResult == Skill.ChainedTargetResult.ChoosingChainedTarget)
            {
                TargetingStageChanged?.Invoke();
                return;
            }

            if (chainedResult == Skill.ChainedTargetResult.ResolvedPrimaryTarget)
            {
                skillToUse.SetUsed(true);
                ClearActiveSkill();
                return;
            }
        }

        try
        {
            skillToUse.ExecuteTarget(tile);
        }
        catch (Exception ex)
        {
            GD.PrintErr($"Error executing skill: {ex}");
        }

        ClearStraightHighlights();

        if (_hoveredTile != null)
        {
            try { _hoveredTile.SetSelected(false); } catch { }
            _hoveredTile = null;
        }

        skillToUse.SetUsed(true);
        ClearActiveSkill();
    }
}
