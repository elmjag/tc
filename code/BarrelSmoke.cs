using Godot;

public partial class BarrelSmoke : Node3D, AnimatedNode
{
    const float MAX_SMOKE_SIZE = 3.6f;

    [Export]
    public MeshInstance3D SmokeCloud;

    public void ApplyPosture(NodePosture posture)
    {
        var bsPosture = (BarrelSmokePosture)posture;
        Position = bsPosture.Position;
        Rotation = new Vector3(0, bsPosture.Rotation, 0);

        var mesh = (SphereMesh)SmokeCloud.Mesh;
        var sizeMeters = MAX_SMOKE_SIZE * bsPosture.Size;
        mesh.Radius = sizeMeters;
        mesh.Height = sizeMeters / 2;
    }
}
