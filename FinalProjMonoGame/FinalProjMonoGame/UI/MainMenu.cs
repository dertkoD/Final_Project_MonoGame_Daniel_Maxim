using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace FinalProjMonoGame.UI;

public class MainMenu : Menu
{
    public Action OnStart { get; set; }

    public MainMenu(GraphicsDevice gd, SpriteFont font, Action onStart)
        : base(gd, font)
    {
        OnStart = onStart; // can be reassigned later (e.g., by Game1 / transitions)
    }

    protected override string Title => null; // No title for main menu (keep your style)

    protected override void BuildContent()
    {
        var start = CreateButton("Start", _ => OnStart?.Invoke());
        var exit  = CreateButton("Exit",  _ => Environment.Exit(0));

        // Same vertical spacing as before
        AddButton(start, new Vector2(0, -100));
        AddButton(exit,  new Vector2(0,  100));
    }
}