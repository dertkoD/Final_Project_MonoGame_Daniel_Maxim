using Microsoft.Xna.Framework;

namespace FinalProjMonoGame;

public class Arrow : Enemy
{
    public bool IsSpinning { get; private set; } = false;
    private float _angularSpeedDeg = 0f;

    public Arrow() : base("Arrow")
    {
        scale = new Vector2(0.4f, 0.4f);
        Damage = 1;
    }

    // Включаем «падение со спином»
    public void StartSpin(Vector2 initialVelocity, float angularSpeedDeg, float gravity)
    {
        Velocity = initialVelocity;
        Gravity = gravity;
        _angularSpeedDeg = angularSpeedDeg;
        IsSpinning = true;

        IgnorePlayerCollision = true; // после отбивания телу игрока урон не наносит
    }

    protected override void OnUpdate(GameTime gameTime)
    {
        if (IsSpinning)
        {
            rotation += _angularSpeedDeg * (float)gameTime.ElapsedGameTime.TotalSeconds;
        }
        else
        {
            FaceVelocity(); // до отбивания летит «носом вперёд»
        }
    }
}