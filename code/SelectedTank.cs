using Godot;

public partial class SelectedTank : Node2D
{
    const float SELECTION_RADIOUS = 8;
    const float SELECTION_WIDTH = 1;
    Color SELECTION_COLOR = Colors.Aqua;

    public override void _Draw()
    {
        base._Draw();
        DrawArc(
            Vector2.Zero,
            Convert.MetersToPixels(SELECTION_RADIOUS),
            0,
            Mathf.Tau,
            32,
            SELECTION_COLOR,
            Convert.MetersToPixels(SELECTION_WIDTH)
        );
    }
}
