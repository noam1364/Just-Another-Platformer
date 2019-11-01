using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using Project_Poseidon.NeuroEvolution.Utils;
using static Global;
using static CurrentLevel;
using static AssetManager;
using static BossLevel;
using System.Collections.Generic;

namespace Project_Poseidon
{
    public class NeatMain : Game, Global.IMainClass
    {
        #region data
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        public Menu menu;
        private int time;
        private NeatManager neatManager;
        private Boss currentPlayer;
        #endregion data
        public NeatMain()
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
            neatManager = new NeatManager();
            neatManager.start();
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
            for(int i = 0;i < time;i++)
            {
                ListHandler();
                foreach(GameObject obj in objects)
                    obj.update();
                if(!((PlayerBot)player).isLive)
                    switchToNextPlayer();
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
                if(InputHandler.KeyDown(Keys.Up))
                    time += 10;
                if(InputHandler.KeyDown(Keys.Down) && time >= 11)
                    time -= 10;
                if(InputHandler.KeyDown(Keys.Enter))
                    gameOverSequence();
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
            NumDisplayHandler.draw(new Point(winWidth - 500, 0), neatManager.generation);
            NumDisplayHandler.draw(new Point(winWidth - 500, 80), neatManager.currentPlayer);
            MaskGenerator.drawMaskPicture(MaskGenerator.getUpdatedMask());
            NumDisplayHandler.draw(new Point(0, 300), ((PlayerBot)player).getFitness());
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
        public void restart()
        {
            gameOverSequence();
        }
        private void switchToNextPlayer()
        {
            neatManager.currentPlayer++;
            neatManager.update();
            currentPlayer = neatManager.playerList[neatManager.currentPlayer];
            loadLevel(CurrentLevel.levelNum);
        }
        public void gameOverSequence(bool isDrown = false)
        {   
        }
        public void finishLevel(int stars=-1)
        {

        }
        public void pauseGame(GameState s = GameState.gameOn)
        {
            menu = new PauseMenu(s: s);
        }
        public void startNewLevel(int level)
        {
            currentPlayer = neatManager.playerList[neatManager.currentPlayer];
            loadLevel(level);
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
