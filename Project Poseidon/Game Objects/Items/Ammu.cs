using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


public class Ammu : Item
{
    private const int bulletsToGive = 3;
    public Ammu(Platform pl, int offset = 0):base(AssetManager.getSprite("Ammu"),pl,offset)
    {

    }
    public Ammu(Point pos) : base(AssetManager.getSprite("Ammu"),pos)
    {

    }
    protected override void action()    
    {
        base.action();
        CurrentLevel.player.giveBullets(bulletsToGive);
        CurrentLevel.RemoveFromGame(this);
    }
}
