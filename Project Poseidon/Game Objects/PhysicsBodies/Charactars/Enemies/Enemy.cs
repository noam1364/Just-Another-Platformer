using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using static BossLevel;
using static CurrentLevel;
using static Global;
using System.Collections.Generic;
public abstract class Enemy : Character, GameObject.IAttacker
{
    #region data
    
    private static Dictionary<Type, float> AttackDelay = new Dictionary<Type, float>() { { typeof(Slime), 0.7f },{typeof(Zombie),0.5f },
        { typeof(Snowman),0.9f},{typeof(Boss),0.5f } };                                 
    
    private static Dictionary<Type, Vision> Visions = new Dictionary<Type, Vision>
    {{typeof(Slime),null },{ typeof(Zombie),new Vision(600,new int[]{-15,90})},{typeof(Snowman),new Vision(1400,80)}};

    public const float enemyAttackInputDelay = 0.35f, enemyAttackDelay = 0.7f; /// inputBlock (for player) time in sec after enemy attack 
    protected int[] range; ///range of Xcords the enemy is allowed to roam in | range[0]-->range[1] 
    protected int dtAttack;
    protected bool wasHit, flipMovWhenPosible;  ///true if character has taken damage last cycle | for AI perposes
    #endregion data
    #region ctor
    public Enemy(Platform pl,int offset = 0, dir facing = dir.right)
        : base(pl,facing)
    {   ///offset-->offset of the spawn pint from the platforms center | position gives is temporary
        dtAttack = getTics(getAttackDelay()) + 1;
        wasHit = false;
        this.range = new int[] { pl.X(), pl.XRight() };
        int midRange = (this.range[0] + this.range[1]) / 2;
        setCharacterMode(CharacterMode.Walk);
        ///check if desiered location is in the bounds,else,place enemy as close as possible to it
        setPosBottom(pl.Y());
        if(midRange + offset+width()/2 > this.range[1])
            setPosRight(range[1]);
        else if(midRange + offset - width() / 2 < this.range[0])
            setPosLeft(range[0]);
        else
            setPosXCenter(midRange + offset);
    }
    #endregion ctor
    #region logic
    public override void update()
    {   //changing the order so that 'base.update' is last needs to be considered | would enable setting speeds bigger then range
        base.update();
        if(getCharacterMode() != CharacterMode.Walk&&!getFreeFall())    ///if an enemy is not in freeFall,it should walk
            setCharacterMode(CharacterMode.Walk);
        AI();
        dtAttack++;///increment attack intrecval counter
        wasHit = false;
    }
    protected abstract void AI();
    #endregion logic
    #region actions
    protected virtual void tryAttack()  ///checks if the enemy can attack the player,and attacks if so
    {
        if(intersects(player) && allowedToAttack())
        {
            dtAttack = 0;
            if(!facesThePlayer()) flipMovement();   ///if the enemy doesnt face the player - flip towards him and recoil by the new direction
            player.attackedBy(this,getDir());
        }
    }
    protected override void takeDamage(int damage)
    {
        base.takeDamage(damage);
        wasHit = true;
    }
    protected override void actionCollFromTop(GameObject body)
    {
        GameObject prevRefFrame = referanceFrame;
        base.actionCollFromTop(body);
        if(prevRefFrame==null)
        {
            range[0] = body.X();
            range[1] = body.XRight();
        }
        else if(!referanceFrame.Equals(prevRefFrame))    ///update the bounds only if its a new reference frame
        {
            range[0] = body.X();
            range[1] = body.XRight();
        }
    }
    protected override void actionCollFromLeft(GameObject body)
    {
        base.actionCollFromLeft(body);
        if(body is Obsticle)
            range[1] = body.X();
    }
    protected override void actionCollFromRight(GameObject body)
    {
        base.actionCollFromRight(body);
        if(body is Obsticle)
            range[0] = body.XRight();
    }
    #endregion actions
    #region get&set&utilities
    protected bool allowedToAttack()
    {   ///returns the universal role for if an enemy attack is allowed
        return dtAttack >= getTics(getAttackDelay()) && getHealth() > 0 && !isAttackAnimActive();
    }
    protected virtual bool isPlayerInClearSight()
    {
        return Visions[GetType()].isPlayerInClearSight(this);
    }
    protected dir hitMovementBound()
    {
        dir boundHitted = 0;
        if(X() <= range[0] || XRight() >= range[1])  ///if enemy hit an edge of his movement range boundery
        {
            if(X() <= range[0])
                boundHitted = dir.left;
            else if(XRight() >= range[1])
                boundHitted = dir.right;
        }
        if(getDir() == boundHitted) return boundHitted;
        return 0;
    }
    public bool facesThePlayer()
    {   ///if the enemy faces right, player.x-x should be positive,otherwise it should be negetive, assuming enemy is facing the player
        return ((dir)Math.Sign(player.XCenter() - XCenter()) == getDir());
    }
    protected dir getDirToPlayer()
    {
        return (dir)Math.Sign(player.XCenter() - XCenter());
    }
    public int getVisionLength()
    {
        return Visions[this.GetType()].getLength();
    }
    private float getAttackDelay()
    {
        try { return AttackDelay[this.GetType()]; }
        catch { return 0; }
    }
    #endregion get&set&utilities
}


