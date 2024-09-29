using Godot;

public struct Posture
{
    public Vector3 Position;

    /* rotation in the plane, radians */
    public float BaseRotation;
    public float TurretRotation;

    public Posture(Vector3 Position, float BaseRotation, float TurretRotation)
    {
        this.Position = Position;
        this.BaseRotation = BaseRotation;
        this.TurretRotation = TurretRotation;
    }

    public Posture(Tank tank)
    {
        Position = tank.Position;
        BaseRotation = tank.Rotation.Y;
        TurretRotation = tank.Turret.Rotation.Y;
    }

    public readonly Vector3 ForwardDirection()
    {
        return Vector3.Forward.Rotated(Vector3.Up, BaseRotation);
    }

    public readonly Vector3 BackDirection()
    {
        return Vector3.Back.Rotated(Vector3.Up, BaseRotation);
    }
}
