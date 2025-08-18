using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FinalProjMonoGame.PlayerClasses;

/// Thin orchestrator for the player: owns the state machine and delegates to subsystems.
/// Rendering comes from Animation base; combat/health/colliders live in dedicated classes.
public class Player : Animation
{
    // Subsystems
    public PlayerController Controller { get; private set; }
    public PlayerAnimationController Anim { get; private set; }
    public PlayerHealth Health { get; private set; }
    public PlayerCombat Combat { get; private set; }
    public PlayerColliders Colliders { get; private set; }

    // Keep public exposure for backward-compat
    public Collider SwordCollider => Colliders.Sword;
    public Collider ShieldCollider => Colliders.Shield;
    public Collider BodyCollider => Colliders.Body;
    
    // Backward-compat for UI, etc.
    public int MaxHP => Health.MaxHP;
    public int HP    => Health.HP;

    public bool FacingRight { get; private set; } = true;
    public PlayerState State { get; private set; } = PlayerState.Idle;

    // Global input toggle (e.g., during death/menus).
    public bool ControlIsEnabled => Controller.ControlsEnabled;
    public void SetControlsEnabled(bool enabled) => Controller.ControlsEnabled = enabled;

    // Bubble up heal-on-deflect for HUD/SFX hooks.
    public event Action<int>? OnDeflectHeal
    {
        add { Combat.OnDeflectHeal += value; }
        remove { Combat.OnDeflectHeal -= value; }
    }

    public Player() : base(PlayerAnimationController.IdleAnim)
    {
        // base sprite setup
        position = Game1.ScreenCenter;
        scale = new Vector2(1.5f, 1.5f);
        originPosition = OriginPosition.Center;
        effect = SpriteEffects.None;

        // compose
        Controller = new PlayerController();
        Anim = new PlayerAnimationController(this);
        Health = new PlayerHealth();
        Combat = new PlayerCombat(this, Health, Anim);
        Colliders = new PlayerColliders();

        // init subsystems
        Health.Init();
        Colliders.Init();
        Anim.Init();

        // wire collider triggers
        Colliders.Sword.OnTrigger += Combat.OnSwordTrigger;
        Colliders.Shield.OnTrigger += Combat.OnShieldTrigger;
        Colliders.Body.OnTrigger += Combat.OnBodyTrigger;

        // wire health -> orchestrator reactions
        Health.OnDamaged += hp =>
        {
            if (State != PlayerState.Defend)
            {
                State = PlayerState.Hurt;
                Anim.PlayHurtOnce();
                AudioManager.PlaySoundEffect("PlayerHurt", isLoop: false, volume: 1f);
            }
            else
            {
                // still in defend; small visual kick
                Anim.TriggerShieldBlockFx();
            }
        };
        Health.OnDied += () =>
        {
            State = PlayerState.Dead;
            Controller.ControlsEnabled = false;
            Anim.PlayDeathOnce();
            AudioManager.PlaySoundEffect("PlayerDeath", isLoop: false, volume: 1f);
            Game1.Instance.TriggerGameOver(this, 2.0);
        };
    }

    public override void Update(GameTime gameTime)
    {
        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

        // health/cooldowns/anim fx
        Health.Update(dt);
        Combat.Update(dt);
        Anim.Update(dt);

        // read controls and update facing/state desires
        var ctrlOut = Controller.Update(State);
        FacingRight = Controller.FacingRight;
        effect = FacingRight ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

        // state machine
        switch (State)
        {
            case PlayerState.Dead:
                break; // wait for death anim to finish (no transition here)

            case PlayerState.Hurt:
                break; // ignore input while in stagger

            case PlayerState.Defend:
                if (!ctrlOut.DefendHeld)
                {
                    ToIdle();
                }

                break;

            case PlayerState.Attack:
                // wait for non-looping attack clip to end
                break;

            case PlayerState.Idle:
                if (ctrlOut.DefendHeld)
                {
                    State = PlayerState.Defend;
                    Anim.PlayDefendLoop();
                }
                else if (ctrlOut.AttackPressed && Combat.CanAttack())
                {
                    State = PlayerState.Attack;
                    Anim.PlayAttackOnce();
                    AudioManager.PlaySoundEffect("PlayerHit", isLoop: false, volume: 1f);
                    Combat.OnAttackStarted();
                }

                break;
        }

        // advance sprite animation frames
        base.Update(gameTime);

        // animation-completed exits
        if (State == PlayerState.Attack && !IsAnimating())
            ToIdle();
        if (State == PlayerState.Hurt && !IsAnimating())
            ToIdle();

        // finally, sync colliders
        Colliders.UpdateRects(this);
    }

    private void ToIdle()
    {
        State = PlayerState.Idle;
        Anim.PlayIdleLoop();
    }

    // Backward-compat convenience wrappers 
    public void SetMaxHp(int max) => Health.SetMaxHp(max);
    public void Heal(int amount) => Health.Heal(amount);
    public void Damage(int amount) => Health.Damage(amount);
    public void SetDeflectHealThreshold(int threshold) => Combat.SetDeflectHealThreshold(threshold);
    public void ResetDeflectStreak() => Combat.ResetDeflectStreak();
}
