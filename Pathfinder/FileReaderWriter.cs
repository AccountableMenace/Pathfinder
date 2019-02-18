using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pathfinder
{
    //Get list of pattern files from document directory
    class FileReaderWriter
    {
        public List<FileListStruct> getListOfFiles(string path)
        {
            List<FileListStruct> fileList = new List<FileListStruct>();

            DirectoryInfo directoryInfo = new DirectoryInfo(path);
            //get all files into list
            FileInfo[] filesArr = directoryInfo.GetFiles();
            List<FileInfo> files = new List<FileInfo>();
            foreach (FileInfo file in filesArr)
            {
                if (file.Extension == ".pattern" || file.Extension == ".bmp")
                {
                    files.Add(file);
                }
            }
            foreach (FileInfo file in files)
            {
                fileList.Add(new FileListStruct
                {
                    name = file.Name,
                    fullPath = file.FullName,
                    extension = file.Extension
                });
            }
            return fileList;

        }
        public int[,] getPattern(FileListStruct file)
        {
            using (StreamReader sr = new StreamReader(file.fullPath))
            {
                try
                {
                    string[] outputRaw = sr.ReadToEnd().Split(new[] { '\n', '\r', ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                    int[,] outFormatted = new int[Convert.ToInt32(outputRaw[0]), Convert.ToInt32(outputRaw[1])];
                    int k = 2;
                    for (int i = 0; i < int.Parse(outputRaw[1]); i++)
                    {
                        for (int j = 0; j < Convert.ToInt32(outputRaw[0]); j++)
                        {
                            if (k < outputRaw.Length && outputRaw[k] != null)
                                int.TryParse(outputRaw[k], out outFormatted[j, i]);

                            k++;
                        }
                    }
                    return outFormatted;
                }
                catch (Exception er)
                {
                    System.Windows.Forms.MessageBox.Show("Error, invalid file. The file is most likely corrupt");
                }
                return new int[1, 1];
            }
        }

        public void saveFile(string data, int[,] gridData, string fullFilePath)
        {
            using (TextWriter textWrite = new StreamWriter(fullFilePath))
            {
                textWrite.Write(data);
                for (int j = 0; j < gridData.GetLength(1); j++)
                {
                    for (int i = 0; i < gridData.GetLength(0); i++)
                    {
                        //data.Add(gridData[i, j] + " ");
                        textWrite.Write(gridData[i, j] + " ");
                    }
                    textWrite.Write("\n");
                }
                //textWrite.Write(fullFilePath, data);
            }
        }
        public void saveImage(int[,] gridData, string fullFilePath)
        {
            Bitmap img = new Bitmap(gridData.GetLength(0), gridData.GetLength(1));
            //Graphics drawing = Graphics.FromImage(img);
            for (int i = 0; i < gridData.GetLength(1); i++)
            {
                for (int j = 0; j < gridData.GetLength(0); j++)
                {
                    if (gridData[j, i] == 1)
                    {
                        img.SetPixel(j, i, Color.Black);
                    }
                    else if (gridData[j, i] == 2)
                    {
                        img.SetPixel(j, i, Color.FromArgb(0, 255, 0));
                    }
                }
            }
            img.Save(fullFilePath);
        }
    }
}
