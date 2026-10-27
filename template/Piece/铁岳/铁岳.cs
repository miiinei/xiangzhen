using Godot;

public partial class 铁岳 : Piece
{
    public override void _Ready()
    {
        // Keep the demo's existing traversal profile; facing/movement live in Piece.
        ObstacleGroup = ObstacleGroups.FlyingUnit;
        base._Ready();
    }
}
