using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace FinalProjMonoGame;

public class MainMenu : IUpdateable, IDrawable
{
    private const string WindowSprite = "Window";
    private const string ButtonSprite = "Button";
    private const string SelectorSprite = "Selector";

    private readonly Button startBtn;
    private readonly Button exitBtn;
    private readonly Button[] buttons;
    private int selected = 0;
    private KeyboardState prev;

    private readonly Action onStart;

    private readonly SpriteSheetInfo windowSheet;
    private readonly SpriteSheetInfo selectorSheet;
    private Rectangle windowRect;

    public MainMenu(GraphicsDevice gd, SpriteFont font, Action onStart)
    {
        this.onStart = onStart;

        windowSheet = SpriteManager.GetSprite(WindowSprite);
        selectorSheet = SpriteManager.GetSprite(SelectorSprite);

        // Размер окна 
        var winSize = new Point(1800, 800);
        windowRect = CenteredRect(winSize, Game1.ScreenCenter);

        // Кнопки
        startBtn = new Button(gd);
        exitBtn  = new Button(gd);
        
        var btnWidth  = windowRect.Width - 1000; 
        var btnHeight = 240;                    

        // Позиции по центру
        startBtn.SetCenteredBounds(new Point(btnWidth, btnHeight), Game1.ScreenCenter + new Vector2(0, -100));
        exitBtn.SetCenteredBounds (new Point(btnWidth, btnHeight), Game1.ScreenCenter + new Vector2(0,  100));

        startBtn.SetSprite(ButtonSprite, frame: 0, scale: 1f, stretch: true);
        exitBtn.SetSprite (ButtonSprite, frame: 0, scale: 1f, stretch: true);

        // Текст
        float textScale = 1.6f;
        startBtn.SetText(font, "Start", Color.White, textScale);
        exitBtn.SetText (font, "Exit",  Color.White, textScale);

        // Клики
        startBtn.IsClicked += _ => onStart?.Invoke();
        exitBtn.IsClicked  += _ => Environment.Exit(0);

        buttons = new[] { startBtn, exitBtn };
    }

    public void Update(GameTime gameTime)
    {
        var ks = Keyboard.GetState();

        if (Pressed(Keys.Up, ks))   Move(-1);
        if (Pressed(Keys.Down, ks)) Move(+1);
        if (Pressed(Keys.Enter, ks)) buttons[selected].IsClicked?.Invoke(buttons[selected]);

        foreach (var b in buttons) b.Update(gameTime);

        prev = ks;
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        DrawWindow(spriteBatch);
        foreach (var b in buttons) b.Draw(spriteBatch);
        DrawSelector(spriteBatch, buttons[selected]);
    }

    // ---------- helpers ----------

    private void Move(int delta) => selected = (selected + delta + buttons.Length) % buttons.Length;

    private bool Pressed(Keys key, KeyboardState ks) => ks.IsKeyDown(key) && !prev.IsKeyDown(key);

    private static Rectangle CenteredRect(Point size, Vector2 center) =>
        new Rectangle((int)(center.X - size.X / 2f), (int)(center.Y - size.Y / 2f), size.X, size.Y);

    private void DrawWindow(SpriteBatch sb)
    {
        if (windowSheet?.texture == null) return;

        int fw = windowSheet.texture.Width  / Math.Max(1, windowSheet.columns);
        int fh = windowSheet.texture.Height / Math.Max(1, windowSheet.rows);
        var src = new Rectangle(0, 0, fw, fh);

        sb.Draw(windowSheet.texture,
            destinationRectangle: windowRect,
            sourceRectangle: src,
            color: Color.White,
            rotation: 0f,
            origin: Vector2.Zero,
            effects: SpriteEffects.None,
            layerDepth: 0.1f);
    }

    private void DrawSelector(SpriteBatch sb, Button target)
    {
        if (selectorSheet?.texture == null) return;

        int sw = selectorSheet.texture.Width  / Math.Max(1, selectorSheet.columns);
        int sh = selectorSheet.texture.Height / Math.Max(1, selectorSheet.rows);
        var src = new Rectangle(0, 0, sw, sh);

        float targetH = Math.Max(1f, target.Bounds.Height * 0.6f);
        float scale = targetH / sh;

        Vector2 selSize = new Vector2(sw * scale, sh * scale);
        const float gap = 12f;

        Vector2 selPos = new Vector2(
            target.Bounds.Left - gap - selSize.X,
            target.Bounds.Center.Y - selSize.Y / 2f
        );

        sb.Draw(selectorSheet.texture, selPos, src, Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 0.11f);
    }
}