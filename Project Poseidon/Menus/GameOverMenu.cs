using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Media;
using static Global;
using Project_Poseidon;

public class GameOverMenu : PauseMenu 
{
    public GameOverMenu()
    {
        bg.setPosition(new Point(winWidth / 2 - bg.width() / 2, (winHeight - bg.height()) / 2));
        finger.setRotation(0);
        finger.setPosition(new Point(730, bg.Y()+230));
        dy = 150;
    }
    protected override void actionSpace()
    {
        switch(fingerPos.Y)
        {
            case (0):
            Global.game.startNewLevel(CurrentLevel.levelNum);
            break;

            case (1):
            switchToLevelMenu();
            break;
        }
        PlayFingerChooseSound();
    }
    protected override void actionEscape()
    {
        switchToLevelMenu();
        PlayFingerChooseSound();
    }
}
