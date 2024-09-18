using Godot;

public partial class Repo : Node
{
    static Node3D _CameraRig;
    static Camera3D _Camera;
    static MeshInstance3D _Ground;
    static Level _Level;
    static Loader _Loader;
    static Overlays _Overlays;

    /* scenes */
    public static PackedScene TankScene = LoadScene("tank");
    public static PackedScene GhostTankScene = LoadScene("ghost_tank");

    static PackedScene LoadScene(string name)
    {
        return GD.Load<PackedScene>($"res://scenes/{name}.tscn");
    }

    public override void _Ready()
    {
        var game = GetNode<Node3D>("/root/Game");

        _CameraRig = game.GetNode<Node3D>("CameraRig");
        _Camera = _CameraRig.GetNode<Camera3D>("Camera");
        _Ground = _CameraRig.GetNode<MeshInstance3D>("Ground");
        _Level = game.GetNode<Level>("Level");
        _Loader = game.GetNode<Loader>("Loader");
        _Overlays = game.GetNode<Overlays>("Overlays");
    }

    public static Node3D CameraRig
    {
        get { return _CameraRig; }
    }

    public static Camera3D Camera
    {
        get { return _Camera; }
    }

    public static MeshInstance3D Ground
    {
        get { return _Ground; }
    }

    public static Vector2 GroundPlaneSize
    {
        get { return ((PlaneMesh)_Ground.Mesh).Size; }
    }

    public static Level Level
    {
        get { return _Level; }
    }

    public static Loader Loader
    {
        get { return _Loader; }
    }

    public static Overlays Overlays
    {
        get { return _Overlays; }
    }

    /*
     * number of pixels in the overlay texture,
     *
     * overlay texture is quadratic,
     * so width and height are equal
     */
    public static int OverlaysPixels
    {
        get { return _Overlays.Size.X; }
    }
}
