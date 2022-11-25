using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace Algoraph.Scripts
{
    internal class Grapher : GrapherBase
    {
        public Grapher(Editor ed): base(ed)
        {

        }


        bool IsTree()
        {
            return this.nodes.Count + 1 == this.arcs.Count;
        }
        

        #region Spanning Tree: Prim's + Kruskal's
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

        private async Task<bool> IsSpannedTreeHighlightRedundant(List<Node> connector, SelectedArcs selectedArcs, List<Arc> usedArcs)
        {
            foreach (Node n in this.nodes)
            {
                if (!connector.Contains(n))
                    return false;
            }

            foreach(Arc arc in usedArcs) 
            {
                selectedArcs.AddItem(arc);
                await Task.Delay(2000);
            }

            return true;
        }

        public async Task<bool> Prims(List<Node> connector, SelectedArcs selectedArcs, List<Arc>? usedArcs = null)
        {
            usedArcs ??= new List<Arc>();
            if (await IsSpannedTreeHighlightRedundant(connector, selectedArcs, usedArcs)) return true;

            List<Arc> outBoundArcs = GetOutBoundArcs(connector);
            if (outBoundArcs.Count == 0) return false;
            uint[] arcWeights = outBoundArcs.Select(a => a.weight).ToArray();
            Arc minArc = outBoundArcs[Array.IndexOf(arcWeights, arcWeights.Min())];
            usedArcs.Add(minArc);
            Node[] c = minArc.connections;
            Node nodeToAdd = connector.Contains(c[0]) ? c[1] : c[0];
            connector.Add(nodeToAdd);

            return await Prims(connector, selectedArcs, usedArcs);
        }

        #endregion

        #region Dijkstra’s Algorithm

        private void BackTrackTrace(Node startNode, Node currentNode, Node[] previous, SelectedArcs selectedArcs)
        {
            if (currentNode == startNode) return;
            Arc? nextArc = currentNode.arcConnections.Find(a
                => a.GetConnectedNode(currentNode) == previous[this.nodes.IndexOf(currentNode)]);
            selectedArcs.AddItem(nextArc);
            if (nextArc == null) return;
            BackTrackTrace(startNode, nextArc.GetConnectedNode(currentNode), previous, selectedArcs);
        }

        public Node[]? DijkstrasInfo(Node target, out uint[] weighting, Node? endNodeTrace = null, SelectedArcs? selectedArcs = null)
        {
            int nodeCount = this.nodes.Count;

            // Initialising all the empty arrays/lists for Dijkstras..
            uint[] weightsFromTarget = new uint[nodeCount];
            Array.Fill(weightsFromTarget, uint.MaxValue);
            Node[] previous = new Node[nodeCount];
            List<int> unvisitedIndexes = Enumerable.Range(0, this.nodes.Count).ToList();
            
            // Setting the target node index for the shortest path array to 0 (rest will be infinity [uint.MaxValue])
            int targetIndex = this.nodes.IndexOf(target);
            weightsFromTarget[targetIndex] = 0;

            Node[]? dkInfo = DijkstrasRecursive(target, weightsFromTarget, previous, unvisitedIndexes, out weighting);
            if (endNodeTrace != null && selectedArcs != null)
            {
                BackTrackTrace(target, endNodeTrace, previous, selectedArcs);
            }
            return dkInfo;
        }

        private Node[]? DijkstrasRecursive(Node current, uint[] weightsFromTarget, Node[] previous, List<int> unvisitedIndexes, out uint[]? weighting)
        {
            if (unvisitedIndexes.Count == 0)
            {
                weighting = weightsFromTarget;
                return previous;
            }


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

            if (!unvisitedIndexes.Remove(currentIndex))
            {
                weighting = null;
                return null;
            }


            uint[] weightsIgnoreVisited = weightsFromTarget.Select(
                w => unvisitedIndexes.Contains(Array.IndexOf(weightsFromTarget, w))? w: uint.MaxValue).ToArray();

            int minWeightUnvisitedIndex = Array.IndexOf(weightsIgnoreVisited, weightsIgnoreVisited.Min());

            Node nextTargetNode = this.nodes[minWeightUnvisitedIndex];
            return DijkstrasRecursive(nextTargetNode, weightsFromTarget, previous, unvisitedIndexes, out weighting);
        }

        #endregion
    }
}
