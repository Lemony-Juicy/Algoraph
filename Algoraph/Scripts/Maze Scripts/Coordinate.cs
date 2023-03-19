using System;

namespace Algoraph.Scripts.Maze_Scripts
{
    internal class Coordinate
    {
        public int x { get; private set; }
        public int y { get; private set; }

        public Coordinate(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public float Distance(Coordinate c)
        {
            // Pythagoras's theorem
            double x_squared = Math.Pow(c.x-x, 2);
            double y_squared = Math.Pow(c.y-y, 2);

            return (float)Math.Pow(x_squared + y_squared, 0.5);
        }

        public Coordinate Add(int x, int y)
        {
            return new Coordinate(this.x + x, this.y + y);
        }

        public override bool Equals(object? obj)
        {
            Coordinate? c = obj as Coordinate;
            return c != null && c.x == x && c.y == y;
        }
    }
}
