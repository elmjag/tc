using System.Collections.Generic;
using Godot;

public partial class Level : Node
{
    List<Tank> Tanks = new List<Tank>();

    Dictionary<Tank, TankTurnActions> TankTurns = new Dictionary<Tank, TankTurnActions>();
    Dictionary<TankTurnActions, List<Node3D>> GhostTanks =
        new Dictionary<TankTurnActions, List<Node3D>>();

    public override void _Ready()
    {
        CreateTank(new Vector3(-20f, 0, 16f), Mathf.Pi / 4);
        CreateTank(new Vector3(0f, 0, 16f), 0);
        CreateTank(new Vector3(20f, 0, 16f), -(Mathf.Pi / 10));

        CreateNpcTank(new Vector3(-10f, 0, -32f), 0);
        CreateNpcTank(new Vector3(10f, 0, -32f), 0);
    }

    void CreateNpcTank(Vector3 position, float rotation)
    {
        AddChild(Repo.Loader.InstantiateNpcTank(position, rotation));
    }

    void CreateTank(Vector3 position, float rotation)
    {
        var tank = Repo.Loader.InstantiateTank(position, rotation);
        tank.AnimationFinished += () => RemoveTankTrunActions(tank);

        AddChild(tank);
        Tanks.Add(tank);
    }

    void RemoveGhostTanks(Tank tank)
    {
        var turnActions = TankTurns[tank];
        foreach (var node in GhostTanks[turnActions])
        {
            RemoveChild(node);
        }

        GhostTanks.Remove(turnActions);
    }

    void RemoveTankTrunActions(Tank tank)
    {
        RemoveGhostTanks(tank);
        TankTurns.Remove(tank);
        Repo.Overlays.Redraw();
    }

    /*
     * public API
     */

    public void StartTurnAnimation()
    {
        var turnAnimationStartTick = Time.GetTicksMsec();
        foreach (var tank in Tanks)
        {
            var turnActions = Repo.Level.GetTankTurnActions(tank);
            if (turnActions != null)
            {
                tank.StartTurnAnimation(turnAnimationStartTick, turnActions);
            }
        }
    }

    public Node3D CreateGhostTank(Posture posture, TankTurnActions turnActions)
    {
        var ghostTank = Repo.Loader.InstantiateGhostTank(posture);
        AddChild(ghostTank);
        GhostTanks[turnActions].Insert(0, ghostTank);

        return ghostTank;
    }

    /* tank turn actions methods */

    public IEnumerable<TankTurnActions> GetTankTurnActions()
    {
        return TankTurns.Values;
    }

    ///
    /// returns null if the tank does not have any turn actions created
    ///
    public TankTurnActions GetTankTurnActions(Tank tank)
    {
        if (!TankTurns.ContainsKey(tank))
        {
            return null;
        }

        return TankTurns[tank];
    }

    ///
    /// return initial ghost tank
    ///
    public Node3D SetupNewTurn(Tank tank)
    {
        if (TankTurns.ContainsKey(tank))
        {
            RemoveGhostTanks(tank);
        }

        var tankPosture = new Posture(tank);
        var turn = new TankTurnActions(tankPosture);
        turn.AddMoveAction();

        TankTurns[tank] = turn;
        GhostTanks[turn] = new List<Node3D>();

        Repo.Overlays.Redraw();

        return CreateGhostTank(tankPosture, turn);
    }

    ///
    /// returns new ghost tank instance
    ///
    public Node3D AddMoveAction(Tank tank)
    {
        var turnActions = TankTurns[tank];
        var startPosture = turnActions.AddMoveAction();

        return CreateGhostTank(startPosture, turnActions);
    }

    public void AddFireAction(Tank tank, Node3D target)
    {
        // TODO: implement this
        GD.Print("BANG BANG");
    }

    public void UpdateLastMoveAction(Tank tank, Node3D moveTo)
    {
        TankTurns[tank].UpdateLastMoveAction(new Posture(moveTo));
        Repo.Overlays.Redraw();
    }

    public bool LastTankActionValid(Tank tank)
    {
        return TankTurns[tank].LastTankActionValid();
    }

    public void FinalizeTurnActions(Tank tank)
    {
        var turnActions = TankTurns[tank];
        turnActions.RemoveLastAction();

        /* remove last ghost tank */
        var ghostTanks = GhostTanks[turnActions];
        var lastGhostTank = ghostTanks[0];
        ghostTanks.RemoveAt(0);
        RemoveChild(lastGhostTank);

        if (turnActions.IsEmpty())
        {
            /* this is an 'empty' actions list, remove it */
            TankTurns.Remove(tank);
        }
    }
}
