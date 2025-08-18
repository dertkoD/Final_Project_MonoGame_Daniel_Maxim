using Microsoft.Xna.Framework.Input;

namespace FinalProjMonoGame.PlayerClasses;

// Reads keyboard and outputs high-level intents per frame.
// Keeps facing direction; respects ControlsEnabled and state gating.
public sealed class PlayerController
{
    // Previous frame snapshot for edge detection (Pressed).
    private KeyboardState _prev;
    
    // Visual facing; updated from directional keys when allowed.
    public bool FacingRight { get; private set; } = true;
    
    // Master input gate (e.g., death/menu/transition).
    public bool ControlsEnabled { get; set; } = true;

    // Polls keyboard and returns intents; updates FacingRight if state allows.
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
        
        // Emit intents only when controls are enabled and state is interactive.
        if (ControlsEnabled && (state == PlayerState.Idle || state == PlayerState.Attack || state == PlayerState.Defend))
        {
            o.DefendHeld = keys.IsKeyDown(Keys.LeftShift); // hold to defend
            o.AttackPressed = Pressed(Keys.E); // tap to attack
        }

        _prev = keys; // must be last: keep edge detection stable
        return o; 
    }
}