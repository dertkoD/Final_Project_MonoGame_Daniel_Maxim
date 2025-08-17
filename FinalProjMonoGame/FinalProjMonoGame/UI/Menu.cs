using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace FinalProjMonoGame.UI;

public abstract class Menu : IUpdateable, IDrawable
{
    protected readonly GraphicsDevice Gd;
    protected readonly SpriteFont Font;

    /// <summary>Used by animated transitions (e.g. slide the whole menu vertically).</summary>
    public float SlideOffsetY { get; set; } = 0f;

    // Sprite names (override if needed)
    protected virtual string WindowSpriteName => "Window";
    protected virtual string ButtonSpriteName => "Button";
    protected virtual string SelectorSpriteName => "Selector";

    // Layout defaults (override if needed)
    protected virtual Point WindowSize => new Point(1800, 800);
    protected virtual Point ButtonSize => new Point(WindowSize.X - 1000, 240);

    // Optional title (override to show a title)
    protected virtual string Title => null;
    protected virtual Vector2 TitleOffset => new Vector2(0, -160f);
    protected virtual float TitleScale => 2.2f;
    protected virtual Color TitleColor => Color.White;

    protected readonly List<(Button btn, Vector2 baseCenterOffset)> entries = new();
    protected int selected = 0;
    private KeyboardState prev;

    private SpriteSheetInfo windowSheet;
    private SpriteSheetInfo selectorSheet;
    private Rectangle windowRectBase; // centered around ScreenCenter
    private Rectangle windowRectDraw; // actually drawn (with SlideOffsetY applied)

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

    /// <summary>Create a ready-to-use button with our menu visuals.</summary>
    protected Button CreateButton(string text, Action<Button> onClick)
    {
        var b = new Button(Gd);
        b.SetSprite(ButtonSpriteName, frame: 0, scale: 1f, stretch: true);
        b.SetText(Font, text, Color.White, scale: 1.6f);
        if (onClick != null) b.IsClicked += onClick;
        return b;
    }

    /// <summary>Add a button and tell where it should be (relative to screen center).</summary>
    protected void AddButton(Button b, Vector2 baseCenterOffset)
    {
        entries.Add((b, baseCenterOffset));
    }

    /// <summary>Called when Enter is pressed. Default: invoke the selected button's click handler.</summary>
    protected virtual void OnEnterPressed(Button current)
    {
        current?.IsClicked?.Invoke(current);
    }

    public virtual void Update(GameTime gameTime)
    {
        UpdateLayout();

        var ks = Keyboard.GetState();
        if (Pressed(Keys.Up, ks)) Move(-1);
        if (Pressed(Keys.Down, ks)) Move(+1);
        if (Pressed(Keys.Enter, ks)) OnEnterPressed(entries.Count > 0 ? entries[selected].btn : null);

        for (int i = 0; i < entries.Count; i++)
            entries[i].btn.Update(gameTime);

        prev = ks;
    }

    public virtual void Draw(SpriteBatch sb)
    {
        DrawWindow(sb);
        if (!string.IsNullOrEmpty(Title)) DrawTitle(sb);

        for (int i = 0; i < entries.Count; i++)
            entries[i].btn.Draw(sb);

        if (entries.Count > 0)
            DrawSelector(sb, entries[selected].btn);
    }

    // ---------- layout & drawing helpers ----------

    private void UpdateLayout()
    {
        windowRectDraw = windowRectBase;
        windowRectDraw.Offset(0, (int)SlideOffsetY);

        for (int i = 0; i < entries.Count; i++)
        {
            var center = Game1.ScreenCenter + entries[i].baseCenterOffset + new Vector2(0, SlideOffsetY);
            entries[i].btn.SetCenteredBounds(ButtonSize, center);
        }
    }

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

    private void DrawSelector(SpriteBatch sb, Button target)
    {
        if (selectorSheet?.texture == null || target == null) return;

        int sw = selectorSheet.texture.Width / Math.Max(1, selectorSheet.columns);
        int sh = selectorSheet.texture.Height / Math.Max(1, selectorSheet.rows);
        var src = new Rectangle(0, 0, sw, sh);

        float targetH = Math.Max(1f, target.Bounds.Height * 0.6f);
        float scale = targetH / sh;

        Vector2 selSize = new(sw * scale, sh * scale);
        const float gap = 12f;
        Vector2 selPos = new(
            target.Bounds.Left - gap - selSize.X,
            target.Bounds.Center.Y - selSize.Y / 2f
        );

        sb.Draw(selectorSheet.texture, selPos, src, Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 0.11f);
    }

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

    private void Move(int delta) => selected = (selected + delta + entries.Count) % entries.Count;
    private bool Pressed(Keys k, KeyboardState ks) => ks.IsKeyDown(k) && !prev.IsKeyDown(k);

    private static Rectangle CenteredRect(Point size, Vector2 center) =>
        new((int)(center.X - size.X / 2f), (int)(center.Y - size.Y / 2f), size.X, size.Y);

    /// <summary>Derived classes must add their buttons here via CreateButton + AddButton.</summary>
    protected abstract void BuildContent();
}
