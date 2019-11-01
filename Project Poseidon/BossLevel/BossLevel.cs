using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using Project_Poseidon.Static_Classes;
using static Project_Poseidon.NeuroEvolution.NeuralNetwork;
using static Global;
using static CurrentLevel;
using static AssetManager;
using static BossLevel.Tree;
using Project_Poseidon;

public class BossLevel : Game
{
    #region data
    private int stars;
    ///key:distance | value:amount of stars it deserves
    private Dictionary<int, int> starD;
    ///minDx[0] --> between 2 small obsts | minDx[1] between tall and non tall | minDx[2] -- >between 2 talls
    protected readonly int[] bulletShootY = new int[] { 600, 800, 870 },minDx = new int[] { 650,800,1000};
    protected readonly Dictionary<int, int> speeds = new Dictionary<int, int>() { {0,15 },{150,17 },{300, 20 }};
    protected const double obsticleProb = 0.05,bulletProb = 0.8,cloudProb = 0.005;
    public const int groundLevel = 900,playerX=450,bossX=200,cloudMaxY = 250;

    public int groundVx, obstDx = 0, x = 0, backgroundVx = 2;
    private Boss boss;
    private TreeShape lastObst;
    public static INetwork bestBrain;
    protected Queue<TreeShape> toCreate;
    protected List<Tree> obsticles;
    protected List<Drawable> clouds;
    public Platform ground;
    protected ScrollingBackground secondaryBackground;
    #endregion data
    public BossLevel(Dictionary<int,int>staringDistances)
    {
        stars = -1;
        starD = staringDistances;
        HudManager.init(new HudManager.HudElement[] {HudManager.HudElement.Life,HudManager.HudElement.Stars,
            HudManager.HudElement.Clock,HudManager.HudElement.DistanceIcon});
        door = new GameObject(0, 0, 0, 0);
        key = new Key(new Point(0, 0));
        coin = new Coin(new Point(0, 0));
        sea = null;
        RemoveFromGame(player);
        ListHandler();
        bg = new ScrollingBackground(AssetManager.getSprite("Level" + levelNum), new Point(0, 0));
        secondaryBackground = new ScrollingBackground(AssetManager.getSprite("SuperLong"), new Point(0, groundLevel));
        ground = new Platform(0, groundLevel, winWidth, winWidth);
        groundVx = speeds[0];
        toCreate = new Queue<Tree.TreeShape>();
        lastObst = TreeShape.Small;
        ///ground must be created before the boss and the player
        player = new BossPlayer(ground);
        bestBrain = DataHandler.ReadFromBinaryFile<Network>(NeatManager.bestPlayerUrl);
        boss = new Boss(bestBrain,ground);
        obsticles = new List<Tree>();
        clouds = new List<Drawable>();
        AddToGame(boss);
        AddToGame(player);
        AddToGame(ground);
        AddToGame(secondaryBackground);
        getGame().bossLevel = this;
    }
    #region Added methods
    public virtual void GameLogicUpdate()
    {
        ListHandler();
        foreach(GameObject obj in objects)
            obj.update();
        obstGenerator();
        obsticleMovementManager();
        cloudGenerator();
        backMovementManager();

        int distance = getDistance();
        int prevStars = stars;///if the amount of stars has changed,play a sound
        foreach(int d in starD.Keys)
        {
            if(distance >= d)
                stars = starD[d];
        }
        if(prevStars!=stars)
            MediaManager.playSoundEffect("Coin");

        if(player.isAlive())
        {
            ReciveGameInput();
            if(speeds.ContainsKey(distance))
                groundVx = speeds[distance];
        }
        else
        {
            finishLevel();
        }
        foreach(Tree obst in obsticles)
        {
            if(boss.intersects(obst))
            {
                finishLevel();
                boss.killBoss();
            }
        }
    }
    protected void backMovementManager()
    {
        secondaryBackground.move(groundVx);
        if(bg is ScrollingBackground)
            ((ScrollingBackground)bg).move(backgroundVx);
        foreach(Drawable cloud in clouds)
            cloud.addPosX(-backgroundVx);
    }
    protected void cloudGenerator()
    {
        if((float)random.NextDouble() < cloudProb)
            generateCloud();
    }
    protected void generateCloud()
    {
        Texture2D[] cloudTex = AssetManager.getSprite("Cloud");
        int choise = (int)(random.NextDouble() * cloudTex.Length);
        int y = (int)(random.NextDouble() * cloudMaxY);
        Drawable newCloud = new Drawable(cloudTex[choise], new Point(winWidth, y));
        AddToGame(newCloud);
        clouds.Add(newCloud);
    }
    protected void obstGenerator()
    {
        if((float)random.NextDouble() < obsticleProb)
            toCreate.Enqueue(Tree.randomShape());
        if(toCreate.Count>0)
            generateObst();
    }
    protected void obsticleMovementManager()
    {
        x += groundVx;
        obstDx+=groundVx;
        ///move obsticles
        List<Tree> toRemove = new List<Tree>();
        foreach(Tree obj in obsticles)
        {
            obj.addPosX(-groundVx);
            if(obj.XRight() < 0)
            {
                RemovedFromGame(obj);
                toRemove.Add(obj);
            }
        }
        foreach(Tree obj in toRemove)
            obsticles.Remove(obj);
    }
    protected void generateObst()
    {
        if(obstDx < minDx[0])
            return;  ///the minial dx for the creation of any type of obsticle
        TreeShape choise = toCreate.Peek();
        Tree obst;
           
        if(lastObst==TreeShape.Tall && choise == TreeShape.Tall && obstDx < minDx[2])
        {
            return; ///the spawned tree and last one are tall,but there is not enough obstDx to spawn,so exit
        }
        else
        {
            if((lastObst==TreeShape.Tall || choise == TreeShape.Tall) && obstDx < minDx[1])
            {
                return;///the spawned tree or last one is tall,but there is not enough obstDx to spawn,so exit
            }
        }
        toCreate.Dequeue();
        obst = new Tree(ground, choise);
        lastObst = choise;
        obst.setPosBottom(ground.Y());
        obst.setPosLeft(winWidth);
        AddToGame(obst);
        this.obsticles.Add(obst);
        obstDx = -obst.width(); ///so that obstDx is the actual space between the obsticles
    }
        
    protected virtual void ReciveGameInput()
    {
        BossPlayer p = (BossPlayer)player;
        if(InputHandler.KeyStrokeFull(Keys.LeftShift))
            MediaManager.muteAction();
        if(InputHandler.KeyStrokeFull(Keys.Escape))
            game.pauseGame();
        if(!InputHandler.IsInputBlocked())
        {
            if(InputHandler.KeyDown(Keys.W))
                p.smallJump();
            if(InputHandler.KeyDown(Keys.Space))
                p.bigJump();
        }
    }
    public virtual void DisplayHUD()
    {
        HudManager.displayHud(new int[] {gameTime/fps, getDistance()}, player.getHealth(),stars);
    }
    public virtual void restart()
    {
        toCreate = new Queue<Tree.TreeShape>();
    }
    public void finishLevel()
    {
        backgroundVx = 0;
        groundVx = 0;
        JustAnotherPlatformer g = Global.getGame();
        bool levelCompleted=false;
        if(!boss.isAlive()||stars>=0)
            levelCompleted = true;
        if(levelCompleted)
            g.finishLevel(stars);
        else
            g.gameOver();
    }
    protected int getDistance()
    {
        return x / 104;
    }
    public GameObject getClosestObst(int x0)
    {
        if(obsticles.Count == 0) return null;
        int dMin = winWidth;
        int d = 0;
        GameObject obj = null;
        foreach(Obsticle obst in obsticles)
        {
            d = obst.X() - x0;
            if(d > 0 && d < dMin)
            {
                obj = obst;
                dMin = d;
            }
        }
        return obj;
    }
    /// <summary>
    /// Returns the X differance between obj1 and the obsticle after it
    /// </summary>
    /// <param name="obj1"></param>
    /// <returns></returns>
    public int diffBetweenObst(GameObject obj1)
    {
        if(obsticles.Count < 2 || obj1 == null) return 0;
        List<GameObject> copyLst = new List<GameObject>(obsticles);
        int tempD = winWidth, dMin = winWidth;
        foreach(Obsticle obst in obsticles)
        {
            tempD = obst.X() - obj1.XRight();
            if(obst.X() > obj1.XRight() && tempD < dMin)    ///if obst is ahead of obj1, and the distance between them is minimal
            {
                dMin = tempD;
            }
        }
        if(dMin == winWidth) return 0;  ///if there are no other objects
        else return dMin;
    }
    #endregion Added methods
    public class MovingCannon : Cannon
    {
        private int[] bounds;
        private int vy = 2;
        public MovingCannon(int[] bounds, dir facing) : base(new Point(0), facing)
        {
            isActive = false;
            this.bounds = bounds;
            setPosRight(winWidth);
            setPosYCenter((bounds[0] + bounds[1]) / 2);
        }
        public override void update()
        {
            base.update();
            ///move
            addPosY(vy);
            bool buttomBound = YCenter() >= bounds[1], topBound = YCenter() <= bounds[0];
            if(buttomBound ||topBound )
            {
                vy *= -1;
            }
        }
    }
    public class BossPlayer : Player
    {
        private IAttacker lastAttcker;
        private const int creepSpeed = 1;
        public BossPlayer(Platform pl) : base(pl)
        {
            lastAttcker = null;
            setPosBottom(groundLevel);
            setPosLeft(playerX);
        }
        public override void update()
        {
            isMoved = true;
            base.update();
            if((X() <= 0 ||health<0)&& !freeFall)
                die();
            
            int dx = playerX - X();
            if(dx < 0)
                addPosX(-creepSpeed);
            if(dx > 0)
                addPosX(creepSpeed);
        }
        public override Type GetType()
        {
            return typeof(Player);
        }
        public void smallJump()
        {
            postInputJump();
        }
        public void bigJump()
        {
            postInputJump(-40);
        }
        public override void attackedBy(IAttacker attacker, dir dir = dir.Still)
        {
            if(!attacker.Equals(lastAttcker) || lastAttcker == null)
            {
                base.attackedBy(attacker, dir.left);
                lastAttcker = attacker;
            }
        }
    }
    public class Boss : Enemy
    {
        public INetwork brain;
        public Boss(Platform pl) : this(DataHandler.ReadFromBinaryFile<Network>(NeatManager.bestPlayerUrl),pl)
        {

        }
        public Boss(INetwork brain,Platform pl) : base(pl)
        {
            setPosRight(bossX);
            this.brain = brain;
        }
        
        protected override void AI()
        {
            tryAttack();
            BossLevel g = Global.getBossLevel();
            GameObject closest = g.getClosestObst(XRight());
            int diffBetweenObst=0;

            if(closest != null) g.diffBetweenObst(closest);  
            float[] input = new float[6];
            ///input:
            /// 0 --> distance to next obsticle
            /// 1 --> next obsticle height
            /// 2 --> next obsticle width
            /// 3--> distance between the closest obsticle and the one after it
            /// 4--> player height
            /// 5 --> game speed
            if(closest == null)
            {
                input[0] = 0;
                input[1] = 0;
                input[2] = 0;
                input[3] = 0;
            }
            else
            {
                input[0] = ((closest.X() - XRight()) / 100);
                input[1] = closest.height() / 100;
                input[2] = closest.width() / 100;
                input[3] = (diffBetweenObst) / 100;
            }
            input[4] = (groundLevel - YBottom()) / 100;
            input[5] = (float)(g.groundVx / 10);

            float[] output = brain.getOutput(input);
            output = MathService.softmax(output);
            int result = MathService.getMaxIdx(output);
            if(result == 0)
                jump(-32);      ///small jump
            else if(result == 1)
                jump(-40);      ///big jump
            ///if result==3 --> do nothing
        }
        
        public void killBoss()
        {
            die();
        }
        public override string getTypeToString()
        {
            return "Boss";
        }
    }
    public class Tree:Obsticle
    {
        public enum TreeShape { Small=0,Wide=1,Tall=2};
        private TreeShape shape;
        public Tree(Platform pl, TreeShape shape) : base(pl)
        {
            this.shape = shape;
            Texture2D[] arr = AssetManager.getSprite("Tree/" + shape.ToString());
            int idx = MathService.randomInt(0, arr.Length);
            setTexture(new Texture2D[]{arr[idx]});
        }
        public Tree(Platform pl,int shape):this(pl,(TreeShape)shape)
        {

        }
        public bool isWide()
        {
            return shape == TreeShape.Wide;
        }
        public bool isTall()
        {
            return shape == TreeShape.Tall;
        }
        public static TreeShape randomShape()
        {
            return (TreeShape)MathService.randomInt(0,Enum.GetValues(typeof(TreeShape)).Length); 
        }
    }
    public class ScrollingBackground:Drawable
    {
        private Texture2D tex;
        private Rectangle rect1, rect2;
        public ScrollingBackground(Texture2D[] tex, Point pos) : base(tex, pos)
        {
            this.tex = tex[0];
            rect1 = new Rectangle(pos.X, pos.Y, this.tex.Width, this.tex.Height);
            rect2 = new Rectangle(pos.X+this.tex.Width, pos.Y, this.tex.Width, this.tex.Height);
        }
        public override void draw()
        {
            Global.sb.Draw(tex, rect1, Color.White);
            Global.sb.Draw(tex, rect2, Color.White);
        }

        public override void update()
        {
            if(rect1.X + tex.Width <= 0)
                rect1.X = rect2.X + tex.Width;
            if(rect2.X + tex.Width <= 0)
                rect2.X = rect1.X + tex.Width;
        }
        public void move(int dx)
        {
            rect1.X -= dx;
            rect2.X -= dx;
            update();
        }
    }
}
