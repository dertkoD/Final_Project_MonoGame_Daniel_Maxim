using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FinalProjMonoGame;

// Orchestrates the seamless menu→game (and reverse) reveal:
// - Slides the menu vertically via a callback
// - Optionally eases parallax speed and ground Y offset
// - Calls onComplete() when all animated parts finish
public class MenuTransition : IUpdateable, IDrawable
{
    private readonly ParallaxBackground _parallax; 
    private readonly GroundLayer _ground; 
    private readonly Action<float> _setMenuSlideOffsetY; 
    private readonly Action _onComplete; 
    
    private Phase _phase = Phase.Idle;

    private Config _cfg;

    // Normalized timers for each animated part.
    private float _tMenu;
    private float _tGround; 

    // Computed start/end Y for menu slide (based on screen size and direction).
    private float _menuFromY, _menuToY;

    public MenuTransition(
        ParallaxBackground parallax,
        GroundLayer ground,
        Action<float> setMenuSlideOffsetY,
        Action onComplete)
    {
        _parallax = parallax;
        _ground = ground;
        _setMenuSlideOffsetY = setMenuSlideOffsetY ?? throw new ArgumentNullException(nameof(setMenuSlideOffsetY));
        _onComplete = onComplete;
    }

    public void Begin() => Begin(Config.DefaultMainMenuStart());

    // Begin(): capture config, set initial positions, kick off parallax/ground tweens.
    public void Begin(Config cfg)
    {
        _cfg = cfg;
        _phase = Phase.Running;
        _tMenu = 0f;
        _tGround = 0f;

        int screenH = Game1.ScreenSize.Y;
        float offTop = -screenH;
        float offBottom = screenH;

        if (cfg.MenuEnterFlag)
        {
            _menuFromY = (cfg.MenuDirection == SlideDirection.Up) ? offTop : offBottom;
            _menuToY = 0f;
        }
        else
        {
            _menuFromY = 0f;
            _menuToY = (cfg.MenuDirection == SlideDirection.Up) ? offTop : offBottom;
        }

        _setMenuSlideOffsetY(_menuFromY);

        if (_cfg.AnimateParallax && _parallax != null)
            _parallax.SmoothSpeed(_cfg.ParallaxTargetMultiplier, _cfg.ParallaxDuration);

        if (_cfg.AnimateGround && _ground != null)
            _ground.SetYOffset(_cfg.GroundFromY);
    }

    // Update(): advance timers, lerp values with easing, invoke completion when both reach 1.
    public void Update(GameTime gameTime)
    {
        if (_phase != Phase.Running) return;

        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

        _tMenu += (_cfg.MenuDuration <= 0f ? 1f : dt / _cfg.MenuDuration);
        float m = MathHelper.Clamp(_tMenu, 0f, 1f);
        float menuY = MathHelper.Lerp(_menuFromY, _menuToY, EaseOutCubic(m));
        _setMenuSlideOffsetY(menuY);

        float g = 1f;
        if (_cfg.AnimateGround && _ground != null)
        {
            _tGround += (_cfg.GroundDuration <= 0f ? 1f : dt / _cfg.GroundDuration);
            g = MathHelper.Clamp(_tGround, 0f, 1f);
            float gy = MathHelper.Lerp(_cfg.GroundFromY, _cfg.GroundToY, EaseInOutCubic(g));
            _ground.SetYOffset(gy);
        }

        if (m >= 1f && g >= 1f)
        {
            _phase = Phase.Done;
            _onComplete?.Invoke();
        }
    }

    public void Draw(SpriteBatch spriteBatch) { }

    // Easing helpers: cubic out for menu, cubic in-out for ground (slightly heavier feel).
    private static float EaseOutCubic(float x) => 1f - (float)Math.Pow(1f - x, 3f);

    private static float EaseInOutCubic(float x)
    {
        return x < 0.5f
            ? 4f * x * x * x
            : 1f - (float)Math.Pow(-2f * x + 2f, 3f) * 0.5f;
    }
}