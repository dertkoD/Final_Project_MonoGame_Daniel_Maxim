using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FinalProjMonoGame;

public class MenuTransition : IUpdateable, IDrawable
{
    private readonly ParallaxBackground _parallax;
    private readonly GroundLayer _ground;
    private readonly Action<float> _setMenuSlideOffsetY; // menu.SlideOffsetY setter (pixels)
    private readonly Action _startGame;

    private enum Phase
    {
        Idle,
        Running,
        Done
    }

    private Phase _phase = Phase.Idle;

    // timings
    private readonly float _stopParallaxTime = 0.5f;
    private readonly float _menuSlideTime = 0.5f;
    private readonly float _groundSlideTime = 0.7f;
    private float _t = 0f;

    // anim values
    private float _menuStartY = 0f, _menuEndY;
    private float _groundStartY, _groundEndY = 0f;

    public MenuTransition(ParallaxBackground parallax, GroundLayer groundForTransition,
        Action<float> setMenuSlideOffsetY, Action startGame)
    {
        _parallax = parallax;
        _ground = groundForTransition;
        _setMenuSlideOffsetY = setMenuSlideOffsetY;
        _startGame = startGame;

        // Start positions: menu at 0 (on screen), ground below the screen
        _menuStartY = 0f;
        _menuEndY = -Game1.ScreenSize.Y; // move menu up by a full screen
        _groundStartY = Game1.ScreenSize.Y; // slide ground from bottom
        _ground.SetYOffset(_groundStartY);
    }

    public void Begin()
    {
        if (_phase != Phase.Idle) return;
        _phase = Phase.Running;

        // Kick off smooth stop on parallax
        _parallax?.SmoothStop(_stopParallaxTime);

        _t = 0f;
    }

    public void Update(GameTime gameTime)
    {
        if (_phase != Phase.Running) return;

        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
        _t += dt;

        // Easing helpers
        float EaseOutCubic(float x) => 1f - (float)Math.Pow(1f - MathHelper.Clamp(x, 0f, 1f), 3f);

        float EaseInOutCubic(float x)
        {
            x = MathHelper.Clamp(x, 0f, 1f);
            return x < 0.5f ? 4f * x * x * x : 1f - (float)Math.Pow(-2f * x + 2f, 3f) / 2f;
        }

        // 1) Menu slide up (0..menuSlideTime)
        float tm = Math.Min(_t / _menuSlideTime, 1f);
        float menuY = MathHelper.Lerp(_menuStartY, _menuEndY, EaseOutCubic(tm));
        _setMenuSlideOffsetY?.Invoke(menuY);

        // 2) Ground slides in (start immediately; lasts groundSlideTime)
        float tg = Math.Min(_t / _groundSlideTime, 1f);
        float groundY = MathHelper.Lerp(_groundStartY, _groundEndY, EaseInOutCubic(tg));
        _ground.SetYOffset(groundY);

        // 3) When both finished, start the game
        if (tm >= 1f && tg >= 1f)
        {
            _phase = Phase.Done;
            _startGame?.Invoke();
        }
    }

    public void Draw(SpriteBatch spriteBatch)
    {
    }
}