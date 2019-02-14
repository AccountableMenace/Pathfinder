using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Pathfinder
{
    public partial class MazeGenerationConfig : Form
    {
        public int algorithm { get; set; }
        public Point StartPoint { get; set; }
        public Point EndPoint { get; set; }

        Font font;
        Label algorithmLbl = new Label();
        ComboBox algorithmDropDown = new ComboBox();
        Label startEndPointLbl = new Label();
        TextBox StartX = new TextBox();
        TextBox StartY = new TextBox();
        TextBox EndX = new TextBox();
        TextBox EndY = new TextBox();
        Button okBtn = new Button();
        Button cancelBtn = new Button();

        public MazeGenerationConfig(Size gridSize)
        {
            InitializeComponent();
            font = new Font(this.Font.FontFamily, (int)(this.Font.Height * 1.25));
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ClientSize = new Size(250, 300);
            this.Text = "Maze Generation Settings";
            AcceptButton = okBtn;
            InitializeControls(gridSize);
            UpdateLayout();
        }

        private void UpdateLayout()
        {
            // Margins 
            int topMarginOrGap = 10;
            int HPadding = 10;

            //gridSizeLbl Label
            algorithmLbl.Size = TextRenderer.MeasureText(algorithmLbl.Text, font);
            algorithmLbl.Location = new Point(HPadding, topMarginOrGap);

            //algorithmDropDown ToolStripDropDown
            algorithmDropDown.Size = new Size((int)(ClientSize.Width - HPadding * 2), (int)(this.Font.GetHeight() * 1.5));
            algorithmDropDown.Location = new Point(HPadding, algorithmLbl.Bottom + topMarginOrGap);

            //startEndPointLbl Label
            startEndPointLbl.Size = new Size(ClientSize.Width -HPadding, TextRenderer.MeasureText(startEndPointLbl.Text, font).Height * 2);
            startEndPointLbl.Location = new Point(HPadding, algorithmDropDown.Bottom + topMarginOrGap);

            //StartX TextBox
            StartX.Size = new Size((int)(ClientSize.Width / 2 - HPadding * 1.5), (int)(this.Font.GetHeight() * 1.5));
            StartX.Location = new Point(HPadding, startEndPointLbl.Bottom + topMarginOrGap);

            //StartY TextBox
            StartY.Size = new Size((int)(ClientSize.Width / 2 - HPadding * 1.5), (int)(this.Font.GetHeight() * 1.5));
            StartY.Location = new Point(HPadding + StartX.Right, StartX.Top);

            //EndX TextBox
            EndX.Size = new Size((int)(ClientSize.Width / 2 - HPadding * 1.5), (int)(this.Font.GetHeight() * 1.5));
            EndX.Location = new Point(HPadding, StartX.Bottom + topMarginOrGap);

            //EndY TextBox
            EndY.Size = new Size((int)(ClientSize.Width / 2 - HPadding * 1.5), (int)(this.Font.GetHeight() * 1.5));
            EndY.Location = new Point(HPadding + EndX.Right, EndX.Top);

            //okBtn Button
            okBtn.Size = new Size((int)(ClientSize.Width / 2 - HPadding * 1.5), (int)(this.Font.GetHeight() * 2));
            okBtn.Location = new Point(HPadding, EndY.Bottom + topMarginOrGap);

            //cancelBtn Button
            cancelBtn.Size = new Size((int)(ClientSize.Width / 2 - HPadding * 1.5), (int)(this.Font.GetHeight() * 2));
            cancelBtn.Location = new Point(HPadding + okBtn.Right, okBtn.Top);
        }
        private void InitializeControls(Size gridSize)
        {
            //algorithmLbl label
            algorithmLbl.Text = "Select Algorithm";
            algorithmLbl.Font = font;
            Controls.Add(algorithmLbl);

            //algorithmDropDown ToolStripDropDown
            ToolStripItem item = new ToolStripMenuItem();
            item.Text = "Recursive backtracking";
            item.Name = "GridViewRowID";
            algorithmDropDown.Items.Add(item);
            algorithmDropDown.SelectedIndex = 0;
            Controls.Add(algorithmDropDown);

            //startEndPointLbl label
            startEndPointLbl.Text = "Enter Start / End Coordinates";
            startEndPointLbl.Font = font;
            Controls.Add(startEndPointLbl);

            //StartX TextBox
            StartX.Text = "0";
            Controls.Add(StartX);

            //StartY TextBox
            StartY.Text = "0";
            Controls.Add(StartY);

            //EndX TextBox
            //get the closest to the bottom right corner pathfindable cell
            EndX.Text = (gridSize.Width - 1 - ((gridSize.Width + 1) % 2)).ToString();
            Controls.Add(EndX);

            //EndY TextBox
            EndY.Text = (gridSize.Height - 1 - ((gridSize.Height + 1) % 2)).ToString();
            Controls.Add(EndY);

            //cancelBtn Button
            cancelBtn.FlatStyle = FlatStyle.Flat;
            cancelBtn.Text = "Cancel";
            cancelBtn.Click += CancelBtn_Click;
            Controls.Add(cancelBtn);

            //okBtn Button
            okBtn.FlatStyle = FlatStyle.Flat;
            okBtn.Text = "Generate";
            okBtn.Click += OkBtn_Click;
            Controls.Add(okBtn);
        }

        private void CancelBtn_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void OkBtn_Click(object sender, EventArgs e)
        {
            StartPoint = new Point(int.Parse(StartX.Text), int.Parse(StartY.Text));
            EndPoint = new Point(int.Parse(EndX.Text), int.Parse(EndY.Text));

            algorithm = algorithmDropDown.SelectedIndex;
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
