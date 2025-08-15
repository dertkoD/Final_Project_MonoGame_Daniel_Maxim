using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FinalProjMonoGame;


public class Sprite : IUpdateable, IDrawable
{
    public enum OriginPosition
    {
        TopLeft,
        Center
    }
    
    public Vector2 position = Vector2.Zero;
    public Vector2 scale = Vector2.One;
    public float rotation = 0.0f;
    public SpriteEffects effect = SpriteEffects.None;
    public OriginPosition originPosition = OriginPosition.Center;
    
    protected Vector2 origin = Vector2.Zero;
    protected Texture2D texture;
    protected Rectangle? sourceRectangle = null;

    public Rectangle rect { get; set; }

    public Sprite(string textureName)
    {
        this.texture = SpriteManager.GetSprite(textureName).texture;

        rect = GetDestRectangle(texture.Bounds);
    }

    protected virtual void SetOrigin(OriginPosition originPosition)
    {
        switch (originPosition)
        {
            case OriginPosition.TopLeft:
                origin = Vector2.Zero;
                break;
                
            case OriginPosition.Center:
                origin = new Vector2(texture.Width * 0.5f, texture.Height * 0.5f);
                break;
        }
    }

    public virtual void Update(GameTime gameTime)
    {
        SetOrigin(originPosition);
        
        var src = sourceRectangle ?? texture.Bounds;
        rect = GetDestRectangle(src);
    }

    protected Rectangle GetDestRectangle(Rectangle rect)
    {
        int width = (int)(rect.Width * scale.X);
        int height = (int)(rect.Height * scale.Y);

        int pos_x = (int)(position.X - origin.X * scale.X);
        int pos_y  = (int)(position.Y - origin.Y * scale.Y);

        return new Rectangle(pos_x, pos_y, width, height);
    }

    public virtual void Draw(SpriteBatch _spriteBatch)
    {
        _spriteBatch.Draw(
            texture,
            position,
            sourceRectangle,
            Color.White,
            MathHelper.ToRadians(rotation),
            origin,
            scale,
            effect,
            0
        );
    }
}