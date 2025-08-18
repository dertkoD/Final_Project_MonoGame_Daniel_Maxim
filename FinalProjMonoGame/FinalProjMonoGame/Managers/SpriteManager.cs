using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace FinalProjMonoGame;

// Central registry for spritesheets loaded from Content.
// AddSprite() loads once and stores grid meta; GetSprite() returns the metadata by name.
public class SpriteManager
{
    // Shared Content reference; set by the first constructed SpriteManager.
    static ContentManager content;
    // Name → spritesheet info (texture + columns/rows).
    static Dictionary<string, SpriteSheetInfo> sprites = new Dictionary<string, SpriteSheetInfo>();
    
    // Must be created early (e.g., in Game1.LoadContent) before AddSprite calls.
    public SpriteManager(ContentManager contentManager)
    {
        content = contentManager; // no ownership/lifetime, just a handle
    }
    
    // Registers a spritesheet by name and loads its texture from the Content pipeline.
    // columns/rows define the uniform frame grid (defaults to 1×1 for static sprites).
    public static void AddSprite(string spriteName, string fileName, int columns = 1, int rows = 1)
    {
        sprites[spriteName] = new SpriteSheetInfo();
        sprites[spriteName].texture = content.Load<Texture2D>(fileName);
        sprites[spriteName].columns = columns;
        sprites[spriteName].rows = rows;
    }

    // Returns metadata for a previously registered sprite.
    // Assumes spriteName exists; callers should ensure AddSprite was called at startup.
    public static SpriteSheetInfo GetSprite(string spriteName)
    {
        return sprites[spriteName];
    }
}