namespace FinalProjMonoGame.PlayerClasses;

// Per-frame intents returned to the orchestrator.
public struct Output
{
    public bool AttackPressed;  // edge: true only on key-down this frame
    public bool DefendHeld;     // level: true while key is held
}