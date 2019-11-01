using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using static Global;
using static CurrentLevel;
using static AssetManager;
using Project_Poseidon.Static_Classes;
using System.Collections.Generic;

namespace Project_Poseidon
{
    public class JustAnotherPlatformer : Game,Global.IMainClass
    {
        #region data
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        public Menu menu;
        private static Animation gameOverPlayerAnim;
        private static SoundEffectInstance gameOverSound;
        public BossLevel bossLevel;
        #endregion data
        public JustAnotherPlatformer()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            graphics.PreferredBackBufferWidth = winWidth;
            graphics.PreferredBackBufferHeight = winHeight;
            Window.Title = "Just Another Platformer";
        }
        #region Base Logic 
        protected override void Initialize()
        {
            base.Initialize();
            menu = new StartMenu();
            setState(GameState.StartMenu);
            Global.game = this;
            CurrentLevel.player = new Player();
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
            ///no 'else' statement because if during one game state the is a change, that way the new stated is updated and then drawn
            GameState state = getGameState();
            if(isMenu())
                menu.update();
            if(gameOn())
            {
                if(bossLevel==null)
                    GameLogicUpdate();
                else
                    bossLevel.GameLogicUpdate();
                UpdateGameTime();
            }
            if(state == GameState.GameOverSeq)
                GameOverSeqUpdate();
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
                if(menu is GameOverMenu||menu is PauseMenu||menu is LevelFinishMenu||getGameState()==GameState.GameOverSeq)
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
            ReciveGameInput();
            ListHandler();
            foreach(GameObject obj in objects)
                obj.update();
        }
        public void GameDraw()
        {
            bg.draw();
            foreach(GameObject obj in objects)
                obj.draw();
            if(bossLevel == null)
                DispalyHUD();
            else
                bossLevel.DisplayHUD();
        }
        private void GameOverSeqUpdate()
        {
            ///the game over animation of the player is updated through Animation.draw()
            if(!gameOverPlayerAnim.isLive() && !MediaManager.isSoundEffectPlaying(gameOverSound))
                gameOver();
            else
                ListHandler();
            if(InputHandler.KeyStrokeFull(Keys.Escape))
                pauseGame(GameState.GameOverSeq);
        }
        private void ReciveGameInput()
        {
            if(InputHandler.KeyStrokeFull(Keys.LeftShift))
                MediaManager.muteAction();
            if(InputHandler.KeyStrokeFull(Keys.Escape))
                pauseGame();
            if(!InputHandler.IsInputBlocked())
            {
                if(InputHandler.KeyDown(Keys.W))
                    player.postInputJump();
                if(InputHandler.KeyDown(Keys.D))
                    player.postInputMove(dir.right);
                if(InputHandler.KeyDown(Keys.A))
                    player.postInputMove(dir.left);
                if(InputHandler.KeyStroke(Keys.L))
                    player.shootBullet();
            }
        }
        private void DispalyHUD()
        {
            HudManager.displayHud(new int[] {gameTime/fps,player.getAmmu() }, player.getHealth());
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

        }
        public void gameOverSequence(bool isDrown = false)
        {
            setState(GameState.GameOverSeq);
            if(isDrown)
            {
                MediaManager.playSoundEffect("Character/Drown");
                gameOverSound = MediaManager.playSoundEffect("Player/Drown");
                gameOverPlayerAnim = new DrowmAnimation(AssetManager.getSprite(player.GetType().ToString() + "/Drown"),
                    player.getPosition(), player.getEffects(), false, true);
            }
            else
            {
                gameOverSound = MediaManager.playSoundEffect("Player/Die");
                gameOverPlayerAnim = new Animation(AssetManager.getSprite("Player/Die"), player.getPosition(), player.getEffects(), false, true);
            }
        }
        public void finishLevel(int stars = -1)
        {   ///three stars-->ome for collecting the coin,second for good time,third for not losing health
            if(stars==-1)
            {
                stars = player.getCoins();
                if(gameTime <= getTics(starWorthyTime)) stars++;
                if(player.getHealth() == player.getMaxHealth() && !player.getWasEverHit()) stars++;
            }
            int prevScore = (int)getProgressScore(levelNum);
            if(prevScore<stars)                                 ///if this score is better the the previously scored score on this level,update it
                changeProgress(levelNum, (ProgressMode)stars);

            if(getProgressScore(levelNum+1) == ProgressMode.Locked)///if the next level is locked,open it
                changeProgress(levelNum + 1, ProgressMode.Open);
            
            
            menu = new LevelFinishMenu(stars);
        }
        public void gameOver()
        {
            menu = new GameOverMenu();
        }
        public void pauseGame(GameState s = GameState.gameOn)
        {
            menu = new PauseMenu(s: s);
        }
        public void startNewLevel(int level)
        {
            Texture2D[] guiArr = new Texture2D[] { AssetManager.getSprite("Clock")[0], AssetManager.getSprite("AmmuIcon")[0] };
            HudManager.init(new HudManager.HudElement[] {HudManager.HudElement.Life,HudManager.HudElement.Clock,
            HudManager.HudElement.AmmuIcon});
            bossLevel = null;
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
