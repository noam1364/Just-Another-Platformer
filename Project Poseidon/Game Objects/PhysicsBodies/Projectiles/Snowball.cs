using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using static Global;
using static CurrentLevel;
using System.Collections.Generic;


public class Snowball : Projectile
{
    public Snowball(Point pos,dir d) : base("SnowballBreak",pos,d)
    {
        setTexture(AssetManager.getSprite("Snowman/Snowball")); ///must be set manually because the word "Snowball" alone
        ///appears in more then one file folder, so AssetManager might return the wrong file
    }
}
