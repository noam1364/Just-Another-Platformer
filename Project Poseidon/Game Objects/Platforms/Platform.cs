using Microsoft.Xna.Framework;
using System.Collections.Generic;
using static CurrentLevel;
using System;

public class Platform : GameObject
{
    private bool specialCo; ///if true,Coefficient of friction is ground specific,otherwise it is the default fkCo
    private static Dictionary<LevelType, float> FkCoef = new Dictionary<LevelType, float>() 
    { {LevelType.Ground,1.8f },{LevelType.Ice,1.8f } }; ///kinetic friction coeffitients of normal friction platforms
    private static Dictionary<LevelType, float> SpecialFkCoef = new Dictionary<LevelType, float>()
    { {LevelType.Ground,1.8f },{LevelType.Ice,0.6f } }; ///kinetic friction coeffitients of special friction platforms
    #region ctor    
    public Platform(int x, int y, int width, int height,bool specialCo=false) :
        this(new Rectangle(x, y, width, height),specialCo)
    {
       
    }
    public Platform(Rectangle rect, bool specialCo = false) : base(rect)
    {
        this.specialCo = specialCo;
    }
    public Platform()
    {

    }
    #endregion ctor
    #region logic
    public override void update()
    {
        base.update();
    }
    /// <summary>
    /// Adds object to the platform's update list
    /// </summary>
    /// <param name="obj"></param>
    public virtual void addObjToPlat(GameObject obj)
    {

    }
    #endregion logic
    public virtual Vector2 getV()
    {
        return new Vector2(0,0);
    }
    public virtual Vector2 getDeltaPos()
    {
        return getV();
    }
    public float getFrictionCo()
    {   ///A switch statement will only add unnecesarry code
        if(!specialCo) return FkCoef[levelType];
        return SpecialFkCoef[levelType];
    }
    public float getRegularFrictionCo()
    {
        return FkCoef[levelType];
    }
}

  
