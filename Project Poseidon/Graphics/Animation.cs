using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using static Global;
using System;

public class Animation : GameObject
{
    #region data
    private bool removed,updateInGameOverSeq;
    private MovingPlatform referenceFrame;
    protected int idx;
    private const int framesPerSprite = 6;
    private int[] counter;
    private bool originFlipped;
    Texture2D[] tex;
    SpriteEffects effects;
    #endregion data
    public Animation(Texture2D[] tex, Point loc, SpriteEffects e = SpriteEffects.None, bool locByCenter = true
        , bool updateInGameOverSeq=false,Platform referenceFrame=null) : base(0,0,tex[0].Width,tex[0].Height)
    {
        if(referenceFrame is MovingPlatform)
            this.referenceFrame = (MovingPlatform)referenceFrame;
        else
            this.referenceFrame = null;
        effects = e;
        this.tex = tex;
        removed = false;
        counter = new int[tex.Length];
        if(e == SpriteEffects.None) originFlipped = false;
        else originFlipped = true;
        idx = 0;
        this.updateInGameOverSeq = updateInGameOverSeq;

        if(locByCenter)
        {
            setPosXCenter(loc.X);
            setPosYCenter(loc.Y);
        }
        else
            setPosition(loc);
        if(!locByCenter)
        {
            if(XRight() > Global.winWidth) setPosRight(winWidth);
            else if(X() < 0) setPosLeft(0);
        }
        else
        {
            if(XCenter() > Global.winWidth) setPosXCenter(winWidth);
            else if(XCenter() < 0) setPosXCenter(0);
        }
        CurrentLevel.AddToGame(this);
    }
    ~Animation()
    {
        
    }
    public override void draw()
    {                   ///has update functionality for smooth drawing perpouses
        bool toUpdate = this.toUpdate();
        if(referenceFrame != null&&gameOn())    ///adjusting for moving platforms
        {
            Vector2 v = referenceFrame.getV();
            addPosX((int)v.X);
            addPosY((int)v.Y);
        }
        if(idx < tex.Length)
        {
            Vector2 pos, origin;
            origin = new Vector2(0);
            pos = new Vector2(X(), Y());
            sb.Draw(tex[idx], pos, new Rectangle(0, 0, tex[idx].Width, tex[idx].Height), Color.White, 0,origin, new Vector2(1), effects, 0);
            if(toUpdate)    
                counter[idx]++;
            if(counter[idx] >= framesPerSprite) idx++;
        }
        if(idx>=tex.Length)
            animEndReached();
    }
    protected virtual void animEndReached()
    {
        if(isMenu()||(getGameState()==GameState.GameOverSeq||updateInGameOverSeq))
        { ///an animation that runs in a game revealing menu and ends,must freeze,not die
            idx = tex.Length - 1;
            removed = true; ///animation is considered over,but not remove from lists and still being drawn by the last frame
        }
        else if(!removed)
            remove();
    }
    protected void remove()
    {
        if(!isMenu())   ///if a game revealing menu is running,no animation is allowed to die
            CurrentLevel.RemoveFromGame(this);
        removed = true;
    }
    public bool isLive()
    {                       
        return !removed;
    }
    protected bool toUpdate()
    {
        return gameOn() || (getGameState() == GameState.GameOverSeq && updateInGameOverSeq);
    }
}
