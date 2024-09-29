using Godot;

public partial class Tank : Vehicle, AnimatedNode
{
    void SetRotation(Node3D node, float rotation)
    {
        node.Rotation = new Vector3(Rotation.X, rotation, Rotation.Z);
    }

    float GetGunLength()
    {
        var mesh = (CylinderMesh)((MeshInstance3D)Gun).Mesh;
        return mesh.Height;
    }

    /*
     *
     * public API
     *
     */

    [Export]
    public Node3D Turret;

    [Export]
    public Node3D Gun;

    public void ApplyPosture(NodePosture posture)
    {
        var tankPosture = (TankPosture)posture;
        Position = tankPosture.Position;
        SetRotation(this, tankPosture.BaseRotation);
        SetRotation(Turret, tankPosture.TurretRotation);
    }

    public Vector3 GetBarrelEndPosition()
    {
        var forwardDir = Vector3.Forward.Rotated(Vector3.Up, Gun.GlobalRotation.Y);
        return Gun.GlobalPosition + forwardDir * GetGunLength() / 2;
    }
}
