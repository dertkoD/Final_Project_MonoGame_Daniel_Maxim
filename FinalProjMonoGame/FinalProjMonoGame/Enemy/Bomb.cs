using Microsoft.Xna.Framework;

namespace FinalProjMonoGame;

public class Bomb : Enemy
{
    // NEW: чтобы не взрывалась дважды
    private bool exploded = false;

    public Bomb() : base("Bomb")
    {
        scale = new Vector2(0.15f, 0.15f);
        Damage = 30;
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

    public void Explode()
    {
        if (exploded) return;    // защита от повторов
        exploded = true;

        // урон игроку
        if (player != null)
        {
            player.Damage(Damage);
            player.ResetDeflectStreak();
        }

        // FX взрыва
        var fx = new ExplosionFx("Explosion");
        fx.position = position;
        fx.scale = new Vector2(0.7f, 0.7f);
        fx.PlayOnceAndAutoRemove(1);
        SceneManager.Add(fx);

        // обезвредить бомбу
        Velocity = Vector2.Zero;
        if (collider != null) collider.rect = Rectangle.Empty;
        Destroy();
    }
}