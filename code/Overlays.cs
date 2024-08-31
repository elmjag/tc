using Godot;

public partial class Overlays : SubViewport
{
    Vector2 GroundSize;
    float XScale;
    float YScale;

    /* selected tank marker widget */
    Node2D SelectedTank;

    public override void _Ready()
    {
        var ground = Repo.Ground;

        /* draw overlays on ground mesh */
        var material = (StandardMaterial3D)ground.MaterialOverlay;
        material.AlbedoTexture = GetTexture();

        /* calculate scaling factors for position tranformations */
        GroundSize = ((PlaneMesh)ground.Mesh).Size;
        XScale = Size.X / GroundSize.X;
        YScale = Size.Y / GroundSize.Y;

        /* grab reference to selected tank widget */
        SelectedTank = (Node2D)FindChild("SelectedTank");
    }

    Vector2 GetLocalPosition(Vector3 pos)
    {
        pos -= Repo.Camera.GlobalPosition;
        float x = (pos.X + (GroundSize.X / 2.0f)) * XScale;
        float y = (pos.Z + (GroundSize.Y / 2.0f)) * YScale;

        return new Vector2(x, y);
    }

    /*
     * public API
     */

    public void MarkSelectedTank(Vector3 tankPosition)
    {
        SelectedTank.Position = GetLocalPosition(tankPosition);
        SelectedTank.Visible = true;
    }

    public void UnmarkSelectedTank()
    {
        SelectedTank.Visible = false;
    }
}
