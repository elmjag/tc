using Godot;

public partial class Tank : Node3D
{
    public static Tank GetByCollider(StaticBody3D collider)
    {
        return (Tank)collider.GetNode("../..");
    }

    public Rid getColliderRid()
    {
        var body = (StaticBody3D)FindChild("StaticBody3D");
        return body.GetRid();
    }
}
