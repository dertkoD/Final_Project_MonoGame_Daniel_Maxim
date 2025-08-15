using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace FinalProjMonoGame;

public class Player : Animation
{
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
    }

    public void OnTrigger(object o)
    {
        //AudioManager.PlaySoundEffect("Collect", false, 0.2f);
        
        Console.WriteLine("Trigger with " + o.ToString());

        if (o is Enemy enemy)
        {
            SceneManager.Remove(enemy.collider);
            SceneManager.Remove(enemy);
        }
    }
    public void OnCollision(object o)
    {
        //AudioManager.PlaySoundEffect("Bounce");
        
        //position = prevPos;
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
                PlayAnimation(inLoop: false, fps: 12); // play once
                
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
        if (collider != null) collider.rect = rect; 
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