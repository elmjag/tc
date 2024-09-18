using Godot;

public partial class Paths : Node2D
{
    public override void _Draw()
    {
        base._Draw();

        foreach (var tankPath in Repo.Level.GetTankPaths())
        {
            tankPath.DrawOverlays(this);
        }
    }
}
