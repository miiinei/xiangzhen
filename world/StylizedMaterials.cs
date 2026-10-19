using Godot;

public static class StylizedMaterials
{
    private const string MaterialPath = "res://shaders/low_poly.tres";
    private static readonly StringName BaseColorParameter = "base_color";
    private static readonly StringName SelectionGlowParameter = "selection_glow";
    private static readonly ShaderMaterial Template = GD.Load<ShaderMaterial>(MaterialPath);

    public static ShaderMaterial Create(Color color, Color glow = default)
    {
        var material = (ShaderMaterial)Template.Duplicate();
        material.SetShaderParameter(BaseColorParameter, color);
        material.SetShaderParameter(SelectionGlowParameter, glow);
        return material;
    }
}