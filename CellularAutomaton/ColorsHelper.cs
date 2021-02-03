using System;
using System.Drawing;

namespace CellularAutomaton
{
    public static class ColorsHelper
    {
        static readonly Random rnd = new Random();

        public static Color GetColor()
        {
            var gen = rnd.NextDouble() * (1 - 0.1) + 0.1;

            Color firstColor = Color.FromArgb(rnd.Next(245, 255), rnd.Next(20, 255), rnd.Next(20, 255), rnd.Next(20, 255));
            Color secondColor = Color.FromArgb(rnd.Next(245, 255), rnd.Next(20, 255), rnd.Next(20, 255), rnd.Next(20, 255));

            return MixColors(firstColor, secondColor, gen);
        }


        private static Color MixColors(Color first, Color second, double percentage)
        {
            return Color.FromArgb
            (
                first.A + (int)((second.A - first.A) * percentage),
                first.R + (int)((second.R - first.R) * percentage),
                first.G + (int)((second.G - first.G) * percentage),
                first.B + (int)((second.B - first.B) * percentage)
            );
        }
    }
}
