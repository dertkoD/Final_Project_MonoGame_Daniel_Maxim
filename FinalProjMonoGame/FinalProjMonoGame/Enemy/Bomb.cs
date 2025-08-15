using Microsoft.Xna.Framework;

namespace FinalProjMonoGame;

public class Bomb : Enemy
{
    public float BlastRadius { get; set; } = 120f;
    private bool wasDeflected = false;

    public Bomb() : base("Bomb")
    {
        scale = new Vector2(0.15f, 0.15f);
        Damage = 30;
    }

    // вызвать из игрока, когда он «отбил» бомбу
    public void Deflect(Vector2 newVelocity)
    {
        wasDeflected = true;
        Velocity = newVelocity;
    }

    protected override void OnExitScreen()
    {
        // после того как бомба прошла через экран:
        if (!wasDeflected) Explode(); // взрыв только если НЕ отбили
        Destroy();
    }

    private void Explode()
    {
        // урон AoE игроку прямоугольной аппроксимацией круга
        if (player != null && player.collider != null)
        {
            var aoe = new Rectangle(
                (int)(position.X - BlastRadius),
                (int)(position.Y - BlastRadius),
                (int)(BlastRadius * 2),
                (int)(BlastRadius * 2)
            );
            if (aoe.Intersects(player.collider.rect))
                player.OnTrigger(this);
        }

        // FX взрыва (если есть анимация)
        var fx = new ExplosionFx("Explosion");
        fx.position = position;
        fx.scale = new Vector2(0.18f, 0.18f);
        SceneManager.Add(fx);
        fx.PlayOnceAndAutoRemove(12);
    }
}