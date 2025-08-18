using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace FinalProjMonoGame.UI;

// base class for menus
// common widow, title, button handling and selection logic
public abstract class Menu : IUpdateable, IDrawable
{
    protected readonly GraphicsDevice Gd;
    protected readonly SpriteFont Font;

    /// used by animated transitions
    public float SlideOffsetY { get; set; } = 0f;

    // sprite names
    protected virtual string WindowSpriteName => "Window";
    protected virtual string ButtonSpriteName => "Button";
    protected virtual string SelectorSpriteName => "Selector";

    // layout defaults
    protected virtual Point WindowSize => new Point(1800, 800);
    protected virtual Point ButtonSize => new Point(WindowSize.X - 1000, 240);

    // optional title
    protected virtual string Title => null;
    protected virtual Vector2 TitleOffset => new Vector2(125f, -125f);
    protected virtual float TitleScale => 2.2f;
    protected virtual Color TitleColor => Color.Red;

    protected readonly List<(Button btn, Vector2 baseCenterOffset)> entries = new();
    
    // hovered/selected button index
    protected int selected = -1;
    
    
    private SpriteSheetInfo windowSheet;
    private SpriteSheetInfo selectorSheet;
    
    private Rectangle windowRectBase; // centered around ScreenCenter
    protected Rectangle windowRectDraw; // actually drawn (with SlideOffsetY applied)

    protected Menu(GraphicsDevice gd, SpriteFont font)
    {
        Gd = gd;
        Font = font;

        windowSheet = SpriteManager.GetSprite(WindowSpriteName);
        selectorSheet = SpriteManager.GetSprite(SelectorSpriteName);

        windowRectBase = CenteredRect(WindowSize, Game1.ScreenCenter);
        
        BuildContent();
        UpdateLayout();
    }

    // create a ready-to-use button with text and click handling
    protected Button CreateButton(string text, Action<Button> onClick)
    {
        var b = new Button(Gd);
        b.SetSprite(ButtonSpriteName, frame: 0, scale: 1f, stretch: true);
        b.SetText(Font, text, Color.White, scale: 1.6f);
        if (onClick != null) b.IsClicked += onClick;
        return b;
    }

    // add a button and tell where it should be (relative to screen center)
    protected void AddButton(Button b, Vector2 baseCenterOffset)
    {
        entries.Add((b, baseCenterOffset));
    }
    
    public virtual void Update(GameTime gameTime)
    {
        UpdateLayout();

        // button updates
        for (int i = 0; i < entries.Count; i++)
            entries[i].btn.Update(gameTime);

        // mouse position tracking
        var ms = Mouse.GetState();
        selected = -1;
        for (int i = 0; i < entries.Count; i++)
        {
            if (entries[i].btn.Bounds.Contains(ms.Position))
            {
                selected = i;
                break;
            }
        }

        if (selected >= entries.Count) selected = -1;
    }

    public virtual void Draw(SpriteBatch sb)
    {
        DrawWindow(sb);
        if (!string.IsNullOrEmpty(Title)) DrawTitle(sb);

        for (int i = 0; i < entries.Count; i++)
            entries[i].btn.Draw(sb);

        if (selected >= 0 && selected < entries.Count)
            DrawSelector(sb, entries[selected].btn);
    }

    // layout & drawing helpers

    private void UpdateLayout()
    {
        windowRectDraw = windowRectBase;
        windowRectDraw.Offset(0, (int)SlideOffsetY);

        // position each button centered at its offset
        for (int i = 0; i < entries.Count; i++)
        {
            var center = Game1.ScreenCenter + entries[i].baseCenterOffset + new Vector2(0, SlideOffsetY);
            entries[i].btn.SetCenteredBounds(ButtonSize, center);
        }
    }

    // draws the background window rect with window sprite
    private void DrawWindow(SpriteBatch sb)
    {
        if (windowSheet?.texture == null) return;

        int fw = windowSheet.texture.Width / Math.Max(1, windowSheet.columns);
        int fh = windowSheet.texture.Height / Math.Max(1, windowSheet.rows);
        var src = new Rectangle(0, 0, fw, fh);

        sb.Draw(
            windowSheet.texture,
            destinationRectangle: windowRectDraw,
            sourceRectangle: src,
            color: Color.White,
            rotation: 0f,
            origin: Vector2.Zero,
            effects: SpriteEffects.None,
            layerDepth: 0.1f
        );
    }

    // draws a selector arrow to the currently hovered button
    private void DrawSelector(SpriteBatch sb, Button target)
    {
        if (selectorSheet?.texture == null || target == null) return;

        int sw = selectorSheet.texture.Width / Math.Max(1, selectorSheet.columns);
        int sh = selectorSheet.texture.Height / Math.Max(1, selectorSheet.rows);
        var src = new Rectangle(0, 0, sw, sh);

        // scale selector to 60% of button height
        float targetH = Math.Max(1f, target.Bounds.Height * 0.6f);
        float scale = targetH / sh;

        Vector2 selSize = new(sw * scale, sh * scale);
        const float gap = 12f;
        Vector2 selPos = new(
            target.Bounds.Left - gap - selSize.X,
            target.Bounds.Center.Y - selSize.Y / 2f
        );

        sb.Draw(selectorSheet.texture, selPos, src, Color.White, 0f, Vector2.Zero, 
            scale, SpriteEffects.None, 0.11f);
    }

    // draws the optional menu title at the top of the menu window
    private void DrawTitle(SpriteBatch sb)
    {
        Vector2 size = Font.MeasureString(Title) * TitleScale;
        Vector2 center = Game1.ScreenCenter + TitleOffset + new Vector2(0, SlideOffsetY);

        // slight shadow + main text
        sb.DrawString(Font, Title, center + new Vector2(1, 1), Color.Black * 0.6f,
            0f, size * 0.5f, TitleScale, SpriteEffects.None, 0.10005f);
        sb.DrawString(Font, Title, center, TitleColor,
            0f, size * 0.5f, TitleScale, SpriteEffects.None, 0.1001f);
    }
    // utility to build a rectangle centered at a point
    protected static Rectangle CenteredRect(Point size, Vector2 center) =>
        new((int)(center.X - size.X / 2f), (int)(center.Y - size.Y / 2f), size.X, size.Y);

    // derived classes must add their buttons here via CreateButton + AddButton
    protected abstract void BuildContent();
}
