using Microsoft.Xna.Framework.Graphics;

namespace FinalProjMonoGame;

public class Layer
{
    public Texture2D Texture;
    public float BaseSpeed;   // pixels per second (positive -> scrolls to the left)
    public float OffsetX;     // current horizontal offset in pixels (after scaling)
    public float Scale;       // scale to fit screen height
    public float Alpha;  
}