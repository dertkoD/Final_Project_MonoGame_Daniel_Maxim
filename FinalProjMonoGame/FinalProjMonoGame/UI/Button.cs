using System;
using FinalProjMonoGame.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace FinalProjMonoGame;

public class Button : UIElement
{
    public Rectangle Bounds;

    // background sprite
    private SpriteSheetInfo spriteSheet;
    private int frameIndex = 0;

    // scale for "old mode" (without bounds)
    private float textureScale = 1f;

    // draw mode flags
    private bool useSprite = true;
    private bool stretchToBounds = true; // stretching to bounds work as default

    // fallback fill
    private Texture2D fallbackPixel;
    private Color fillColor = Color.Transparent;

    // visual tints
    private Color baseTint = Color.White;
    private Color hoverTint = Color.White;
    private bool isHovering;
    
    // text overlay
    private SpriteFont font;
    private string text = "";
    private Color textColor = Color.White;
    private float textScale = 1f;

    // layering
    private float layerDepth = 0.5f;

    private MouseState prevMouse;
    
    // click callback
    public Action<Button> IsClicked;
    
    public Button(GraphicsDevice gd)
    {
        // 1x1 white texture for fallback fill mode
        fallbackPixel = new Texture2D(gd, 1, 1);
        fallbackPixel.SetData(new[] { Color.White });

        // default button size near screen center
        Bounds = new Rectangle(
            (int)(Game1.ScreenCenter.X - 200),
            (int)(Game1.ScreenCenter.Y - 40),
            400, 80
        );

        prevMouse = Mouse.GetState();
    }
    
    // use a sprite as the button background, set stretch=true to fill bounds
    public void SetSprite(string spriteName, int frame = 0, float scale = 1f, bool stretch = true)
    {
        spriteSheet = SpriteManager.GetSprite(spriteName);
        frameIndex = frame;
        textureScale = scale;
        useSprite = true;
        stretchToBounds = stretch;
    }

    // solid color fill
    public void SetFillColor(Color color)
    {
        fillColor = color;
        useSprite = false;
    }

    // draw a solid color instead of sprite
    public void SetTint(Color normal, Color hover)
    {
        baseTint = normal;
        hoverTint = hover;
    }

    // set button label, color and scale
    public void SetText(SpriteFont font, string content, Color color, float scale = 1f)
    {
        this.font = font;
        text = content;
        textColor = color;
        textScale = scale;
    }

    // set draw order
    public void SetLayerDepth(float depth) => layerDepth = MathHelper.Clamp(depth, 0f, 1f); 
    // assign the Bounds rect
    public void SetBounds(Rectangle rect) => Bounds = rect;
    
    // place button so that its center equals the given point
    public void SetCenteredBounds(Point size, Vector2 center)
    {
        Bounds = new Rectangle(
            (int)(center.X - size.X * 0.5f),
            (int)(center.Y - size.Y * 0.5f),
            size.X, size.Y);
    }

    // logic
    
    // update hover state and detect click
    protected override void OnUpdate(GameTime gameTime)
    {
        var ms = Mouse.GetState();
        
        isHovering = Bounds.Contains(ms.Position);

        // edge-triggered press inside Bounds
        bool releasedInside = isHovering 
                       && ms.LeftButton == ButtonState.Pressed
                       && prevMouse.LeftButton == ButtonState.Released;
        
        if (releasedInside)
            IsClicked?.Invoke(this);

        prevMouse = ms;
    }

    // draw background and then centered text
    protected override void OnDraw(SpriteBatch spriteBatch)
    {
        Color tint = isHovering ? hoverTint : baseTint;

        // 1) background
        if (!useSprite)
        {
            // solid color fill
            if (fillColor.A > 0)
                spriteBatch.Draw(fallbackPixel, Bounds, null, fillColor, 0f, 
                    Vector2.Zero, SpriteEffects.None, layerDepth);
        }
        else if (spriteSheet?.texture != null && Bounds.Width > 0 && Bounds.Height > 0)
        {
            // calculate source rect for the chosen frame
            int fw = spriteSheet.texture.Width / Math.Max(1, spriteSheet.columns);
            int fh = spriteSheet.texture.Height / Math.Max(1, spriteSheet.rows);
            int total = Math.Max(1, spriteSheet.columns * spriteSheet.rows);

            int idx = ((frameIndex % total) + total) % total;
            int fx = idx % spriteSheet.columns;
            int fy = idx / spriteSheet.columns;

            Rectangle src = new Rectangle(fx * fw, fy * fh, fw, fh);

            if (stretchToBounds)
            {
                // stretch exactly to Bounds
                spriteBatch.Draw(
                    texture: spriteSheet.texture,
                    destinationRectangle: Bounds,
                    sourceRectangle: src,
                    color: tint,
                    rotation: 0f,
                    origin: Vector2.Zero,
                    effects: SpriteEffects.None,
                    layerDepth: layerDepth
                );
            }
            else
            {
                // old mode: draw at textureScale, centered in Bounds (no stretching)
                Vector2 center = new(Bounds.X + Bounds.Width / 2f, Bounds.Y + Bounds.Height / 2f);
                Vector2 origin = new(src.Width / 2f, src.Height / 2f);

                spriteBatch.Draw(
                    texture: spriteSheet.texture,
                    position: center,
                    sourceRectangle: src,
                    color: tint,
                    rotation: 0f,
                    origin: origin,
                    scale: textureScale,
                    effects: SpriteEffects.None,
                    layerDepth: layerDepth
                );
            }
        }

        // 2) centered text overlay (optional)
        if (font != null && !string.IsNullOrEmpty(text))
        {
            Vector2 tsize = font.MeasureString(text) * textScale;
            Vector2 center = new(Bounds.X + Bounds.Width / 2f, Bounds.Y + Bounds.Height / 2f);
            center = new((int)Math.Round(center.X), (int)Math.Round(center.Y)); // snap to pixels

            // small drop shadow + text
            spriteBatch.DrawString(font, text, center + new Vector2(1, 1), Color.Black * 0.6f,
                0f, tsize * 0.5f, textScale, SpriteEffects.None, layerDepth + 0.00005f);
            spriteBatch.DrawString(font, text, center, textColor,
                0f, tsize * 0.5f, textScale, SpriteEffects.None, layerDepth + 0.0001f);
        }
    }
}