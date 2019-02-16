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
    public partial class InfoForm : Form
    {
        RichTextBox text = new RichTextBox();
        Button okBtn = new Button();
        public InfoForm(string version)
        {
            this.ClientSize = new Size(350, 400);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Text = "Settings";
            this.AcceptButton = okBtn;
            InitializeComponent();
            //text RichTextBox
            text.Location = new Point(10, 10);
            text.Size = new Size(ClientSize.Width - 20, ClientSize.Height - 60);
            text.ReadOnly = true;
            text.Font = new Font("Arial", this.Font.SizeInPoints + 3 );
            text.Rtf = @"{\rtf1\pc \b1Available Shortcuts:\b0\par \par \b Mouse wheel:\b0  Move up / down\par \par \b Shift + Mouse wheel:\b0  Move left / right\par \par \b Control + Mouse wheel:\b0  Zoom in / out\par \par Holding x increases speed\par \par Accountable Menace, Version " + version + "";
            //text.Rtf = @"{\rtf1\pc ytes}";
            Controls.Add(text);
            //okBtn Button
            okBtn.Location = new Point(text.Left, text.Bottom + 10);
            okBtn.Size = new Size(text.Width, 30);
            okBtn.Text = "Return";
            okBtn.Click += OkBtn_Click;
            Controls.Add(okBtn);
        }

        private void OkBtn_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
