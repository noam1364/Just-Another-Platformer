using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


public class Coin : Item 
{
    public Coin(Platform pl,int offset=0) : base(AssetManager.getSprite("Coin"),pl,offset)
    {
        CurrentLevel.coin = this;
    }
    public Coin(Point pos) : base(AssetManager.getSprite("Coin"),pos)
    {
        CurrentLevel.coin = this;
    }

    protected override void action()
    {
        base.action();
        CurrentLevel.player.giveCoin();
        CurrentLevel.RemoveFromGame(this);
    }

}
