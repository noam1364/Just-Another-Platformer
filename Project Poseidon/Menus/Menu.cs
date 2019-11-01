using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Input;
using Project_Poseidon;
using static Global;
using static InputHandler;
using System.Reflection;
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;

public abstract class Menu 
{
    ///the boundery of movement of the cursor in each type of menu,from 0 to 1-NumOfSlots
    private Dictionary<Type, Point> boundDict = new Dictionary<Type, Point>()
    { {typeof(LevelMenu), new Point(5,1)},{typeof(PauseMenu),new Point(0,2) },{ typeof(GameOverMenu),new Point(0,1)},
        { typeof(LevelFinishMenu),new Point(0,2) },{typeof(StartMenu),new Point(0,1)} };
    
    protected int alpha,dy,dx;
    protected Point fingerPos;
    protected Drawable bg;
    protected Drawable finger;
    protected Drawable mute;
    private Texture2D[] muteTex;   ///muteTex[0] is muted | muteTex[1] is unmuted

    public Menu()
    {
        muteTex = AssetManager.getSprite("MuteIcon");
        bg = new Drawable(AssetManager.getSprite(GetType().ToString()), new Point(0));
        finger = new Drawable(AssetManager.getSprite("Finger"), new Point(0),0.1f);
        mute = new Drawable(muteTex[1], new Point(0,0));
        mute.setPosRight(winWidth);
        setState(this.GetType().ToString());
        this.fingerPos = new Point(0, 0);
        InputHandler.unblockInput();
    }
    public virtual void update()
    {
        if(KeyStroke(Keys.LeftShift))
            MediaManager.muteAction();
        if(KeyStroke(Keys.Space))
            actionSpace();
        else if(KeyStroke(Keys.A))
            actionA();
        else if(KeyStroke(Keys.S))
            actionS();
        else if(KeyStroke(Keys.D))
            actionD();
        else if(KeyStroke(Keys.W))
            actionW();
        else if(KeyStrokeFull(Keys.Escape))
            actionEscape();
        else if(KeyStroke(Keys.T))
            actionT();
        InputHandler.update();
    }
    public virtual void draw()
    {
        bg.draw();
        drawIcons();
        finger.draw();
    }
    public virtual void drawIcons()
    {
        Texture2D soundIconTex;
        if(MediaManager.isMuted())
            soundIconTex = muteTex[0];
        else
            soundIconTex = muteTex[1];
        mute.setTexture(soundIconTex);
        mute.draw();
    }
    protected abstract void actionSpace();
    protected abstract void actionEscape();
    protected virtual void actionA()
    {
        if(fingerPos.X > 0)  ///0-->the first slot in a row
        {
            finger.addPosX(-dx);
            fingerPos.X--;
            playFingerMovSound();
        }
        else
        {
            if(getBounds().X>0) ///if the menu contains rows
                PlayFingerStuckSound();
        }
    }
    protected virtual void actionS()
    {
        if(fingerPos.Y < getBounds().Y)
        {
            finger.addPosY(dy);
            fingerPos.Y++;
            playFingerMovSound();
        }
        else
        {
            if(getBounds().Y>0) ///if the menu contains collums
            PlayFingerStuckSound();
        }
    }
    protected virtual void actionD()
    {
        if(fingerPos.X < getBounds().X)
        {
            finger.addPosX(dx);
            fingerPos.X++;
            playFingerMovSound();
        }
        else
        {
            if(getBounds().X>0)
                PlayFingerStuckSound();
        }
            
    }
    protected virtual void actionW()
    {
        if(fingerPos.Y > 0)
        {
            playFingerMovSound();
            finger.addPosY(-dy);
            fingerPos.Y--;
        }
        else
        {
            if(getBounds().Y>0)///if the menu has collums
                PlayFingerStuckSound();
        }
            
    }
    protected virtual void actionT()
    {

    }

    protected static void playFingerMovSound()
    {
        MediaManager.playSoundEffect(AssetManager.getSoundEffect("Finger"));
    }
    protected static void PlayFingerStuckSound()
    {
        MediaManager.playSoundEffect(AssetManager.getSoundEffect("FingerStuck"));
    }
    protected static void PlayFingerChooseSound()
    {
        MediaManager.playSoundEffect(AssetManager.getSoundEffect("FingerChooseStart"));
    }
    protected static void PlayFingerChooseStartSound()
    {
        MediaManager.playSoundEffect(AssetManager.getSoundEffect("FingerChooseEnd"));
    }
    protected static void PlayFingerChooseEndSound()
    {
        MediaManager.playSoundEffect(AssetManager.getSoundEffect("FingerChooseFull"));
    }
    /// <summary>
    /// Returns the movement bound of the finger
    /// </summary>
    /// <returns></returns>
    protected Point getBounds()
    {
        return boundDict[this.GetType()];
    }
    #region menu switch methods
    /// <summary>
    /// Switches from cuurent menu to LevelMenu
    /// </summary>
    /// <param name="level">The level on which the cursor is to point at the initial state of the menu.</param>
    public static void switchToLevelMenu(int level=-1)
    {
        if(level == -1) level = CurrentLevel.levelNum;
        if(level > CurrentLevel.numOfLevels) level = 1;
        Global.game.setMenu(new LevelMenu(level));
    }
    #endregion menu switch methods
}
