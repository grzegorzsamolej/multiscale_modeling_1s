using System.Drawing;

namespace CellularAutomaton
{
    public class Cell
    {
        private int _state;
        public int State 
        { 
            get => _state; 
            set 
            {
                _state = value;
                switch (_state)
                {
                    case -1:
                        Color = Color.Black;
                        break;
                    case 0:
                        Color = Color.White;
                        break;
                    default:
                        Color = ColorsHelper.GetColor();
                        break;
                }
            } 
        }
        public int X { get; private set; }
        public int Y { get; private set; }
        public Color Color { get; set; }
        public bool StateChanged { get; set; }
        public bool IsBorder { get; set; }

        public void SetState(int state)
        {
            _state = state;
            StateChanged = true;
        }

        public Cell(int x, int y)
        {
            X = x;
            Y = y;
            Color = Color.White;
            State = 0;
        }
    }
}
