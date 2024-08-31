using Godot;

public partial class Repo : Node
{
    static Node3D _CameraRig;
    static Camera3D _Camera;
    static MeshInstance3D _Ground;
    static Loader _Loader;
    static Overlays _Overlays;

    public override void _Ready()
    {
        var root = GetNode<Node3D>("/root/root");

        _CameraRig = root.GetNode<Node3D>("CameraRig");
        _Camera = _CameraRig.GetNode<Camera3D>("Camera");
        _Ground = _CameraRig.GetNode<MeshInstance3D>("Ground");
        _Loader = root.GetNode<Loader>("Loader");
        _Overlays = root.GetNode<Overlays>("Overlays");
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
