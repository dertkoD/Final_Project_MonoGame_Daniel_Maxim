using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
namespace FinalProjMonoGame.UI;

public abstract class UIElement: IUpdateable, IDrawable
{
    // top left default position
    public Vector2 Position { get; set; } = Vector2.Zero;

    // checks whether update should run
    public bool Enabled { get; set; } = true;

    // checks whether draw should render
    public bool Visible { get; set; } = true;

    public virtual void Update(GameTime gameTime)
    {
        if (!Enabled) return;
        OnUpdate(gameTime);
    }

    public virtual void Draw(SpriteBatch spriteBatch)
    {
        if (!Visible) return;
        OnDraw(spriteBatch);
    }

    protected abstract void OnUpdate(GameTime gameTime);
    protected abstract void OnDraw(SpriteBatch spriteBatch);
}