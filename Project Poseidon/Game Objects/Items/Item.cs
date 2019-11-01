using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using static Global;
using static CurrentLevel;

public class Item : Drawable
{
    #region data
    #endregion data
    #region ctor
    public Item(Texture2D[] tex, Point pos) : base(tex, new Point(pos.X - tex[0].Width / 2, pos.Y - tex[0].Height / 2))
    {               ///pos-->the middle of the item

    }
    public Item(Texture2D tex,Point pos) : this(new Texture2D[] { tex},pos)
    {

    }
    public Item(Texture2D tex, Platform pl, int offset = 0) : this(new Texture2D[] { tex }, pl, offset)
    {

    }
    public Item(Texture2D[] tex,Platform pl,int offset = 0) : this(tex, new Point(pl.XCenter()+offset,pl.Y()-100))
    {            ///locate the middle of the item in the middle if the platform,and 100 px above its surface
        pl.addObjToPlat(this);
    }
    public Item()
    {

    }
    ~Item()
    {

    }
    #endregion ctor
    public override void update()
    {
        base.update();
        if(player.intersects(this))  
            action();
    }
    protected virtual void action() ///the action that happens when the player collects the item
    {
        MediaManager.playSoundEffect(GetType().ToString());
    }
}

