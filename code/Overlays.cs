using Godot;

public partial class Overlays : SubViewport
{
    /* selected tank marker widget */
    Node2D SelectedTank;
    Node2D TurnActionsCanvas;

    public override void _Ready()
    {
        var ground = Repo.Ground;

        /* draw overlays on ground mesh */
        var material = (StandardMaterial3D)ground.MaterialOverlay;
        material.AlbedoTexture = GetTexture();

        /* grab reference to selected tank widget */
        SelectedTank = (Node2D)FindChild("SelectedTank");
        TurnActionsCanvas = (Node2D)FindChild("TurnActionsCanvas");
    }

    /*
     *
     * public API
     *
     */


    public void Redraw()
    {
        /* redraw all tank path outlines */
        TurnActionsCanvas.QueueRedraw();
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
