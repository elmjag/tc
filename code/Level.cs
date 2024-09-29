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

        Repo.TurnAnimator.AnimationFinished += () => RemoveTankTrunActions();
    }

    void CreateNpcTank(Vector3 position, float rotation)
    {
        AddChild(Repo.Loader.InstantiateNpcTank(position, rotation));
    }

    void CreateTank(Vector3 position, float rotation)
    {
        var tank = Repo.Loader.InstantiateTank(position, rotation);

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

    void RemoveTankTrunActions()
    {
        foreach (var tank in TankTurns.Keys)
        {
            RemoveGhostTanks(tank);
        }
        TankTurns.Clear();
        Repo.Overlays.Redraw();
    }

    /*
    * public API
    */

    public void StartTurnAnimation()
    {
        Repo.TurnAnimator.StartTurnAnimation(TankTurns);
    }

    public Tank CreateGhostTank(TankPosture posture, TankTurnActions turnActions)
    {
        var ghostTank = Repo.Loader.InstantiateGhostTank(posture);
        AddChild(ghostTank);
        GhostTanks[turnActions].Insert(0, ghostTank);

        return ghostTank;
    }

    /*
    * tank turn actions methods
    */

    public IEnumerable<TankTurnActions> GetAllTankTurnActions()
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
    public Tank SetupNewTurn(Tank tank)
    {
        if (TankTurns.ContainsKey(tank))
        {
            RemoveGhostTanks(tank);
        }

        var tankPosture = new TankPosture(tank);
        var turn = new TankTurnActions(tankPosture);
        turn.AddMoveAction(tank);

        TankTurns[tank] = turn;
        GhostTanks[turn] = new List<Node3D>();

        Repo.Overlays.Redraw();

        return CreateGhostTank(tankPosture, turn);
    }

    ///
    /// returns new ghost tank instance
    ///
    public Tank AddMoveAction(Tank tank)
    {
        var turnActions = TankTurns[tank];
        var startPosture = turnActions.AddMoveAction(tank);

        return CreateGhostTank(startPosture, turnActions);
    }

    public void UpdateLastMoveAction(Tank tank, Tank moveTo)
    {
        TankTurns[tank].UpdateLastMoveAction(new TankPosture(moveTo));
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

        Repo.Overlays.Redraw();
    }
}
