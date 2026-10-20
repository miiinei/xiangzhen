using Godot;

public static class StylizedMaterials
{
    private const string MaterialPath = "res://shaders/low_poly.tres";
    private const string GrassMaterialPath = "res://shaders/grassland.tres";
    private static readonly StringName BaseColorParameter = "base_color";
    private static readonly StringName SelectionGlowParameter = "selection_glow";
    private static readonly ShaderMaterial Template = GD.Load<ShaderMaterial>(MaterialPath);
    private static readonly ShaderMaterial GrassTemplate = GD.Load<ShaderMaterial>(GrassMaterialPath);

    public static ShaderMaterial Create(Color color, Color glow = default, bool grass = false)
    {
        var material = (ShaderMaterial)(grass ? GrassTemplate : Template).Duplicate();
        material.SetShaderParameter(BaseColorParameter, color);
        material.SetShaderParameter(SelectionGlowParameter, glow);
        return material;
    }
}