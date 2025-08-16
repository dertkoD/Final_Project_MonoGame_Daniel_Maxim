using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FinalProjMonoGame;

public class ParallaxBackground : IUpdateable, IDrawable
{
    private readonly List<Layer> _layers = new List<Layer>();
    private readonly int _screenW;
    private readonly int _screenH;

    public ParallaxBackground(IEnumerable<(string spriteName, float speed, float alpha)> config)
    {
        _screenW = Game1.ScreenSize.X;
        _screenH = Game1.ScreenSize.Y;

        foreach (var (spriteName, speed, alpha) in config)
        {
            var sheet = SpriteManager.GetSprite(spriteName);
            var tex = sheet.texture; // SpriteSheetInfo exposes .texture

            float scale = (float)_screenH / tex.Height; // fit by height, preserve pixel aspect
            _layers.Add(new Layer
            {
                Texture = tex,
                Speed = speed,
                OffsetX = 0f,
                Scale = scale,
                Alpha = MathHelper.Clamp(alpha, 0f, 1f)
            });
        }
    }

    public void Update(GameTime gameTime)
    {
        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

        foreach (var layer in _layers)
        {
            if (Math.Abs(layer.Speed) < 0.0001f) continue;

            layer.OffsetX += layer.Speed * dt;

            // wrap offset to keep values bounded
            float tileW = layer.Texture.Width * layer.Scale;
            if (tileW <= 0.001f) continue;

            // keep OffsetX in [-tileW, tileW)
            if (layer.OffsetX >= tileW) layer.OffsetX -= tileW;
            if (layer.OffsetX <= -tileW) layer.OffsetX += tileW;
        }
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        foreach (var layer in _layers)
        {
            var tex = layer.Texture;
            float scale = layer.Scale;

            // destination size (scaled to screen height)
            int drawW = (int)Math.Ceiling(tex.Width * scale);
            int drawH = _screenH;

            // starting X so that we always cover the whole screen
            // We scroll to the LEFT when Speed > 0 (i.e., texture moves left)
            int startX = (int)-layer.OffsetX - drawW; // one tile to the left for safety

            // Draw enough tiles to cover the screen + one extra on each side
            // This also handles both positive and negative speeds.
            for (int x = startX; x < _screenW + drawW * 2; x += drawW)
            {
                var dest = new Rectangle(x, 0, drawW, drawH);
                spriteBatch.Draw(tex, dest, Color.White * layer.Alpha);
            }
        }
    }

    // ===== Convenience factory setups for your forest pack =====
    public static ParallaxBackground ForestForMenu()
    {
        // Slower, gentle movement for menu
        return new ParallaxBackground(new (string, float, float)[]
        {
            ("WoodsFourth", 5f, 1f), // far
            ("WoodsThird", 10f, 1f),
            ("WoodsSecond", 15f, 1f),
            ("WoodsFirst", 22f, 1f), // near
            ("Vines", 28f, 0.95f) // nearest decorative layer (slightly dim)
        });
    }

    public static ParallaxBackground ForestForGame()
    {
        // Slightly faster to feel alive during gameplay
        return new ParallaxBackground(new (string, float, float)[]
        {
            ("WoodsFourth", 6f, 1f), // far
            ("WoodsThird", 12f, 1f),
            ("WoodsSecond", 20f, 1f),
            ("WoodsFirst", 30f, 1f), // near
            ("Vines", 40f, 0.95f)
        });
    }
}