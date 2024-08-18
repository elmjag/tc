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

public partial class Input : Node
{
    const float MIN_ZOOM = 6.0f;
    const float MAX_ZOOM = 200.0f;
    const float GHOST_TANK_ROT_STEP = Mathf.Pi / 16.0f;

    MouseProjector mouseProjector;

    /* drag ground state */
    bool dragGround = false;
    Vector3 dragStart;

    /* selected tank state */
    Tank selectedTank = null;
    Node3D ghostTank = null;

    public override void _Ready()
    {
        mouseProjector = new MouseProjector(Repo.Camera, Repo.Loader);
    }

    private void DoDragGround(Vector2 mouse_position)
    {
        var position = mouseProjector.GetGroundPosition(mouse_position);
        Repo.Camera.GlobalTranslate(dragStart - position);
    }

    private void HandleMouseMotion(InputEventMouseMotion @event)
    {
        if (dragGround)
        {
            DoDragGround(@event.Position);
        }

        if (selectedTank != null)
        {
            var ground_pos = mouseProjector.GetGroundPosition(@event.Position);
            ghostTank.Position = new Vector3(ground_pos.X, ghostTank.Position.Y, ground_pos.Z);
        }
    }

    private void StartDraggingGround(Vector2 screen_position)
    {
        dragGround = true;
        dragStart = mouseProjector.GetGroundPosition(screen_position);
    }

    private void StopDraggingGround()
    {
        dragGround = false;
    }

    private void ChangeZoom(bool zoom_in, Vector2 mouse_position)
    {
        var position = mouseProjector.GetGroundPosition(mouse_position);

        /* zoom in/out by changing camera's Y (height) position */
        var ydelta = zoom_in ? -1.0f : 1.0f;
        var gpos = Repo.Camera.GlobalPosition;
        gpos.Y = Math.Clamp(Repo.Camera.GlobalPosition.Y + ydelta, MIN_ZOOM, MAX_ZOOM);
        Repo.Camera.GlobalPosition = gpos;

        /*
         * adjust X-Z position of the camera
         * so that we zoom in/out at the mouse pointer position
         */

        /* project mouse pointer on the ground at the new height position */
        var new_position = mouseProjector.GetGroundPosition(mouse_position);

        /* move camera so that mouse is over same position as before changed height */
        Repo.Camera.GlobalTranslate(position - new_position);
        Repo.Ground.Position = new Vector3(0, 0, -Repo.Camera.Position.Y);
    }

    private void CheckTankSelection(Vector2 mouse_position)
    {
        var new_selection = mouseProjector.GetTankAtPosition(mouse_position);
        if (new_selection == selectedTank)
        {
            /* already selected tank click */
            return;
        }

        selectedTank?.SetRenderStyle(Tank.RenderStyle.Default);
        selectedTank = new_selection;

        if (selectedTank != null)
        {
            selectedTank.SetRenderStyle(Tank.RenderStyle.Selected);
            ghostTank = Repo.Loader.InstantiateGhostTank(
                selectedTank.Position,
                selectedTank.Rotation
            );
        }
        else
        {
            ghostTank = null;
        }
    }

    private void RotateGhostTank(bool rotate_clock_wise)
    {
        var angle = rotate_clock_wise ? GHOST_TANK_ROT_STEP : -GHOST_TANK_ROT_STEP;
        ghostTank.Rotate(Vector3.Up, angle);
    }

    private void HandleScrollWheel(bool scroll_up, Vector2 position)
    {
        if (ghostTank != null)
        {
            RotateGhostTank(!scroll_up);
        }
        else
        {
            ChangeZoom(scroll_up, position);
        }
    }

    private void HandletMouseButton(InputEventMouseButton @event)
    {
        if (@event.Pressed)
        {
            switch (@event.ButtonIndex)
            {
                case MouseButton.Left:
                    StartDraggingGround(@event.Position);
                    CheckTankSelection(@event.Position);
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
    }
}
