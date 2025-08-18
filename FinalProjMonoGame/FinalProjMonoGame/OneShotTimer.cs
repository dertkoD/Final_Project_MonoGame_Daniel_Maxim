using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FinalProjMonoGame;

// One-shot countdown timer that invokes a callback exactly once.
// Frame-rate independent: uses GameTime delta to decrement.
public class OneShotTimer : IUpdateable, IDrawable
{
    private float _left; // remaining time in seconds
    private readonly Action _onFire; // callback to invoke on expiry
    private bool _fired; // guards against multiple invokes

    // Negative input is clamped to 0 => fires on the next Update tick.
    public OneShotTimer(float seconds, Action onFire)
    {
        _left   = seconds < 0f ? 0f : seconds;
        _onFire = onFire;
    }

    // Decrements remaining time and invokes the callback once when <= 0.
    // No allocations here; relies on caller-provided delegate.
    public void Update(GameTime gameTime)
    {
        if (_fired) return;
        _left -= (float)gameTime.ElapsedGameTime.TotalSeconds;
        if (_left <= 0f)
        {
            _fired = true;
            _onFire?.Invoke(); // safe if onFire is null
        }
    }

    // No visual; exists to satisfy IDrawable in the scene graph.
    public void Draw(SpriteBatch spriteBatch) { }
}