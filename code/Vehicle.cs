using Godot;

public partial class Vehicle : Node3D
{
    /*
     *
     * public API
     *
     */

    public Rid GetColliderRid()
    {
        var body = (StaticBody3D)FindChild("StaticBody3D");
        return body.GetRid();
    }

    public static Vehicle GetByCollider(StaticBody3D collider)
    {
        return (Vehicle)collider.GetNode("../..");
    }
}
