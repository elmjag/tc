using System;
using Godot;

class FireAction : TurnAction
{
    const float QUATER_TURN = MathF.PI / 2;

    /* in radians per second */
    const float TURRET_ROTATION_SPEED = 0.8f;

    /* the time we run firing animation, in seconds */
    const float FIRING_ANIMATION_TIME = 0.1f;

    /* the shooter */
    Tank Tank;

    /* the start posture for this action */
    TankPosture ShootingPosture;

    /* the end posture for this action */
    TankPosture FinalPosture;

    /* where to shoot */
    Vector3 TargetPosition;

    BarrelSmoke BarrelSmoke;

    public FireAction(Tank tank, TankPosture shootingPosture, Node3D target)
    {
        Tank = tank;
        ShootingPosture = new TankPosture(shootingPosture);
        FinalPosture = new TankPosture(shootingPosture);
        TargetPosition = target.Position;

        var toTarget = TargetPosition - ShootingPosture.Position;
        FinalPosture.TurretRotation =
            Utils.GetVectorAngle(toTarget) - shootingPosture.BaseRotation + QUATER_TURN;
    }

    float GetTurretRotationDistance()
    {
        return Utils.AngleDistance(ShootingPosture.TurretRotation, FinalPosture.TurretRotation);
    }

    /// lazy instantiation of barrel smoke node
    BarrelSmoke GetBarrelSmoke()
    {
        if (BarrelSmoke is null)
        {
            BarrelSmoke = Repo.Loader.InstantiateBarrelSmoke();
        }

        return BarrelSmoke;
    }

    float GetTurrentTurningTime()
    {
        return Math.Abs(GetTurretRotationDistance()) / TURRET_ROTATION_SPEED;
    }

    /*
    * public API
    */

    /*
    * TurnAction implementation
    */
    public override float AnimationLength
    {
        get { return GetTurrentTurningTime() + FIRING_ANIMATION_TIME; }
    }

    public override bool IsValid()
    {
        throw new System.NotImplementedException();
    }

    public override TankPosture GetFinalPosture()
    {
        return FinalPosture;
    }

    public override void DrawOverlays(Node2D canavas)
    {
        var from = Convert.GetOverlayPosition(ShootingPosture.Position);
        var to = Convert.GetOverlayPosition(TargetPosition);

        canavas.DrawLine(from, to, Colors.Burlywood);
    }

    public override AnimationPosture[] GetTurnAnimationPostures(float animationTick)
    {
        var tankPosture = new TankPosture(ShootingPosture);
        var turrentTurningTime = GetTurrentTurningTime();
        var turnProgression = animationTick / turrentTurningTime;

        if (turnProgression <= 1.0f)
        {
            /* we are turning the torrent in the target's direction */
            tankPosture.TurretRotation =
                ShootingPosture.TurretRotation + GetTurretRotationDistance() * turnProgression;

            return new AnimationPosture[] { new AnimationPosture(Tank, tankPosture) };
        }

        /* we are playing the 'fire the cannon' animation */
        tankPosture.TurretRotation = FinalPosture.TurretRotation;
        var barrelEnd = Tank.GetBarrelEndPosition();
        var smokeProgression = (animationTick - turrentTurningTime) / FIRING_ANIMATION_TIME;

        var smokeAngle = FinalPosture.BaseRotation + FinalPosture.TurretRotation - QUATER_TURN;

        return new AnimationPosture[]
        {
            new AnimationPosture(Tank, tankPosture),
            new AnimationPosture(
                GetBarrelSmoke(),
                new BarrelSmokePosture(barrelEnd, smokeAngle, smokeProgression)
            ),
        };
    }

    public override void AnimationFinished()
    {
        BarrelSmoke?.QueueFree();
        BarrelSmoke = null;
    }
}
