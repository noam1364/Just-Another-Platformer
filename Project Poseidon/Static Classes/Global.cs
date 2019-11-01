using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using Project_Poseidon;
using System;
using System.Collections.Generic;

public static class Global  ///a static class for global const variables and other global objects(cm&sb)
{
    #region data
    ///Global variables:
    public static IMainClass game;
    public static ContentManager cm;
    public static SpriteBatch sb;
    public static int gameTime = 0;
    public const float gameBegin = 1;   ///a period of time (in seconds) after the start of level in which cannons dont shoot
    public static Random random = new Random(System.DateTime.Now.Millisecond);
    private static List<ProgressMode> progress;
    ///Constants:
    public const string progressFileUrl = "Progress";
    public const int winWidth = 1920, winHeight = 1080, fps = 60;
    ///enums:
    public enum GameState { gameOn, LevelMenu,StartMenu,GameOverMenu,PauseMenu,LevelFinishMenu,GameOverSeq,None};
    public enum dir { right = 1, left = -1,Still = 0 };
    ///gameState
    private static GameState state = GameState.None;
    public enum ProgressMode { Locked = -1, Open = -2, ZeroStars = 0, OneStar = 1, TwoStars = 2, ThreeStars = 3 }
    public interface IMainClass
    {
        Menu getMenu();
        void setMenu(Menu m);
        void finishLevel(int stars=-1);
        void pauseGame(GameState s = GameState.gameOn);
        void gameOverSequence(bool isDrown=false);
        void startNewLevel(int l);
        void restart();
    }
    #endregion data
    #region methods
    public static void print(string s)
    {
        System.Diagnostics.Debug.WriteLine(s);
    }
    public static void print(int s)
    {
        System.Diagnostics.Debug.WriteLine(s);
    }
    /// <summary>
    /// Returns the current instance of JustANotherPlatformer, the main class
    /// </summary>
    /// <returns></returns>
    public static JustAnotherPlatformer getGame()
    {
        return (JustAnotherPlatformer)game;
    }
    public static BossLevel getBossLevel()
    {
        return getGame().bossLevel;
    }
    public static int getTics(float sec)
    {
        return (int)(sec * fps);
    }
    public static int getTimeInSec()
    {
        return gameTime / fps;
    }
    public static bool gameOn()
    {
        return state == GameState.gameOn;
    }
    public static bool bossLevelOn()
    {
        return getGame().bossLevel != null;
    }
    public static GameState getGameState()
    {
        return state;
    }
    public static bool isMenu()
    {
        return state.ToString().Contains("Menu");
    }
    public static void setState(GameState st)
    {
        GameState prevState = state, newState = st; ///so that 'NewGameStateAudio' is called upon an updated Global.getState()
        state = st;
        MediaManager.NewGameStateAudio(newState, prevState);
        if(st == GameState.gameOn)
            InputHandler.blockSpace();
    }
    public static void setState(string s)
    {   ///set gameState by string
        foreach(GameState st in Enum.GetValues(typeof(GameState)))
        {
            if(st.ToString() == s)
                setState(st);
        }
    }
    public static List<ProgressMode> getProgress()
    {
        if(progress == null)
            progress = DataHandler.ReadFromXmlFile<List<ProgressMode>>(progressFileUrl);
        if(progress == null)    ///safety only
            initProgress();
        return progress;
    }
    public static ProgressMode getProgressScore(int levelNum)
    {
        if(levelNum - 1 >= progress.Count) return ProgressMode.Locked;
        return progress[levelNum - 1];
    }
    public static void changeProgress(int level,ProgressMode prog)
    {
        if(level-1 >= progress.Count) return;
        progress[level - 1] = prog;
        DataHandler.WriteToXmlFile(progressFileUrl, progress);
    }
    public static void initProgress()
    {
        List<ProgressMode> prog = new List<ProgressMode>();
        prog.Add(ProgressMode.Open);
        for(int i = 1;i < 12;i++)
            prog.Add(ProgressMode.Locked);
        DataHandler.WriteToXmlFile<List<ProgressMode>>(progressFileUrl, prog);
        progress = prog;
    }
    /// <summary>
    /// Returns true if there is a saved game with actual progress, so the user can press 'Continue' on the StartMenu
    /// </summary>
    /// <returns></returns>
    public static bool isThereASavedGame()
    {
        List<ProgressMode> prog = DataHandler.ReadFromXmlFile<List<ProgressMode>>(progressFileUrl);
        if(prog == null) return false;
        if(prog[0] != ProgressMode.Open) return true;
        for(int i = 1;i < prog.Count;i++)
        {
            if(prog[i] != ProgressMode.Locked) return true;
        }
        return false;
    }
    /// <summary>
    /// Returns the number of the last level that is open but has not been completed yet.
    /// if all levels are completed, the method will return the first one that is not 3-stars scored
    /// </summary>
    /// <returns></returns>
    public static int lastOpenLevel()
    {
        if(progress == null)
            progress = DataHandler.ReadFromXmlFile<List<ProgressMode>>(progressFileUrl);
        int level = 1;
        bool flag = false;
        for(int i = 0;i < progress.Count;i++)
        {
            if(progress[i] == Global.ProgressMode.Open)
            {
                level = i + 1;
                flag = true;
            }
        }
        if(flag)
            return level;
        else ///if all levels are already completed,find the first one that needs to be re completed
        {
            for(int i=0;i<progress.Count;i++)
            {
                if(progress[i] != ProgressMode.ThreeStars)
                {
                    return i + 1;
                }
            }
        }
        return 1;   ///for safety
    }
    #endregion  methods 
}



       