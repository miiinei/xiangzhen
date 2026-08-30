using Godot;
using System;

public partial class SkillPanel : Control
{
    public event Action PanelClicked;
    public event Action<bool> ReleaseModeChanged;
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
        var panel = GetNodeOrNull<Panel>("Panel");
        _skillContainer = GetNodeOrNull<HBoxContainer>("Panel/MarginContainer/VBoxContainer/SkillIconContainer");
        _titleLabel = GetNodeOrNull<Label>("Panel/MarginContainer/VBoxContainer/TitleLabel");

        if (panel != null)
        {
            var style = new StyleBoxFlat();
            style.BgColor = new Color(0.12f, 0.12f, 0.12f, 1f);
            style.BorderColor = new Color(0.32f, 0.32f, 0.32f, 1f);
            style.BorderWidthLeft = 2;
            style.BorderWidthTop = 2;
            style.BorderWidthRight = 2;
            style.BorderWidthBottom = 2;
            panel.AddThemeStyleboxOverride("panel", style);
            panel.Modulate = new Color(1f, 1f, 1f, 1f);
        }

        if (_skillContainer != null)
        {
            _skillContainer.AddThemeConstantOverride("separation", 8);
        }
        _defaultSkillScene = GD.Load<PackedScene>("res://template/Skill/skill.tscn");

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

        for (var i = 0; i < 4; i++)
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

            skill.Name = !skill.IsEmptySlot ? (skill.SkillName ?? $"Skill{i + 1}") : $"EmptySlot{i + 1}";
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

        if (tile.State == TileState.Mountain && !_activeSkill.CanTargetMountain)
        {
            return false;
        }

        if (_currentPiece?.OccupiedTile == null)
        {
            return false;
        }

        var startPos = _currentPiece.OccupiedTile.GridPosition;

        if (_activeSkill.Straight)
        {
            if (!World.TryGetHexStraightDirection(startPos, tile.GridPosition, out _))
            {
                return false;
            }
        }

        if (_activeSkill.Range > 0)
        {
            int distance = World.HexDistance(startPos, tile.GridPosition);
            if (distance > _activeSkill.Range)
            {
                return false;
            }
        }

        return true;
    }

    public void OnTileHover(Tile tile)
    {
        if (_activeSkill == null)
            return;

        if (!IsValidTarget(tile))
        {
            ClearStraightHighlights();
            if (_hoveredTile != null)
            {
                try { _hoveredTile.SetSelected(false); } catch { }
                _hoveredTile = null;
            }
            return;
        }

        if (_hoveredTile == tile)
            return;

        if (_hoveredTile != null)
        {
            try { _hoveredTile.SetSelected(false); } catch { }
        }

        _hoveredTile = tile;
        try { _hoveredTile.SetSelected(true); } catch { }

        ClearStraightHighlights();

        if (_activeSkill.Straight && _currentPiece?.OccupiedTile != null && _currentPiece.WorldRef != null)
        {
            var startPos = _currentPiece.OccupiedTile.GridPosition;
            if (World.TryGetHexStraightDirection(startPos, tile.GridPosition, out var dirAxial))
            {
                int range = _activeSkill.Range > 0 ? _activeSkill.Range : 1;
                var linePositions = World.GetHexStraightLine(startPos, dirAxial, range);
                foreach (var pos in linePositions)
                {
                    var lineTile = _currentPiece.WorldRef.GetTile(pos);
                    if (lineTile != null)
                    {
                        lineTile.SetRangeOverlay(true, new Color(1f, 0.2f, 0.2f, 0.35f));
                        _straightHighlightedTiles.Add(lineTile);
                    }
                }
            }
        }
    }

    public void OnTileClick(Tile tile)
    {
        if (_activeSkill == null)
            return;

        if (!IsValidTarget(tile))
            return;

        var multiTarget = _activeSkill.MultiTarget;
        var skillToUse = _activeSkill;

        try
        {
            skillToUse.Execute(tile);
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

        if (multiTarget)
        {
            _multiTargetCount++;
            // multi-target skills allow exactly 2 targets
            if (_multiTargetCount >= 2)
            {
                skillToUse.SetUsed(true);
                ClearActiveSkill();
            }
        }
        else
        {
            // single-target skills exit after first click
            skillToUse.SetUsed(true);
            ClearActiveSkill();
        }
    }
}
