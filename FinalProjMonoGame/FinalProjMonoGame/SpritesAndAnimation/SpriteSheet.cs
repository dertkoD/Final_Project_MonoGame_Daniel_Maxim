using Microsoft.Xna.Framework;
namespace FinalProjMonoGame;

public class SpriteSheet
{
    public SpriteSheetInfo SpriteSheetInfo {  get; }

    public SpriteSheet(SpriteSheetInfo spriteSheetInfo)
    {
        this.SpriteSheetInfo = spriteSheetInfo;
    }
    public Rectangle this[int index_x, int index_y]
    {
        get
        {
            Point location = new Point(
                (int)(SpriteSheetInfo.texture.Width * ((float)index_x / SpriteSheetInfo.columns)),
                (int)(SpriteSheetInfo.texture.Height * ((float)index_y / SpriteSheetInfo.rows))
                );
            
              Point size = new Point(
                  (int)(SpriteSheetInfo.texture.Width * (1.0f / SpriteSheetInfo.columns)),
                  (int)(SpriteSheetInfo.texture.Height * (1.0f / SpriteSheetInfo.rows))
                  );
              
            
          return new Rectangle(location, size);
        }
    }

}