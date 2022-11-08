using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Algoraph.Scripts
{
    internal class Grapher : GrapherBase
    {
        public Grapher(Editor ed) : base(ed)
        {

        }

        private List<Arc> GetArcs(List<Node> connector)
        {
            List<Arc> result = new List<Arc>();
            foreach (Node node in connector)
            {
                foreach (Arc arc in node.arcConnections)
                {
                    Node connectingNode = arc.GetConnectedNode(node);

                    // check bool to ensure the arc is filtered so that it is only added to the
                    // connector arcs IF ARC DOESN'T CONNECT BACK TO CONNECTOR (PREVENTS CYCLES)
                    if (!result.Contains(arc) && !connector.Contains(connectingNode))
                        result.Add(arc);
                }
            }

            return result;
        }

        private bool IsSpannedTree(List<Node> connector)
        {
            foreach (Node n in this.nodes)
            {
                if (!connector.Contains(n))
                    return false;
            }

            return true;
        }

        public bool Prims(List<Node> connector, SelectedArcs selectedArcs, List<Arc>? usedArcs = null)
        {
            usedArcs ??= new List<Arc>();

            if (IsSpannedTree(connector))
            {
                foreach (Arc a in this.arcs)
                {
                    if (!usedArcs.Contains(a))
                    {
                        a.Check();
                        selectedArcs.AddItem(a);
                    }

                }
                return true;
            }

            uint minWeight = uint.MaxValue;
            int minWeighIndex = int.MaxValue;

            List<Arc> arcs = GetArcs(connector);
            for (int i = 0; i < arcs.Count; i++)
            {
                if (arcs[i].weight < minWeight)
                {
                    minWeight = arcs[i].weight;
                    minWeighIndex = i;
                }
            }
            if (minWeighIndex == int.MaxValue)
                return false;

            usedArcs.Add(arcs[minWeighIndex]);

            Node[] c = arcs[minWeighIndex].connections;
            Node nodeToAdd = connector.Contains(c[0]) ? c[1] : c[0];
            connector.Add(nodeToAdd);

            return Prims(connector, selectedArcs, usedArcs);
        }

    }
}
