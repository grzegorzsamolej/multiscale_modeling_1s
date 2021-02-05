using System;
using System.Collections.Generic;

namespace CellularAutomaton
{
    public class NeighbourSeeker
    {
        private readonly Grid _grid;

        public NeighbourSeeker(Grid grid)
        {
            _grid = grid;
        }

        public List<Cell> GetNeighbours(Cell cell, BoundaryConditions boundaryConditions, SeekMethod method, bool substructural)
        {
            switch (method)
            {
                case SeekMethod.Rule1:
                case SeekMethod.Rule4:
                    return GetNeighboursUsingRule1(cell, boundaryConditions, substructural);
                case SeekMethod.Rule2:
                    return GetNeighboursUsingRule2(cell, boundaryConditions, substructural);
                case SeekMethod.Rule3:
                    return GetNeighboursUsingRule3(cell, boundaryConditions, substructural);
                default:
                    throw new ArgumentException("Selected method doesn't exist");
            }
        }

        public List<Cell> GetAllNeighboursInRadius(Cell cell, BoundaryConditions boundaryConditions, int radius)
        {
            List<Cell> neighbours = new List<Cell>();

            for (int i = -radius; i <= radius; i++)
            {
                for (int j = -radius; j <= radius; j++)
                {
                    if (i == 0 && j == 0)
                        continue;
                    int x, y, state;
                    if (boundaryConditions == BoundaryConditions.Periodic)
                    {
                        x = (i + cell.X) >= 0 ? (i + cell.X) % (_grid.XSize) : (i + cell.X) + _grid.XSize;
                        y = (j + cell.Y) >= 0 ? (j + cell.Y) % (_grid.YSize) : (j + cell.Y) + _grid.YSize;
                        state = _grid.GridContainer[x][y].State;
                    }
                    else
                    {
                        x = i + cell.X;
                        y = j + cell.Y;

                        if (x < 0 || y < 0 || x >= _grid.XSize || y >= _grid.YSize)
                            state = -2;
                        else
                            state = _grid.GridContainer[x][y].State;
                    }

                    if (state > -2)
                    {
                        neighbours.Add(_grid.GridContainer[x][y]);
                    }
                        
                }
            }
            return neighbours;
        }

        private List<Cell> GetNeighboursUsingRule1(Cell cell, BoundaryConditions boundaryConditions, bool substructural)
        {
            List<Cell> neighbours = new List<Cell>();

            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    if (i == 0 && j == 0)
                        continue;
                    int x, y, state;
                    Cell currentCell;
                    if (boundaryConditions == BoundaryConditions.Periodic)
                    {
                        x = (i + cell.X) >= 0 ? (i + cell.X) % (_grid.XSize) : (i + cell.X) + _grid.XSize;
                        y = (j + cell.Y) >= 0 ? (j + cell.Y) % (_grid.YSize) : (j + cell.Y) + _grid.YSize;
                        currentCell = _grid.GridContainer[x][y];
                        state = currentCell.State;
                    }
                    else
                    {
                        x = i + cell.X;
                        y = j + cell.Y;

                        if (x < 0 || y < 0 || x >= _grid.XSize || y >= _grid.YSize)
                        {
                            state = 0;
                        }
                        else
                        {
                            currentCell = _grid.GridContainer[x][y];
                            state = currentCell.State;
                        }
                            
                    }

                    if (state > 0)
                    {
                        currentCell = _grid.GridContainer[x][y];

                        if ((substructural && cell.State == currentCell.SubStructuralState) || (!currentCell.DualPhaseProtected && !substructural))
                        {
                            neighbours.Add(_grid.GridContainer[x][y]);
                        }
                    }
                }
            }
            return neighbours;
        }

        private List<Cell> GetNeighboursUsingRule2(Cell cell, BoundaryConditions boundaryConditions, bool substructural)
        {
            int[,] neighbourPairs = GetNeighbourPairs(SeekMethod.Rule2);
            return GetNeighboursUsingRule2Or3(cell, neighbourPairs, boundaryConditions, substructural);
        }

        private List<Cell> GetNeighboursUsingRule3(Cell cell, BoundaryConditions boundaryConditions, bool substructural)
        {
            int[,] neighbourPairs = GetNeighbourPairs(SeekMethod.Rule3);
            return GetNeighboursUsingRule2Or3(cell, neighbourPairs, boundaryConditions, substructural);
        }

        private List<Cell> GetNeighboursUsingRule2Or3(Cell cell, int[,] neighbourPairs, BoundaryConditions boundaryConditions, bool substructural)
        {
            List<Cell> neighbours = new List<Cell>();

            for (int i = 0; i <= 3; i++)
            {
                int xPair = neighbourPairs[i, 0];
                int yPair = neighbourPairs[i, 1];
                int x, y, state;
                Cell currentCell;

                if (boundaryConditions == BoundaryConditions.Periodic)
                {
                    x = (xPair + cell.X) >= 0 ? (xPair + cell.X) % (_grid.XSize) : _grid.XSize - 1;
                    y = (yPair + cell.Y) >= 0 ? (yPair + cell.Y) % (_grid.YSize) : _grid.YSize - 1;
                    currentCell = _grid.GridContainer[x][y];
                    state = currentCell.State;
                }
                else
                {
                    x = xPair + cell.X;
                    y = yPair + cell.Y;

                    if (x < 0 || y < 0 || x >= _grid.XSize || y >= _grid.YSize)
                        state = 0;
                    else
                    {
                        currentCell = _grid.GridContainer[x][y];
                        state = currentCell.State;
                    }
                }

                if (state > 0)
                {
                    currentCell = _grid.GridContainer[x][y];

                    if ((substructural && cell.State == currentCell.SubStructuralState) || (!currentCell.DualPhaseProtected && !substructural))
                    {
                        neighbours.Add(_grid.GridContainer[x][y]);
                    }
                }    
            }
            return neighbours;
        }

        private int[,] GetNeighbourPairs(SeekMethod method)
        {
            int[,] neighbourPairs2 = { { -1, 0 }, { 1, 0 }, { 0, -1 }, { 0, 1 } };
            int[,] neighbourPairs3 = { { -1, -1 }, { 1, 1 }, { 1, -1 }, { -1, 1 } };
            return method == SeekMethod.Rule2 ? neighbourPairs2 : neighbourPairs3;
        }
    }


    public enum SeekMethod
    {
        Rule1 = 1, //Moore
        Rule2 = 2, //NearestMoore
        Rule3 = 3, //FurtherMoore
        Rule4 = 4
    }
}
