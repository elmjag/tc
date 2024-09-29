using System;
using Godot;

class TankPaths
{
    const float GHOST_TANK_ROT_STEP = Mathf.Pi / 16.0f;

    MouseProjector MouseProjector;

    Tank SelectedTank = null;
    bool RotateTurret = false;
    NpcTank SelectedNpcTank = null;
    Tank GhostTank = null;

    public TankPaths(MouseProjector mproj)
    {
        MouseProjector = mproj;
    }

    void AddNewWayPoint()
    {
        var level = Repo.Level;
        GhostTank = level.AddMoveAction(SelectedTank);
    }

    void HandleNewTankSelected(Tank selected)
    {
        var overlays = Repo.Overlays;
        var level = Repo.Level;

        SelectedTank = selected;

        overlays.MarkSelectedTank(SelectedTank);
        GhostTank = level.SetupNewTurn(SelectedTank);
    }

    void FinishPathCreation()
    {
        if (SelectedTank == null)
        {
            return;
        }

        Repo.Overlays.UnmarkSelectedTank();
        Repo.Level.FinalizeTurnActions(SelectedTank);

        SelectedTank = null;
        GhostTank = null;
    }

    void HandleLeftClick(Vector2 position)
    {
        var level = Repo.Level;

        if (SelectedTank == null)
        {
            /* check if a tank was clicked */
            var new_selection = MouseProjector.GetTankAtPosition(position);
            if (new_selection != null)
            {
                HandleNewTankSelected(new_selection);
            }
        }
        else if (SelectedNpcTank != null)
        {
            AddNewWayPoint();
        }
        else if (level.LastTankActionValid(SelectedTank))
        {
            /* add new way point */
            AddNewWayPoint();
        }
    }

    void HandleRightClick()
    {
        FinishPathCreation();
    }

    void HandleScrollWheel(bool scrollUp)
    {
        if (GhostTank == null)
        {
            return;
        }

        /* rotate ghost tank */
        var angle = scrollUp ? GHOST_TANK_ROT_STEP : -GHOST_TANK_ROT_STEP;
        if (RotateTurret)
        {
            GhostTank.Turret.Rotate(Vector3.Up, angle);
        }
        else
        {
            GhostTank.Rotate(Vector3.Up, angle);
        }

        Repo.Level.UpdateLastMoveAction(SelectedTank, GhostTank);
    }

    void MaybeCreateFireAction(Tank shooter, Node3D npcTank)
    {
        var level = Repo.Level;
        var actions = level.GetTankTurnActions(SelectedTank);
        var lastAction = actions.GetLastAction();

        if (lastAction is FireAction)
        {
            /* already fire action, nothing to do */
            return;
        }

        actions.RemoveLastAction();
        actions.AddFireAction(shooter, npcTank);
        Repo.Overlays.Redraw();
    }

    void MaybeCreateMoveAction(Tank tank)
    {
        var level = Repo.Level;
        var actions = level.GetTankTurnActions(SelectedTank);
        var lastAction = actions.GetLastAction();

        if (lastAction is MoveAction)
        {
            /* already fire action, nothing to do */
            return;
        }

        actions.RemoveLastAction();
        actions.AddMoveAction(tank);
    }

    /*
     * public API
     */
    public bool IsActive()
    {
        return SelectedTank != null;
    }

    /*
    * incoming events hooks
    */

    public void HandleMouseClick(MouseButton button, Vector2 position)
    {
        switch (button)
        {
            case MouseButton.Left:
                HandleLeftClick(position);
                break;
            case MouseButton.Right:
                HandleRightClick();
                break;
            case MouseButton.WheelUp:
                HandleScrollWheel(true);
                break;
            case MouseButton.WheelDown:
                HandleScrollWheel(false);
                break;
        }
    }

    public void HandleMouseMotion(Vector2 position)
    {
        if (GhostTank == null)
        {
            return;
        }

        SelectedNpcTank = MouseProjector.GetNpcTankAtPosition(position);
        if (SelectedNpcTank != null)
        {
            GhostTank.Visible = false;

            var aimMark = Repo.AimMark;
            aimMark.Visible = true;
            aimMark.Position = Convert.GetOverlayPosition(SelectedNpcTank.Position);

            MaybeCreateFireAction(SelectedTank, SelectedNpcTank);
        }
        else
        {
            GhostTank.Visible = true;
            Repo.AimMark.Visible = false;

            MaybeCreateMoveAction(SelectedTank);

            GhostTank.Position = MouseProjector.GetGroundPosition(position);
            Repo.Level.UpdateLastMoveAction(SelectedTank, GhostTank);
        }
    }

    public void HandleKey(InputEventKey @event)
    {
        if (@event.Keycode == Key.Escape && @event.Pressed)
        {
            FinishPathCreation();
            return;
        }

        if (@event.Keycode == Key.Shift)
        {
            RotateTurret = @event.Pressed;
            return;
        }
    }
}

public partial class Input : Node
{
    const float MIN_ZOOM = 6.0f;
    const float MAX_ZOOM = 200.0f;
    const float ZOOM_STEP = 12.0f;
    const float GROUND_SCALER = 2.0f;

    MouseProjector mouseProjector;

    /* drag ground state */
    bool dragGround = false;
    Vector3 dragStart;

    TankPaths TankPaths;

    static void ResizeGroundPlane()
    {
        var plane = (PlaneMesh)Repo.Ground.Mesh;
        plane.Size = Vector2.One * Repo.Camera.GlobalPosition.Y * GROUND_SCALER;
    }

    /*
     * adjust the size of the ground mesh to reflect current zoom level
     *
     * i.e. make it bigger or smaller as we move camera up and down
     */
    public override void _Ready()
    {
        mouseProjector = new MouseProjector(Repo.Camera, Repo.Loader);

        TankPaths = new TankPaths(mouseProjector);

        ResizeGroundPlane();
    }

    void DoDragGround(Vector2 mouse_position)
    {
        var position = mouseProjector.GetGroundPosition(mouse_position);
        Repo.CameraRig.GlobalTranslate(dragStart - position);
        Repo.Overlays.Redraw();
    }

    void HandleMouseMotion(InputEventMouseMotion @event)
    {
        TankPaths.HandleMouseMotion(@event.Position);

        if (dragGround)
        {
            DoDragGround(@event.Position);
        }
    }

    void StartDraggingGround(Vector2 screen_position)
    {
        dragGround = true;
        dragStart = mouseProjector.GetGroundPosition(screen_position);
    }

    void StopDraggingGround()
    {
        dragGround = false;
    }

    void ChangeZoom(bool zoom_in, Vector2 mouse_position)
    {
        /* zoom in/out by changing camera's Y (height) position */
        var ydelta = zoom_in ? -ZOOM_STEP : ZOOM_STEP;
        var gpos = Repo.Camera.GlobalPosition;
        gpos.Y = Math.Clamp(Repo.Camera.GlobalPosition.Y + ydelta, MIN_ZOOM, MAX_ZOOM);
        Repo.Camera.GlobalPosition = gpos;

        ResizeGroundPlane();
        Repo.Overlays.Redraw();

        // TODO: code below does not work anymore, fix it!
        // /*
        //  * adjust X-Z position of the camera
        //  * so that we zoom in/out at the mouse pointer position
        //  */

        // /* project mouse pointer on the ground at the new height position */
        // var new_position = mouseProjector.GetGroundPosition(mouse_position);

        // /* move camera so that mouse is over same position as before changed height */
        // var position = mouseProjector.GetGroundPosition(mouse_position);
        // Repo.Camera.GlobalTranslate(position - new_position);
        // Repo.Ground.Position = new Vector3(0, 0, -Repo.Camera.Position.Y);
    }

    void HandleScrollWheel(bool scroll_up, Vector2 position)
    {
        if (TankPaths.IsActive())
        {
            return;
        }

        ChangeZoom(scroll_up, position);
    }

    void HandletMouseButton(InputEventMouseButton @event)
    {
        if (@event.Pressed)
        {
            TankPaths.HandleMouseClick(@event.ButtonIndex, @event.Position);

            switch (@event.ButtonIndex)
            {
                case MouseButton.Left:
                    StartDraggingGround(@event.Position);
                    break;
                case MouseButton.WheelUp:
                    HandleScrollWheel(true, @event.Position);
                    break;
                case MouseButton.WheelDown:
                    HandleScrollWheel(false, @event.Position);
                    break;
            }
        }
        else
        {
            switch (@event.ButtonIndex)
            {
                case MouseButton.Left:
                    StopDraggingGround();
                    break;
            }
        }
    }

    void HandleKey(InputEventKey @event)
    {
        TankPaths.HandleKey(@event);
        if (@event.Pressed)
        {
            if (@event.Keycode == Key.Space)
            {
                Repo.Level.StartTurnAnimation();
            }
        }
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseMotion me)
        {
            HandleMouseMotion(me);
        }
        else if (@event is InputEventMouseButton be)
        {
            HandletMouseButton(be);
        }
        else if (@event is InputEventKey ke)
        {
            HandleKey(ke);
        }
    }
}
