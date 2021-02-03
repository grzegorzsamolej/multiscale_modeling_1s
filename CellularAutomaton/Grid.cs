namespace CellularAutomaton
{
    public class Grid
    {
        public Cell[][] GridContainer { get; private set; }
        public int XSize { get; private set; }
        public int YSize { get; private set; }
        public bool NeedsRefresh { get; set; } = true;
        public int ActiveCellsCount { get; set; }

        public void InitGrid(int x, int y)
        {
            ActiveCellsCount = 0;

            XSize = x;
            YSize = y;

            GridContainer = new Cell[XSize][];
            for (int i = 0; i < XSize; i++)
            {
                GridContainer[i] = new Cell[YSize];
                for (int j = 0; j < YSize; j++)
                {
                    GridContainer[i][j] = new Cell(i, j);
                }
            }

            NeedsRefresh = false;
        }
    }
}
