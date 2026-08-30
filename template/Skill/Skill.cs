using Godot;
using System;

public partial class Skill : Control
{
    [Export]
    public Texture2D IconTexture { get; set; } = null;

    [Export]
    public string SkillName { get; set; } = "Skill";

    [Export]
    public int Range { get; set; } = 0;

    [Export]
    public bool CanTargetMountain { get; set; } = false;

    [Export]
    public bool Straight { get; set; } = false;

    [Export]
    public bool MultiTarget { get; set; } = false;

    [Export]
    public bool IsEmptySlot { get; set; } = false;

    public bool IsUsed { get; set; } = false;

    protected TextureRect Sprite;
    protected Panel Frame;

    public override void _Ready()
    {
        EnsureVisualTree();

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
                Frame.Modulate = new Color(0.55f, 0.55f, 0.55f, 0.75f);
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
            Sprite.CustomMinimumSize = new Vector2(64f, 64f);
            Sprite.SetSize(new Vector2(64f, 64f));
        }

        CustomMinimumSize = new Vector2(64f, 64f);
        SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter;
        SizeFlagsVertical = Control.SizeFlags.ShrinkCenter;
        SetSize(new Vector2(64f, 64f));

        if (Frame != null)
        {
            Frame.CustomMinimumSize = new Vector2(64f, 64f);
            Frame.SetSize(new Vector2(64f, 64f));
        }
    }

    private void EnsureVisualTree()
    {
        // Expect `Frame` and `Frame/TextureRect` to be defined in the scene (skill.tscn).
        Frame = GetNodeOrNull<Panel>("Frame");
        Sprite = GetNodeOrNull<TextureRect>("Frame/TextureRect");

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
            Frame.Modulate = new Color(1.4f, 1.2f, 0.6f, 1f);
        }
        else
        {
            Frame.Modulate = new Color(0.85f, 0.85f, 0.85f, 1f);
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
            Modulate = new Color(0.45f, 0.45f, 0.45f, 0.75f);
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

    // Execute the skill on a target tile. Override in specific skills.
    public virtual void Execute(Tile target)
    {
        GD.Print($"Executing skill {SkillName} on tile {target?.GridPosition}");
    }
}
