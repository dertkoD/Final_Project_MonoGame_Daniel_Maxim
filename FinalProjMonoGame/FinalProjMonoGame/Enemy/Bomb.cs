using Microsoft.Xna.Framework;

namespace FinalProjMonoGame;

public class Bomb : Enemy
{
    // NEW: чтобы не взрывалась дважды
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

        // отключаем физику и коллизии с игроком
        Velocity = Vector2.Zero;
        Gravity = 0f;
        IgnorePlayerCollision = true;

        // ДОБАВЬ: урон игроку — только если не игнорируем
        if (!ignorePlayer && player != null && player.collider != null && collider != null)
        {
            if (collider.Intersects(player.collider))
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
        if (!exploded) // не крутим после взрыва
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            rotation += SpinDegPerSec * dt;
            if (rotation >= 360f) rotation -= 360f; // чтобы не росло до бесконечности
        }

        base.Update(gameTime); // физика/столкновения как раньше
    }
}