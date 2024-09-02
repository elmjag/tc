using System.Collections.Generic;
using Godot;
using Godot.Collections;

public partial class Loader : Node
{
    List<Rid> TanksRids = new List<Rid>();

    void InstantiateTank(PackedScene tank_scene, Vector3 offset, float angle)
    {
        var tank = tank_scene.Instantiate<Tank>();
        tank.Translate(offset);
        tank.RotateY(angle);

        TanksRids.Add(tank.getColliderRid());
        AddChild(tank);
    }

    public Array<Rid> GetTankRids()
    {
        return new Array<Rid>(TanksRids);
    }

    public Array<Rid> GetGroundRid()
    {
        var body = (StaticBody3D)Repo.Ground.FindChild("StaticBody3D");
        return new Array<Rid>(new Rid[1] { body.GetRid() });
    }

    public Node3D InstantiateGhostTank(Vector3 position, Vector3 rotation)
    {
        var ghost_tank = Repo.GhostTankScene.Instantiate<Node3D>();
        ghost_tank.Position = position;
        ghost_tank.Rotation = rotation;
        AddChild(ghost_tank);

        return ghost_tank;
    }

    public void RemoveGhostTank(Node ghostTank)
    {
        RemoveChild(ghostTank);
    }

    public override void _Ready()
    {
        InstantiateTank(Repo.TankScene, new Vector3(-20f, 0, 0), Mathf.Pi / 10);
        InstantiateTank(Repo.TankScene, Vector3.Zero, 0f);
        InstantiateTank(Repo.TankScene, new Vector3(20f, 0, 0), -(Mathf.Pi / 10));
    }
}
