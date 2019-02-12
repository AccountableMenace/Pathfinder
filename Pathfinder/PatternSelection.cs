using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Text.RegularExpressions;

namespace Pathfinder
{
    public partial class PatternSelection : Form
    {
        Button okBtn = new Button();
        Button closeBtn = new Button();
        Button saveNew = new Button();
        Button saveReplace = new Button();

        Label Lbl = new Label();
        ListBox PatternList = new ListBox();

        ContextMenuStrip contextMenuStrip = new ContextMenuStrip();


        string pathToFile;
        int[,] gridData;
        public int[,] ReturnPattern { get; set; }
        List<FileListStruct> fileList;
        FileReaderWriter fileReaderWriter = new FileReaderWriter();

        public PatternSelection(int[,] mainGridData)
        {
            InitializeComponent();
            pathToFile = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(Application.UserAppDataPath), @"..\"));
            gridData = mainGridData;
            //disallow resizing
            //this.FormBorderStyle = FormBorderStyle.FixedDialog;
            //this.MaximizeBox = false;
            //this.MinimizeBox = false;
            this.ClientSize = new Size(500, 300);
            AcceptButton = okBtn;
            Font font = new Font(this.Font.FontFamily, 16);

            //gridSize Label
            Lbl.Text = "Choose Pattern:";
            Lbl.Font = font;
            Controls.Add(Lbl);

            //PatternList
            PatternList.MouseDown += PatternList_MouseDown;
            Controls.Add(PatternList);

            //okBtn Button
            okBtn.FlatStyle = FlatStyle.Flat;
            okBtn.Text = "Load";
            okBtn.Click += OkBtn_Click;
            Controls.Add(okBtn);

            //closeBtn Button
            closeBtn.FlatStyle = FlatStyle.Flat;
            closeBtn.Text = "Cancel";
            closeBtn.Click += CloseBtn_Click;
            Controls.Add(closeBtn);

            //saveNew Button
            saveNew.FlatStyle = FlatStyle.Flat;
            saveNew.Text = "Save As";
            saveNew.Click += SaveNew_Click;
            Controls.Add(saveNew);

            //saveReplace Button
            saveReplace.FlatStyle = FlatStyle.Flat;
            saveReplace.Text = "Replace Save";
            saveReplace.Click += SaveReplace_Click;
            Controls.Add(saveReplace);

            //menuStrip 
            ToolStripMenuItem menuItem = new ToolStripMenuItem { Text = "Delete" };
            menuItem.Click += MenuItem_Click;
            contextMenuStrip.Items.Add(menuItem);

            //Locations


            //gridSize
            Lbl.Location = new Point((int)(ClientSize.Width * 0.05), (int)(ClientSize.Height * 0.06));
            Lbl.Size = TextRenderer.MeasureText(Lbl.Text, font);

            int buttonSize = (int)(ClientSize.Width * .21);
            int buttonGap = (int)(ClientSize.Width * .02);
            //saveNew
            saveNew.Location = new Point((int)(ClientSize.Width * 0.05), (int)(ClientSize.Height * .8));
            saveNew.Size = new Size(buttonSize, okBtn.Size.Height);

            //saveReplace
            saveReplace.Location = new Point(saveNew.Right + buttonGap, saveNew.Top);
            saveReplace.Size = new Size(buttonSize, okBtn.Size.Height);

            //okBtn (Load)
            okBtn.Location = new Point(saveReplace.Right + buttonGap, saveNew.Top);
            okBtn.Size = new Size(buttonSize, TextRenderer.MeasureText("0", font).Height);

            //closeBtn
            closeBtn.Location = new Point(okBtn.Right + buttonGap, saveNew.Top);
            closeBtn.Size = new Size(buttonSize, okBtn.Size.Height);

            //PatternList
            PatternList.Location = new Point((int)(ClientSize.Width * 0.05), Lbl.Bottom + (int)(ClientSize.Height * 0.06));
            PatternList.Size = new Size((int)(ClientSize.Width * .9), (int)((ClientSize.Height * .94) - PatternList.Location.Y - (ClientSize.Height - okBtn.Top)));

            //Fill list with items from file path 
            fileList = fileReaderWriter.getListOfFiles(pathToFile);
            PatternList.DataSource = fileList;

        }

        private void SaveReplace_Click(object sender, EventArgs e)
        {
            DialogResult msgRes = MessageBox.Show(this, "Are you sure you want to replace " + PatternList.SelectedItem + " ?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Asterisk);
            if (msgRes == DialogResult.Yes)
            {
                fileReaderWriter.saveFile(String.Format("{0} {1}\n", gridData.GetLength(0), gridData.GetLength(1)), gridData, (pathToFile + PatternList.SelectedItem));
            }
        }

        private void SaveNew_Click(object sender, EventArgs e)
        {
            string fileName;
            using (InputForm inputForm = new InputForm("Enter file name:"))
            {
                if (inputForm.ShowDialog() == DialogResult.OK)
                {
                    Regex regex = new Regex(@"^[\w\-. ]+$");
                    fileName = inputForm.ReturnValue;
                    Match match = regex.Match(fileName);
                    if (match.Success)
                    {
                        fileReaderWriter.saveFile(String.Format("{0} {1}\n", gridData.GetLength(0), gridData.GetLength(1)), gridData, (pathToFile + fileName + ".pattern"));
                    }
                    else
                    {
                        MessageBox.Show(this, "Invalid file name!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }

                }
            }
            //update pattern list
            fileList = fileReaderWriter.getListOfFiles(pathToFile);
            PatternList.DataSource = null;
            PatternList.DataSource = fileList;
        }

        private void CloseBtn_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }

        private void OkBtn_Click(object sender, EventArgs e)
        {
            if (PatternList.SelectedItem != null)
            {
                ReturnPattern = fileReaderWriter.getPattern(fileList[PatternList.SelectedIndex]);
                DialogResult = DialogResult.OK;
                Close();
            }
            else
            {
                MessageBox.Show(this, "No file selected!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void PatternList_MouseDown(object sender, MouseEventArgs e)
        {
            //MessageBox.Show("Test");
            if (e.Button != MouseButtons.Right) return;
            PatternList.SelectedIndex = PatternList.IndexFromPoint(e.Location);
            contextMenuStrip.Show(Cursor.Position);
            contextMenuStrip.Visible = true;

        }

        private void MenuItem_Click(object sender, EventArgs e)
        {
            DialogResult msgRes = MessageBox.Show(this, "Are you sure you want to delete " + fileList[PatternList.SelectedIndex] + " ?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Asterisk, MessageBoxDefaultButton.Button2);
            if (msgRes == DialogResult.Yes)
            {
                File.Delete(fileList[PatternList.SelectedIndex].fullPath);
            }
            fileList = fileReaderWriter.getListOfFiles(pathToFile);
            PatternList.DataSource = null;
            PatternList.DataSource = fileList;
        }



    }
}
