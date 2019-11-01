using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using static Global;
using static AssetManager;
using Project_Poseidon;
using System.Drawing;
using System.Reflection;
using MathNet.Numerics.LinearAlgebra;
using static BossLevel;

public static class CurrentLevel
{
    #region data
    public enum LevelType { Ground, Ice, Desert, Moon};
    public static LevelType levelType;
    public enum Surface { Air = 0, Platform = 1, Player = 5, Enemy = -20, Obstacle = -10, Target = 20, Projectile = 0 };
    
    ///for game:
    public static Drawable bg;
    public static GameObject door;   ///a rectangle that represents the end of the level | top to bottom of door sprite
    public static List<GameObject> sea;
    public static Player player;

    public static List<GameObject> objects = new List<GameObject>();     ///musr be ordered : enemy,pl,...  
    public static List<GameObject> toRemove = new List<GameObject>();
    public static List<GameObject> toAdd = new List<GameObject>();

    public static int levelNum;
    public static float starWorthyTime = 0;
    public static Coin coin;
    public static Key key;

    public const int numOfLevels = 12;
    #endregion data

    public static void AddToGame(GameObject obj)
    {
        if(obj is Player)
        {
            foreach(GameObject obj2 in objects)
            {
                if(obj2 is Player)
                    return;
            }
        }
        if(!objects.Contains(obj)&&!toAdd.Contains(obj))
            toAdd.Add(obj);
    }
    public static void RemoveFromGame(GameObject obj)   
    {
        toRemove.Add(obj);
    }
    public static bool RemovedFromGame(GameObject obj)
    {
        return toRemove.Contains(obj) || !objects.Contains(obj);
    }
    public static void sortObjList()///sort 'objects' so that the Platforms and items are first 
    {///so that PhysicsBody can update based on relevant Platform locations and Items are drawen first
     ///order : Platform,Item,Projectile,the Rest  | Drawables are first for ScrollingBackground to work properly
        List<GameObject> plats = new List<GameObject>(), items = new List<GameObject>(), proj = new List<GameObject>(),
            theRest = new List<GameObject>(),draw = new List<GameObject>();
        foreach(GameObject obj in objects)
        {
            if(obj is Platform)
                plats.Add(obj);
            else if(obj is Item)
                items.Add(obj);
            else if(obj is Projectile)
                proj.Add(obj);
            else if(obj is Drawable&&!(obj is Player))  ///player is to be drawen above all
                draw.Add(obj);
            else
                theRest.Add(obj);
        }
        plats.AddRange(items);
        plats.AddRange(proj);
        plats.AddRange(theRest);
        draw.AddRange(plats);
        objects = draw;
    }
    public static void ListHandler()
    {   ///garbae collector & adds new objects to CurrentLevel.objects
        ///adding objects to CurrentLevel.objects:
        foreach(GameObject obj in toAdd)
            objects.Add(obj);
        ///garbage collection:
        foreach(GameObject obj in toRemove)
            objects.Remove(obj);
        ///re initializing the lists
        toRemove = new List<GameObject>();
        toAdd = new List<GameObject>();
        ///sort new list
        CurrentLevel.sortObjList();
    }
    public static void loadLevel(int l) 
    {
        levelNum = l;
        ///set level Type
        if(levelNum <= 6) levelType = LevelType.Ground;
        else if(levelNum <= 12) levelType = LevelType.Ice;
        ///initiate level data
        objects.Clear();
        toAdd.Clear();
        toRemove.Clear();
        setState(GameState.gameOn);
        player = new Player();
        AddToGame(player);
        ///init level
        gameTime = 0;
        if(l == -999) InitLevelTest();
        else
        {
            try
            {
                bg = new Drawable(AssetManager.getSprite("Level"+levelNum), new Point(0, 0));
                typeof(CurrentLevel).GetMethod(
                    "InitLevel" + levelNum, BindingFlags.NonPublic | BindingFlags.Static).Invoke(null, null);
            }
            catch
            {
                Menu.switchToLevelMenu(l);
            }
        }
        sortObjList();
        ListHandler();
    }
    private static void InitLevel1()
    {
        sea = new List<GameObject>() { new GameObject(624, 850, 1088, 240) };
        starWorthyTime = 5f;
        List<Platform>pl = new List<Platform> {new Platform(0, 456, 624, 624),
        new Platform(817, 626, 416, 208),
        new Platform(1712, 248, 208, 832) };
        pl.Add(new MovingPlatform((pl[1].Y() + pl[2].Y()) / 2, new int[] { pl[1].XRight(), pl[2].X() - 70 }));

        player.setPosLeft(50);
        player.setPosBottom(pl[0].Y());
        door = new GameObject(1839, 46, 158, 205);
        key = new Key(pl[1]);
        coin = new Coin(pl[3]);
        objects.Add(new Spike(pl[0]));
        objects.Add(new Slime(pl[1]));
        objects.Add(coin); objects.Add(key);objects.Add(door);; objects.AddRange(pl);
    }
    private static void InitLevel2()
    {
        sea = new List<GameObject>() { new GameObject(0, 0, 0, 0) };
        starWorthyTime = 11f;
        door = new GameObject(1839, 83, 158, 201);

        List<Platform> pl = new List<Platform> { new Platform(0, 922, 1920, 158),
        new Platform(0, 650, 832, 104),
        new Platform(1504, 284, 406, 104),
        new Platform(0,284,416,104),
        new MovingPlatform(400,new int[]{750,1504 })};
        player.setPosBottom(pl[0].Y());
        player.setPosLeft(pl[0].X() + 100);

        List<Enemy> enemy = new List<Enemy> { new Slime(pl[3],offset:150), new Slime(pl[0],offset:300),
            new Zombie(pl[0],facing:dir.left,offset:-100) };
        key = new Key(pl[3]);
        coin = new Coin(pl[1], -300);
        List<Item> items = new List<Item> { key,coin, new Ammu(pl[1]) };

        Texture2D cannonTex = getSprite("CannonShot")[0];
        int cw = cannonTex.Width, ch = cannonTex.Height;
        Cannon c = new Cannon(new Point(winWidth - cw, pl[1].Y() - player.height() / 2 - ch / 2), dir.left);

        objects = new List<GameObject> { };
        objects.AddRange(enemy);
        objects.AddRange(pl); objects.AddRange(items); objects.Add(c);
    }
    private static void InitLevel3()
    {
        sea = new List<GameObject>() { new GameObject(417, 899, 831, 191) };
        starWorthyTime = 14;
        door = new GameObject(1462, 829, 114, 149);

        List<Platform> pl = new List<Platform> { new Platform(0,751,416,329),
        new Platform(0,353,417,104),
        new Platform(736,651,208,208),
        new Platform(1248,250,208,832),
        new Platform(1712,456,208,104),
        new Platform(1456,721,208,104),
        new Platform(1456,976,464,104),
        new Platform(1144,509,104,104)};
        pl.Add(new MovingPlatform(pl[1].Y(), new int[] { pl[1].XRight() + 100, pl[3].X() - 100 }));

        player.setPosBottom(pl[0].Y());
        player.setPosLeft(pl[0].X() + 100);

        List<Enemy> enemy = new List<Enemy> { new Slime(pl[0],offset:100),
        new Zombie(pl[2])};

        Obsticle obst = new Spike(pl[1]);
        obst.setPosRight(pl[1].XRight());

        List<Item> items = new List<Item> { new Key(pl[1]), new Coin(pl[4]) };
        items[0].addPosX(-100);

        Cannon c = new Cannon(new Point(pl[3].XRight(), pl[5].Y() - getSprite("CannonShot")[0].Height), dir.right);

        objects = new List<GameObject>();
        objects.AddRange(enemy);
        objects.AddRange(pl); objects.AddRange(items); objects.Add(obst);
        objects.Add(c);
    }
    private static void InitLevel4()
    {
        sea = new List<GameObject>() { new GameObject(1120, 1006, 390, 88) };
        starWorthyTime = 15f;
        door = new GameObject(1800, 65, 110, 170);
        List<Platform> pl = new List<Platform> {new Platform(0,976,1122,104),
        new Platform(1510,976,410,104),
        new Platform(300,580,624,104),
        new Platform(300,215,624,104),
        new Platform(1174,216,104,104),
        new Platform(1504,215,416,105)};
        List<Platform> movePl = new List<Platform> {
        new VerticallyMovingPlaform(1675,new int[]{ pl[2].Y(),pl[2].Y()+280}),
        new MovingPlatform(580,new int[]{pl[2].XRight()+player.width(),1625 }),
        new VerticallyMovingPlaform(40,new int[]{pl[3].Y(),pl[2].YBottom()+50 })};
        pl.AddRange(movePl);

        player.setPosBottom(pl[0].Y());
        player.setPosLeft(100);

        Obsticle obst = new Spike(pl[0], pl[0].width() / 3);

        List<Enemy> enemy = new List<Enemy> { new Slime(pl[2]),
        new Zombie(pl[3]),new Zombie(pl[0],offset:200)};

        Key k = new Key(new Point(0, 0));
        k.setPosXCenter((pl[4].XRight() + pl[5].X()) / 2); k.setPosYCenter(pl[4].YCenter());
        Coin c = new Coin(new Point(0, 0));
        c.setPosXCenter(movePl[2].XCenter()); c.setPosYCenter(50);
        c.setPosYCenter(pl[3].YCenter()); c.setPosXCenter((pl[3].XRight() + pl[4].X()) / 2);
        List<Item> items = new List<Item> { c, k };

        objects = new List<GameObject>();
        objects.AddRange(enemy);
        objects.AddRange(pl);
        objects.AddRange(items); objects.Add(obst);
    }
    private static void InitLevel5()
    {
        sea = new List<GameObject>() { new GameObject(370, 1006, 208, 84) };
        starWorthyTime = 10f;
        door = new GameObject(0, 220, 130, 190);
        List<Platform> pl = new List<Platform> { new Platform(0,976,370,104),
        new Platform(578,976,272,104),
        new Platform(850,852,208,208),
        new Platform(1058,976,846,104),
        new Platform(1103,373,416,104),
        new Platform(611,373,208,104),
        new Platform(0,373,280,104)};
        Texture2D playerStandTex = getSprite("Player/Walk")[0];
        pl.Add(new VerticallyMovingPlaform(1702, new int[] { pl[4].Y(), pl[0].Y() - playerStandTex.Height - 10 }));

        player.setPosBottom(pl[0].Y()); player.setPosLeft(70);
        int plWidth = getSprite("Medium/Ground")[0].Width;
        List<Enemy> enemy = new List<Enemy> { new Zombie(pl[3], offset: 200), new Slime(pl[4], offset: -plWidth / 2) };

        Obsticle obst = new Spike(pl[2]); obst.setPosRight(pl[2].XRight());

        Texture2D cannonTex = getSprite("CannonShot")[0];
        List<Cannon> cannons = new List<Cannon> { new Cannon(new Point(790, 893), dir.left),
        new Cannon(new Point(1920-cannonTex.Width,pl[4].Y()-cannonTex.Height-100),dir.left)};

        List<Item> items = new List<Item> { new Coin(new Point(cannons[1].X(), cannons[1].Y() - 100)), new Key(pl[5]) };
        objects = new List<GameObject> { obst };
        objects.AddRange(enemy); objects.AddRange(pl); objects.AddRange(items); objects.AddRange(cannons);
    }
    private static void InitLevel6()
    {
        getGame().bossLevel = new BossLevel(
            new Dictionary<int, int>() { { 1000, 0 }, { 2000, 1 }, { 3000, 2 }, { 5000, 3 } });
    }
    private static void InitLevel7()
    {
        sea = new List<GameObject>() { new GameObject(624, 995, 416, 90) };
        starWorthyTime = 15f;
        door = new GameObject(750, 47, 157, 201);
        List<Platform> pl = new List<Platform> {new Platform(0,976,624,104),
        new Platform(1040,976,880,104),new Platform(624,550,416,104,true),
        new Platform(624,248,416,104),new Platform(1504,248,416,104),new Platform(1220,248,104,104)};
        pl.Add(new VerticallyMovingPlaform(1300, new int[] { pl[2].YCenter(), pl[1].Y() - player.height() - 10 }));
        pl.Add(new VerticallyMovingPlaform(208, new int[] { pl[3].Y(), pl[2].YBottom() }));

        player.setPosBottom(pl[0].Y()); player.setPosLeft(100);
        List<GameObject>lst = new List<GameObject> {new Zombie(pl[1],dir.left,-300),new Snowman(pl[2],dir.left),new Slime(pl[4]),new Coin(pl[4]),
            new Key(new Point(104,(pl[2].Y()+pl[3].Y())/2)),new Ammu(pl[2]),new Spike(pl[0])};
        objects.AddRange(pl);objects.AddRange(lst);
    }
    private static void InitLevel8()
    {
        starWorthyTime = 15f;
        List<Platform> pl = new List<Platform>{new Platform(750, 248, 208, 832),
            new Platform(1166, 456, 416, 104,  true),
            new Platform(1504, 703, 416, 104),
            new Platform(1270, 976, 650, 104),
            new Platform(1166, 560, 104, 900),
            new Platform(0, 976, 750, 104),
            new Platform(646,740,104,104),
            new Platform(646,352,104,104),
            new Platform(314,548,104,104),
        new Platform(0,352,104,104)};
        sea = new List<GameObject>() { new GameObject(969, 757, 208, 322) };
        starWorthyTime = 25f;
        door = new GameObject(1726, 853, 158, 123);
        Cannon c = new Cannon(new Point(pl[9].XRight(), pl[9].Y()), dir.right);
        player.setPosBottom(pl[0].Y()); player.setPosXCenter(pl[0].XCenter());
        List<GameObject> lst = new List<GameObject> {new Snowman(pl[1],dir.left),
            new Slime(pl[2],dir.right,200),new Zombie(pl[3]),new Coin(pl[5]),
            new Key(pl[9]),new Snowman(pl[5]),c,new Ammu(pl[6])};
        objects.AddRange(pl);objects.AddRange(lst);
    }
    private static void InitLevel9()
    {
        starWorthyTime = 22f;
        List<Platform> pl = new List<Platform>() {new Platform(0,976,416,104,true),
        new Platform(1088,976,832,104,true),
        new Platform(1291,590,104,104),
        new Platform(831,590,104,104),
        new Platform(0,590,416,104),
        new Platform(1504,250,416,104)};
        List<Platform> movPl = new List<Platform>() {
            new MovingPlatform(pl[0].Y()-104,new int[] {pl[0].XRight()+50,pl[1].X()-50 },size:MovingPlatform.plSize.Small),
        new VerticallyMovingPlaform(1690,new int[]{pl[2].Y(),pl[0].Y()-100 },size:MovingPlatform.plSize.Small),
        new VerticallyMovingPlaform(1062,new int[]{pl[5].Y()-50,pl[2].YBottom()},size:MovingPlatform.plSize.Small),
        new MovingPlatform(pl[5].Y()-50,new int[]{pl[4].XRight(),pl[3].XRight()},size:MovingPlatform.plSize.Medium)};
        sea = new List<GameObject>() { new GameObject(417, 1002, 671, 100) };
        door = new GameObject(1726, 48, 160, 202);
        player.setPosBottom(pl[0].Y()); player.setPosXCenter(pl[0].XCenter());
        objects.AddRange(pl); objects.AddRange(movPl);
        Coin coin = new Coin(new Point(0, 0));
        coin.setPosXCenter(246); coin.setPosYCenter(pl[5].YCenter());
        objects.AddRange(new List<GameObject>() {new Key(pl[3]),coin,new Snowman(pl[1]),new Slime(pl[5]),new Snowman(pl[4]),
        new Ammu(pl[2])});
    }
    private static void InitLevel10()
    {
        starWorthyTime = 18f;
        List<Platform> pl = new List<Platform>() {new Platform(0,976,334,104),
        new Platform(644,976,832,104),new Platform(1073,740,208,104,true),
        new Platform(104,580,832,104),new Platform(751,171,416,104),
            new VerticallyMovingPlaform(1430,new int[]{171,580+52},true,MovingPlatform.plSize.Large)};
        door = new GameObject(872, 48, 157, 130);
        sea = new List<GameObject>() { new GameObject(332,1015,312,65),new GameObject(1476,1013,444,67) };
        player.setPosBottom(pl[0].Y());player.setPosRight(40);
        Key k = new Key(new Point(0, 720));k.setPosXCenter(sea[1].XCenter());
        List<Item> items = new List<Item>() {k,new Coin(new Point(70,350)),new Ammu(pl[2])};
        List<Enemy> enemies = new List<Enemy>() {new Snowman(pl[1],dir.left),new Slime(pl[3],dir.left,-150),new Slime(pl[3],offset:150),
            new Zombie(pl[5]) };
        objects.AddRange(pl);objects.AddRange(items);objects.Add(new Cannon(new Point(0, 470), dir.right));
        objects.AddRange(enemies);
    }
    private static void InitLevel11()
    {
        starWorthyTime = 9f;
        List<Platform> pl = new List<Platform>() {new Platform(0,976,360,104),
        new Platform(672,976,1040,104),new Platform(1712,872,208,208),
        new Platform(1593,500,416,104), ///player starts here
            new Platform(0,200,416,104),new Platform(0,630,312,104),new Platform(558,380,832,104,true)};
        door = new GameObject(77,774,158,202);
        sea = new List<GameObject>() { new GameObject(360,998,312,82)};
        player.setPosBottom(pl[3].Y());player.setPosRight(Global.winWidth - 50);player.flipSprite();
        List<Enemy> enemies = new List<Enemy>() {new Snowman(pl[4]),new Zombie(pl[1],dir.right,600),new Slime(pl[6],dir.right,200),
            new Slime(pl[6],dir.left,-200) };
        Cannon c = new Cannon(new Point(0,0),dir.left);
        c.setPosRight(winWidth);c.setPosBottom(pl[2].Y()-50);
        Key key = new Key(new Point(pl[2].XCenter(),pl[2].Y()-170));
        objects.AddRange(pl);objects.AddRange(enemies);objects.AddRange(new List<GameObject>()
        { key,c,new Coin(pl[5]),new Ammu(pl[6]),new Spike(pl[1]) });
    }
    private static void InitLevel12()
    {
        ((JustAnotherPlatformer)game).bossLevel = new BossLevel(
            new Dictionary<int, int>() { { 1500, 0 }, { 2500, 1 }, { 3500, 2 }, { 6000, 3 } });
    }
    private static void InitLevelTest()
    {
        List<Platform> pl = new List<Platform> { new Platform(0, 1080 - 208, 1920, 208),
        new Platform(0,769,208,208)};
        bg = new Drawable(cm.Load<Texture2D>("Assets/Sprites/Levels/Test"), new Point(0, 0));
        player.setPosBottom(pl[0].Y()); player.setPosRight(100);
        objects = new List<GameObject>(); objects.AddRange(pl);
        door = new GameObject(0, 0, 0, 0);
        player.giveBullets(10000);
        objects.Add(player);
    }
}
