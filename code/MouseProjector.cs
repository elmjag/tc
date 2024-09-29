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
