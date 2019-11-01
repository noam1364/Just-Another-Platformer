using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Media;
using Project_Poseidon;
using System.Collections.Generic;

public class StartMenu : Menu
{
    public StartMenu()
    {
        finger.setPosRight(620);
        finger.setPosYCenter(376);
        finger.setRotation(0);
        dy = 189;
        if(Global.isThereASavedGame())
            fingerPos.Y = 1;
        else
            fingerPos.Y = 0;
        finger.addPosY(fingerPos.Y * dy);
    }
    protected override void actionSpace()
    {
        if(fingerPos.Y == 0)    ///New Game
        {
            Global.initProgress(); ///reLock all of the unlock levels - start a new game
            switchToLevelMenu(1);
            PlayFingerChooseSound();
        }
        else
        {
            if(Global.isThereASavedGame())  ///if there is a saved game with actual progress,continue it
            {
                switchToLevelMenu(Global.lastOpenLevel());
                PlayFingerChooseSound();
            }
            else
                PlayFingerStuckSound(); ///if the user pressed 'Continue' but there is no saved game to continue
        }
    }
    protected override void actionEscape()
    {   //Exit Game to be implemented
        
    }
}
