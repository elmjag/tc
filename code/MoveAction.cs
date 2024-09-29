using System.Linq;
using Godot;

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
    Color LINE_COLOR = Colors.Orange;

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
        rotation = SegmentUtil.GetAngle(direction) + Mathf.Pi / 2.0f;
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
        canavas.DrawLine(from, to, LINE_COLOR, OverlayPathWidth, true);
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

class MoveAction : TurnAction
{
    /* the movement speed, in meter/second */
    const float MOVEMENT_SPEED = 10f;

    Posture _From;
    Posture _To;

    PathSegment[] Segments = null;

    public MoveAction(Posture from, Posture to)
    {
        _From = from;
        _To = to;
        UpdatePathSegments();
    }

    void UpdatePathSegments()
    {
        var arcSegment = SegmentUtil.GetArcSegment(_From, _To);
        if (arcSegment != null)
        {
            var fromSegment = new LineSegment(_From.Position, arcSegment.GetStartPosition());
            var toSegment = new LineSegment(arcSegment.GetEndPosition(), _To.Position);

            Segments = new PathSegment[] { fromSegment, arcSegment, toSegment };
        }
        else
        {
            Segments = null;
        }
    }

    float PathLength()
    {
        return Segments.Aggregate(0f, (sum, segment) => sum + segment.Length);
    }

    /*
    * public API
    */
    public Posture To
    {
        get { return _To; }
    }

    public void UpdateToWaypoint(Posture to)
    {
        _To = to;
        UpdatePathSegments();
    }

    /*
    * TurnAction implementation
    */
    public override bool IsValid()
    {
        return Segments != null;
    }

    public override void DrawOverlays(Node2D canavas)
    {
        if (!IsValid())
        {
            /* not a valid move action currently, nothing to draw */
            return;
        }

        foreach (var segment in Segments)
        {
            segment.DrawOverlays(canavas);
        }
    }

    public override float AnimationLength
    {
        get { return PathLength() / MOVEMENT_SPEED; }
    }

    public override Posture GetAnimatedPosture(float animationTick)
    {
        var currentPosition = animationTick * MOVEMENT_SPEED;

        foreach (var segment in Segments)
        {
            if (currentPosition < segment.Length)
            {
                return segment.GetPosture(currentPosition);
            }
            currentPosition -= segment.Length;
        }

        var lastSegment = Segments.Last();
        return lastSegment.GetPosture(lastSegment.Length);
    }
}
