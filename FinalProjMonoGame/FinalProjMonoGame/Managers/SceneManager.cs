using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FinalProjMonoGame;

public class SceneManager : IUpdateable, IDrawable
{
    private static List<IUpdateable> updateables = new List<IUpdateable>();
    private static List<IDrawable> drawables = new List<IDrawable>();
    
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

    public static T Create<T>() where T : IDrawable, new()
    {
        T item = new T();
        Add(item);
        
        return item;
    }

    public static void Add<T>(T item) where T: IDrawable//, IUpdateable
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

    public void Update(GameTime gameTime)
    {
        for (int i = 0; i < updateables.Count; i++)
        {
            updateables[i].Update(gameTime);
        }
    }

    public void Draw(SpriteBatch _spriteBatch)
    {
        foreach (IDrawable drawable in drawables)
        {
            drawable.Draw(_spriteBatch);
        }
    }
    
    public static void Clear()
    {
        updateables.Clear();
        drawables.Clear();
    }

    public static void SwitchTo(System.Action setup)
    {
        Clear();
        setup?.Invoke();
    }
}