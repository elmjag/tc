using System.Collections.Generic;
using Godot;
using Godot.Collections;

public partial class Loader : Node
{
    List<Rid> TanksRids = new List<Rid>();

    /*
     *
     * public API
     *
     */

    public Array<Rid> GetTankRids()
    {
        return new Array<Rid>(TanksRids);
    }

    public Array<Rid> GetGroundRid()
    {
        var body = (StaticBody3D)Repo.Ground.FindChild("StaticBody3D");
        return new Array<Rid>(new Rid[1] { body.GetRid() });
    }

    public Tank InstantiateTank(Vector3 offset, float angle)
    {
        var tank = Repo.TankScene.Instantiate<Tank>();
        tank.Translate(offset);
        tank.RotateY(angle);

        TanksRids.Add(tank.getColliderRid());

        return tank;
    }

    public Node3D InstantiateGhostTank(Vector3 position, Vector3 rotation)
    {
        var ghostTank = Repo.GhostTankScene.Instantiate<Node3D>();
        ghostTank.Position = position;
        ghostTank.Rotation = rotation;

        return ghostTank;
    }
}
