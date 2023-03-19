using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls.Primitives;
using Algoraph.Scripts.Maze_Scripts;

namespace Algoraph.Scripts
{
    internal class Node
    {
        public static float radius = 15f;

        public List<Node> nodeConnections { get; private set; }
        public List<Arc> arcConnections { get; private set; }

        public string name { get; private set; } = "";
        private Point location;
        public ToggleButton nodeButton { get; private set; }


        public Node(Editor ed, Point location, string name = "", List<Node>? connections = null)
        {
            this.nodeConnections = connections ?? new List<Node>();
            this.arcConnections = new List<Arc>();

            object s = IsMazeNode()? ed.FindResource("mazeNodeUI"): ed.FindResource("nodeUI");

            // This is the UI object that will be displayed on the screen
            this.nodeButton = new ToggleButton()
            {
                Style = (Style)s,
                Background = (LinearGradientBrush)ed.FindResource("OrangeGradient"),
                // MinWidth has been binded to be the actual width for the ellipse
                MinWidth = Node.radius * 2,
                Tag = this
            };

            this.ChangeName(name);

            SetLocation(location.X, location.Y);

            // Ensures the button events are only enabled if the Node is not a MazeNode
            // This is to prevent the user deleting maze nodes
            if (!IsMazeNode())
            {
                this.nodeButton.Checked += ed.Node_Checked;
                this.nodeButton.Unchecked += ed.Node_Unchecked;
                this.nodeButton.MouseEnter += ed.Node_Enter;
                this.nodeButton.MouseLeave += ed.Node_Leave;
            }
            
            // Setting the Z layer for this object to be rendered to be high,
            // so it renders above the arc
            Canvas.SetZIndex(this.nodeButton, 500);
        }

        public bool IsMazeNode()
        {
            return this.GetType() == typeof(MazeNode);
        }

        public Point GetLocation()
        {
            return Point.Add(location, new Vector(nodeButton.Width * 0.5, nodeButton.Height * 0.5));
        }

        public void Uncheck()
        {
            this.nodeButton.IsChecked = false;
        }

        public void AddToCanvas(Canvas canvas)
        {
            canvas.Children.Add(nodeButton);
        }

        public void RemoveFromCanvas(Canvas canvas)
        {
            if (!canvas.Children.Contains(nodeButton)) return;
            canvas.Children.Remove(nodeButton);
        }

        bool AddConnection(Node node, Arc arc)
        {
            if (!nodeConnections.Contains(node))
            {
                nodeConnections.Add(node);
                node.nodeConnections.Add(this);
            }

            if (!arcConnections.Contains(arc))
            {
                arcConnections.Add(arc);
                node.arcConnections.Add(arc);
                return true;
            }
            return false;
        }

        bool RemoveConnection(Node node, Arc arc)
        {
            bool removedNode = nodeConnections.Remove(node);
            return arcConnections.Remove(arc) && removedNode;
        }

        public void SetLocation(double X, double Y)
        {
            X -= nodeButton.Width*0.5;
            Y -= nodeButton.Height*0.5;

            this.location = new Point(X, Y);
            Canvas.SetLeft(nodeButton, X);
            Canvas.SetTop(nodeButton, Y);
        }

        public bool ChangeName(string newName)
        {
            this.name = newName;
            this.nodeButton.Content = newName;
            return true;
        }

        public static bool ConnectNodes(Node n1, Node n2, Arc arc)
        {
            return n1.AddConnection(n2, arc) && n2.AddConnection(n1, arc);
        }

        public static bool DisconnectNodes(Node n1, Node n2, Arc arc)
        {
            return n1.RemoveConnection(n2, arc) && n2.RemoveConnection(n1, arc);
        }

        public static string GetNextName(IEnumerable<Node> nodes)
        {
            string[] names = nodes.Select(n => n.name).ToArray();
            string currentName = "N1";

            while (names.Contains(currentName))
            {
                string currentCount = currentName[1..].ToString();
                int num = Convert.ToInt16(currentCount) + 1;
                currentName = "N" + num.ToString();
            }
            return currentName;
        }
    }
}
