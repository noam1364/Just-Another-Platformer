using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using static Global;
using Project_Poseidon;

public class Cannon : Drawable
{
    #region data
    private const float attackInterval = 1.2f;///time between shots 
    private const int spaceBetweenBulletAndCannon = 5;///in px
    protected bool isActive;    ///if true,shoots on regular intervals
    private Platform plat;
    #endregion data
    #region ctor
    public Cannon(Point pos,dir facing) : base(AssetManager.getSprite("CannonShot")[AssetManager.getSprite("CannonShot").Length-1],pos)
    {
        isActive = true;
        Texture2D sprite = AssetManager.getSprite("CannonShot")[4];
        plat = new Platform(pos.X, pos.Y, sprite.Width, sprite.Height);
        CurrentLevel.AddToGame(plat);
        if(facing == dir.left)
            flipSprite();
    }
    #endregion ctor
    #region logic
    public override void update()
    {
        base.update();
        plat.setPosition(getPosition());///if any change was made to the cannons position,its platform moves with it
        if(!isActive) return;
        if(gameTime % getTics(attackInterval) == 0 && gameTime > getTics(Global.gameBegin)) ///if an attackInterval amount of seconds has passed
            shootBullet();
    }
    public void shootBullet()
    {
        int y = YCenter() - AssetManager.getSprite("Bullet")[0].Height / 2;
        Bullet b = new Bullet(new Point(0, y), getSpriteDir());
        if(getSpriteDir() == dir.right)
            b.setPosLeft(XRight() + spaceBetweenBulletAndCannon);
        else
            b.setPosRight(X() - spaceBetweenBulletAndCannon);
        setTexture(AssetManager.getSprite("CannonShot"), 1, true);
        MediaManager.playSoundEffect("CannonShot");
    }
    #endregion logic
}
