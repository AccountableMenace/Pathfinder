using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;

namespace Pathfinder
{
    public partial class Form1 : Form
    {
        /*
        ---------------TO FIX--------------------
        Greatly reduce lag when saving patterns





        */
        //Variables
        Timer updater = new Timer();
        Point InitialOffset;
        bool drawEraser;
        int drawType = 1;
        bool isBusy = false;
        bool firstTimesettingGridSize = true;


        //Controls
        Button startPathfinding = new Button();
        Button AStarpathFind = new Button();
        Button drawWallBtn = new Button();
        Button drawStartBtn = new Button();
        Button clearGridBtn = new Button();
        Button fillGridBtn = new Button();
        Button clearPathBtn = new Button();
        Button loadPatternBtn = new Button();
        Button gridDimensionsBtn = new Button();
        Button randomBtn = new Button();

        Label distanceLbl = new Label();
        Label timingLbl = new Label();

        //Drawing
        Point cursor;
        Rectangle rect;
        Graphics graphics;

        //Grid (Size determinde from dialog window)
        int[,] grid /*= new int[100,50]*/;
        //Grid size
        Size rectSize;


        public Form1()
        {
            InitializeComponent();
            //Form properties
            this.DoubleBuffered = true;
            this.ClientSize = new Size(1280, 700);
            this.WindowState = FormWindowState.Maximized;
            this.MinimumSize = new Size(510, 480);

            //Control declaration -----------------------------
            //updater Timer
            updater.Interval = 1000;
            updater.Tick += Updater_Tick;

            //startPathfinding Button
            startPathfinding.FlatStyle = FlatStyle.Flat;
            startPathfinding.Text = "Run Search";
            startPathfinding.Click += new EventHandler(this.startPathfinding_click);
            Controls.Add(startPathfinding);

            //AStarpathFind Button
            AStarpathFind.FlatStyle = FlatStyle.Flat;
            AStarpathFind.Text = "Run A* Search";
            AStarpathFind.Click += AStarpathFind_Click;
            Controls.Add(AStarpathFind);

            //clear grid Button
            clearGridBtn.FlatStyle = FlatStyle.Flat;
            clearGridBtn.Text = "Clear Everything";
            clearGridBtn.Click += new EventHandler(this.clearBtn_click);
            Controls.Add(clearGridBtn);

            //clearPath Button
            clearPathBtn.FlatStyle = FlatStyle.Flat;
            clearPathBtn.Text = "Clear Path";
            clearPathBtn.Click += ClearPath_Click;
            Controls.Add(clearPathBtn);

            //fill grid Button
            fillGridBtn.FlatStyle = FlatStyle.Flat;
            fillGridBtn.Text = "Fill Grid With Walls";
            fillGridBtn.Click += new EventHandler(this.fillGridBtn_click);
            Controls.Add(fillGridBtn);

            //drawWall Button
            drawWallBtn.FlatStyle = FlatStyle.Flat;
            drawWallBtn.Text = "Wall";
            drawWallBtn.Click += new EventHandler(this.drawWallBtn_click);
            drawWallBtn.Font = new Font(this.Font, FontStyle.Bold);
            drawWallBtn.BackColor = Color.Black;
            drawWallBtn.ForeColor = Color.White;
            drawWallBtn.FlatAppearance.BorderColor = Color.Red;
            drawWallBtn.FlatAppearance.BorderSize = 2;
            Controls.Add(drawWallBtn);

            //drawStart Button
            drawStartBtn.FlatStyle = FlatStyle.Flat;
            drawStartBtn.Text = "End / Start Point";
            drawStartBtn.Click += new EventHandler(this.drawStartBtn_click);
            drawStartBtn.BackColor = Color.Lime;
            drawStartBtn.FlatAppearance.BorderColor = Color.Red;
            drawStartBtn.FlatAppearance.BorderSize = 0;
            Controls.Add(drawStartBtn);


            //distanceLbl Label
            distanceLbl.Text = "Distance: ";
            Controls.Add(distanceLbl);

            //timingLbl Label
            timingLbl.Text = "Time taken: ";
            Controls.Add(timingLbl);

            //loadPatternBtn Button
            loadPatternBtn.FlatStyle = FlatStyle.Flat;
            loadPatternBtn.Text = "Save / load pattern";
            loadPatternBtn.Click += LoadPatternBtn_Click;
            Controls.Add(loadPatternBtn);

            //gridDimensionsBtn Button
            gridDimensionsBtn.FlatStyle = FlatStyle.Flat;
            gridDimensionsBtn.Text = "Grid size";
            gridDimensionsBtn.Click += GridDimensionsBtn_Click;
            Controls.Add(gridDimensionsBtn);

            //randomBtn Button
            randomBtn.FlatStyle = FlatStyle.Flat;
            randomBtn.Text = "Random Menu";
            randomBtn.Click += RandomBtn_Click;
            Controls.Add(randomBtn);


            ////declaration end ---------------------------------

            GridDimensionsBtn_Click(this, new EventArgs());
        }

        private void RandomBtn_Click(object sender, EventArgs e)
        {
            Random rnd = new Random();
            for (int i = 0; i < grid.GetLength(0); i++)
            {
                for (int j = 0; j < grid.GetLength(1); j++)
                {
                    grid[i, j] = rnd.Next(2);
                }
            }
            this.Invalidate();
        }

        private void GridDimensionsBtn_Click(object sender, EventArgs e)
        {
            Size sendSize;
            //Grid size query on application start
            if (grid != null)
                sendSize = new Size(grid.GetLength(0), grid.GetLength(1));
            else sendSize = new Size(100, 50);

            using (GridPropertiesWindow gridPropertiesWindow = new GridPropertiesWindow(sendSize))
            {
                if (gridPropertiesWindow.ShowDialog() == DialogResult.OK)
                {
                    grid = new int[gridPropertiesWindow.ReturnValues[0], gridPropertiesWindow.ReturnValues[1]];
                    firstTimesettingGridSize = false;
                    UpdateLocations();
                    this.Invalidate();
                }
                else if (firstTimesettingGridSize)
                {
                    Environment.Exit(0);
                }
            }
        }

        private void UpdateLocations()
        {
            //paths{
            //startPathfinding
            startPathfinding.Location = new Point((int)(this.Width * 0.05), (int)(this.Height * 0.01));
            startPathfinding.Size = new Size((int)(this.Width * 0.1), (int)(this.Height * .035));

            //AStarpathFind
            AStarpathFind.Location = new Point((int)(this.Width * 0.05), startPathfinding.Bottom + (int)(this.Height * .005));
            AStarpathFind.Size = new Size((int)(this.Width * 0.1), (int)(this.Height * .035));
            //}
            
            //Clear {
            //clearGridBtn
            clearGridBtn.Location = new Point(startPathfinding.Right + (int)(this.Width * 0.01), startPathfinding.Top);
            clearGridBtn.Size = new Size((int)(this.Width * 0.1), (int)(this.Height * .035));

            //clearPathBtn
            clearPathBtn.Location = new Point(clearGridBtn.Left, clearGridBtn.Bottom + (int)(this.Height * .005));
            clearPathBtn.Size = new Size((int)(this.Width * 0.1), (int)(this.Height * .035));
            // }

            //fillGridBtn
            fillGridBtn.Location = new Point(clearPathBtn.Right + (int)(this.Width * 0.01), startPathfinding.Top);
            fillGridBtn.Size = new Size((int)(this.Width * 0.1), (int)(this.Height * 0.075));


            //Brushes {
            //drawWall
            drawWallBtn.Location = new Point(fillGridBtn.Right + (int)(this.Width * .01), startPathfinding.Top);
            drawWallBtn.Size = new Size((int)(this.Height * .07), (int)(this.Height * .035));

            //drawStart
            drawStartBtn.Location = new Point(drawWallBtn.Left, drawWallBtn.Bottom + (int)(this.Height * .005));
            drawStartBtn.Size = drawWallBtn.Size;
            // }

            //for label size determination
            Size maxSize = TextRenderer.MeasureText("Time taken: 00.000s ", this.Font);

            //distanceLbl Label
            distanceLbl.Location = new Point(drawStartBtn.Right + (int)(this.Width * 0.01), startPathfinding.Top);
            distanceLbl.Size = maxSize;

            //timingLbl Label
            timingLbl.Location = new Point(drawStartBtn.Right + (int)(this.Width * 0.01), (int)(distanceLbl.Bottom));
            timingLbl.Size = maxSize;

            //loadPatternBtn
            loadPatternBtn.Location = new Point(timingLbl.Right + (int)(this.Width * 0.01), startPathfinding.Top);
            loadPatternBtn.Size = new Size((int)(this.Height * 0.120), (int)(this.Height * .075));

            //gridDimensionsBtn 
            gridDimensionsBtn.Location = new Point(loadPatternBtn.Right + (int)(this.Width * 0.01), startPathfinding.Top);
            gridDimensionsBtn.Size = new Size((int)(this.Width * 0.1), (int)(this.Height * 0.075));

            //randomBtn
            randomBtn.Location = new Point(gridDimensionsBtn.Right + (int)(this.Width * 0.01), startPathfinding.Top);
            randomBtn.Size = new Size((int)(this.Width * 0.1), (int)(this.Height * 0.075));


            //Grid offset from top left corner
            InitialOffset = new Point(startPathfinding.Left, (int)(AStarpathFind.Bottom + this.Height * 0.01));


            //Get new max grid size
            Size maxGridCellSize = new Size((int)(((this.ClientSize.Width * 0.97) - InitialOffset.X) / grid.GetLength(0)), (int)(((this.ClientSize.Height * 0.99) - InitialOffset.Y) / grid.GetLength(1)));
            if (maxGridCellSize.Width <= maxGridCellSize.Height) { rectSize = new Size(maxGridCellSize.Width, maxGridCellSize.Width); }
            else { rectSize = new Size(maxGridCellSize.Height, maxGridCellSize.Height); }
        }

        private void drawStartBtn_click(object sender, EventArgs e)
        {
            drawType = 2;
            HighlightSelectedButton("colors");
        }

        private void drawWallBtn_click(object sender, EventArgs e)
        {
            drawType = 1;
            HighlightSelectedButton("colors");
        }

        private void clearBtn_click(object sender, EventArgs e)
        {
            ResetGrid(false, true);
        }
        private void ClearPath_Click(object sender, EventArgs e)
        {
            ResetGrid(true, true);
        }

        private void fillGridBtn_click(object sender, EventArgs e)
        {
            for (int i = 0; i < grid.GetLength(0); i++)
            {
                for (int j = 0; j < grid.GetLength(1); j++)
                {
                    grid[i, j] = 1;
                }
            }
            this.Invalidate();
        }

        private async void startPathfinding_click(object sender, EventArgs e)
        {
            if (isBusy == false)
            {
                isBusy = true;
                Stopwatch pathfindTime = new Stopwatch();
                pathfindTime.Restart();
                updater.Start();
                this.BackColor = ColorTranslator.FromHtml("#bdc3c7");
                Point[] StartEnd = getStartEndPoints();
                
                
                ResetGrid(true, false);
                long[] returnedData = { };
                ToggleButtons(false);
                await Task.Run(() => returnedData = RunPathFindThread(StartEnd[0], StartEnd[1]));
                ToggleButtons(true);

                pathfindTime.Stop();
                long timeTaken = pathfindTime.ElapsedMilliseconds;

                distanceLbl.Text = "Distance : " + returnedData[0];
                timingLbl.Text = "Time taken: " + ((double)timeTaken / 1000).ToString() + " s";
                this.Invalidate();
                this.BackColor = default(Color);
                updater.Stop();
                isBusy = false;
            }
            else MessageBox.Show(this, "Path search already in progress", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private Point[] getStartEndPoints()
        {
            Point Start = new Point(-1, -1), End = Start;
            int count = 0;
            for (int i = 0; i < grid.GetLength(0); i++)
            {
                for (int j = 0; j < grid.GetLength(1); j++)
                {
                    if (grid[i, j] == 2 && count == 0)
                    {
                        count++;
                        Start = new Point(i, j);
                    }
                    else if (grid[i, j] == 2 && count == 1)
                    {
                        count++;
                        End = new Point(i, j);
                        break;
                    }
                }
                if (count == 2) return new Point[] { Start, End };
            }
            return new Point[] { Start, End };
        }

        private async void AStarpathFind_Click(object sender, EventArgs e)
        {
            Stopwatch pathfindTime = new Stopwatch();
            isBusy = true;
            pathfindTime.Restart();
            this.BackColor = ColorTranslator.FromHtml("#bdc3c7");
            ResetGrid(true, false);
            //
            Point[] StartEnd = getStartEndPoints();
            ToggleButtons(false);

            List<Point>[] pathData = new List<Point>[2];
            if (StartEnd[0].X >= 0 && StartEnd[1].X >= 0)
            {
                await Task.Run(() => pathData = aStarPathfind.aStarGetPath(grid, StartEnd[0], StartEnd[1]));
                try
                {
                    string test = pathData[0].ToString();
                    if (test != null)
                    {
                        foreach (var pathCell in pathData[0])
                        {
                            if (grid[pathCell.X, pathCell.Y] != 2)
                            {
                                grid[pathCell.X, pathCell.Y] = 3;
                            }
                        }
                        distanceLbl.Text = "Distance : " + pathData.Count();
                    }
                    else
                    {
                        MessageBox.Show("No Path", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                    }
                    if (pathData[1]?.Count != 0)
                    {
                        foreach (var pathCell in pathData[1])
                        {
                            if (grid[pathCell.X, pathCell.Y] == 0)
                            {
                                grid[pathCell.X, pathCell.Y] = -1;
                            }
                        }
                        distanceLbl.Text = "Distance : " + pathData[0].Count();
                    }
                }
                catch (Exception)
                {
                    MessageBox.Show("Error While Searching For Path", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                }


            } 
            ToggleButtons(true);
            //
            pathfindTime.Stop();
            long timeTaken = pathfindTime.ElapsedMilliseconds;
            timingLbl.Text = "Time taken: " + ((double)timeTaken / 1000).ToString() + " s";
            this.Invalidate();
            this.BackColor = default(Color);
            isBusy = false;
        }

        private void Form1_ResizeEnd(object sender, EventArgs e)
        {
            UpdateLocations();
            this.Invalidate();
        }

        private void Form1_MouseClick(object sender, MouseEventArgs e)
        {
            AddObstacle(e, InitialOffset, rectSize);
        }

        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && drawType == 1)
            {
                AddObstacle(e, InitialOffset, rectSize);
            }
        }



        private Point GetHoveredArrayIndex(Point hoverPoint, Point initOffset, Size size)
        {
            if (hoverPoint.X < initOffset.X || hoverPoint.Y < initOffset.Y)
            {
                return new Point(-1, -1);
            }
            Point offset = new Point(hoverPoint.X - initOffset.X, hoverPoint.Y - initOffset.Y);
            if (size.Width != 0 || size.Height != 0)
            {
                offset.X /= size.Width;
                offset.Y /= size.Height;
            }
            return offset;
        }


        private void AddObstacle(MouseEventArgs eMouse, Point initOffset, Size size)
        {
            if (isBusy == false)
            {
                Point index = GetHoveredArrayIndex(eMouse.Location, initOffset, size);
                //set draw color
                try
                {
                    //check for out of boundaries points
                    if (index.X >= 0 && index.Y >= 0 && index.X < grid.GetLength(0) && index.Y < grid.GetLength(1))
                    {
                        if (drawEraser)
                        {
                            grid[index.X, index.Y] = 0;
                        }
                        else if (drawType == 1)
                        {
                            grid[index.X, index.Y] = 1;
                        }
                        else if (drawType == 2)
                        {
                            grid[index.X, index.Y] = 2;
                        }
                        this.Invalidate(new Rectangle(eMouse.X - size.Width, eMouse.Y - size.Height, size.Width * 2, size.Height * 2));
                    }
                }
                catch (Exception) { }
            }
            else
            {
                MessageBox.Show(this, "Can't draw obstacles while pathfinding!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }


        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            //refresh graphics
            graphics = e.Graphics;

            //get point range which to refresh
            Point from = GetHoveredArrayIndex(Point.Round(e.Graphics.ClipBounds.Location), InitialOffset, rectSize);
            Point to = GetHoveredArrayIndex(Point.Round(new PointF(e.Graphics.ClipBounds.Location.X + e.Graphics.ClipBounds.Width, e.Graphics.ClipBounds.Location.Y + e.Graphics.ClipBounds.Height)), InitialOffset, rectSize);

            //BUG: If atleast one of the values is out of range, the other one will be reset too, which is bad in this case, but I can't explain it , especially over text
            //Use this message box and click on the edge cells, (top right, bottom left) - MessageBox.Show(from.ToString() + " " + to.ToString());
            //check for out of boundaries points

            if (from.X < 0) { from.X = 0; }
            if (from.Y < 0) { from.Y = 0; }
            if (to.X > grid.GetLength(0) - 1) { to.X = grid.GetLength(0) - 1; }
            if (to.Y > grid.GetLength(1) - 1) { to.Y = grid.GetLength(1) - 1; }


            //MessageBox.Show(e.Graphics.ClipBounds.Location.ToString());
            for (int i = from.X; i <= to.X; i++)
            {
                for (int j = from.Y; j <= to.Y; j++)
                {
                    cursor = new Point((i * rectSize.Width + InitialOffset.X), (j * rectSize.Height + InitialOffset.Y));
                    rect = new Rectangle(cursor, rectSize);
                    graphics.DrawRectangle(new Pen(Brushes.Black, 1), rect);

                    if (grid[i, j] == 1)
                    {
                        graphics.FillRectangle(Brushes.Black, rect.X + 1, rect.Y + 1, rect.Width - 1, rect.Height - 1);
                    }
                    else if (grid[i, j] == 2)
                    {
                        graphics.FillRectangle(Brushes.Lime, rect.X + 1, rect.Y + 1, rect.Width - 1, rect.Height - 1);
                    }
                    else if (grid[i, j] == 3)
                    {
                        graphics.FillRectangle(Brushes.Cyan, rect.X + 1, rect.Y + 1, rect.Width - 1, rect.Height - 1);
                    }
                    else if (grid[i, j] < 0)
                    {
                        graphics.FillRectangle(Brushes.Yellow, rect.X + 1, rect.Y + 1, rect.Width - 1, rect.Height - 1);
                    }
                    //draw value of each cell unless it's empty
                    if (grid[i, j] != 0 && TextRenderer.MeasureText("123", this.Font).Width <= rectSize.Width)
                    {
                        graphics.DrawString(grid[i, j].ToString(), this.Font, Brushes.Red, cursor);
                    }
                }
            }
        }

        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            Point index;
            try
            {
                //if mouse clicks on a color with the same type of brush, sets to eraser instead of brush
                //check for out of boundaries points
                index = GetHoveredArrayIndex(e.Location, InitialOffset, rectSize);
                if (index.X >= 0 && index.Y >= 0 && index.X < grid.GetLength(0) && index.Y < grid.GetLength(1) && grid[index.X, index.Y] == drawType)
                    drawEraser = true;
                else drawEraser = false;
            }
            catch (Exception)
            {
                drawEraser = false;
            }
        }



        private long[] RunPathFindThread(Point Start, Point End)
        {
            //returns grid automatically + label values 
            //Task<long[]> FindPathAwait = SamplePathfinding(Start, End, grid);
            return SamplePathfinding(Start, End, grid);

        }
        private static long[] SamplePathfinding(Point start, Point end, int[,] gridData)
        {
            List<SamplePathfindStruct> SamplePFData = new List<SamplePathfindStruct>();
            List<SamplePathfindStruct> adjacentCells = new List<SamplePathfindStruct>();
            // int length = grid.GetLength(0) * grid.GetLength(1);
            var newCell = new SamplePathfindStruct();
            newCell.x = end.X;
            newCell.y = end.Y;
            newCell.count = 0;
            SamplePFData.Add(newCell);

            Stopwatch test1 = new Stopwatch();
            Stopwatch test2 = new Stopwatch();
            Stopwatch test3 = new Stopwatch();
            Stopwatch test4 = new Stopwatch();
            //log time
            bool repeat = true;
            int unseenCellCount;
            do
            {
                unseenCellCount = 0;
                for (int j = 0; j < SamplePFData.Count && repeat; j++)
                {
                    int xOffset, yOffset, gridLengthX = gridData.GetLength(0) - 1, gridLengthY = gridData.GetLength(1) - 1;
                    int val;
                    
                    for (int i = 0; i < 4 && repeat; i++)
                    {
                        if (i == 0) { xOffset = 0; yOffset = 1; }
                        else if (i == 1) { xOffset = 1; yOffset = 0; }
                        else if (i == 2) { xOffset = 0; yOffset = -1; }
                        else { xOffset = -1; yOffset = 0; }
                        newCell.x = SamplePFData[j].x + xOffset;
                        newCell.y = SamplePFData[j].y + yOffset;
                        newCell.count = SamplePFData[j].count + 1;
                        //Prevent out of boundaries for array
                        if (newCell.x < 0) { newCell.x = 0; }
                        if (newCell.y < 0) { newCell.y = 0; }
                        if (newCell.x > gridLengthX) { newCell.x = gridLengthX; }
                        if (newCell.y > gridLengthY) { newCell.y = gridLengthY; }
                        //try
                        //{
                        val = gridData[newCell.x, newCell.y];
                        if (val == 0 || val > 1)
                        {
                            //check if reached the finish
                            if (newCell.x == start.X && newCell.y == start.Y)
                            {
                                repeat = false;
                            }
                            else if (gridData[newCell.x, newCell.y] == 0)
                            {
                                gridData[newCell.x, newCell.y] = -(newCell.count);
                            }
                            adjacentCells.Add(newCell);
                            unseenCellCount++;
                        }
                        //}
                        //catch { Console.WriteLine(String.Format("Out of Bounds, Caught, Cell : {0} {1} {2}", newCell.x.ToString(), newCell.y.ToString(), newCell.count.ToString())); }
                    }
                    //check if there are matching adjacent cells and cells in the main list, where main cell count is less than adjacent cell
                    //if they do - remove them
                    foreach (var mainCell in SamplePFData)
                    {
                        foreach (var adjacentCell in adjacentCells)
                        {
                            if (mainCell.x == adjacentCell.x && mainCell.y == adjacentCell.y && mainCell.count <= adjacentCell.count)
                            {
                                unseenCellCount--;
                                adjacentCells.Remove(adjacentCell);
                                break;
                            }
                        }
                    }
                    SamplePFData.AddRange(adjacentCells);
                    adjacentCells.Clear();
                }
            } while (repeat && unseenCellCount > 0);
            long distance = DrawShortestPath(start, end, gridData);
            return new long[] { distance };

            
        }

        //resets values, e.g. pathfinder negative values or other shit ffs this explanation
        private void ResetGrid(bool negativesOnly, bool refresh)
        {
            for (int i = 0; i < grid.GetLength(0); i++)
            {
                for (int j = 0; j < grid.GetLength(1); j++)
                {
                    if (grid[i, j] < 0 && negativesOnly == true)
                    {
                        grid[i, j] = 0;
                    }
                    else if (negativesOnly == false) grid[i, j] = 0;

                    //independent on negativesOnly value
                    if (grid[i, j] == 3)
                    {
                        grid[i, j] = 0;
                    }
                }
            }
            //whether to refresh the screen
            if (refresh == true)
            {
                this.Invalidate();
            }
        }

        private static int DrawShortestPath(Point Start, Point End, int[,] gridData)
        {
            Point Cursor = Start;
            int distance = 0;
            ////Set grid's start and end point calues to 2, how the should be
            //grid[Start.X, Start.Y] = 2;
            //grid[End.X, End.Y] = 2;

            //until k < a big number, that shouldnt cause any problems
            bool repeat = true;
            for (int k = 0; k < gridData.GetLength(0) * gridData.GetLength(1) && repeat; k++)
            {
                int xOffset, yOffset, index = -1;
                //look in all 4 directions
                int max = 1 - (int)Math.Pow(2, 31), cur = 1 - (int)Math.Pow(2, 31);
                for (int i = 0; i < 4; i++)
                {

                    if (i == 0) { xOffset = 0; yOffset = 1; }
                    else if (i == 1) { xOffset = 1; yOffset = 0; }
                    else if (i == 2) { xOffset = 0; yOffset = -1; }
                    else { xOffset = -1; yOffset = 0; }

                    try
                    {
                        //check for out of boundaries points
                        if (Cursor.X + xOffset >= 0 && Cursor.Y + yOffset >= 0 && Cursor.X + xOffset < gridData.GetLength(0) && Cursor.Y + yOffset < gridData.GetLength(1))
                            cur = gridData[Cursor.X + xOffset, Cursor.Y + yOffset];


                    }
                    catch (Exception) { }
                    if (cur == -1)
                    {
                        index = i;
                        repeat = false;
                        break;
                    }
                    else if (cur > max && cur < 0)
                    {
                        max = cur;
                        index = i;
                    }
                }
                if (index == 0) { Cursor.Y += 1; }
                else if (index == 1) { Cursor.X += 1; }
                else if (index == 2) { Cursor.Y -= 1; }
                else if (index == 3) { Cursor.X -= 1; }

                if (Cursor != Start)
                {
                    gridData[Cursor.X, Cursor.Y] = 3;
                    distance++;
                }
                else if (Cursor == Start)
                {
                    distance++;
                    repeat = false;
                }
                else { MessageBox.Show("Error","Error", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1); break; }
            }
            return distance + 1;
            //distanceLbl.Text = "Distance: " + distance.ToString();
        }

        private void HighlightSelectedButton(string type)
        {
            if (type == "colors")
            {
                if (drawType == 2)
                {
                    drawStartBtn.Font = new Font(this.Font, FontStyle.Bold);
                    drawWallBtn.Font = new Font(this.Font, FontStyle.Regular);
                    drawWallBtn.FlatAppearance.BorderSize = 0;
                    drawStartBtn.FlatAppearance.BorderSize = 2;
                }
                else
                {
                    drawStartBtn.Font = new Font(this.Font, FontStyle.Regular);
                    drawWallBtn.Font = new Font(this.Font, FontStyle.Bold);
                    drawWallBtn.FlatAppearance.BorderSize = 2;
                    drawStartBtn.FlatAppearance.BorderSize = 0;
                }
            }
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            UpdateLocations();
        }

        private void Updater_Tick(object sender, EventArgs e)
        {
            this.Invalidate();
        }
        private void LoadPatternBtn_Click(object sender, EventArgs e)
        {
            using (PatternSelection patternSelection = new PatternSelection(grid))
            {
                if (patternSelection.ShowDialog() == DialogResult.OK)
                {
                    grid = patternSelection.ReturnPattern;
                }
                else { }
                UpdateLocations();
                this.Invalidate();
            }
        }
        private void ToggleButtons(bool enable)
        {
            foreach (Button btn in Controls.OfType<Button>())
            { 
                btn.Enabled = enable; 
            }
        }
    }
}




//<0 - Path length
//1 - Wall
//2 - Start / end point
//3 - Drawn Path