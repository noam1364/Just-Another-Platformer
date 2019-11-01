using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using static Global;
using static CurrentLevel;
using Project_Poseidon;
using System;

public class Player : Character
{
    #region data
    private const int startBulletNum = 5, bufferBulletSpawn = 10; /// ~ | distance from player that a bullet is spawned at
    private bool wasEverHit, hasKey; 
    protected bool isMoved;
    ///for star bonus | ~ | true if player moved left/right in the current game cycle, for drawing perposes
    private int coins,bullets; 
    #endregion data
    #region ctor
    public Player(Platform pl=null) :base(pl,dir.right)  
    {   
        wasEverHit = false;
        hasKey = false;
        isMoved = false;
        bullets = startBulletNum;
        coins = 0;
        hasKey = false;
        setCharacterMode(CharacterMode.Stand);
    }
    #endregion ctor
    #region Player logic
    public override void update()
    {
        base.update();
        if(isMoved&&!freeFall)
            setCharacterMode(CharacterMode.Walk);
        else
            setCharacterMode(CharacterMode.Stand);
        isMoved = false;
        if(door.contains(this) && CurrentLevel.player.gotKey())
            enterDoor();
    }
    #endregion Player logic
    #region actions
    public void postInputMove(dir dirX)
    {
        if(getDir() != dirX) ///if movement by the input is in a new direction,flipSprite
            flipMovement();
        setVx(getV0xAdj());
        isMoved = true;
    }
    /// <summary>
    /// </summary>
    /// <param name="vy">The v0 of the jump | flag vy=0 means the player will jump the default v0y</param>
    public void postInputJump(int vy = 0)  
    {
        if(vy == 0) vy = getV0y();
        if(!getFreeFall())
            MediaManager.playSoundEffect("Player/Jump");    //to be implemnted with an enum in character class
        jump(vy);
    }
    public void shootBullet()
    {
        if(bullets>0)
        {
            Bullet b = new Bullet(new Point(0, Y() + 44), getDir());  ///44-->distance from the head to the gun of the player
            if(getDir() == dir.right)
                b.setPosLeft(XRight()+bufferBulletSpawn);
            else
                b.setPosRight(X()-bufferBulletSpawn);
            bullets--;
            MediaManager.playSoundEffect(b.GetType().ToString());
        }
    }
    protected override void takeDamage(int damage)
    {
        base.takeDamage(damage);
        InputHandler.blockInput(Enemy.enemyAttackInputDelay);
        wasEverHit = true;
    }
    protected override void actionCollSea()
    {
        kill(); ///remove from game so wont be drawen normally while gameOverSequance is running
        game.gameOverSequence(true);
    }
    protected override void die()
    {   ///when player dies,the game is over
        kill(); ///remove from game so wont be drawen normally while gameOverSequance is running
        game.gameOverSequence();
    }
    protected virtual void enterDoor()
    {
        Global.game.finishLevel();
    }
    #endregion actions
    #region get&set
    public void giveCoin()
    {
        coins++;
    }
    public int getCoins()
    {
        return coins;
    }
    public int getAmmu()
    {
        return bullets;
    }
    public void giveBullets(int x)
    {
        bullets += x;
    }
    public void giveKey()   ///when player collects level key
    {
        hasKey = true;
    }
    public bool getWasEverHit()
    {
        return wasEverHit;
    }
    public bool gotKey()
    {
        if(key == null) return false;
        return hasKey;
    }
    #endregion get&set
}
