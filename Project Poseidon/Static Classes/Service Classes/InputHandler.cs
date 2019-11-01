using Microsoft.Xna.Framework.Input;

public static class InputHandler
{
    private static KeyboardState previous;
    private static bool inputBlocked = false, spaceBlocked= false;
    private static int blockTimer = 0;
    public static void blockSpace()
    {
        spaceBlocked = true;
    }
    public static bool IsInputBlocked()
    {
        return inputBlocked;
    }
    public static void blockInput(float sec)
    {
        blockTimer = Global.getTics(sec);
        inputBlocked = true;
    }
    public static void unblockInput()
    {
        inputBlocked = false;
    }
    /// <summary>
    /// Returns true if the key was just pressed
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public static bool KeyStroke(Keys key)
    {
        return Keyboard.GetState().IsKeyDown(key) && !previous.IsKeyDown(key);
    }
    /// <summary>
    /// Returns true if the key is currently down
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public static bool KeyDown(Keys key)
    {
        if(key == Keys.Space && spaceBlocked) return false;
        return Keyboard.GetState().IsKeyDown(key);
    }
    /// <summary>
    /// Returns true for a full keyStroke press,key was pressed and released.
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public static bool KeyStrokeFull(Keys key)
    {   
        return Keyboard.GetState().IsKeyUp(key) && previous.IsKeyDown(key);
    }
    public static void update()
    {
        previous = Keyboard.GetState();
        if(!Keyboard.GetState().IsKeyDown(Keys.Space) && spaceBlocked)
            spaceBlocked = false;
        if(inputBlocked)
        {
            blockTimer--;
            if(blockTimer == 0)
                inputBlocked = false;
        }
    }
}
