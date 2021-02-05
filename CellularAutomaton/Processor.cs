using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Threading;

namespace CellularAutomaton
{
    public class Processor
    {
        public BoundaryConditions boundaryConditions { get; set; }
        public int MutationProbability = 50;
        public bool Substructural { get; set; }

        private readonly Grid _grid;
        private readonly NeighbourSeeker _seeker;
        private readonly Drawing _drawing;

        private readonly Dispatcher _dispatcher;
        

        public Processor(Grid grid, Drawing drawing, Dispatcher dispatcher)
        {
            _grid = grid;
            _seeker = new NeighbourSeeker(_grid);
            _drawing = drawing;
            _dispatcher = dispatcher;
        }

        public void RandomizeStates(int maxStateCount)
        {
            var rnd = new Random();

            List<Cell> allCells = new List<Cell>();

            for (int m = 0; m < _grid.XSize; m++)
            {
                for (int j = 0; j < _grid.YSize; j++)
                {
                    allCells.Add(_grid.GridContainer[m][j]);
                }
            }

            var freeCells = allCells.Where(x => x.State == 0).Count();

            if (maxStateCount > freeCells)
            {
                maxStateCount = freeCells;
            }

            int i = maxStateCount;
            while (i > 0)
            {
                var x = rnd.Next(0, allCells.Count);
                var cell = allCells[x];

                if (cell.State == 0)
                {
                    cell.State = ++_grid.ActiveCellsCount;
                    i--;
                }
            }
        }

        public void SubStructuralRandomization(int maxStateCount)
        {
            var rnd = new Random();

            List<Cell> allCells = new List<Cell>();

            for (int m = 0; m < _grid.XSize; m++)
            {
                for (int j = 0; j < _grid.YSize; j++)
                {
                    allCells.Add(_grid.GridContainer[m][j]);
                }
            }

            var groups = allCells.Where(x => x.State > 0).GroupBy(g => g.State);

            foreach (var group in groups.Where(x => x.Count() > 1))
            {
                var cellCount = group.Count();
                var state = group.Key;

                if (maxStateCount > cellCount)
                {
                    maxStateCount = cellCount;
                }

                int i = maxStateCount;
                var tempGroup = group.ToList();
                while (i > 0)
                {
                    var x = rnd.Next(0, cellCount);
                    var cell = tempGroup[x];

                    if (cell.State == state)
                    {
                        cell.State = ++_grid.ActiveCellsCount;
                        cell.SubStructuralState = state;
                        i--;
                    }
                }
            }
        }

        public void Generate(GenerationAlghorithm alghorithm, CancellationToken ct)
        {
            switch (alghorithm)
            {
                case GenerationAlghorithm.MooreRule:
                    BasicAlghorithm(SeekMethod.Rule1, ct);
                    break;
                case GenerationAlghorithm.VonNeumanRule:
                    BasicAlghorithm(SeekMethod.Rule2, ct);
                    break;
                case GenerationAlghorithm.Rule3:
                    BasicAlghorithm(SeekMethod.Rule3, ct);
                    break;
                case GenerationAlghorithm.ComplexRule:
                    ComplexAlghorithm(ct);
                    break;
            }
        }

        public void ComplexAlghorithm(CancellationToken ct)
        {
            bool gridCompleted;
            Random rnd = new Random();

            do
            {
                gridCompleted = true;
                for (int x = 0; x < _grid.XSize; x++)
                {
                    for (int y = 0; y < _grid.YSize; y++)
                    {
                        if (ct.IsCancellationRequested)
                        {
                            return;
                        }
                        var cell = _grid.GridContainer[x][y];
                        if (cell.State != 0 && !Substructural)
                        {
                            continue;
                        }

                        var neighbours = _seeker.GetNeighbours(cell, boundaryConditions, SeekMethod.Rule1, Substructural).Where(x => x.State > 0 && !x.StateChanged);

                        var color = neighbours.GroupBy(x => x.Color).OrderByDescending(o => o.Count()).FirstOrDefault();

                        if (color != null && color.Count() >= 5)
                        {
                            gridCompleted = false;
                            cell.Color = color.Key;
                            cell.SetState(color.First().State);
                            if (Substructural)
                            {
                                cell.SubStructuralState = color.First().SubStructuralState;
                            }
                            continue;
                        }

                        neighbours = _seeker.GetNeighbours(cell, boundaryConditions, SeekMethod.Rule2, Substructural).Where(x => x.State > 0 && !x.StateChanged);

                        color = neighbours.GroupBy(x => x.Color).OrderByDescending(o => o.Count()).FirstOrDefault();

                        if (color != null && color.Count() >= 3)
                        {
                            gridCompleted = false;
                            cell.Color = color.Key;
                            cell.SetState(color.First().State);
                            if (Substructural)
                            {
                                cell.SubStructuralState = color.First().SubStructuralState;
                            }
                            continue;
                        }

                        neighbours = _seeker.GetNeighbours(cell, boundaryConditions, SeekMethod.Rule3, Substructural).Where(x => x.State > 0 && !x.StateChanged);

                        color = neighbours.GroupBy(x => x.Color).OrderByDescending(o => o.Count()).FirstOrDefault();

                        if (color != null && color.Count() >= 3)
                        {
                            gridCompleted = false;
                            cell.Color = color.Key;
                            cell.SetState(color.First().State);
                            if (Substructural)
                            {
                                cell.SubStructuralState = color.First().SubStructuralState;
                            }
                            continue;
                        }

                        neighbours = _seeker.GetNeighbours(cell, boundaryConditions, SeekMethod.Rule4, Substructural).Where(x => x.State > 0 && !x.StateChanged);
                        color = neighbours.GroupBy(x => x.Color).OrderByDescending(o => o.Count()).FirstOrDefault();

                        if (color != null)
                        {
                            if (rnd.Next(100) < MutationProbability)
                            {
                                cell.Color = color.Key;
                                cell.SetState(color.First().State);
                                if (Substructural)
                                {
                                    cell.SubStructuralState = color.First().SubStructuralState;
                                }

                            }
                            gridCompleted = false;
                            continue;
                        }
                    }
                }

                for (int x = 0; x < _grid.XSize; x++)
                {
                    for (int y = 0; y < _grid.YSize; y++)
                    {
                        _grid.GridContainer[x][y].StateChanged = false;
                    }
                }

                _dispatcher.Invoke(() =>
                {
                    _drawing.DrawGrid();
                });
            }
            while (!gridCompleted);
        }

        private void BasicAlghorithm(SeekMethod rule, CancellationToken ct)
        {
            bool gridCompleted;

            do
            {
                gridCompleted = true;
                for (int x = 0; x < _grid.XSize; x++)
                {
                    for (int y = 0; y < _grid.YSize; y++)
                    {
                        if (ct.IsCancellationRequested)
                        {
                            return;
                        }
                        var cell = _grid.GridContainer[x][y];
                        if (cell.State != 0 && !Substructural)
                        {
                            continue;
                        }

                        var neighbours = _seeker.GetNeighbours(cell, boundaryConditions, rule, Substructural).Where(x => x.State > 0 && !x.StateChanged);
                        var color = neighbours.GroupBy(x => x.Color).OrderByDescending(o => o.Count()).FirstOrDefault();

                        if (color != null)
                        {
                            gridCompleted = false;
                            cell.Color = color.Key;
                            cell.SetState(color.First().State);
                            if (Substructural)
                            {
                                cell.SubStructuralState = color.First().SubStructuralState;
                            }

                        }
                    }
                }

                for (int x = 0; x < _grid.XSize; x++)
                {
                    for (int y = 0; y < _grid.YSize; y++)
                    {
                        _grid.GridContainer[x][y].StateChanged = false;
                    }
                }

                _dispatcher.Invoke(() =>
                {
                    _drawing.DrawGrid();
                });
            }
            while (!gridCompleted);
        }
    }

    public enum GenerationAlghorithm
    {
        ComplexRule = 1, 
        MooreRule = 2,
        VonNeumanRule = 3,
        Rule3 = 4,
    }

    public enum SecondPhaseMethod
    {
        DualPhase = 1,
        SubStructural = 2
    }

    public enum BoundaryConditions
    {
        Absorbing = 1,
        Periodic = 2
    }
}
