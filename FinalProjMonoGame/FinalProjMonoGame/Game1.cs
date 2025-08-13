using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace FinalProjMonoGame;

public class Game1 : Game
{
    public static Vector2 ScreenCenter;

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
        
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _audioManager = new AudioManager(Content);
        _spriteManager = new SpriteManager(Content);

        SpriteFont _quivertFont = Content.Load<SpriteFont>("Fonts/Quivert");
        SpriteManager.AddSprite("Window", "Sprites/MainMenuBackground");
        SpriteManager.AddSprite("Button", "Sprites/ButtonShield");
        SpriteManager.AddSprite("Selector", "Sprites/Selector");

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
            // TODO: здесь регистрируешь игровые объекты/сцены
            // Например: SceneManager.Add(new GameplayRoot(...));
        });
    }
}