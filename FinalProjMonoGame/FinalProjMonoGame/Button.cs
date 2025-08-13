using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace FinalProjMonoGame;

public class Button : IUpdateable, IDrawable
{
    public Rectangle Bounds;

    // Спрайт фона (из SpriteManager)
    private SpriteSheetInfo spriteSheet;
    private int frameIndex = 0;

    // Масштаб для "старого" режима (когда не тянем под Bounds)
    private float textureScale = 1f;

    // Режимы отрисовки
    private bool useSprite = false;
    private bool stretchToBounds = true; // ВАЖНО: по умолчанию растягиваем под Bounds

    // Фолбек-заливка
    private Texture2D fallbackPixel;
    private Color fillColor = Color.Transparent;

    // Визуал
    private Color baseTint = Color.White;
    private Color hoverTint = Color.White;
    private bool isHovering;
    private bool wasPressedLastFrame;

    // Текст
    private SpriteFont font;
    private string text = "";
    private Color textColor = Color.White;
    private float textScale = 1f;

    // Слои
    private float layerDepth = 0.5f;

    // Клик
    public Action<Button> IsClicked;

    public Button(GraphicsDevice gd)
    {
        fallbackPixel = new Texture2D(gd, 1, 1);
        fallbackPixel.SetData(new[] { Color.White });
    }

    // ---------- ПУБЛИЧНОЕ API ----------

    /// <summary>Задать фон из SpriteManager. stretch=true — тянуть спрайт под всю кнопку.</summary>
    public void SetSprite(string spriteName, int frame = 0, float scale = 1f, bool stretch = true)
    {
        spriteSheet = SpriteManager.GetSprite(spriteName);
        frameIndex = frame;
        textureScale = scale;
        useSprite = true;
        stretchToBounds = stretch;
    }

    /// <summary>Цветовая заливка вместо спрайта.</summary>
    public void SetFillColor(Color color)
    {
        fillColor = color;
        useSprite = false;
    }

    public void SetTint(Color normal, Color hover)
    {
        baseTint = normal;
        hoverTint = hover;
    }

    public void SetText(SpriteFont font, string content, Color color, float scale = 1f)
    {
        this.font = font;
        text = content;
        textColor = color;
        textScale = scale;
    }

    public void SetLayerDepth(float depth) => layerDepth = MathHelper.Clamp(depth, 0f, 1f);

    /// <summary>Установить прямоугольник кнопки (позиция и размер).</summary>
    public void SetBounds(Rectangle rect) => Bounds = rect;

    /// <summary>Изменить только размер (оставляя позицию).</summary>
    public void SetSize(int width, int height)
    {
        Bounds = new Rectangle(Bounds.X, Bounds.Y, width, height);
    }

    /// <summary>Изменить только позицию (оставляя размер).</summary>
    public void SetPosition(int x, int y)
    {
        Bounds = new Rectangle(x, y, Bounds.Width, Bounds.Height);
    }

    /// <summary>Установить размер по центру в точке.</summary>
    public void SetCenteredBounds(Point size, Vector2 center)
    {
        Bounds = new Rectangle(
            (int)(center.X - size.X * 0.5f),
            (int)(center.Y - size.Y * 0.5f),
            size.X, size.Y);
    }

    // ---------- ЛОГИКА ----------

    public void Update(GameTime gameTime)
    {
        var mouse = Mouse.GetState();
        isHovering = Bounds.Contains(mouse.Position);

        bool pressedNow = mouse.LeftButton == ButtonState.Pressed;
        bool wasPressed = wasPressedLastFrame;

        if (pressedNow && !wasPressed)
            wasPressedLastFrame = true;

        bool releasedNow = mouse.LeftButton == ButtonState.Released && wasPressedLastFrame;
        if (releasedNow && isHovering)
            IsClicked?.Invoke(this);

        if (!pressedNow) wasPressedLastFrame = false;
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        Color tint = isHovering ? hoverTint : baseTint;

        // 1) Фон
        if (!useSprite)
        {
            if (fillColor.A > 0)
                spriteBatch.Draw(fallbackPixel, Bounds, null, fillColor, 0f, Vector2.Zero, SpriteEffects.None, layerDepth);
        }
        else if (spriteSheet?.texture != null && Bounds.Width > 0 && Bounds.Height > 0)
        {
            int fw = spriteSheet.texture.Width / Math.Max(1, spriteSheet.columns);
            int fh = spriteSheet.texture.Height / Math.Max(1, spriteSheet.rows);
            int total = Math.Max(1, spriteSheet.columns * spriteSheet.rows);

            int idx = ((frameIndex % total) + total) % total;
            int fx = idx % spriteSheet.columns;
            int fy = idx / spriteSheet.columns;

            Rectangle src = new Rectangle(fx * fw, fy * fh, fw, fh);

            if (stretchToBounds)
            {
                // Тянем ровно под Bounds (и по ширине, и по высоте)
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
                // Старый режим: рисуем по scale по центру Bounds
                Vector2 scaled = new Vector2(src.Width * textureScale, src.Height * textureScale);
                Vector2 pos = new Vector2(
                    Bounds.X + (Bounds.Width - scaled.X) / 2f,
                    Bounds.Y + (Bounds.Height - scaled.Y) / 2f
                );
                // Снэп к пиксельной сетке
                pos = new Vector2((int)Math.Round(pos.X), (int)Math.Round(pos.Y));

                spriteBatch.Draw(
                    texture: spriteSheet.texture,
                    position: pos,
                    sourceRectangle: src,
                    color: tint,
                    rotation: 0f,
                    origin: Vector2.Zero,
                    scale: textureScale,
                    effects: SpriteEffects.None,
                    layerDepth: layerDepth
                );
            }
        }

        // 2) Текст по центру
        if (font != null && !string.IsNullOrEmpty(text))
        {
            Vector2 tsize = font.MeasureString(text) * textScale;
            Vector2 center = new Vector2(Bounds.X + Bounds.Width / 2f, Bounds.Y + Bounds.Height / 2f);
            center = new Vector2((int)Math.Round(center.X), (int)Math.Round(center.Y));

            // Лёгкая тень + основной
            spriteBatch.DrawString(font, text, center + new Vector2(1, 1), Color.Black * 0.6f,
                0f, tsize * 0.5f, textScale, SpriteEffects.None, layerDepth + 0.00005f);
            spriteBatch.DrawString(font, text, center, textColor,
                0f, tsize * 0.5f, textScale, SpriteEffects.None, layerDepth + 0.0001f);
        }
    }
}