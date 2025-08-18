using System;
using System.Collections.Generic;
using FinalProjMonoGame.Transition;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FinalProjMonoGame;

// Multi-layer horizontally scrolling background.
// Each layer fits screen height, scrolls at its own base speed, and tiles in X.
public class ParallaxBackground : IUpdateable, IDrawable
{
    private readonly List<Layer> _layers = new List<Layer>();
    private readonly int _screenW;
    private readonly int _screenH;

    // Global speed multiplier (can be eased toward a target over duration).
    private float _speedMultiplier = 1f; // global speed multiplier
    private float _speedMultiplierTarget = 1f; // for smooth changes
    private float _speedLerpTime = 0f; // current time
    private float _speedLerpDuration = 0f; // duration

    // Instantiation picks textures by name, scales each to screen height, and stores per-layer settings.
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
                BaseSpeed = speed,
                OffsetX = 0f,
                Scale = scale,
                Alpha = MathHelper.Clamp(alpha, 0f, 1f)
            });
        }
    }

    //Immediately sets the global speed multiplier (1 = normal, 0 = stop)
    public void SetSpeedMultiplier(float m) => _speedMultiplier = _speedMultiplierTarget = MathHelper.Clamp(m, 0f, 5f);

    // Smoothly transitions to target multiplier over duration seconds
    public void SmoothSpeed(float targetMultiplier, float durationSec = 0.5f)
    {
        _speedMultiplierTarget = MathHelper.Clamp(targetMultiplier, 0f, 5f);
        _speedLerpDuration = Math.Max(0.0001f, durationSec);
        _speedLerpTime = 0f;
    }

    //Convenience: smoothly stop parallax
    public void SmoothStop(float durationSec = 0.5f) => SmoothSpeed(0f, durationSec);

    // Update: ease multiplier if needed; advance each layer's OffsetX; wrap by tile width.
    public void Update(GameTime gameTime)
    {
        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

        // ease global speed multiplier if needed
        if (Math.Abs(_speedMultiplier - _speedMultiplierTarget) > 0.0001f)
        {
            _speedLerpTime += dt;
            float t = MathHelper.Clamp(_speedLerpTime / _speedLerpDuration, 0f, 1f);
            // Smoothstep easing
            t = t * t * (3f - 2f * t);
            _speedMultiplier = MathHelper.Lerp(_speedMultiplier, _speedMultiplierTarget, t);
        }

        foreach (var layer in _layers)
        {
            float speed = layer.BaseSpeed * _speedMultiplier;
            if (Math.Abs(speed) < 0.0001f) continue;

            layer.OffsetX += speed * dt;

            // wrap offset to keep values bounded
            float tileW = layer.Texture.Width * layer.Scale;
            if (tileW <= 0.001f) continue;

            if (layer.OffsetX >= tileW) layer.OffsetX -= tileW;
            if (layer.OffsetX <= -tileW) layer.OffsetX += tileW;
        }
    }

    // Draw: tile each layer across the screen using its scaled width and current offset.
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
            int startX = (int)-layer.OffsetX - drawW; // one tile to the left for safety

            for (int x = startX; x < _screenW + drawW * 2; x += drawW)
            {
                var dest = new Rectangle(x, 0, drawW, drawH);
                spriteBatch.Draw(tex, dest, Color.White * layer.Alpha);
            }
        }
    }

    // Ready-made presets for your forest art packs (menu/game speeds differ).
    public static ParallaxBackground ForestForMenu()
    {
        return new ParallaxBackground(new (string, float, float)[]
        {
            ("WoodsFourth", 5f, 1f), // far
            ("WoodsThird", 10f, 1f),
            ("WoodsSecond", 15f, 1f),
            ("WoodsFirst", 22f, 1f), // near
            ("Vines", 28f, 0.95f)
        });
    }

    public static ParallaxBackground ForestForGame()
    {
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