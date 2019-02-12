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

    public partial class GridPropertiesWindow : Form
    {
        Button okBtn = new Button();
        Button closeBtn = new Button();


        Label gridSize = new Label();

        public int[] ReturnValues { get; set; } = new int[2];

        TextBox gridX = new TextBox();
        TextBox gridY = new TextBox();
        public GridPropertiesWindow(Size currentSize)
        {
            InitializeComponent();
            //disallow resizing
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ClientSize = new Size(300, 200);
            AcceptButton = okBtn;
            Font font = new Font(this.Font.FontFamily, 16);

            //gridSize Label
            gridSize.Text = "Enter grid dimensions:";
            gridSize.Font = font;
            Controls.Add(gridSize);

            //gridX TextBox
            gridX.Text = currentSize.Width.ToString();
            gridX.Font = font;
            Controls.Add(gridX);

            //gridY TextBox
            gridY.Text = currentSize.Height.ToString();
            gridY.Font = font;
            Controls.Add(gridY);


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


            //gridSize
            gridSize.Location = new Point((int)(ClientSize.Width * 0.06), (int)(ClientSize.Height * 0.06));
            gridSize.Size = TextRenderer.MeasureText(gridSize.Text, font);



            //gridX
            gridX.Location = new Point((int)(ClientSize.Width * 0.06), gridSize.Bottom + (int)(ClientSize.Height * 0.06));
            gridX.Size = new Size((int)(ClientSize.Width * 0.41), (int)(ClientSize.Height * .15));

            //gridY
            gridY.Location = new Point(gridX.Right + (int)(ClientSize.Width * 0.06), gridX.Top);
            gridY.Size = new Size((int)(ClientSize.Width * 0.41), TextRenderer.MeasureText("0", font).Height);

            //okBtn
            okBtn.Location = new Point((int)(ClientSize.Width * 0.06), (int)(ClientSize.Height * .8));
            okBtn.Size = new Size((int)(ClientSize.Width * 0.41), TextRenderer.MeasureText("0", font).Height);

            //closeBtn
            closeBtn.Location = new Point(okBtn.Right + (int)(ClientSize.Width * 0.06), okBtn.Top);
            closeBtn.Size = new Size((int)(ClientSize.Width * 0.41), okBtn.Size.Height);

        }

        private void CloseBtn_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }

        private void OkBtn_Click(object sender, EventArgs e)
        {
            int.TryParse(gridX.Text, out ReturnValues[0]);
            int.TryParse(gridY.Text, out ReturnValues[1]);
            for (int i = 0; i < ReturnValues.Length; i++)
            {
                if (ReturnValues[i] < 1) { ReturnValues[i] = 1; }
            }
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
