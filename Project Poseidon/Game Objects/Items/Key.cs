using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


public class Key : Item
{
    public enum KeyType { Normal,Ice};
    public Key(Point pos) : base(AssetManager.getSprite("Key"),pos)
    {
        CurrentLevel.key = this;
    }
    public Key(Platform pl,int offset=0) : base(AssetManager.getSprite("Key"),pl,offset)
    {
        CurrentLevel.key = this;
    }
    ~Key()
    {

    }
    protected override void action()
    {
        base.action();
        CurrentLevel.player.giveKey();
        CurrentLevel.RemoveFromGame(this);
    }
}
