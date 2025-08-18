using Microsoft.Xna.Framework;
namespace FinalProjMonoGame;

// Read-only helper that slices a texture into grid-aligned frames.
// Indexer returns the Rectangle for [column, row] in the sheet.
public class SpriteSheet
{
    public SpriteSheetInfo SpriteSheetInfo {  get; }

    public SpriteSheet(SpriteSheetInfo spriteSheetInfo)
    {
        this.SpriteSheetInfo = spriteSheetInfo;
    }
    
    // Computes frame rectangle by dividing texture width/height by columns/rows.
    public Rectangle this[int index_x, int index_y]
    {
        get
        {
            // 'location' is top-left of the frame; 'size' is one grid cell.
            // Works for any uniform grid without padding.
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