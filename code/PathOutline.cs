using System.Collections.Generic;
using Godot;

public partial class PathOutline : Node2D
{
    Color PATH_COLOR = Colors.Yellow;

    const float TURN_RADIUS_METERS = 4;
    const float PATH_WIDTH_METERS = 1;

    List<Node3D> Waypoints = new List<Node3D>();

    static float TurnRadiusPixels
    {
        get { return Convert.MetersToPixels(TURN_RADIUS_METERS); }
    }

    static float PathWidthPixels
    {
        get { return Convert.MetersToPixels(PATH_WIDTH_METERS); }
    }

    (float, float) AngleDistances(float from, float to)
    {
        float clockwise;

        if (from < to)
        {
            clockwise = to - from;
        }
        else
        {
            clockwise = Mathf.Tau - from + to;
        }

        var counter_clockwise = Mathf.Tau - clockwise;

        return (clockwise, counter_clockwise);
    }

    (float, float) EnsureOrder(float start, float end, bool clockwise)
    {
        if (clockwise)
        {
            /* ensure clock-wise arch draw order */
            if (start < end)
            {
                return (start, end);
            }
            else
            {
                return (start, end + Mathf.Tau);
            }
        }
        else
        {
            /* ensure counter-clock-wise arch draw order */
            if (start > end)
            {
                return (start, end);
            }
            else
            {
                return (Mathf.Tau + start, end);
            }
        }
    }

    void DrawPathSegment(
        Vector2 center,
        Vector2 fromPosition,
        float fromRotation,
        Vector2 toPosition,
        float toRotation
    )
    {
        fromRotation += Mathf.Pi;

        var fromDir = Vector2.Up.Rotated(fromRotation);
        var toDir = Vector2.Up.Rotated(toRotation);

        var betweenAngle = fromDir.AngleTo(toDir) / 2;
        Vector2 betweenDir;

        if (betweenAngle < 0)
        {
            betweenAngle = toDir.AngleTo(fromDir) / 2;
            betweenDir = Vector2.Up.Rotated(toRotation + betweenAngle);
        }
        else
        {
            betweenDir = Vector2.Up.Rotated(fromRotation + betweenAngle);
        }

        var betaAngle = Mathf.Pi / 2 - betweenAngle;

        var centerDisplacement =
            Mathf.Sin(Mathf.Pi / 2) * TurnRadiusPixels / Mathf.Sin(betweenAngle);
        var circleTouchLength = Mathf.Sin(betaAngle) * TurnRadiusPixels / Mathf.Sin(betweenAngle);
        var arcCenter = center + (betweenDir * centerDisplacement);
        var toTurnPoint = center + (toDir * circleTouchLength);
        var fromTurnPoint = center + (fromDir * circleTouchLength);
        var arcFromPointAngle = Vector2.Right.AngleTo(fromTurnPoint - arcCenter);
        var arcToPointAngle = Vector2.Right.AngleTo(toTurnPoint - arcCenter);

        var (clockwise, counterClockwise) = AngleDistances(arcFromPointAngle, arcToPointAngle);

        var (startArc, endArc) = EnsureOrder(
            arcFromPointAngle,
            arcToPointAngle,
            clockwise < counterClockwise
        );

        DrawLine(toPosition, toTurnPoint, PATH_COLOR, PathWidthPixels);
        DrawArc(arcCenter, TurnRadiusPixels, startArc, endArc, 16, PATH_COLOR, PathWidthPixels);
        DrawLine(fromPosition, fromTurnPoint, PATH_COLOR, PathWidthPixels);
    }

    ///
    /// Calculate turning point, and from and to vectors of one path segment,
    /// all in overlay pixels coordinates.
    ///
    /// If not a valid path segment, the returned turning point variant is 'null'.
    ///
    (Variant, Vector2, float, Vector2, float) GetPathSegmentCoordinates(
        Node3D fromWaypoint,
        Node3D toWaypoint
    )
    {
        var fromPosition = Convert.GetOverlayPosition(fromWaypoint);
        var fromRotation = -fromWaypoint.Rotation.Y;
        var toPosition = Convert.GetOverlayPosition(toWaypoint);
        var toRotation = -toWaypoint.Rotation.Y;

        var fromDir = new Vector2(0, -1f).Rotated(fromRotation);
        var toDir = new Vector2(0, 1f).Rotated(toRotation);

        Variant intersection = Geometry2D.LineIntersectsLine(
            fromPosition,
            fromDir,
            toPosition,
            toDir
        );

        return (intersection, fromPosition, fromRotation, toPosition, toRotation);
    }

    public override void _Draw()
    {
        base._Draw();

        for (int i = 0; i < Waypoints.Count - 1; i += 1)
        {
            var fromWaypoint = Waypoints[i + 1];
            var toWaypoint = Waypoints[i];

            var (turnPoint, fromPosition, fromRotation, toPosition, toRotation) =
                GetPathSegmentCoordinates(fromWaypoint, toWaypoint);

            if (turnPoint.Obj != null)
            {
                DrawPathSegment(
                    turnPoint.AsVector2(),
                    fromPosition,
                    fromRotation,
                    toPosition,
                    toRotation
                );
            }
        }
    }

    /*
    * public API
    */

    public void Redraw()
    {
        QueueRedraw();
    }

    public void AddNewWaypoint(Node3D waypoint)
    {
        Waypoints.Insert(0, waypoint);
        QueueRedraw();
    }

    public void RemoveLastWaypoint()
    {
        Waypoints.RemoveAt(0);
        QueueRedraw();
    }

    public List<Node3D> GetWaypointNodes()
    {
        return Waypoints;
    }

    public bool IsLastWayoutValid()
    {
        var fromWaypoint = Waypoints[1];
        var toWaypoint = Waypoints[0];

        var (turnPoint, _, _, _, _) = GetPathSegmentCoordinates(fromWaypoint, toWaypoint);

        return turnPoint.Obj != null;
    }
}
