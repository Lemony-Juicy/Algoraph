using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Xml.Linq;

namespace Algoraph.Scripts
{
    internal class GrapherBase
    {
        public List<Node> nodes { get; private set; }
        public List<Arc> arcs { get; private set; }


        protected Editor ed;

        public NodeInfoTable[] GetNodeInfo()
        {
            return nodes.Select(n => new NodeInfoTable()
            {
                Node = n.name,
                Adjacencies = n.nodeConnections.Stringify()
            }).ToArray() ;
        }

        #region Constructor + Presets
        public GrapherBase(Editor ed, List<Node>? nodes = null)
        {
            // if nodes is null, add new empty List, else use the nodes list.
            this.nodes = nodes ?? new List<Node>();
            this.ed = ed;
            this.arcs = new List<Arc>();
        }

        public static Vector AngleToPoint(double angle, double size)
        {
            double X = Math.Cos(angle);
            double Y = Math.Sin(angle);
            return new Vector(X, Y) * size;
        }

        public void Connect(Node node1, Node node2, uint weight = 1) // Connecting Two Nodes
        {
            if (node1 == node2) return;
            if (node1.nodeConnections.Contains(node2))
                Disconnect(node1, node2);
            Arc arc = new(node1, node2, weight, ed);
            arcs.Add(arc);
            arc.AddToCanvas(ed.mainCanvas, !ed.methods.hideWeightsCheckBox.IsChecked);
            Node.ConnectNodes(node1, node2, arc);
        }

        public void Disconnect(Node node1, Node node2) // Connecting Two Nodes
        {
            Arc? arc = Arc.GetConnectingArc(node1, node2);
            Node.DisconnectNodes(node1, node2, arc);
            RemoveArc(arc);
        }

        public void CompleteGraphArcs()
        {
            /* This generates arcs in the graph so that every node is connected to every other node */
            ClearArcs();

            for (int i = 0; i < nodes.Count; i++)
            {
                for (int j = 0; j < nodes.Count; j++)
                {
                    // The (i-1) != j bit is to prevent arcs from the two point repeating
                    // EG: node A and B and B and A.
                    if (i != j && (i - 1) != j)
                        Connect(nodes[i], nodes[j]);
                }
            }
        }

        List<int> GetRemainingNodeIndices(List<int> used)
        {
            List<int> remaining = new List<int>();
            for (int i = 0; i < nodes.Count; i++)
            {
                if (!used.Contains(i))
                    remaining.Add(i);
            }
            return remaining;
        }

        public void CreateRandomTree()
        {
            if (nodes.Count == 0) return;
            ClearArcs();

            List<int> used = new List<int>();
            for (int i = 0; i < nodes.Count - 1; i++)
            {
                if (!used.Contains(i))
                {
                    used.Add(i);
                    List<int> remaining = GetRemainingNodeIndices(used);
                    //if (remaining.Count == 0) return;
                    Node nodeToConnect = nodes[remaining.SelectRandom()];
                    Connect(nodes[i], nodeToConnect, (uint)CustomExtentions.random.Next(1, 100));
                }
            }
        }

        public void RandomConnections()
        {
            /* This generates random arcs in the graph */
            CreateRandomTree(); 

            for (int i = 0; i < nodes.Count; i++)
            {
                for (int j = 0; j < nodes.Count; j++)
                {
                    // The (i-1) != j bit is to prevent arcs from the two point repeating
                    // EG: node A and B and B and A.
                    if (i != j && (i - 1) != j && CustomExtentions.random.Next(1, 7) == 2 && !nodes[i].nodeConnections.Contains(nodes[j]))
                        Connect(nodes[i], nodes[j], (uint)CustomExtentions.random.Next(1, 100));
                }
            }
        }

        public void RegularNodes(int degree, float size, Point startAt, double offsetAngle = 0)
        {
            double angle = Math.Tau / degree;
            for (int i = 1; i <= degree; i++)
            {
                Point pos = Point.Add(startAt, AngleToPoint(angle * i + offsetAngle, size));
                if (!AddNode(pos))
                    return;
            }
        }
        #endregion


        #region Removing/Adding Nodes

        public bool AddNode(Point pos)
        {
            if (nodes.Count >= 30)
            {
                Editor.ShowError("Maximum amount of nodes allowed is 30, So you cannot create anymore nodes.");
                return false;
            }
            Node newNode = new Node(ed, pos, name: Node.GetNextName(nodes));
            nodes.Add(newNode);
            newNode.AddToCanvas(ed.mainCanvas);
            return true;
        }

        public void RemoveNode(Node node)
        {
            node.RemoveFromCanvas(ed.mainCanvas);
            foreach (Arc arc in node.arcConnections.ToArray())
            {
                Node nodeConnected = arc.GetConnectedNode(node);
                Node.DisconnectNodes(nodeConnected, node, arc);
                RemoveArc(arc);
            }
            nodes.Remove(node);
        }

        public void RemoveArc(Arc arc)
        {
            Node.DisconnectNodes(arc.connections[0], arc.connections[1], arc);
            arc.RemoveFromCanvas(ed.mainCanvas);
            arcs.Remove(arc);
        }

        public void ClearArcs()
        {
            foreach (Arc arc in arcs)
            {
                Node.DisconnectNodes(arc.connections[0], arc.connections[1], arc);
                arc.RemoveFromCanvas(ed.mainCanvas);
            }
            arcs.Clear();
        }

        public void ClearNodes()
        {
            foreach (Node node in nodes)
                node.RemoveFromCanvas(ed.mainCanvas);
            nodes.Clear();
        }

        #endregion

        public bool ValidNodeName(string name)
        {
            return nodes.Find(n => n.name == name) == null;
        }

        public void ChangeNodeSize(float newRadius)
        {
            float oldRadius = Node.radius;
            foreach (Node node in nodes)
            {
                Point currentPos = node.GetLocation();
                node.SetLocation(oldRadius - newRadius + currentPos.X, oldRadius - newRadius + currentPos.Y);
                node.nodeButton.Width = newRadius * 2;
                node.nodeButton.Height = node.nodeButton.Width;
            }
            Node.radius = newRadius;
        }

        public void MoveNode(Node? node, Point pos)
        {
            if (node == null) return;

            node.SetLocation(pos.X, pos.Y);
            foreach (Arc arc in node.arcConnections)
            {
                arc.UpdateArc(arc.weight);
            }
        }

        public void DisplayArcWeights(bool show = false)
        {
            if (show)
            {
                foreach (Arc arc in arcs)
                    arc.RenderWeights(ed.mainCanvas);
            }
            else
            {
                foreach (Arc arc in arcs)
                    arc.DerenderWeights(ed.mainCanvas);
            }
        }

        public Node? GetClosestNode(Point pos)
        {
            double smallestDistance = double.MaxValue;
            Node? currentBest = null;

            foreach (Node node in nodes)
            {
                double distance = (node.GetLocation() - pos).Length;
                if (distance < smallestDistance)
                {
                    currentBest = node;
                    smallestDistance = distance;
                }
            }
            return currentBest;
        }

        public double GetClosestNodeDistance(Point pos)
        {
            try
            {
                return nodes.Select(n => (n.GetLocation() - pos).Length).Min();
            }
            catch (InvalidOperationException) 
            { 
                return double.MaxValue; 
            }
        }
    }
}
