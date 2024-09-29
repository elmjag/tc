using Godot;

public partial class AimMark : Node2D
{
    const float OUTER_RADIOUS = 8;
    const float INNER_RADIOUS = 5;
    const float LINE_WIDTH = 1;
    Color COLOR = Colors.Red;

    public override void _Draw()
    {
        var outerRadiusPixels = Convert.MetersToPixels(OUTER_RADIOUS);
        var lineWidthPixels = Convert.MetersToPixels(LINE_WIDTH);

        base._Draw();
        DrawArc(Vector2.Zero, outerRadiusPixels, 0, Mathf.Tau, 32, COLOR, lineWidthPixels);

        var dissplacement = Convert.MetersToPixels(INNER_RADIOUS);

        DrawLine(
            new Vector2(0, -dissplacement),
            new Vector2(0, -outerRadiusPixels),
            COLOR,
            lineWidthPixels
        );

        DrawLine(
            new Vector2(0, dissplacement),
            new Vector2(0, outerRadiusPixels),
            COLOR,
            lineWidthPixels
        );

        DrawLine(
            new Vector2(-dissplacement, 0),
            new Vector2(-outerRadiusPixels, 0),
            COLOR,
            lineWidthPixels
        );

        DrawLine(
            new Vector2(dissplacement, 0),
            new Vector2(outerRadiusPixels, 0),
            COLOR,
            lineWidthPixels
        );
    }
}
