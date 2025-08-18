using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FinalProjMonoGame;

public class OneShotTimer : IUpdateable, IDrawable
{
    private float _left;
    private readonly System.Action _onFire;
    private bool _fired;

    public OneShotTimer(float seconds, System.Action onFire)
    {
        _left   = seconds < 0f ? 0f : seconds;
        _onFire = onFire;
    }

    public void Update(GameTime gameTime)
    {
        if (_fired) return;
        _left -= (float)gameTime.ElapsedGameTime.TotalSeconds;
        if (_left <= 0f)
        {
            _fired = true;
            _onFire?.Invoke();
        }
    }

    public void Draw(SpriteBatch spriteBatch) { }
}