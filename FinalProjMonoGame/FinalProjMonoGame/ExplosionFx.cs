using Microsoft.Xna.Framework;

namespace FinalProjMonoGame;

public class ExplosionFx : Animation
{
    public ExplosionFx(string animName) : base(animName) { }

    public void PlayOnceAndAutoRemove(int fps)
    {
        PlayAnimation(inLoop: false, fps: fps);
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        if (!IsAnimating()) SceneManager.Remove(this);
    }
}