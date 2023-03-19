using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace Algoraph.Scripts.Maze_Scripts
{
    internal class MazeNode : Node
    {
        // Relative position is the relative position in the grid of nodes, 
        // EG: 
        public Coordinate relativePos { get; private set; }

        // These attributes are for the A* Algorithm
        public float F_cost = float.PositiveInfinity;
        public float G_cost = float.PositiveInfinity;
        public float H_cost = float.PositiveInfinity;

        public MazeNode? previous = null;


        Coordinate[] adjacentCoords;

        List<MazeNode> adjacencies;

        public bool isEdge { get; private set; }
        public bool isCorner { get; private set; }

        public MazeNode(Editor editor, Point location, Coordinate relativePos, int graphWidth, int graphHeight, List<Node>? connections = null, string name = "") : base(editor, location, name, connections)
        {
            int x = relativePos.x;
            int y = relativePos.y;

            isEdge = x == 0 || x == graphWidth - 1 || y == 0 || y == graphHeight - 1;
            isCorner = (x == 0 || x == graphWidth - 1) && (y == 0 || y == graphHeight - 1);

            this.relativePos = relativePos;
            adjacentCoords = new Coordinate[4]
            {
                relativePos.Add(1, 0),
                relativePos.Add(-1, 0),
                relativePos.Add(0, 1),
                relativePos.Add(0, -1)

            };
        }

        public void SetAdjecentNodes(MazeNode[] nodes)
        {
            adjacencies = nodes.Where(n => Array.Find(adjacentCoords, a => a.Equals(n.relativePos)) != null).ToList();
        }

        public IEnumerable<MazeNode> GetFreeAdjacentNodes(List<MazeNode> connector)
        {
            return adjacencies.Where(a => !nodeConnections.Contains(a) && !connector.Contains(a));
        }

        public void ChangeColour(Brush colour)
        {
            this.nodeButton.Background = colour;
        }

        public static MazeNode? FindNode(Coordinate coord, MazeNode[] nodes)
        {
            return Array.Find(nodes, n => n.relativePos.Equals(coord));
        }

        public bool IsConnected(Node node)
        {
            return nodeConnections.Contains(node);
        }
    }
}
