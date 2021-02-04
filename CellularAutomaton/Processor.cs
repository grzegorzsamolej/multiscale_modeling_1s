using System;
using System.Linq;
using System.Threading;
using System.Windows.Threading;

namespace CellularAutomaton
{
    public class Processor
    {
        private readonly Grid _grid;
        private readonly NeighbourSeeker _seeker;
        private readonly Drawing _drawing;
        private readonly bool _openBorders;
        private readonly Dispatcher _dispatcher;
        private readonly int _mutationProbability = 50;

        public Processor(Grid grid, Drawing drawing, Dispatcher dispatcher, bool openBorders, int mutationProbability)
        {
            _grid = grid;
            _seeker = new NeighbourSeeker(_grid);
            _drawing = drawing;
            _openBorders = openBorders;
            _dispatcher = dispatcher;
            if (mutationProbability > 0 && mutationProbability < 100)
            {
                _mutationProbability = mutationProbability;
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
                        if (cell.State != 0)
                        {
                            continue;
                        }

                        var neighbours = _seeker.GetNeighbours(cell, _openBorders == true, SeekMethod.Rule1).Where(x => x.State > 0 && !x.StateChanged);

                        var color = neighbours.GroupBy(x => x.Color).OrderByDescending(o => o.Count()).FirstOrDefault();

                        if (color != null && color.Count() >= 5)
                        {
                            gridCompleted = false;
                            _grid.GridContainer[x][y].Color = color.Key;
                            _grid.GridContainer[x][y].SetState(color.First().State);
                            continue;
                        }

                        neighbours = _seeker.GetNeighbours(cell, _openBorders == true, SeekMethod.Rule2).Where(x => x.State > 0 && !x.StateChanged);

                        color = neighbours.GroupBy(x => x.Color).OrderByDescending(o => o.Count()).FirstOrDefault();

                        if (color != null && color.Count() >= 3)
                        {
                            gridCompleted = false;
                            _grid.GridContainer[x][y].Color = color.Key;
                            _grid.GridContainer[x][y].SetState(color.First().State);
                            continue;
                        }

                        neighbours = _seeker.GetNeighbours(cell, _openBorders == true, SeekMethod.Rule3).Where(x => x.State > 0 && !x.StateChanged);

                        color = neighbours.GroupBy(x => x.Color).OrderByDescending(o => o.Count()).FirstOrDefault();

                        if (color != null && color.Count() >= 3)
                        {
                            gridCompleted = false;
                            _grid.GridContainer[x][y].Color = color.Key;
                            _grid.GridContainer[x][y].SetState(color.First().State);
                            continue;
                        }

                        neighbours = _seeker.GetNeighbours(cell, _openBorders == true, SeekMethod.Rule4).Where(x => x.State > 0 && !x.StateChanged);
                        color = neighbours.GroupBy(x => x.Color).OrderByDescending(o => o.Count()).FirstOrDefault();

                        if (color != null)
                        {
                            if (rnd.Next(100) < _mutationProbability)
                            {
                                _grid.GridContainer[x][y].Color = color.Key;
                                _grid.GridContainer[x][y].SetState(color.First().State);
                                
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
                        if (cell.State != 0)
                        {
                            continue;
                        }

                        var neighbours = _seeker.GetNeighbours(cell, _openBorders == true, rule).Where(x => x.State > 0 && !x.StateChanged);
                        var color = neighbours.GroupBy(x => x.Color).OrderByDescending(o => o.Count()).FirstOrDefault();

                        if (color != null)
                        {
                            gridCompleted = false;
                            _grid.GridContainer[x][y].Color = color.Key;
                            _grid.GridContainer[x][y].SetState(color.First().State);

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
}
