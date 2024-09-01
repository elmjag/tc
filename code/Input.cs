using System;
using Godot;
using Godot.Collections;

class MouseProjector
{
    const float RAY_LENGTH = 1000f;

    Camera3D camera;
    Loader loader;
    PhysicsDirectSpaceState3D spaceState;

    public MouseProjector(Camera3D camera, Loader loader)
    {
        this.camera = camera;
        this.loader = loader;
        spaceState = camera.GetWorld3D().DirectSpaceState;
    }

    private Dictionary ProjectMousePosition(Vector2 mouse_position, Array<Rid> exclude)
    {
        var from = camera.ProjectRayOrigin(mouse_position);
        var to = from + (camera.ProjectRayNormal(mouse_position) * RAY_LENGTH);
        var query = PhysicsRayQueryParameters3D.Create(from, to, exclude: exclude);
        return spaceState.IntersectRay(query);
    }

    public Vector3 GetGroundPosition(Vector2 mouse_position)
    {
        var result = ProjectMousePosition(mouse_position, loader.GetTankRids());
        var position = (Vector3)result["position"];

        return position;
    }

    public Tank GetTankAtPosition(Vector2 mouse_position)
    {
        var result = ProjectMousePosition(mouse_position, loader.GetGroundRid());
        Variant collider;
        if (!result.TryGetValue("collider", out collider))
        {
            /* no tank found at mouse position */
            return null;
        }

        return Tank.GetByCollider((StaticBody3D)collider);
    }
}

class TankPaths
{
    const float GHOST_TANK_ROT_STEP = Mathf.Pi / 16.0f;

    MouseProjector MouseProjector;

    /* selected tank state */
    Tank SelectedTank = null;
    Node3D GhostTank = null;

    public TankPaths(MouseProjector mproj)
    {
        this.MouseProjector = mproj;
    }

    void CreateGhostTank(Vector3 position, Vector3 rotation)
    {
        GhostTank = Repo.Loader.InstantiateGhostTank(position, rotation);
    }

    void AddNewWayPoint()
    {
        CreateGhostTank(GhostTank.Position, GhostTank.Rotation);
    }

    void HandleNewTankSelected(Tank selected)
    {
        SelectedTank = selected;

        Repo.Overlays.MarkSelectedTank(SelectedTank.GlobalPosition);
        CreateGhostTank(SelectedTank.Position, SelectedTank.Rotation);
    }

    void FinishPathCreation()
    {
        if (SelectedTank == null)
        {
            return;
        }

        SelectedTank = null;
        Repo.Loader.RemoveGhostTank(GhostTank);
        GhostTank = null;
        Repo.Overlays.UnmarkSelectedTank();
    }

    void HandleLeftClick(Vector2 position)
    {
        if (SelectedTank == null)
        {
            /* check if a tank was clicked */
            var new_selection = MouseProjector.GetTankAtPosition(position);
            if (new_selection != null)
            {
                HandleNewTankSelected(new_selection);
            }
        }
        else
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
        GhostTank.Rotate(Vector3.Up, angle);
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

        var ground_pos = MouseProjector.GetGroundPosition(position);
        GhostTank.Position = new Vector3(ground_pos.X, GhostTank.Position.Y, ground_pos.Z);
    }

    public void HandleKeyPressed(Key key)
    {
        if (key == Key.Escape)
        {
            FinishPathCreation();
        }
    }
}

public partial class Input : Node
{
    const float MIN_ZOOM = 6.0f;
    const float MAX_ZOOM = 200.0f;
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
        var ydelta = zoom_in ? -1.0f : 1.0f;
        var gpos = Repo.Camera.GlobalPosition;
        gpos.Y = Math.Clamp(Repo.Camera.GlobalPosition.Y + ydelta, MIN_ZOOM, MAX_ZOOM);
        Repo.Camera.GlobalPosition = gpos;

        ResizeGroundPlane();

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
        if (@event.Pressed)
        {
            TankPaths.HandleKeyPressed(@event.Keycode);
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
