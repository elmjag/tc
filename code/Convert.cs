using Godot;

public class Convert
{
    /*
     * convert world meters length to overlay pixels length
     */
    public static float MetersToPixels(float meters)
    {
        var pixels_per_meter = Repo.OverlaysPixels / Repo.GroundPlaneSize.X;
        return meters * pixels_per_meter;
    }

    static Vector2 ToOverlayPosition(Vector3 pos)
    {
        /* calculate scaling factors for position tranformations */
        var GroundSize = Repo.GroundPlaneSize;
        var OverlaysSize = Repo.Overlays.Size;
        var XScale = OverlaysSize.X / GroundSize.X;
        var YScale = OverlaysSize.Y / GroundSize.Y;

        pos -= Repo.Camera.GlobalPosition;
        float x = (pos.X + (GroundSize.X / 2.0f)) * XScale;
        float y = (pos.Z + (GroundSize.Y / 2.0f)) * YScale;

        return new Vector2(x, y);
    }

    ///
    /// calculate the node's position in overlay texture coordinates
    ///
    public static Vector2 GetOverlayPosition(Node3D node)
    {
        return ToOverlayPosition(node.Position);
    }

    ///
    /// convert a world position to overlay texture coordinates
    ///
    public static Vector2 GetOverlayPosition(Vector3 worldPosition)
    {
        return ToOverlayPosition(worldPosition);
    }
}
