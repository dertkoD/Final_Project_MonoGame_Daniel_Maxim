using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FinalProjMonoGame.UI;

public class TextBlock : UIElement
{
    public Rectangle Bounds;
    
    public SpriteFont Font { get; set; }
    public string Text { get; set; } = string.Empty;
    public Color Color { get; set; } = Color.White;
    public float Scale { get; set; } = 1f;
    public float LineSpacingMul { get; set; } = 1f; 
    
    public bool Shadow { get; set; } = true; 
    public Color ShadowColor { get; set; } = new Color(0,0,0,160);
    
    public enum HAlign { Left, Center, Right }
    public HAlign Align { get; set; } = HAlign.Left;
    
    public TextBlock(SpriteFont font) { Font = font; } 
    
    protected override void OnUpdate(GameTime gameTime) {} 
    protected override void OnDraw(SpriteBatch sb)
    {
        if (Font == null || string.IsNullOrEmpty(Text) || Bounds.Width <= 0 || Bounds.Height <= 0)
            return;

        var lines = Wrap(Text.Replace("\r\n","\n"), Bounds.Width);
        float lineH = Font.LineSpacing * Scale * Math.Max(0.01f, LineSpacingMul);
        float y = Bounds.Y;

        foreach (var line in lines)
        {
            if (y + lineH > Bounds.Bottom) break;

            float lineW = Font.MeasureString(line).X * Scale;
            float x = Align switch
            {
                HAlign.Center => Bounds.X + (Bounds.Width - lineW) * 0.5f,
                HAlign.Right => Bounds.Right - lineW, _ => Bounds.X
            };

            if (Shadow)
                sb.DrawString(Font, line, new Vector2((int)x + 1, (int)y + 1), ShadowColor, 0f, Vector2.Zero, Scale, SpriteEffects.None, 0.13f);

            sb.DrawString(Font, line, new Vector2((int)x, (int)y), Color, 0f, Vector2.Zero, Scale, SpriteEffects.None, 0.14f);
            y += lineH;
        }
    }

    private List<string> Wrap(string text, int maxWidth)
    {
        var result = new List<string>();
        var paragraphs = text.Split('\n');

        foreach (var p in paragraphs)
        {
            if (string.IsNullOrEmpty(p))
            {
                result.Add("");
                continue;
            }
            
            var words = p.Split(' ');
            string current = "";
            foreach (var w in words)
            {
                string test = string.IsNullOrEmpty(current) ? w : current + " " + w;
                float width = Font.MeasureString(test).X * Scale;

                if (width <= maxWidth || string.IsNullOrEmpty(current))
                    current = test;
                else
                {
                    result.Add(current);
                    current = w;
                }
            }
            if (!string.IsNullOrEmpty(current)) result.Add(current);
        }
        return result;
    }
}