using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_Poseidon;
using static Global;

public class PauseMenu : Menu
{
    private static GameState stateToreturnTo;
    public PauseMenu(GameState s = GameState.gameOn)
    {
        stateToreturnTo = s;
        bg.setPosition(new Point(winWidth / 2 - bg.width() / 2, (winHeight - bg.height()) / 2));
        finger.setRotation(0);
        finger.setPosition(new Point(750, 410));
        dy = 200;
        dx = 0;
        if(!(this is LevelFinishMenu || this is GameOverMenu))
            PlayFingerChooseSound();
    }
    protected override void actionSpace()
    {
        switch (fingerPos.Y)
        {
            case (0):
                setState(stateToreturnTo);     ///right back to the game from where it stopped
            break;

            case (1):
                Global.game.startNewLevel(CurrentLevel.levelNum);  ///restart level
            break;

            case (2):
                switchToLevelMenu();    ///exit level back to level menu
            break;
        }
        PlayFingerChooseSound();
    }
    protected override void actionEscape()
    {
        setState(stateToreturnTo);   ///right back to the game from where it stopped
        PlayFingerChooseSound();
    }
    public static GameState getStateToReturnTo()
    {
        return stateToreturnTo;
    }
}
