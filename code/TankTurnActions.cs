using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Godot;

public abstract class TurnAction
{
    public abstract float AnimationLength { get; }

    /// animationTick - animation tick in seconds
    public abstract AnimationPosture[] GetTurnAnimationPostures(float animationTick);

    public abstract TankPosture GetFinalPosture();

    /// Is this turn action executable in current state.
    public abstract bool IsValid();

    public abstract void DrawOverlays(Node2D canavas);

    ///
    /// this method is called when animation of this action have finished.
    ///
    public virtual void AnimationFinished()
    {
        /* this is a NOP by default */
    }
}

///
/// A list of action a player tank will perform during next turn.
///
public class TankTurnActions
{
    TankPosture StartPosture;
    List<TurnAction> Actions = new List<TurnAction>();

    MoveAction GetLastMoveAction()
    {
        foreach (var action in Actions)
        {
            if (action is MoveAction moveAction)
            {
                return moveAction;
            }
        }
        /* there are no move actions */
        return null;
    }

    TankPosture GetLastActionFinalPosture()
    {
        var lastAction = GetLastAction();
        if (lastAction is null)
        {
            return StartPosture;
        }

        return lastAction.GetFinalPosture();
    }

    /*
    * public API
    */

    public TankTurnActions(TankPosture startPosture)
    {
        StartPosture = startPosture;
    }

    /// returns initial 'to' posture
    public TankPosture AddMoveAction(Tank tank)
    {
        /* we start with same from and to postures */
        var initialFromTo = GetLastActionFinalPosture();
        var moveAction = new MoveAction(tank, initialFromTo, initialFromTo);
        Actions.Insert(0, moveAction);

        return initialFromTo;
    }

    public void AddFireAction(Tank shooter, Node3D target)
    {
        var fireAction = new FireAction(shooter, GetLastActionFinalPosture(), target);
        Actions.Insert(0, fireAction);
    }

    public bool LastTankActionValid()
    {
        /* this method not defined if there are no actions */
        Trace.Assert(Actions.Count > 0);

        var lastAction = Actions[0];
        return lastAction.IsValid();
    }

    public void UpdateLastMoveAction(TankPosture moveTo)
    {
        var lastMoveAction = GetLastMoveAction();
        Trace.Assert(lastMoveAction != null);
        lastMoveAction.UpdateToWaypoint(moveTo);
    }

    public TurnAction GetLastAction()
    {
        if (IsEmpty())
        {
            return null;
        }

        return Actions[0];
    }

    public void RemoveLastAction()
    {
        if (IsEmpty())
        {
            return;
        }

        Actions.RemoveAt(0);
    }

    public void DrawOverlays(Node2D canavas)
    {
        foreach (var action in Actions)
        {
            action.DrawOverlays(canavas);
        }
    }

    public bool IsEmpty()
    {
        return Actions.Count == 0;
    }

    public (bool, TurnAction, AnimationPosture[]) GetNextActionPostures(ulong AnimationTick)
    {
        var currentTickSecs = AnimationTick / 1000f;

        /*
         * actions are stored in 'reversed' order,
         * i.e. last added action is at the beggining of the list,
         * put them into temprally first order
         */
        var temporallyFirstActions = new List<TurnAction>(Actions);
        temporallyFirstActions.Reverse();

        foreach (var action in temporallyFirstActions)
        {
            if (currentTickSecs < action.AnimationLength)
            {
                return (false, action, action.GetTurnAnimationPostures(currentTickSecs));
            }
            currentTickSecs -= action.AnimationLength;
        }

        var lastAction = temporallyFirstActions.Last();
        return (true, lastAction, lastAction.GetTurnAnimationPostures(lastAction.AnimationLength));
    }

    TurnAction PreviousAction = null;

    public (bool, AnimationPosture[]) GetTurnAnimationPostures(ulong AnimationTick)
    {
        var (finished, action, postures) = GetNextActionPostures(AnimationTick);
        if (action != PreviousAction)
        {
            PreviousAction?.AnimationFinished();
            PreviousAction = action;
        }

        if (finished)
        {
            PreviousAction.AnimationFinished();
        }

        return (finished, postures);
    }
}
