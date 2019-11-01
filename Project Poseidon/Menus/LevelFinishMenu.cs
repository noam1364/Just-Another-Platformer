using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Media;
using static Global;
using System;

public class LevelFinishMenu : PauseMenu
{
    private Drawable[] stars;
    private int star;
    public LevelFinishMenu(int star)
    {   ///load the star sprites and ititiate their positions
        this.star = star;
        stars = new Drawable[3];
        for(int i = 0;i < stars.Length;i++)
            stars[i] = new Drawable(AssetManager.getSprite("EmptyStar"),new Point(0,0));
        stars[1].setPosXCenter(bg.XCenter());
        stars[1].setPosTop(CurrentLevel.bg.Y()+20);
        for(int i=0;i<stars.Length;i+=2)
            stars[i].setPosYCenter(stars[1].YBottom());
        stars[0].setPosRight(stars[1].X());
        stars[2].setPosLeft(stars[1].XRight());
    }

    protected override void actionSpace()
    {
        switch(fingerPos.Y)
        {
            case (0):
                CurrentLevel.loadLevel(CurrentLevel.levelNum+1);    ///next level
            break;

            case (1):
                CurrentLevel.loadLevel(CurrentLevel.levelNum);  ///restart level
            break;

            case (2):
                switchToLevelMenu(CurrentLevel.levelNum+1);    ///exit level back to level menu,point at the next level
            break;
        }
        PlayFingerChooseSound();
    }
    protected override void actionEscape()
    {
        PlayFingerChooseSound();
        switchToLevelMenu();
    }
    public override void draw()
    {
        base.draw();
        for(int i=0;i< star;i++)
            stars[i].setTexture(AssetManager.getSprite("FullStar"));
        for(int i = 0;i < stars.Length;i++)
            stars[i].draw();
    }
}
