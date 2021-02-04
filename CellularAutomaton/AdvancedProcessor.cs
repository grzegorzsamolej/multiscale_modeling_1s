using System.Linq;
using System.Threading;

namespace CellularAutomaton
{
    public class AdvancedProcessor
    {
        private readonly Grid _grid;
        private readonly NeighbourSeeker _seeker;
        private readonly bool _openBorders;

        public AdvancedProcessor(Grid grid, bool openBorders)
        {
            _grid = grid;
            _seeker = new NeighbourSeeker(_grid);
            _openBorders = openBorders;
        }

        public void CalculateBorders(int borderSize)
        {
            for (int x = 0; x < _grid.XSize; x++)
            {
                for (int y = 0; y < _grid.YSize; y++)
                {
                    _grid.GridContainer[x][y].IsBorder = false;
                }
            }

            for (int x = 0; x < _grid.XSize; x++)
            {
                for (int y = 0; y < _grid.YSize; y++)
                {
                    var cell = _grid.GridContainer[x][y];
                    if (cell.State == -1)
                    {
                        continue;
                    }
                    var neighbours = _seeker.GetAllNeighboursInRadius(cell, _openBorders, borderSize);
                    if (neighbours.Any(n => n.State > 0 && n.State != cell.State))
                    {
                        cell.IsBorder = true;
                    }
                }
            }
        }
    }
}
