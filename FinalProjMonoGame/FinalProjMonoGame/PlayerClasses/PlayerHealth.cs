using System;

namespace FinalProjMonoGame.PlayerClasses;

public sealed class PlayerHealth
{
    public int MaxHP { get; private set; } = 5;
    public int HP { get; private set; }

    private float _hurtCooldown;
    private const float HurtInvuln = 0.30f;

    public bool IsDead => HP <= 0;
    public bool CanTakeDamage => _hurtCooldown <= 0f && !IsDead;

    public event Action<int>? OnDamaged;     
    public event Action<int>? OnHealed;      
    public event Action? OnDied;

    public void Init()
    {
        HP = MaxHP;
    }

    public void Update(float dt)
    {
        _hurtCooldown = MathF.Max(0f, _hurtCooldown - dt);
    }

    public void SetMaxHp(int max)
    {
        MaxHP = Math.Max(1, max);
        HP = Math.Min(HP, MaxHP);
    }

    public void Heal(int amount)
    {
        if (amount <= 0 || IsDead) return;
        HP = Math.Min(MaxHP, HP + amount);
        OnHealed?.Invoke(HP);
    }

    public void Damage(int amount)
    {
        if (amount <= 0 || IsDead) return;
        if (!CanTakeDamage) return;

        HP = Math.Max(0, HP - amount);
        _hurtCooldown = HurtInvuln;

        if (HP <= 0)
        {
            OnDied?.Invoke();
        }
        else
        {
            OnDamaged?.Invoke(HP);
        }
    }
}