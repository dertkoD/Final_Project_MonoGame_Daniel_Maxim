using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FinalProjMonoGame.UI;

public class TimerUI : UIElement
{
    private readonly SpriteFont _font;

    public Color Color = Color.White;
    public float Scale = 1f;
    public bool AlignCenter = true;

    private TimeSpan _elapsed = TimeSpan.Zero;

    public TimerUI(SpriteFont font)
    {
        _font = font;
        //default top center position
        Position = new Vector2(0, 16);
    }

    public void Reset() => _elapsed = TimeSpan.Zero;

    protected override void OnUpdate(GameTime gameTime)
    {
        _elapsed += gameTime.ElapsedGameTime;
    }

    protected override void OnDraw(SpriteBatch spriteBatch)
    {
        string text = FormatTime(_elapsed);
        Vector2 sizeUnscaled = _font.MeasureString(text);
        // decides whether to align center or left
        Vector2 origin = AlignCenter ? sizeUnscaled * 0.5f : Vector2.Zero; 

        spriteBatch.DrawString(
            _font,
            text,
            Position,
            Color,
            rotation: 0f,
            origin: origin,
            scale: Scale,
            effects: SpriteEffects.None,
            layerDepth: 0f
            );
    }

    private static string FormatTime(TimeSpan t)
    {
        return $"{(int)t.TotalMinutes:00}:{t.Seconds:00}";
    }
}