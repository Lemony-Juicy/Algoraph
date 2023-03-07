using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Algoraph.Scripts.Maze_Scripts
{
    internal class Maze
    {
        WriteableBitmap bm;
        public Color pathColour;
        public Color bgColour;
        MazeGraph mazeGraph;

        int mazeWidth;
        int mazeHeight;
        Random random = new Random();


        /// <summary>
        /// MAZE WIDTH AND HEIGHT MUST BE ODD
        /// </summary>
        /// <param name="mazeWidth"></param>
        /// <param name="mazeHeight"></param>
        /// <param name="bm"></param>
        /// <param name="pathColour"></param>
        public Maze(MazeGraph mazeGraph, int mazeWidth, int mazeHeight, WriteableBitmap bm, Color pathColour, Color bgColour)
        {
            this.mazeWidth = mazeWidth;
            this.mazeHeight = mazeHeight;

            this.mazeGraph = mazeGraph;

            this.pathColour = pathColour;
            this.bgColour = bgColour;
            this.bm = bm;

            ResetMaze();
        }

        public void ClearCanvas()
        {
            bm.FillRectangle(0, 0, mazeWidth, mazeHeight, bgColour);
        }

        public void ResetMaze()
        {
            mazeGraph.Reset();
            ClearCanvas();
        }

        public void CreateMazeFromNodes(Node[] nodes)
        {
            ClearCanvas();
            foreach (MazeNode node in nodes)
            {
                foreach (MazeNode con in node.nodeConnections)
                {
                    Coordinate mazeCoord1 = node.relativePos.Mult(2).Add(1, 1);
                    Coordinate mazeCoord2 = con.relativePos.Mult(2).Add(1, 1);
                    bm.DrawRectangle(mazeCoord1.x, mazeCoord1.y, mazeCoord2.x, mazeCoord2.y, pathColour);
                }
            }
        }

        int GetRandomMazeYCoord()
        {
            int[] y_coords = Enumerable.Range(1, (mazeHeight - 2) / 2).ToArray();
            return y_coords[random.Next(y_coords.Length)] * 2 + 1;
        }

        public async Task MakeMaze(Coordinate? graphPosition = null)
        {
            ResetMaze();
            graphPosition ??= new Coordinate(random.Next(mazeGraph.graphWidth), random.Next(mazeGraph.graphHeight));

            MazeNode? start = MazeNode.FindNode(graphPosition, mazeGraph.nodes);
            if (start != null)
            {
                await mazeGraph.CreateSimpleMazeAnimation();
                CreateMazeFromNodes(mazeGraph.nodes);
            }

            bm.SetPixel(0, GetRandomMazeYCoord(), pathColour);
            bm.SetPixel(mazeWidth - 1, GetRandomMazeYCoord(), pathColour);
        }
    }
}
