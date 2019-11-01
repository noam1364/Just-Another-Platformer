using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;


public class Spike : Obsticle   
{
    public Spike(Platform pl, int offset = -1):base(pl,AssetManager.getSprite("Spike"),offset)
    {

    }
    
}
