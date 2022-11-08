using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls.Primitives;

namespace Algoraph.Scripts
{
    // THE ELLIPSE HAS ITS LOCATION BASED ON ITS TOP LEFT CORNER (RECT)!!
    internal class Node
    {
        private static string currentName = "N1";
        public static float radius = 12.5f;

        public List<Node> nodeConnections { get; private set; }
        public List<Arc> arcConnections { get; private set; }
        public string name { get; private set; }
        private Point location;
        public ToggleButton nodeButton { get; private set; }

        public Node(Editor editor, Point location, List<Node>? connections = null)
        {
            this.nodeConnections = connections ?? new List<Node>();
            this.arcConnections = new List<Arc>();

            object s = Application.Current.FindResource("nodeUI");
            this.nodeButton = new ToggleButton()
            {
                Style = (Style)s,
                Background = (LinearGradientBrush)editor.FindResource("OrangeGradient"),
                MinWidth = Node.radius * 2
            };

            SetLocation(location.X, location.Y);
            this.name = currentName;
            this.nodeButton.Name = currentName;

            this.nodeButton.Checked += editor.Node_Checked;
            this.nodeButton.Unchecked += editor.Node_Unchecked;
            this.nodeButton.MouseEnter += editor.Node_Enter;
            this.nodeButton.MouseLeave += editor.Node_Leave;

            IncrementName();
            Canvas.SetZIndex(this.nodeButton, 500);
        }

        public Point GetLocation()
        {
            return Point.Add(location, new Vector(50-radius, 50-radius));
        }

        public void Uncheck()
        {
            this.nodeButton.IsChecked = false;
        }

        public void AddToCanvas(Canvas canvas)
        {
            //if (canvas.Children.Contains(nodeButton)) return;
            canvas.Children.Add(nodeButton);
        }

        public void RemoveFromCanvas(Canvas canvas)
        {
            if (!canvas.Children.Contains(nodeButton)) return;
            canvas.Children.Remove(nodeButton);
        }

        public void AddConnection(Node node, Arc arc)
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
            }

        }

        public void RemoveConnection(Node node, Arc arc)
        {
            if (nodeConnections.Contains(node))
            {
                nodeConnections.Remove(node);
                node.nodeConnections.Remove(this);
            }

            if (arcConnections.Contains(arc))
            {
                arcConnections.Remove(arc);
                node.arcConnections.Remove(arc);
            }
        }

        public void ClearConnections()
        {
            nodeConnections.Clear();
            arcConnections.Clear();
        }

        public void SetLocation(double X, double Y)
        {

            this.location = new Point(X-50, Y-50);
            Canvas.SetLeft(nodeButton, X-50);
            Canvas.SetTop(nodeButton, Y-50);
        }


        public string GetNodeConnectionNames()
        {
            if (nodeConnections.Count == 0) return "None";
            StringBuilder names = new StringBuilder();
            names.Append("[");
            for (int i = 0; i < nodeConnections.Count - 1; i++)
            {
                names.Append(nodeConnections[i].name + ", ");
            }
            names.Append(nodeConnections.Last().name);
            names.Append("]");
            return names.ToString();
        }

        public void ChangeName(string newName)
        {
            this.name = newName;
            this.nodeButton.Name = newName;
        }


        public static bool CheckNameInFormat(string name)
        {
            int result;
            int.TryParse(name.Substring(1), out result);
            return name.First().ToString() == "N" && result != 0;
        }

        static void IncrementName()
        {
            string currentCount = currentName.Substring(1).ToString();
            int num = Convert.ToInt16(currentCount) + 1;
            currentName = "N" + num.ToString();
        }

        public static void DecrementName()
        {
            string currentCount = currentName.Substring(1).ToString();
            int num = Convert.ToInt16(currentCount) - 1;
            currentName = "N" + num.ToString();
        }

        public static void ResetCurrentName()
        {
            currentName = "N1";
        }
    }
}
