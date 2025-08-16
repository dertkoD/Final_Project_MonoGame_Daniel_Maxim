using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FinalProjMonoGame;

public class Text : IUpdateable, IDrawable
{
    private SpriteFont font;

    public string text;
    public Vector2 position = Vector2.Zero;
    public Vector2 scale = Vector2.One;
    public float rotation = 0.0f;

    private Vector2 textCenter;
    public Text(SpriteFont font)
    {
        this.font = font;
    }

    public virtual void Update(GameTime gameTime)
    {
        textCenter = font.MeasureString(text) * 0.5f;  
    }

    public void Draw(SpriteBatch _spriteBatch)
    {
        _spriteBatch.DrawString(
            font,
            text,
            position,
            Color.White,
            MathHelper.ToRadians(rotation),
            textCenter,
            scale,
            SpriteEffects.None,
            0
        );
    }
}