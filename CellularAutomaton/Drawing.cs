﻿using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Media.Imaging;

namespace CellularAutomaton
{
    public class Drawing
    {
        private Bitmap _bitmap;
        private Graphics _graphics;
        private Grid _grid;
        private System.Windows.Controls.Image _drawingImage;
        private int _drawPanelWidth;
        private int _drawPanelHeight;
        private double _cellSize;
        private double _cellPadding = 1.0;
        public bool ShowBorders = false;

        public Drawing(System.Windows.Controls.Image image, Grid grid)
        {
            _drawingImage = image;
            _grid = grid;
        }

        public void DrawGrid()
        {
            _bitmap = new Bitmap(_drawPanelWidth, _drawPanelHeight);
            _graphics = Graphics.FromImage(_bitmap);
            for (int i = 0; i < _grid.GridContainer.Length; i++)
            {
                for (int j = 0; j < _grid.GridContainer[i].Length; j++)
                {
                    var cell = _grid.GridContainer[i][j];
                    int left = (int)(cell.X * _cellSize);
                    int top = (int)(cell.Y * _cellSize);

                    int right = (int)(((double)(cell.X + 1.0) * _cellSize) - _cellPadding);
                    int bottom  = (int)(((double)(cell.Y + 1.0) * _cellSize) - _cellPadding);

                    if (left == right)
                    {
                        if (left > 1) left--;
                        else right++;
                    }
                    if (top == bottom)
                    {
                        if (top > 1) top--;
                        else bottom++;
                    }
                    Rectangle rectangle = Rectangle.FromLTRB(left,top,right,bottom);

                    if (ShowBorders && cell.IsBorder)
                    {
                        DrawRectangle(rectangle);
                    }
                    else
                    {
                        DrawRectangle(rectangle, cell.Color);
                    }
                        
                }
            }
            _drawingImage.Source = BitmapToBitmapImage(_bitmap);
        }

        public void SwitchGrid(bool show)
        {
            _cellPadding = show ? 1.0 : 0.0;
            DrawGrid();
        }

        public void InvaildateGrid()
        {
            _grid.NeedsRefresh = true;
        }

        public void SetGridParameters(int xCount, int yCount)
        {
            _drawPanelWidth = (int)_drawingImage.Width;
            _drawPanelHeight = (int)_drawingImage.Height;

            var cellWidth = _drawPanelWidth / (double)(xCount);
            var cellHeight = _drawPanelHeight / (double)(yCount);

            _cellSize = Math.Min(cellHeight, cellWidth);

            if (_grid.NeedsRefresh)
            {
                _grid.InitGrid(xCount, yCount);
            }
        }

        public void UpdateCell(Cell cell, bool exclude)
        {
            var currentCell = _grid.GridContainer[cell.X][cell.Y];
            currentCell.DualPhaseProtected = false;
            if (cell.State == -1 && exclude)
            {
                currentCell.State = 0;
            }
            else if (cell.State == 0 && exclude)
            {
                currentCell.State = -1;
            }
            else if (cell.State == -1 || cell.State == 0)
            {
                currentCell.State = ++_grid.ActiveCellsCount;
            }
            else if (cell.State > 0 && exclude)
            {
                _grid.ActiveCellsCount--;
                currentCell.State = -1;
            }
            else if (cell.State > 0)
            {
                _grid.ActiveCellsCount--;
                currentCell.State = 0;
            }
        }

        public Cell GetCellByPosition(int x, int y)
        {
            var xValue = (int)(x / _cellSize);
            var yValue = (int)(y / _cellSize);

            if (_grid.GridContainer.Length <= xValue || _grid.GridContainer[xValue].Length <= yValue)
            {
                return null;
            }

            return _grid.GridContainer[xValue][yValue];
        }

        public System.Windows.Controls.Image GetImage()
        {
            return _drawingImage;
        }

        public double GetCellSize()
        {
            return _cellSize;
        }

        private void DrawRectangle(Rectangle rectangle, Color color)
        {
            Brush brush;
            brush = new SolidBrush(color);
            _graphics.FillRectangle(
                brush,
                rectangle
            );
        }

        private void DrawRectangle(Rectangle rectangle)
        {
            LinearGradientBrush brush = new LinearGradientBrush(rectangle, Color.Black, Color.Gray, 0, false);
            _graphics.FillRectangle(
                brush,
                rectangle
            );
        }

        private BitmapImage BitmapToBitmapImage(Bitmap src)
        {
            using (var memory = new MemoryStream())
            {
                src.Save(memory, ImageFormat.Png);
                memory.Position = 0;

                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.Freeze();

                return bitmapImage;
            }
        }
    }
}
