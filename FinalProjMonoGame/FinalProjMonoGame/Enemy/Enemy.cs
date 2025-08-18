using System;
using FinalProjMonoGame.PlayerClasses;
using Microsoft.Xna.Framework;

namespace FinalProjMonoGame;

// Base class for all enemies: owns movement, gravity, lifetime, and player interactions.
// Derive and override OnUpdate / OnDestroy / OnExitScreen for specific behavior.
public abstract class Enemy : Sprite
{
    public Player player; // target reference (set by spawner)
    public Collider collider; // trigger AABB kept in sync with sprite rect

    public Vector2 Velocity { get; set; } = Vector2.Zero; // px/sec
    public float Gravity { get; set; } = 0f;  // px/sec^2 (applied to Y)
    public virtual int Damage { get; protected set; } = 0;
    public bool IsAlive { get; private set; } = true;
    
    // When true, skip Body vs enemy collision (used for brief immunity, deflects, etc).
    public bool IgnorePlayerCollision { get; set; } = false;

    // Screen lifecycle: only allow “despawn when offscreen” after first time we’re visible.
    protected bool hasEnteredViewport = false;
    protected float offscreenPadding = 48f; // hysteresis so we don't pop on edges

    protected Enemy(string spriteName) : base(spriteName)
    {
        originPosition = OriginPosition.Center;

        collider = SceneManager.Create<Collider>();
        collider.isTrigger = true; // enemies notify the player's colliders; no physics resolution here
    }

    public override void Update(GameTime gameTime)
    {
        if (!IsAlive) return;

        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

        // Integrate velocity + gravity
        var v = Velocity;
        if (Gravity != 0f)  v.Y += Gravity * dt;
        position += Velocity * dt;
        Velocity = v;

        // Update sprite (rect, sourceRect, etc.) and mirror collider to visual rect.
        base.Update(gameTime); 
        if (collider != null) collider.rect = rect;

        // Mark that we became visible at least once (enables “despawn after leaving screen”).
        if (IsOnScreen()) hasEnteredViewport = true;

        // Hook for subclasses (e.g., rotate to face target, animate, AI state).
        OnUpdate(gameTime);

        // Player interactions 
        if (player != null && collider != null)
        {
            // Sword/shield triggers: let the player's colliders decide what to do.
            if (player is Player pl)
            {
                if (pl.SwordCollider != null && collider.Intersects(pl.SwordCollider))
                    pl.SwordCollider.Notify(this);

                if (pl.ShieldCollider != null && collider.Intersects(pl.ShieldCollider))
                    pl.ShieldCollider.Notify(this);
            }

            // Body hit (unless temporarily ignored): deals damage to the player side.
            if (!IgnorePlayerCollision && player?.BodyCollider != null && collider.Intersects(player.BodyCollider))
            {
                player.BodyCollider.Notify(this);
                return; // stop further processing this frame (enemy may explode/destroy)
            }
        }

        // Despawn once we've been visible and then fully left the screen area.
        if (hasEnteredViewport && IsCompletelyOffscreen())
            OnExitScreen();
    }

    // Per-frame custom logic for derived enemies (movement tweaks, rotation, etc).
    protected virtual void OnUpdate(GameTime gameTime)
    {
    }

    // Called when leaving the screen area after being visible; default: destroy.
    protected virtual void OnExitScreen() => Destroy();

    // Rotate sprite to face its velocity
    protected void FaceVelocity(float deadZone = 0.0001f)
    {
        if (Velocity.LengthSquared() > deadZone)
            rotation = MathHelper.ToDegrees((float)System.Math.Atan2(Velocity.Y, Velocity.X));
    }

    // Safe one-shot destroy: calls subclass hook, removes collider and self from scene.
    public virtual void Destroy()
    {
        if (!IsAlive) return;
        IsAlive = false;

        OnDestroy();

        if (collider != null) SceneManager.Remove(collider);
        SceneManager.Remove(this);
    }

    // Optional cleanup hook for subclasses
    protected virtual void OnDestroy()
    {
    }

    // Visibility helper: “became visible” test with inward padding to avoid flicker.
    protected bool IsOnScreen()
    {
        var r = Game1.ScreenBounds;
        r.Inflate(-(int)offscreenPadding, -(int)offscreenPadding);
        return r.Intersects(rect);
    }

    // Despawn helper: fully outside an outward-padded screen rect.
    protected bool IsCompletelyOffscreen()
    {
        var r = Game1.ScreenBounds;
        r.Inflate((int)offscreenPadding, (int)offscreenPadding);
        return !r.Intersects(rect);
    }
}