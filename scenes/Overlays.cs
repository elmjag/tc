using Godot;

public partial class Overlays : SubViewport
{
    public override void _Ready()
    {
        /* draw overlays on ground mesh */
        var material = (StandardMaterial3D)Repo.Ground.MaterialOverlay;
        material.AlbedoTexture = GetTexture();
    }
}
