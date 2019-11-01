using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using static BossLevel;
using static CurrentLevel;

public class GameObject ///represents the object appering in the game,has a location and size
{
    ///The recoil for each sub-class of type IAttacker
    private static Dictionary<Type, Point> Recoil = new Dictionary<Type, Point>()
    { {typeof(Slime),new Point(15, -20) },{typeof(Zombie),new Point(30, -10) },{typeof(Snowman),new Point(0,0) },
        {typeof(Bullet),new Point(30,-20) },{typeof(Snowball),new Point(20,-10) },{typeof(Spike),new Point(15,-25)},
        {typeof(Tree),new Point(20,-15) } };

    ///V0 for different sub-classes of game objects
    private static Dictionary<Type, Point> V0 = new Dictionary<Type, Point>
    {{typeof(Player),new Point(15,-32)},{typeof(Slime),new Point(1, 0)},{typeof(Zombie),new Point(3, -15) },{typeof(Snowman),new Point(2, 0)},
        {typeof(Bullet),new Point(60,0) },{typeof(Snowball),new Point(50,-30) } };

    ///Damage for each sub-class of IAttacker
    private static Dictionary<Type, int> Damage = new Dictionary<Type, int>()
    { {typeof(Slime),-3 },{typeof(Zombie),-5 },{typeof(Snowman),0 } ,
        {typeof(Bullet),-5 },{typeof(Snowball),-2 },{typeof(Spike),-2},{ typeof(Tree),-2},{typeof(Boss),-5 } };

    private static Dictionary<Type, float> DragCoef = new Dictionary<Type, float>() { {typeof(Character),0.5f },{typeof(Snowball),0.1f } };

    private Rectangle rect;
    #region ctor
    public GameObject()
    {
        if(this.rect==null)
            this.rect = new Rectangle(0, 0, 0, 0);
        //CurrentLevel.AddToGame(this);
    }
    public GameObject(Rectangle rect) : this()
    {
        this.rect = rect;
    }
    public GameObject(Point loc,Point dims):this()
    {
        this.rect = new Rectangle(loc, dims);
    }
    public GameObject(int x,int y,int w,int h):this()
    {
        this.rect = new Rectangle(x,y,w,h);
    }
    #endregion ctor
    #region add&set methods
    public void addPosX(int n)
    {
        rect.Offset(n,0);
    }
    public void addPosY(int n)
    {
        rect.Offset(0,n);
    }
    public virtual void setPosTop(int y)
    {
        rect.Location = new Point(rect.X, y);
    }
    public virtual void setPosLeft(int x)
    {
        rect.Location = new Point(x, rect.Y);
    }
    public virtual void setPosBottom(int y)
    {
        rect.Location = new Point(rect.X, y-rect.Height);
    }
    public virtual void setPosRight(int x)
    {
        rect.Location = new Point(x - rect.Width, rect.Y);
    }
    public virtual void setPosXCenter(int x)
    {
        rect.Location = new Point(x - rect.Width / 2, rect.Y);
    }
    public virtual void setPosYCenter(int y)
    {
        rect.Location = new Point(rect.X, y - rect.Height / 2);
    }
    public void setPosition(Point loc)
    {
        rect.Location = loc;
    }
    public void setRect(Rectangle rect)
    {
        this.rect = rect;
    }
    public void setDimentions(int w,int h)
    {
        rect = new Rectangle(rect.X, rect.Y, w, h);
    }
    public void setWidth(int w)
    {
        this.rect.Width = w;
    }
    public void setHeight(int h)
    {
        this.rect.Height = h;
    }
    #endregion add&set methods
    #region get methods
    public int X()
    {
        return rect.X;
    }
    public int XRight()
    {
        return rect.Right;
    }
    public int XCenter()
    {
        return rect.Center.X;
    }
    public int Y()
    {
        return rect.Y;
    }
    public int width()
    {
        return rect.Width;
    }
    public int height()
    {
        return rect.Height;
    }
    public int YBottom()
    {
        return rect.Bottom;
    }
    public int YCenter()
    {
        return rect.Center.Y;
    }
    public virtual Rectangle GetRect()  ///return a rectangle with cords of drawable and its relative dimentions
    {
        return rect;
    }
    public Point getPosition()
    {
        return rect.Location;
    }
    public Point getCenterPosition()
    {
        return new Point(XCenter(), YCenter());
    }
    public Vector2 getCenterPositionVec()
    {
        return new Vector2((float)XCenter(),(float)YCenter());
    }
    public virtual int getPrevX()
    {
        return X();
    }
    public virtual int getPrevXRight()
    {
        return XRight();
    }
    public virtual int getPrevY()
    {
        return Y();
    }
    public virtual int getPrevYBottom()
    {
        return YBottom();
    }
    public Point[] getIndices()
    {
        return new Point[] { getPosition(), new Point(XRight(), Y()),new Point(XRight(),YBottom()),
        new Point(X(),YBottom())};
    }
    new public virtual Type GetType()
    {
        return base.GetType();
    }
    public virtual string getTypeToString()
    {
        return GetType().ToString();
    }
    #endregion get methods
    public virtual bool intersects(GameObject obj)
    {
        if(obj == null) return false;
        return rect.Intersects(obj.rect);
    }
    public virtual bool intersects(List<GameObject>lst)
    {
        if(lst == null) return false;
        foreach(GameObject obj in lst)
        {
            if(intersects(obj)) return true;
        }
        return false;
    }
    public virtual bool intersects(Rectangle rec)
    {
        return rect.Intersects(rec);
    }
    public bool contains(GameObject obj)
    {
        return this.GetRect().Contains(obj.GetRect());
    }
    public bool contains(Vector2 p)
    {
        return rect.Contains(p);
    }
    public bool contains(Point p)
    {
        return rect.Contains(p);
    }
    public virtual void update()    
    {

    }
    public virtual void draw()
    {

    }
    protected virtual void kill()
    {
        CurrentLevel.RemoveFromGame(this);
    }
    public struct SDrawable///contains all Drawable info of the platform | c# doesnt allow multiple inheratance
    {               //TEMPORARY IMPLIMENTATION OF DRAWABLE AND PLATFORM MULTIPLE INHERATANCE
        public Texture2D tex;
        public SpriteEffects effects;
        public float rotation;
    }

    public interface IAttacker
    {
        Point getRecoil();
        int getDamage();
    }
    public Point getRecoil()
    {
        return Recoil[GetType()];
    }
    public int getDamage()
    {
        return Damage[GetType()];
    }
    public int getV0xVal()  ///get vox adjusted for the direction
    {
        return V0[GetType()].X;
    }
    public int getV0y()
    {
        return V0[GetType()].Y;
    }
    public Point getV0()
    {
        return V0[GetType()];
    }
    public float getDragCoef()
    {
        Type t = this.GetType();
        float coef = 0f;
        while(true)
        {
            if(DragCoef.TryGetValue(t, out coef))
                return DragCoef[t];
            else
                t = t.BaseType;
        }
    }
}
