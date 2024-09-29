using System;
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

    void SetRotation(Node3D node, float rotation)
    {
        node.Rotation = new Vector3(Rotation.X, rotation, Rotation.Z);
    }

    public override void _Process(double delta)
    {
        var AnimationTime = Time.GetTicksMsec() - TurnAnimationStartTick;
        var (pathFinished, posture) = TurnActions.GetAnimatedPosture(AnimationTime);

        Position = posture.Position;
        SetRotation(this, posture.BaseRotation);
        SetRotation(Turret, posture.TurretRotation);

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

    [Export]
    public Node3D Turret;

    public void StartTurnAnimation(ulong turnAnimationStartTick, TankTurnActions turnActions)
    {
        TurnAnimationStartTick = turnAnimationStartTick;
        TurnActions = turnActions;
        SetProcess(true);
    }
}
