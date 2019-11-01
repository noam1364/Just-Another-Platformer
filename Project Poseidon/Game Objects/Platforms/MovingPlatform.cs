using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

public class MovingPlatform : Platform,Drawable.IDrawable
{
    #region data
    public enum plSize {Small, Medium ,Large};
    protected int[] range;    ///range[0]-->Leftest the left side can go | range[1]-->rightest the right side can go
    protected const int vValue = 5;
    protected Vector2 v;
    protected Point prevPos;
    private List<GameObject> plObjs;     ///all objetcs on the platform
    private SDrawable drawable;
    #endregion data
    #region ctor
    public MovingPlatform(int y, int[] range,bool specialCo = false, plSize size = plSize.Medium) : 
        base(new Rectangle(0, 0, 0,0), specialCo)
    {
        string special = "";
        if(specialCo) special = "/Special";
        drawable.tex = AssetManager.getSprite("MovingPlatform"+"/"+size.ToString()+special)[0];
        setWidth(drawable.tex.Width);setHeight(drawable.tex.Height);
        drawable.effects = SpriteEffects.None;
        drawable.rotation = 0f;
        this.range = range;
        plObjs = new List<GameObject>();
        prevPos = getPosition();
        v = new Vector2(vValue, 0);
        ///set Real Position - pos given to Platform ctor is temporary
        setPosXCenter((range[0]+range[1])/2);
        setPosTop(y);
    }
    #endregion ctor
    #region logic
    public override void update()
    { ///***movementManager() must be called first so that characters on VerticallyMovingPlaform will stand smoothlly
        prevPos = getPosition();
        movementManager();
        List<GameObject> toKeep = new List<GameObject>();
        foreach(GameObject obj in plObjs)   ///check wich objects remained on the platform
        {
            objSharedPlatformMovement(obj);
            if(CurrentLevel.objects.Contains(obj))   
                toKeep.Add(obj);    
        }
        plObjs = toKeep;
    }
    protected virtual void movementManager()
    {
        if(X() < range[0] || XRight() > range[1])  ///if it hits an edge of his movement range,flipSprite movement
        {
            flipMovement();
        }
        addPosX((int)v.X);
    }
    protected void objSharedPlatformMovement(GameObject obj) ///handels non-PhysicsBody on the platform,
    {                                                               ///so they move with it
        obj.addPosX((int)v.X);obj.addPosY((int)v.Y);
    }
    public override void addObjToPlat(GameObject obj)
    {
        plObjs.Add(obj);
    }
    protected void flipMovement()
    {
        v.X *= -1;
        v.Y *= -1;
    }
    #endregion logic
    #region get
    public override int getPrevX()
    {
        return prevPos.X;
    }
    public override int getPrevXRight()
    {
        return prevPos.X + width();
    }
    public override int getPrevY()
    {
        return prevPos.Y;
    }
    public override int getPrevYBottom()
    {
        return prevPos.Y + height();
    }
    public override Vector2 getV()
    {
        return v;
    }
    #endregion get
    #region Interface Methods Implementation (IDrawable)
    public override void draw()
    {
        Global.sb.Draw(drawable.tex, new Vector2(X(), Y()), new Rectangle(0, 0, drawable.tex.Width, drawable.tex.Height),
            Color.White, 0, new Vector2(0, 0), new Vector2(1), SpriteEffects.None, 0);
    }
    public void flipSprite()
    {
        if(drawable.effects == SpriteEffects.None)
            drawable.effects = SpriteEffects.FlipHorizontally;
        else
            drawable.effects = SpriteEffects.None;
    }
    public void rotate(int deg)
    {
        drawable.rotation += deg;
    }
    public void setRotation(int deg)
    {
        drawable.rotation = deg;
    }
    public void flipOrigin()
    {

    }
    #endregion Interface Methods Implementation (IDrawable)
}

   


