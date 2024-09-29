using Godot;

public partial class Tank : Vehicle
{
    [Signal]
    public delegate void AnimationFinishedEventHandler();

    /*
     * time stamp when this turn animation started,
     * as reported by Time.GetTicksMsec()
     */
    ulong TurnAnimationStartTick = 0;
    TankTurnActions TurnActions;

    public override void _Ready()
    {
        SetProcess(false);
    }

    public override void _Process(double delta)
    {
        var AnimationTime = Time.GetTicksMsec() - TurnAnimationStartTick;
        var (pathFinished, posture) = TurnActions.GetAnimatedPosture(AnimationTime);

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

    public void StartTurnAnimation(ulong turnAnimationStartTick, TankTurnActions turnActions)
    {
        TurnAnimationStartTick = turnAnimationStartTick;
        TurnActions = turnActions;
        SetProcess(true);
    }
}
