using FinalProjMonoGame.UI;
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

    private KeyboardState _prevKeys;

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
        
        _quivertFont = Content.Load<SpriteFont>("Fonts/Quivert");
        
        // main menu UI
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
            var player = SceneManager.Create<Player>();
            //player.SetDeflectHealThreshold(3);

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
}