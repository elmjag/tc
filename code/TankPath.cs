using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using Godot;
using Godot.NativeInterop;

public struct Posture
{
    public Vector3 Position;
    public float Rotation; /* rotation in the plane, radians */

    public Posture(Vector3 Position, float Rotation)
    {
        this.Position = Position;
        this.Rotation = Rotation;
    }

    public Posture(Node3D node)
    {
        Position = node.Position;
        Rotation = node.Rotation.Y;
    }

    public Vector3 ForwardDirection()
    {
        return Vector3.Forward.Rotated(Vector3.Up, Rotation);
    }

    public Vector3 BackDirection()
    {
        return Vector3.Back.Rotated(Vector3.Up, Rotation);
    }
}

abstract class PathSegment
{
    const float PATH_WIDTH_METERS = 1;

    public abstract float Length { get; }
    public abstract Posture GetPosture(float position);
    public abstract void DrawOverlays(Node2D canavas);

    protected float OverlayPathWidth
    {
        get { return Convert.MetersToPixels(PATH_WIDTH_METERS); }
    }
}

class LineSegment : PathSegment
{
    Vector3 start;
    Vector3 direction;
    float rotation;
    float length;

    public LineSegment(Vector3 from, Vector3 to)
    {
        start = from;
        var movement = to - start;
        length = movement.Length();
        direction = movement.Normalized();
        this.rotation = SegmentUtil.GetAngle(direction) + Mathf.Pi / 2.0f;
    }

    /*
     * PathSegment class implementation
     */

    public override float Length
    {
        get { return length; }
    }

    public override void DrawOverlays(Node2D canavas)
    {
        var from = Convert.GetOverlayPosition(start);
        var to = Convert.GetOverlayPosition(start + direction * length);
        canavas.DrawLine(from, to, Colors.Aqua, OverlayPathWidth, true);
    }

    public override Posture GetPosture(float position)
    {
        /* handle 'overshot' at the end of the path */
        if (position > Length)
        {
            position = Length;
        }

        var pos = start + direction * position;

        return new Posture(pos, rotation);
    }
}

class ArcSegment : PathSegment
{
    public const float TURN_RADIUS_METERS = 12;

    Vector3 center;
    float angleLength;
    float length;
    bool clockwise;
    Vector3 startDir;
    Vector3 endDir;

    float overlayStartAngle;
    float overlayEndAngle;

    public ArcSegment(Vector3 center, Vector3 startDir, Vector3 endDir, bool clockwise)
    {
        this.center = center;
        this.startDir = startDir;
        this.endDir = endDir;
        this.clockwise = clockwise;

        /* calculate arc length in meters */
        angleLength = startDir.AngleTo(endDir);
        length = TURN_RADIUS_METERS * angleLength;

        /* calculate angles for overlay graphics */
        (overlayStartAngle, overlayEndAngle) = OverAngles(
            SegmentUtil.GetAngle(startDir),
            SegmentUtil.GetAngle(endDir)
        );
    }

    (float, float) OverAngles(float startAngle, float endAngle)
    {
        var start = -startAngle;
        var end = -endAngle;

        if (clockwise && start > end)
        {
            end += Mathf.Tau;
        }
        else if (!clockwise && end > start)
        {
            start += Mathf.Tau;
        }

        return (start, end);
    }

    public Vector3 GetStartPosition()
    {
        return GetPosture(0).Position;
    }

    public Vector3 GetEndPosition()
    {
        return GetPosture(length).Position;
    }

    /*
     * PathSegment class implementation
     */

    public override float Length
    {
        get { return length; }
    }

    public override Posture GetPosture(float position)
    {
        var currentAngle = (position / length) * angleLength;

        if (clockwise)
        {
            currentAngle *= -1f;
        }

        var currentDir = startDir.Rotated(Vector3.Up, currentAngle);
        var currentPosition = center - currentDir * TURN_RADIUS_METERS;

        var postureRotation = SegmentUtil.GetAngle(currentDir);
        if (clockwise)
        {
            postureRotation += Mathf.Pi;
        }

        return new Posture(currentPosition, postureRotation);
    }

    public override void DrawOverlays(Node2D canavas)
    {
        var overlayRadious = Convert.MetersToPixels(TURN_RADIUS_METERS);

        canavas.DrawArc(
            Convert.GetOverlayPosition(center),
            overlayRadious,
            overlayStartAngle,
            overlayEndAngle,
            32,
            Colors.Crimson,
            OverlayPathWidth,
            true
        );

        canavas.DrawLine(
            Convert.GetOverlayPosition(center),
            Convert.GetOverlayPosition(center - startDir * 5),
            Colors.Green
        );

        canavas.DrawLine(
            Convert.GetOverlayPosition(center),
            Convert.GetOverlayPosition(center - endDir * 5),
            Colors.Red
        );
    }
}

class SegmentUtil
{
    public static Variant Intersection(Posture from, Posture to)
    {
        var fromPosition = new Vector2(from.Position.X, from.Position.Z);
        var toPosition = new Vector2(to.Position.X, to.Position.Z);

        var fromDir = new Vector2(0f, -1f).Rotated(-from.Rotation);
        var toDir = new Vector2(0f, 1f).Rotated(-to.Rotation);

        return Geometry2D.LineIntersectsLine(fromPosition, fromDir, toPosition, toDir);
    }

    public static float GetAngle(Vector3 direction)
    {
        var angle = Vector3.Left.AngleTo(direction);
        if (Vector3.Left.Cross(direction).Y < 0)
        {
            return Mathf.Tau - angle;
        }

        return angle;
    }

    public static ArcSegment GetArcSegment(Posture from, Posture to)
    {
        var intersectionVariant = Intersection(from, to);
        if (intersectionVariant.Obj == null)
        {
            return null;
        }

        var intersection2D = intersectionVariant.AsVector2();
        var intersection = new Vector3(intersection2D.X, 0, intersection2D.Y);

        var fromDirection = from.BackDirection();
        var toDirection = to.ForwardDirection();

        var angle = fromDirection.AngleTo(toDirection) / 2.0f;

        var dissplacement =
            Mathf.Sin(Mathf.Pi / 2) * ArcSegment.TURN_RADIUS_METERS / Mathf.Sin(angle);

        Vector3 startDir;
        Vector3 endDir;
        bool clockwiseArc = fromDirection.Cross(toDirection).Y > 0;
        if (clockwiseArc)
        {
            angle *= -1f;
            startDir = fromDirection.Rotated(Vector3.Up, Mathf.Pi * .5f);
            endDir = toDirection.Rotated(Vector3.Up, Mathf.Pi * -.5f);
        }
        else
        {
            startDir = fromDirection.Rotated(Vector3.Up, Mathf.Pi * -.5f);
            endDir = toDirection.Rotated(Vector3.Up, Mathf.Pi * .5f);
        }

        var arcCenter = intersection + toDirection.Rotated(Vector3.Up, angle) * dissplacement;

        return new ArcSegment(arcCenter, startDir, endDir, clockwiseArc);
    }
}

public class TankPath
{
    const float SPEED = 10f; /* meters per seconds */

    List<PathSegment> Segments = new List<PathSegment>();
    List<Node3D> Waypoints = new List<Node3D>();

    void AddPathSegments(Node3D from, Node3D to)
    {
        var fromPosture = new Posture(from);
        var toPosture = new Posture(to);

        var arcSegment = SegmentUtil.GetArcSegment(fromPosture, toPosture);
        if (arcSegment != null)
        {
            Segments.Add(new LineSegment(fromPosture.Position, arcSegment.GetStartPosition()));
            Segments.Add(arcSegment);
            Segments.Add(new LineSegment(arcSegment.GetEndPosition(), toPosture.Position));
        }
    }

    (bool, Posture) FindCurrentPosture(float pathPosition)
    {
        var curPos = pathPosition;
        foreach (var segment in Segments)
        {
            if (curPos < segment.Length)
            {
                return (false, segment.GetPosture(curPos));
            }
            curPos -= segment.Length;
        }

        var lastSegment = Segments.Last();
        return (true, lastSegment.GetPosture(lastSegment.Length));
    }

    /*
     * public API
     */

    public void DrawOverlays(Node2D canavas)
    {
        foreach (var segment in Segments)
        {
            segment.DrawOverlays(canavas);
        }
    }

    public void AddWaypoint(Node3D waypoint)
    {
        if (Waypoints.Count() > 0)
        {
            AddPathSegments(Waypoints[0], waypoint);
        }

        Waypoints.Insert(0, waypoint);
    }

    public bool IsLastWaypointValid()
    {
        /* this method is only defined after first waypoint have been added */
        Trace.Assert(Waypoints.Count > 0);

        return (Waypoints.Count - 1) * 3 == Segments.Count;
    }

    public void UpdateLastWaypoint()
    {
        /* this method is only defined after first waypoint have been added */
        Trace.Assert(Waypoints.Count > 0);

        if (IsLastWaypointValid())
        {
            /* remove 3 last path segments */
            Segments.RemoveRange(Segments.Count - 3, 3);
        }

        AddPathSegments(Waypoints[1], Waypoints[0]);
    }

    public List<Node3D> GetWaypoints()
    {
        return Waypoints;
    }

    public (bool, Posture) GetPosture(ulong AnimationTick)
    {
        var secsElapsed = AnimationTick / 1000f;
        var pathPosition = secsElapsed * SPEED;

        return FindCurrentPosture(pathPosition);
    }

    public Node3D RemoveLastWaypoint()
    {
        /* this method is only defined after first waypoint have been added */
        Trace.Assert(Waypoints.Count > 0);

        if (IsLastWaypointValid())
        {
            /* remove 3 last path segments */
            Segments.RemoveRange(Segments.Count - 3, 3);
        }

        var removedWaypoint = Waypoints[0];
        Waypoints.RemoveAt(0);

        return removedWaypoint;
    }
}
