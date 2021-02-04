using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace CellularAutomaton
{
    public partial class MainWindow : Window
    {
        private Drawing _drawing;
        private Grid _grid;
        private CancellationTokenSource _source = new CancellationTokenSource();
        private CancellationToken _token;
        private bool _isSimulationComplete;
        private bool _bordersCalculated;

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
            xCount.ValueChanged += RefreshGrid;
            yCount.ValueChanged += RefreshGrid;

            generationRule.ItemsSource = Enum.GetValues(typeof(GenerationAlghorithm)).Cast<GenerationAlghorithm>();
            generationRule.SelectedIndex = 0;

            showGrid.Checked += ShowGrid_Checked;
            showGrid.Unchecked += ShowGrid_Checked;

            borderSize.ValueChanged += borderSize_ValueChanged;
        }

        private void ShowGrid_Checked(object sender, RoutedEventArgs e)
        {
            _drawing.SwitchGrid(showGrid.IsChecked == true);
        }

        private void RefreshGrid(object sender, EventArgs e)
        {
            _isSimulationComplete = false;
            _bordersCalculated = false;
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

            _drawing.SetGridParameters(xCount.Value.Value, yCount.Value.Value);
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
            }
        }


        private async void RunGeneration_Click(object sender, RoutedEventArgs e)
        {
            if (_isSimulationComplete) 
            {
                return;
            }
            _isSimulationComplete = false;
            _bordersCalculated = false;
            var openBorder = openBorders.IsChecked == true;
            var rule = (GenerationAlghorithm)generationRule.SelectedItem;
            var mutationProbabilityValue = mutationProbability.Value.Value;
            var brSize = borderSize.Value.Value;

            EnableControls(false);
            stopGeneration.IsEnabled = true;

            _token = _source.Token;
            await Task.Run(() => RunGeneration(rule, openBorder, mutationProbabilityValue, _token), _token);

            EnableControls(true);
            stopGeneration.IsEnabled = false;
            _source.Dispose();
            _source = new CancellationTokenSource();
            _isSimulationComplete = true;

            if (showBorders.IsChecked == true)
            {
                DrawBorders();
            }
        }        
        
        private void StopGeneration(object sender, RoutedEventArgs e)
        {
            if (_source != null)
            {
                _source.Cancel();
            }
        }

        private void EnableControls(bool enable)
        {
            runGeneration.IsEnabled = enable;
            xCount.IsEnabled = enable;
            yCount.IsEnabled = enable;
            MaxStatesCount.IsEnabled = enable;
            randomizeStatesBtn.IsEnabled = enable;
            cleanGrid.IsEnabled = enable;
            openBorders.IsEnabled = enable;
            generationRule.IsEnabled = enable;
            borderSize.IsEnabled = enable;
            showBorders.IsEnabled = enable;
        }

        private void RunGeneration(GenerationAlghorithm alghorithm, bool openBorders, int mutationProbability, CancellationToken ct)
        {
            var processor = new Processor(_grid, _drawing, Dispatcher, openBorders, mutationProbability);
            processor.Generate(alghorithm, ct);
        }

        private async void DrawBorders()
        {
            var openBorder = openBorders.IsChecked == true;
            var brSize = borderSize.Value.Value;

            EnableControls(false);

            await Task.Run(() => {
                var processor = new AdvancedProcessor(_grid, openBorder);
                processor.CalculateBorders(brSize);
            });

            EnableControls(true);
            _bordersCalculated = true;

            _drawing.DrawGrid();
        }

        private void RandomizeStates(object sender, RoutedEventArgs e)
        {
            _isSimulationComplete = false;
            _bordersCalculated = false;
            _drawing.InvaildateGrid();
            UpdateGridParameters();

            var rnd = new Random();

            var maxState = MaxStatesCount.Value.Value;

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

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            StopGeneration(null, null);
        }

        private void showBorders_Checked(object sender, RoutedEventArgs e)
        {
            if (!_bordersCalculated && _isSimulationComplete)
            {
                DrawBorders();
            }
            _drawing.ShowBorders = showBorders.IsChecked == true;
            _drawing.DrawGrid();
        }

        private void borderSize_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            _bordersCalculated = false;
            if (!_isSimulationComplete || showBorders.IsChecked != true)
            {
                return;
            }

            DrawBorders();
        }
    }
}
