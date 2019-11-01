using System;
using Microsoft.Xna.Framework;
using static Global;
using MathNet.Numerics.LinearAlgebra;
using static CurrentLevel;
using static Project_Poseidon.NeuroEvolution.NeuralNetwork;
using Project_Poseidon.NeuroEvolution.Utils;
using static Project_Poseidon.NeuroEvolution.Utils.BfsSearch;
using Project_Poseidon.NeuroEvolution;
/// <summary>
/// Experimental class. Designed to be a bot that can play all of the game by itself.
/// </summary>
public class PlayerBot : Player
{
    public static readonly int[] nonNeatBrainStructure = new int[] {winWidth*winHeight/MaskGenerator.maskScaleSq,50,50,4 };
    public int maxLifetime;
    public INetwork brain;
    public bool isLive;
    private int lifetime,maxFitness,fitness;   ///time this bot is alive
    public PlayerBot():base()
    {
        lifetime = 0;
        maxLifetime = 200;
        maxFitness = 0;
        fitness = 0;
        isLive = true;
    }
	public PlayerBot(INetwork brain):this()
    {
        this.brain = brain;
    }
    public void init()
    {
        isLive = true;
    }
    public override void update()
    {
        if(!isLive) return;
        ///AI logic
        base.update();
        lifetime++;
        ///feed network
        Vector<float> input = MaskGenerator.getUpdatedMask();
        float[] arr = input.ToArray();
        float[] result = brain.getOutput(arr);
        ///result[0]--Jump | result[1]--Left | result[2]--Right | result[3]--Shoot
        if(result[1] > result[2] && result[1] > 0.5) player.postInputMove(dir.left);
        else if(result[0] > result[1] && result[0] > 0.5) player.postInputMove(dir.right);
        if(result[0] > 0.5) player.postInputJump();
        if(result[3] > 0.5)
            player.shootBullet();
        fitness = (int)calcFitness();
        ///if the bot is making progress,give him extra time
        if(fitness>maxFitness)
        {
            maxFitness = fitness;
            maxLifetime += 15;
        }
        ///kill when lifetime is up
        if(lifetime > maxLifetime)
            die();
    }
    public void Destroy()
    {
        kill();
    }
    protected override void kill()
    {
        base.kill();
        isLive = false;
        brain.SetFitness(calcFitness());
    }
    protected override void die()
    {
        kill();
    }
    protected override void actionCollSea()
    {
        die();
    }
    public override Type GetType()
    {   ///for compatability with the rest of the game
        return typeof(Player);
    }
    public int getFitness()
    {
        return fitness;
    }
    protected override void enterDoor()
    {
        fitness += 1000;
        kill();
    }
    private float calcFitness()
    {
        float score = 0f, scoreUnit = 4000;
        ///the function of 'temp' is to check if the addition we are making to 'score' is negetive,
        ///and if so to make it 0. else, there might be a situation where a player without a key has a higher score
        ///then a player with a key,but that is far from the coin,for example.
        score += getHealth() * 50;
        if(player.gotKey()) score += scoreUnit;
        if(player.getCoins() > 0) score += scoreUnit;
        return score + scoreUnit - dToObj(getTarget());
    }
    /// <summary>
    /// Returns the current objective of the player
    /// </summary>
    /// <returns></returns>
    public GameObject getTarget()
    {
        ///if the player got the key and the coin,he must go to the door
        bool k = gotKey(), c = getCoins() > 0;
        if(k && c) return door;
        ///if the player has only one of them,he needs to get the other
        //for training (perormace):
        ///for level 1:
        if(k) return coin;
        else return key;
        //for real:
        int dToKey = (int)dToObj(key),dToCoin = (int)dToObj(coin);
        return dToKey > dToCoin ? (GameObject)coin:(GameObject)key;
    }
    private float dToObj(GameObject obj = null)
    {
        if(obj == null)
            return 0;
        else
            return MaskGenerator.maskScale*
                BfsSearch.calcShortestDistance(MaskGenerator.getUpdatedMaskAsMat(), getCenterPosition(), obj.getCenterPosition());
    }
    public override void draw()
    {
        base.draw();
        //if(target == null) target = door;
    }
}
