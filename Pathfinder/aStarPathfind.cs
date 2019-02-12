using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
namespace Pathfinder
{
    class aStarPathfind
    {
        public static int[,] debugOnlyCostFromStart { get; set; }
        static AStarMap[,] grid;
        public static List<Point>[] aStarGetPath(int[,] gridData, Point startPoint, Point endPoint, bool debug)
        {
            bool found = false;
            Point neighbouringPoint;
            grid = FormatData(gridData);
            int curCost, yOffset = 0, xOffset = 0;

            //List<Point> evaluated = new List<Point>();
            AStarMap current = new AStarMap();
            List<Point> toEvaluate = new List<Point>();
            Point currentPoint;
            //int evaluatedCount = 0;

            //add starting position to evaluate
            toEvaluate.Add(startPoint);
            grid[startPoint.X, startPoint.Y].costFromStart = 0;
            grid[startPoint.X, startPoint.Y].fullCost = StraightDistance(startPoint, endPoint);
            grid[startPoint.X, startPoint.Y].isEvaluated = false;

            while (toEvaluate.Count > 0)
            {
                currentPoint = FindLowestFullCostLocation(toEvaluate);
                if (currentPoint == endPoint)
                {
                    //Found, now break
                    found = true;
                    break;
                }
                current = grid[currentPoint.X, currentPoint.Y];
                toEvaluate.Remove(currentPoint);
                //evaluated.Add(currentPoint);
                grid[currentPoint.X, currentPoint.Y].isEvaluated = true;
                //foreach existing neighbour
                for (int i = 0; i < 4; i++)
                {
                    xOffset = 0;
                    yOffset = 0;
                    if (i == 0) { yOffset = -1; }
                    else if (i == 1) { xOffset = 1; }
                    else if (i == 2) { yOffset = 1; }
                    else { xOffset = -1; }
                    neighbouringPoint = currentPoint;
                    neighbouringPoint.X += xOffset;
                    neighbouringPoint.Y += yOffset;
                    if (pointExistsAndNotObstacle(neighbouringPoint) && !grid[neighbouringPoint.X, neighbouringPoint.Y].isEvaluated)
                    {
                        curCost = grid[currentPoint.X, currentPoint.Y].costFromStart + 1;
                        if (!toEvaluate.Contains(neighbouringPoint))
                        {
                            toEvaluate.Add(neighbouringPoint);
                        }
                        if (curCost < grid[neighbouringPoint.X, neighbouringPoint.Y].fullCost)
                        {
                            //current best path, record it
                            grid[neighbouringPoint.X, neighbouringPoint.Y].cameFrom = currentPoint;
                            grid[neighbouringPoint.X, neighbouringPoint.Y].costFromStart = curCost;
                            grid[neighbouringPoint.X, neighbouringPoint.Y].fullCost = curCost + StraightDistance(neighbouringPoint, endPoint);
                        }
                    }
                }

            }
            //pathData[0] - Path, pathData[1] = searched cells
            if (debug) debugOnlyCostFromStart = returnCostFromStart(grid);
            if (found) return new List<Point>[] { returnPath(grid, startPoint, endPoint), returnSearched(grid) };
            else return new List<Point>[2];// Point notFound = new Point() { 1, 1 };


        }



        public static AStarMap[,] FormatData(int[,] grid)
        {
            int defVal = (int)Math.Pow(2, 31) - 1;
            AStarMap[,] formatted = new AStarMap[grid.GetLength(0), grid.GetLength(1)];
            for (int i = 0; i < grid.GetLength(0); i++)
            {
                for (int j = 0; j < grid.GetLength(1); j++)
                {
                    formatted[i, j].value = grid[i, j];
                    formatted[i, j].fullCost = defVal;
                    formatted[i, j].costFromStart = defVal;
                }
            }
            return formatted;
        }

        public static int StraightDistance(Point A, Point B)
        {
            //get distance from a to b
            // Δa² + Δb² 
            return (int)Math.Sqrt((A.X - B.X) * (A.X - B.X) + (B.Y - B.Y) * (B.Y - B.Y));
        }
        public static List<Point> returnPath(AStarMap[,] dataSet, Point start, Point cursor)
        {
            //start with end and follow "cameFrom" values, adding them to the array
            List<Point> path = new List<Point>();
            while (cursor != start)
            {
                path.Add(cursor);
                cursor = dataSet[cursor.X, cursor.Y].cameFrom;
            }
            return path;
        }

        public static List<Point> returnSearched(AStarMap[,] dataSet)
        {
            //start with end and follow "cameFrom" values, adding them to the array
            List<Point> path = new List<Point>();
            for (int i = 0; i < dataSet.GetLength(0); i++)
            {
                for (int j = 0; j < dataSet.GetLength(1); j++)
                {
                    if (dataSet[i, j].isEvaluated)
                    {
                        path.Add(new Point(i, j));
                    }

                }
            }
            return path;
        }


        private static Point FindLowestFullCostLocation(List<Point> data)
        {
            int min = (int)Math.Pow(2, 31) - 1;
            Point index = new Point();
            int i = 0;
            foreach (Point point in data)
            {
                if (grid[point.X, point.Y].fullCost < min)
                {
                    min = grid[point.X, point.Y].fullCost;
                    index = data[i];
                }
                i++;
            }
            return index;
        }

        private static bool pointExistsAndNotObstacle(Point point)
        {
            if (point.X >= 0 && point.Y >= 0 && point.X < grid.GetLength(0) && point.Y < grid.GetLength(1) && grid[point.X, point.Y].value != 1)
            {
                return true;
            }
            else return false;
        }

        private static int[,] returnCostFromStart(AStarMap[,] dataSet)
        {
            //return each costfromstart value for debug fun
            int[,] cost = new int[dataSet.GetLength(0), dataSet.GetLength(1)];
            for (int i = 0; i < dataSet.GetLength(0); i++)
            {
                for (int j = 0; j < dataSet.GetLength(1); j++)
                {
                    cost[i,j] = dataSet[i, j].costFromStart;
                }
            }
            return cost;
        }
    }

    //data structure
    struct AStarMap
    {
        public int value { get; set; }
        public int costFromStart { get; set; }
        public int fullCost { get; set; }
        public Point cameFrom { get; set; }
        public bool isEvaluated { get; set; }
    }
}
