using FinalProjMonoGame.PlayerClasses;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
namespace FinalProjMonoGame.UI;

public class HpUI : UIElement
{
    private readonly Player _player;
    private readonly string _heartSpriteName;
    private Texture2D _heartTexture;

    public float Scale = 1f;
    public int Spacing = 6;

    public Color FullTint = Color.White;
    
    public HpUI(Player player, string heartSpriteName = "HP")
    {
        _player = player;
        _heartSpriteName = heartSpriteName;

        // default anchor
        Position = new Vector2(16, 16);

        _heartTexture = SpriteManager.GetSprite(_heartSpriteName).texture;
    }

    // no per frame logic, everything depends on player's current HP
    protected override void OnUpdate(GameTime gameTime) {}

    protected override void OnDraw(SpriteBatch spriteBatch)
    {
        if (_heartTexture == null)
            _heartTexture = SpriteManager.GetSprite(_heartSpriteName).texture;

        int cur = _player.HP;
        
        if (cur <= 0 || _heartTexture == null)
            return;
        
        // calculating HP size
        int heartW = (int)(_heartTexture.Width * Scale);
        int heartH = (int)(_heartTexture.Height * Scale);
        
        // drawing one heart per current HP
        for (int i = 0; i < cur; i++)
        {
            var dest = new Rectangle(
                (int)Position.X + i * (heartW + Spacing),
                (int)Position.Y,
                heartW,
                heartH
            );
            
            spriteBatch.Draw(
                _heartTexture,
                destinationRectangle: dest,
                sourceRectangle: null,
                color: FullTint,
                rotation: 0f,
                origin: Vector2.Zero, 
                effects: SpriteEffects.None,
                layerDepth: 0f
                );
        }
    }
}