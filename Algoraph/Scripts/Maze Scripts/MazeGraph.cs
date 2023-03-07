using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;


namespace Algoraph.Scripts.Maze_Scripts
{
    internal class MazeGraph
    {
        #region Initialisation
        public MazeNode[] nodes { get; private set; }
        List<Arc> arcs;

        public int graphWidth { get; private set; }
        public int graphHeight { get; private set; }

        Editor ed;
        Random random = new Random();
        MazeNode? startNode = null;
        MazeNode? endNode = null;

        public MazeGraph(Editor ed, int graphWidth, int graphHeight)
        {
            this.graphWidth = graphWidth;
            this.graphHeight = graphHeight;
            this.nodes = new MazeNode[graphWidth * graphHeight];
            this.arcs = new List<Arc>();
            this.ed = ed;
        }

        void AddConnection(MazeNode node1, MazeNode node2)
        {
            Arc arc = new Arc(node1, node2);
            arcs.Add(arc);

            Node.ConnectNodes(node1, node2, arc);
            arc.AddToCanvas(ed.mazeCanvas, renderWeight: false);
        }


        void ClearCheckedNodes()
        {
            if (startNode != null)
            {
                startNode.ChangeColour(Brushes.Transparent);
                startNode = null;
            }
            if (endNode != null)
            {
                endNode.ChangeColour(Brushes.Transparent);
                endNode = null;
            }
        }

        void ResetInfoTextBlocks()
        {
            ed.methods.DFS_TextBlock.Text = "";
            ed.methods.BFS_TextBlock.Text = "";
            ed.methods.Astar_TextBlock.Text = "";
        }

        void ResetAnnotations()
        {
            foreach (Arc arc in arcs)
            {
                arc.ChangeColour(Brushes.Black);
            }
        }

        void OnNodeChecked(object sender, RoutedEventArgs e)
        {
            if (ed.CheckIsProcessing())
            {
                return;
            }
            ResetAnnotations();
            ResetInfoTextBlocks();
            if (startNode == null)
            {
                startNode = ((ToggleButton)sender).Tag as MazeNode;
                startNode.ChangeColour(Brushes.LawnGreen);
            }
            else if (endNode == null)
            {
                endNode = ((ToggleButton)sender).Tag as MazeNode;
                endNode.ChangeColour(Brushes.Red);
            }
            else
            {
                startNode.Uncheck();
                endNode.Uncheck();
                ClearCheckedNodes();
                startNode = ((ToggleButton)sender).Tag as MazeNode;
                startNode.ChangeColour(Brushes.LawnGreen);
            }
        }


        public void Reset()
        {
            ResetInfoTextBlocks();
            ed.mazeCanvas.Children.Clear();
            for (int x = 0, i = 0; x < graphWidth; x++)
            {
                for (int y = 0; y < graphHeight; y++)
                {
                    double locX = (double)x / graphWidth * ed.mazeCanvas.ActualWidth * 0.95 + 35;
                    double locY = (double)y / graphHeight * ed.mazeCanvas.ActualHeight * 0.95 + 35;
                    Point location = new(locX, locY);
                    this.nodes[i] = new MazeNode(ed, location, new Coordinate(x, y), graphWidth, graphHeight, name: $" ");
                    this.nodes[i].ChangeColour(Brushes.Transparent);
                    this.nodes[i].AddToCanvas(ed.mazeCanvas);

                    this.nodes[i].nodeButton.Checked += OnNodeChecked;
                    i++;
                }
            }
            

            foreach(MazeNode n in this.nodes)
            {
                n.SetAdjecentNodes(nodes);
            }
        }
        #endregion

        #region Generating Maze

        MazeNode SelectFreeNodeFromConnector(List<MazeNode> connector)
        {
            IEnumerable<MazeNode> freeNodes = connector.Where(con => con.GetFreeAdjacentNodes(connector).Count() > 0);
            return freeNodes.SelectRandom();
        }

        async Task<bool> ContinueRandomConnection(MazeNode from, List<MazeNode> connector)
        {
            MazeNode current = from;
            IEnumerable<MazeNode> freeNodes = current.GetFreeAdjacentNodes(connector);
            while (freeNodes.Count() > 0)  // If we can't go any further in the branch
            {   
                MazeNode nodeTo = freeNodes.SelectRandom();
                connector.Add(nodeTo);

                if (connector.Count == nodes.Length)
                    return false;

                await ed.WaitForNextCycle();
                AddConnection(current, nodeTo);

                current = nodeTo;
                freeNodes = current.GetFreeAdjacentNodes(connector); 
            }
            return true;
        }

        public async Task CreateSimpleMazeAnimation()
        {
            if (ed.CheckIsProcessing())
                return;

            ed.SetProcessing(true);
            DateTime startTime = DateTime.Now;
            Reset();
            ClearCheckedNodes();
            MazeNode startNode = nodes.SelectRandom();
            if (startNode == null) return;

            List<MazeNode> connector = new List<MazeNode>() { startNode };
            while (connector.Count != nodes.Length)
            {
                MazeNode node = SelectFreeNodeFromConnector(connector);
                MazeNode nodeTo = node.GetFreeAdjacentNodes(connector).SelectRandom();
                AddConnection(node, nodeTo);
                connector.Add(nodeTo);
                await ed.WaitForNextCycle();
            }

            TimeSpan timeSpan = DateTime.Now - startTime;
            ed.methods.createMazeTimeText.Text = $"Time:\n{Math.Round(timeSpan.TotalSeconds, 3)}s";
            ed.SetProcessing(false);
        }

        public async Task CreateComplexMazeAnimation()
        {
            if (ed.CheckIsProcessing())
                return;

            ed.SetProcessing(true);
            DateTime startTime = DateTime.Now;
            Reset();
            ClearCheckedNodes();
            MazeNode startNode = nodes.SelectRandom();
            if (startNode == null) return;

            List<MazeNode> connector = new List<MazeNode>() { startNode };
            MazeNode from = startNode;

            while (await ContinueRandomConnection(from, connector))
            {
                from = SelectFreeNodeFromConnector(connector);
            }

            TimeSpan timeSpan = DateTime.Now - startTime;
            ed.methods.createMazeTimeText.Text = $"Time:\n{Math.Round(timeSpan.TotalSeconds, 3)}s";
            ed.SetProcessing(false);
        }

        #endregion

        #region MazeSolvers

        void BackTrackDFS(MazeNode current, MazeNode to, bool highlight = true)
        {
            List<MazeNode> visited = new List<MazeNode>() { current };
            Stack<MazeNode> nextToVisit = new Stack<MazeNode>();

            while (current != to)
            {
                foreach (MazeNode node in current.nodeConnections)
                {
                    if (!visited.Contains(node))
                        nextToVisit.Push(node);
                }
                MazeNode next = nextToVisit.Pop();
                MazeNode toConnect = current;
                if (!next.IsConnected(current))
                    toConnect = (MazeNode)next.nodeConnections.Find(n => visited.Contains(n));

                Arc redundantArc = Arc.GetConnectingArc(next, toConnect);
                if (highlight)
                    redundantArc.ChangeColour(Brushes.Firebrick);
                redundantArc.ChangeZIndex(50);
                visited.Add(current);
                current = next;
            }
        }

        public async Task DFS()
        {
            if (ed.CheckIsProcessing())
                return;

            ResetAnnotations();
            if (startNode == null || endNode == null)
            {
                Editor.ShowError("The Start and End nodes have not been picked yet. " +
                    "Please generate a random maze with the corresponding button, " +
                    "and then click on two points for the start (green) and end (red) " +
                    "points before trying to perform Depth First Search on it.");
                return;
            }

            ed.SetProcessing(true);
            DateTime startTime = DateTime.Now;

            List<MazeNode> visited = new List<MazeNode>() { startNode };
            Stack<MazeNode> nextToVisit = new Stack<MazeNode>();
            MazeNode current = startNode;

            while (current != endNode)
            {
                foreach (MazeNode node in current.nodeConnections.Where(n => !visited.Contains(n)))
                    nextToVisit.Push(node);

                MazeNode next = nextToVisit.Pop();
                MazeNode toConnect = current;

                // If the next node is not adjacent to the current one (if current is a dead end node)
                if (!next.IsConnected(current))
                {
                    toConnect = (MazeNode)next.nodeConnections.Find(n => visited.Contains(n));
                    BackTrackDFS(current, toConnect);
                }
                Arc.GetConnectingArc(next, toConnect).ChangeColour(Brushes.LawnGreen);

                current = next;
                await ed.WaitForNextCycle();
                visited.Add(current);
            }

            ed.SetProcessing(false);
            TimeSpan timeSpan = DateTime.Now - startTime;
            ed.methods.DFS_TextBlock.Text = $"Time: {Math.Round(timeSpan.TotalSeconds, 3)}s\nVisited Nodes: {visited.Count}";
        }

        public async Task BFS()
        {
            if (ed.CheckIsProcessing())
                return;

            ResetAnnotations();
            if (startNode == null || endNode == null)
            {
                Editor.ShowError("The Start and End nodes have not been picked yet. " +
                    "Please generate a random maze with the corresponding button, " +
                    "and then click on two points for the start (green) and end (red) " +
                    "points before trying to perform Breadth First Search on it.");
                return;
            }
            DateTime startTime = DateTime.Now;
            ed.SetProcessing(true);

            List<MazeNode> visited = new List<MazeNode>() { startNode };
            Queue<MazeNode> nextToVisit = new Queue<MazeNode>();
            MazeNode current = startNode;

            while (current != endNode)
            {
                foreach (MazeNode node in current.nodeConnections.Where(n => !visited.Contains(n)))
                    nextToVisit.Enqueue(node);

                MazeNode next = nextToVisit.Dequeue();
                MazeNode toConnect = (MazeNode)next.nodeConnections.Find(n => visited.Contains(n));
                Arc.GetConnectingArc(next, toConnect).ChangeColour(Brushes.Maroon);
                current = next;
                await ed.WaitForNextCycle();
                visited.Add(current);
            }

            ed.SetProcessing(false);
            TimeSpan timeSpan = DateTime.Now - startTime;
            ed.methods.BFS_TextBlock.Text = $"Time: {Math.Round(timeSpan.TotalSeconds, 3)}s\nVisited Nodes: {visited.Count}";

            // BACKTRACKING TO FIND ROUTE. 
            // How it works is it goes through the visited list reversed, starting from end node.
            // It will loop through until it finds a node connected to endnode, then set that node to current
            // And the cycle repeats.
            visited.Reverse();
            current = visited.First();  // End node
            List<Node> backtrackedNodes = new List<Node>() { current };
            foreach (MazeNode node in visited)
            {
                bool notInBacktrackNodes = !backtrackedNodes.Contains(node);
                if (notInBacktrackNodes && current.IsConnected(node))
                {
                    Arc arc = Arc.GetConnectingArc(current, node);
                    current = node;
                    arc.ChangeColour(Brushes.LawnGreen);
                    arc.ChangeZIndex(100);
                }
                if (notInBacktrackNodes)
                    backtrackedNodes.Add(node);
            }
        }

        public async Task A_star()
        {
            // G_cost = distance of node from starting node
            // H_cost = distance of node from end node
            // F_cost = sum of G_cost and H_cost

            if (ed.CheckIsProcessing())
                return;

            ResetAnnotations();
            if (startNode == null || endNode == null)
            {
                Editor.ShowError("The Start and End nodes have not been picked yet. " +
                    "Please generate a random maze with the corresponding button, " +
                    "and then click on two points for the start (green) and end (red) " +
                    "points before trying to perform the A Star Algorithm on it.");
                return;
            }
            ed.SetProcessing(true);
            DateTime startTime = DateTime.Now;

            List<MazeNode> visited = new List<MazeNode>() { startNode };
            List<MazeNode> toExamine = new List<MazeNode>();

            MazeNode current = startNode;

            while (current != endNode)
            {
                foreach (MazeNode con in current.nodeConnections.Where(n => !visited.Contains(n)).Cast<MazeNode>())
                {
                    float G_cost = con.relativePos.Distance(startNode.relativePos);
                    float H_cost = con.relativePos.Distance(endNode.relativePos);
                    float F_cost = G_cost + H_cost;
                    bool addToExamineList = !toExamine.Contains(con);
                    if (F_cost < con.F_cost || addToExamineList)
                    {
                        con.F_cost = F_cost;
                        con.H_cost = H_cost;
                        con.G_cost = G_cost;

                        con.previous = current;
                        if (addToExamineList)
                        {
                            toExamine.Add(con);
                            await ed.WaitForNextCycle();
                            Arc.GetConnectingArc(current, con).ChangeColour(Brushes.Crimson);
                        }
                    }
                }
                current = toExamine.MinBy(n => n.F_cost);
                IEnumerable<MazeNode> sameFcostNodes = toExamine.Where(n => n.F_cost == current.F_cost);
                if (sameFcostNodes.Count() > 1)
                    current = toExamine.MinBy(n => n.H_cost);
              
                toExamine.Remove(current);
                visited.Add(current);
            }
            TimeSpan timeSpan = DateTime.Now - startTime;
            ed.methods.Astar_TextBlock.Text = $"Time: {Math.Round(timeSpan.TotalSeconds, 3)}s\nVisited Nodes: {visited.Count}";

            current = endNode;
            // Back-Tracing
            while ( current != startNode ) 
            {
                Arc.GetConnectingArc(current, current.previous).ChangeColour(Brushes.LawnGreen);
                current = current.previous;
            }
            ed.SetProcessing(false);
        }

        #endregion
    }
}
