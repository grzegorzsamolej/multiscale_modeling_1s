using System.Collections.Generic;
using System.Linq;

namespace CellularAutomaton
{
    public class AdvancedProcessor
    {
        private readonly Grid _grid;
        private readonly NeighbourSeeker _seeker;

        public AdvancedProcessor(Grid grid)
        {
            _grid = grid;
            _seeker = new NeighbourSeeker(_grid);
        }

        public void CalculateBorders(int borderSize, bool openBorders)
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
                    var neighbours = _seeker.GetAllNeighboursInRadius(cell, openBorders, borderSize);
                    if (neighbours.Any(n => n.State > 0 && n.State != cell.State))
                    {
                        cell.IsBorder = true;
                    }
                }
            }
        }

        public void CalculateBorderForArea(Cell cell, bool openBorders)
        {
            List<Cell> selectedCells = new List<Cell>();
            for (int x = 0; x < _grid.XSize; x++)
            {
                for (int y = 0; y < _grid.YSize; y++)
                {
                    var checkCell = _grid.GridContainer[x][y];
                    checkCell.IsBorder = false;
                    if (checkCell.State == cell.State)
                    {
                        selectedCells.Add(checkCell);
                    }
                    else
                    {
                        checkCell.State = 0;
                    }
                }
            }

            selectedCells.ForEach(x => {
                var neighbours = _seeker.GetAllNeighboursInRadius(x, openBorders, 1);
                if (neighbours.Any(n => n.State != x.State))
                {
                    x.IsBorder = true;
                }
            });

            selectedCells.ForEach(x => x.State = 0);
        }
    }
}
