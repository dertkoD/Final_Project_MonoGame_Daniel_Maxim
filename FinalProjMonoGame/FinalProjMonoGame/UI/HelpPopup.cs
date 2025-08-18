using System;
using System.Collections.Generic;
using System.Numerics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Vector2 = Microsoft.Xna.Framework.Vector2;


namespace FinalProjMonoGame.UI;

public class HelpPopup : UIElement
{
    public Rectangle Bounds;
    public string Text { get; set; } = string.Empty;

    // appearance
    public int Padding { get; set; } = 40;
    public float TextScale { get; set; } = 1.1f;
    public float LineSpacingMul { get; set; } = 1.15f;
    public Color TextColor { get; set; } = Color.White;
    public Color FallbackPanelColor { get; set; } = new Color(0, 0, 0, 170);

    private readonly SpriteFont _font;
    private readonly string _windowSpriteName;
    private Texture2D _pixel;

    public HelpPopup(SpriteFont font, string windowSpriteName = "Window")
    {
        _font = font;
        _windowSpriteName = windowSpriteName;
    }

    protected override void OnUpdate(GameTime gameTime) {}

    protected override void OnDraw(SpriteBatch sb)
    {
        EnsurePixel(sb.GraphicsDevice);
        sb.Draw(_pixel, Bounds, FallbackPanelColor);

        var area = new Rectangle(
            Bounds.X + Padding,
            Bounds.Y + Padding,
            Math.Max(1, Bounds.Width  - Padding * 2),
            Math.Max(1, Bounds.Height - Padding * 2)
        );

        DrawWrappedString(sb, _font, Text ?? string.Empty, area, TextColor, TextScale, LineSpacingMul);
    }

    private void EnsurePixel(GraphicsDevice gd)
    {
        if (_pixel != null) return;
        _pixel = new Texture2D(gd, 1, 1);
        _pixel.SetData(new[] { Color.White });
    }

    // ------- Measuring & drawing (share tokenization) -------

    private static List<string> TokenizePreservingSpaces(string text)
    {
        var tokens = new List<string>();
        if (string.IsNullOrEmpty(text)) return tokens;

        for (int i = 0; i < text.Length;)
        {
            char c = text[i];

            if (c == '\r') { i++; continue; }
            if (c == '\n') { tokens.Add("\n"); i++; continue; }

            if (c == ' ')
            {
                int j = i;
                while (j < text.Length && text[j] == ' ') j++;
                tokens.Add(text.Substring(i, j - i));
                i = j;
                continue;
            }

            int k = i;
            while (k < text.Length && text[k] != ' ' && text[k] != '\n' && text[k] != '\r') k++;
            tokens.Add(text.Substring(i, k - i));
            i = k;
        }
        return tokens;
    }

    public int MeasureRequiredHeight(int width)
    {
        int innerW = Math.Max(1, width - Padding * 2);
        var tokens = TokenizePreservingSpaces(Text ?? string.Empty);

        float spaceW = _font.MeasureString(" ").X * TextScale;
        float lineH  = _font.LineSpacing * TextScale * Math.Max(0.01f, LineSpacingMul);

        float x = 0;
        int lines = 1;

        foreach (var t in tokens)
        {
            if (t == "\n")
            {
                x = 0;
                lines++;
                continue;
            }

            bool isSpaces = t.Length > 0 && t[0] == ' ';
            float w = isSpaces ? spaceW * t.Length : _font.MeasureString((string)t).X * TextScale;

            if (x + w > innerW)
            {
                // new line; drop leading spaces
                x = 0;
                lines++;
                if (isSpaces) continue;
            }

            x += w;
        }

        float total = Padding * 2 + lines * lineH;
        return (int)Math.Ceiling(total);
    }

    private static void DrawWrappedString(SpriteBatch sb, SpriteFont font, string text,
        Rectangle area, Color color, float scale, float lineMul)
    {
        var tokens = TokenizePreservingSpaces(text);
        float spaceW = font.MeasureString(" ").X * scale;
        float x = area.X;
        float y = area.Y;
        float lineH = font.LineSpacing * scale * Math.Max(0.01f, lineMul);

        foreach (var t in tokens)
        {
            if (t == "\n")
            {
                x = area.X;
                y += lineH;
                if (y + lineH > area.Bottom) break;
                continue;
            }

            bool isSpaces = t.Length > 0 && t[0] == ' ';
            float w = isSpaces ? spaceW * t.Length : font.MeasureString(t).X * scale;

            if (x + w > area.Right)
            {
                x = area.X;
                y += lineH;
                if (y + lineH > area.Bottom) break;
                if (isSpaces) continue; // drop leading spaces on new line
            }

            if (!isSpaces)
                sb.DrawString(font, (string)t, new Vector2((int)x, (int)y), color, 0f, Vector2.Zero, 
                    scale, SpriteEffects.None, 0.13f);

            x += w;
        }
    }
}