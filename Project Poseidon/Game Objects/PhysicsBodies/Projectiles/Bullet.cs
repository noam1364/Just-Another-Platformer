using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_Poseidon;
using static Global;
using System;

public class Bullet : Projectile
{
    public Bullet(Point pos,dir d) : base("Explosion",pos,d)
    {

    }
    public override void update()
    {
        int v = Math.Abs(getVx()),step = getVx()/v;
        for(int i=0;i<v;i++)    ///so that bullet wont skip over an object that is narrower then its Velocity
        {
            if(!isLive) return;
            addPosX(step);
            collitionManager();
        }
    }
    ~Bullet()
    {
    }
   
}
