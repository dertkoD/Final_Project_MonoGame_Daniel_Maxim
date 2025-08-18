using Microsoft.Xna.Framework;

namespace FinalProjMonoGame;

// Animated sprite built on top of Sprite. Drives frame stepping from a grid-based spritesheet.
public class Animation : Sprite
{
    private SpriteSheet spriteSheet;
    
    // Grid layout of the current spritesheet (used to compute frame rectangles).
    private int columns;
    private int rows;

    // Current frame indices in the grid (x: column, y: row).
    private int index_x = 0;
    private int index_y = 0;
    
    // Timing: per-frame accumulator, target fps, loop flag, and play/stop state.
    private double frameTimer = 0;
    private bool animating = false;
    private int fps;
    private bool inLoop;
    
    public Animation(string animationName) : base(animationName)
    {
        ChangeAnimation(animationName);
    }
    
    private void InitFirstFrameAndOrigin()
    {
        index_x = 0;
        index_y = 0;
        sourceRectangle = spriteSheet[0, 0];
        
        switch (originPosition)
        {
            case OriginPosition.TopLeft:
                origin = Vector2.Zero;
                break;
            case OriginPosition.Center:
                origin = new Vector2(
                    sourceRectangle.Value.Width * 0.5f,
                    sourceRectangle.Value.Height * 0.5f
                );
                break;
        }
        
        rect = GetDestRectangle(sourceRectangle.Value);
    }
    
    // Switches to a new spritesheet and immediately snaps to frame [0,0] with proper origin/rect.
    public void ChangeAnimation(string animationName)
    {
        spriteSheet = new SpriteSheet(SpriteManager.GetSprite(animationName));
        texture = spriteSheet.SpriteSheetInfo.texture;
        columns = spriteSheet.SpriteSheetInfo.columns;
        rows    = spriteSheet.SpriteSheetInfo.rows;

        InitFirstFrameAndOrigin();
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
    
    // Starts playback with given fps and loop behavior (idempotent-safe).
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
        int totalFrames = columns * rows;
        int currentIndex = index_y * columns + index_x;
        double timePerFrame = 1.0 / fps;

        double remainingFrames = (totalFrames - 1 - currentIndex) + (1.0 - MathHelper.Clamp((float)(frameTimer / timePerFrame), 0f, 1f));
        double remainingTime = remainingFrames * timePerFrame;

        return normalized ? remainingTime / (totalFrames * timePerFrame) : remainingTime;
    }

    // Advances frame cursor respecting loop/non-loop; stops at the last frame if not looping.
    public void MoveNextFrame()
    {
        frameTimer = 0;

        if (inLoop)
        {
            index_x++;
            if (index_x == columns)
            {
                index_x = 0;
                index_y = (index_y + 1) % rows;
            }
        }
        else
        {
            if (index_x + 1 < columns) index_x++;
            else if (index_y + 1 < rows) { index_y++; index_x = 0; }
            else { animating = false; }
        }
    }

    // Control surface for external systems (e.g., hit windows): Pause/Resume/Stop/Reset.
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
        
        InitFirstFrameAndOrigin();    
    }

    bool ShouldGetNextFrame(GameTime gameTime)
    {
        frameTimer += gameTime.ElapsedGameTime.TotalSeconds;
        
        if (frameTimer > (1.0 / fps))
            return true;
        
        return false;
    }

    // Per-frame tick: advances frame when enough time passed, refreshes source/dest rectangles.
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