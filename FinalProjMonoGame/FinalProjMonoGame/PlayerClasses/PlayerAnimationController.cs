namespace FinalProjMonoGame.PlayerClasses;

// Drives Animation API on Player: owns clip names, timings, and small stateful FX (shield flash).
public sealed class PlayerAnimationController
{
    private readonly Player _p;

    // Animation clip names (match your spritesheet keys)
    public const string IdleAnim = "PlayerIdle";
    public const string AttackAnim = "PlayerHit";
    public const string DefendAnim = "PlayerDefend";
    public const string TakingDamageAnim = "TakingDamage";
    public const string DeathAnim = "Death";
    public const string ShieldBlockAnim = "ShieldBlock";

    // Small FX overlay while blocking (temporary swap to ShieldBlock)
    private float _shieldBlockFxTime = 0.25f;
    private float _shieldBlockTimer = 0f;

    public PlayerAnimationController(Player p)
    {
        _p = p;
    }

    // Set default clip on spawn to avoid empty frame on first Update.
    public void Init()
    {
        PlayIdleLoop();
    }

    // Ticks temporary FX timers; does NOT advance base Animation frames (Player.Update() calls that).
    public void Update(float dt)
    {
        if (_shieldBlockTimer > 0f)
        {
            _shieldBlockTimer -= dt;
            if (_shieldBlockTimer <= 0f && _p.State == PlayerState.Defend)
            {
                PlayDefendLoop();
            }
        }
    }
    
    // Single place to talk to Animation base (ChangeAnimation + PlayAnimation).
    private void Play(string clip, bool loop, int fps)
    {
        _p.ChangeAnimation(clip);
        _p.PlayAnimation(loop, fps);
    }

    // Public intents: looped idles/defend and one-shots for attack/hurt/death.
    public void PlayIdleLoop()    => Play(IdleAnim,          true,  8);
    public void PlayAttackOnce()  => Play(AttackAnim,        false, 12);
    public void PlayDefendLoop()  => Play(DefendAnim,        true,  8);
    public void PlayHurtOnce()    => Play(TakingDamageAnim,  false, 12);
    public void PlayDeathOnce()   => Play(DeathAnim,         false, 12);

    // Trigger short overlay while blocking; guarded so it only plays during Defend.
    public void TriggerShieldBlockFx()
    {
        if (_p.State != PlayerState.Defend) return;
        _shieldBlockTimer = _shieldBlockFxTime;
        Play(ShieldBlockAnim, false, 14);
    }
}