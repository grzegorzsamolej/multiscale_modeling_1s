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

        public List<Cell> GetNeighbours(Cell cell, bool openBorder, SeekMethod method)
        {
            switch (method)
            {
                case SeekMethod.Rule1:
                case SeekMethod.Rule4:
                    return GetNeighboursUsingRule1(cell, openBorder);
                case SeekMethod.Rule2:
                    return GetNeighboursUsingRule2(cell, openBorder);
                case SeekMethod.Rule3:
                    return GetNeighboursUsingRule3(cell, openBorder);
                default:
                    throw new ArgumentException("Selected method doesn't exist");
            }
        }

        private List<Cell> GetNeighboursUsingRule1(Cell cell, bool openBorder)
        {
            List<Cell> neighbours = new List<Cell>();

            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    if (i == 0 && j == 0)
                        continue;
                    int x, y, state;
                    if (openBorder)
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
                            state = 0;
                        else
                            state = _grid.GridContainer[x][y].State;
                    }

                    if (state > 0)
                    {
                        neighbours.Add(_grid.GridContainer[x][y]);
                    }
                        
                }
            }
            return neighbours;
        }

        private List<Cell> GetNeighboursUsingRule2(Cell cell, bool openBorder)
        {
            int[,] neighbourPairs = GetNeighbourPairs(SeekMethod.Rule2);
            return GetNeighboursUsingRule2Or3(cell, neighbourPairs, openBorder);
        }

        private List<Cell> GetNeighboursUsingRule3(Cell cell, bool openBorder)
        {
            int[,] neighbourPairs = GetNeighbourPairs(SeekMethod.Rule3);
            return GetNeighboursUsingRule2Or3(cell, neighbourPairs, openBorder);
        }

        private List<Cell> GetNeighboursUsingRule2Or3(Cell cell, int[,] neighbourPairs, bool openBorder)
        {
            List<Cell> neighbours = new List<Cell>();

            for (int i = 0; i <= 3; i++)
            {
                int xPair = neighbourPairs[i, 0];
                int yPair = neighbourPairs[i, 1];
                int x, y, state;

                if (openBorder)
                {
                    x = (xPair + cell.X) >= 0 ? (xPair + cell.X) % (_grid.XSize) : _grid.XSize - 1;
                    y = (yPair + cell.Y) >= 0 ? (yPair + cell.Y) % (_grid.YSize) : _grid.YSize - 1;
                    state = _grid.GridContainer[x][y].State;
                }
                else
                {
                    x = xPair + cell.X;
                    y = yPair + cell.Y;

                    if (x < 0 || y < 0 || x >= _grid.XSize || y >= _grid.XSize)
                        state = 0; //!!! For testing, should be 0 insted of -1
                    else
                        state = _grid.GridContainer[x][y].State;
                }
                if (state > 0)
                    neighbours.Add(_grid.GridContainer[x][y]);
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
