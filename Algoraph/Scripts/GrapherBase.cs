using System;
using System.Collections.Generic;
using System.Windows;

namespace Algoraph.Scripts
{
    internal class GrapherBase
    {
        public List<Node> nodes { get; private set; }
        public List<Arc> arcs { get; private set; }


        protected Editor ed;

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

        public Node? GetNodeFromName(string name)
        {
            return Array.Find(nodes.ToArray(), (n) => name == n.name);
        }

        public Arc? GetArcFromName(string name)
        {
            return Array.Find(arcs.ToArray(), (n) => name == n.name);
        }

        public void Connect(Node node1, Node node2, uint weight = 1) // Connecting Two Nodes
        {
            if (node1 == node2) return;
            if (node1.nodeConnections.Contains(node2))
                Disconnect(node1, node2);
            Arc arc = new(ed, node1, node2, weight: weight);
            arcs.Add(arc);
            arc.AddToCanvas(ed.mainCanvas);
            node1.AddConnection(node2, arc);
        }

        public void Disconnect(Node node1, Node node2) // Connecting Two Nodes
        {
            Arc? arc = GetConnectingArc(node1, node2);
            node1.RemoveConnection(node2, arc);
            RemoveArc(arc);
        }

        public Arc? GetConnectingArc(Node node1, Node node2)
        {
            foreach (Arc arc1 in node1.arcConnections)
            {
                foreach (Arc arc2 in node2.arcConnections)
                {
                    if (arc1 == arc2) return arc1;
                }
            }
            return null;
        }

        public void CompleteGraphArcs()
        {
            /* This generates arcs in the graph so that every node is connected to every other node */
            if (nodes.Count == 0) return;
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

        public void RandomConnections()
        {
            /* This generates random arcs in the graph */
            if (nodes.Count == 0) return;
            ClearArcs();

            for (int i = 0; i < nodes.Count; i++)
            {
                for (int j = 0; j < nodes.Count; j++)
                {
                    // The (i-1) != j bit is to prevent arcs from the two point repeating
                    // EG: node A and B and B and A.
                    if (i != j && (i - 1) != j && ed.random.Next(1, 5) == 2)
                        Connect(nodes[i], nodes[j], (uint)ed.random.Next(1, 100));
                }
            }
        }

        public void RegularNodes(int degree, float size, Point startAt, double offsetAngle = 0)
        {
            double angle = (Math.PI * 2) / degree;
            for (int i = 1; i <= degree; i++)
            {
                Point pos = Point.Add(startAt, AngleToPoint(angle * i + offsetAngle, size));
                AddNode(pos);
            }
        }
        #endregion


        #region Removing/Adding Nodes

        public void AddNode(Point pos)
        {
            Node newNode = new Node(ed, pos);
            nodes.Add(newNode);
            newNode.AddToCanvas(ed.mainCanvas);
        }

        public void RemoveNode(Node node)
        {
            node.RemoveFromCanvas(ed.mainCanvas);
            Node.DecrementName();
            foreach (Arc arc in node.arcConnections.ToArray())
            {
                Node nodeConnected = arc.GetConnectedNode(node);
                nodeConnected.RemoveConnection(node, arc);
                RemoveArc(arc);
            }
            nodes.Remove(node);
        }

        public void RemoveArc(Arc arc)
        {
            arc.connections[0].RemoveConnection(arc.connections[1], arc);
            arc.RemoveFromCanvas(ed.mainCanvas);
            arcs.Remove(arc);
        }

        public void ClearArcs()
        {
            foreach (Arc arc in arcs)
            {
                arc.connections[0].RemoveConnection(arc.connections[1], arc);
                arc.RemoveFromCanvas(ed.mainCanvas);
            }
            Arc.ResetCurrentName();
            arcs.Clear();
        }

        public void ClearNodes()
        {
            foreach (Node node in nodes)
                node.RemoveFromCanvas(ed.mainCanvas);
            Node.ResetCurrentName();
            nodes.Clear();
        }

        #endregion

        public bool NodeNameExists(string name)
        {
            foreach (Node n in nodes)
            {
                if (n.name == name)
                    return true;
            }
            return Node.CheckNameInFormat(name);
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

        public void ChangeArcThickness(float thickness)
        {
            foreach (Arc arc in arcs)
            {
                arc.ChangeThickness(thickness);
            }
            Arc.thickness = thickness;
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

        public void ToggleArcWeights()
        {
            Arc.displayWeight = !Arc.displayWeight;
            if (Arc.displayWeight)
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
    }
}
