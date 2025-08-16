using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FinalProjMonoGame;

public class GroundLayer : IUpdateable, IDrawable
{
    private readonly Texture2D _topTile; // e.g., GroundGrass
    private readonly Texture2D _fillTile; // e.g., Earth

    private readonly int _topW, _topH; // scaled sizes for top tile (screen pixels)
    private readonly int _fillW, _fillH; // scaled sizes for fill tile (screen pixels)

    private int _groundY; // screen-space Y where the player stands (top of earth)
    private float _scrollOffsetX; // optional
    private readonly float _scrollSpeed; // pixels/sec (0 = static)

    private readonly int _overlapPx; // screen pixels to overlap rows (prevents hairline gaps)

    public float GlobalYOffset = 0f; // apply a global vertical offset (for transitions)

    public GroundLayer(string topTileName, string fillTileName, int groundY, int tileScale = 4, float scrollSpeed = 0f,
        int overlapPx = 1)
    {
        _topTile = SpriteManager.GetSprite(topTileName).texture;
        _fillTile = SpriteManager.GetSprite(fillTileName).texture;

        if (tileScale <= 0) tileScale = 1;

        _topW = _topTile.Width * tileScale;
        _topH = _topTile.Height * tileScale;
        _fillW = _fillTile.Width * tileScale;
        _fillH = _fillTile.Height * tileScale;

        _groundY = groundY;
        _scrollSpeed = scrollSpeed;
        _overlapPx = Math.Max(0, overlapPx);
    }

    public void SetGroundY(int groundY) => _groundY = groundY;
    public void SetYOffset(float y) => GlobalYOffset = y;

    public void Update(GameTime gameTime)
    {
        if (Math.Abs(_scrollSpeed) > 0.0001f)
        {
            _scrollOffsetX += _scrollSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
            int tileWidth = Math.Max(1, _fillW);
            if (_scrollOffsetX >= tileWidth) _scrollOffsetX -= tileWidth;
            if (_scrollOffsetX <= -tileWidth) _scrollOffsetX += tileWidth;
        }
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        int screenW = Game1.ScreenSize.X;
        int screenH = Game1.ScreenSize.Y;

        // First draw EARTH fill from groundY to bottom with small vertical overlap to avoid seams.
        int stepY = Math.Max(1, _fillH - _overlapPx);
        int startXFill = (int)(-_scrollOffsetX) - _fillW;

        for (int y = _groundY; y < screenH; y += stepY)
        {
            for (int x = startXFill; x < screenW + _fillW * 2; x += _fillW)
            {
                var dest = new Rectangle(x, (int)(y + GlobalYOffset), _fillW, _fillH);
                spriteBatch.Draw(_fillTile, dest, Color.White);
            }
        }

        // Then draw the top GRASS row overlapping the earth by _overlapPx pixels.
        int grassY = _groundY - _topH + _overlapPx;
        int startXTop = (int)(-_scrollOffsetX) - _topW;
        for (int x = startXTop; x < screenW + _topW * 2; x += _topW)
        {
            var dest = new Rectangle(x, (int)(grassY + GlobalYOffset), _topW, _topH);
            spriteBatch.Draw(_topTile, dest, Color.White);
        }
    }

    // Convenience factory
    public static GroundLayer CreateDefault(int tileScale = 4, int overlapPx = 1)
    {
        int groundY = (int)(Game1.ScreenSize.Y * 0.80f);
        return new GroundLayer("GroundGrass", "Earth", groundY, tileScale, 0f, overlapPx);
    }
}