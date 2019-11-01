using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using static CurrentLevel;
using static Global;
using static System.Math;

public class Snowman : Enemy
{
    public Snowman(Platform pl, dir facing = dir.right, int offset = 0) :
        base(pl, offset, facing)
    {   

    }
    protected override void AI()
    {
        ///2 reasons to flip (if not facing the player) :0.75% chance that snowman will randomly flip 
        ///(if at 150px+- from player) OR player attacked the snowman
        if((wasHit&&!facesThePlayer())|| 
            (Math.Abs(player.YCenter() - YCenter()) < 150 && random.NextDouble() < 0.0075&&!facesThePlayer()))
            flipMovWhenPosible = true; 
        dir boundHitted = hitMovementBound();
        
        if(!getFreeFall()) ///snowman doesnt move in free fall
        {
            if(boundHitted!=0)
            {
                if(getCharacterMode()!=CharacterMode.Attack&&!isPlayerInClearSight())
                    flipMovWhenPosible = true;  ///if snowman hit bound,flip | unless he is shooting at the player or seeing it
                else ///if snowman hit a bound but the player is in sight,stay there and wait for the player
                {
                    if(boundHitted == dir.left) setPosLeft(range[0]);
                    else setPosRight(range[1]);
                    setCharacterMode(CharacterMode.Stand);
                }
            }
            else
            {
                if(getCharacterMode()==CharacterMode.Stand) ///if the snowman did not hit a bound but still stands
                {
                    flipMovWhenPosible = true;
                }
            }
            setVx(getV0xAdj());
            setCharacterMode(CharacterMode.Walk);
            if(flipMovWhenPosible)
            {
                flipMovement();
                flipMovWhenPosible = false;
            }
        }
        tryAttack();
    }
    protected override void tryAttack()
    {
        if(allowedToAttack() && isPlayerInClearSight())
        {
            Snowball s = new Snowball(new Point(XCenter(), Y()), getDir());
            ///data for spawning Snowball:
            Point v0 = s.getV0();
            ///dToPlayer: X>0 when Snowman is left to player | Y>0 when snowman is lower then player (lower on screen - higher Y)
            Vector2 dToPlayer = new Vector2(player.XCenter()- XCenter(), player.YCenter()-YCenter());
            float coef = 0.8f + (float)random.NextDouble() * 0.4f;  ///random number between 0.8-1.2

            ///calc: | Snowman is not very smart,so his calculations leave friction out
            float vx = v0.X * (int)getDirToPlayer(), ///valid only if equation is unsolveable
            vy = v0.Y;
            ///if player roughly at the same height as the snowman or lower(+-100px)-> throw 3x as lower
            if(Abs(dToPlayer.Y)<100||dToPlayer.Y>0) vy /= 3; 

            try
            {///find the time t until the ball will hit the player | dy = v0y*t + 0.5*g*t^2 --> 0.5*gt^2 + v0y*t - dy
                float t = MathService.solveQuadEq(0.5f * g, vy, -dToPlayer.Y)[0]; ///take the bigger solution -->so ball should hit from above
                vx = dToPlayer.X / t;    ///dx = v0x*t --> v0x = dx/t
                if(Abs(vx) > v0.X) vx = v0.X * (int)getDirToPlayer();    ///v0.X is the maximal v0x the snoman can throw at
            }
            catch { }
            s.setVx((int)(vx*coef));s.setVy((int)(vy*coef));
            dtAttack = 0;
            MediaManager.playSoundEffect(GetType().ToString()+"/Attack");
            setCharacterMode(CharacterMode.Attack);
        }
    }
    protected override void flipMovement()
    {
        if(getCharacterMode()!=CharacterMode.Attack)
            base.flipMovement();
    }
}
