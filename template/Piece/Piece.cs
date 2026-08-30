using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public partial class Piece : Thing
{
    [Export]
    public AnimatedSprite2D AnimatedSprite { get; set; }

    [Export]
    public PackedScene Skill1Scene { get; set; }

    [Export]
    public PackedScene Skill2Scene { get; set; }

    [Export]
    public PackedScene Skill3Scene { get; set; }

    [Export]
    public PackedScene Skill4Scene { get; set; }

    public Skill Skill1 { get; set; }
    public Skill Skill2 { get; set; }
    public Skill Skill3 { get; set; }
    public Skill Skill4 { get; set; }

    [Export]
    public int MoveRange { get; set; } = 3;

    public int RemainingMoveRange { get; set; } = 3;

    [Export]
    public int[] MoveGroups { get; set; } = new[] { 0, 2 };

    public override void _Ready()
    {
        base._Ready();

        RemainingMoveRange = MoveRange;
        EnsureSkills();

        if (AnimatedSprite == null)
        {
            AnimatedSprite = GetNodeOrNull<AnimatedSprite2D>("AnimatedSprite2D");
        }

        if (AnimatedSprite != null)
        {
            AnimatedSprite.Position = Vector2.Zero;
        }
    }

    public void EnsureSkills()
    {
        if (Skill1 == null && Skill1Scene != null) Skill1 = Skill1Scene.Instantiate<Skill>();
        if (Skill2 == null && Skill2Scene != null) Skill2 = Skill2Scene.Instantiate<Skill>();
        if (Skill3 == null && Skill3Scene != null) Skill3 = Skill3Scene.Instantiate<Skill>();
        if (Skill4 == null && Skill4Scene != null) Skill4 = Skill4Scene.Instantiate<Skill>();
    }

    public override void ResetTurn()
    {
        RemainingMoveRange = MoveRange;
        Skill1?.ResetTurn();
        Skill2?.ResetTurn();
        Skill3?.ResetTurn();
        Skill4?.ResetTurn();
    }

    public override void SetSelected(bool selected)
    {
        if (AnimatedSprite != null)
        {
            AnimatedSprite.Modulate = selected
                ? new Color(1.8f, 1.4f, 0.9f, 1f)
                : new Color(1f, 1f, 1f, 1f);
        }

        // tile beneath stays highlighted while the piece is not in its default state
        OccupiedTile?.SetSelected(selected);
    }

    public bool CanIgnoreObstacleGroup(int obstacleGroup)
    {
        if (MoveGroups == null)
        {
            return false;
        }

        return MoveGroups.Contains(obstacleGroup);
    }

    public void MoveToTile(Tile targetTile)
    {
        if (OccupiedTile != null)
        {
            OccupiedTile.OccupiedPiece = null;
        }

        OccupiedTile = targetTile;
        targetTile.OccupiedPiece = this;
        Position = targetTile.Position;
    }

    public async Task MoveAlongPathAsync(List<Tile> path, float stepDuration = 0.1f)
    {
        if (path == null || path.Count <= 1)
        {
            return;
        }

        var steps = path.Skip(1).ToList();
        var finalTile = steps.LastOrDefault();

        if (OccupiedTile != null && OccupiedTile.OccupiedPiece == this)
        {
            OccupiedTile.OccupiedPiece = null;
        }

        foreach (var step in steps)
        {
            var tween = CreateTween();
            tween.TweenProperty(this, "position", step.Position, stepDuration)
                .SetTrans(Tween.TransitionType.Linear)
                .SetEase(Tween.EaseType.InOut);

            await ToSignal(tween, Tween.SignalName.Finished);

            if (step == finalTile)
            {
                OccupiedTile = step;
                step.OccupiedPiece = this;
            }
        }
    }
}
