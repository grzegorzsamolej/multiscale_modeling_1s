using System;
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

        public void CalculateBorders(int borderSize, BoundaryConditions boundaryConditions)
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
                    var neighbours = _seeker.GetAllNeighboursInRadius(cell, boundaryConditions, borderSize);
                    if (neighbours.Any(n => n.State > 0 && n.State != cell.State))
                    {
                        cell.IsBorder = true;
                    }
                }
            }
        }

        public void CalculateBorderForArea(Cell cell, BoundaryConditions boundaryConditions)
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
                var neighbours = _seeker.GetAllNeighboursInRadius(x, boundaryConditions, 1);
                if (neighbours.Any(n => n.State != x.State))
                {
                    x.IsBorder = true;
                }
            });

            selectedCells.ForEach(x => x.State = 0);
        }

        public void CleanAllCellsExceptBorders()
        {
            for (int x = 0; x < _grid.XSize; x++)
            {
                for (int y = 0; y < _grid.YSize; y++)
                {
                    _grid.GridContainer[x][y].State = 0;
                }
            }
        }

        public void GenerateInclusions(int inclusionCount, int inclusionSize, BoundaryConditions boundaryConditions, InclusionShape shape, bool simulationCompleted)
        {
            //tu bym dodał clean grid 
            var rnd = new Random();
            if (inclusionCount > _grid.XSize * _grid.YSize)
            {
                inclusionCount = _grid.XSize * _grid.YSize;
            }

            if (inclusionSize > Math.Min(_grid.XSize, _grid.YSize))
            {
                inclusionCount = Math.Min(_grid.XSize, _grid.YSize);
            }

            //get borderCells
            List<Cell> borderCells = new List<Cell>();
            for (int o = 0; o < _grid.XSize; o++)
            {
                for (int p = 0; p < _grid.YSize; p++)
                {
                    if (_grid.GridContainer[o][p].IsBorder)
                        borderCells.Add(_grid.GridContainer[o][p]);
                }
            }
            var borderCellsAmount = borderCells.Count();
            var x = 0;
            var y = 0;

            if (shape == InclusionShape.Square)
            {
                int i = inclusionCount;
                while (i > 0)
                {

                    var cell = getRandomCell(simulationCompleted, rnd, borderCellsAmount, borderCells, ref x, ref y);

                    int flag = 1;
                    var neighbours = _seeker.GetAllNeighboursInRadius(cell, boundaryConditions, inclusionSize / 2);

                    foreach (var element in neighbours)
                    {
                        if (element.State == -1)
                        {
                            flag = 0;
                            break;
                        }
                    }
                    if (cell.State == -1 || flag == 0)
                    {
                        continue;
                    }

                    CreateSquareInculsion(inclusionSize, neighbours);
                    cell.State = -1;
                    i--;

                }
            }
            //no wiadomo nie if else tylko po tym co jest wybrane w gui
            else
            {
                //jak w kwadratach
                List<Cell> cellsToPaintBlack = new List<Cell>();
                int i = inclusionCount;
                while (i > 0)
                {
                    int x_inclusion = 0;
                    int y_inclusion = 0;
                    var cell = getRandomCell(simulationCompleted, rnd, borderCellsAmount, borderCells, ref x_inclusion, ref y_inclusion);

                    cellsToPaintBlack.Add(cell);
                    int flag = 1;
                    //zbiera sąsiadów w "kwadracie" jak w poprzednim
                    var neighbours = _seeker.GetAllNeighboursInRadius(cell, boundaryConditions, inclusionSize);

                    foreach (var element in neighbours)
                    {
                        double lenght = Math.Sqrt((x_inclusion - element.X) * (x_inclusion - element.X) + (y_inclusion - element.Y) * (y_inclusion - element.Y));

                        if (lenght <= inclusionSize - 1)
                        {
                            if (element.State == -1)
                            {
                                flag = 0;
                                cellsToPaintBlack.Clear();
                                break;
                            }
                            else
                            {
                                cellsToPaintBlack.Add(element);
                            }
                        }

                        if (cell.State == -1 || flag == 0)
                        {
                            continue;
                        }
                    }
                    i--;
                }
                foreach (var cell in cellsToPaintBlack)
                    cell.State = -1;
            }
        }


        private Cell getRandomCell(bool simulationCompleted, Random rnd, int borderCellsAmount, List<Cell> borderCells, ref int x, ref int y)
        {
            if (!simulationCompleted)
            {
                x = rnd.Next(0, _grid.XSize);
                y = rnd.Next(0, _grid.YSize);
            }
            else
            {
                var tempNumb = rnd.Next(0, borderCellsAmount);
                x = borderCells.ElementAt(tempNumb).X;
                y = borderCells.ElementAt(tempNumb).Y;
            }
            return _grid.GridContainer[x][y];
        }
        private void CreateSquareInculsion(int inclusionSize, List<Cell> cells)
        {
            if (inclusionSize % 2 == 1)
            {
                cells.ForEach(x => x.State = -1);
                return;
            }

            var xMin = cells.GroupBy(x => x.X).OrderBy(o => o.Key).First().Key;
            var yMin = cells.GroupBy(x => x.Y).OrderBy(o => o.Key).First().Key;


            cells.Where(x => x.X >= xMin && x.X < xMin + inclusionSize && x.Y >= yMin && x.Y < yMin + inclusionSize).ToList().ForEach(x => x.State = -1);
        }
    }

    public enum InclusionShape
    {
        Square,
        Circle
    }
}
