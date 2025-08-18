using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FinalProjMonoGame;

public class Collider : Sprite 
{
    public Rectangle rect;
    public bool isTrigger = false;

    public delegate void Somefunction(object o);
    public event Somefunction OnCollision;
    public event Somefunction OnTrigger;

    // for debug
    Color color = Color.White;
    int thickness = 1;

    public Collider() : base("Pixel")
    {
    }
    public bool Intersects(Collider other)
    {
        return rect.Intersects(other.rect);
    }

    public void Notify(object o)
    {
        if (isTrigger)
            OnTrigger?.Invoke(o);
        else
            OnCollision?.Invoke(o);
    }

    public override void Draw(SpriteBatch _spriteBatch)
    {
        #if DEBUG
        // draw outline bounds
        
        color = Color.Green;
        thickness = 5;
        
        _spriteBatch.Draw(
            texture,
            new Rectangle(rect.X, rect.Y, rect.Width, thickness), // top
            color);

        _spriteBatch.Draw(
            texture,
            new Rectangle(rect.X, rect.Y, thickness, rect.Height), // left
            color);

        _spriteBatch.Draw(
            texture,
            new Rectangle(rect.X + rect.Width - thickness, rect.Y, thickness, rect.Height), // right
            color);

        _spriteBatch.Draw(
            texture,
            new Rectangle(rect.X, rect.Y + rect.Height - thickness, rect.Width, thickness), // bottom
            color);
        
        #endif
    }
}