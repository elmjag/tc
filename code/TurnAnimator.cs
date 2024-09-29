using System.Collections.Generic;
using Godot;

public partial class TurnAnimator : Node
{
    ulong TurnAnimationStartTick;
    Dictionary<Tank, TankTurnActions> TankTurns;

    public override void _Ready()
    {
        SetProcess(false);
    }

    public override void _Process(double delta)
    {
        var AnimationTime = Time.GetTicksMsec() - TurnAnimationStartTick;
        var finishedCounter = 0;

        foreach (var item in TankTurns)
        {
            var tank = item.Key;
            var turnActions = item.Value;

            var (finished, postures) = turnActions.GetTurnAnimationPostures(AnimationTime);
            foreach (var posture in postures)
            {
                var node3d = (Node3D)posture.Node;
                if (node3d.GetParent() == null)
                {
                    Repo.Level.AddChild(node3d);
                }

                posture.Node.ApplyPosture(posture.Posture);
            }

            if (finished)
            {
                finishedCounter += 1;
            }
        }

        if (TankTurns.Count == finishedCounter)
        {
            /* all turn animations are finished */
            SetProcess(false);
            EmitSignal(SignalName.AnimationFinished);
        }
    }

    /*
    * public API
    */
    [Signal]
    public delegate void AnimationFinishedEventHandler();

    public void StartTurnAnimation(Dictionary<Tank, TankTurnActions> tankTurns)
    {
        TurnAnimationStartTick = Time.GetTicksMsec();
        TankTurns = tankTurns;

        SetProcess(true);
    }
}
