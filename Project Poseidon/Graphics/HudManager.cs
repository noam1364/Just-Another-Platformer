using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Project_Poseidon.Static_Classes
{
    public static class HudManager
    {
        private static bool isLife,isStars;
        private static int[] y;
        private static Texture2D[] heartTex = AssetManager.getSprite("Hearts"),
            starTex = AssetManager.getSprite("StarIcon");
        private static int xCenter,dy=10,dx = 10;
        private static List<Texture2D> icons;
        public enum HudElement {AmmuIcon,Clock,DistanceIcon,Stars,Life };

        /// <summary>
        /// Initilizes HudManager with the proper icons to be display at the HUD Display
        /// </summary>
        /// <param name="icons">An array of all the elements of each metric. the order on the screen matches
        /// the order in the array.</param>
        public static void init(HudElement[] elements)
        {
            icons = new List<Texture2D>();
            isLife = false;
            isStars = false;
            foreach(HudElement e in elements)
            {
                switch(e)
                {
                    case HudElement.Life:
                    {
                        isLife = true;
                        break;
                    }
                    case HudElement.Stars:
                    {
                            isStars = true;
                            break;
                    }
                    default:
                    {
                            icons.Add(AssetManager.getSprite(e.ToString())[0]);
                        break;
                    }
                }
            }
            y = new int[icons.Count];
            y[0] =  dy;
            if(isLife)
                y[0] += heartTex[0].Height;
            if(isStars)
                y[0] += starTex[0].Height+dy;
            xCenter = icons[0].Width/2;

            for(int i=1;i<icons.Count;i++)
            {
                if(icons[i].Width + dx > xCenter)
                    xCenter = icons[i].Width;
                y[i] = y[i - 1] + icons[i - 1].Height+dy;
            }
        }
        /// <summary>
        /// Displays the life meter,and all other metrics defined in HudManager.init()
        /// </summary>
        /// <param name="life">amount of life points,each point is half a heart</param>
        /// <param name="data">Each element in the array is a number to display | must be ordered the same way the icon are on the 
        /// screen.</param>
        public static void displayHud(int[] data,int life=0,int stars=0)
        {
            if(isLife)
                displayLife(life);
            if(isStars)
                drawStarMetric(stars);
            int x=0,timeDigY,digH = NumDisplayHandler.digitsTex.Height;
            for(int i=0;i<icons.Count;i++)
            {
                x = xCenter - icons[i].Width / 2;
                timeDigY = y[i] + digH / 2 + (icons[i].Height / 2 - digH);
                Global.sb.Draw(icons[i], new Vector2(x, y[i]), Color.White);
                NumDisplayHandler.draw(new Point(x+icons[i].Width+dx,timeDigY), data[i]);
            }
        }
        /// <summary>
        /// Displays a life meter graphics
        /// </summary>
        /// <param name="life">2 * the amount of hearts to display | each points is half a heart</param>
        private static void displayLife(int life)
        {
            int dx = heartTex[0].Width, j = 0;
            float temp = (life/ 2f);
            for(int i = 0;i < 5;i++)
            {
                if(temp >= 1) j = 2;    ///heartTex[2] is a full heart
                else
                {
                    if(temp == 0.5)
                        j = 1;          ///heartTex[1] is a half heart
                    else
                        j = 0;          ///heartTex[0] is an empty heart
                }
                temp--;
                Global.sb.Draw(heartTex[j], new Vector2(dx * i,0), Color.White);
            }
        }
        private static void drawStarMetric(int stars)
        {
            Texture2D flagTex = AssetManager.getSprite("FlagIcon")[0];
            int dx = starTex[0].Width, j = 0, lastIcon = HudManager.y.Length-1;
            int y =heartTex[0].Height+dy;
            Global.sb.Draw(flagTex, new Vector2(0, y), Color.White);
            if(stars==-1)
                Global.sb.Draw(AssetManager.getSprite("XIcon")[0], new Vector2(flagTex.Width+HudManager.dx, y), Color.White);
            else
            {
                for(int i = 0;i < 3;i++)    ///3 stars
                {
                    if(stars >= 1) j = 1;    ///starTex[1] is a full star
                    else j = 0;             ///starTex[0] is an empty star

                    stars--;
                    Global.sb.Draw(starTex[j], new Vector2(flagTex.Width + dx * i, y), Color.White);
                }
            }
        }
        /// <summary>
        /// draw a star display in a certin location
        /// </summary>
        /// <param name="stars">Number of stars to display</param>
        /// <param name="pos">X value is for the X if the center,Y value is for the Y value of the top</param>\
        /// <param name="middleStarYOffset ">Vertical offset for the middle star,positive for up. the middle star is drawn
        /// in the Y Cord of pos, and the other 2 stars are pushed down</param>
        public static void drawStarIcons(int stars,Point pos,int middleStarYOffset = 0)
        {
            int dx = starTex[0].Width, j = 0;
            int y = pos.Y + dy;
            for(int i = 0;i < 3;i++)    ///3 stars
            {
                if(stars >= 1) j = 1;    ///starTex[1] is a full star
                else j = 0;             ///starTex[0] is an empty star

                stars--;
                int yFinal;
                if(i != 1) yFinal = y +middleStarYOffset;   ///if not the middle star,push down by the given Y offset
                else yFinal = y;
                Global.sb.Draw(starTex[j], new Vector2(pos.X+dx * i-(int)(1.5*dx), yFinal), Color.White);
            }
        }
    }
}
