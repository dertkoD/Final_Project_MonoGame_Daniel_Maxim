using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
namespace FinalProjMonoGame.UI;

public abstract class UIElement: IUpdateable, IDrawable
{
    public Vector2 Position { get; set; }
    public bool Enabled { get; set; } = true;
    public bool Visible { get; set; } = true;

    public void Update(GameTime gameTime)
    {
        if (!Enabled) return;
        OnUpdate(gameTime);
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        if (!Visible) return;
        OnDraw(spriteBatch);
    }

    protected abstract void OnUpdate(GameTime gameTime);
    protected abstract void OnDraw(SpriteBatch spriteBatch);
}