using Microsoft.Xna.Framework;

namespace FinalProjMonoGame;

public class Bomb : Enemy
{
    private bool exploded = false;
    public float SpinDegPerSec = 540f;

    public Bomb() : base("Bomb")
    {
        scale = new Vector2(0.15f, 0.15f);
        Damage = 2;
        
        originPosition = OriginPosition.Center;
    }

    public void Deflect(Vector2 newVelocity)
    {
        IgnorePlayerCollision = true;
        Velocity = newVelocity;
    }

    protected override void OnExitScreen()
    { 
        Destroy();
    }

    public void Explode(bool ignorePlayer = false)
    {
        if (exploded) return;
        exploded = true;

        Velocity = Vector2.Zero;
        Gravity = 0f;
        IgnorePlayerCollision = true;

        if (!ignorePlayer && player != null && player.BodyCollider != null && collider != null)
        {
            if (collider.Intersects(player.BodyCollider))
            {
                player.Damage(Damage);
                player.ResetDeflectStreak();
            }
        }

        var fx = new ExplosionFx("Explosion");
        fx.position = position;
        fx.scale = new Vector2(0.7f, 0.7f);
        SceneManager.Add(fx);
        fx.PlayOnceAndAutoRemove(12);

        Velocity = Vector2.Zero;
        if (collider != null) collider.rect = Rectangle.Empty;
        Destroy();
    }
    
    public override void Update(GameTime gameTime)
    {
        if (!exploded) 
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            rotation += SpinDegPerSec * dt;
            if (rotation >= 360f) rotation -= 360f;
        }

        base.Update(gameTime);
    }
}