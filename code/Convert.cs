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
}
