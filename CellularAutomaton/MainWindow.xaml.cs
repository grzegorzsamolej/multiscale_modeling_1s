using System;
using System.Drawing;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace CellularAutomaton
{
    //TODO
    //1. Reset UI textbox values if value is outside allowed range. It's is checked at code so it will not cause exception, but values in view are not refreshed
    public partial class MainWindow : Window
    {
        private Drawing _drawing;
        private Grid _grid;

        public MainWindow()
        {
            InitializeComponent();
            Loaded += InitGrid;
        }

        private void InitGrid(object sender, EventArgs e)
        {
            _grid = new Grid();
            _drawing = new Drawing(gridPanel, _grid);
            DrawGrid(null, null);

            SizeChanged += DrawGrid;
            xCount.TextChanged += RefreshGrid;
            yCount.TextChanged += RefreshGrid;

            neighbourSeekRule.ItemsSource = Enum.GetValues(typeof(SeekMethod)).Cast<SeekMethod>();
            neighbourSeekRule.SelectedIndex = 0;
        }

        private void RefreshGrid(object sender, EventArgs e)
        {
            _drawing.InvaildateGrid();
            DrawGrid(sender, e);
        }

        private void DrawGrid(object sender, EventArgs e)
        {
            UpdateGridParameters();
            _drawing.DrawGrid();
        }

        private void UpdateGridParameters()
        {
            gridPanel.Width = ImageCanvas.ActualWidth;
            gridPanel.Height = ImageCanvas.ActualHeight;

            int.TryParse(xCount.Text, out var xCellCount);
            int.TryParse(yCount.Text, out var yCellCount);

            xCellCount = xCellCount < 3 ? 3 : xCellCount;
            yCellCount = yCellCount < 3 ? 3 : yCellCount;

            _drawing.SetGridParameters(xCellCount, yCellCount);
        }

        private void GridMouseClick(object sender, MouseButtonEventArgs e)
        {
            var position = e.GetPosition(gridPanel);
            var x = (int)position.X;
            var y = (int)position.Y;

            var cell = _drawing.GetCellByPosition(x, y);

            if (cell != null)
            {
                _drawing.UpdateCell(cell, e.RightButton == MouseButtonState.Pressed);
                _drawing.DrawGrid();

                TestSeek(cell);
            }
        }

        private void TestSeek(Cell cell)
        {
            var seeker = new NeighbourSeeker(_grid);
            var neighbours = seeker.GetNeighbours(cell, openBorders.IsChecked == true, (SeekMethod)neighbourSeekRule.SelectedItem);

            foreach (var n in neighbours)
            {
                for (int x = 0; x < _grid.XSize; x++)
                {
                    for (int y = 0; y < _grid.YSize; y++)
                    {
                        if (_grid.GridContainer[x][y].X == n.X && _grid.GridContainer[x][y].Y == n.Y)
                        {
                            _grid.GridContainer[x][y].Color = Color.Aqua;
                        }
                    }
                }
            }

            _drawing.DrawGrid();
        }

        private void RandomizeStates(object sender, RoutedEventArgs e)
        {
            _drawing.InvaildateGrid();
            UpdateGridParameters();

            var rnd = new Random();
            int.TryParse(MaxStatesCount.Text, out var maxState);

            if (maxState < 1)
            {
                maxState = 1;
            }

            if (maxState > _grid.XSize * _grid.YSize)
            {
                maxState = _grid.XSize * _grid.YSize; 
            }

            int i = maxState;
            while (i > 0)
            {
                var x = rnd.Next(0, _grid.XSize);
                var y = rnd.Next(0, _grid.YSize);

                if (_grid.GridContainer[x][y].State == 0)
                {
                    _grid.GridContainer[x][y].State = ++_grid.ActiveCellsCount;
                    i--;
                }
            }
            _drawing.DrawGrid();
        }
    }
}
