using System;
using System.Collections.Generic;
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
            FileInfo[] files = directoryInfo.GetFiles("*.pattern");
            foreach (FileInfo file in files)
            {
                fileList.Add(new FileListStruct {
                    name = file.Name,
                    fullPath = file.FullName
                }); 
            }
            return fileList;

        }
        public int[,] getPattern(FileListStruct file)
        {
            using (StreamReader sr = new StreamReader(file.fullPath))
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
                    textWrite.Write( "\n");
                }
                //textWrite.Write(fullFilePath, data);
            }
        }
    }
}
