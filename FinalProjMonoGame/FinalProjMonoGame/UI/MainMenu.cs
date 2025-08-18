using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FinalProjMonoGame.UI;

public class MainMenu : Menu
{
    public Action OnStart { get; set; }

    private HelpIcon _icon;
    private HelpPopup _popup;

    // База (неподвижные) координаты иконки — от них считаем сдвиг по SlideOffsetY
    private Rectangle _iconBaseBounds;

    public MainMenu(GraphicsDevice gd, SpriteFont font, Action onStart)
        : base(gd, font)
    {
        OnStart = onStart;
    }

    // Без заголовка (как и раньше)
    protected override string Title => null;

    protected override void BuildContent()
    {
        // Кнопки
        var start = CreateButton("Start", _ => OnStart?.Invoke());
        var exit = CreateButton("Exit", _ => Environment.Exit(0));

        AddButton(start, new Vector2(0, -100));
        AddButton(exit, new Vector2(0, 100));

        // Попап помощи
        _popup = new HelpPopup(Font)
        {
            Visible = false
        };

        _popup.Text =
            "Controls:\n" +
            "A / Left Arrow Key         Face Left\n" +
            "D / Right Arrow Key        Face Right\n" +
            "Hold Shift                 Defend\n" +
            "E                          Attack\n" +
            "M                          Toggle Music\n" +
            "N                          Toggle SFX\n\n" +
            "Goal:\n" +
            "Survive. Time your defenses and attacks while facing the correct direction.\n" +
            "Bomb deals 2 damage, arrow deals 1 damage.\n" +
            "Deflecting 5 projectiles grants 1 HP.\n" +
            "Good Luck! :)";

        // Иконка помощи: сохраняем базовые координаты и задаём Bounds
        _iconBaseBounds = new Rectangle(1380, 470, 120, 120);
        _icon = new HelpIcon(_popup)
        {
            Bounds = _iconBaseBounds
        };
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);

        // Адаптивный размер попапа и его позиция с учётом SlideOffsetY
        int pw = (int)(Game1.ScreenBounds.Width * 0.75f);
        int maxH = (int)(Game1.ScreenBounds.Height * 0.7f);

        float scale = _popup.TextScale;
        const float minScale = 0.80f;
        int ph;

        while (true)
        {
            _popup.TextScale = scale;
            ph = _popup.MeasureRequiredHeight(pw);
            if (ph <= maxH || scale <= minScale) break;
            scale -= 0.05f;
        }

        ph = Math.Min(ph, maxH);

        _popup.Bounds = new Rectangle(
            (int)(Game1.ScreenCenter.X - pw / 2f),
            (int)(Game1.ScreenCenter.Y - ph / 2f + SlideOffsetY),
            pw, ph
        );

        // >>> Фикс: двигать иконку вместе с меню <<<
        _icon.Bounds = new Rectangle(
            _iconBaseBounds.X,
            _iconBaseBounds.Y + (int)SlideOffsetY,
            _iconBaseBounds.Width,
            _iconBaseBounds.Height
        );

        _icon.Update(gameTime);
        _popup.Update(gameTime);
    }

    public override void Draw(SpriteBatch sb)
    {
        base.Draw(sb);
        _icon.Draw(sb);
        if (_popup.Visible)
            _popup.Draw(sb);
    }
}