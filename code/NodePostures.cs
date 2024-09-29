using Godot;

public abstract class NodePosture { }

public interface AnimatedNode
{
    public void ApplyPosture(NodePosture posture);
}

public class BarrelSmokePosture : NodePosture
{
    public Vector3 Position;

    /* as a fraction of the max size, 0.0..1.0 */
    public float Size;
    public float Rotation;

    public BarrelSmokePosture(Vector3 position, float rotation, float size)
    {
        Position = position;
        Rotation = rotation;
        Size = size;
    }
}

public class TankPosture : NodePosture
{
    public Vector3 Position;

    /* rotation in the plane, radians */
    public float BaseRotation;
    public float TurretRotation;

    public TankPosture(Vector3 Position, float BaseRotation, float TurretRotation)
    {
        this.Position = Position;
        this.BaseRotation = BaseRotation;
        this.TurretRotation = TurretRotation;
    }

    public TankPosture(TankPosture orig)
    {
        Position = orig.Position;
        BaseRotation = orig.BaseRotation;
        TurretRotation = orig.TurretRotation;
    }

    public TankPosture(Tank tank)
    {
        Position = tank.Position;
        BaseRotation = tank.Rotation.Y;
        TurretRotation = tank.Turret.Rotation.Y;
    }

    public Vector3 ForwardDirection()
    {
        return Vector3.Forward.Rotated(Vector3.Up, BaseRotation);
    }

    public Vector3 BackDirection()
    {
        return Vector3.Back.Rotated(Vector3.Up, BaseRotation);
    }
}

public class AnimationPosture
{
    public AnimatedNode Node;
    public NodePosture Posture;

    public AnimationPosture(AnimatedNode node, NodePosture posture)
    {
        Node = node;
        Posture = posture;
    }
}
