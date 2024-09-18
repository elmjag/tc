using Godot;

public partial class Overlays : SubViewport
{
    /* selected tank marker widget */
    Node2D SelectedTank;
    Node2D Paths;

    public override void _Ready()
    {
        var ground = Repo.Ground;

        /* draw overlays on ground mesh */
        var material = (StandardMaterial3D)ground.MaterialOverlay;
        material.AlbedoTexture = GetTexture();

        /* grab reference to selected tank widget */
        SelectedTank = (Node2D)FindChild("SelectedTank");
        Paths = (Node2D)FindChild("Paths");
    }

    /*
     *
     * public API
     *
     */


    public void Redraw()
    {
        /* redraw all tank path outlines */
        Paths.QueueRedraw();
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
}
