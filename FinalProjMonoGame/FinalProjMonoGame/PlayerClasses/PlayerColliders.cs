using Microsoft.Xna.Framework;

namespace FinalProjMonoGame.PlayerClasses;

public sealed class PlayerColliders
{
    public Collider Sword { get; private set; } = null!;
    public Collider Shield { get; private set; } = null!;
    public Collider Body { get; private set; } = null!;

    // Body tuning
    private readonly float _bodyWScale = 0.25f;
    private readonly float _bodyHScale = 0.70f;
    private readonly int _bodyYOffset = 6;

    public void Init()
    {
        Body = SceneManager.Create<Collider>();
        Body.isTrigger = true;

        Sword = SceneManager.Create<Collider>();
        Sword.isTrigger = true;

        Shield = SceneManager.Create<Collider>();
        Shield.isTrigger = true;
    }

    public void UpdateRects(Player p)
    {
        // Body
        var r = p.rect;
        int bw = (int)(r.Width * _bodyWScale);
        int bh = (int)(r.Height * _bodyHScale);
        int bx = r.Center.X - bw / 2;
        int by = r.Center.Y - bh / 2 + _bodyYOffset;
        Body.rect = new Rectangle(bx, by, bw, bh);

        // Sword
        if (p.State == PlayerState.Attack)
        {
            Rectangle baseRect = Body.rect != Rectangle.Empty ? Body.rect : r;
            int w = (int)(baseRect.Width * 1.5f);
            int h = baseRect.Height;
            int y = baseRect.Center.Y - h / 2;
            int x = p.FacingRight ? baseRect.Right : baseRect.Left - w;
            int overlap = (int)System.Math.Max(1, baseRect.Width * 0.02f);
            x += p.FacingRight ? -overlap : overlap;
            Sword.rect = new Rectangle(x, y, w, h);
        }
        else Sword.rect = Rectangle.Empty;

        // Shield
        if (p.State == PlayerState.Defend)
        {
            int w = (int)(r.Width * 0.15f);
            int h = (int)(r.Height * 0.5f);
            int x = p.FacingRight ? r.Center.X : r.Center.X - w;
            int y = r.Center.Y - h / 2;
            Shield.rect = new Rectangle(x, y, w, h);
        }
        else Shield.rect = Rectangle.Empty;
    }
}