using Godot;

public partial class Repo : Node
{
    static Camera3D CameraNode;
    static MeshInstance3D GroundNode;
    static Loader LoaderNode;

    public override void _Ready()
    {
        CameraNode = GetNode<Camera3D>("/root/root/gfx/camera");
        GroundNode = GetNode<MeshInstance3D>("/root/root/gfx/camera/ground");
        LoaderNode = GetNode<Loader>(new NodePath("/root/root/loader"));
    }

    public static Camera3D Camera
    {
        get { return CameraNode; }
    }

    public static MeshInstance3D Ground
    {
        get { return GroundNode; }
    }

    public static Loader Loader
    {
        get { return LoaderNode; }
    }
}
