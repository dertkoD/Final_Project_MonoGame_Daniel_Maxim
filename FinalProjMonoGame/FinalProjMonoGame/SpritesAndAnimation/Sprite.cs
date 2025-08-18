using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FinalProjMonoGame;

// Base renderable sprite: position/scale/rotation/origin + optional source rectangle.
public class Sprite : IUpdateable, IDrawable
{
    // World transform and flip state; origin policy (TopLeft or Center).
    public Vector2 position = Vector2.Zero;
    public Vector2 scale = Vector2.One;
    public float rotation = 0.0f;
    public SpriteEffects effect = SpriteEffects.None;
    public OriginPosition originPosition = OriginPosition.Center;
    
    // Texture data and source rect; origin is recomputed from texture or current frame.
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

    // Update recomputes origin and the destination rect each frame (keeps Draw cheap).
    public virtual void Update(GameTime gameTime)
    {
        SetOrigin(originPosition);
        
        var src = sourceRectangle ?? texture.Bounds;
        rect = GetDestRectangle(src);
    }

    // Maps source size + transform into screen-space rectangle (no rotation/flip applied).
    protected Rectangle GetDestRectangle(Rectangle rect)
    {
        int width = (int)(rect.Width * scale.X);
        int height = (int)(rect.Height * scale.Y);

        int pos_x = (int)(position.X - origin.X * scale.X);
        int pos_y  = (int)(position.Y - origin.Y * scale.Y);

        return new Rectangle(pos_x, pos_y, width, height);
    }

    // Draw uses SpriteBatch with current transform and optional sub-rectangle.
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