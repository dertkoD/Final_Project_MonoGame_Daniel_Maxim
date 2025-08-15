using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FinalProjMonoGame;

public class ExplosionFx : Animation
{
    public ExplosionFx(string animName) : base(animName) { }

    public void PlayOnceAndAutoRemove(int fps)
    {
        PlayAnimation(inLoop: false, fps: fps);
    }
    
    /*public override void Draw(SpriteBatch sb)
    {
        // пока нет активной анимации — ничего не рисуем (убирает «мигание шитом»)
        if (!IsAnimating()) return;
        base.Draw(sb);
    }*/

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        if (!IsAnimating()) SceneManager.Remove(this);
    }
}