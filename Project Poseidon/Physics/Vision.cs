using Microsoft.Xna.Framework;
using static CurrentLevel;
using static Global;
using static System.Math;
using System;
using System.Collections.Generic;

public class Vision
{
    private int length;
    private float[] alpha;
    /// <summary>
    /// Crerates a Vision object
    /// </summary>
    /// <param name="length">The max distance the vision can see to</param>
    /// <param name="alpha"> a range of angles the visions sees at</param>
    public Vision(int length,int[] alpha)
    {
        this.length = length;
        this.alpha = new float[] {MathHelper.ToRadians(alpha[0]),MathHelper.ToRadians(alpha[1]) };
    }
    public Vision(int length, int alpha) : this(length,new int[] {-alpha,alpha })
    {
        
    }
    /// <summary>
    /// Returns true if the enemy has the player in his vision, and has a clear sight without
    /// any object in the middle
    /// </summary>
    /// <param name="enemy"></param>
    /// <returns></returns>
    public bool isPlayerInClearSight(Enemy enemy)
    {
        ///A vector from the center of the obj to the player | a vector tho its a Point object
        Point toPlayer = new Point(player.XCenter()-enemy.XCenter(),enemy.YCenter()-player.YCenter());

        int d = (int)Sqrt((Pow(toPlayer.X, 2) + Pow(toPlayer.Y, 2))); ///distance from obj to player | Pythagorian theorem
        if(d > length) return false; ///if player is to far-->return false

        float beta = (float)Atan2(toPlayer.Y, Abs(toPlayer.X));///calculated so that the angle is always in the quadrent 1 or 4 

        dir p = player.getSpriteDir(),e = enemy.getSpriteDir();
        if(beta >= alpha[0] && beta <= alpha[1] && enemy.facesThePlayer())///if the vision is in the promitted angle range
        {   ///check if there is no vision blocking object in the way between the enemy and the player
            Vector2 pos1 = enemy.getCenterPositionVec(), pos2 = player.getCenterPositionVec(), pos = pos1;
            float beta2 = (float)Atan2(toPlayer.Y, toPlayer.X);  ///angle between player and enemy | can now be at ANY quadrent

            for(int i = 0;i < d;i++)
            {
                pos = new Vector2(pos.X+(float)Cos(beta2),pos.Y+(float)Sin(beta2)); ///move along the connecting vector 
                foreach(GameObject obj in objects)
                {
                    if(obj != enemy && obj != player && obj is Platform)
                    {   ///if obj is vision blocking but not the player or איק enemy this vision belongs to
                        if(obj.contains(pos))
                            return false;
                    }
                }
            }
            return true;
        }
        else return false;
    }
    public int getLength()
    {
        return length;
    }
}
