using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace FinalProjMonoGame;

public class Player : Animation
{
    public Collider SwordCollider { get; private set; }
    public Collider ShieldCollider { get; private set; }

    // тюнинг размеров
    private float bodyWScale = 0.55f;   // тело уже спрайта
    private float bodyHScale = 0.70f;
    private int bodyYOffset = 6;        // чуть ниже центра

    // окно активности удара
    private float attackActiveTime = 0.18f;
    
    public int MaxHP { get; private set; } = 5;
    public int HP { get; private set; }
    
    private KeyboardState _prevKeys;
    private enum PlayerState { Idle, Attack, Defend }
    private PlayerState _state = PlayerState.Idle;

    private const string IdleAnim = "PlayerIdle";
    private const string AttackAnim = "PlayerHit";
    private const string DefendAnim = "PlayerDefend";

    private bool _facingRight = true;

    public int DeflectHealThreshold { get; private set; } = 3;
    private int _deflectCount = 0;
    
    public Collider collider;
    
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
        collider = SceneManager.Create<Collider>();
        collider.isTrigger = true;
        collider.OnTrigger += OnBodyTrigger;

        // SWORD
        SwordCollider = SceneManager.Create<Collider>();
        SwordCollider.isTrigger = true;
        SwordCollider.OnTrigger += OnSwordTrigger;

        // SHIELD
        ShieldCollider = SceneManager.Create<Collider>();
        ShieldCollider.isTrigger = true;
        ShieldCollider.OnTrigger += OnShieldTrigger;
    }
    
    private void OnSwordTrigger(object o)
    {
        if (_state != PlayerState.Attack) return;

        if (o is Bomb bomb)
        {
            // Уже отбита? Не трогаем повторно — иначе будет «дрыгать» внутри триггера.
            if (bomb.IgnorePlayerCollision) return;

            // «В другую сторону, откуда прилетела»: просто развернуть вектор.
            Vector2 incoming = bomb.Velocity;
            if (incoming.LengthSquared() < 1f)
            {
                // На всякий, если почти стояла – пихнём вперёд от меча
                incoming = FacingNormal() * 300f;
            }

            Vector2 newVel = -incoming;
            float speed = Math.Max(incoming.Length(), 300f); // минимальная скорость отбивки
            newVel = Vector2.Normalize(newVel) * speed;

            bomb.Deflect(newVel); // внутри выставит IgnorePlayerCollision = true

            // Небольшой «тычок» наружу, чтобы сразу выйти из тритга меча и больше не триггериться
            bomb.position += Vector2.Normalize(newVel) * 8f;

            RegisterDeflect();
            AudioManager.PlaySoundEffect("DeflectBomb", false, 1f);
        }
    }

    private void OnShieldTrigger(object o)
    {
        if (_state != PlayerState.Defend) return;
        if (o is Arrow arrow)
        {
            Vector2 n = FacingNormal();
            Vector2 newVel = Vector2.Reflect(arrow.Velocity, n) * 1.05f;
            if (newVel.LengthSquared() < 1f) newVel = n * Math.Max(400f, arrow.Velocity.Length());
            arrow.Velocity = newVel;

            // чтобы отлетевшая стрела не била тело
            arrow.IgnorePlayerCollision = true;
            RegisterDeflect();
            AudioManager.PlaySoundEffect("DeflectArrow", false, 1f);
        }
    }
    
    public void OnCollision(object o)
    {

    }

    public override void Update(GameTime gameTime)
    {
        var keys = Keyboard.GetState();
        bool Pressed(Keys k) => keys.IsKeyDown(k) && _prevKeys.IsKeyUp(k);
        
        // facing direction
        if (keys.IsKeyDown(Keys.A) || keys.IsKeyDown(Keys.Left))
            _facingRight = false;
        else if (keys.IsKeyDown(Keys.D) || keys.IsKeyDown(Keys.Right))
            _facingRight = true;

        // apply visual flip
        effect = _facingRight ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

        // defend/Attack logic, hold Q to defend (loops while held)
        if (keys.IsKeyDown(Keys.Q))
        {
            if (_state != PlayerState.Defend)
            {
                _state = PlayerState.Defend;
                ChangeAnimation(DefendAnim);
                PlayAnimation(inLoop: true, fps: 8);

                AudioManager.PlaySoundEffect("PlayerDefend", isLoop: false, volume: 1f);
            }
        }
        else
        {
            // one-shot attack on E
            if (Pressed(Keys.E) && _state != PlayerState.Attack)
            {
                _state = PlayerState.Attack;
                ChangeAnimation(AttackAnim);
                PlayAnimation(inLoop: false, fps: 12);
                AudioManager.PlaySoundEffect("PlayerHit", isLoop: false, volume: 1f);
            }
        }

        // return to idle after attack or after Q release
        if (_state == PlayerState.Attack && !IsAnimating())
        {
            ToIdle();
        }
        else if (_state == PlayerState.Defend && !keys.IsKeyDown(Keys.Q))
        {
            ToIdle();
        }

        _prevKeys = keys;
        
        base.Update(gameTime);
        
        
        UpdateBodyCollider();     
        UpdateSwordCollider();  
        UpdateShieldCollider();   
    }
    
    private void UpdateBodyCollider()
    {
        var r = rect;
        int w = (int)(r.Width * bodyWScale);
        int h = (int)(r.Height * bodyHScale);
        int x = r.Center.X - w / 2;
        int y = r.Center.Y - h / 2 + bodyYOffset;
        collider.rect = new Rectangle(x, y, w, h);
    }

    private void UpdateSwordCollider()
    {
        bool active = (_state == PlayerState.Attack);
        if (!active) { SwordCollider.rect = Rectangle.Empty; return; }

        Rectangle body = (collider != null && collider.rect != Rectangle.Empty) ? collider.rect : rect;

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
    
    private void OnBodyTrigger(object o)
    {
        if (o is Enemy enemy)
        {
            Damage(enemy.Damage);
            ResetDeflectStreak();
            enemy.Destroy();
        }
    }
    
    private Vector2 FacingNormal()
    {
        // Нормаль «наружу перед мечом»: вправо или влево
        return _facingRight ? new Vector2(1f, 0f) : new Vector2(-1f, 0f);
    }

    private void ToIdle()
    {
        _state = PlayerState.Idle;
        ChangeAnimation(IdleAnim);
        PlayAnimation(inLoop: true, fps: 8);
    }
    
    // player HP system
    public void Heal(int amount)
    {
        if (amount <= 0) return;
        HP = Math.Min(MaxHP, HP + amount);
    }
    
    public void Damage(int amount)
    {
        if (amount <= 0) return;
        HP = Math.Max(0, HP - amount);
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