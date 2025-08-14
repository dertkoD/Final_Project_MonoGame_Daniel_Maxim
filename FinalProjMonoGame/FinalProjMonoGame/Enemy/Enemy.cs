using System;
using Microsoft.Xna.Framework;

namespace FinalProjMonoGame;

public abstract class Enemy : Sprite
{
    public Player player;
    public Collider collider;

    public Vector2 Velocity { get; set; } = Vector2.Zero;
    public float Gravity { get; set; } = 0f; // 0 = без гравитации
    public int Damage { get; set; } = 10;
    public bool IsAlive { get; private set; } = true;

    // экранная логика
    protected bool hasEnteredViewport = false;
    protected float offscreenPadding = 48f;

    protected Enemy(string spriteName) : base(spriteName)
    {
        originPosition = OriginPosition.Center;

        collider = SceneManager.Create<Collider>();
        collider.isTrigger = true;
    }

    public override void Update(GameTime gameTime)
    {
        if (!IsAlive) return;

        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

        // простая физика
        var v = Velocity;
        if (Gravity != 0f)  v.Y += Gravity * dt;
        position += Velocity * dt;
        Velocity = v;

        base.Update(gameTime); // обновит rect/origin
        if (collider != null) collider.rect = rect;

        // отмечаем факт входа в экран
        if (IsOnScreen()) hasEnteredViewport = true;

        OnUpdate(gameTime); // хук для наследников

        // уведомление игрока при пересечении (дальше сам игрок решит, что делать)
        if (player != null && player.collider != null && collider != null)
        {
            if (collider.Intersects(player.collider))
                collider.Notify(this);
        }

        // после того как объект хоть раз был в экранe — удаляем при выходе
        if (hasEnteredViewport && IsCompletelyOffscreen())
            OnExitScreen();
    }

    protected virtual void OnUpdate(GameTime gameTime)
    {
    }

    protected virtual void OnExitScreen() => Destroy();

    // поворот по Velocity (для стрелы)
    protected void FaceVelocity(float deadZone = 0.0001f)
    {
        if (Velocity.LengthSquared() > deadZone)
            rotation = MathHelper.ToDegrees((float)System.Math.Atan2(Velocity.Y, Velocity.X));
    }

    public virtual void Destroy()
    {
        if (!IsAlive) return;
        IsAlive = false;

        OnDestroy();

        if (collider != null) SceneManager.Remove(collider);
        SceneManager.Remove(this);
    }

    protected virtual void OnDestroy()
    {
    }

    // --- экранные проверки ---
    protected bool IsOnScreen()
    {
        var r = Game1.ScreenBounds;
        r.Inflate(-(int)offscreenPadding, -(int)offscreenPadding);
        return r.Intersects(rect);
    }

    protected bool IsCompletelyOffscreen()
    {
        var r = Game1.ScreenBounds;
        r.Inflate((int)offscreenPadding, (int)offscreenPadding);
        return !r.Intersects(rect);
    }
}