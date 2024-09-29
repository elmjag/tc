using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Godot;

abstract class TurnAction
{
    public abstract float AnimationLength { get; }

    /// animationTick - animation tick in seconds
    public abstract Posture GetAnimatedPosture(float animationTick);

    /// Is this turn action executable in current state.
    public abstract bool IsValid();

    public abstract void DrawOverlays(Node2D canavas);
}

///
/// A list of action a player tank will perform during next turn.
///
public class TankTurnActions
{
    Posture StartPosture;
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

    Posture GetLastMovePosture()
    {
        var lastMoveAction = GetLastMoveAction();
        if (lastMoveAction != null)
        {
            return lastMoveAction.To;
        }

        return StartPosture;
    }

    /*
    * public API
    */

    public TankTurnActions(Posture startPosture)
    {
        StartPosture = startPosture;
    }

    /// returns initial 'to' posture
    public Posture AddMoveAction()
    {
        /* we start with same from and to postures */
        var initialFromTo = GetLastMovePosture();
        var moveAction = new MoveAction(initialFromTo, initialFromTo);
        Actions.Insert(0, moveAction);

        return initialFromTo;
    }

    public bool LastTankActionValid()
    {
        /* this method not defined if there are no actions */
        Trace.Assert(Actions.Count > 0);

        var lastAction = Actions[0];
        return lastAction.IsValid();
    }

    public void UpdateLastMoveAction(Posture moveTo)
    {
        var lastMoveAction = GetLastMoveAction();
        Trace.Assert(lastMoveAction != null);
        lastMoveAction.UpdateToWaypoint(moveTo);
    }

    public void RemoveLastAction()
    {
        Actions.RemoveAt(0);
    }

    public void AddShootAction() { }

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

    public (bool, Posture) GetAnimatedPosture(ulong AnimationTick)
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
                return (false, action.GetAnimatedPosture(currentTickSecs));
            }
            currentTickSecs -= action.AnimationLength;
        }

        var lastAction = temporallyFirstActions.Last();
        var lastPosture = lastAction.GetAnimatedPosture(lastAction.AnimationLength);
        return (true, lastPosture);
    }
}
