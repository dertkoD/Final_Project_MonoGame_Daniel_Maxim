using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace FinalProjMonoGame;

public class Player : Animation
{
    private int speed = 500;
    private KeyboardState prevKeyState = Keyboard.GetState();

    public Collider collider;
    
    private Vector2 prevPos;
    public Player() : base("NewForm")
    {
        position = Game1.ScreenCenter;
        scale = new Vector2(0.2f, 0.2f);
        PlayAnimation();
        
        collider = SceneManager.Create<Collider>();
        
        AudioManager.PlaySong("Theme");
    }

    public void OnTrigger(object o)
    {
        AudioManager.PlaySoundEffect("Collect", false, 0.2f);
        
        Console.WriteLine("Trigger with " + o.ToString());

        if (o is Enemy enemy)
        {
            SceneManager.Remove(enemy.collider);
            SceneManager.Remove(enemy);
        }
    }
    public void OnCollision(object o)
    {
        AudioManager.PlaySoundEffect("Bounce");
        
        position = prevPos;
    }

    public override void Update(GameTime gameTime)
    {
        prevPos = position;

        KeyboardState state = Keyboard.GetState();
        if (state.IsKeyDown(Keys.NumPad1))
        {
            ChangeAnimation("Normal");
            PlayAnimation();
        }
        if (state.IsKeyDown(Keys.NumPad1))
        {
            ChangeAnimation("Duck1");
            PlayAnimation();
        }
        if (state.IsKeyDown(Keys.NumPad2))
        {
            ChangeAnimation("Duck2");
            PlayAnimation();
        }
        if (state.IsKeyDown(Keys.NumPad3))
        {
            ChangeAnimation("Egret2");
            PlayAnimation();
        }
        
        if (state.IsKeyDown(Keys.NumPad9))
        {
            AudioManager.SongVolume = 0.5f;
        }
        if (state.IsKeyDown(Keys.NumPad8))
        {
            AudioManager.SoundEffectVolume = 0.2f;
        }
        if (state.IsKeyDown(Keys.NumPad7) && prevKeyState.IsKeyUp(Keys.NumPad7))
        {
            AudioManager.MuteAudio();
        }
        if (state.IsKeyDown(Keys.NumPad6) && prevKeyState.IsKeyUp(Keys.NumPad6))
        {
            AudioManager.UnmuteAudio();
        }

        if (state.IsKeyDown(Keys.D))
        {
            position.X += speed * (float)gameTime.ElapsedGameTime.TotalSeconds;
            effect = SpriteEffects.FlipHorizontally;
        }
        if (state.IsKeyDown(Keys.A))
        {
            position.X -= speed * (float)gameTime.ElapsedGameTime.TotalSeconds;
            effect = SpriteEffects.None;
        }
        if (state.IsKeyDown(Keys.W))
        {
            position.Y -= speed * (float)gameTime.ElapsedGameTime.TotalSeconds;
        }
        if (state.IsKeyDown(Keys.S))
        {
            position.Y += speed * (float)gameTime.ElapsedGameTime.TotalSeconds;
        }

        prevKeyState = state;
        base.Update(gameTime);

        collider.rect = rect;
       // collider.rect.Size -= new Point(50, 50);
       // collider.rect.Location += new Point(50, 50);
    }

}