using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pathfinder
{
    struct mazeStruct
    {
        public int value { get; set; }
        public bool explored { get; set; }

    }
    class MazeGenerator
    {            
        //Recursive Backtracker
        public mazeStruct[,] GenerateMazeRecursiveBacktrack(Size size)
        {
            mazeStruct[,] grid = new mazeStruct[size.Width, size.Height];
            Random rnd = new Random();
            Point zerozero = new Point(0, 0);

            List<Point> stack = new List<Point>();
            //fill grid
            for (int i = 0; i < size.Width; i++)
            {
                for (int j = 0; j < size.Height; j++)
                {
                    grid[i, j].value = 1;
                    grid[i, j].explored = false;
                }
            }

            grid[0, 0].value = 0;
            int index;
            Point randNeighbour, current = new Point(0, 0);
            grid[0, 0].explored = true;
            do
            {
                //get last in list cell
                index = stack.Count - 1;
                if (index < 0) index = 0;
                //get this point
                Point here;
                try { here = new Point(stack[index].X, stack[index].Y); }
                catch (Exception) { here = zerozero; }

                //get random neighbour
                int[] directions = shuffle(new int[] { 0, 1, 2, 3 }, rnd);
                randNeighbour = getRandUnvisitedNeighbour(current, 0, size, directions, grid); // dont need size, has grid already (technically yes, but it looks better)
                if (randNeighbour != zerozero)
                {
                    //push to stack
                    stack.Add(new Point(current.X, current.Y));

                    //remove wall inbetween
                    grid[(current.X + randNeighbour.X) / 2, (current.Y + randNeighbour.Y) / 2].value = 0;
                    //remove wall on neighbour
                    grid[randNeighbour.X, randNeighbour.Y].value = 0;

                    //set neighbour to current
                    current = new Point(randNeighbour.X, randNeighbour.Y);
                    //mark as explored
                    grid[randNeighbour.X, randNeighbour.Y].explored = true;
                }
                else if (stack.Count > 0)
                {
                    //pop cell from stack
                    current = stack[stack.Count - 1];
                    stack.RemoveAt(stack.Count - 1);

                }
            } while (unexploredExist(grid) /*&& stack.Count > 0*/);
            return grid;
        }

        private bool unexploredExist(mazeStruct[,] grid)
        {
            //check only cells available to the maze
            for (int i = 0; i < grid.GetLength(0); i += 2)
            {
                for (int j = 0; j < grid.GetLength(1); j += 2)
                {
                    if (grid[i, j].explored == false) return true;
                }
            }

            return false;
        }

        public static Point getRandUnvisitedNeighbour(Point point, int index, Size size, int[] neighbourIndexes, mazeStruct[,] gridData)
        {
            int xOffset = 0, yOffset = 0;
            if (index == 4) return new Point();
            else if (neighbourIndexes[index] == 0) yOffset = -2; // 2, because mazes work this way
            else if (neighbourIndexes[index] == 1) xOffset = 2;
            else if (neighbourIndexes[index] == 2) yOffset = 2;
            else if (neighbourIndexes[index] == 3) xOffset = -2;
            if (!pointExistsAndNotVisited(new Point(point.X + xOffset, point.Y + yOffset), size, gridData))
            {
                return getRandUnvisitedNeighbour(point, index + 1, size, neighbourIndexes, gridData);
            }
            else return new Point(point.X + xOffset, point.Y + yOffset);

        }
        private static bool pointExistsAndNotVisited(Point point, Size size, mazeStruct[,] gridData)
        {
            if (point.X >= 0 && point.Y >= 0 && point.X < size.Width && point.Y < size.Height && !gridData[point.X, point.Y].explored)
            {
                return true;
            }
            else return false;
        }

        private static int[] shuffle(int[] values, Random rnd)
        {
            //int temp, index;
            //for (int i = 0; i < values.Count(); i++)
            //{
            //    index = rnd.Next(values.Count());
            //    temp = values[index];
            //    values[index] = values[i];
            //    values[i] = temp;
            //}
            return values.OrderBy(x => rnd.Next()).ToArray();
        }
    }
}
