using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace CellularAutomaton
{
    public class SaveLoadHelper
    {
        private Grid _grid;
        private Drawing _drawing;

        public SaveLoadHelper(Grid grid, Drawing drawing)
        {
            _grid = grid;
            _drawing = drawing;
        }

        public void Save(string path)
        {
            var extension = Path.GetExtension(path);

            switch (extension.ToLower())
            {
                case ".jpg":
                    SaveToJpg(path);
                    break;
                case ".bmp":
                    SaveToBmp(path);
                    break;
                case ".txt":
                    SaveToText(path);
                    break;
            }
        }

        public void Load(string path)
        {
            var extension = Path.GetExtension(path);

            switch (extension.ToLower())
            {
                case ".jpg":
                case ".bmp":
                    ReadFromInage(path);
                    break;
                case ".txt":
                    ReadFromText(path);
                    break;
            }
        }

        public void SaveToJpg(string path)
        {
            JpegBitmapEncoder jpegBitmapEncoder = new JpegBitmapEncoder
            {
                QualityLevel = 100,

            };
            jpegBitmapEncoder.Frames.Add(BitmapFrame.Create(PrepareRender()));
            using (FileStream fileStream = new FileStream(path, FileMode.Create))
            {
                jpegBitmapEncoder.Save(fileStream);
                fileStream.Flush();
                fileStream.Close();
            }
        }

        public void SaveToBmp(string path)
        {
            BmpBitmapEncoder bitmapEncoder = new BmpBitmapEncoder();
            bitmapEncoder.Frames.Add(BitmapFrame.Create(PrepareRender()));
            using (FileStream fileStream = new FileStream(path, FileMode.Create))
            {
                bitmapEncoder.Save(fileStream);
                fileStream.Flush();
                fileStream.Close();
            }
        }

        public void ReadFromInage(string path)
        {
            ImageSource imageSource = new BitmapImage(new Uri(path));
            _drawing.GetImage().Source = imageSource;
        }

        public void SaveToText(string path)
        {
            using (StreamWriter writetext = new StreamWriter(path))
            {
                writetext.WriteLine(_grid.XSize + " " + _grid.YSize);
                for (var x = 0; x < _grid.XSize; x++)
                {
                    for (var y = 0; y < _grid.YSize; y++)
                    {
                        writetext.WriteLine(x + " " + y + " " + _grid.GridContainer[x][y].State);
                    }
                }
            }
        }

        public void ReadFromText(string path)
        {
            int counter = 0;
            string line;

            StreamReader file = new StreamReader(path);
            while ((_ = file.ReadLine()) != null)
            {
                counter++;
            }
            string[] linesArray = new string[counter];

            StreamReader readFile = new StreamReader(path);

            counter = 0;
            while ((line = readFile.ReadLine()) != null)
            {
                linesArray[counter] = line;
                counter++;
            }

            string[] tempLine = Regex.Split(linesArray[0], " ");
            int x = int.Parse(tempLine[0]);
            int y = int.Parse(tempLine[1]);

            _grid.InitGrid(x, y);

            Dictionary<int, System.Drawing.Color> colors = new Dictionary<int, System.Drawing.Color>();

            int fillCounter = 0;
            for (int i = 0; i < x; i++)
            {

                for (int j = 0; j < y; j++)
                {
                    tempLine = Regex.Split(linesArray[fillCounter + 1], " ");

                    var state = int.Parse(tempLine[2]);

                    if (colors.ContainsKey(state))
                    {
                        _grid.GridContainer[i][j].SetState(state);
                        _grid.GridContainer[i][j].Color = colors[state];
                    }
                    else
                    {
                        _grid.GridContainer[i][j].State = state;
                        ++_grid.ActiveCellsCount;
                        colors.Add(state, _grid.GridContainer[i][j].Color);
                    }
                    _grid.GridContainer[i][j].StateChanged = false;
                    fillCounter++;
                }
            }
            _drawing.DrawGrid();
        }

        private RenderTargetBitmap PrepareRender()
        {
            var xSize = _drawing.GetCellSize() * _grid.XSize;
            var ySize = _drawing.GetCellSize() * _grid.YSize;
            var image = _drawing.GetImage();
            RenderTargetBitmap renderTargetBitmap = new RenderTargetBitmap((int)xSize,
                                                               (int)ySize,
                                                               96, 96, PixelFormats.Default);
            renderTargetBitmap.Render(image);
            return renderTargetBitmap;
        }
    }
}
