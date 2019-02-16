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
    public partial class SettingsWindow : Form
    {
        public int[] returnSize { get; set; } = new int[2];
        public bool[] returnSettings { get; set; } = new bool[2];

        Label gridSizeLbl = new Label();
        TextBox gridXTxt = new TextBox();
        TextBox gridYTxt = new TextBox();

        Button okBtn = new Button();
        Button closeBtn = new Button();

        CheckBox gridBoundaryCheckBox = new CheckBox();

        CheckBox debugCheckBox = new CheckBox();

        //available settings - grid boundary, debug options;
        public SettingsWindow(Size currentSize, bool gridBoundary, bool debugOptions)
        {
            InitializeComponent();
            //disallow resizing
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;   
            this.MinimizeBox = false;
            this.ClientSize = new Size(250, 200);
            this.Text = "Settings";
            this.AcceptButton = okBtn;
            this.CancelButton = closeBtn;
            Font font = new Font(this.Font.FontFamily, 16);
            Font fontSmall = new Font(this.Font.FontFamily, 12);

            //Configuration
            //gridSize Label
            gridSizeLbl.Text = "Enter grid dimensions:";
            gridSizeLbl.Font = font;
            Controls.Add(gridSizeLbl);

            //gridXTxt TextBox
            gridXTxt.Text = currentSize.Width.ToString();
            gridXTxt.Font = font;
            Controls.Add(gridXTxt);

            //gridYTxt TextBox
            gridYTxt.Text = currentSize.Height.ToString();
            gridYTxt.Font = font;
            Controls.Add(gridYTxt);

            //gridBoundaryCheckBox CheckBox
            gridBoundaryCheckBox.Text = "Display Grid Boundary";
            gridBoundaryCheckBox.Checked = gridBoundary;
            gridBoundaryCheckBox.Font = fontSmall;
            Controls.Add(gridBoundaryCheckBox);

            //debugCheckBox CheckBox
            debugCheckBox.Text = "Display debug options";
            debugCheckBox.Checked = debugOptions;
            debugCheckBox.Font = fontSmall;
            Controls.Add(debugCheckBox);


            //okBtn Button
            okBtn.FlatStyle = FlatStyle.Flat;
            okBtn.Text = "Ok";
            okBtn.Click += OkBtn_Click;
            Controls.Add(okBtn);

            //closeBtn Button
            closeBtn.FlatStyle = FlatStyle.Flat;
            closeBtn.Text = "Cancel";
            closeBtn.Click += CloseBtn_Click;
            Controls.Add(closeBtn);



            //Locations

            // Margins 
            int topMarginOrGap = 10;
            int bottomMargin = 10;

            //gridSizeLbl Label
            gridSizeLbl.Size = TextRenderer.MeasureText(gridSizeLbl.Text, font);
            gridSizeLbl.Location = new Point((int)(ClientSize.Width * 0.06), topMarginOrGap);

            //gridXTxt TextBox
            gridXTxt.Size = new Size((int)(ClientSize.Width * 0.41), TextRenderer.MeasureText("0", font).Height);
            gridXTxt.Location = new Point((int)(ClientSize.Width * 0.06), gridSizeLbl.Bottom + topMarginOrGap);

            //gridYTxt TextBox
            gridYTxt.Size = new Size((int)(ClientSize.Width * 0.41), TextRenderer.MeasureText("0", font).Height);
            gridYTxt.Location = new Point(gridXTxt.Right + (int)(ClientSize.Width * 0.06), gridXTxt.Top);


            //gridBoundaryCheckBox CheckBox
            gridBoundaryCheckBox.Size = (new Size((TextRenderer.MeasureText(gridBoundaryCheckBox.Text, fontSmall).Width + gridBoundaryCheckBox.Margin.Horizontal), (fontSmall.Height + gridBoundaryCheckBox.Margin.Vertical)));
            gridBoundaryCheckBox.Location = new Point((int)(ClientSize.Width * 0.06), gridYTxt.Bottom + topMarginOrGap);

            //debugCheckBox CheckBox
            debugCheckBox.Size = (new Size((TextRenderer.MeasureText(debugCheckBox.Text, fontSmall).Width + debugCheckBox.Margin.Horizontal), fontSmall.Height + debugCheckBox.Margin.Vertical));
            debugCheckBox.Location = new Point((int)(ClientSize.Width * 0.06), gridBoundaryCheckBox.Bottom);


            //Buttons Glued to the bottom
            //okBtn Button
            okBtn.Size = new Size((int)(ClientSize.Width * 0.41), TextRenderer.MeasureText("0", font).Height);
            okBtn.Location = new Point((int)(ClientSize.Width * 0.06), ClientSize.Height - okBtn.Height - bottomMargin);

            //closeBtn Button
            closeBtn.Size = new Size((int)(ClientSize.Width * 0.41), okBtn.Size.Height);
            closeBtn.Location = new Point(okBtn.Right + (int)(ClientSize.Width * 0.06), okBtn.Top);
        }

        private void CloseBtn_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }

        private void OkBtn_Click(object sender, EventArgs e)
        {
            //return Size
            int.TryParse(gridXTxt.Text, out returnSize[0]);
            int.TryParse(gridYTxt.Text, out returnSize[1]);
            for (int i = 0; i < returnSize.Length; i++)
            {
                if (returnSize[i] < 1) { returnSize[i] = 1; }
            }

            //return Settings
            returnSettings[0] = gridBoundaryCheckBox.Checked;
            returnSettings[1] = debugCheckBox.Checked;
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
