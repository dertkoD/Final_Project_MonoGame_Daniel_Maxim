using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace FinalProjMonoGame;

public class Player : Animation
{
    public Collider SwordCollider { get; private set; }
    public Collider ShieldCollider { get; private set; }
    public Collider BodyCollider;

    // окно неуязвимости после получения урона
    private float _hurtCooldown;
    private const float HurtInvuln = 0.30f;

    // тюнинг размеров хитбокса тела
    private float bodyWScale = 0.55f;
    private float bodyHScale = 0.70f;
    private int bodyYOffset = 6;

    // окно активности удара (если понадобится для таймингов хитбокса)
    private float attackActiveTime = 0.18f;

    public int MaxHP { get; private set; } = 5;
    public int HP { get; private set; }

    private KeyboardState _prevKeys;

    private enum PlayerState { Idle, Attack, Defend, Hurt, Dead }
    private PlayerState _state = PlayerState.Idle;

    private const string IdleAnim = "PlayerIdle";
    private const string AttackAnim = "PlayerHit";
    private const string DefendAnim = "PlayerDefend";
    private const string TakingDamageAnim = "TakingDamage";
    private const string DeathAnim = "Death";

    private bool _facingRight = true;

    private int _deflectCount = 0;
    public int DeflectHealThreshold { get; private set; } = 3;

    public bool ControlIsEnabled { get; private set; } = true;
    public void SetControlsEnabled(bool enabled) => ControlIsEnabled = enabled;

    public event Action<int> OnDeflectHeal;

    public Player() : base(IdleAnim)
    {
        HP = MaxHP;

        ChangeAnimation(IdleAnim);
        PlayAnimation(inLoop: true, fps: 8);

        position = Game1.ScreenCenter;
        scale = new Vector2(1.5f, 1.5f);
        originPosition = OriginPosition.Center;
        effect = SpriteEffects.None;

        // BODY
        BodyCollider = SceneManager.Create<Collider>();
        BodyCollider.isTrigger = true;
        BodyCollider.OnTrigger += OnBodyTrigger;

        // SWORD
        SwordCollider = SceneManager.Create<Collider>();
        SwordCollider.isTrigger = true;
        SwordCollider.OnTrigger += OnSwordTrigger;

        // SHIELD
        ShieldCollider = SceneManager.Create<Collider>();
        ShieldCollider.isTrigger = true;
        ShieldCollider.OnTrigger += OnShieldTrigger;
    }

    // ======== Триггеры оружия / щита / тела ========

    private void OnSwordTrigger(object o)
    {
        if (_state != PlayerState.Attack) return;

        if (o is Bomb bomb)
        {
            if (bomb.IgnorePlayerCollision) return;

            Vector2 incoming = bomb.Velocity;
            if (incoming.LengthSquared() < 1f)
                incoming = FacingNormal() * 300f;

            Vector2 newVel = -incoming;
            float speed = Math.Max(incoming.Length(), 300f);
            newVel = Vector2.Normalize(newVel) * speed;

            bomb.Deflect(newVel);
            bomb.position += Vector2.Normalize(newVel) * 8f;

            RegisterDeflect();
            AudioManager.PlaySoundEffect("DeflectBomb", false, 1f);
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

            if (Math.Abs(bounce.X) < 120f)
                bounce.X = 120f * (n.X >= 0 ? 1f : -1f);

            float gravity = 1400f;
            float spinSpeedDegPerSec = 1080f;
            arrow.StartSpin(bounce, spinSpeedDegPerSec, gravity);

            RegisterDeflect();
            AudioManager.PlaySoundEffect("DeflectArrow", false, 1f);
        }
    }

    private void OnShieldTrigger(object o)
    {
        if (_state != PlayerState.Defend) return;

        if (o is Bomb bomb)
        {
            bomb.Explode(ignorePlayer: true);

            // Штраф за щит
            Damage(1);
            ResetDeflectStreak();

            AudioManager.PlaySoundEffect("DeflectBomb", false, 1f);
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

            if (Math.Abs(bounce.X) < 120f)
                bounce.X = 120f * (n.X >= 0 ? 1f : -1f);

            float gravity = 1400f;
            float spinSpeedDegPerSec = 1080f;
            arrow.StartSpin(bounce, spinSpeedDegPerSec, gravity);

            RegisterDeflect();
            AudioManager.PlaySoundEffect("DeflectArrow", false, 1f);
        }
    }

    private void OnBodyTrigger(object o)
    {
        if (HP <= 0 || _hurtCooldown > 0f) return;

        switch (o)
        {
            case Bomb bomb:
                bomb.Explode();            // Bomb.Explode сам начислит урон при пересечении
                ResetDeflectStreak();
                break;

            case Arrow arrow:
                Damage(arrow.Damage);
                ResetDeflectStreak();
                arrow.Destroy();
                break;

            case Enemy enemy:
                // общий случай на всякий: контактный урон
                Damage(enemy.Damage);
                ResetDeflectStreak();
                enemy.Destroy();
                break;
        }

        _hurtCooldown = HurtInvuln;
    }

    // ======== Главный Update со стейт-машиной ========

    public override void Update(GameTime gameTime)
    {
        _hurtCooldown = Math.Max(0f, _hurtCooldown - (float)gameTime.ElapsedGameTime.TotalSeconds);

        var keys = Keyboard.GetState();
        bool Pressed(Keys k) => keys.IsKeyDown(k) && _prevKeys.IsKeyUp(k);

        // Глобальная проверка смерти (на случай внешних модификаций HP)
        if (HP <= 0 && _state != PlayerState.Dead)
        {
            _state = PlayerState.Dead;
            ControlIsEnabled = false;
            ChangeAnimation(DeathAnim);
            PlayAnimation(inLoop: false, fps: 12);
            Game1.Instance.TriggerGameOver(this, 2.0);
        }

        // В управляемых состояниях — обновляем направление
        if (_state == PlayerState.Idle || _state == PlayerState.Attack || _state == PlayerState.Defend)
        {
            if (keys.IsKeyDown(Keys.A) || keys.IsKeyDown(Keys.Left)) _facingRight = false;
            else if (keys.IsKeyDown(Keys.D) || keys.IsKeyDown(Keys.Right)) _facingRight = true;
            effect = _facingRight ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
        }

        // Локальная логика состояний
        switch (_state)
        {
            case PlayerState.Dead:
                // Ничего — доигрываем анимацию смерти
                break;

            case PlayerState.Hurt:
                // В Hurt игнорим ввод (стаггер). Если хочешь кэнсел щитом — скажи.
                break;

            case PlayerState.Defend:
                // Держим Q — остаёмся в Defend; отпустили — в Idle
                if (!keys.IsKeyDown(Keys.Q))
                    ToIdle();
                break;

            case PlayerState.Attack:
                // Ждём окончания анимации атаки
                break;

            case PlayerState.Idle:
                if (keys.IsKeyDown(Keys.Q))
                {
                    _state = PlayerState.Defend;
                    ChangeAnimation(DefendAnim);
                    PlayAnimation(inLoop: true, fps: 8);
                    AudioManager.PlaySoundEffect("PlayerDefend", isLoop: false, volume: 1f);
                }
                else if (Pressed(Keys.E))
                {
                    _state = PlayerState.Attack;
                    ChangeAnimation(AttackAnim);
                    PlayAnimation(inLoop: false, fps: 12);
                    AudioManager.PlaySoundEffect("PlayerHit", isLoop: false, volume: 1f);
                }
                
                break;
        }

        // Продвигаем анимации
        base.Update(gameTime);

        // Унифицированные выходы из анимационных стейтов
        if (_state == PlayerState.Attack && !IsAnimating())
            ToIdle();

        if (_state == PlayerState.Hurt && !IsAnimating())
            ToIdle();

        _prevKeys = keys;

        // Коллайдеры
        UpdateBodyCollider();
        UpdateSwordCollider();
        UpdateShieldCollider();
    }

    // ======== Хитбоксы ========

    private void UpdateBodyCollider()
    {
        var r = rect;
        int w = (int)(r.Width * bodyWScale);
        int h = (int)(r.Height * bodyHScale);
        int x = r.Center.X - w / 2;
        int y = r.Center.Y - h / 2 + bodyYOffset;
        BodyCollider.rect = new Rectangle(x, y, w, h);
    }

    private void UpdateSwordCollider()
    {
        bool active = (_state == PlayerState.Attack);
        if (!active) { SwordCollider.rect = Rectangle.Empty; return; }

        Rectangle body = (BodyCollider != null && BodyCollider.rect != Rectangle.Empty) ? BodyCollider.rect : rect;

        int w = (int)(body.Width * 0.45f);
        int h = body.Height;
        int y = body.Center.Y - h / 2;

        int x = _facingRight ? body.Right : body.Left - w;
        int overlap = (int)Math.Max(1, body.Width * 0.02f);
        x += _facingRight ? -overlap : overlap;

        SwordCollider.rect = new Rectangle(x, y, w, h);
    }

    private void UpdateShieldCollider()
    {
        bool active = (_state == PlayerState.Defend);

        if (!active)
        {
            ShieldCollider.rect = Rectangle.Empty;
            return;
        }

        int w = (int)(rect.Width * 0.3f);
        int h = (int)(rect.Height * 0.75f);
        int x = _facingRight ? rect.Center.X : rect.Center.X - w;
        int y = rect.Center.Y - h / 2;
        ShieldCollider.rect = new Rectangle(x, y, w, h);
    }

    // ======== Вспомогательные ========

    private Vector2 FacingNormal()
    {
        return _facingRight ? new Vector2(1f, 0f) : new Vector2(-1f, 0f);
    }

    private void ToIdle()
    {
        _state = PlayerState.Idle;
        ChangeAnimation(IdleAnim);
        PlayAnimation(inLoop: true, fps: 8);
    }

    // HP

    public void Heal(int amount)
    {
        if (amount <= 0 || _state == PlayerState.Dead) return;
        HP = Math.Min(MaxHP, HP + amount);
    }

    public void Damage(int amount)
    {
        if (amount <= 0 || _state == PlayerState.Dead) return;

        HP = Math.Max(0, HP - amount);

        if (HP > 0)
        {
            _state = PlayerState.Hurt;
            ChangeAnimation(TakingDamageAnim);
            PlayAnimation(inLoop: false, fps: 12);
        }
        else
        {
            _state = PlayerState.Dead;
            ControlIsEnabled = false;
            ChangeAnimation(DeathAnim);
            PlayAnimation(inLoop: false, fps: 12);
            Game1.Instance.TriggerGameOver(this, 2.0);
        }
    }

    public void SetMaxHp(int max)
    {
        MaxHP = Math.Max(1, max);
        HP = Math.Min(HP, MaxHP);
    }

    // deflect -> HP interaction

    public void SetDeflectHealThreshold(int threshold)
    {
        DeflectHealThreshold = threshold;
        ResetDeflectStreak();
    }

    public void RegisterDeflect()
    {
        _deflectCount++;

        if (_deflectCount >= DeflectHealThreshold)
        {
            _deflectCount = 0;
            Heal(1);
            OnDeflectHeal?.Invoke(HP);
        }
    }

    public void ResetDeflectStreak()
    {
        _deflectCount = 0;
    }
}