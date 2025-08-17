using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FinalProjMonoGame.UI;

public class EndGameScreen : Menu
{
    private readonly Action onRestart;
    private readonly Action onMainMenu;

    public EndGameScreen(GraphicsDevice gd, SpriteFont font, Action onRestart, Action onMainMenu)
        : base(gd, font)
    {
        this.onRestart = onRestart;
        this.onMainMenu = onMainMenu;
    }

    protected override string Title => "You Lose";

    protected override void BuildContent()
    {
        var restart = CreateButton("Restart",   _ => onRestart?.Invoke());
        var menu    = CreateButton("Main Menu", _ => onMainMenu?.Invoke());

        AddButton(restart, new Vector2(0, -50));
        AddButton(menu,    new Vector2(0, 100));
    }
}