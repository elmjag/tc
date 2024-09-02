using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class Overlays : SubViewport
{
    /* selected tank marker widget */
    Node2D SelectedTank;

    /* path outlines */
    Dictionary<Tank, PathOutline> TankPaths = new Dictionary<Tank, PathOutline>();

    public override void _Ready()
    {
        var ground = Repo.Ground;

        /* draw overlays on ground mesh */
        var material = (StandardMaterial3D)ground.MaterialOverlay;
        material.AlbedoTexture = GetTexture();

        /* grab reference to selected tank widget */
        SelectedTank = (Node2D)FindChild("SelectedTank");
    }

    void RemoveTankPath(PathOutline path)
    {
        /*
         * remove all ghost tank nodes for this path
         */
        var waypointNodes = path.GetWaypointNodes();

        /* skip the last node, as it's not a ghost tank, but the 'real' one */
        var ghostNodes = waypointNodes.Take(waypointNodes.Count - 1);
        foreach (var node in ghostNodes)
        {
            node.GetParent().RemoveChild(node);
        }

        /* remove the path outline overlay widget itself */
        RemoveChild(path);
    }

    /*
     *
     * public API
     *
     */

    public void Redraw()
    {
        /* redraw all tank path outlines */
        foreach (var item in TankPaths)
        {
            item.Value.Redraw();
        }
    }

    /*
     * tank selection methods
     */

    public void MarkSelectedTank(Node3D tank)
    {
        SelectedTank.Position = Convert.GetOverlayPosition(tank);
        SelectedTank.Visible = true;
    }

    public void UnmarkSelectedTank()
    {
        SelectedTank.Visible = false;
    }

    /*
     * tank path outline methods
     */

    public void StartNewTankPath(Tank tank, Node3D firstWaypoint)
    {
        var pathOutline = Repo.PathOutlineScene.Instantiate<PathOutline>();

        pathOutline.AddNewWaypoint(tank); /* start waypoint */
        pathOutline.AddNewWaypoint(firstWaypoint); /* first goto waypoint */

        if (TankPaths.ContainsKey(tank))
        {
            /* remove old path */
            RemoveTankPath(TankPaths[tank]);
        }

        TankPaths[tank] = pathOutline;
        AddChild(pathOutline);
    }

    public void FinishTankPath(Tank tank)
    {
        var pathOutline = TankPaths[tank];
        pathOutline.RemoveLastWaypoint();

        /*
         * if there is only one waypoint,
         * then user have aborted creating the path,
         * remove it
         */
        if (pathOutline.GetWaypointNodes().Count <= 1)
        {
            RemoveTankPath(pathOutline);
        }
    }

    public void AddNewWaypoint(Tank tank, Node3D waypoint)
    {
        TankPaths[tank].AddNewWaypoint(waypoint);
    }

    public void UpdateLastWaypoint(Tank tank)
    {
        TankPaths[tank].Redraw();
    }

    public bool IsLastWayoutValid(Tank tank)
    {
        return TankPaths[tank].IsLastWayoutValid();
    }
}
