using Godot;

///
/// the overlays canvas used for visualization of turn actions
///
public partial class TurnActionsCanvas : Node2D
{
    public override void _Draw()
    {
        base._Draw();

        foreach (var action in Repo.Level.GetAllTankTurnActions())
        {
            action.DrawOverlays(this);
        }
    }
}
