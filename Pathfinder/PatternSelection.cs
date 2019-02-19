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
        Button loadBtn = new Button();
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
        ImageReader imgRead = new ImageReader();

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
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Text = "Patterns";

            AcceptButton = loadBtn;
            Font font = new Font(this.Font.FontFamily, 16);
            Font fontSmall = new Font(this.Font.FontFamily, 10);

            //gridSize Label
            Lbl.Text = "Choose Pattern:";
            Lbl.Font = font;
            Controls.Add(Lbl);

            //PatternList
            PatternList.MouseDown += PatternList_MouseDown;
            PatternList.Font = fontSmall;
            Controls.Add(PatternList);

            //okBtn Button
            loadBtn.FlatStyle = FlatStyle.Flat;
            loadBtn.Text = "Load";
            loadBtn.Click += loadBtn_Click;
            Controls.Add(loadBtn);

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
            //load right click context menu
            ToolStripMenuItem menuItem = new ToolStripMenuItem { Text = "Load" };
            menuItem.Click += MenuItemLoad_Click;
            contextMenuStrip.Items.Add(menuItem);
            //delete right click menu context
            menuItem = new ToolStripMenuItem { Text = "Save" };
            menuItem.Click += MenuItemSave_Click;
            contextMenuStrip.Items.Add(menuItem);
            //save right click menu context
            menuItem = new ToolStripMenuItem { Text = "Delete" };
            menuItem.Click += MenuItemDelete_Click;
            contextMenuStrip.Items.Add(menuItem);




            //Locations


            //gridSize
            Lbl.Location = new Point((int)(ClientSize.Width * 0.05), (int)(ClientSize.Height * 0.06));
            Lbl.Size = TextRenderer.MeasureText(Lbl.Text, font);

            int buttonSize = (int)(ClientSize.Width * .21);
            int buttonGap = (int)(ClientSize.Width * .02);
            //saveNew
            saveNew.Location = new Point((int)(ClientSize.Width * 0.05), (int)(ClientSize.Height * .8));
            saveNew.Size = new Size(buttonSize, loadBtn.Size.Height);

            //saveReplace
            saveReplace.Location = new Point(saveNew.Right + buttonGap, saveNew.Top);
            saveReplace.Size = new Size(buttonSize, loadBtn.Size.Height);

            //okBtn (Load)
            loadBtn.Location = new Point(saveReplace.Right + buttonGap, saveNew.Top);
            loadBtn.Size = new Size(buttonSize, TextRenderer.MeasureText("0", font).Height);

            //closeBtn
            closeBtn.Location = new Point(loadBtn.Right + buttonGap, saveNew.Top);
            closeBtn.Size = new Size(buttonSize, loadBtn.Size.Height);

            //PatternList
            PatternList.Location = new Point((int)(ClientSize.Width * 0.05), Lbl.Bottom + (int)(ClientSize.Height * 0.06));
            PatternList.Size = new Size((int)(ClientSize.Width * .9), (int)((ClientSize.Height * .94) - PatternList.Location.Y - (ClientSize.Height - loadBtn.Top)));

            //Fill list with items from file path 
            fileList = fileReaderWriter.getListOfFiles(pathToFile);
            PatternList.DataSource = fileList;

        }
        private void SaveReplace_Click(object sender, EventArgs e)
        {
            if (fileList[PatternList.SelectedIndex].extension == ".pattern")
            {
                DialogResult msgRes = MessageBox.Show(this, "Are you sure you want to replace " + PatternList.SelectedItem + " ?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Asterisk);
                if (msgRes == DialogResult.Yes)
                {
                    fileReaderWriter.saveFile(String.Format("{0} {1}\n", gridData.GetLength(0), gridData.GetLength(1)), gridData, (pathToFile + PatternList.SelectedItem));
                }
            }
            else if (fileList[PatternList.SelectedIndex].extension == ".bmp")
            {
                DialogResult msgRes = MessageBox.Show(this, "Are you sure you want to replace " + PatternList.SelectedItem + " ?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Asterisk);
                if (msgRes == DialogResult.Yes)
                {
                        fileReaderWriter.saveImage(gridData, (pathToFile + PatternList.SelectedItem));
                }
            }
        }

        private void SaveNew_Click(object sender, EventArgs e)
        {
            string fileName, extension;
            using (InputForm inputForm = new InputForm("Enter file name:"))
            {
                if (inputForm.ShowDialog() == DialogResult.OK)
                {
                    Regex regex = new Regex(@"^[\w\-. ]+$");
                    fileName = inputForm.ReturnValue;
                    extension = inputForm.ReturnType;
                    Match match = regex.Match(fileName);
                    if (match.Success && extension == ".pattern")
                    {
                        fileReaderWriter.saveFile(String.Format("{0} {1}\n", gridData.GetLength(0), gridData.GetLength(1)), gridData, (pathToFile + fileName + ".pattern"));
                    }
                    else if (match.Success && extension == ".bmp (Bitmap)")
                    {
                        fileReaderWriter.saveImage(gridData, (pathToFile + fileName + ".bmp"));
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

        private void loadBtn_Click(object sender, EventArgs e)
        {
            if (PatternList.SelectedItem != null)
            {
                if (fileList[PatternList.SelectedIndex].extension == ".pattern")
                    ReturnPattern = fileReaderWriter.getPattern(fileList[PatternList.SelectedIndex]);
                else if (fileList[PatternList.SelectedIndex].extension == ".bmp")
                    ReturnPattern = imgRead.GetImagePixels(fileList[PatternList.SelectedIndex]);
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

        private void MenuItemDelete_Click(object sender, EventArgs e)
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
        private void MenuItemLoad_Click(object sender, EventArgs e)
        {
            loadBtn_Click(this, e);
        }
        private void MenuItemSave_Click(object sender, EventArgs e)
        {
            SaveReplace_Click(this, e);
        }



    }
}
