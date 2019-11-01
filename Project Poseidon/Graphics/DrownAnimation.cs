using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using static Global;
using System;

public class DrowmAnimation:Animation
{
    private static int drownSpeed = 3;
	public DrowmAnimation(Texture2D[] tex, Point loc, SpriteEffects e = SpriteEffects.None, bool locByCenter = true
        , bool updateInGameOverSeq = false) :base(tex,loc,e,locByCenter,updateInGameOverSeq)
    {

    }
    public override void draw() ///has update functionality,for smoothness and simplicity
    {
        base.draw();
        if(!toUpdate()) return;
        if(Y() <= winHeight)    ///drown to the bottom
            addPosY(drownSpeed);
        else
            remove();
    }
    protected override void animEndReached()
    {   ///drownAnimation doesn't end when idx reached the last frame --> only when it sunk to the bottom of the sea
        idx = 0;
    }
}
