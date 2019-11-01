using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using static CurrentLevel;
using static Enemy;

public class Obsticle : Drawable,GameObject.IAttacker    //TODO: create sub classes for each type of Obsticle
{
    #region data
    #endregion data
    #region ctor
    public Obsticle(Platform pl,Texture2D[] tex,int offset = -1) : base(tex,pl.getPosition()) ///constructor for when
    {   ///offset=-1 --> a flag that says that no offset was passed             Obsticle is built on a Platform
        pl.addObjToPlat(this);
        setPosBottom(pl.Y());
        if(offset == -1) ///if flag-->set the obst in the center of the platform
            setPosXCenter(pl.XCenter());
        else
        {
            if(offset < pl.width()/2&&offset>-pl.width()/2)
                setPosLeft(pl.X() + offset);
            else ///if offset is to big,set it at one of the edges
            {
                if(offset > 0)
                    setPosRight(pl.XRight());
                else
                    setPosLeft(pl.X());
            }
        }
    }
    public Obsticle(Platform pl,int offset = -1):this(pl,null,offset)
    {

    }
    #endregion ctor
    public override void update()
    {
        if(intersects(player))
            player.attackedBy(this,(Global.dir)Math.Sign(player.XCenter()-XCenter()));  ///direction - away from Obsticle
    }
}
