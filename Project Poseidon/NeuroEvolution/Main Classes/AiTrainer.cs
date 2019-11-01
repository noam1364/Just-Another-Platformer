using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using Project_Poseidon.NeuroEvolution.Utils;
using static Global;
using static AssetManager;
using static CurrentLevel;
using Project_Poseidon.NeuroEvolution;
using System.Collections.Generic;

namespace Project_Poseidon
{
    public class AiTrainer : Game, Global.IMainClass
    {
        #region data
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        public Menu menu;
        private int time;
        #endregion data
        public AiTrainer()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            graphics.PreferredBackBufferWidth = winWidth;
            graphics.PreferredBackBufferHeight = winHeight;
            Window.Title = "AiTrainer";
        }
        #region Base Logic 
        protected override void Initialize()
        {
            base.Initialize();
            NeuroEvolutionManager.Initiate();
            NeuroEvolutionManager.CreateFirstGen();
            time = 1;
            menu = new StartMenu();
            setState(GameState.StartMenu);
            Global.game = this;
        }
        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            cm = Content;
            sb = spriteBatch;
            AssetManager.Initiate();
            MediaManager.Initiate();
        }
        protected override void UnloadContent()
        {

        }
        protected override void Update(GameTime gameTime)
        {
            MediaPlayer.IsMuted = true;
            MediaPlayer.Volume = 0;
            MediaManager.killAllSound();
            GameState state = getGameState();
            if(gameOn())
                GameLogicUpdate();
            else
                menu.update();
            InputHandler.update();
            base.Update(gameTime);
        }
        protected override void Draw(GameTime gameTime)
        {
            sb.Begin();
            if(gameOn())
                GameDraw();
            else
            {
                if(menu is GameOverMenu || menu is PauseMenu || menu is LevelFinishMenu || getGameState() == GameState.GameOverSeq)
                    GameDraw();
                if(isMenu())
                    menu.draw();
            }
            sb.End();
            base.Draw(gameTime);
        }
        #endregion Base Logic
        #region Added methods
        private void GameLogicUpdate()
        {   ///order matters - so that bullets that are spawned during input can be added to the game without one cycle delay
            ReciveInput();
            for(int i=0;i<time;i++)
            {
                ListHandler();
                foreach(GameObject obj in CurrentLevel.objects)
                    obj.update();
                if(!((PlayerBot)player).isLive)
                    gameOverSequence();
                MaskGenerator.updateMask(((PlayerBot)player).getTarget());
                UpdateGameTime();
            }
        }
        public void GameDraw()
        {
            bg.draw();
            foreach(GameObject obj in objects)
                obj.draw();
            DispalyHUD();
        }
        private void ReciveInput()
        {
            if(InputHandler.KeyStrokeFull(Keys.Escape))
                pauseGame();
            if(!InputHandler.IsInputBlocked())
            {
                if(InputHandler.KeyStroke(Keys.Up))
                    time += 10;
                if(InputHandler.KeyDown(Keys.Down) && time >= 11)
                    time -= 10;
                if(InputHandler.KeyStroke(Keys.Enter ))
                    gameOverSequence();
                if(InputHandler.KeyStroke(Keys.Space))
                    ((PlayerBot)player).maxLifetime = 500;
            }
        }
        private void DispalyHUD()
        {
            ///heart display
            Texture2D[] heartTex = AssetManager.getSprite("Hearts");
            int dx = heartTex[0].Width, j = 0;
            float temp = (player.getHealth() / 2f);
            for(int i = 0;i < 5;i++)
            {
                if(temp >= 1) j = 2;
                else
                {
                    if(temp == 0.5)
                        j = 1;
                    else
                        j = 0;
                }
                temp--;
                sb.Draw(heartTex[j], new Vector2(dx * i, 0), Color.White);
            }
            ///time display
            Texture2D clockTex = getSprite("Clock")[0], ammuIconTex = getSprite("AmmuIcon")[0];

            float time = gameTime / fps;
            int clockIconHeight = heartTex[0].Height + 5;
            sb.Draw(clockTex, new Vector2(0, clockIconHeight), Color.White);
            int digH = NumDisplayHandler.digitsTex.Height;
            int timeDigsHeight = clockIconHeight + digH / 2 + (clockTex.Height / 2 - digH);
            NumDisplayHandler.draw(new Point(clockTex.Width + 10, timeDigsHeight), (int)time);

            ///bullet display
            int ammuIconHeight = clockIconHeight + clockTex.Height + 20, ammuIconX = clockTex.Width / 2 - ammuIconTex.Width / 2;
            sb.Draw(ammuIconTex, new Vector2(ammuIconX, ammuIconHeight));
            int ammuDigsHeight = ammuIconHeight + digH / 2 + (ammuIconTex.Height / 2 - digH);
            NumDisplayHandler.draw(new Point(ammuIconX + ammuIconTex.Width + 20, ammuDigsHeight), player.getAmmu());

            ///display Evolution stats
            NumDisplayHandler.draw(new Point(winWidth - 500, 0), NeuroEvolutionManager.currentGen);
            NumDisplayHandler.draw(new Point(winWidth - 500, 80), NeuroEvolutionManager.currentSpecies);
            MaskGenerator.drawMaskPicture(MaskGenerator.getUpdatedMask());
            NumDisplayHandler.draw(new Point(0, 300),((PlayerBot)player).getFitness());
        }
        private void ListHandler()
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
        private void UpdateGameTime()
        {
            if(gameTime >= int.MaxValue)    ///at 60fps, will happen after 414.25 days
                gameTime = 0;
            gameTime++;
        }
        ///in game change game state methods:
        public void gameOverSequence(bool isDrown = false)
        {   ///current bot failed the game
            RemoveFromGame(player); ///safety
            //NeuroEvolutionManager.deadPopulation.Add((BossPlayerBot)player);
            NeuroEvolutionManager.currentSpecies++;
            if(NeuroEvolutionManager.currentSpecies>=NeuroEvolutionManager.populationSize)
            {
                NeuroEvolutionManager.CreateNextGen();
            }

            if(player != null)
            {
                if(player is PlayerBot)
                    player = new PlayerBot(NeuroEvolutionManager.population[NeuroEvolutionManager.currentSpecies]);
                else
                    player = new Player();
            }
            else
                player = new PlayerBot(NeuroEvolutionManager.population[NeuroEvolutionManager.currentSpecies]);
            CurrentLevel.loadLevel(CurrentLevel.levelNum); 
        }
        public void finishLevel(int stars=-1)
        {   ///three stars-->one for collecting the coin,second for good time,third for not losing health
            gameOverSequence();
        }
        public void restart()
        {

        }
        public void pauseGame(GameState s = GameState.gameOn)
        {
            menu = new PauseMenu(s: s);
        }
        public void startNewLevel(int level)
        {
            player = new PlayerBot(NeuroEvolutionManager.population[NeuroEvolutionManager.currentSpecies]);
            CurrentLevel.loadLevel(level);
            MediaManager.Initiate();
            gameTime = 1;   ///so gameTime dependent processes will be delayed by action cycle (-one game tick)
            setState(GameState.gameOn);
            MediaManager.killAllEffects();
        }
        public Menu getMenu()
        {
            return menu;
        }
        public void setMenu(Menu m)
        {
            menu = m;
        }
        #endregion Added methods
    }
}
