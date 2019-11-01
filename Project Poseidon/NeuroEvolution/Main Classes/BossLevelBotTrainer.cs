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
using Project_Poseidon;

public class BossLevelBotTrainer : BossLevel
{
    #region data
    private int time = 1;
    NeatManager neatManager;
    #endregion data
    public BossLevelBotTrainer():base(new Dictionary<int, int>() { { 0,0} })
    {
        neatManager = new NeatManager();
        neatManager.start();
        restart();
        HudManager.init(new HudManager.HudElement[] {HudManager.HudElement.DistanceIcon,HudManager.HudElement.Clock });
    }
    #region Added methods
    public override void GameLogicUpdate()
    {   
        for(int i=0;i<time;i++)
        {
            if(speeds.ContainsKey(getDistance()))
                groundVx = speeds[getDistance()];
            neatManager.update();
            List<Boss> toRemove = new List<Boss>();
            foreach(BossBot player in neatManager.playerList)
            {
                foreach(Obsticle obst in obsticles)
                {
                    if(obst.intersects(player)&&player.isActive)
                    {
                        player.killBot();
                        toRemove.Add(player);
                    }
                }
            }

            foreach(BossBot looser in toRemove)
                neatManager.playerList.Remove(looser);

            ReciveGameInput();
            ListHandler();
            foreach(GameObject obj in objects)
                obj.update();
            obstGenerator();
            obsticleMovementManager();
            cloudGenerator();
            backMovementManager();
            gameTime++;
        }
        gameTime--;
    }
    protected override void ReciveGameInput()
    {
        if(InputHandler.KeyStrokeFull(Keys.Up))
            time = 10;
        if(InputHandler.KeyDown(Keys.Down) && time >= 11)
            time -= 10;
        if(InputHandler.KeyStrokeFull(Keys.Escape))
            game.pauseGame();
    }
    public override void DisplayHUD()
    {
        HudManager.displayHud(new int[] { x / 104, gameTime / fps });
        NumDisplayHandler.draw(new Point(40,150),neatManager.generation);
    }
    public override void restart()
    {
        base.restart();
        //gameTime = 0;
        x = 0;
        RemoveFromGame(player);
        player = null;
        bg = new ScrollingBackground(AssetManager.getSprite("Level" + levelNum), new Point(0, 0));
        secondaryBackground = new ScrollingBackground(AssetManager.getSprite("SuperLong"), new Point(0, groundLevel));
        groundVx = speeds[0];
        ground = new Platform(0, groundLevel, winWidth, winWidth);
        objects.Clear();
        obsticles = new List<Tree>();
        clouds = new List<Drawable>();
        AddToGame(ground);
        AddToGame(secondaryBackground);
        objects.AddRange(neatManager.playerList);
    }
    #endregion
    public class BossBot:Boss
    {

        public bool isActive;
        private int fitness = 0;
        
        public BossBot(INetwork brain, Platform pl) : base(brain,pl)
        {
            isActive = true;
        }
        public override void update()
        {
            if(isActive)
                base.update();
        }
        public void init()
        {
            isActive = true;
        }
        public void killBot()
        {
            float fit = getBossLevel().x;
            brain.SetFitness(fit * fit);
            isActive = false;
        }
        public int getFitness()
        {
            return fitness;
        }
        public override Type GetType()
        {
            return typeof(Boss);
        }
        public override void draw()
        {
            if(isActive)
                base.draw();
        }
    }
}

