using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace FinalProjMonoGame.UI;

public class EndGameScreen : UIElement
{
    private const string WindowSprite = "Window";
    private const string ButtonSprite = "Button";
    private readonly GraphicsDevice gd;
    private readonly SpriteFont font;

    private readonly Text titleText;
    private readonly Button restartButton;
    private readonly Button menuButton;
    private readonly Button[] buttons;
    
    private int selected = 0;
    private KeyboardState prev;

    private readonly SpriteSheetInfo windowSheet;
    private Rectangle windowRect;

    private readonly System.Action onRestart;
    private readonly System.Action onMainMenu;

    public EndGameScreen(GraphicsDevice gd, SpriteFont font, System.Action onRestart, System.Action onMainMenu)
    {
        this.gd = gd;
        this.font = font;
        this.onRestart = onRestart;
        this.onMainMenu = onMainMenu;

        windowSheet = SpriteManager.GetSprite(WindowSprite);

        // window prefs
        var winSize = new Point(1800, 800);
        windowRect = CenteredRect(winSize, Game1.ScreenCenter);

        // title prefs
        titleText = new Text(font)
        {
            text = "You Lose",
            position = Game1.ScreenCenter + new Vector2(0, -160),
            scale = new Vector2(2.2f, 2.2f)
        };

        // buttons
        restartButton = new Button(gd);
        menuButton = new Button(gd);

        var buttonWidth = windowRect.Width - 1000;
        var buttonHeight = 240;

        restartButton.SetCenteredBounds(new Point(buttonWidth, buttonHeight),
            Game1.ScreenCenter + new Vector2(0, -50));
        menuButton.SetCenteredBounds(new Point(buttonWidth, buttonHeight),
            Game1.ScreenCenter + new Vector2(0, 100));

        restartButton.SetSprite(ButtonSprite, frame: 0, scale: 1f, stretch: true);
        menuButton.SetSprite(ButtonSprite, frame: 0, scale: 1f, stretch: true);

        float textScale = 1.6f;
        restartButton.SetText(font, "Restart", Color.White, textScale);
        menuButton.SetText(font, "Main Menu", Color.White, textScale);

        restartButton.IsClicked += _ => onRestart?.Invoke();
        menuButton.IsClicked += _ => onMainMenu?.Invoke();

        buttons = new[] { restartButton, menuButton };
    }

    protected override void OnUpdate(GameTime gameTime)
    {
        foreach (var button in buttons) button.Update(gameTime);
    }

    protected override void OnDraw(SpriteBatch spriteBatch)
    {
        DrawWindow(spriteBatch);
        titleText.Update(default);
        titleText.Draw(spriteBatch);
        foreach (var button in buttons) button.Draw(spriteBatch);
    }

    private void Move(int delta) => selected = (selected + delta + buttons.Length) % buttons.Length;
    private bool Pressed(Keys key, KeyboardState ks) => ks.IsKeyDown(key) && !prev.IsKeyDown(key);

    private static Rectangle CenteredRect(Point size, Vector2 center) =>
        new Rectangle((int)(center.X - size.X / 2f), (int)(center.Y - size.Y / 2f), size.X, size.Y);

    private void DrawWindow(SpriteBatch sb)
    {
        if (windowSheet?.texture == null) return;

        int fw = windowSheet.texture.Width / System.Math.Max(1, windowSheet.columns);
        int fh = windowSheet.texture.Height / System.Math.Max(1, windowSheet.rows);
        var src = new Rectangle(0, 0, fw, fh);
        
        sb.Draw(windowSheet.texture,
            destinationRectangle: windowRect,
            sourceRectangle: src,
            color: Color.White,
            rotation: 0f,
            origin: Vector2.Zero,
            effects: SpriteEffects.None,
            layerDepth: 0.1f
        );
    }
}