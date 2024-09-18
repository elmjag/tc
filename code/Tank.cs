using Godot;

public partial class Tank : Node3D
{
    [Signal]
    public delegate void AnimationFinishedEventHandler();

    /*
     * time stamp when this turn animation started,
     * as reported by Time.GetTicksMsec()
     */
    ulong TurnAnimationStartTick = 0;
    TankPath TankPath;

    public override void _Ready()
    {
        SetProcess(false);
    }

    public override void _Process(double delta)
    {
        var AnimationTime = Time.GetTicksMsec() - TurnAnimationStartTick;
        var (pathFinished, posture) = TankPath.GetPosture(AnimationTime);

        Position = posture.Position;
        Rotation = new Vector3(Rotation.X, posture.Rotation, Rotation.Z);

        /* we have reached the end of the path */
        if (pathFinished)
        {
            SetProcess(false);
            EmitSignal(SignalName.AnimationFinished);
        }
    }

    /*
     *
     * public API
     *
     */

    public static Tank GetByCollider(StaticBody3D collider)
    {
        return (Tank)collider.GetNode("../..");
    }

    public Rid getColliderRid()
    {
        var body = (StaticBody3D)FindChild("StaticBody3D");
        return body.GetRid();
    }

    public void StartTurnAnimation(ulong turnAnimationStartTick, TankPath tankPath)
    {
        TurnAnimationStartTick = turnAnimationStartTick;
        TankPath = tankPath;
        SetProcess(true);
    }
}
