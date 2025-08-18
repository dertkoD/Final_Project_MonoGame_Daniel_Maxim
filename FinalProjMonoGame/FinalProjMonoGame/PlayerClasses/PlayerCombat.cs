using System;
using Microsoft.Xna.Framework;

namespace FinalProjMonoGame.PlayerClasses;

public class PlayerCombat
{
    private readonly Player _p;
    private readonly PlayerHealth _health;
    private readonly PlayerAnimationController _anim;

    private const float AttackMinCooldown = 0.85f;
    private float _attackCooldownTimer = 0f;

    private int _deflectCount = 0;
    public int DeflectHealThreshold { get; private set; } = 5;

    public event Action<int>? OnDeflectHeal; // passes HP after heal

    public PlayerCombat(Player p, PlayerHealth health, PlayerAnimationController anim)
    {
        _p = p;
        _health = health;
        _anim = anim;
    }

    public void Update(float dt)
    {
        _attackCooldownTimer = MathF.Max(0f, _attackCooldownTimer - dt);
    }

    public bool CanAttack() => _attackCooldownTimer <= 0f;
    public void OnAttackStarted() => _attackCooldownTimer = AttackMinCooldown;

    public void SetDeflectHealThreshold(int threshold)
    {
        DeflectHealThreshold = threshold;
        ResetDeflectStreak();
    }

    public void ResetDeflectStreak() => _deflectCount = 0;

    private Vector2 FacingNormal() => _p.FacingRight ? new Vector2(1f, 0f) : new Vector2(-1f, 0f);

    // === Triggers ===
    public void OnSwordTrigger(object o)
    {
        if (_p.State != PlayerState.Attack) return;

        if (o is Bomb bomb)
        {
            if (bomb.IgnorePlayerCollision) return;

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

    public void OnShieldTrigger(object o)
    {
        if (_p.State != PlayerState.Defend) return;

        if (o is Bomb bomb)
        {
            bomb.Explode(ignorePlayer: true);
            _health.Damage(1); // penalty
            ResetDeflectStreak();
            AudioManager.PlaySoundEffect("Explosion", false, 1f);
            return;
        }

        if (o is Arrow arrow)
        {
            if (arrow.IsSpinning) return;

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