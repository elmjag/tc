using System;
using Godot;


public partial class WorldCamera : Camera3D
{
    const float MIN_ZOOM = 6.0f;
    const float MAX_ZOOM = 200.0f;

    private bool dragGround = false;
    private Vector3 dragStart;

    public override void _Notification(int what)
    {
        if (what == NotificationWMMouseExit)
        {
            /*
             * mouse pointer outside of the window,
             * stop dragging ground
             */
            this.dragGround = false;
        }
    }

    private void HandleLeftButton(bool pressed, Vector3 position)
    {
        this.dragGround = pressed;
        if (this.dragGround)
        {
            this.dragStart = position;
        }
    }

    private void ChangeZoom(bool zoom_in, Vector2 screen_position, Vector3 ground_position)
    {
        /* zoom in/out by changing camera's Y (height) position */
        var ydelta = zoom_in ? -1.0f : 1.0f;
        var gpos = this.GlobalPosition;
        gpos.Y = Math.Clamp(this.GlobalPosition.Y + ydelta, MIN_ZOOM, MAX_ZOOM);
        this.GlobalPosition = gpos;

        /*
         * adjust X-Z position of the camera
         * so that we zoom in/out at the mouse pointer position
         */

        /* project mouse pointer on the ground at the new height position */
        var from = this.ProjectRayOrigin(screen_position);
        var to = from + (this.ProjectRayNormal(screen_position) * 1000);
        var spaceState = this.GetWorld3D().DirectSpaceState;
        var query = PhysicsRayQueryParameters3D.Create(from, to);
        var result = spaceState.IntersectRay(query);
        var new_position = (Vector3)result["position"];

        /* move camera so that mouse is over same position as before changed height */
        this.GlobalTranslate(ground_position - new_position);
    }

    public void GroundInputEvent(Camera3D camera, InputEvent @event, Vector3 position, Vector3 normal, int shape_idx)
    {
        if (@event is InputEventMouseButton mouse_button_event)
        {
            switch (mouse_button_event.ButtonIndex)
            {
                case MouseButton.Left:
                    this.HandleLeftButton(mouse_button_event.Pressed, position);
                    break;
                case MouseButton.WheelUp:
                    this.ChangeZoom(true, mouse_button_event.Position, position);
                    break;
                case MouseButton.WheelDown:
                    this.ChangeZoom(false, mouse_button_event.Position, position);
                    break;
            }
            return;
        }

        if (this.dragGround && (@event is InputEventMouseMotion motion))
        {
            this.GlobalTranslate(this.dragStart - position);
        }

    }
}
