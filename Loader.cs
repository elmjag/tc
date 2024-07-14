using System.Collections.Generic;
using Godot;
using Godot.Collections;

public partial class Loader : Node
{
    Node3D GhostTank;
    List<Rid> TanksRids = new List<Rid>();
    PackedScene ghostTankScene = GD.Load<PackedScene>("res://scenes/ghost_tank.tscn");

    private void InstantiateTank(PackedScene tank_scene, Vector3 offset, float angle)
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
        var ground = GetNode("/root/root/ground");
        var body = (StaticBody3D)ground.FindChild("StaticBody3D");
        return new Array<Rid>(new Rid[1] { body.GetRid() });
    }

    public Node3D InstantiateGhostTank(Vector3 position, Vector3 rotation)
    {
        var ghost_tank = ghostTankScene.Instantiate<Node3D>();
        ghost_tank.Position = position;
        ghost_tank.Rotation = rotation;
        AddChild(ghost_tank);

        return ghost_tank;
    }

    public override void _Ready()
    {
        var scene = GD.Load<PackedScene>("res://scenes/tank.tscn");
        InstantiateTank(scene, new Vector3(-20f, 0, 0), Mathf.Pi / 10);
        InstantiateTank(scene, Vector3.Zero, 0f);
        InstantiateTank(scene, new Vector3(20f, 0, 0), -(Mathf.Pi / 10));

        scene = GD.Load<PackedScene>("res://scenes/ghost_tank.tscn");
        GhostTank = scene.Instantiate<Node3D>();
        GhostTank.Visible = false;
        AddChild(GhostTank);
    }
}
