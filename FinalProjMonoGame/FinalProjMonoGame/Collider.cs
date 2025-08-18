using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FinalProjMonoGame;

// Axis-aligned rectangle collider. Can act as trigger (no physics) or solid collider.
// Fires events via Notify(); draws debug outline in DEBUG builds.
public class Collider : Sprite 
{
    public Rectangle rect; // world-space AABB used for tests and debug draw
    public bool isTrigger = false;  // true => raise OnTrigger instead of OnCollision

    // Event surface
    public delegate void Somefunction(object o);
    public event Somefunction OnCollision;
    public event Somefunction OnTrigger;

    // Debug rendering params (wireframe lines built from 1x1 "Pixel" texture).

    Color color = Color.White;
    int thickness = 1;

    public Collider() : base("Pixel")
    {
    }
    
    // AABB vs AABB overlap test. Callers choose when to run it (e.g., per frame).
    public bool Intersects(Collider other)
    {
        return rect.Intersects(other.rect);
    }

    // Dispatches appropriate event based on trigger/solid mode. No physics resolution here.
    public void Notify(object o)
    {
        if (isTrigger)
            OnTrigger?.Invoke(o);
        else
            OnCollision?.Invoke(o);
    }

    // Debug-only outline of the collider rectangle.
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