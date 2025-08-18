using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FinalProjMonoGame.UI;

public class MainMenu : Menu
{
    public Action OnStart { get; set; }

    private HelpIcon _icon;
    private HelpPopup _popup;

    // fixed position of the help icon
    private Rectangle _iconBaseBounds;

    public MainMenu(GraphicsDevice gd, SpriteFont font, Action onStart)
        : base(gd, font)
    {
        OnStart = onStart;
    }
    
    // no title for the main menu
    protected override string Title => null;

    protected override void BuildContent()
    {
        // buttons
        var start = CreateButton("Start", _ => OnStart?.Invoke());
        var exit = CreateButton("Exit", _ => Environment.Exit(0));

        // button positioning
        AddButton(start, new Vector2(0, -100));
        AddButton(exit, new Vector2(0, 100));

        // help popup, hidden at start
        _popup = new HelpPopup(Font)
        {
            Visible = false
        };

        // text setup
        _popup.Text =
            "Controls:                                                            In-game pause:\n" +
                "A / Left Arrow Key         Face Left                    P             Toggle Pause\n" +
                "D / Right Arrow Key       Face Right                  Esc          Main Menu (during pause)\n" +
                "Hold Shift                        Defend\n" +
                "E                                      Attack\n" +
                "M                                     Toggle Music\n" +
                "N                                      Toggle SFX\n\n" +
                "Goal:\n" +
                "Survive. Time your defenses and attacks while facing the correct direction.\n" +
                "Bomb deals 2 damage, arrow deals 1 damage.\n" +
                "Attacking protects you from bombs and arrows, but the timing is harder than defending.\n" + 
                "Defending protects you from arrows, but bombs still deal 1 damage.\n" + 
            "Deflecting 5 projectiles grants 1 HP.\n" +
            "Good Luck! :)";

        // defining help icon's base bounds
        _iconBaseBounds = new Rectangle(1380, 470, 120, 120);
        // create the icon and give it its rect
        _icon = new HelpIcon(_popup)
        {
            Bounds = _iconBaseBounds
        };
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);

        // adaptive popup size 
        int pw = (int)(Game1.ScreenBounds.Width * 0.75f);
        int maxH = (int)(Game1.ScreenBounds.Height * 0.7f);

        // scaling down the text until it fits in the maxH
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

        // center position of the popup
        _popup.Bounds = new Rectangle(
            (int)(Game1.ScreenCenter.X - pw / 2f),
            (int)(Game1.ScreenCenter.Y - ph / 2f + SlideOffsetY),
            pw, ph
        );

        // move help icon together with menu sliding
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