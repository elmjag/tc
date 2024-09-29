using System;
using System.Linq;
using Godot;

abstract class PathSegment
{
    const float PATH_WIDTH_METERS = 1;

    public abstract float Length { get; }
    public abstract TankPosture GetPosture(float position);
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
        rotation = Utils.GetVectorAngle(direction) + Mathf.Pi / 2.0f;
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

    public override TankPosture GetPosture(float position)
    {
        /* handle 'overshot' at the end of the path */
        if (position > Length)
        {
            position = Length;
        }

        var pos = start + direction * position;

        /* note that turret roitation is handled by the MoveAction class */
        return new TankPosture(pos, rotation, 0);
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
            Utils.GetVectorAngle(startDir),
            Utils.GetVectorAngle(endDir)
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

    public override TankPosture GetPosture(float position)
    {
        var currentAngle = (position / length) * angleLength;

        if (clockwise)
        {
            currentAngle *= -1f;
        }

        var currentDir = startDir.Rotated(Vector3.Up, currentAngle);
        var currentPosition = center - currentDir * TURN_RADIUS_METERS;

        var postureRotation = Utils.GetVectorAngle(currentDir);
        if (clockwise)
        {
            postureRotation += Mathf.Pi;
        }

        /* note that turret roitation is handled by the MoveAction class */
        return new TankPosture(currentPosition, postureRotation, 0);
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

class Utils
{
    public static Variant Intersection(TankPosture from, TankPosture to)
    {
        var fromPosition = new Vector2(from.Position.X, from.Position.Z);
        var toPosition = new Vector2(to.Position.X, to.Position.Z);

        var fromDir = new Vector2(0f, -1f).Rotated(-from.BaseRotation);
        var toDir = new Vector2(0f, 1f).Rotated(-to.BaseRotation);

        return Geometry2D.LineIntersectsLine(fromPosition, fromDir, toPosition, toDir);
    }

    ///
    /// clamp angle beween -pi..pi
    ///
    public static float ClampAngle(float angle)
    {
        if (angle < -MathF.PI)
        {
            return angle + MathF.Tau;
        }
        if (angle > MathF.PI)
        {
            return angle - MathF.Tau;
        }

        return angle;
    }

    ///
    /// Get Vector's angle in the 'absolute' ground coordinate system.
    ///
    /// This assumes that the Vector in the Ground plane.
    ///
    public static float GetVectorAngle(Vector3 direction)
    {
        var angle = Vector3.Left.AngleTo(direction);
        if (Vector3.Left.Cross(direction).Y < 0)
        {
            return ClampAngle(Mathf.Tau - angle);
        }

        return ClampAngle(angle);
    }

    public static ArcSegment GetArcSegment(TankPosture from, TankPosture to)
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

    public static float AngleDistance(float from, float to)
    {
        return ClampAngle(to - from);
    }
}

class MoveAction : TurnAction
{
    /* the movement speed, in meter/second */
    const float MOVEMENT_SPEED = 10f;

    Tank Tank;
    TankPosture _From;
    TankPosture _To;

    PathSegment[] Segments = null;

    public MoveAction(Tank tank, TankPosture from, TankPosture to)
    {
        Tank = tank;
        _From = from;
        _To = to;
        UpdatePathSegments();
    }

    void UpdatePathSegments()
    {
        var arcSegment = Utils.GetArcSegment(_From, _To);
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

    TankPosture GetAnimatedPosture(float animationTick)
    {
        var currentPosition = animationTick * MOVEMENT_SPEED;

        foreach (var segment in Segments)
        {
            if (currentPosition < segment.Length)
            {
                return SetCurrnetTurretRotation(segment.GetPosture(currentPosition), animationTick);
            }
            currentPosition -= segment.Length;
        }

        return GetFinalPosture();
    }

    /*
    * public API
    */
    public TankPosture To
    {
        get { return _To; }
    }

    public void UpdateToWaypoint(TankPosture to)
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

    public override TankPosture GetFinalPosture()
    {
        return _To;
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

    TankPosture SetCurrnetTurretRotation(TankPosture posture, float animationTick)
    {
        var start = _From.TurretRotation;
        var distance = Utils.AngleDistance(start, _To.TurretRotation);

        posture.TurretRotation = start + distance * (animationTick / AnimationLength);

        return posture;
    }

    public override AnimationPosture[] GetTurnAnimationPostures(float animationTick)
    {
        var tankPosture = GetAnimatedPosture(animationTick);
        return new AnimationPosture[] { new AnimationPosture(Tank, tankPosture) };
    }
}
