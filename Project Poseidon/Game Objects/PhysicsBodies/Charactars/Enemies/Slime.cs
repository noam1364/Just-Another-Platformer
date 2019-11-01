using Microsoft.Xna.Framework;
using static AssetManager;
using static CurrentLevel;
using static Global;

public class Slime : Enemy
{
    public Slime(Platform pl, dir facing = dir.right, int offset = 0) :
        base(pl, offset, facing)
    {      ///3-->MaxHealth | (1,0)-->v0
        flipOrigin();   ///sprites default direction is right and default origin is left
    }
    protected override void AI()
    {   ///A basic AI that includes const movement on xAxis and collition checking with player and bounds
        movementManager();
        tryAttack();
    }
    private void movementManager()
    {
        setVx(getV0xAdj());
        if(X() <= range[0] || XRight() >= range[1])  ///if enemy hit an edge of his movement range,flipSprite movement
        {
            bool hitLeftBound = X() <= range[0] && getDirX() == dir.left,
            hitRightBound = XRight() >= range[1] && getDirX() == dir.right;
            if(hitLeftBound || hitRightBound) flipMovement();
            ///allow the character 1 game cycle in the edge of the range
            if(hitLeftBound)
                setPosLeft(range[0]);
            else if(hitRightBound)  ///'if' statement is neccecary because enemy might by exactly on the edge 
                setPosRight(range[1]);
        }
    }
    protected override void flipMovement()
    {
        base.flipMovement();
        flipOrigin();   ///for drawing perposes
    }
    protected override void actionCollFromRight(GameObject body)
    {   ///collition from the right of body
        base.actionCollFromRight(body);
        range[0] = body.XRight();
    }
    protected override void actionCollFromLeft(GameObject body)
    {   ///collition from the left of body
        base.actionCollFromLeft(body);
        range[1] = body.X();
    }
}
