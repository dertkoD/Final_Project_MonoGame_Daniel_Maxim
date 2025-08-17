using System;
using Microsoft.Xna.Framework;

namespace FinalProjMonoGame;

public abstract class Enemy : Sprite
{
    public Player player;
    public Collider collider;

    public Vector2 Velocity { get; set; } = Vector2.Zero;
    public float Gravity { get; set; } = 0f;
    public virtual int Damage { get; protected set; } = 0;
    public bool IsAlive { get; private set; } = true;
    public bool IgnorePlayerCollision { get; set; } = false;


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

        var v = Velocity;
        if (Gravity != 0f)  v.Y += Gravity * dt;
        position += Velocity * dt;
        Velocity = v;

        base.Update(gameTime); 
        if (collider != null) collider.rect = rect;

        if (IsOnScreen()) hasEnteredViewport = true;

        OnUpdate(gameTime);

        if (player != null && collider != null)
        {
            if (player is Player pl)
            {
                if (pl.SwordCollider != null && collider.Intersects(pl.SwordCollider))
                    pl.SwordCollider.Notify(this);

                if (pl.ShieldCollider != null && collider.Intersects(pl.ShieldCollider))
                    pl.ShieldCollider.Notify(this);
            }

            if (!IgnorePlayerCollision && player?.BodyCollider != null && collider.Intersects(player.BodyCollider))
            {
                player.BodyCollider.Notify(this);
                return;
            }
        }

        if (hasEnteredViewport && IsCompletelyOffscreen())
            OnExitScreen();
    }

    protected virtual void OnUpdate(GameTime gameTime)
    {
    }

    protected virtual void OnExitScreen() => Destroy();

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