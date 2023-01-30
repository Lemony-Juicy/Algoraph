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

        public void SaveState()
        {
            Saver.Save(this.nodes, this.arcs);
        }


        public bool LoadState() 
        {
            GraphStateData? data;
            NodesArcsTuple tuple;
            try // Try to load the data. If not possible, Let user know that this file is not supported
            {
                data = Saver.Load();
                if (data == null) return false;
                tuple = GraphStateDataMethods.GetNodeArcTuple(ed, data);
            }
            catch { return false; }

            Node[] nodes = tuple.nodes;
            Arc[] arcs = tuple.arcs;    

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
                await Task.Delay(2000);
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

        #endregion

        #region Dijkstra’s Algorithm

        /// <summary>
        /// Traces path by selecting the arcs in the editor from the Dijkstra's data
        /// </summary>
        /// <param name="startNode"></param>
        /// <param name="currentNode"></param>
        /// <param name="previous"></param>
        /// <param name="selectedArcs"></param>
        private void BackTrackTrace(Node startNode, Node currentNode, Node[] previous, SelectedArcs selectedArcs)
        {
            if (currentNode == startNode) return;
            Arc? nextArc = currentNode.arcConnections.Find(a
                => a.GetConnectedNode(currentNode) == previous[this.nodes.IndexOf(currentNode)]);
            selectedArcs.AddItem(nextArc);
            if (nextArc == null) return;
            BackTrackTrace(startNode, nextArc.GetConnectedNode(currentNode), previous, selectedArcs);
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
        public Node[]? DijkstrasInfo(Node startNode, out uint[] weighting, Node? endNode = null, SelectedArcs? selectedArcs = null)
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
            if (endNode != null && selectedArcs != null)
                BackTrackTrace(startNode, endNode, previous, selectedArcs);
            
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
    }
}
