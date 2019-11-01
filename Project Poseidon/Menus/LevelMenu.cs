using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using Project_Poseidon;

public class LevelMenu : Menu 
{
    private Drawable lockIcon;
    /// <summary>
    /// Creates a new LevelMenu menu
    /// </summary>
    /// <param name="level">The number of the level of which the finger should initially point to</param>
    public LevelMenu(int level = 1)
    {
        finger.setPosition(new Point(114, 360));
        lockIcon = new Drawable(AssetManager.getSprite("LockIcon"),new Point(0,0),0.05f);
        dx = 300;
        dy = 250;
        finger.setRotation(-90);
        if(level <= 0) level = 1;   ///protection
        //temp:
        if(level % 6 == 0)
            fingerPos = new Point((level - 1) % 6, level / 6 - 1);
        else
            fingerPos = new Point(level%6-1, level/6);    ///get position of finger by the last level played
        Point bounds = getBounds();
        if(fingerPos.X > bounds.X) fingerPos.X = bounds.X;
        if(fingerPos.Y > bounds.Y) fingerPos.Y = bounds.Y;
        finger.addPosX(fingerPos.X * dx);finger.addPosY(fingerPos.Y*dy);  
    }
    public override void drawIcons()
    {
        base.drawIcons();
        List<Global.ProgressMode> progress = Global.getProgress();
        Point bounds = getBounds(), pos0 = new Point(155,190),gridPos;
        ///pos0 --> the X is center the Y is Top
        for(int i=0;i<progress.Count;i++)
        {
            int level = i + 1;
            if(level % 6 == 0)
                gridPos = new Point((level - 1) % 6, level / 6 - 1);
            else
                gridPos = new Point(level % 6 - 1, level / 6);
            
            if(progress[i] ==Global.ProgressMode.Locked)
            {
                lockIcon.setPosXCenter(pos0.X);
                lockIcon.setPosTop(pos0.Y);
                lockIcon.addPosX(gridPos.X * dx); lockIcon.addPosY(gridPos.Y * dy);
                lockIcon.draw();
            } 
            else if(progress[i]!=Global.ProgressMode.Open)    ///if only open,there is no star display
            {
                int middleStarOffset = 20;
                Point starPos = new Point(pos0.X + gridPos.X * dx, pos0.Y + gridPos.Y * dy);
                Project_Poseidon.Static_Classes.HudManager.drawStarIcons((int)progress[i],starPos,middleStarOffset);
            }  
        }
    }
    protected override void actionSpace()
    {
        int chosenLevel = fingerPos.Y * 6 + fingerPos.X + 1; ///6 levels in a row 
        //if(Global.getProgress()[chosenLevel - 1] !=Global.ProgressMode.Locked)
       // {
            PlayFingerChooseSound();
            Global.game.startNewLevel(chosenLevel);  
       // }
       // else
       //     PlayFingerStuckSound();
    }
    protected override void actionEscape()
    {
        PlayFingerChooseSound();
        Global.game.setMenu(new StartMenu());
    }
    protected override void actionT()
    {
        CurrentLevel.loadLevel(-999);
    }
}
