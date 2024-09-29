using System.Collections.Generic;
using Godot;
using Godot.Collections;

public partial class Loader : Node
{
    List<Rid> TanksRids = new List<Rid>();
    List<Rid> NpcTanksRids = new List<Rid>();

    public Rid GetGroundRid()
    {
        var body = (StaticBody3D)Repo.Ground.FindChild("StaticBody3D");
        return body.GetRid();
    }

    /*
     *
     * public API
     *
     */

    public Array<Rid> GetRids(
        bool excludeGround = false,
        bool excludeTanks = false,
        bool excludeNpcTanks = false
    )
    {
        List<Rid> rids = new List<Rid>();

        if (!excludeGround)
        {
            rids.Add(GetGroundRid());
        }

        if (!excludeTanks)
        {
            rids.AddRange(TanksRids);
        }

        if (!excludeNpcTanks)
        {
            rids.AddRange(NpcTanksRids);
        }

        return new Array<Rid>(rids);
    }

    public Tank InstantiateTank(Vector3 offset, float angle)
    {
        var tank = Repo.TankScene.Instantiate<Tank>();
        tank.Translate(offset);
        tank.RotateY(angle);

        TanksRids.Add(tank.GetColliderRid());

        return tank;
    }

    public Node3D InstantiateNpcTank(Vector3 offset, float angle)
    {
        var tank = Repo.NpcTankScene.Instantiate<NpcTank>();
        tank.Translate(offset);
        tank.RotateY(angle);

        NpcTanksRids.Add(tank.GetColliderRid());

        return tank;
    }

    public Tank InstantiateGhostTank(TankPosture posture)
    {
        var ghostTank = Repo.GhostTankScene.Instantiate<Tank>();

        ghostTank.Position = posture.Position;
        ghostTank.Rotation = new Vector3(0, posture.BaseRotation, 0);
        ghostTank.Turret.Rotation = new Vector3(0, posture.TurretRotation, 0);

        return ghostTank;
    }

    public BarrelSmoke InstantiateBarrelSmoke()
    {
        return Repo.BarrelSmoke.Instantiate<BarrelSmoke>();
    }
}
