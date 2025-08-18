using Microsoft.Xna.Framework.Input;

namespace FinalProjMonoGame.PlayerClasses;

public sealed class PlayerController
{
    private KeyboardState _prev;
    public bool FacingRight { get; private set; } = true;
    public bool ControlsEnabled { get; set; } = true;

    public struct Output
    {
        public bool AttackPressed;  // edge
        public bool DefendHeld;     // level
    }

    public Output Update(PlayerState state)
    {
        var keys = Keyboard.GetState();
        bool Pressed(Keys k) => keys.IsKeyDown(k) && _prev.IsKeyUp(k);

        // Update facing when allowed (Idle/Attack/Defend only)
        if (state == PlayerState.Idle || state == PlayerState.Attack || state == PlayerState.Defend)
        {
            if (keys.IsKeyDown(Keys.A) || keys.IsKeyDown(Keys.Left)) FacingRight = false;
            else if (keys.IsKeyDown(Keys.D) || keys.IsKeyDown(Keys.Right)) FacingRight = true;
        }

        Output o = default;
        if (ControlsEnabled && (state == PlayerState.Idle || state == PlayerState.Attack || state == PlayerState.Defend))
        {
            o.DefendHeld = keys.IsKeyDown(Keys.LeftShift);
            o.AttackPressed = Pressed(Keys.E);
        }

        _prev = keys;
        return o;
    }
}