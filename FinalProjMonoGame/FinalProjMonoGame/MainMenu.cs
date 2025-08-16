using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace FinalProjMonoGame;

public class MainMenu : IUpdateable, IDrawable
{
    public Action OnStart { get; set; }

    // Уже было — теперь реально используется в лейауте
    public float SlideOffsetY { get; set; } = 0f;

    private const string WindowSprite = "Window";
    private const string ButtonSprite = "Button";
    private const string SelectorSprite = "Selector";

    private readonly Button startBtn;
    private readonly Button exitBtn;
    private readonly Button[] buttons;
    private int selected = 0;
    private KeyboardState prev;

    private readonly SpriteSheetInfo windowSheet;
    private readonly SpriteSheetInfo selectorSheet;

    // Базовые размеры/позиции (без сдвига), чтобы было удобно пересчитывать лейаут
    private readonly Point windowSize = new Point(1800, 800);
    private Rectangle windowRectBase; // центрирован вокруг ScreenCenter (без SlideOffsetY)
    private Rectangle windowRectDraw; // фактический прямоугольник отрисовки (со SlideOffsetY)

    // Базовые центры кнопок (без SlideOffsetY)
    private readonly Vector2 startBtnBaseCenterOffset = new Vector2(0, -100);
    private readonly Vector2 exitBtnBaseCenterOffset  = new Vector2(0,  100);

    // Размеры кнопок
    private readonly Point btnSize;

    public MainMenu(GraphicsDevice gd, SpriteFont font, Action onStart)
    {
        // Можно задавать и через конструктор (для обратной совместимости)...
        this.OnStart = onStart;

        windowSheet   = SpriteManager.GetSprite(WindowSprite);
        selectorSheet = SpriteManager.GetSprite(SelectorSprite);

        // Центрируем окно (базовый, без SlideOffsetY)
        windowRectBase = CenteredRect(windowSize, Game1.ScreenCenter);

        // Кнопки
        startBtn = new Button(gd);
        exitBtn  = new Button(gd);

        var btnWidth  = windowSize.X - 1000;
        var btnHeight = 240;
        btnSize = new Point(btnWidth, btnHeight);

        // Статические визуальные настройки спрайтов/текста задаём 1 раз
        startBtn.SetSprite(ButtonSprite, frame: 0, scale: 1f, stretch: true);
        exitBtn.SetSprite (ButtonSprite, frame: 0, scale: 1f, stretch: true);

        float textScale = 1.6f;
        startBtn.SetText(font, "Start", Color.White, textScale);
        exitBtn.SetText (font, "Exit",  Color.White, textScale);

        // Клики
        startBtn.IsClicked += _ => OnStart?.Invoke();             // <— вызываем публичный колбэк
        exitBtn.IsClicked  += _ => Environment.Exit(0);

        buttons = new[] { startBtn, exitBtn };

        // Первичный расчёт лейаута
        UpdateLayout();
    }

    public void Update(GameTime gameTime)
    {
        // Пересчитываем лейаут каждый кадр — SlideOffsetY может меняться снаружи (анимация)
        UpdateLayout();

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

    // ---------- layout & helpers ----------

    private void UpdateLayout()
    {
        // 1) Окно: берём базовый прямоугольник и добавляем вертикальный сдвиг
        windowRectDraw = windowRectBase;
        windowRectDraw.Offset(0, (int)SlideOffsetY);

        // 2) Кнопки: центры относительно ScreenCenter + базовый оффсет + SlideOffsetY
        var startCenter = Game1.ScreenCenter + startBtnBaseCenterOffset + new Vector2(0, SlideOffsetY);
        var exitCenter  = Game1.ScreenCenter + exitBtnBaseCenterOffset  + new Vector2(0, SlideOffsetY);

        startBtn.SetCenteredBounds(btnSize, startCenter);
        exitBtn.SetCenteredBounds (btnSize, exitCenter);
    }

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
            destinationRectangle: windowRectDraw, // <— используем смещённый прямоугольник
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