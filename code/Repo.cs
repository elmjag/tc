using Godot;

public partial class Repo : Node
{
    static Node3D _CameraRig;
    static Camera3D _Camera;
    static MeshInstance3D _Ground;
    static Level _Level;
    static Loader _Loader;
    static TurnAnimator _TurnAnimator;
    static Overlays _Overlays;
    static AimMark _AimMark;

    /* scenes */
    public static PackedScene TankScene = LoadScene("tank");
    public static PackedScene NpcTankScene = LoadScene("npc_tank");
    public static PackedScene GhostTankScene = LoadScene("ghost_tank");
    public static PackedScene BarrelSmoke = LoadScene("barrel_smoke");

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
        _TurnAnimator = game.GetNode<TurnAnimator>("TurnAnimator");
        _Overlays = game.GetNode<Overlays>("Overlays");
        _AimMark = _Overlays.GetNode<AimMark>("AimMark");
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

    public static TurnAnimator TurnAnimator
    {
        get { return _TurnAnimator; }
    }

    public static Overlays Overlays
    {
        get { return _Overlays; }
    }

    public static AimMark AimMark
    {
        get { return _AimMark; }
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
