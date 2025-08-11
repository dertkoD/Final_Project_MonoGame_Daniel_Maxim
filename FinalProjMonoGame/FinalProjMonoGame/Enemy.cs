using System;
using Microsoft.Xna.Framework;

namespace FinalProjMonoGame;

public class Enemy : Animation
{
    public Player player = null;
    public Collider collider;
    
    public Enemy() : base("NewForm")
    {
        position = Game1.ScreenCenter + new Vector2(0, -500);
        scale = new Vector2(0.2f, 0.2f);
        PlayAnimation();

        collider = SceneManager.Create<Collider>();
        collider.isTrigger = true;
    }

    
    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        
        collider.rect = rect;
        
        if (player != null)
        {
            if (collider.Intersects(player.collider))
                collider.Notify(this);
        }
    }

}