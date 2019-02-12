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
    public partial class InputForm : Form
    {
        Label titleLbl = new Label();
        TextBox txtBox = new TextBox();
        Button Confirm = new Button();
        Button Cancel = new Button();

        public string ReturnValue { get; set; } = "undefined";

        public InputForm(string titleText)
        {
            InitializeComponent();

            //disallow resizing
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ClientSize = new Size(300, 200);
            AcceptButton = Confirm;
            Font font = new Font(this.Font.FontFamily, 16);

            //titleLbl Label
            titleLbl.Text = titleText;
            titleLbl.Font = new Font("Arial", 16);
            Controls.Add(titleLbl);

            //txtBox TextBox
            txtBox.Font = new Font("Arial", 16);
            Controls.Add(txtBox);

            //Confirm Button
            Confirm.FlatStyle = FlatStyle.Flat;
            Confirm.Text = "Ok";
            Confirm.Click += Confirm_Click;
            Controls.Add(Confirm);

            //Cancel Button
            Cancel.FlatStyle = FlatStyle.Flat;
            Cancel.Text = "Cancel";
            Cancel.Click += Cancel_Click;
            Controls.Add(Cancel);


            //SIZES / LOCATIONS
            int buttonSize = (int)(ClientSize.Width * 0.44);

            //titleLbl
            titleLbl.Location = new Point((int)(ClientSize.Width * 0.04), (int)(ClientSize.Height * 0.04));
            titleLbl.Size = TextRenderer.MeasureText(titleLbl.Text, font);

            //txtBox
            txtBox.Location = new Point((int)(ClientSize.Width * 0.04), titleLbl.Bottom + (int)(ClientSize.Height * 0.04));
            txtBox.Size = new Size((int)(this.ClientSize.Width * .92), txtBox.Height);

            //Confirm Button
            Confirm.Location = new Point((int)(ClientSize.Width * 0.04), txtBox.Bottom + (int)(ClientSize.Height * 0.04));
            Confirm.Size = new Size(buttonSize, titleLbl.Height);

            //Cancel Button
            Cancel.Location = new Point(Confirm.Right + (int)(ClientSize.Width * 0.04), txtBox.Bottom + (int)(ClientSize.Height * 0.04));
            Cancel.Size = new Size(buttonSize, titleLbl.Height);

            this.ClientSize = new Size(ClientSize.Width, Cancel.Bottom + (int)(ClientSize.Width * 0.04));

        }

        private void Confirm_Click(object sender, EventArgs e)
        {
            if (!String.IsNullOrWhiteSpace(txtBox.Text))
            {
                ReturnValue = txtBox.Text;
                DialogResult = DialogResult.OK;
                Close();
            }
            else { MessageBox.Show(this, "You must enter a file name", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private void Cancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
