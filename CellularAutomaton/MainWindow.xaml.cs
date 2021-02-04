using System;
using System.Drawing;
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
        private Processor _processor;
        private AdvancedProcessor _advancedProcessor;
        private int _dualPhaseState;
        private Color _dualPhaseColor;
        private bool _borderSelection;

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

            secondPhaseMethod.ItemsSource = Enum.GetValues(typeof(SecondPhaseMethod)).Cast<SecondPhaseMethod>();
            secondPhaseMethod.SelectedIndex = 0;

            showGrid.Checked += ShowGrid_Checked;
            showGrid.Unchecked += ShowGrid_Checked;

            borderSize.ValueChanged += borderSize_ValueChanged;

            _processor = new Processor(_grid, _drawing, Dispatcher);
            _advancedProcessor = new AdvancedProcessor(_grid);
        }

        private void ShowGrid_Checked(object sender, RoutedEventArgs e)
        {
            _drawing.SwitchGrid(showGrid.IsChecked == true);
        }

        private void RefreshGrid(object sender, EventArgs e)
        {
            _isSimulationComplete = false;
            _processor.Substructural = false;
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

            if (_isSimulationComplete && cell != null && e.LeftButton == MouseButtonState.Pressed)
            {
                if (_borderSelection)
                {
                    _advancedProcessor.CalculateBorderForArea(cell, openBorders.IsChecked == true);
                    _bordersCalculated = true;
                    showBorders.IsChecked = true;
                    _drawing.DrawGrid();
                    _borderSelection = false;
                return;
                }

                int state = cell.State;
                if (_dualPhaseState < 1)
                {
                    cell.State = ++_grid.ActiveCellsCount;
                    cell.DualPhaseProtected = true;
                    _dualPhaseState = cell.State;
                    _dualPhaseColor = cell.Color;
                    cell.DualPhaseProtected = true;
                }
                else
                {
                    cell.SetState(_dualPhaseState);
                    cell.Color =_dualPhaseColor;
                    cell.DualPhaseProtected = true;
                }
                for (int i = 0; i < _grid.XSize; i++)
                {
                    for (int j = 0; j < _grid.YSize; j++)
                    {
                        var checkCell = _grid.GridContainer[i][j];
                        if (checkCell.State == state)
                        {
                            checkCell.SetState(cell.State);
                            checkCell.Color = cell.Color;
                            checkCell.DualPhaseProtected = true;
                        }
                    }
                }
                _drawing.DrawGrid();
                return;
            }

            if (cell != null)
            {
                _drawing.UpdateCell(cell, e.RightButton == MouseButtonState.Pressed);
                _drawing.DrawGrid();
            }
        }


        private async void RunGeneration_Click(object sender, RoutedEventArgs e)
        {
            if (_isSimulationComplete && (SecondPhaseMethod)secondPhaseMethod.SelectedItem == SecondPhaseMethod.DualPhase)
            {
                PrepareForDualPhase();
            }
            else if (_isSimulationComplete && (SecondPhaseMethod)secondPhaseMethod.SelectedItem == SecondPhaseMethod.SubStructural)
            {
                PrepareForSubStructural();
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

            if (!_token.IsCancellationRequested)
            {
                _isSimulationComplete = true;
            }

            _source.Dispose();
            _source = new CancellationTokenSource();


            if (showBorders.IsChecked == true)
            {
                DrawBorders();
            }
        }

        private void PrepareForSubStructural()
        {
            for (int i = 0; i < _grid.XSize; i++)
            {
                for (int j = 0; j < _grid.YSize; j++)
                {
                    _grid.GridContainer[i][j].StateChanged = false;
                }
            }
            _processor.Substructural = true;
            _processor.SubStructuralRandomization(MaxStatesCount.Value.Value);
            _drawing.DrawGrid();
        }

        private void PrepareForDualPhase()
        {
            _processor.Substructural = false;
            for (int i = 0; i < _grid.XSize; i++)
            {
                for (int j = 0; j < _grid.YSize; j++)
                {
                    var checkCell = _grid.GridContainer[i][j];
                    if (!checkCell.DualPhaseProtected && checkCell.State > -1)
                    {
                        checkCell.State = 0;
                    }
                }
            }
            _processor.RandomizeStates(MaxStatesCount.Value.Value);
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
            _processor.OpenBorders = openBorders;
            _processor.MutationProbability = mutationProbability;
            _processor.Generate(alghorithm, ct);
        }

        private async void DrawBorders()
        {
            var openBorder = openBorders.IsChecked == true;
            var brSize = borderSize.Value.Value;

            EnableControls(false);

            await Task.Run(() => {
                _advancedProcessor.CalculateBorders(brSize, openBorder);
            });

            EnableControls(true);
            _bordersCalculated = true;

            _drawing.DrawGrid();
        }

        private void RandomizeStates_Click(object sender, RoutedEventArgs e)
        {
            _isSimulationComplete = false;
            _bordersCalculated = false;
            _drawing.InvaildateGrid();
            UpdateGridParameters();

            _processor.RandomizeStates(MaxStatesCount.Value.Value);
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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            _borderSelection = true;
        }
    }
}
