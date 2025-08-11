using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FinalProjMonoGame;

public class Animation : Sprite
{
    private SpriteSheet spriteSheet;
    
    private int columns;
    private int rows;

    private int index_x = 0;
    private int index_y = 0;
    
    private double frameTimer = 0;
    private bool animating = false;
    private int fps;
    private bool inLoop;
    
    public Animation(string animationName) : base(animationName)
    {
        ChangeAnimation(animationName);
    }
    
    public void ChangeAnimation(string animationName)
    {
        spriteSheet = new SpriteSheet(SpriteManager.GetSprite(animationName));

        texture = spriteSheet.SpriteSheetInfo.texture; 
        columns = spriteSheet.SpriteSheetInfo.columns;
        rows = spriteSheet.SpriteSheetInfo.rows;
    }
    
    protected override void SetOrigin(OriginPosition originPosition)
    {
        switch (originPosition)
        {
            case OriginPosition.TopLeft:
                origin = Vector2.Zero;
                break;
                
            case OriginPosition.Center:
                origin = new Vector2(sourceRectangle.Value.Size.X * 0.5f, sourceRectangle.Value.Size.Y * 0.5f);
                break;
        }
    }
    
    public void PlayAnimation(bool inLoop = true, int fps = 60)
    {
        this.fps = fps;
        this.inLoop = inLoop;
        
        ResetAnimation();
        animating = true;
    }

    public bool IsAnimating()
    {
        return animating;
    }

    public double GetTimeRemaining(bool normalized = true)
    {
        int totalFrames = columns + rows;
        double deltaFrame = 1.0 / fps;
        double totalTime = totalFrames * deltaFrame;

        float remainingTime = MathHelper.Clamp((float)(totalTime - frameTimer), 0.0f, (float)totalTime);
        
        return (normalized)? remainingTime / totalTime : remainingTime;
    }

    public void PauseAnimation()
    {
        animating = false;
    }

    public void ResumeAnimation()
    {
        animating = true;
    }

    public void StopAnimation()
    {
        PauseAnimation();
        ResetAnimation();
    }

    public void ResetAnimation()
    {
        frameTimer = 0;
        index_x = 0;
        index_y = 0;
    }

    bool ShouldGetNextFrame(GameTime gameTime)
    {
        frameTimer += gameTime.ElapsedGameTime.TotalSeconds;
        
        if (frameTimer > (1.0 / fps))
            return true;
        
        return false;
    }

    public void MoveNextFrame()
    {
        frameTimer = 0;

        if (inLoop)
        {
            index_x++;

            if (index_x == columns)
            {
                index_y++;
                index_y %= rows;
            }

            index_x %= columns;
        }
        else
        {
            if (index_x + 1 < columns)
                index_x++;
            else if (index_y + 1 < rows)
            {
                index_y++;
                index_x = 0;
            }
        }
    }

    public override void Update(GameTime gameTime)
    {
        if (animating)
        {
            if (ShouldGetNextFrame(gameTime))
                MoveNextFrame();
        }

        sourceRectangle = spriteSheet[index_x, index_y];
        
        Rectangle r = sourceRectangle ?? new Rectangle(0, 0, 0, 0);;
        rect = GetDestRectangle(r);
        
        base.Update(gameTime);
    }
}