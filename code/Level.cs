using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class Level : Node
{
    List<Tank> Tanks = new List<Tank>();
    Dictionary<Tank, TankPath> tankPaths = new Dictionary<Tank, TankPath>();

    public override void _Ready()
    {
        CreateTank(new Vector3(-20f, 0, 16f), Mathf.Pi / 4);
        CreateTank(new Vector3(0f, 0, 16f), 0);
        CreateTank(new Vector3(20f, 0, 16f), -(Mathf.Pi / 10));
    }

    void CreateTank(Vector3 position, float rotation)
    {
        var tank = Repo.Loader.InstantiateTank(position, rotation);
        tank.AnimationFinished += () => RemoveTankPath(tank);

        AddChild(tank);
        Tanks.Add(tank);
    }

    void RemoveGhostTanks(TankPath tankPath)
    {
        var waypoints = tankPath.GetWaypoints();
        var ghosts = waypoints.Take(waypoints.Count - 1);

        foreach (var node in ghosts)
        {
            RemoveChild(node);
        }
    }

    void RemoveTankPath(Tank tank)
    {
        RemoveGhostTanks(tankPaths[tank]);
        tankPaths.Remove(tank);
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
            var tankPath = Repo.Level.GetTankPath(tank);
            if (tankPath != null)
            {
                tank.StartTurnAnimation(turnAnimationStartTick, tankPath);
            }
        }
    }

    public Node3D CreateGhostTank(Vector3 position, Vector3 rotation)
    {
        var ghostTank = Repo.Loader.InstantiateGhostTank(position, rotation);
        AddChild(ghostTank);

        return ghostTank;
    }

    /* tank path methods */

    public IEnumerable<TankPath> GetTankPaths()
    {
        return tankPaths.Values;
    }

    ///
    /// returns null if the tank does not have a path created
    ///
    public TankPath GetTankPath(Tank tank)
    {
        if (!tankPaths.ContainsKey(tank))
        {
            return null;
        }

        return tankPaths[tank];
    }

    public void StartNewTankPath(Tank tank, Node3D firstWaypoint)
    {
        var tankPath = new TankPath();
        tankPath.AddWaypoint(tank);
        tankPath.AddWaypoint(firstWaypoint);

        if (tankPaths.ContainsKey(tank))
        {
            /* remove old path */
            RemoveGhostTanks(tankPaths[tank]);
        }

        tankPaths[tank] = tankPath;

        Repo.Overlays.Redraw();
    }

    public void AddNewWaypoint(Tank tank, Node3D waypoint)
    {
        tankPaths[tank].AddWaypoint(waypoint);
    }

    public void UpdateLastWaypoint(Tank tank)
    {
        tankPaths[tank].UpdateLastWaypoint();
        Repo.Overlays.Redraw();
    }

    public bool IsLastWayoutValid(Tank tank)
    {
        return tankPaths[tank].IsLastWaypointValid();
    }

    public void FinishPathCreation(Tank tank)
    {
        var tankPath = tankPaths[tank];

        /* remove last waypoint */
        var ghostTank = tankPath.RemoveLastWaypoint();
        RemoveChild(ghostTank);

        /* this is an 'empty' path now, remove it */
        if (tankPath.GetWaypoints().Count <= 1)
        {
            tankPaths.Remove(tank);
        }

        Repo.Overlays.Redraw();
    }
}
