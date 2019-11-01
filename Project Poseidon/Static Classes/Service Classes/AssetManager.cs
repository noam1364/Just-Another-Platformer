using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.IO;
using static Global;
using System;
using System.Reflection;

public static class AssetManager
{
    private static Dictionary<string, Texture2D[]> sprites = new Dictionary<string, Texture2D[]>();
    private static Dictionary<string, Song> songs = new Dictionary<string, Song>();
    private static Dictionary<string, SoundEffect> soundEffects = new Dictionary<string, SoundEffect>();
    public enum AssetType {Sprite,Audio };
    public static void Initiate()   ///initiates 'sprites' with all Sprites saves in 'Sprites' directory
    {                               ///and splits them into 1D array by the subDirectory they are saved in
        MethodInfo loadArr = typeof(AssetManager).GetMethod("getAssetArrFromDir", BindingFlags.NonPublic | BindingFlags.Static),
            loadAsset = typeof(AssetManager).GetMethod("getAssetFromDir", BindingFlags.NonPublic | BindingFlags.Static);
        initDict<Texture2D[]>(sprites, "SpritesAndEffects",loadArr);
        initDict<Song>(songs, "Songs", loadAsset);
        initDict<SoundEffect>(soundEffects, "spritesAndEffects", loadAsset);
    }
    private static void initDict<T>(Dictionary<string,T>dict,string root,MethodInfo load)
    {   ///load:must return an object of type T ,and recive a directory url as a 'string'
        Type typeToLoad;
        if(typeof(T).IsArray)   ///if T is an array,we want to load an asset of its element type, not another array
            typeToLoad = typeof(T).GetElementType();
        else
            typeToLoad = typeof(T);
        load = load.MakeGenericMethod(typeToLoad);

        string suffix = "";     ///in case of song 
        if(typeof(T) == typeof(SoundEffect))
            suffix = "SoundEffect";
        else if(typeof(T) == typeof(Texture2D[]))
            suffix = "Sprites";
            
        root = Directory.GetCurrentDirectory() + "\\Content\\Assets\\"+root+"\\";
        List<string> finalDirs = getFinalDirectories(root); ///urls of each subDirectory containing An asset
        foreach(string dir in finalDirs)    ///init the dictionary with simplified  keys (location inside 'root') and values
        {///only if the directory contains the wanted type of asset | a directory with only sprites will not contain a "Sprites" subDir
            string key = (dir.Replace(root, "")).Replace("\\", "/");
            if(suffix.Length>0) key.Replace(suffix, "");
            if((typeToLoad==typeof(Texture2D)&&!dir.Contains("SoundEffect"))||dir.EndsWith(suffix)) 
                dict[key] = (T)load.Invoke(null,new object[] { dir });
        }
    }
    /// <summary>
    /// A recursive method that recives the url of a directory, and returns a list of the urls 
    /// of all last level directories (dirs that doesnt contain more dirs)
    /// </summary>
    /// <param name="url"></param>
    /// <returns></returns>
    private static List<string> getFinalDirectories(string url) 
    {  
        List<string> retList = new List<string>();
        string[] subDirs = Directory.GetDirectories(url);
        if(subDirs.Length == 0) ///if reached final nesting level of the directory
        {
            List<string> temp = new List<string>(); ///return the url of the current directory
            temp.Add(url);                          ///it is the last in its branch 
            return temp;
        }
        foreach(string str in subDirs)  ///if there are subDirs - scan them further recursively
        {
            List<string> temp = getFinalDirectories(str);
            if(temp!=null)
                retList.AddRange(temp);
        }
        return retList;
    }
    private static T[] getAssetArrFromDir<T>(string dir)    
    {
        string[] urls = Directory.GetFiles(dir);
        if(urls.Length == 0) return null;
        T[] arr = new T[urls.Length];
        
        string url;
        for(int i=0;i<arr.Length;i++)   ///for each url in the dir,remove file extentions and load the file to the array
        {
            url = removeFileExt(urls[i]);
                arr[i] = Global.cm.Load<T>(url);
        }
        return arr;
    }
    private static T getAssetFromDir<T>(string dir)
    {
        string[] files = Directory.GetFiles(dir);
        if(files.Length == 0) return default(T);
        Type t = typeof(T).GetType();
        dir = files[0];
        dir = dir.Replace(".wma", ""); ///for SoundEffects | Reqieres a more roubust solution
        dir = dir.Replace(".xnb", ""); 
        return Global.cm.Load<T>(dir);
    }
    /// <summary>
    /// A method that recives an addreas of a sprite and returns it.
    /// </summary>
    /// <param name="key">The addreas of the sprite. If the addreas is unique,a partial key can be passed.</param>
    /// <returns></returns>
    public static Texture2D[] getSprite(string key) ///recives a partial key of an element in 'sprites',and returns
    {                                               ///the corresponding value | makes accecing the dictionary simpler
        key = key.Replace("//","/");    ///cathing mistakes in key names
        key = key.Replace("\\", "/");
        Dictionary<string, Texture2D[]>.KeyCollection keys = sprites.Keys;
        List<string> keyLst = new List<string>();
        foreach(string k in keys)
            keyLst.Add(k);
        string keyWithLvlType = key +"/"+ CurrentLevel.levelType.ToString();
        foreach(string k in keyLst)
        {
            if(k == keyWithLvlType || k.Contains(keyWithLvlType))
                return sprites[k];
        }
        ///try without the levelType suffix
        foreach(string k in keyLst)
        {
            if((k == key) || k.Contains(key))
                return sprites[k];
        }
        return null;
        throw new NotImplementedException();   
    }
    /// <summary>
    /// A method that recives an addreas of a song and returns it.
    /// </summary>
    /// <param name="key">The addreas of the song. If the addreas is unique,a partial key can be passed.</param>
    /// <returns></returns>
    public static Song getSong(string key) 
    {                                               
        key = key.Replace("//", "/");    ///cathing mistakes in key names
        key = key.Replace("\\", "/");
        Dictionary<string, Song>.KeyCollection keys = songs.Keys;
        List<string> keyLst = new List<string>();
        foreach(string k in keys)
            keyLst.Add(k);
        string keyWithLvlType = key + "/" + CurrentLevel.levelType.ToString();
        foreach(string k in keyLst)
        {
            if(k == keyWithLvlType || k.Contains(keyWithLvlType))
                return songs[k];
        }
        ///try without the levelType suffix
        foreach(string k in keyLst)
        {
            if((k == key) || k.Contains(key))
                return songs[k];
        }
        return null;
        throw new NotImplementedException();   //to implement:WrongSpriteKeyException
    }
    /// <summary>
    /// A method that recives an addreas of a soundEffect and returns it.
    /// </summary>
    /// <param name="key">The addreas of the soundEffect. If the addreas is unique,a partial key can be passed.</param>
    /// <returns></returns>
    public static SoundEffect getSoundEffect(string key) 
    {                                              
        key = key.Replace("//", "/");    ///cathing mistakes in key names
        key = key.Replace("\\", "/");
        Dictionary<string, SoundEffect>.KeyCollection keys = soundEffects.Keys;
        List<string> keyLst = new List<string>();
        foreach(string k in keys)
            keyLst.Add(k);
        string keyWithLvlType = key + "/" + CurrentLevel.levelType.ToString();
        foreach(string k in keyLst)
        {
            if(k == keyWithLvlType || k.Contains(keyWithLvlType))
                return soundEffects[k];
        }
        ///try without the levelType suffix
        foreach(string k in keyLst)
        {
            if((k == key) || k.Contains(key))
                return soundEffects[k];
        }
        return null;
        throw new NotImplementedException();   //to implement:WrongSpriteKeyException
    }
    private static string removeFileExt(string s)
    {
        return s.Split('.')[0];
    }
}
