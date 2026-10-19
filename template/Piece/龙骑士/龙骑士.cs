using Godot;

public partial class 龙骑士 : Piece
{
    private const int DragonKnightMoveRange = 3;
    private static readonly int[] DragonKnightTraversableGroups =
    {
        ObstacleGroups.Ground,
        ObstacleGroups.FlyingUnit
    };

    public override void _Ready()
    {
        base._Ready();

        MoveRange = DragonKnightMoveRange;
        ObstacleGroup = ObstacleGroups.FlyingUnit;
        MoveGroups = DragonKnightTraversableGroups;
    }
}
