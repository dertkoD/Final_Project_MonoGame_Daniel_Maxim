using Microsoft.Xna.Framework.Graphics;

namespace FinalProjMonoGame;

// Metadata describing a spritesheet grid: texture + columns/rows.
// Used by SpriteSheet to calculate per-frame rectangles.
public class SpriteSheetInfo
{
    public Texture2D texture { get; set; }
    public int columns { get; set; }
    public int rows { get; set; }
}