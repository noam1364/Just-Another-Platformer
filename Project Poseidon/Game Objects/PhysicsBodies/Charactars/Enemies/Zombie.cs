using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using static CurrentLevel;
using static Global;

public class Zombie : Enemy
{
    private const int chaseSpeedMultiplier = 2;
    ///when trying to jump exactly when zombie hit an Object from the side,the zombie is in the middle
    ///of collition cheking in PhysicsBody and for 2 long reasons this might interups jumping,so it is delayed
    private bool jumpNextCycle;   
    public Zombie(Platform pl, dir facing = dir.right, int offset = 0) :
        base(pl, offset, facing)
    {   ///(600,50-->visionDims | (3,-15)-->v0 | 15-->MaxHealth 
        jumpNextCycle = false;
        flipMovWhenPosible = false;
    }
    protected override void AI()
    {
        if(wasHit)
            flipMovWhenPosible = true;
        if(jumpNextCycle)
            jump(getV0y());
        jumpNextCycle = false;
        dir dirToPlayer = getDirToPlayer(), boundHited = hitMovementBound();
        if(!getFreeFall())
        {
            if(boundHited != 0)  ///if zombie hit an edge of his movement range boundery
            {
                if(!isPlayerInClearSight())  ///if zombie hit a bound but the player is in sight,stay there and wait for the player
                    flipMovement();
                else ///if zombie is at bound and sees player on the other side,stay and jump
                {
                    if(boundHited == dir.left) setPosLeft(range[0]);
                    else setPosRight(range[1]);
                    jumpNextCycle = true;
                }
            }
            if(!jumpNextCycle)
            {
                int v = getV0xAdj();
                bool playerInSight = isPlayerInClearSight();
                if(playerInSight) v *= chaseSpeedMultiplier;///chase player if sees it
                setVx(v);
                if(flipMovWhenPosible)  ///for cases when zombie was hit in the back,needs to flip as he lands back
                {
                    if(getDir() != dirToPlayer)
                        flipMovement(); ///flip only if truly neccecary
                    flipMovWhenPosible = false;
                }
            }
        }
        tryAttack();
    }
    protected override void actionCollFromRight(GameObject body)
    {
        base.actionCollFromRight(body);
        actionCollFromSide(body, dir.right);
    }
    protected override void actionCollFromLeft(GameObject body)
    {
        base.actionCollFromLeft(body);
        actionCollFromSide(body, dir.left);
    }
    private void actionCollFromSide(GameObject body, dir dir)
    {                           ///collition from 'dir' side of an object
        dir otherDir;
        if(dir == dir.left) otherDir = dir.right;
        else otherDir = dir.left;
        if(getSpriteDir() == otherDir)  ///if there is and obsticle ,turn around unless zombie sees the player
        {                               ///at the other side of the obsticle
            if(!isPlayerInClearSight())  ///if zombie hit a bound but the player is in sight,stay there and wait for the player
                flipMovement();                         ///otherwise,flip
            else
            {
                jumpNextCycle = true;
            }
        }
    }
}
