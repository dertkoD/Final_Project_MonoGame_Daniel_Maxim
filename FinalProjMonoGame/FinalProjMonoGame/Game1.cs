using System;
using FinalProjMonoGame.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using FinalProjMonoGame.PlayerClasses;

namespace FinalProjMonoGame;

public class Game1 : Game
{
    // Screen refs
    public static Vector2   ScreenCenter;
    public static Rectangle ScreenBounds;
    public static Point     ScreenSize;

    public static Game1 Instance { get; private set; }

    // GFX & managers
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private SpriteManager _spriteManager;
    private AudioManager  _audioManager;
    private SpriteFont    _quivertFont;

    // Global input/pause
    private KeyboardState _prevKeys;
    private bool _inGameplay = false;
    private bool _isPaused   = false;

    public Game1()
    {
        Instance = this;

        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;

        _graphics.IsFullScreen = true;
        _graphics.PreferredBackBufferWidth  = 1920;
        _graphics.PreferredBackBufferHeight = 1080;

        ScreenCenter = new Vector2(_graphics.PreferredBackBufferWidth * 0.5f,
                                   _graphics.PreferredBackBufferHeight * 0.5f);
        ScreenSize   = new Point(_graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight);
        ScreenBounds = new Rectangle(0, 0, ScreenSize.X, ScreenSize.Y);
    }

    protected override void Initialize()
    {
        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch   = new SpriteBatch(GraphicsDevice);
        _audioManager  = new AudioManager(Content);
        _spriteManager = new SpriteManager(Content);

        _quivertFont = Content.Load<SpriteFont>("Fonts/Quivert");

        // Assets 

        // UI
        SpriteManager.AddSprite("Window",   "Sprites/MainMenuBackground");
        SpriteManager.AddSprite("Button",   "Sprites/ButtonShield");
        SpriteManager.AddSprite("Selector", "Sprites/Selector");
        SpriteManager.AddSprite("HelpIcon", "Sprites/HelpIcon");

        // Player anims
        SpriteManager.AddSprite("PlayerIdle",    "Sprites/Player/PlayerIdle",    columns: 8, rows: 1);
        SpriteManager.AddSprite("PlayerHit",     "Sprites/Player/PlayerHit",     columns: 8, rows: 1);
        SpriteManager.AddSprite("PlayerDefend",  "Sprites/Player/PlayerDefend",  columns: 8, rows: 1);
        SpriteManager.AddSprite("TakingDamage",  "Sprites/Player/TakingDamage",  columns: 3, rows: 1);
        SpriteManager.AddSprite("Death",         "Sprites/Player/DeathAnim",     columns: 10, rows: 1);
        SpriteManager.AddSprite("ShieldBlock",   "Sprites/Player/BlockShield",   columns: 4, rows: 1);

        // HP UI
        SpriteManager.AddSprite("HP", "Sprites/Player/HP");

        // Enemies
        SpriteManager.AddSprite("Arrow",     "Sprites/Enemy/Arrow");
        SpriteManager.AddSprite("Bomb",      "Sprites/Enemy/Bomb");
        SpriteManager.AddSprite("Explosion", "Sprites/Enemy/BombExplosion", columns: 3, rows: 2);

        // Background/ground
        SpriteManager.AddSprite("WoodsFirst",   "Sprites/Backgrounds/WOODS - First");
        SpriteManager.AddSprite("WoodsSecond",  "Sprites/Backgrounds/WOODS - Second");
        SpriteManager.AddSprite("WoodsThird",   "Sprites/Backgrounds/WOODS - Third");
        SpriteManager.AddSprite("WoodsFourth",  "Sprites/Backgrounds/WOODS - Fourth");
        SpriteManager.AddSprite("Vines",        "Sprites/Backgrounds/VINES - Second");
        SpriteManager.AddSprite("Earth",        "Sprites/Backgrounds/Earth");
        SpriteManager.AddSprite("GroundGrass",  "Sprites/Backgrounds/GroundGrass");

        // Debug
        SpriteManager.AddSprite("Pixel", "Sprites/pixel");

        // Audio
        AudioManager.AddSong("MainMenuTrack", "Audio/Music/MainMenuTrack");
        AudioManager.AddSong("GameTrack",     "Audio/Music/GameTrack");
        AudioManager.AddSoundEffect("PlayerHit",    "Audio/SFX/PlayerHit");
        AudioManager.AddSoundEffect("PlayerDefend", "Audio/SFX/PlayerDefend");
        AudioManager.AddSoundEffect("PlayerHurt",   "Audio/SFX/PlayerHurt");
        AudioManager.AddSoundEffect("PlayerDeath",  "Audio/SFX/PlayerDeath");
        AudioManager.AddSoundEffect("Explosion",    "Audio/SFX/Explosion");

        // ---- initial screen: main menu
        SwitchToMainMenu(animateEnter: true);
    }

    protected override void Update(GameTime gameTime)
    {
        var ks = Keyboard.GetState();

        bool Pressed(Keys k) => ks.IsKeyDown(k) && _prevKeys.IsKeyUp(k);

        // Audio toggles
        if (Pressed(Keys.M)) AudioManager.ToggleMusic();
        if (Pressed(Keys.N)) AudioManager.ToggleSfx();

        // Pause/unpause in gameplay
        if (_inGameplay && Pressed(Keys.P)) _isPaused = !_isPaused;

        // In gameplay and paused: Esc => go to main menu
        if (_inGameplay && _isPaused && Pressed(Keys.Escape))
        {
            GoToMainMenu();
            _prevKeys = ks;
            base.Update(gameTime);
            return;
        }

        if (!_isPaused)
            SceneManager.Instance.Update(gameTime);

        _prevKeys = ks;
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        _spriteBatch.Begin();
        SceneManager.Instance.Draw(_spriteBatch);
        _spriteBatch.End();

        base.Draw(gameTime);
    }

    // Scene helpers

    // Build main menu scene (parallax + ground + menu + transition to StartGame)
    private MainMenu BuildMainMenuScene()
    {
        var parallax = ParallaxBackground.ForestForMenu();
        SceneManager.Add(parallax);

        var groundTrans = new GroundLayer("GroundGrass", "Earth",
            groundY: (int)(ScreenSize.Y * 0.79f),
            tileScale: 6,
            scrollSpeed: 0f,
            overlapPx: 15);
        groundTrans.SetYOffset(ScreenSize.Y);
        SceneManager.Add(groundTrans);

        var menu = new MainMenu(GraphicsDevice, _quivertFont, onStart: null);
        SceneManager.Add(menu);

        var transition = new MenuTransition(
            parallax,
            groundTrans,
            setMenuSlideOffsetY: y => menu.SlideOffsetY = y,   // OK: локальная переменная
            onComplete: StartGame);
        SceneManager.Add(transition);

        menu.OnStart = () => transition.Begin();
        return menu;
    }

    // Slide any menu in from top (overlay-only; does not touch parallax/ground)
    private void SlideInMenuLike(Action<float> setOffsetY, float duration = 0.6f)
    {
        var overlayTrans = new MenuTransition(
            parallax: null,
            ground:   null,
            setMenuSlideOffsetY: setOffsetY,
            onComplete: null);
        SceneManager.Add(overlayTrans);
        overlayTrans.Begin(Config.MenuEnter(fromTop: true, duration: duration));
    }

    // Switch to main menu scene (optionally with slide-in)
    private void SwitchToMainMenu(bool animateEnter)
    {
        _inGameplay = false;
        _isPaused   = false;

        SceneManager.SwitchTo(() =>
        {
            var menu = BuildMainMenuScene();
            if (animateEnter)
                SlideInMenuLike(y => menu.SlideOffsetY = y, 0.6f);
        });

        AudioManager.PlaySong("MainMenuTrack", isLoop: true, volume: 0.7f);
    }

    // Public short-hands
    private void GoToMainMenu() => SwitchToMainMenu(animateEnter: true);

    private void StartGame()
    {
        _inGameplay = true;
        _isPaused   = false;

        SceneManager.SwitchTo(() =>
        {
            // Background (you can swap to ForestForGame() if нужно движение)
            var parallax = ParallaxBackground.ForestForGame();
            SceneManager.Add(parallax);

            var ground = new GroundLayer("GroundGrass", "Earth",
                groundY: (int)(ScreenSize.Y * 0.79f),
                tileScale: 6,
                scrollSpeed: 0f,
                overlapPx: 15);
            SceneManager.Add(ground);

            // Player
            var player = SceneManager.Create<Player>();

            // Enemy spawner
            var leftSpawn  = new Vector2(-120f, ScreenCenter.Y);
            var rightSpawn = new Vector2(ScreenBounds.Right + 120f, ScreenCenter.Y);
            var spawner = SceneManager.Create<EnemySpawner>();
            spawner.Init(player, leftSpawn, rightSpawn);

            // UI: HP + Timer
            var hpUi = new HpUI(player)
            {
                Position = new Vector2(16, 16),
                Scale    = 1f,
                Spacing  = 6
            };
            SceneManager.Add(hpUi);

            var timer = new TimerUI(_quivertFont)
            {
                Position   = new Vector2(ScreenCenter.X, 35),
                Scale      = 1.5f,
                AlignCenter = true
            };
            timer.Reset();
            SceneManager.Add(timer);
        });

        AudioManager.PlaySong("GameTrack", isLoop: true, volume: 0.7f);
    }

    // Delay then show end-game overlay; reuses SwitchToMainMenu for "Main Menu"
    public void TriggerGameOver(Player player, double delaySeconds)
    {
        SceneManager.Add(new OneShotTimer((float)delaySeconds, () =>
        {
            var endMenu = new EndGameScreen(
                GraphicsDevice, _quivertFont,
                onRestart:  StartGame,
                onMainMenu: () => SwitchToMainMenu(true)
            );
            SceneManager.Add(endMenu);

            // Slide overlay in (no parallax/ground motion here)
            SlideInMenuLike(y => endMenu.SlideOffsetY = y, 0.6f);

            AudioManager.PlaySong("MainMenuTrack", isLoop: true, volume: 0.7f);
        }));
    }
}