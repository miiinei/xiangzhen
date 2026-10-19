using Godot;

public enum TileState { Grass, Mountain, Water }

public partial class Tile : Node3D
{
    private const float DefaultHexRadius = 1f;
    private const float DefaultOutlineWidth = 0.012f;
    private const float DefaultHighlightWidthScale = 2f;
    private const string OutlineWidthRangeHint = "0.001,0.2,0.001";
    private const string HighlightWidthScaleRangeHint = "1.0,10.0,0.1";
    private const float BaseMeshHeight = 0.16f;
    private const float TargetPreviewMeshHeight = 0.02f;
    private const int MeshRingCount = 1;
    private static readonly NodePath BaseMeshNodePath = "BaseMesh";
    private static readonly NodePath BaseOutlineNodePath = "BaseOutline";
    private static readonly NodePath TargetPreviewNodePath = "TargetPreview";
    private static readonly Color TargetPreviewColor = new(1f, 0.2f, 0.2f, 0.32f);
    private static readonly Color MountainColor = new(0.38f, 0.34f, 0.28f);
    private static readonly Color WaterColor = new(0.12f, 0.34f, 0.48f);
    private static readonly Color GrassColor = new(0.48f, 0.68f, 0.28f);

    [Export] public Vector2I GridPosition { get; set; }
    [Export] public float HexRadius { get; set; } = DefaultHexRadius;
    private TileState _state;

    [Export]
    public TileState State
    {
        get => _state;
        set
        {
            _state = value;
            if (IsNodeReady()) RefreshBaseMesh();
        }
    }
    [Export(PropertyHint.Range, OutlineWidthRangeHint)] public float OutlineWidth { get; set; } = DefaultOutlineWidth;
    [Export(PropertyHint.Range, HighlightWidthScaleRangeHint)] public float HighlightWidthScale { get; set; } = DefaultHighlightWidthScale;
    public World WorldRef { get; set; }
    public Piece OccupiedPiece { get; set; }
    public bool IsOccupied => OccupiedPiece != null;
    public int ObstacleGroup => State == TileState.Mountain ? ObstacleGroups.Mountain : ObstacleGroups.Ground;
    private static readonly Color NormalOutlineColor = new(0.68f, 0.7f, 0.72f, 0.8f);
    private static readonly Color HighlightOutlineColor = new(0.2f, 0.7f, 1f, 1f);
    private MeshInstance3D _baseMesh;
    private MeshInstance3D _baseOutline;
    private MeshInstance3D _targetPreview;
    private readonly bool[] _rangeHighlightedEdges = new bool[HexGeometry.EdgeCount];
    private bool _isSelected;

    public override void _Ready()
    {
        _baseMesh = GetNodeOrNull<MeshInstance3D>(BaseMeshNodePath);
        _baseOutline = GetNodeOrNull<MeshInstance3D>(BaseOutlineNodePath);
        _targetPreview = GetNodeOrNull<MeshInstance3D>(TargetPreviewNodePath);

        RefreshBaseMesh();

        if (_baseOutline != null)
        {
            _baseOutline.MaterialOverride = CreateLineMaterial(Colors.White);
            RefreshBaseOutline();
        }

        if (_targetPreview != null)
        {
            _targetPreview.Mesh = CreateHexMesh(TargetPreviewMeshHeight);
            _targetPreview.MaterialOverride = CreateMaterial(TargetPreviewColor, true);
        }
    }

    public void SetSelected(bool selected)
    {
        _isSelected = selected;
        RefreshBaseOutline();
    }

    public void SetState(TileState state)
    {
        State = state;
        WorldRef?.NotifyBoardStateChanged();
    }

    public void SetTargetPreview(bool visible) => _targetPreview.Visible = visible;
    public void SetRangeOverlay(bool enabled, Color? color = null)
    {
        for (int edge = 0; edge < _rangeHighlightedEdges.Length; edge++)
        {
            _rangeHighlightedEdges[edge] = enabled;
        }
        RefreshBaseOutline();
    }

    public void SetRangeOutlineEdges(bool[] visibleEdges, Color color)
    {
        for (int edge = 0; edge < _rangeHighlightedEdges.Length; edge++)
        {
            _rangeHighlightedEdges[edge] = visibleEdges != null && edge < visibleEdges.Length && visibleEdges[edge];
        }
        RefreshBaseOutline();
    }
    public Vector2[] GetHexagonVertices()
    {
        var vertices = new Vector2[HexGeometry.EdgeCount];
        for (int index = 0; index < vertices.Length; index++)
        {
            float angle = Mathf.DegToRad(HexGeometry.FirstVertexAngleDegrees + index * HexGeometry.AngleStepDegrees);
            vertices[index] = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * HexRadius;
        }
        return vertices;
    }
    private Color GetBaseColor() => State switch { TileState.Mountain => MountainColor, TileState.Water => WaterColor, _ => GrassColor };

    private void RefreshBaseMesh()
    {
        if (_baseMesh == null) return;
        _baseMesh.Mesh = CreateHexMesh(BaseMeshHeight);
        _baseMesh.MaterialOverride = StylizedMaterials.Create(GetBaseColor(), grass: State == TileState.Grass);
    }

    private CylinderMesh CreateHexMesh(float height) => new() { TopRadius = HexRadius, BottomRadius = HexRadius, Height = height, RadialSegments = HexGeometry.EdgeCount, Rings = MeshRingCount };
    private static StandardMaterial3D CreateMaterial(Color color, bool transparent = false) => new() { AlbedoColor = color, Transparency = transparent ? BaseMaterial3D.TransparencyEnum.Alpha : BaseMaterial3D.TransparencyEnum.Disabled };
    private void RefreshBaseOutline()
    {
        if (_baseOutline == null) return;

        var vertices = new Vector3[HexGeometry.EdgeCount];
        var widths = new float[HexGeometry.EdgeCount];
        var colors = new Color[HexGeometry.EdgeCount];
        for (int index = 0; index < HexGeometry.EdgeCount; index++)
        {
            float angle = Mathf.DegToRad(HexGeometry.FirstVertexAngleDegrees + index * HexGeometry.AngleStepDegrees);
            vertices[index] = new Vector3(Mathf.Cos(angle) * HexRadius, 0f, Mathf.Sin(angle) * HexRadius);
            bool highlighted = _isSelected || _rangeHighlightedEdges[index];
            widths[index] = HexRadius * OutlineWidth * (highlighted ? HighlightWidthScale : 1f);
            colors[index] = highlighted ? HighlightOutlineColor : NormalOutlineColor;
        }

        var mesh = new ImmediateMesh();
        mesh.SurfaceBegin(Mesh.PrimitiveType.Triangles);
        for (int index = 0; index < HexGeometry.EdgeCount; index++)
        {
            int nextIndex = (index + 1) % HexGeometry.EdgeCount;
            AddOutlineBand(mesh, vertices[index], vertices[nextIndex], widths[index], colors[index]);

            int previousIndex = index == 0 ? HexGeometry.EdgeCount - 1 : index - 1;
            Vector3 previousInward = GetEdgeInward(vertices[previousIndex], vertices[index], widths[previousIndex]);
            Vector3 currentInward = GetEdgeInward(vertices[index], vertices[nextIndex], widths[index]);
            Color jointColor = _isSelected || _rangeHighlightedEdges[previousIndex] || _rangeHighlightedEdges[index]
                ? HighlightOutlineColor
                : NormalOutlineColor;
            AddColoredVertex(mesh, vertices[index], jointColor);
            AddColoredVertex(mesh, vertices[index] + previousInward, jointColor);
            AddColoredVertex(mesh, vertices[index] + currentInward, jointColor);
        }
        mesh.SurfaceEnd();
        _baseOutline.Mesh = mesh;
    }

    private static void AddOutlineBand(ImmediateMesh mesh, Vector3 start, Vector3 end, float width, Color color)
    {
        Vector3 inward = GetEdgeInward(start, end, width);
        AddColoredVertex(mesh, start, color);
        AddColoredVertex(mesh, end, color);
        AddColoredVertex(mesh, end + inward, color);
        AddColoredVertex(mesh, start, color);
        AddColoredVertex(mesh, end + inward, color);
        AddColoredVertex(mesh, start + inward, color);
    }

    private static Vector3 GetEdgeInward(Vector3 start, Vector3 end, float width) =>
        -((start + end) * 0.5f).Normalized() * width;

    private static void AddColoredVertex(ImmediateMesh mesh, Vector3 vertex, Color color)
    {
        mesh.SurfaceSetColor(color);
        mesh.SurfaceAddVertex(vertex);
    }

    private static StandardMaterial3D CreateLineMaterial(Color color) => new()
    {
        AlbedoColor = color,
        ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded,
        VertexColorUseAsAlbedo = true,
        CullMode = BaseMaterial3D.CullModeEnum.Disabled
    };
}