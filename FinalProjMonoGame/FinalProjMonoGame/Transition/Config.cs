namespace FinalProjMonoGame;

// Aggregates transition parameters for menu/parallax/ground.
// Use the named factories below for common scenarios (enter / exit / start game).
public struct Config
{
    public bool MenuEnterFlag; // true: bring to screen, false: remove from screen
    public SlideDirection MenuDirection; 
    public float MenuDuration;

    public bool AnimateGround; // animate ground vertical offset
    public float GroundFromY; 
    public float GroundToY; 
    public float GroundDuration; 

    public bool AnimateParallax; // tween global parallax speed multiplier
    public float ParallaxTargetMultiplier; 
    public float ParallaxDuration; 

    // Preset: starting a new game from the main menu — stop parallax, raise ground, hide menu upward.
    public static Config DefaultMainMenuStart()
    {
        int screenH = Game1.ScreenSize.Y;
        return new Config
        {
            // Move the menu UP
            MenuEnterFlag = false,
            MenuDirection = SlideDirection.Up,
            MenuDuration = 0.60f,

            // Raise the earth from below (from the screen) to 0
            AnimateGround = true,
            GroundFromY = screenH,
            GroundToY = 0f,
            GroundDuration = 0.75f,

            // Stop the parallax smoothly (visually the camera is fixed)
            AnimateParallax = true,
            ParallaxTargetMultiplier = 0f,
            ParallaxDuration = 0.60f
        };
    }

    // Preset: bring menu in (fromTop=true => from -screenH to 0).
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

    // Preset: move menu out (toTop=true => 0 to -screenH).
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