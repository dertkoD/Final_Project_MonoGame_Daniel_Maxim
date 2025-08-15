using Microsoft.Xna.Framework;

namespace FinalProjMonoGame;

public class Arrow : Enemy
{
    public Arrow() : base("Arrow")
    {
        scale = new Vector2(0.4f, 0.4f); 
        Damage = 1;
    }

    protected override void OnUpdate(GameTime gameTime)
    {
        FaceVelocity();
    }
}