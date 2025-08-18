using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
namespace FinalProjMonoGame.UI;

// base class for all ui elements in the game
// provides Update and Draw with corresponding Enabled/Visible checks
public abstract class UIElement: IUpdateable, IDrawable
{
    public Vector2 Position { get; set; }
    public bool Enabled { get; set; } = true; // by default runs Update
    public bool Visible { get; set; } = true; // by default runs Draw

    public void Update(GameTime gameTime)
    {
        if (!Enabled) return; // checks the flag
        OnUpdate(gameTime);
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        if (!Visible) return; // checks the flag
        OnDraw(spriteBatch);
    }

    protected abstract void OnUpdate(GameTime gameTime); // Update logic to be overriden in derived classes
    protected abstract void OnDraw(SpriteBatch spriteBatch); // Draw logic to be overriden in derived classes
}