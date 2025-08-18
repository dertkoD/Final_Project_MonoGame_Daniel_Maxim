using System;
using FinalProjMonoGame.PlayerClasses;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FinalProjMonoGame;

// Spawns arrows/bombs toward the player at random intervals from given points.
// Auto-pauses when player is dead or controls are disabled; can be paused externally.
public class EnemySpawner : IDrawable, IUpdateable
{
    public bool IsPaused { get; private set; } = false;
    public void Pause()  => IsPaused = true; // external pause
    public void Resume() => IsPaused = false;
    
    private readonly Random _rng = new Random();
    private Vector2[] _points = Array.Empty<Vector2>(); // candidate spawn points
    private Player _player;

    // Spawn cadence (seconds): randomized per spawn between min/max.
    public float MinInterval { get; set; } = 1.2f;
    public float MaxInterval { get; set; } = 2.5f;
    private float _timer;

    // Arrow tuning (speed range + lifetime used by Arrow itself).
    public float ArrowSpeedMin { get; set; } = 550f;
    public float ArrowSpeedMax { get; set; } = 750f;
    
    // Bomb tuning (simple ballistic throw configured by gravity + time window).
    public float BombGravity { get; set; } = 450f;
    public float BombTimeMin { get; set; } = 0.9f;
    public float BombTimeMax { get; set; } = 1.6f;

    // Wire player + spawn points (falls back to offscreen left/right if none provided).
    public void Init(Player player, params Vector2[] spawnPoints)
    {
        _player = player ?? throw new ArgumentNullException(nameof(player));
        _points = spawnPoints != null && spawnPoints.Length > 0
            ? spawnPoints
            : new[]
            {
                new Vector2(-100, Game1.ScreenCenter.Y),
                new Vector2(Game1.ScreenBounds.Right + 100, Game1.ScreenCenter.Y)
            };

        ResetTimer();
    }

    
    // Countdown → when zero, spawn one enemy and reset timer.
    // Early-returns if paused, player dead, or input disabled (game over).
    public void Update(GameTime gameTime)
    {
        if (_player == null || _points.Length == 0) return;
        
        if (IsPaused || _player.HP <= 0 || !_player.ControlIsEnabled)
            return;

        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
        _timer -= dt;
        if (_timer <= 0f)
        {
            SpawnOne();
            ResetTimer();
        }
    }

    // Spawner has no visuals; kept to satisfy IDrawable contract.
    public void Draw(SpriteBatch spriteBatch)
    {
    }

    // Randomize next interval within [MinInterval, MaxInterval].
    private void ResetTimer()
    {
        _timer = Lerp(MinInterval, MaxInterval, (float)_rng.NextDouble());
    }

    // Pick a spawn point; ensure it starts offscreen; choose Arrow or Bomb 50/50.
    private void SpawnOne()
    {
        Vector2 spawn = _points[_rng.Next(_points.Length)];
        
        if (IsOnScreen(spawn))
            spawn = PushOffscreen(spawn);
        
        if (_rng.Next(2) == 0)
            SpawnArrow(spawn);
        else
            SpawnBomb(spawn);
    }

    // Arrow: shoot directly toward the player's current position with random speed.
    private void SpawnArrow(Vector2 spawn)
    {
        var arrow = SceneManager.Create<Arrow>();
        arrow.player = _player;
        arrow.position = spawn;

        Vector2 toPlayer = _player.position - spawn;
        if (toPlayer.LengthSquared() < 0.0001f) toPlayer = new Vector2(1, 0);
        toPlayer.Normalize();

        float speed = Lerp(ArrowSpeedMin, ArrowSpeedMax, (float)_rng.NextDouble());
        arrow.Velocity = toPlayer * speed;
    }

    // Bomb: compute a simple ballistic throw to reach the player in time 't' under gravity 'g'.
    private void SpawnBomb(Vector2 spawn)
    {
        var bomb = SceneManager.Create<Bomb>();
        bomb.player = _player;
        bomb.position = spawn;

        float g = BombGravity;
        float t = Lerp(BombTimeMin, BombTimeMax, (float)_rng.NextDouble());
        if (t < 0.2f) t = 0.2f;

        Vector2 delta = _player.position - spawn;
        float vx = delta.X / t;
        float vy = (delta.Y - 0.5f * g * t * t) / t;

        bomb.Velocity = new Vector2(vx, vy);
        bomb.Gravity = g;
    }

    // helpers
    private static float Lerp(float a, float b, float t) => a + (b - a) * MathHelper.Clamp(t, 0, 1);

    private static bool IsOnScreen(Vector2 p)
    {
        return Game1.ScreenBounds.Contains(new Point((int)p.X, (int)p.Y));
    }

    // If a chosen point is on-screen, push it just outside the nearest edge (with padding).
    private static Vector2 PushOffscreen(Vector2 p, float pad = 64f)
    {
        var r = Game1.ScreenBounds;

        float left = Math.Abs(p.X - r.Left);
        float right = Math.Abs(r.Right - p.X);
        float top = Math.Abs(p.Y - r.Top);
        float bottom = Math.Abs(r.Bottom - p.Y);

        float m = Math.Min(Math.Min(left, right), Math.Min(top, bottom));

        if (m == left) return new Vector2(r.Left - pad, p.Y);
        if (m == right) return new Vector2(r.Right + pad, p.Y);
        if (m == top) return new Vector2(p.X, r.Top - pad);

        return new Vector2(p.X, r.Bottom + pad);
    }
}