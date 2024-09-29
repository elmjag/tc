using Godot;

public struct Posture
{
    public Vector3 Position;
    public float Rotation; /* rotation in the plane, radians */

    public Posture(Vector3 Position, float Rotation)
    {
        this.Position = Position;
        this.Rotation = Rotation;
    }

    public Posture(Node3D node)
    {
        Position = node.Position;
        Rotation = node.Rotation.Y;
    }

    public readonly Vector3 ForwardDirection()
    {
        return Vector3.Forward.Rotated(Vector3.Up, Rotation);
    }

    public readonly Vector3 BackDirection()
    {
        return Vector3.Back.Rotated(Vector3.Up, Rotation);
    }
}
