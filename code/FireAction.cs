using System;
using Godot;

class FireAction : TurnAction
{
    /* in radians per second */
    const float TURRET_ROTATION_SPEED = 0.8f;

    /* the start posture for this action */
    Posture ShootingPosture;

    /* the end posture for this action */
    Posture FinalPosture;

    /* where to shoot */
    Vector3 TargetPosition;

    public FireAction(Posture shootingPosture, Node3D target)
    {
        FinalPosture = ShootingPosture = shootingPosture;
        TargetPosition = target.Position;

        var toTarget = TargetPosition - ShootingPosture.Position;
        FinalPosture.TurretRotation =
            Utils.GetVectorAngle(toTarget) - shootingPosture.BaseRotation + MathF.PI / 2;
    }

    float GetTurretRotationDistance()
    {
        return Utils.AngleDistance(ShootingPosture.TurretRotation, FinalPosture.TurretRotation);
    }

    /*
    * public API
    */

    /*
    * TurnAction implementation
    */
    public override float AnimationLength
    {
        get { return Math.Abs(GetTurretRotationDistance()) / TURRET_ROTATION_SPEED; }
    }

    public override bool IsValid()
    {
        throw new System.NotImplementedException();
    }

    public override Posture GetFinalPosture()
    {
        return FinalPosture;
    }

    public override void DrawOverlays(Node2D canavas)
    {
        var from = Convert.GetOverlayPosition(ShootingPosture.Position);
        var to = Convert.GetOverlayPosition(TargetPosition);

        canavas.DrawLine(from, to, Colors.Burlywood);
    }

    public override Posture GetAnimatedPosture(float animationTick)
    {
        var posture = ShootingPosture;

        posture.TurretRotation =
            ShootingPosture.TurretRotation
            + GetTurretRotationDistance() * (animationTick / AnimationLength);

        return posture;
    }
}
