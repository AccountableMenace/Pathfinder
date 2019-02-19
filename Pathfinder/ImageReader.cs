using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.IO;

namespace Pathfinder
{
    class ImageReader
    {
        public int[,] GetImagePixels(FileListStruct file)
        {
            Bitmap img = new Bitmap(file.fullPath);
            Color pixel;
            int[,] gridData = new int[img.Width,img.Height];
            for (int i = 0; i < img.Width; i++)
            {
                for (int j = 0; j < img.Height; j++)
                {
                    pixel = img.GetPixel(i, j);
                    if (pixel == Color.FromArgb(0,0,0))
                    {
                        gridData[i, j] = 1;
                    }
                    else if (pixel == Color.FromArgb(0, 255, 0))
                    {
                        gridData[i, j] = 2;
                    }
                    else if (pixel == Color.FromArgb(255, 255, 0))
                    {
                        gridData[i, j] = -1;
                    }
                    else if (pixel == Color.FromArgb(0, 255, 255))
                    {
                        gridData[i, j] = 3;
                    }

                }
            }
            return gridData;
        }
    }
}
