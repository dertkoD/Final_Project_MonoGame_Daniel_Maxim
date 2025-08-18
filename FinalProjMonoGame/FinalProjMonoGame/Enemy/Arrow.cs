using Microsoft.Xna.Framework;

namespace FinalProjMonoGame;

// Simple projectile enemy. Flies nose-first toward target;
// after a deflect, switches to a spinning fall and stops hurting the player's body.
public class Arrow : Enemy
{
    // Spinning mode (after deflect); rotation driven by _angularSpeedDeg in Update().
    public bool IsSpinning { get; private set; } = false;
    private float _angularSpeedDeg = 0f;

    public Arrow() : base("Arrow")
    {
        scale = new Vector2(0.4f, 0.4f);
        Damage = 1;
    }

    // Enter "spin & fall" mode:
    // - sets new velocity & gravity supplied by caller (combat logic)
    // - enables visual spin
    // - ignores player body collision to prevent cheap hits after deflect
    public void StartSpin(Vector2 initialVelocity, float angularSpeedDeg, float gravity)
    {
        Velocity = initialVelocity;
        Gravity = gravity;
        _angularSpeedDeg = angularSpeedDeg;
        IsSpinning = true;

        IgnorePlayerCollision = true; // no body damage while falling/spinning
    }

    // While spinning: advance rotation every frame.
    // Otherwise: face current velocity vector (readable flight direction).
    protected override void OnUpdate(GameTime gameTime)
    {
        if (IsSpinning)
        {
            rotation += _angularSpeedDeg * (float)gameTime.ElapsedGameTime.TotalSeconds;
        }
        else
        {
            FaceVelocity(); // align sprite with travel direction
        }
    }
}