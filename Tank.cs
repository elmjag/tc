using Godot;

public partial class Tank : Node3D
{
    public enum RenderStyle
    {
        Default,
        Selected,
    }

    Node3D SelectedDonut;

    public static Tank GetByCollider(StaticBody3D collider)
    {
        return (Tank)collider.GetNode("../..");
    }

    public void SetRenderStyle(RenderStyle style)
    {
        switch (style)
        {
            case RenderStyle.Default:
                SelectedDonut.Visible = false;
                break;
            case RenderStyle.Selected:
                SelectedDonut.Visible = true;
                break;
        }
    }

    public Rid getColliderRid()
    {
        var body = (StaticBody3D)FindChild("StaticBody3D");
        return body.GetRid();
    }

    public override void _Ready()
    {
        SelectedDonut = (Node3D)FindChild("selected");
    }
}
