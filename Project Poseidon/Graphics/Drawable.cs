using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using static Global;
public class Drawable : GameObject, Drawable.IDrawable
{
    #region data
    private Texture2D[] texture,prevTex;
    private float rotation,layer;
    private bool originFlipped,canBeInterrupted; /// ~ | when a Drawable need to be removed but needs to complete the animation first
    protected SpriteEffects effects;
    private int curTexIndex,curTexTimer,animationLifespan;  ///lifespan = -1 -->infinite animation (lifespan in cycles,not seconds)
    public const int framesPerSprite = 10;/// /10-->each 10 game frames (1/6 sec) the sprite shifts to next one
    #endregion data
    #region ctor
    public Drawable(Point pos,float layer = 0):this()
    {
        this.layer = layer;
        setPosition(pos);
    }
    public Drawable(Texture2D texture, Point pos,float layer = 0) : this(new Texture2D[] { texture }, pos,layer)
    {
    }
    public Drawable(Texture2D[] texture, Point pos,float layer = 0) : this()
    {   ///default constructor
        this.layer = layer;
        this.texture = texture;
        if(texture != null)
            setRect(new Rectangle(pos.X,pos.Y, texture[0].Width, texture[0].Height));
    }
    public Drawable() : base()
    {
        layer = 0;
        this.rotation = 0;
        this.effects = SpriteEffects.None;
        curTexIndex = 0;
        curTexTimer = 0;
        animationLifespan = -1;
        prevTex = texture;
        originFlipped = false;
        canBeInterrupted = true;
    }
    #endregion
    #region public methods
    public override void update()
    {
        base.update();
        curTexTimer++;
        if(curTexTimer >= framesPerSprite)
        {
            curTexTimer = 0;
            curTexIndex++;
        }
        if(curTexIndex >=texture.Length)
        {
            curTexIndex = 0;
            animationLifespan--;
            if(animationLifespan == 0)  ///when a finite Animation ends
                    setTexture(prevTex);
        }
    }
    public override Rectangle GetRect()
    {
        if(texture.Length == 1) return base.GetRect();
        else
        {
            Texture2D curTex = texture[curTexIndex];
            Rectangle rec = new Rectangle(X(), Y(), curTex.Width, curTex.Height);
            return rec;
        }
    }
    public override void draw()
    {
        if(texture == null) return;
        if(texture.Length == 1)
            sb.Draw(texture[0], new Vector2(X(), Y()), new Rectangle(0, 0, texture[0].Width, texture[0].Height), Color.White, rotation, new Vector2(0, 0), new Vector2(1), effects,layer);
        else
        {
            Vector2 origin, pos;
            Texture2D curTex = texture[curTexIndex];
            if(originFlipped)   ///if origin flipped-->calculate it
            {
                origin = new Vector2(curTex.Width, 0);
                pos = new Vector2(XRight(), Y());
            }
            else                ///else-->set origin to default value
            {
                origin = new Vector2(0);
                pos = new Vector2(X(), Y());
            }
            Rectangle rec = new Rectangle(0, 0, curTex.Width, curTex.Height);
            sb.Draw(curTex, pos, rec, Color.White, rotation, origin, new Vector2(1), effects, layer);
        }
    }
    public void flipSprite()
    {
        if(effects == SpriteEffects.None)
            effects = SpriteEffects.FlipHorizontally;
        else
            effects = SpriteEffects.None;

    }
    public void flipOrigin()
    {
        originFlipped = !originFlipped;
    }
    public void rotate(int deg)
    {
        rotation += (deg / 360f) * 2 * 3.141592653589793238462643383279f; 
    }
    #region get&set
    public void setRotation(int deg)
    {
        rotation = (deg / 360f) * 2 * MathHelper.Pi; ;
    }
    public bool setTexture(Texture2D tex, int lifespan = -1, bool canBeInterrupted = false)
    {
        return setTexture(new Texture2D[] { tex }, lifespan, canBeInterrupted);
    }
    public bool setTexture(Texture2D[] tex, int lifespan = -1,bool canBeInterrupted = false)
    {
        //if((texture == tex && animationLifespan == lifespan && this.killWhenAnimOver == killWhenAnimOver))
        //    return;
        ///only an infinite animation or a just-finishied finite animation can be interupted,unless specified otherwise
        if(!this.canBeInterrupted && animationLifespan > 0) return false;
        
        ///if allowed,set new animation:
        if(lifespan == -1) canBeInterrupted = true; ///all infinite animations can be interrupted
        else this.canBeInterrupted = canBeInterrupted;
        if(animationLifespan<=-1)   ///save current texture as a texture to return to only if it was infinite
            prevTex = texture;
        animationLifespan = lifespan;
        setRect(new Rectangle(X(), Y(), tex[0].Width, tex[0].Height));
        texture = tex;
        curTexIndex = 0;
        curTexTimer = 0;
        return true;
    }
    public dir getSpriteDir()
    {
        if(effects == SpriteEffects.FlipHorizontally)return dir.left;   ///default direction for all sprites is facing right
        return dir.right;                                              ///so flipped horizontally means left
    }
    public SpriteEffects getEffects()
    {
        return effects;
    }
    public bool canSwitchAnim()
    { 
        return animationLifespan <= 0;
    }
    #endregion get&setset 
    #endregion public methods
    public interface IDrawable
    {
        void draw();
        void flipSprite();
        void flipOrigin();
        void rotate(int deg);
        void setRotation(int deg);
    }
}
