using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace FinalProjMonoGame;

public class SpriteManager
{
    static ContentManager content;
    static Dictionary<string, SpriteSheetInfo> sprites = new Dictionary<string, SpriteSheetInfo>();
    
    public SpriteManager(ContentManager contentManager)
    {
        content = contentManager;
    }
    public static void AddSprite(string spriteName, string fileName, int columns = 1, int rows = 1)
    {
        sprites[spriteName] = new SpriteSheetInfo();
        sprites[spriteName].texture = content.Load<Texture2D>(fileName);
        sprites[spriteName].columns = columns;
        sprites[spriteName].rows = rows;
    }

    public static SpriteSheetInfo GetSprite(string spriteName)
    {
        return sprites[spriteName];
    }
}