using Microsoft.Xna.Framework;

namespace FinalProjMonoGame;

// Throwable enemy that spins during flight and explodes on demand.
// After exploding it spawns a one-shot FX and destroys itself.
public class Bomb : Enemy
{
    private bool exploded = false; // guards against double explosion
    public float SpinDegPerSec = 540f; // visual spin rate while in air

    public Bomb() : base("Bomb")
    {
        scale = new Vector2(0.15f, 0.15f);
        Damage = 2;
        
        originPosition = OriginPosition.Center;
    }

    // Sword deflect: give it a new velocity and ignore body hits for a short while (handled by caller).
    public void Deflect(Vector2 newVelocity)
    {
        IgnorePlayerCollision = true;
        Velocity = newVelocity;
    }

    // If it leaves the screen after being visible -> despawn.
    protected override void OnExitScreen()
    { 
        Destroy();
    }

    // Detonate now. When ignorePlayer=false and still overlapping the body, apply damage.
    public void Explode(bool ignorePlayer = false)
    {
        if (exploded) return;
        exploded = true;

        // Stop motion and disable further player-body hits.
        Velocity = Vector2.Zero;
        IgnorePlayerCollision = true;

        if (!ignorePlayer && player != null && player.BodyCollider != null && collider != null)
        {
            if (collider.Intersects(player.BodyCollider))
            {
                player.Damage(Damage);
                player.ResetDeflectStreak();
            }
        }

        // Spawn explosion FX (auto-removes itself when finished).
        var fx = new ExplosionFx("Explosion");
        fx.position = position;
        fx.scale = new Vector2(0.7f, 0.7f);
        SceneManager.Add(fx);
        fx.PlayOnceAndAutoRemove(12);

        // Blank the collider and remove this bomb from the scene.
        Velocity = Vector2.Zero;
        if (collider != null) collider.rect = Rectangle.Empty;
        Destroy();
    }
    
    public override void Update(GameTime gameTime)
    {
        // Visual spin while not exploded.
        if (!exploded) 
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            rotation += SpinDegPerSec * dt;
            if (rotation >= 360f) rotation -= 360f;
        }

        // Movement, collider sync, and offscreen checks are handled by Enemy.Update.
        base.Update(gameTime);
    }
}