using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Input;
namespace Pathfinder
{
    public partial class Form1 : Form
    {
        /*
        ---------------TO DO--------------------
        autosave patterns (is that really necessary?)
        Reduce lag on zooming / shifting view (massive grids) (how?)
        Fucking ui (just no)
        Update version numbers

        */
        //Variables
        Timer updater = new Timer();
        //Stopwatch test = new Stopwatch();
        Point InitialOffset;
        bool drawEraser;
        int drawType = 1;
        bool firstTimesettingGridSize = true;
        bool drawCost = false;
        bool controlDown = false;
        bool shiftDown = false;
        bool xDown = false;
        string version = "v1.1.0";
        DateTime startTime = DateTime.Now;
        List<Control> controlsToUpdate = new List<Control>();
        Point topLeftIndex = new Point(0, 0);

        //Resources
        Image saveIcon = Pathfinder.Properties.Resources.saveIcon;
        Image settingsIcon = Pathfinder.Properties.Resources.settingsIcon;
        Image randomIcon = Pathfinder.Properties.Resources.randomIcon;
        Image brushIcon = Pathfinder.Properties.Resources.brushIcon;
        Image eraseIcon = Pathfinder.Properties.Resources.eraseIcon;
        Image fillIcon = Pathfinder.Properties.Resources.fillIcon;
        Image pathIcon = Pathfinder.Properties.Resources.pathIcon;
        Image zoomIconPlus = Pathfinder.Properties.Resources.zoomIconPlus;
        Image zoomIconMinus = Pathfinder.Properties.Resources.zoomIconMinus;
        Image resetIcon = Pathfinder.Properties.Resources.resetIcon;
        Image helpIcon = Pathfinder.Properties.Resources.helpIcon;
        //Settings
        bool gridBoundary = true, debug = false;

        //Controls 
        VScrollBar vScrollBar = new VScrollBar();
        HScrollBar hScrollBar = new HScrollBar();
        //StatusBar
        StatusBar statusBar = new StatusBar();
        StatusBarPanel gridSizePanel = new StatusBarPanel();
        StatusBarPanel distancePanel = new StatusBarPanel();
        StatusBarPanel timingPanel = new StatusBarPanel();
        StatusBarPanel stepCountPanel = new StatusBarPanel();
        //

        Button AStarpathFind = new Button();
        Button startPathfinding = new Button();
        Button clearGridBtn = new Button();
        Button clearPathBtn = new Button();
        Button fillGridBtn = new Button();
        Button drawWallBtn = new Button();
        Button drawStartBtn = new Button();
        Button mazeBtn = new Button();
        Button settingsBtn = new Button();
        Button loadPatternBtn = new Button();
        Button zoomInBtn = new Button();
        Button resetViewBtn = new Button();
        Button zoomOutBtn = new Button();
        Button helpBtn = new Button();

        //Drawing
        Point cursor;
        Rectangle rect;
        Graphics graphics;

        //Grid (Size determinde from dialog window)
        int[,] grid;
        //Grid size
        Size rectSize;
        int[,] debugOnlyCostFromStart;


        //detect isBusy value change
        private bool _isBusy;
        public event System.EventHandler BusyChanged;
        protected virtual void OnBusyChanged()
        {
            if (BusyChanged != null) BusyChanged(this, EventArgs.Empty);
            foreach (Button btn in Controls.OfType<Button>())
            {
                btn.Enabled = !isBusy;
            }
        }
        public bool isBusy
        {
            get { return _isBusy; }
            set
            {
                _isBusy = value;
                OnBusyChanged();
            }
        }

        public Form1()
        {
            InitializeComponent();
            //zoom detection
            this.MouseWheel += Form1_MouseWheel;
            this.KeyDown += Form1_KeyDown;
            this.KeyUp += Form1_KeyUp;

            Console.WriteLine(getTime() + "Initialized");
            //Form properties
            this.DoubleBuffered = true;
            this.ClientSize = new Size(1280, 700);
            this.WindowState = FormWindowState.Maximized;
            this.MinimumSize = new Size(510, 480);
            this.Text = "Pathfinder " + version + " by Accountable Menace";
            //allow key down detection anywhere
            this.KeyPreview = true;

            //Control initialization             
            InitializeAll();
            //Who settings menu
            settingsBtn_Click(this, new EventArgs());
            UpdateLayout();

            resetCellSize();
        }

        private async void GenerateMaze_Click(object sender, EventArgs e)
        {
            using (MazeGenerationConfig mazeGeneratorWindow = new MazeGenerationConfig(new Size(grid.GetLength(0), grid.GetLength(1))))
            {
                if (mazeGeneratorWindow.ShowDialog() == DialogResult.OK)
                {
                    //grid = patternSelection.ReturnPattern;
                    if (mazeGeneratorWindow.algorithm == 0)
                    {
                        isBusy = true;
                        MazeGenerator mazeGenerator = new MazeGenerator();
                        mazeStruct[,] maze = new mazeStruct[0, 0];/* = mazeGenerator.GenerateMazeRecursiveBacktrack(new Size(grid.GetLength(0), grid.GetLength(1)));*/
                        await Task.Run(() => maze = mazeGenerator.GenerateMazeRecursiveBacktrack(new Size(grid.GetLength(0), grid.GetLength(1))));
                        for (int i = 0; i < maze.GetLength(0); i++)
                        {
                            for (int j = 0; j < maze.GetLength(1); j++)
                            {
                                grid[i, j] = maze[i, j].value;
                            }
                        }
                        isBusy = false;
                    }

                    try
                    {
                        grid[mazeGeneratorWindow.StartPoint.X, mazeGeneratorWindow.StartPoint.Y] = 2;
                        grid[mazeGeneratorWindow.EndPoint.X, mazeGeneratorWindow.EndPoint.Y] = 2;
                    }
                    catch (Exception) { MessageBox.Show("Invalid coordinates selected"); }
                    await Task.Run(() => this.Invalidate());
                }

            }
        }

        private void settingsBtn_Click(object sender, EventArgs e)
        {
            Size sendSize;
            //Grid size query on application start
            if (grid != null)
                sendSize = new Size(grid.GetLength(0), grid.GetLength(1));
            else sendSize = new Size(100, 50);

            using (SettingsWindow settingsWindow = new SettingsWindow(sendSize, gridBoundary, debug))
            {
                if (settingsWindow.ShowDialog() == DialogResult.OK)
                {
                    //get new size
                    //only if size is different
                    int[] newsize = new int[] { settingsWindow.returnSize[0], settingsWindow.returnSize[1] };
                    if (firstTimesettingGridSize || (grid.GetLength(0) != newsize[0] || grid.GetLength(1) != newsize[1]))
                        grid = new int[newsize[0], newsize[1]];
                    //get settings
                    gridBoundary = settingsWindow.returnSettings[0];
                    debug = settingsWindow.returnSettings[1];
                    if (debug) MessageBox.Show("Debug mode is on, you may experience bugs!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    firstTimesettingGridSize = false;
                    UpdateLayout();
                    resetCellSize();
                    this.Invalidate();
                }
                else if (firstTimesettingGridSize)
                {
                    Environment.Exit(0);
                }
            }
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
            Task.Run(() => this.Invalidate());
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
                await Task.Run(() => returnedData = RunPathFindThread(StartEnd[0], StartEnd[1]));

                pathfindTime.Stop();
                long timeTaken = pathfindTime.ElapsedMilliseconds;

                distancePanel.Text = "Distance: " + returnedData[0];
                timingPanel.Text = "Time taken: " + ((double)timeTaken / 1000).ToString() + " s";
                this.Invalidate();
                this.BackColor = default(Color);
                updater.Stop();
                isBusy = false;
            }
            else MessageBox.Show(this, "Path search already in progress", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            statusBarUpdateGridSize();
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
            Console.Write(getTime() + "A* Search started ");
            Stopwatch pathfindTime = new Stopwatch();
            isBusy = true;
            pathfindTime.Restart();
            this.BackColor = ColorTranslator.FromHtml("#bdc3c7");
            ResetGrid(true, false);
            distancePanel.Text = "Distance: N/A";
            //
            Point[] StartEnd = getStartEndPoints();
            Console.Write("at positions " + StartEnd[0] + " and " + StartEnd[1] + "\n");

            List<Point>[] pathData;
            pathData = new List<Point>[2];


            if (StartEnd[0].X >= 0 && StartEnd[1].X >= 0)
            {
                //pathData[0] - Path, pathData[1] = searched cells
                await Task.Run(() => pathData = aStarPathfind.aStarGetPath(grid, StartEnd[0], StartEnd[1], debug));
                try
                {
                    if (pathData[0] != null)
                    {
                        foreach (var pathCell in pathData[0])
                        {
                            if (grid[pathCell.X, pathCell.Y] != 2)
                            {
                                grid[pathCell.X, pathCell.Y] = 3;
                            }
                        }
                        distancePanel.Text = "Distance: " + pathData[0].Count();
                    }
                    else
                    {
                        distancePanel.Text = "Distance: N/A";
                        pathfindTime.Stop();
                        Console.WriteLine(getTime() + "Couldn't find path, Start " + StartEnd[0].ToString() + ", goal " + StartEnd[1].ToString());
                        MessageBox.Show("No Path", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                        pathfindTime.Start();
                    }
                    if (pathData[1] != null)
                    {
                        foreach (var pathCell in pathData[1])
                        {
                            if (grid[pathCell.X, pathCell.Y] == 0)
                            {
                                grid[pathCell.X, pathCell.Y] = -1;
                            }
                        }
                    }
                }
                catch (Exception er)
                {
                    pathfindTime.Stop();
                    Console.WriteLine(getTime() + "Error While Searching For Path, caught exception: \n   " + er);
                    MessageBox.Show("Error While Searching For Path", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                    pathfindTime.Start();
                }


            }
            if (debug) debugOnlyCostFromStart = aStarPathfind.debugOnlyCostFromStart;
            //
            drawCost = true;
            pathfindTime.Stop();
            long timeTaken = pathfindTime.ElapsedMilliseconds;
            timingPanel.Text = "Time taken: " + ((double)timeTaken / 1000).ToString() + " s";
            stepCountPanel.Text = "Step count: " + aStarPathfind.stepCount;
            this.Invalidate();
            this.BackColor = default(Color);
            isBusy = false;
            statusBarUpdateGridSize();
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
            AStarpathFind.Focus();
            if (isBusy == false)
            {
                Point index = GetHoveredArrayIndex(eMouse.Location, initOffset, size);
                //add zoom offset
                index.X += topLeftIndex.X;
                index.Y += topLeftIndex.Y;
                //set draw color
                try
                {
                    //check for out of boundaries points
                    if (index.X >= 0 && index.Y >= 0 && index.X < grid.GetLength(0) && index.Y < grid.GetLength(1))
                    {
                        if (debug) debugOnlyCostFromStart = null;
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
                catch (Exception er) { Console.WriteLine(getTime() + "Error, caught exception: \n   " + er); }
            }
            else
            {
                //MessageBox.Show(this, "Can't draw obstacles while pathfinding!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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


            //Make this look better, there shouldn't be code literally repeating twice like this

            //Do not display grid out of set boundaries
            if (from.X < 0) { from.X = 0; }
            if (from.Y < 0) { from.Y = 0; }
            //if (to.X > grid.GetLength(0) - 1) { to.X = grid.GetLength(0) - 1; }
            //if (to.Y > grid.GetLength(1) - 1) { to.Y = grid.GetLength(1) - 1; }

            //Zoom offset
            from.X += topLeftIndex.X;
            from.Y += topLeftIndex.Y;
            to.X += topLeftIndex.X;
            to.Y += topLeftIndex.Y;
            //Check if not out of bounds again
            if (from.X < 0) { from.X = 0; }
            if (from.Y < 0) { from.Y = 0; }
            if (to.X > grid.GetLength(0) - 1) { to.X = grid.GetLength(0) - 1; }
            if (to.Y > grid.GetLength(1) - 1) { to.Y = grid.GetLength(1) - 1; }

            for (int i = from.X; i <= to.X; i++)
            {
                for (int j = from.Y; j <= to.Y; j++)
                {
                    cursor = new Point(((i - topLeftIndex.X) * rectSize.Width + InitialOffset.X), ((j - topLeftIndex.Y) * rectSize.Height + InitialOffset.Y));
                    rect = new Rectangle(cursor, rectSize);
                    //draw rectangle boundaries
                    int offset = 0;
                    //If boundary enabled
                    if (gridBoundary)
                    { //draw rectangles and set offset
                        graphics.DrawRectangle(new Pen(Brushes.Black, 1), rect);
                        offset = 1;
                    }

                    if (grid[i, j] == 1)
                    {
                        graphics.FillRectangle(Brushes.Black, rect.X + offset, rect.Y + offset, rect.Width - offset, rect.Height - offset);
                    }
                    else if (grid[i, j] == 2)
                    {
                        graphics.FillRectangle(new SolidBrush(ColorTranslator.FromHtml("#4cd137")), rect.X + offset, rect.Y + offset, rect.Width - offset, rect.Height - offset);
                    }
                    else if (grid[i, j] == 3)
                    {
                        graphics.FillRectangle(Brushes.Cyan, rect.X + offset, rect.Y + offset, rect.Width - offset, rect.Height - offset);
                    }
                    else if (grid[i, j] < 0)
                    {
                        graphics.FillRectangle(Brushes.Yellow, rect.X + offset, rect.Y + offset, rect.Width - offset, rect.Height - offset);
                    }
                    //draw value of each cell unless it's empty
                    if ((grid[i, j] == 3 || grid[i, j] == -1) /*&& TextRenderer.MeasureText("123", this.Font).Width <= rectSize.Width*/)
                    {
                        //graphics.DrawString(grid[i, j].ToString(), this.Font, Brushes.Red, cursor);
                        if (drawCost && debug && debugOnlyCostFromStart != null) graphics.DrawString(debugOnlyCostFromStart[i, j].ToString(), this.Font, Brushes.Red, cursor);
                    }
                }
            }
            //if grid visibility setting is disabled 
            if (!gridBoundary)
            {
                //draw outside grid boundary, if set to not display each cell
                //and make the grid move, when shifting the view
                //and make it thicc
                Point startDrawPoint = new Point(InitialOffset.X - topLeftIndex.X * rectSize.Width + 1, InitialOffset.Y - topLeftIndex.Y * rectSize.Height + 1);
                Rectangle drawArea = new Rectangle(startDrawPoint.X, startDrawPoint.Y, rect.Width * grid.GetLength(0), rect.Height * grid.GetLength(1));
                graphics.DrawRectangle(new Pen(Brushes.Black, 2), drawArea);

                //remove everything left of the left margin
                graphics.FillRectangle(new SolidBrush(this.BackColor), 0, 0, InitialOffset.X, ClientSize.Height);
            }
        }

        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            Point index;
            try
            {
                //if mouse clicks on a color with the same type of brush, sets to eraser instead of brush
                //check for out of boundaries points
                index = GetHoveredArrayIndex(e.Location, new Point(InitialOffset.X - topLeftIndex.X * rectSize.Width, InitialOffset.Y - topLeftIndex.Y * rectSize.Height), rectSize);
                if (index.X >= 0 && index.Y >= 0 && index.X < grid.GetLength(0) && index.Y < grid.GetLength(1) && grid[index.X, index.Y] == drawType)
                    drawEraser = true;
                else drawEraser = false;
            }
            catch (Exception er)
            {
                Console.WriteLine(getTime() + "Error, caught exception: \n   " + er);
                drawEraser = false;
            }
        }



        private long[] RunPathFindThread(Point Start, Point End)
        {
            //returns grid automatically + label values 
            return FloodPathfinding(Start, End, grid);

        }
        private static long[] FloodPathfinding(Point start, Point end, int[,] gridData)
        {
            List<FloodPathfindStruct> FloodPFData = new List<FloodPathfindStruct>();
            List<FloodPathfindStruct> adjacentCells = new List<FloodPathfindStruct>();
            // int length = grid.GetLength(0) * grid.GetLength(1);
            var newCell = new FloodPathfindStruct();
            newCell.x = end.X;
            newCell.y = end.Y;
            newCell.count = 0;
            FloodPFData.Add(newCell);

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
                for (int j = 0; j < FloodPFData.Count && repeat; j++)
                {
                    int xOffset, yOffset, gridLengthX = gridData.GetLength(0) - 1, gridLengthY = gridData.GetLength(1) - 1;
                    int val;

                    for (int i = 0; i < 4 && repeat; i++)
                    {
                        if (i == 0) { xOffset = 0; yOffset = 1; }
                        else if (i == 1) { xOffset = 1; yOffset = 0; }
                        else if (i == 2) { xOffset = 0; yOffset = -1; }
                        else { xOffset = -1; yOffset = 0; }
                        newCell.x = FloodPFData[j].x + xOffset;
                        newCell.y = FloodPFData[j].y + yOffset;
                        newCell.count = FloodPFData[j].count + 1;
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
                    foreach (var mainCell in FloodPFData)
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
                    FloodPFData.AddRange(adjacentCells);
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
                    catch (Exception er) { Console.WriteLine("(Time unavailable) " + "Error, caught exception: \n   " + er); }
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
                else { MessageBox.Show("Error", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1); break; }
            }
            return distance + 1;
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
                    drawStartBtn.FlatAppearance.BorderSize = 1;
                }
                else
                {
                    drawStartBtn.Font = new Font(this.Font, FontStyle.Regular);
                    drawWallBtn.Font = new Font(this.Font, FontStyle.Bold);
                    drawWallBtn.FlatAppearance.BorderSize = 1;
                    drawStartBtn.FlatAppearance.BorderSize = 0;
                }
            }
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            UpdateLayout();
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
                UpdateLayout();
                resetCellSize();
                this.Invalidate();
            }
        }
        TimeSpan timeFromStart;
        private string getTime()
        {
            timeFromStart = DateTime.Now - startTime;

            return timeFromStart.Hours + ":" + timeFromStart.Minutes + ":" + timeFromStart.Seconds + " >> ";
        }
        private void UpdateLayout()
        {
            //reset zoom and offset
            topLeftIndex = new Point(0, 0);
            int Hpadding = (int)(this.Width * 0.01);
            int Vpadding = (int)(this.Height * .005);
            Point TopLeftButtonPoint = new Point((int)(this.Width * 0.05), (int)(this.Height * 0.01));
            Point TopRightButtonPoint = new Point(ClientSize.Width - vScrollBar.Width - Hpadding, (int)(this.Height * 0.01));
            Size fullButtonSize = new Size((int)(this.Width * 0.1), (int)(this.Height * 0.075));
            Size halfButtonSize = new Size((int)(this.Width * 0.1), (int)(this.Height * 0.035));
            //set minimum size for buttons
            //fulsized
            if (fullButtonSize.Width < 140)
                fullButtonSize.Width = 140;
            if (fullButtonSize.Height < 60)
                fullButtonSize.Height = 60;
            //halfsized
            if (halfButtonSize.Width < 140)
                halfButtonSize.Width = 140;
            if (halfButtonSize.Height < 28)
                halfButtonSize.Height = 28;

            //if (fontSizeHalf < 13) fontSizeHalf = 13;
            Font fontFull = new Font("Arial", (int)(fullButtonSize.Height * 0.2), GraphicsUnit.Pixel);

            //Docked controls
            //vScrollBar
            vScrollBar.Maximum = grid.GetLength(1) * 3;
            vScrollBar.Value = (int)(vScrollBar.Maximum / 2);
            //hScrollBar
            hScrollBar.Maximum = grid.GetLength(0) * 3;
            hScrollBar.Value = (int)(hScrollBar.Maximum / 2);

            //AStarpathFind
            AStarpathFind.Location = TopLeftButtonPoint;
            AStarpathFind.Size = fullButtonSize;
            AStarpathFind.Font = fontFull;
            setupButtonIcon(AStarpathFind, pathIcon);

            //clearGridBtn
            clearGridBtn.Location = new Point(AStarpathFind.Right + Hpadding, TopLeftButtonPoint.Y);
            clearGridBtn.Size = halfButtonSize;
            setupButtonIcon(clearGridBtn, eraseIcon);

            //clearPathBtn
            clearPathBtn.Location = new Point(clearGridBtn.Left, clearGridBtn.Bottom + Vpadding);
            clearPathBtn.Size = halfButtonSize;
            setupButtonIcon(clearPathBtn, eraseIcon);

            //fillGridBtn
            fillGridBtn.Location = new Point(clearPathBtn.Right + Hpadding, TopLeftButtonPoint.Y);
            fillGridBtn.Size = halfButtonSize;
            setupButtonIcon(fillGridBtn, fillIcon);


            //Brushes {
            //drawWall
            drawWallBtn.Location = new Point(fillGridBtn.Left, TopLeftButtonPoint.Y + halfButtonSize.Height + Vpadding);
            drawWallBtn.Size = new Size((int)(0.5 * (halfButtonSize.Width - Hpadding)), halfButtonSize.Height);
            //set icon to brush
            drawWallBtn.Image = new Bitmap(brushIcon, (int)(drawWallBtn.Height * .8), (int)(drawWallBtn.Height * .8));

            //drawStart
            drawStartBtn.Location = new Point(drawWallBtn.Left + drawWallBtn.Width + Hpadding, drawWallBtn.Top);
            drawStartBtn.Size = drawWallBtn.Size;
            drawStartBtn.Image = new Bitmap(brushIcon, (int)(drawStartBtn.Height * .8), (int)(drawStartBtn.Height * .8));
            // }

            //for label size determination
            Size maxSize = TextRenderer.MeasureText("Time taken: 00.000s ", this.Font);

            //mazeBtn
            mazeBtn.Location = new Point(drawStartBtn.Right + Hpadding, TopLeftButtonPoint.Y);
            mazeBtn.Font = fontFull;
            mazeBtn.Size = fullButtonSize;
            //mazeBtn.Font = fontFull;
            setupButtonIcon(mazeBtn, randomIcon);

            //settingsBtn 
            settingsBtn.Location = new Point(mazeBtn.Right + Hpadding, TopLeftButtonPoint.Y);
            settingsBtn.Size = halfButtonSize;
            setupButtonIcon(settingsBtn, settingsIcon);

            //loadPatternBtn
            loadPatternBtn.Location = new Point(settingsBtn.Left, TopLeftButtonPoint.Y + halfButtonSize.Height + Vpadding);
            loadPatternBtn.Size = halfButtonSize;
            setupButtonIcon(loadPatternBtn, saveIcon);


            //Right allign

            //helpBtn
            helpBtn.Size = new Size(halfButtonSize.Height, halfButtonSize.Height);
            helpBtn.Location = new Point(TopRightButtonPoint.X - helpBtn.Width, TopRightButtonPoint.Y);
            setupButtonIcon(helpBtn, helpIcon);
            helpBtn.ImageAlign = ContentAlignment.MiddleCenter;//manually reset alignment

            //zoomOutBtn
            zoomOutBtn.Size = new Size(halfButtonSize.Height, halfButtonSize.Height);
            zoomOutBtn.Location = new Point(helpBtn.Left - Hpadding / 2 - zoomOutBtn.Width, TopRightButtonPoint.Y);
            setupButtonIcon(zoomOutBtn, zoomIconMinus);
            zoomOutBtn.ImageAlign = ContentAlignment.MiddleCenter;//manually reset alignment

            //resetViewBtn
            resetViewBtn.Size = new Size(halfButtonSize.Height, halfButtonSize.Height);
            resetViewBtn.Location = new Point(zoomOutBtn.Left - Hpadding / 2 - resetViewBtn.Width, TopRightButtonPoint.Y);
            setupButtonIcon(resetViewBtn, resetIcon);
            resetViewBtn.ImageAlign = ContentAlignment.MiddleCenter;//manually reset alignment

            //zoomInBtn
            zoomInBtn.Size = new Size(halfButtonSize.Height, halfButtonSize.Height);
            zoomInBtn.Location = new Point(resetViewBtn.Left - Hpadding / 2 - zoomInBtn.Width, TopRightButtonPoint.Y);
            setupButtonIcon(zoomInBtn, zoomIconPlus);
            zoomInBtn.ImageAlign = ContentAlignment.MiddleCenter;//manually reset alignment


            //Grid offset from top left corner
            InitialOffset = new Point(TopLeftButtonPoint.X, TopLeftButtonPoint.Y + fullButtonSize.Height + 2 * Vpadding);



            //Debug layout 
            if (debug)
            {
                //AStarpathFind
                AStarpathFind.Location = TopLeftButtonPoint;
                AStarpathFind.Size = halfButtonSize;
                AStarpathFind.Font = this.Font;
                setupButtonIcon(AStarpathFind, pathIcon);

                //startPathfinding
                startPathfinding.Location = new Point(AStarpathFind.Left, TopLeftButtonPoint.Y + halfButtonSize.Height + Vpadding);
                startPathfinding.Size = halfButtonSize;
                startPathfinding.Visible = true;
            }
            else
            {
                //Reset only the visibilities
                startPathfinding.Visible = false;
            }
            statusBarUpdateGridSize();
        }

        private async void Form1_ResizeEnd(object sender, EventArgs e)
        {
            UpdateLayout();
            await Task.Run(() => this.Invalidate());
        }
        //Call ResizeEnd event when clicking maximise buttons or double clicking title bar
        const int WM_SYSCOMMAND = 0x0112;
        const int SC_MAXIMIZE = 0xF030;
        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            int wParam = (m.WParam.ToInt32() & 0xFFF0);
            if (wParam == 0xF030 || wParam == 0xF020 || wParam == 0xF120)
            {
                this.OnResizeEnd(EventArgs.Empty);
            }
        }

        private void InitializeAll()
        {
            ToolTip ResetTooltip = new ToolTip();
            //updater Timer
            updater.Interval = 1000;
            updater.Tick += Updater_Tick;


            //statusBar StatusBar
            statusBar.Dock = DockStyle.Bottom;
            statusBar.ShowPanels = true;
            statusBar.Panels.Add(gridSizePanel);
            gridSizePanel.AutoSize = StatusBarPanelAutoSize.Contents;
            statusBar.Panels.Add(distancePanel);
            distancePanel.AutoSize = StatusBarPanelAutoSize.Contents;
            statusBar.Panels.Add(timingPanel);
            timingPanel.AutoSize = StatusBarPanelAutoSize.Contents;
            statusBar.Panels.Add(stepCountPanel);
            stepCountPanel.AutoSize = StatusBarPanelAutoSize.Contents;
            Controls.Add(statusBar);

            //Vertical Scroll bar
            //vScrollBar.Size = new Size(20, ClientSize.Height);
            vScrollBar.Dock = DockStyle.Right;
            vScrollBar.ValueChanged += VScrollBar_ValueChanged;
            Controls.Add(vScrollBar);

            //Horizontal Scroll bar
            hScrollBar.Dock = DockStyle.Bottom;
            hScrollBar.ValueChanged += HScrollBar_ValueChanged;
            Controls.Add(hScrollBar);


            //AStarpathFind Button
            AStarpathFind.FlatStyle = FlatStyle.Flat;
            AStarpathFind.Text = " A* Pathfind";
            AStarpathFind.Click += AStarpathFind_Click;
            Controls.Add(AStarpathFind);

            //startPathfinding Button
            startPathfinding.FlatStyle = FlatStyle.Flat;
            startPathfinding.Text = "Run Flood Search";
            startPathfinding.Click += new EventHandler(this.startPathfinding_click);
            startPathfinding.Visible = false;
            ResetTooltip.SetToolTip(startPathfinding, "Very inneficient");
            Controls.Add(startPathfinding);

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
            //drawWallBtn.Text = "Wall";
            drawWallBtn.Click += new EventHandler(this.drawWallBtn_click);
            drawWallBtn.Font = new Font(this.Font, FontStyle.Bold);
            drawWallBtn.BackColor = Color.Black;
            drawWallBtn.ForeColor = Color.White;
            drawWallBtn.FlatAppearance.BorderColor = Color.Red;
            drawWallBtn.FlatAppearance.BorderSize = 1;
            ResetTooltip.SetToolTip(drawWallBtn, "Draw walls");
            Controls.Add(drawWallBtn);

            //drawStart Button
            drawStartBtn.FlatStyle = FlatStyle.Flat;
            //drawStartBtn.Text = "Start / End Point";
            drawStartBtn.Click += new EventHandler(this.drawStartBtn_click);
            drawStartBtn.BackColor = (Color)ColorTranslator.FromHtml("#4cd137");
            drawStartBtn.FlatAppearance.BorderColor = Color.Red;
            drawStartBtn.FlatAppearance.BorderSize = 0;
            ResetTooltip.SetToolTip(drawStartBtn, "Set start / end points");
            Controls.Add(drawStartBtn);

            //mazeBtn Button
            mazeBtn.FlatStyle = FlatStyle.Flat;
            mazeBtn.Text = " Maze Menu";
            mazeBtn.Click += GenerateMaze_Click;
            Controls.Add(mazeBtn);

            //settingsBtn Button
            settingsBtn.FlatStyle = FlatStyle.Flat;
            settingsBtn.Text = "Settings";
            settingsBtn.Click += settingsBtn_Click;
            Controls.Add(settingsBtn);

            //loadPatternBtn Button
            loadPatternBtn.FlatStyle = FlatStyle.Flat;
            loadPatternBtn.Text = " Save / Load Pattern";
            loadPatternBtn.Click += LoadPatternBtn_Click;
            Controls.Add(loadPatternBtn);

            //zoomInBtn Button
            zoomInBtn.FlatStyle = FlatStyle.Flat;
            zoomInBtn.Click += ZoomInBtn_Click;
            ResetTooltip.SetToolTip(zoomInBtn, "Zoom in");
            Controls.Add(zoomInBtn);

            //resetViewBtn Button
            resetViewBtn.FlatStyle = FlatStyle.Flat;
            resetViewBtn.Click += ResetViewBtn_Click;
            ResetTooltip.SetToolTip(resetViewBtn, "Reset to default view");
            Controls.Add(resetViewBtn);

            //zoomOutBtn Button
            zoomOutBtn.FlatStyle = FlatStyle.Flat;
            zoomOutBtn.Click += ZoomOutBtn_Click;
            ResetTooltip.SetToolTip(zoomOutBtn, "Zoom out");
            Controls.Add(zoomOutBtn);

            //helpBtn Button
            helpBtn.FlatStyle = FlatStyle.Flat;
            helpBtn.Click += HelpBtn_Click;
            ResetTooltip.SetToolTip(helpBtn, "See keyboard shortcuts");
            Controls.Add(helpBtn);
        }

        private void setupButtonIcon(Button button, Image icon)
        {// sets and alligns an image to button
            button.Image = new Bitmap(icon, (int)(button.Height * .5), (int)(button.Height * .5));
            button.ImageAlign = ContentAlignment.MiddleLeft;
            button.TextAlign = ContentAlignment.MiddleRight;
            button.TextImageRelation = TextImageRelation.ImageBeforeText;

        }

        private void Form1_MouseWheel(object sender, MouseEventArgs e)
        {
            //Set offset, depending on whether the user is holding down x
            int vOffsetAmount = xDown ? (int)Math.Ceiling(this.Height / rectSize.Height * 0.2) : (int)Math.Ceiling(this.Height / rectSize.Height * 0.04);
            int hOffsetAmount = xDown ? (int)Math.Ceiling(this.Height / rectSize.Height * 0.2) : (int)Math.Ceiling(this.Height / rectSize.Height * 0.04);
            //Zoom in with control
            if (controlDown)
            {
                Zoom(e.Delta > 0 ? true : false);
            }
            //Move sideways with shift
            else if (shiftDown)
            {
                if (e.Delta > 0 && hScrollBar.Value - hOffsetAmount >= 0)
                {
                    hScrollBar.Value -= hOffsetAmount;
                    //this.Invalidate();
                }
                else if (hScrollBar.Value + hOffsetAmount <= hScrollBar.Maximum)
                {
                    hScrollBar.Value += hOffsetAmount;
                    //this.Invalidate();
                }
            }
            //Move up / down by default
            else
            {
                if (e.Delta > 0 && vScrollBar.Value - vOffsetAmount >= 0)
                {
                    vScrollBar.Value -= vOffsetAmount;
                    //this.Invalidate();
                }
                else if (vScrollBar.Value + vOffsetAmount <= vScrollBar.Maximum)
                {
                    vScrollBar.Value += vOffsetAmount;
                    //this.Invalidate();
                }
                //vScrollBar.Value = topLeftIndex.Y + (int)(grid.GetLength(1) * 1.5);

            }

        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            controlDown = e.Control;
            shiftDown = e.Shift;
            if (e.KeyCode == Keys.X) xDown = true;
        }

        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            controlDown = e.Control;
            shiftDown = e.Shift;
            if (e.KeyCode == Keys.X) xDown = false;
        }

        private async void Zoom(bool zoomIn)
        {
            double offsetAmount = xDown ? .3 : .1;
            if (zoomIn)
            {
                rectSize = Size.Add(rectSize, new Size((int)Math.Ceiling(rectSize.Width * offsetAmount), (int)Math.Ceiling(rectSize.Height * offsetAmount)));
            }
            else if (rectSize.Height > 1)
            {
                rectSize = Size.Subtract(rectSize, new Size((int)Math.Ceiling(rectSize.Width * offsetAmount), (int)Math.Ceiling(rectSize.Height * offsetAmount)));
            }
            await Task.Run(() => this.Invalidate());
        }

        private void ZoomOutBtn_Click(object sender, EventArgs e)
        {
            Zoom(false);
        }

        private void ResetViewBtn_Click(object sender, EventArgs e)
        {
            resetCellSize();
            UpdateLayout();
            this.Invalidate();
        }

        private void ZoomInBtn_Click(object sender, EventArgs e)
        {
            Zoom(true);
        }
        //updateTopLeftIndex when shifting view
        private void HScrollBar_ValueChanged(object sender, EventArgs e)
        {
            topLeftIndex.X = hScrollBar.Value - (int)(grid.GetLength(0) * 1.5);
            this.Invalidate();
        }
        //updateTopLeftIndex when shifting view
        private void VScrollBar_ValueChanged(object sender, EventArgs e)
        {
            // update TopLeftIndex value
            topLeftIndex.Y = vScrollBar.Value - (int)(grid.GetLength(1) * 1.5);
            this.Invalidate();
        }
        //resets maximum grid cell size to default
        private void resetCellSize()
        {
            //Get new max grid size
            Size maxGridCellSize = new Size((int)(((this.ClientSize.Width * 0.97) - InitialOffset.X) / grid.GetLength(0)), (int)(((this.ClientSize.Height * 0.99) - InitialOffset.Y) / grid.GetLength(1)));
            if (maxGridCellSize.Width == 0 || maxGridCellSize.Height == 0) { rectSize = new Size(1, 1); }
            else if (maxGridCellSize.Width <= maxGridCellSize.Height) { rectSize = new Size(maxGridCellSize.Width, maxGridCellSize.Width); }
            else { rectSize = new Size(maxGridCellSize.Height, maxGridCellSize.Height); }
        }

        private void HelpBtn_Click(object sender, EventArgs e)
        {
            using (InfoForm infoForm = new InfoForm(version))
            {
                infoForm.ShowDialog();
            }
        }

        private void statusBarUpdateGridSize()
        {
            gridSizePanel.Text = String.Format("Grid size: {0} x {1}", grid.GetLength(0).ToString(), grid.GetLength(1).ToString());
            //distancePanel.Text = "Distance : " +    
        }
    }


    //Allow console to be started
    //oh fuck, do i need all this? (yes)
    //[DllImport("kernel32.dll", SetLastError = true)]
    //[return: MarshalAs(UnmanagedType.Bool)]
    //static extern bool AllocConsole();


}





//<0 - Path length
//1 - Wall
//2 - Start / end point
//3 - Drawn Path