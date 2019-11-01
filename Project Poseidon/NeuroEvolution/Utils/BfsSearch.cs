using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using MathNet.Numerics.LinearAlgebra;
using Priority_Queue;
using static Project_Poseidon.NeuroEvolution.Utils.MaskGenerator;
using static CurrentLevel;

namespace Project_Poseidon.NeuroEvolution.Utils
{
    /// <summary>
    /// A Static class implementing BFS search on a grid,calculates the distance between 2 point on a grid with blocking elements
    /// </summary>
    public static class BfsSearch
    {
        private static bool[,] mask;    ///true--Blocked | False-clear
        private static bool[,] wasChecked;
        private static Point[] dirs = new Point[] {new Point(1,0),new Point(0,1),new Point(-1,0),new Point(0,-1) };
            ///new Point[] {new Point(1,0),new Point(1,1),new Point(1,-1),
            ///new Point(0,1),new Point(-1,1),new Point(-1,0),new Point(-1,-1),new Point(0,-1) };
        private static Point from,to;
        private static Queue<Point> queue;
        private static int nodesLeftInLayer, nodesInNextLayer;

        private static void createMask(Matrix<float> maskParam)
        {
            mask = new bool[maskParam.RowCount, maskParam.ColumnCount];
            bool isBlocked = false;
            Surface val = 0;
            for(int i=0;i<mask.GetLength(0);i++)
            {
                for(int j=0;j<mask.GetLength(1);j++)
                {
                    val = (Surface)maskParam[i, j];
                    if(val == Surface.Air || val == Surface.Target || val == Surface.Player)
                        isBlocked = false;
                    else
                        isBlocked = true;
                    mask[i,j] = isBlocked;
                }
            }
        }
       
        /// <summary>
        /// Calculates the Manhatten distance between 2 points on a grid
        /// </summary>
        /// <param name="maskParam"></param> The grid,double values represent 'Surface' Enum values
        /// <param name="from"></param> Starting point,not accounting for the down scale of the mask
        /// <param name="to"></param> End point,not accounting for the down scale of the mask
        /// <returns>The distance between the 2 points, if impossible to connect 2 point,returns -1</returns>
        public static int calcShortestDistance(Matrix<float> maskParam,Point from,Point to)
        {
            createMask(maskParam);  ///initializes mask
            int distance = 0;
            bool reachedEnd = false;
            nodesLeftInLayer = 1; nodesInNextLayer = 0;
            wasChecked = new bool[mask.GetLength(0), mask.GetLength(1)];
            wasChecked.Initialize(); ///default(bool) == false

            Point current = new Point(from.X, from.Y);
            from = new Point(from.X / maskScale, from.Y / maskScale);
            to = new Point(to.X / maskScale, to.Y / maskScale);

;           queue = new Queue<Point>();
            wasChecked[from.X, from.Y] = true;
            queue.Enqueue(from);

            while(queue.Count!=0&&!reachedEnd)
            {
                current = queue.Dequeue();
                if(current.X==to.X&&current.Y==to.Y)
                {
                    reachedEnd = true;
                    break;
                }
                else
                {
                    exploreNeighbors(current);
                    nodesLeftInLayer--;
                    if(nodesLeftInLayer==0)
                    {
                        nodesLeftInLayer = nodesInNextLayer;
                        nodesInNextLayer = 0;
                        distance++;
                    }
                }
            }

            if(reachedEnd)
                return distance;
            else
                return -1;
        }

        private static void exploreNeighbors(Point pos)
        {
            Point current = new Point(pos.X,pos.Y);
            foreach(Point dir in dirs)
            {
                current = new Point(pos.X, pos.Y);
                current += dir;
                ///if the point is out of the bounds
                if(current.X < 0 || current.Y < 0 || current.X >= mask.GetLength(0) || current.Y >= mask.GetLength(1))
                    continue;
                ///if this location on the mask is blocked or was already checked
                if(wasChecked[current.X, current.Y] || mask[current.X, current.Y] == true)
                    continue;
                queue.Enqueue(current);
                wasChecked[current.X, current.Y] = true;
                nodesInNextLayer++;
            }
        }
    }
}
