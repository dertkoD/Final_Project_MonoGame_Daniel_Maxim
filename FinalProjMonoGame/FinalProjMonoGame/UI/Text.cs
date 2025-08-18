using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FinalProjMonoGame.UI;

public class Text : UIElement
{ 
    public SpriteFont Font { get; set; }
    public string Value { get; set; } = ""; 
    public Color Color { get; set; } = Color.White;
    public float Scale { get; set; } = 1f;
    public float Rotation { get; set; } = 0f;

    // optional shadow
    public bool Shadow { get; set; } = false;
    public Color ShadowColor { get; set; } = new Color(0,0,0,160);

    // caches (only recompute when text/scale changes)
    private string _prev;
    private float _prevScale = -1f;
    private Vector2 _halfSize;

    public Text(SpriteFont font)
    {
        Font = font;
    }

    protected override void OnUpdate(GameTime gameTime)
    {
        if (Font == null) return;

        if (_prev != Value || _prevScale != Scale)
        {
            _prev = Value;
            _prevScale = Scale;
            _halfSize = Font.MeasureString(Value ?? "") * 0.5f * Scale;
        }
    }

    protected override void OnDraw(SpriteBatch sb)
    { 
        if (Font == null || string.IsNullOrEmpty(Value)) return;

        var pos = new Vector2((int)Position.X, (int)Position.Y);
        var origin = _halfSize;

        if (Shadow)
        {
            sb.DrawString(Font, Value, pos + new Vector2(1,1), ShadowColor,
                Rotation, origin, Scale, SpriteEffects.None, 0.13f);
        }

        sb.DrawString(Font, Value, pos, Color,
            Rotation, origin, Scale, SpriteEffects.None, 0.14f);
    }
}