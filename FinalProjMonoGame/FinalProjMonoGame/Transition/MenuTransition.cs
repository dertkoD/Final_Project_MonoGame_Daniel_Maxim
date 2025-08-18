using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FinalProjMonoGame;

public class MenuTransition : IUpdateable, IDrawable
{
    private readonly ParallaxBackground _parallax; // может быть null
    private readonly GroundLayer _ground; // может быть null
    private readonly Action<float> _setMenuSlideOffsetY; // обязателен
    private readonly Action _onComplete; // может быть null

    // -------- Параметры/режим ----------
    public enum SlideDirection
    {
        Up,
        Down
    }

    public struct Config
    {
        // Меню
        public bool MenuEnterFlag; // true: внести на экран, false: увести с экрана
        public SlideDirection MenuDirection; // откуда вносить / куда уводить
        public float MenuDuration; // сек

        // Земля (необязательно)
        public bool AnimateGround;
        public float GroundFromY; // начальный GlobalYOffset
        public float GroundToY; // конечный GlobalYOffset
        public float GroundDuration; // сек

        // Параллакс (необязательно)
        public bool AnimateParallax;
        public float ParallaxTargetMultiplier; // 1 = обычная скорость, 0 = стоп
        public float ParallaxDuration; // сек

        // Удобные пресеты
        public static Config DefaultMainMenuStart()
        {
            int screenH = Game1.ScreenSize.Y;
            return new Config
            {
                // Меню уводим ВВЕРХ
                MenuEnterFlag = false,
                MenuDirection = SlideDirection.Up,
                MenuDuration = 0.60f,

                // Землю поднимаем снизу (с экрана) к 0
                AnimateGround = true,
                GroundFromY = screenH,
                GroundToY = 0f,
                GroundDuration = 0.75f,

                // Параллакс плавно останавливаем (визуально «камера фиксируется»)
                AnimateParallax = true,
                ParallaxTargetMultiplier = 0f,
                ParallaxDuration = 0.60f
            };
        }

        public static Config MenuEnter(bool fromTop, float duration = 0.60f)
        {
            return new Config
            {
                MenuEnterFlag = true,
                MenuDirection = fromTop ? SlideDirection.Up : SlideDirection.Down,
                MenuDuration = duration,

                AnimateGround = false,
                AnimateParallax = false
            };
        }

        public static Config MenuExit(bool toTop, float duration = 0.60f)
        {
            return new Config
            {
                MenuEnterFlag = false,
                MenuDirection = toTop ? SlideDirection.Up : SlideDirection.Down,
                MenuDuration = duration,

                AnimateGround = false,
                AnimateParallax = false
            };
        }
    }

    // -------- Внутреннее состояние ----------
    private enum Phase
    {
        Idle,
        Running,
        Done
    }

    private Phase _phase = Phase.Idle;

    private Config _cfg;

    private float _tMenu; // 0..1
    private float _tGround; // 0..1

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

    /// <summary>Старое поведение: увести главное меню вверх, поднять землю снизу и стартануть игру.</summary>
    public void Begin() => Begin(Config.DefaultMainMenuStart());

    /// <summary>Запуск с пользовательскими флагами/направлением.</summary>
    public void Begin(Config cfg)
    {
        _cfg = cfg;
        _phase = Phase.Running;
        _tMenu = 0f;
        _tGround = 0f;

        // Меню: вычислить старт/финиш по направлению
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

        // Поставить начальное значение меню
        _setMenuSlideOffsetY(_menuFromY);

        // Плавная смена скорости параллакса (если есть)
        if (_cfg.AnimateParallax && _parallax != null)
            _parallax.SmoothSpeed(_cfg.ParallaxTargetMultiplier, _cfg.ParallaxDuration);

        // Землю — сразу в стартовую позицию (если надо)
        if (_cfg.AnimateGround && _ground != null)
            _ground.SetYOffset(_cfg.GroundFromY);
    }

    public void Update(GameTime gameTime)
    {
        if (_phase != Phase.Running) return;

        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

        // --- Меню ---
        _tMenu += (_cfg.MenuDuration <= 0f ? 1f : dt / _cfg.MenuDuration);
        float m = MathHelper.Clamp(_tMenu, 0f, 1f);
        float menuY = MathHelper.Lerp(_menuFromY, _menuToY, EaseOutCubic(m));
        _setMenuSlideOffsetY(menuY);

        // --- Земля (опционально) ---
        float g = 1f;
        if (_cfg.AnimateGround && _ground != null)
        {
            _tGround += (_cfg.GroundDuration <= 0f ? 1f : dt / _cfg.GroundDuration);
            g = MathHelper.Clamp(_tGround, 0f, 1f);
            float gy = MathHelper.Lerp(_cfg.GroundFromY, _cfg.GroundToY, EaseInOutCubic(g));
            _ground.SetYOffset(gy);
        }

        // Завершение
        if (m >= 1f && g >= 1f)
        {
            _phase = Phase.Done;
            _onComplete?.Invoke();
        }
    }

    public void Draw(SpriteBatch spriteBatch) { }

    // -------- EASING --------
    private static float EaseOutCubic(float x) => 1f - (float)Math.Pow(1f - x, 3f);

    private static float EaseInOutCubic(float x)
    {
        return x < 0.5f
            ? 4f * x * x * x
            : 1f - (float)Math.Pow(-2f * x + 2f, 3f) * 0.5f;
    }
}