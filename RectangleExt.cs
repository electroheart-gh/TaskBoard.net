using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskBoardWf
{
    internal class RectangleExt
    {
        // Create Rectangle from two Points, avoiding negative value issues of Rectangle implicitly
        public static Rectangle Create(Point point1, Point point2)
        {
            return Rectangle.FromLTRB(
                Math.Min(point1.X, point2.X),
                Math.Min(point1.Y, point2.Y),
                Math.Max(point1.X, point2.X),
                Math.Max(point1.Y, point2.Y)
            );
        }
    }
}
