using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using static Global;
using static CurrentLevel;
using MathNet.Numerics.LinearAlgebra;

namespace Project_Poseidon.NeuroEvolution.Utils
{
    public static class MaskGenerator
    {
        ///for AI efficiency
        public const int maskScale = 40,drawScale=6,targetMinSize = 100;
        public const int maskScaleSq = maskScale * maskScale;
        public static List<GameObject> toUpdateOnMask = new List<GameObject>();
        public static Vector<float> baseMask;
        public static Vector<float> fullMask;

        public static void updateMask(GameObject playerTarget = null)
        {
            ///each 'scale' pixels in the game in 1 pixel on the mask,for efficiency
            ///the vector is made out of segments with length of colCount (length of a row)
            int colCount = winWidth / maskScale, y = 0, x = 0, y2 = 0, x2 = 0;
            Vector<float> mask = baseMask.Clone();
            int idx = 0;
            Surface val = 0;
            if(playerTarget != null)
                toUpdateOnMask.Add(playerTarget);
            foreach(GameObject obj in toUpdateOnMask)
            {
                x = obj.X(); x2 = obj.XRight();
                y = obj.Y(); y2 = obj.YBottom();
                if(obj is Enemy)
                    val = Surface.Enemy;
                else if(obj is Obsticle)
                    val = Surface.Obstacle;
                else if(obj is Player)
                    val = Surface.Player;
                else if(obj is Projectile)
                    val = Surface.Projectile;
                else if(obj is Platform)    ///moving
                    val = Surface.Platform;
                if(playerTarget != null)
                {
                    if(obj.Equals(playerTarget))
                    {
                        val = Surface.Target;
                        ///give the target a larger footprint on the mask if its small,but make sure it doesnt excede the bounds
                        if(obj.height() <  targetMinSize|| obj.width() < targetMinSize)
                        {
                            Point center = obj.getCenterPosition();
                            int delta = targetMinSize / 2;
                            x = center.X-delta; x2 = center.X+delta; y = center.Y-delta; y2 = center.Y + delta;
                            if(x < 0) x = 0;
                            if(y < 0) y = 0;
                            if(x2 > winWidth) x2 = winWidth;
                            if(y2 > winHeight) y2 = winHeight;
                        }
                    }
                }
                ///if object is to small to appear in the mask,make it large enough
                if(y2 - y < maskScale)
                    y2 = y + maskScale;
                if(x2 - x < maskScale)
                    x2 = x + maskScale;
                /// j is the X cord and i is the y cord | run for the whole space obj takes on the screen
                for(int i = y / maskScale;i < y2 / maskScale;i++)
                {
                    for(int j = x / maskScale;j < x2 / maskScale;j++)
                    {
                        idx = j + i * colCount;
                        mask[idx] = (float)val;
                    }
                }
            }
            if(playerTarget != null)
                toUpdateOnMask.Remove(playerTarget);
            fullMask = mask;
        }
        /// <summary>
        /// returns the most updated mask of the game map. double values represents 'Surface' Enum values and are safe to cast.
        /// this is done so that the mask is also NeuralNetwork friendly
        /// </summary>
        /// <returns></returns>
        public static Vector<float> getUpdatedMask()
        {
            return fullMask;
        }
        public static Matrix<float> getUpdatedMaskAsMat()
        {
            Matrix<float> mat = Matrix<float>.Build.Dense(winWidth / maskScale, winHeight / maskScale);
            Vector<float> mask = getUpdatedMask();
            for(int i = 0;i < winHeight / maskScale;i++)
            {
                for(int j = 0;j < winWidth / maskScale;j++)
                {
                    mat[j, i] = (float)mask[j + i * (winWidth / maskScale)];
                }
            }
            return mat;
        }
        public static Vector<float> getBaseMask()
        {
            int colCount = winWidth / maskScale, rowCount = winHeight / maskScale; ///each 'scale' pixels in the game in 1 pixel on the mask,for efficiency
            Vector<float> mask = Vector<float>.Build.Dense(colCount * rowCount);
            Point temp;
            ///the vector is made out of segments with length of colCount (length of a row)
            /// j is the X cord and i is the y cord
            for(int i = 0;i < winHeight;i+=maskScale)
            {
                for(int j = 0;j <winWidth;j+=maskScale)///run for the full length of the courrent row
                {
                    int idx = j/maskScale + (i/maskScale) * colCount;
                    temp = new Point(j, i);
                    foreach(GameObject obj in objects)
                    {
                        if(obj.contains(temp)&&idx<mask.Count)
                        {
                            if(obj is Platform && !(obj is MovingPlatform))
                                mask[idx] = (float)Surface.Platform;
                            else
                                mask[idx] = (float)Surface.Air;
                        }
                    }
                }
            }
            return mask;
        }
        public static void drawMaskPicture(Vector<float> mask)
        {
            Vector2 offset = new Vector2(700, 0), pos = new Vector2(0);
            Texture2D tex = null;
            Texture2D black = AssetManager.getSprite("Black")[0];
            Texture2D white = AssetManager.getSprite("White")[0];
            Texture2D red = AssetManager.getSprite("Red")[0];
            Texture2D blue = AssetManager.getSprite("Blue")[0];
            Texture2D yellow = AssetManager.getSprite("Yellow")[0];
            Surface val = 0;
            Matrix<float> maskMat = getUpdatedMaskAsMat();
            for(int i = 0;i < winHeight / maskScale;i++)
            {
                for(int j = 0;j < winWidth / maskScale;j++)
                {
                    val = (Surface)maskMat[j, i];
                    pos = new Vector2(j, i) * drawScale;
                    if(val == Surface.Air)
                        tex = white;
                    else if(val == Surface.Platform)
                        tex = black;
                    else if(val == Surface.Enemy || val == Surface.Obstacle)
                        tex = red;
                    else if(val == Surface.Player)
                        tex = blue;
                    else if(val == Surface.Target)
                        tex = yellow;
                    for(int p = 0;p < drawScale;p++)
                    {
                        for(int g = 0;g < drawScale;g++)
                        {
                            sb.Draw(tex, offset + pos + new Vector2(p, g), new Rectangle(0, 0, tex.Width, tex.Height), Color.White, 0, new Vector2(0, 0), new Vector2(1), SpriteEffects.None, 0);
                        }
                    }
                }
            }
        }
    }
}
