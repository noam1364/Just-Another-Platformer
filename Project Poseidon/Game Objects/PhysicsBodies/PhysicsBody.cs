using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using static Global;
using static CurrentLevel;
using System.Collections.Generic;
using static BossLevel;

public abstract class PhysicsBody : Drawable 
{
    #region data
    public const int g = 2;                 ///Free Fall Acceleration
    private int vx, vy;                     /// px/frame
    protected bool freeFall;
    private Rectangle prevPos;  /// prevPos = X(), XRight(), Y(), YBottom()
    protected Platform referanceFrame;    ///if body is on a MovingPlatform,save it as the platforms referanceFrame
    #endregion data
    #region ctor
    public PhysicsBody(Point pos):base(pos)
    {
        init();
    }
    public PhysicsBody() : base()
    {
        init();
    }
    private void init()
    {
        vx = 0;
        vy = 0;
        freeFall = false;   ///because 'kinematics' is called first | bodies must get a chance to be on a Platform
        prevPos = new Rectangle(X(), XRight(), Y(), YBottom());
    }
    #endregion ctor
    #region logic
    public override void update()
    {   //order of method calls MATTERS
        base.update();
        kinematics();
        collitionManager();
        updatePrevLoc();
    }
    private void kinematics()
    {
        ///adjust movement for given reference frame,if there is any
        if(referanceFrame != null)
        {
            addPosX((int)referanceFrame.getV().X);
            addPosY((int)referanceFrame.getV().Y);
        }
        ///y axis movement
        float fkCO = getFrictionCo();   ///current kinetic friction coeffitient
        if(freeFall)
        {
            referanceFrame = null;  ///if body is in freeFall,it is on no platform
            addPosY(vy);
            vy += g;
        }
        ///x axis movement
        addPosX(vx);
        int dir = Math.Sign(vx), ax = (int)(fkCO * g); ///Newton's second law:F=ma-->N*fkCo=m*a --> m*g*fkCo=m*a --> a=g*fkCo;
        vx += ax * dir * -1; ///friction accelerates againt current motion;
        if(Math.Sign(vx) != dir) vx = 0; ///player cannot slow down beyond vx = 0 | if the direction was flipped, vx = 0
    }
    protected virtual void collitionManager()    ///detects collition of the char with platforms and bounds
    {
        ///platform collition:
        freeFall = true;    ///assuming freeFall-->true, unless collition checking foreach loop shows otherwise
        bool notPlayerOrBoss = !this.Equals(player)&&!(this is BossLevelBotTrainer.BossBot)&&!(this is Boss);
        foreach(GameObject obj in objects)
        {
            ///Collitions--> Platform&Cannon:everyone | Obstacle:!player --> player-obst is handled seperately
            if(obj is Platform || obj is Cannon || obj is Obsticle && notPlayerOrBoss )
                collitionDetector(obj); 
        }
        ///window bounds collition:
        if(Y() < 0)    ///top part of the screen
        {
            setPosTop(0);
            vy = 0;
        }
        else  
        {
            if(sea!=null)   ///for safety
            {
                if(intersects(CurrentLevel.sea))
                    actionCollSea();
            }
        }
           ///bottom of the screen
        if(XRight() > winWidth) setPosRight(winWidth); ///right
        else if(X() < 0) setPosLeft(0);             ///left
    }
    private void collitionDetector(GameObject body)
    {
        if(intersects(body))
        {
            if(body.getPrevYBottom() <= getPrevY())  ///collition from bottom of body 
                actionCollFromBottom(body);
            else
            {
                if(body.getPrevY() >= getPrevYBottom()) ///collition from top of body
                    actionCollFromTop(body);
                else
                {
                    if(body.getPrevXRight() <= getPrevX())    ///collition from the right side of the body
                        actionCollFromRight(body);
                    else
                    {
                        if(getPrevX() <= body.getPrevXRight()) ///collition from the left side of the body
                            actionCollFromLeft(body);
                    }
                }
            }
        }
        else if(YBottom() == body.Y()&&getPrevYBottom()==body.getPrevY()&&XRight()>=body.X()&&X()<=body.XRight())
        {///this stands on body 
            actionCollFromTop(body);
        }
    }
    #endregion logic
    #region collition actions
    protected virtual void actionCollFromRight(GameObject body)
    {   ///collition from right side of body
        setPosLeft(body.XRight());
        vx = 0;
    }
    protected virtual void actionCollFromLeft(GameObject body)
    {   ///collition from the left side of body
        setPosRight(body.X());
        vx = 0;
    }
    protected virtual void actionCollFromBottom(GameObject body)
    {       ///collition from the bottom of body
        if(vy < 0) setVy(0); ///so that 'this' can gain speed if hits a verticallyMovingPlatform from below
        setPosTop(body.YBottom());
    }
    protected virtual void actionCollFromTop(GameObject body)
    {   ///collition from the top of body
        vy = 0;
        freeFall = false;
        setPosBottom(body.Y());
        if(body is Platform)
            referanceFrame = (Platform)body;
    }
    protected virtual void actionCollSea()
    {

    }
    #endregion collition actions
    #region get&set&utilities
    public void setVx(int v0x)   ///make the char start moving
    {
        //updatePrevLoc();
        vx = v0x;
    }
    public virtual void setVy(int v0y)   
    {
        referanceFrame = null;
        freeFall = true;
        vy = v0y;
    } 
    public bool getFreeFall()
    {
        return freeFall;
    }
    public int getVx()
    {
        return vx;
    }
    public dir getDirX()
    {
        return (dir)Math.Sign(vx);
    }
    private float getFrictionCo()
    {
        if(freeFall||referanceFrame==null) return this.getDragCoef();
        if(this is Player)  ///only player is affected by different surface type
            return referanceFrame.getFrictionCo();
        return referanceFrame.getRegularFrictionCo();
    }
    public override int getPrevX()
    {
        return prevPos.X;
    }
    public override int getPrevXRight()
    {
        return prevPos.Width;
    }
    public override int getPrevY()
    {
        return prevPos.Y;
    }
    public override int getPrevYBottom()
    {
        return prevPos.Height;
    }
    protected void updatePrevLoc()      ///method that updates 'prevPos',for collition detection perposes
    {
        prevPos.X = X();
        prevPos.Width = XRight();
        prevPos.Y = Y();
        prevPos.Height = YBottom();
    }
    protected void flipVx()
    {
        vx = -vx;
    }
    #endregion get&set 
}
