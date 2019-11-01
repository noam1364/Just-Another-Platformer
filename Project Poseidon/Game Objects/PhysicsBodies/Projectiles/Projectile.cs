using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using System;
using static Global;
using static CurrentLevel;
using System.Collections.Generic;


public class Projectile : PhysicsBody, GameObject.IAttacker
{
    protected bool isLive;  ///for collition purposes,so that after a collition bullet 'knows' not to keep damaging and moving etc.
    protected int bufferSpace = 50; ///bufferSpace:the distance from the shooter that the bullet spawns at
    private string expAnimationUrl;
    public Projectile(string exp, Point pos, dir d) : base(pos)
    {
        setTexture(AssetManager.getSprite(GetType().ToString()));
        if(d == dir.left)   ///bullet texture is faced right as default
        {
            flipSprite();
            setPosRight(pos.X - bufferSpace);
        }
        else
            setPosLeft(pos.X + bufferSpace);
        setVx(getV0xVal() * (int)getSpriteDir());
        setVy(getV0y());
        expAnimationUrl = exp;
        freeFall = true;
        
        isLive = true;
        AddToGame(this);
    }
    public void killProjectile()
    {
        isLive = false;
        if(!CurrentLevel.toRemove.Contains(this))
        {
            CurrentLevel.RemoveFromGame(this);
            new Animation(AssetManager.getSprite(expAnimationUrl), new Point(XCenter(), YCenter()));
            MediaManager.playSoundEffect(expAnimationUrl); 
        }
    }
    protected override void collitionManager()
    {
        foreach(GameObject obj in CurrentLevel.objects)    ///collition checking
        {
            if(intersects(obj) && obj != this && !(obj is Animation) && !(obj is Item))
            {
                killProjectile();
                if(obj is Character)
                {
                    ((Character)obj).attackedBy(this, getDirX());
                }
                else if(obj is Projectile)
                {
                    ((Projectile)obj).killProjectile();
                }
            }
        }
        dir d = getDirX();
        if((this.XRight() > winWidth&&d==dir.right) || (this.X() < 0&&d==dir.left))
            killProjectile();
    }
}
