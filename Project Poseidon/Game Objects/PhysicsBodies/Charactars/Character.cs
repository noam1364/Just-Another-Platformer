using Project_Poseidon;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System;
using static BossLevel;
using static Global;
using static CurrentLevel;

public abstract class Character : PhysicsBody
{   //TODO:minimize 'updatePrevLoc()' to one call each game cycle
    #region data
    ///enum for each drawMode possible
    protected enum CharacterMode { Stand, Walk, Melee, Attack, Hurt };

    ///time in anim cycles that 'hurt' animation is displayed
    private const int hurtAnmationLifespan = 
        (int)((Enemy.enemyAttackInputDelay*Global.fps)/Drawable.framesPerSprite);

    ///dictionary that specifies each animation types lifespan in cycles | each cycle is a full pass on the tex[],with 10 tics for frame
    private static Dictionary<CharacterMode, int> drawModeLifespans =  
        new Dictionary<CharacterMode, int>() { { CharacterMode.Stand,-1 }, { CharacterMode.Walk,-1 },{CharacterMode.Melee,1 },
            {CharacterMode.Attack,1 },{CharacterMode.Hurt,hurtAnmationLifespan } };

    private static Dictionary<Type, int> MaxHealth = new Dictionary<Type, int>()
    {{ typeof(Player),10},{typeof(Slime),3},{typeof(Zombie),9 },{typeof(Snowman),9 },{typeof(Boss),9 } };

    private const float timetoHealthReGen = 3f;
    private const int healthRegenRate = 1;  ///in health points
    protected int health;
    private CharacterMode mode;
    #endregion data
    #region ctor
    public Character(Platform pl,dir facing):base()
    {
        if(pl!=null)
        {
            setPosXCenter(pl.XCenter());
            setPosBottom(pl.Y());
        }
        health = getMaxHealth();
        if(facing == dir.left)
            flipMovement();
        updatePrevLoc(); ///for safety
    }
    #endregion ctor
    #region logic
    public override void update()
    {
        base.update();
        if(health <= 0 && !getFreeFall() && canSwitchAnim())
            die();///character dies only on land
        if(gameTime % getTics(timetoHealthReGen) == 0 && health < getMaxHealth()&&health>0)
        {
            health += healthRegenRate;   ///health regen
            if(health > getMaxHealth()) health = getMaxHealth();
        }
    }
    #endregion logic
    #region actions
    protected virtual void flipMovement()
    {
        flipVx();
        flipSprite();      ///flipSprite sprite
    }
    public void jump(int v0y)
    {
        if(!getFreeFall())
        {
            setVy(v0y);
            setCharacterMode(CharacterMode.Stand);
        }
    }
    public override void setVy(int vy)
    {
        base.setVy(vy);
        setCharacterMode(CharacterMode.Stand);
    }
    protected virtual void die()
    {   ///start dying procces of a Character
        kill();
        health = 0;
        string assetUrl = getTypeToString() + "/" + "Die";
        new Animation(AssetManager.getSprite(assetUrl), getPosition(), getEffects(),false,referenceFrame:this.referanceFrame);
        MediaManager.playSoundEffect(assetUrl);
        //possible scoring
    }
    protected virtual void takeDamage(int damage)
    {
        if(health > 0)    ///if Charcter alradey dead,dont start another hurt animation,
            setCharacterMode(CharacterMode.Hurt); ///in which case 'hurt' animation wont interrupnt the die animation,and is allowed
        health += damage;
        MediaManager.playSoundEffect(this.GetType().ToString()+CharacterMode.Hurt.ToString());
    }
    protected override void actionCollSea()
    {
        MediaManager.playSoundEffect("Drown");///universal SoundEffect 
        health = 0;///character is dead
        kill();
        new DrowmAnimation(AssetManager.getSprite(this.GetType().ToString() + "/Drown"), getPosition(), getEffects(), false);
    }
    public virtual void attackedBy(IAttacker attacker,dir dir = 0)
    {   ///allows an object to attack a character ('this')
        Point recoil = new Point();
        bool hasRecoil = true;
        try
        {
            recoil = attacker.getRecoil();
        }
        catch
        {
            hasRecoil = false;
        }
        
        if(hasRecoil)
        {
            setVx(recoil.X * (int)dir);
            setVy(recoil.Y);
        }
        takeDamage(attacker.getDamage());
    }
    #endregion actions
    #region get&set
    /// <summary>
    /// returns true if the character has health greater then 0
    /// </summary>
    /// <returns></returns>
    public bool isAlive()
    {
        return health >= 0;
    }
    public dir getDir()
    {
        return getSpriteDir();
    }
    public int getV0xAdj()
    {
        return getV0xVal() * (int)getDir();
    }
    public int getHealth()
    {
        return health;
    }
    public virtual int getMaxHealth()
    {
        return MaxHealth[this.GetType()];
    }
    protected bool isAttackAnimActive()
    {
        return mode == CharacterMode.Attack || mode == CharacterMode.Melee;
    }
    protected CharacterMode getCharacterMode()
    {
        return mode;
    }
    protected virtual void setCharacterMode(CharacterMode m) 
    {   ///walk must not be interrupted with another walk,it ruines the animation                          
        if(this.mode == CharacterMode.Walk && m == CharacterMode.Walk) return; 
        string texUrl = getTypeToString() + "/" + m.ToString();
        bool canBeInterrupted = (m == CharacterMode.Attack || m == CharacterMode.Melee);
        ///only attack and melee can be interrupted in the middle,and only be Hurt animation
        if(setTexture(AssetManager.getSprite(texUrl), drawModeLifespans[m], canBeInterrupted))
            mode = m;
    }
    #endregion get&set
}
