using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FinalProjMonoGame;

// One-shot explosion animation that removes itself when finished.
// Usage: var fx = SceneManager.Create<ExplosionFx>("Explosion"); fx.PlayOnceAndAutoRemove(12);
public class ExplosionFx : Animation
{
    public ExplosionFx(string animName) : base(animName) { }

    // Start non-looping playback at the given FPS; lifetime handled in Update().
    public void PlayOnceAndAutoRemove(int fps)
    {
        PlayAnimation(inLoop: false, fps: fps);
    }

    // Tick the animation; once the clip ends, unregister from the scene graph.
    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        if (!IsAnimating()) SceneManager.Remove(this);
    }
}