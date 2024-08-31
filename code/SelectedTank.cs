using Godot;

public partial class SelectedTank : Node2D
{
    const float SELECTION_RADIOUS = 128;
    const float SELECTION_WIDTH = 8;
    Color SELECTION_COLOR = new Color(1, 0, 0);

    public override void _Draw()
    {
        base._Draw();
        DrawArc(
            Vector2.Zero,
            SELECTION_RADIOUS,
            0,
            Mathf.Tau,
            32,
            SELECTION_COLOR,
            SELECTION_WIDTH
        );
    }
}
