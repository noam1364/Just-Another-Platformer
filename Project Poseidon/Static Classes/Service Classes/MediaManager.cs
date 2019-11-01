using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Audio;
using Project_Poseidon;
using static Global;
using System;
using System.Collections.Generic;

public static class MediaManager
{
    ///songs that need to be saved | can be paused and played from middle
    #region data
    private static Song currentlyPlaying;
    private static List<SoundEffectInstance> currentlyPlaingEffects = new List<SoundEffectInstance>();
    private static Dictionary<Song, TimeSpan> TimesPaused = new Dictionary<Song, TimeSpan>() { };
    private static Dictionary<GameState, Song> Songs = new Dictionary<GameState, Song>();
    #endregion data
    #region methods
    public static void Initiate()
    {
        ///initilize each gameState with its own song,and each song with the time to start playing it (currently 0 because song just began)
        foreach(GameState state in Enum.GetValues(typeof(GameState)))
        {
            Songs[state] = AssetManager.getSong(getSongName(state));
            if(Songs[state]!=null)
                TimesPaused[Songs[state]] = new TimeSpan(0);
        }
        currentlyPlaingEffects = new List<SoundEffectInstance>();
    }
    /// <summary>
    /// Plays the currect song accourding to the new gameState and the previous one.
    /// </summary>
    /// <param name="newState"></param>
    /// <param name="prevState"></param>
    private static Song getSongForState(GameState st)
    {
        if(st == GameState.gameOn && bossLevelOn())
            return AssetManager.getSong("BossLevel");
        return Songs[st];
    }
    public static void NewGameStateAudio(GameState newState, GameState prevState)
    {
        Song newSong = getSongForState(newState);
        switch(newState)
        {
            case GameState.StartMenu:
            {
                if(prevState == GameState.None)
                    playSong(newSong);
                break;
            }
            case GameState.LevelMenu:
            {
                if(prevState != GameState.StartMenu && prevState != GameState.PauseMenu)
                    playSong(newSong);
                break;
            }
            case GameState.gameOn:  ///case where a fresh level is starting is handeld in 'CurrentLevel.loadLevel()'
            {
                if(prevState == GameState.PauseMenu)    ///a case of resuming from pause,not restarting
                {
                    if(PauseMenu.getStateToReturnTo()==GameState.GameOverSeq)   ///if restarting a new game,while paused on a GameOverSeq
                    {
                        MediaManager.Initiate();
                        killAllEffects();
                        playSong(GameState.gameOn);
                    }
                    else ///continue and on-going game
                    {
                        resumeSong(newSong);
                        resumeAllEffects();
                    }
                }
                else ///starting a new game
                {
                    MediaManager.Initiate();
                    killAllEffects();
                    playSong(GameState.gameOn);
                }
                break;
            }
            case GameState.GameOverSeq:
            {
                if(prevState == GameState.PauseMenu)
                {
                    resumeAllEffects();
                }
                else ///from gameOn
                    killAllEffects();
                playSong(newSong);
                break;
            }
            default:
            { ///pauseMenu,GameOverMenu,LevelFinish
                playSong(newSong);
                pauseAllEffects();
                break;
            }
        }
    }
    private static void resumeSong(Song song)
    {
        currentlyPlaying = song;
        MediaPlayer.IsRepeating = true;
        MediaPlayer.Play(song, TimesPaused[song]);
    }
    public static void playSong(GameState state)
    {
        Song s;
        if(Global.bossLevelOn() && state == GameState.gameOn)
            s = AssetManager.getSong("BossLevel");
        else
            s = AssetManager.getSong(getSongName(state));
        playSong(s);
    }
    public static void playSong(Song song)
    {
        if(currentlyPlaying != null)
            TimesPaused[currentlyPlaying] = MediaPlayer.PlayPosition;
        currentlyPlaying = song;
        if(song == null)
        {
            MediaPlayer.Stop();
        }
        else
        {
            MediaPlayer.Play(song);
            MediaPlayer.IsRepeating = true;
        }
    }
    public static SoundEffectInstance playSoundEffect(SoundEffect effect)
    {
        if(effect == null||MediaPlayer.IsMuted) return null;
        SoundEffectInstance eff = effect.CreateInstance();
        eff.Play();
        eff.IsLooped = false;
        currentlyPlaingEffects.Add(eff);
        updateCurrentSoundEffects();
        return eff;
    }
    public static SoundEffectInstance playSoundEffect(string e)
    {
        return playSoundEffect(AssetManager.getSoundEffect(e));
    }
    public static void pauseAllEffects()
    {
        foreach(SoundEffectInstance e in currentlyPlaingEffects)
            e.Pause();
    }
    public static void resumeAllEffects()
    {
        foreach(SoundEffectInstance e in currentlyPlaingEffects)
            if(e.State == SoundState.Paused)
                e.Resume();
    }
    public static void killAllEffects()
    {
        foreach(SoundEffectInstance e in currentlyPlaingEffects)
            e.Stop();
        currentlyPlaingEffects = new List<SoundEffectInstance>();
    }
    public static void killAllSound()
    {
        killAllEffects();
        MediaPlayer.Stop();
    }
    public static bool isSoundEffectPlaying(SoundEffectInstance eff)
    {
        if(eff == null) return false;
        return eff.State == SoundState.Playing;
    }
    public static string getSongName(GameState state)
    {
        if(state == GameState.StartMenu || state == GameState.PauseMenu || state == GameState.LevelMenu)
            return "LevelMenu";
        else return state.ToString();
    }
    private static void updateCurrentSoundEffects()
    {
        SoundEffectInstance[] arr = new SoundEffectInstance[currentlyPlaingEffects.Count];
        currentlyPlaingEffects.CopyTo(arr);
        foreach(SoundEffectInstance effect in arr)   ///update currentlyPlayingSoundEffects
        {
            if(effect.State == SoundState.Stopped)
            {
                currentlyPlaingEffects.Remove(effect);
                effect.Dispose();
            }
        }
    }
    public static void muteAction()
    {
        if(MediaPlayer.Volume == 0)
        {
            MediaPlayer.Volume = 1;
            MediaPlayer.IsMuted = false;
        }
        else
        {
            MediaPlayer.IsMuted = true;
            MediaPlayer.Volume = 0;
        }
    }
    public static bool isMuted()
    {
        return MediaPlayer.Volume == 0;
    }
    #endregion methods
}
