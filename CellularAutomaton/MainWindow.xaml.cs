using System;
using System.Drawing;
using System.IO;
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
        private SaveLoadHelper _saveLoadHelper;

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

            boundaryConditions.ItemsSource = Enum.GetValues(typeof(BoundaryConditions)).Cast<BoundaryConditions>();
            boundaryConditions.SelectedIndex = 0;

            inclusionShape.ItemsSource = Enum.GetValues(typeof(InclusionShape)).Cast<InclusionShape>();
            inclusionShape.SelectedIndex = 0;

            showGrid.Checked += ShowGrid_Checked;
            showGrid.Unchecked += ShowGrid_Checked;

            borderSize.ValueChanged += borderSize_ValueChanged;

            _processor = new Processor(_grid, _drawing, Dispatcher);
            _advancedProcessor = new AdvancedProcessor(_grid);
            _saveLoadHelper = new SaveLoadHelper(_grid, _drawing);
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
                    _advancedProcessor.CalculateBorderForArea(cell, (BoundaryConditions)boundaryConditions.SelectedItem);
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
            var rule = (GenerationAlghorithm)generationRule.SelectedItem;
            var mutationProbabilityValue = mutationProbability.Value.Value;
            var brSize = borderSize.Value.Value;

            EnableControls(false);
            stopGeneration.IsEnabled = true;

            _token = _source.Token;
            var boundConditions = (BoundaryConditions)boundaryConditions.SelectedItem;
            await Task.Run(() => RunGeneration(rule, boundConditions, mutationProbabilityValue, _token), _token);

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
            boundaryConditions.IsEnabled = enable;
            generationRule.IsEnabled = enable;
            borderSize.IsEnabled = enable;
            showBorders.IsEnabled = enable;
            mutationProbability.IsEnabled = enable;
            secondPhaseMethod.IsEnabled = enable;
            showBorders.IsEnabled = enable;
            borderSize.IsEnabled = enable;
            removeCellsExceptBorders.IsEnabled = enable;
            borderSelection.IsEnabled = enable;
        }

        private void RunGeneration(GenerationAlghorithm alghorithm, BoundaryConditions boundaryConditions, int mutationProbability, CancellationToken ct)
        {
            _processor.boundaryConditions = boundaryConditions;
            _processor.MutationProbability = mutationProbability;
            _processor.Generate(alghorithm, ct);
        }

        private async void DrawBorders()
        {
            var brSize = borderSize.Value.Value;

            EnableControls(false);
            var boundConditions = (BoundaryConditions)boundaryConditions.SelectedItem;

            await Task.Run(() => {
                _advancedProcessor.CalculateBorders(brSize, boundConditions);
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

        private void removeCellsExceptBorders_Click(object sender, RoutedEventArgs e)
        {
            if (_isSimulationComplete)
            {
                if (!_isSimulationComplete)
                {
                    DrawBorders();
                }
                _drawing.ShowBorders = true;
                showBorders.IsChecked = true;
                _advancedProcessor.CleanAllCellsExceptBorders();
                _drawing.DrawGrid();
            }
        }

        private void generateInclusions_Click(object sender, RoutedEventArgs e)
        {
            if (_isSimulationComplete)
            {
                _advancedProcessor.CalculateBorders(1, (BoundaryConditions)boundaryConditions.SelectedItem);
                _advancedProcessor.GenerateInclusions(inclusionCount.Value.Value, inclusionSize.Value.Value, (BoundaryConditions)boundaryConditions.SelectedItem, (InclusionShape)inclusionShape.SelectedItem, true);
            }
            else
            {
                _advancedProcessor.GenerateInclusions(inclusionCount.Value.Value, inclusionSize.Value.Value, (BoundaryConditions)boundaryConditions.SelectedItem, (InclusionShape)inclusionShape.SelectedItem, false);
            }
            _drawing.DrawGrid();
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            var filename = OpenDialog(new Microsoft.Win32.SaveFileDialog());
            if (!string.IsNullOrEmpty(filename))
            {
                _saveLoadHelper.Save(filename);
            }
        }

        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            var filename = OpenDialog(new Microsoft.Win32.OpenFileDialog());
            if (!string.IsNullOrEmpty(filename))
            {
                _saveLoadHelper.Load(filename);
            }
            if (Path.GetExtension(filename) == ".txt")
            {
                _isSimulationComplete = true;
            }
        }

        private string OpenDialog(Microsoft.Win32.FileDialog dlg)
        {
            dlg.FileName = "save";
            dlg.DefaultExt = ".bmp";
            dlg.Filter = "Bitmap Image (.bmp)|*.bmp|Jpeg image (.jpg)|*.jpg|Text file (.txt)|*.txt";

            bool? result = dlg.ShowDialog();

            if (result == true)
            {
                return dlg.FileName;
                
            }

            return string.Empty;
        }

        private void MenuItem_Click_2(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
