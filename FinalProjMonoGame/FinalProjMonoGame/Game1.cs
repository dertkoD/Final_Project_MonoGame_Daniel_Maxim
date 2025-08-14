using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace FinalProjMonoGame;

public class Game1 : Game
{
    public static Vector2 ScreenCenter;
    public static Rectangle ScreenBounds;
    public static Point ScreenSize;

    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private SpriteManager _spriteManager;
    private AudioManager _audioManager;
    private SpriteFont _quivertFont;

    public Game1()
    {
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
        
        // main menu UI
        SpriteManager.AddSprite("Window", "Sprites/MainMenuBackground");
        SpriteManager.AddSprite("Button", "Sprites/ButtonShield");
        SpriteManager.AddSprite("Selector", "Sprites/Selector");

        // character animations
        SpriteManager.AddSprite("PlayerIdle", "Sprites/Player/PlayerIdle", columns: 8, rows: 1);
        SpriteManager.AddSprite("PlayerHit", "Sprites/Player/PlayerHit", columns: 9, rows: 1);
        SpriteManager.AddSprite("PlayerDefend", "Sprites/Player/PlayerDefend", columns: 8, rows: 1);
        
        // enemies
        SpriteManager.AddSprite("Arrow", "Sprites/Enemy/Arrow");
        SpriteManager.AddSprite("Bomb", "Sprites/Enemy/Bomb");
        SpriteManager.AddSprite("Explosion", "Sprites/Enemy/BombExplosion", columns: 3, rows: 2);
        
        // debug
        SpriteManager.AddSprite("Pixel", "Sprites/pixel");
        
        var menu = new MainMenu(GraphicsDevice, _quivertFont, onStart: StartGame);
        SceneManager.Add(menu);
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
            Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

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
            var player =SceneManager.Create<Player>();
            
            // Расположение двух spawn-точек СРАЗУ за краями экрана
            var leftSpawn  = new Vector2(-120f, ScreenCenter.Y);
            var rightSpawn = new Vector2(ScreenBounds.Right + 120f, ScreenCenter.Y);

            var spawner = SceneManager.Create<EnemySpawner>();
            spawner.Init(player, leftSpawn, rightSpawn);
        });
    }
}