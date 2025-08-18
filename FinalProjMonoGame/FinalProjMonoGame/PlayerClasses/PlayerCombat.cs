using System;
using Microsoft.Xna.Framework;

namespace FinalProjMonoGame.PlayerClasses;

// Handles combat interactions: attack cooldown, deflects, and damage application.
// Called from collider triggers owned by PlayerColliders.
public class PlayerCombat
{
    private readonly Player _p;
    private readonly PlayerHealth _health;
    private readonly PlayerAnimationController _anim;

    // Minimum time between sword attacks (prevents spam).
    private const float AttackMinCooldown = 0.85f;
    private float _attackCooldownTimer = 0f;

    // Deflect streak reward (heal after N successful deflects).
    private int _deflectCount = 0;
    public int DeflectHealThreshold { get; private set; } = 5;

    // Notifies UI/FX when heal-on-deflect happens.
    public event Action<int>? OnDeflectHeal; // passes HP after heal

    public PlayerCombat(Player p, PlayerHealth health, PlayerAnimationController anim)
    {
        _p = p;
        _health = health;
        _anim = anim;
    }

    // Frame-rate independent cooldown ticking.
    public void Update(float dt)
    {
        _attackCooldownTimer = MathF.Max(0f, _attackCooldownTimer - dt);
    }

    public bool CanAttack() => _attackCooldownTimer <= 0f;
    public void OnAttackStarted() => _attackCooldownTimer = AttackMinCooldown;

    // Allows tuning the heal cadence at runtime (also resets current streak).
    public void SetDeflectHealThreshold(int threshold)
    {
        DeflectHealThreshold = threshold;
        ResetDeflectStreak();
    }

    public void ResetDeflectStreak() => _deflectCount = 0;

    // Unit facing as a 2D normal (right or left).
    private Vector2 FacingNormal() => _p.FacingRight ? new Vector2(1f, 0f) : new Vector2(-1f, 0f);

    // Sword trigger: only active during Attack state 
    public void OnSwordTrigger(object o)
    {
        if (_p.State != PlayerState.Attack) return;

        if (o is Bomb bomb)
        {
            if (bomb.IgnorePlayerCollision) return;

            // Mirror bomb velocity; enforce a reasonable minimum and nudge out of the sword.
            Vector2 incoming = bomb.Velocity;
            if (incoming.LengthSquared() < 1f) incoming = FacingNormal() * 300f;

            Vector2 newVel = -incoming;
            float speed = Math.Max(incoming.Length(), 300f);
            newVel = Vector2.Normalize(newVel) * speed;

            bomb.Deflect(newVel);
            bomb.position += Vector2.Normalize(newVel) * 8f;
            RegisterDeflect();
            return;
        }

        if (o is Arrow arrow)
        {
            if (arrow.IsSpinning) return;

            // Bounce arrow and start a spinning fall (more readable than perfect reflect).
            Vector2 n = FacingNormal();
            Vector2 incoming = arrow.Velocity;
            if (incoming.LengthSquared() < 1f) incoming = n * 450f;

            Vector2 reflected = Vector2.Reflect(incoming, n);
            float keep = 0.45f;
            float upKick = 420f;
            Vector2 bounce = reflected * keep + new Vector2(0f, -upKick);
            if (Math.Abs(bounce.X) < 120f) bounce.X = 120f * (n.X >= 0 ? 1f : -1f);

            float gravity = 1400f;
            float spinSpeedDegPerSec = 1080f;
            arrow.StartSpin(bounce, spinSpeedDegPerSec, gravity);
            RegisterDeflect();
        }
    }

    // Shield trigger: only active during Defend state
    public void OnShieldTrigger(object o)
    {
        if (_p.State != PlayerState.Defend) return;

        if (o is Bomb bomb)
        {
            // Shield vs bomb: detonate immediately and apply a 1-damage penalty.
            bomb.Explode(ignorePlayer: true);
            _health.Damage(1); // penalty
            ResetDeflectStreak();
            AudioManager.PlaySoundEffect("Explosion", false, 1f);
            return;
        }

        if (o is Arrow arrow)
        {
            if (arrow.IsSpinning) return;

            // Same stylish bounce+spin as sword, plus block SFX and a short shield FX.
            Vector2 n = FacingNormal();
            Vector2 incoming = arrow.Velocity;
            if (incoming.LengthSquared() < 1f) incoming = -n * 450f;

            Vector2 reflected = Vector2.Reflect(incoming, n);
            float keep = 0.45f;
            float upKick = 420f;
            Vector2 bounce = reflected * keep + new Vector2(0f, -upKick);
            if (Math.Abs(bounce.X) < 120f) bounce.X = 120f * (n.X >= 0 ? 1f : -1f);

            float gravity = 1400f;
            float spinSpeedDegPerSec = 1080f;
            arrow.StartSpin(bounce, spinSpeedDegPerSec, gravity);

            RegisterDeflect();
            AudioManager.PlaySoundEffect("PlayerDefend", false, 1f);
            _anim.TriggerShieldBlockFx();
        }
    }

    // Body trigger: apply damage to the player
    public void OnBodyTrigger(object o)
    {
        if (!_health.CanTakeDamage) return;

        switch (o)
        {
            case Bomb bomb:
                bomb.Explode();
                AudioManager.PlaySoundEffect("Explosion", false, 1f);
                ResetDeflectStreak();
                break;
            case Arrow arrow:
                _health.Damage(arrow.Damage);
                ResetDeflectStreak();
                arrow.Destroy();
                break;
        }
    }

    // Increments deflect streak; heal and notify when threshold reached.
    private void RegisterDeflect()
    {
        _deflectCount++;
        if (_deflectCount >= DeflectHealThreshold)
        {
            _deflectCount = 0;
            _health.Heal(1);
            OnDeflectHeal?.Invoke(_health.HP);
        }
    }
}