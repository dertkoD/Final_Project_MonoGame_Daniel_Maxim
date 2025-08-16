using FinalProjMonoGame.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace FinalProjMonoGame;

public class Game1 : Game
{
    public static Vector2 ScreenCenter;
    public static Rectangle ScreenBounds;
    public static Point ScreenSize;
    
    public static Game1 Instance { get; private set; }

    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private SpriteManager _spriteManager;
    private AudioManager _audioManager;
    private SpriteFont _quivertFont;

    private KeyboardState _prevKeys;

    public Game1()
    {
        Instance = this;
        
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        
        _graphics.IsFullScreen = true;
        _graphics.PreferredBackBufferWidth = 1920;
        _graphics.PreferredBackBufferHeight = 1080;
        
        ScreenCenter = new Vector2(_graphics.PreferredBackBufferWidth * 0.5f, _graphics.PreferredBackBufferHeight * 0.5f);
        ScreenSize = new Point(_graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight);
        ScreenBounds = new Rectangle(0, 0, ScreenSize.X, ScreenSize.Y);
    }

    protected override void Initialize()
    {
        // TODO: Add your initialization logic here

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _audioManager = new AudioManager(Content);
        _spriteManager = new SpriteManager(Content);
        
        _quivertFont = Content.Load<SpriteFont>("Fonts/Quivert");
        
        // main menu / endgame UI
        SpriteManager.AddSprite("Window", "Sprites/MainMenuBackground");
        SpriteManager.AddSprite("Button", "Sprites/ButtonShield");
        SpriteManager.AddSprite("Selector", "Sprites/Selector");

        // character animations
        SpriteManager.AddSprite("PlayerIdle", "Sprites/Player/PlayerIdle", columns: 8, rows: 1);
        SpriteManager.AddSprite("PlayerHit", "Sprites/Player/PlayerHit", columns: 8, rows: 1);
        SpriteManager.AddSprite("PlayerDefend", "Sprites/Player/PlayerDefend", columns: 8, rows: 1);
        
        // hp ui
        SpriteManager.AddSprite("HP", "Sprites/Player/HP");
        
        // audio content
        AudioManager.AddSong("MainMenuTrack", "Audio/Music/MainMenuTrack");
        AudioManager.AddSong("GameTrack", "Audio/Music/GameTrack");
        AudioManager.AddSoundEffect("PlayerHit", "Audio/SFX/PlayerHit");
        AudioManager.AddSoundEffect("PlayerDefend", "Audio/SFX/PlayerDefend");
        
        // enemies
        SpriteManager.AddSprite("Arrow", "Sprites/Enemy/Arrow");
        SpriteManager.AddSprite("Bomb", "Sprites/Enemy/Bomb");
        SpriteManager.AddSprite("Explosion", "Sprites/Enemy/BombExplosion", columns: 3, rows: 2);
        
        // background
        SpriteManager.AddSprite("WoodsFirst","Sprites/Backgrounds/WOODS - First");
        SpriteManager.AddSprite("WoodsSecond","Sprites/Backgrounds/WOODS - Second");
        SpriteManager.AddSprite("WoodsThird","Sprites/Backgrounds/WOODS - Third");
        SpriteManager.AddSprite("WoodsFourth","Sprites/Backgrounds/WOODS - Fourth");
        SpriteManager.AddSprite("Vines","Sprites/Backgrounds/VINES - Second");
        
        // debug
        SpriteManager.AddSprite("Pixel", "Sprites/pixel");
        
        SceneManager.Add(ParallaxBackground.ForestForMenu());
        
        var menu = new MainMenu(GraphicsDevice, _quivertFont, onStart: StartGame);
        SceneManager.Add(menu);
        
        AudioManager.PlaySong("MainMenuTrack", isLoop: true, volume: 0.7f);
    }

    protected override void Update(GameTime gameTime)
    {
        var ks = Keyboard.GetState();
        
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
            ks.IsKeyDown(Keys.Escape))
            Exit();

        // toggle M for music, N for SFX
        bool Pressed(Keys k) => ks.IsKeyDown(k) && _prevKeys.IsKeyUp(k);
        if (Pressed(Keys.M))
            AudioManager.ToggleMusic();
        if (Pressed(Keys.N))
            AudioManager.ToggleSfx();

        _prevKeys = ks;
        
        SceneManager.Instance.Update(gameTime);
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
    
    private void StartGame()
    {
        SceneManager.SwitchTo(() =>
        {
            SceneManager.Add(new ParallaxBackground(new (string,float,float)[] {
                ("WoodsFourth", 0f, 1f),
                ("WoodsThird",  0f, 1f),
                ("WoodsSecond", 0f, 1f),
                ("WoodsFirst",  0f, 1f),
                ("Vines",       0f, 0.95f)
            }));            
            var player =SceneManager.Create<Player>();
            
            // Расположение двух spawn-точек СРАЗУ за краями экрана
            var leftSpawn  = new Vector2(-120f, ScreenCenter.Y);
            var rightSpawn = new Vector2(ScreenBounds.Right + 120f, ScreenCenter.Y);

            var spawner = SceneManager.Create<EnemySpawner>();
            spawner.Init(player, leftSpawn, rightSpawn);

            var hpUi = new HpUI(player)
            {
                Position = new Vector2(16, 16),
                Scale = 1f,
                Spacing = 6
            };
            SceneManager.Add(hpUi);

            var timer = new TimerUI(_quivertFont)
            {
                Position = new Vector2(ScreenCenter.X, 35),
                Scale = 1.5f,
                AlignCenter = true,
            };
            SceneManager.Add(timer);
        });
        AudioManager.PlaySong("GameTrack", isLoop: true, volume: 0.7f);
    }

    public void TriggerGameOver(Player player, double delaySeconds)
    {
        var delay = new EndGameDelay(
            delaySeconds,
            GraphicsDevice,
            _quivertFont,
            player,
            onRestart: StartGame,
            onMainMenu: () =>
            {
                SceneManager.SwitchTo(() =>
                {
                    var menu = new MainMenu(GraphicsDevice, _quivertFont, onStart: StartGame);
                    SceneManager.Add(menu);
                });
                AudioManager.PlaySong("MainMenuTrack", isLoop: true, volume: 0.7f);
            });
        SceneManager.Add(delay);
    }
}