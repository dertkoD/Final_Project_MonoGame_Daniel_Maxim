using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FinalProjMonoGame;

// Minimal scene graph: keeps two flat lists (updateables/drawables) and ticks/draws them in order.
// Order = order of insertion; no layers/z-index here (add your own if needed).
public class SceneManager : IUpdateable, IDrawable
{
    // Global registries for the current scene.
    private static List<IUpdateable> updateables = new List<IUpdateable>();
    private static List<IDrawable> drawables = new List<IDrawable>();
    
    // Singleton to access from anywhere 
    private static SceneManager instance;
    public static SceneManager Instance
    {
        get
        {
            if (instance == null)
                instance = new SceneManager();
            
            return instance;
        }
    }

    // Convenience: create, add to lists, and return a new node.
    public static T Create<T>() where T : IDrawable, new()
    {
        T item = new T();
        Add(item);
        
        return item;
    }

    // Registers an object into one or both lists depending on implemented interfaces.
    public static void Add<T>(T item) where T: IDrawable
    {
        if (item is IUpdateable updateable)
        {
            updateables.Add(updateable);
        }
        if (item is IDrawable drawable)
        {
            drawables.Add(item);
        }
    }

    // Unregisters from lists; safe to call even if item wasn’t present.
    public static void Remove<T>(T item) where T: IDrawable
    {
        if (item is IUpdateable updateable)
        {
            updateables.Remove(updateable);
        }
        if (item is IDrawable drawable)
        {
            drawables.Remove(item);
        }
    }

    // Per-frame tick: iterates in insertion order.
    // Note: if objects add/remove during Update, consider iterating over a copy.
    public void Update(GameTime gameTime)
    {
        for (int i = 0; i < updateables.Count; i++)
        {
            updateables[i].Update(gameTime);
        }
    }

    // Per-frame draw: also insertion order; no sorting/layers here.
    public void Draw(SpriteBatch _spriteBatch)
    {
        foreach (IDrawable drawable in drawables)
        {
            drawable.Draw(_spriteBatch);
        }
    }
    
    // Clears both lists — effectively destroys the current scene (no disposal logic here).
    public static void Clear()
    {
        updateables.Clear();
        drawables.Clear();
    }

    // Hard scene switch: wipe current nodes, then run provided setup to build the next scene.
    public static void SwitchTo(System.Action setup)
    {
        Clear();
        setup?.Invoke();
    }
}