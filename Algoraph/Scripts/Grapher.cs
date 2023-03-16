using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Algoraph.Scripts
{
    internal class Grapher : GrapherBase
    {
        public Grapher(Editor ed): base(ed)
        {

        }

        #region Saving and Loading

        public void SaveState(string path)
        {
            string jsonString = Saver.GetJsonData(this.nodes, this.arcs, header_item: Saver.HEADER_ITEM);
            Saver.SaveJsonData(jsonString, path);
        }


        public bool LoadState(string? path = null, string? jsonString = null) 
        {
            Node[]? nodes; 
            Arc[]? arcs;

            

            // Try to load the data. If not possible, Let user know that this file is not supported
            try
            {
                if (jsonString == null && path != null)
                    jsonString = Saver.LoadJsonData(path);             

                if (!Saver.LoadNodesArcs(ed, jsonString, out arcs, out nodes))
                    return false;
            }
            catch { return false; }  

            this.nodes.AddRange(nodes);
            this.arcs.AddRange(arcs);

            foreach(Node n in nodes)
                n.AddToCanvas(ed.mainCanvas);

            foreach(Arc a in arcs)
                a.AddToCanvas(ed.mainCanvas);   

            return true;
        }

        #endregion

        #region Useful Tools

        public bool IsFullyConnected()
        {
            List<Node> connector = new List<Node>() { this.nodes[0] };
            while (connector.Count != this.nodes.Count)
            {
                var outBoundArcs = GetOutBoundArcs(connector);

                if (outBoundArcs.Count == 0) return false;

                var c = outBoundArcs[0].connections;

                Node nodeToAdd = connector.Contains(c[0]) ? c[1] : c[0];
                connector.Add(nodeToAdd);
            }
            return true;
        }

        public bool IsTree()
        {
            return this.nodes.Count + 1 == this.arcs.Count;
        }


        #endregion

        #region Spanning Trees
        private List<Arc> GetOutBoundArcs(List<Node> connector)
        {
            List<Arc> result = new();
            foreach (Node node in connector)
            {
                foreach (Arc arc in node.arcConnections)
                {
                    // check bool to ensure the arc is filtered so that it is only added to the
                    // connector arcs IF ARC DOESN'T CONNECT BACK TO CONNECTOR (PREVENTS CYCLES)
                    Node? connectingNode = arc.GetConnectedNode(node);
                    if (!result.Contains(arc) && !connector.Contains(connectingNode))
                        result.Add(arc);
                }
            }
            return result;
        }

        /// <summary>
        /// Checks whether the current tree state is fully done.
        /// </summary>
        /// <param name="connector">Nodes in the tree</param>
        /// <param name="selectedArcs">The SelectedArcs class, so if fully spanned can be added to, and highlighted</param>
        /// <param name="usedArcs">Arcs that have already been visited by the algorithm</param>
        /// <returns></returns>
        private async Task<bool> IsFullySpannedTree(List<Node> connector, SelectedArcs selectedArcs, List<Arc> usedArcs)
        {
            if (connector.Count != this.nodes.Count) return false;

            foreach(Arc arc in usedArcs) 
            {
                selectedArcs.AddItem(arc);
                await ed.WaitForNextCycle();
            }
            return true;
        }

        public async Task Prims(List<Node> connector, SelectedArcs selectedArcs, List<Arc>? usedArcs = null)
        {
            usedArcs ??= new List<Arc>();
            if (await IsFullySpannedTree(connector, selectedArcs, usedArcs)) 
                return;

            List<Arc> outBoundArcs = GetOutBoundArcs(connector);
            uint[] arcWeights = outBoundArcs.Select(a => a.weight).ToArray();

            Arc minArc = outBoundArcs[Array.IndexOf(arcWeights, arcWeights.Min())];
            usedArcs.Add(minArc);
            Node[] c = minArc.connections;
            Node nodeToAdd = connector.Contains(c[0]) ? c[1] : c[0];
            connector.Add(nodeToAdd);

            await Prims(connector, selectedArcs, usedArcs);
        }

        public async Task Kruskals(SelectedArcs selectedArcs)
        {
            // Essentially finding the minimum arc, and then performing prim's algorithm on it.
            uint minArcWeight = this.arcs.Min(a => a.weight);
            Arc minArc = this.arcs.Find(a => a.weight == minArcWeight);

            await Prims(minArc.connections.ToList(), selectedArcs, new List<Arc> { minArc});
        }

        #endregion

        #region Dijkstra’s Algorithm

        /// <summary>
        /// Traces path by selecting the arcs in the editor from the Dijkstra's data
        /// </summary>
        /// <param name="startNode"></param>
        /// <param name="currentNode"></param>
        /// <param name="previous"></param>
        /// <param name="selectedArcs"></param>
        public async Task BackTrackTrace(Node startNode, Node currentNode, Node[] previous, SelectedArcs selectedArcs)
        {
            if (currentNode == startNode) return;
            Arc? nextArc = currentNode.arcConnections.Find(a
                => a.GetConnectedNode(currentNode) == previous[this.nodes.IndexOf(currentNode)]);
            selectedArcs.AddItem(nextArc);
            if (nextArc == null) return;
            await ed.WaitForNextCycle();
            await BackTrackTrace(startNode, nextArc.GetConnectedNode(currentNode), previous, selectedArcs);
        }


        /// <summary>
        /// Returns the previous nodes for the current node in order of index. Eg: at index 5: Node N2
        /// It will give the shortest path of all nodes from a starting node in the weighting array.
        /// </summary>
        /// <param name="startNode">The start node to being from</param>
        /// <param name="weighting">A list for which to put the weightings in</param>
        /// <param name="endNode">The end node to backtrace. If this is null, no backtracing will take place.</param>
        /// <param name="selectedArcs">The selectedarcs object so arcs can be selected if backtracing in the editor</param>
        /// <returns></returns>
        public Node[]? DijkstrasInfo(Node startNode, out uint[] weighting)
        {
            int nodeCount = this.nodes.Count;

            // Initialising all the empty arrays/lists for Dijkstras..
            uint[] weightsFromTarget = new uint[nodeCount];
            Array.Fill(weightsFromTarget, uint.MaxValue);
            Node[] previous = new Node[nodeCount];
            List<int> unvisitedIndexes = Enumerable.Range(0, this.nodes.Count).ToList();
            
            // Setting the target node index for the shortest path array to 0 (rest will be infinity [uint.MaxValue])
            int targetIndex = this.nodes.IndexOf(startNode);
            weightsFromTarget[targetIndex] = 0;

            Node[]? dkInfo = DijkstrasRecursive(startNode, weightsFromTarget, previous, unvisitedIndexes);
            weighting = weightsFromTarget;
            return dkInfo;
        }

        private Node[]? DijkstrasRecursive(Node current, uint[] weightsFromTarget, Node[] previous, List<int> unvisitedIndexes)
        {
            // First base case of recursion
            if (unvisitedIndexes.Count == 0) return previous; 

            int currentIndex = this.nodes.IndexOf(current);
            uint currentWeight = weightsFromTarget[currentIndex];
            foreach (Arc connArc in current.arcConnections)
            {
                Node connNode = connArc.GetConnectedNode(current);
                int connIndex = this.nodes.IndexOf(connNode);
                uint total = connArc.weight + currentWeight;
                if (total < weightsFromTarget[connIndex])
                {
                    weightsFromTarget[connIndex] = total;
                    previous[connIndex] = current;
                }
            }

            // If removing current index from unvisited indices did not happen, unvisited indices is empty
            // So this is the second base case for the recursion
            if (!unvisitedIndexes.Remove(currentIndex)) return previous;

            // Holds all the weights that haven't been visited (the one's that have been visited are set to infinity)
            uint[] weightsIgnoreVisited = weightsFromTarget.Select(
                w => unvisitedIndexes.Contains(Array.IndexOf(weightsFromTarget, w))? w: uint.MaxValue).ToArray();

            int minWeightUnvisitedIndex = Array.IndexOf(weightsIgnoreVisited, weightsIgnoreVisited.Min());

            Node nextTargetNode = this.nodes[minWeightUnvisitedIndex];
            return DijkstrasRecursive(nextTargetNode, weightsFromTarget, previous, unvisitedIndexes);
        }

        #endregion

        #region Route Inspection

        Node[] GetOddNodes()
        {
            return nodes.Where(node => node.nodeConnections.Count % 2 == 1).ToArray();
        }

        public async Task<uint> RouteInspection(SelectedArcs selectedArcs)
        {
            uint totalWeight = 0;
            foreach(Arc a in this.arcs)
                totalWeight += a.weight;

            Node[] n = GetOddNodes();
            
            if (n.Length == 0)  // Eulerian
            {
                return totalWeight;
            }
            if (n.Length == 2)  // Semi Eulerian
            {
                Node[] previous = DijkstrasInfo(n[0], out uint[] weighting);
                await BackTrackTrace(n[0], n[1], previous, selectedArcs);
                return totalWeight + weighting[this.nodes.IndexOf(n[1])];
            }
            if (n.Length == 4) // Not Eulerian
            {
                Node[] previous1 = DijkstrasInfo(n[0], out uint[] a);
                uint ab = a[1];
                uint ac = a[2];
                uint ad = a[3];
                Node[] previous2 = DijkstrasInfo(n[1], out uint[] b);
                uint bc = b[2];
                uint bd = b[3];
                Node[] previous3 = DijkstrasInfo(n[2], out uint[] c);
                uint cd = c[3];
                uint[][] pairings = new uint[][] 
                { 
                    new uint[2] {ab, cd},
                    new uint[2] {ad, bc},
                    new uint[2] {ac, bd},
                };

                Node[][] previouses = new Node[][]
                {
                    previous1, previous3, previous1, previous2, previous1, previous2
                };

                Node[][] node_pairings = new Node[][]
                {
                    new Node[] {n[0], n[1]},
                    new Node[] {n[2], n[3]},

                    new Node[] {n[0], n[3]},
                    new Node[] {n[1], n[2]},

                    new Node[] {n[0], n[2]},
                    new Node[] {n[1], n[3]},
                };
                uint[] minWeight = pairings.MinBy(x => x[0] + x[1]);
                int index = (Array.IndexOf(pairings, minWeight)+1)*2-1;
                Node[] np1 = node_pairings[index];
                await BackTrackTrace(np1[0], np1[1], previouses[index], selectedArcs);
                Node[] np2 = node_pairings[index-1];
                await BackTrackTrace(np2[0], np2[1], previouses[index-1], selectedArcs);
                return minWeight[0] + minWeight[1] + totalWeight;
            }
            return 0;
        }

        #endregion
    }
}
