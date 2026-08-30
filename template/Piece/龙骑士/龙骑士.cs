using Godot;

public partial class 龙骑士 : Piece
{
    public override void _Ready()
    {
        base._Ready();

        MoveRange = 3;
        ObstacleGroup = 2;
        MoveGroups = new[] { 0, 2 };
    }
}
