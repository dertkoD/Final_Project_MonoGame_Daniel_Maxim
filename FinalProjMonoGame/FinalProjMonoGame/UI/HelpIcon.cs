using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace FinalProjMonoGame.UI;

public class HelpIcon : UIElement
{
    public Rectangle Bounds;
    private readonly string _spriteName;
    private Texture2D _texture;
    private readonly HelpPopup _popup;

    public Color Tint = Color.White;

    public HelpIcon(HelpPopup popup, string spriteName = "HelpIcon")
    {
        _popup = popup;
        _spriteName = spriteName;
    }

    protected override void OnUpdate(GameTime gameTime)
    {
        if (_texture == null)
            _texture = SpriteManager.GetSprite(_spriteName).texture;

        var mouse = Mouse.GetState();
        _popup.Visible = Bounds.Contains(mouse.Position); // checks the visibility of the popup 
    }

    protected override void OnDraw(SpriteBatch sb)
    {
        if (_texture != null && Bounds.Width > 0 && Bounds.Height > 0)
        {
            sb.Draw(_texture, destinationRectangle: Bounds, color: Tint);
        }
    }
}