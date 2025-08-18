using FinalProjMonoGame.PlayerClasses;
using FinalProjMonoGame.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FinalProjMonoGame;

public class EndGameDelay : IUpdateable, IDrawable
{
    private double remaining;
    private bool shown;

    private readonly GraphicsDevice gd;
    private readonly SpriteFont font;
    private readonly System.Action onRestart;
    private readonly System.Action onMainMenu;
    private readonly Player player;

    public EndGameDelay(double delaySeconds, GraphicsDevice gd, SpriteFont font,
        Player player, System.Action onRestart, System.Action onMainMenu)
    {
        remaining = System.Math.Max(0, delaySeconds);
        this.gd = gd;
        this.font = font;
        this.player = player;
        this.onRestart = onRestart;
        this.onMainMenu = onMainMenu;
        
        player?.SetControlsEnabled(false);
    }

    public void Update(GameTime gameTime)
    {
        if (shown) return;

        remaining -= gameTime.ElapsedGameTime.TotalSeconds;
        if (remaining <= 0)
        {
            shown = true;
            SceneManager.SwitchTo(() =>
            {
                var screen = new EndGameScreen(gd, font, onRestart, onMainMenu);
                SceneManager.Add(screen);
            });
        }
    }
    
    public void Draw(SpriteBatch _spriteBatch) {}
}