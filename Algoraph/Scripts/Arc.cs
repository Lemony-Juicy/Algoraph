using System.Windows.Controls;
using System.Windows.Shapes;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace Algoraph.Scripts
{
    internal class Arc
    {
        public Node[] connections = new Node[2];
        public uint weight { get; private set; }

        private ToggleButton arcButton;
        public Label weightLabel { get; private set; }

        public Arc(Node node1, Node node2, uint weight = 1, Editor? ed = null)
        {
            object s = ed == null ? Application.Current.FindResource("mazeArcUI") : ed.FindResource("arcUI");
            this.arcButton = new ToggleButton
            {
                Style = s as Style,
                Tag = this,
            };

            weightLabel = new Label()
            {
                Style = (Style)Application.Current.FindResource("WeightLabel")
            };

            this.arcButton.ApplyTemplate();
            ConnectLine(node1, node2, weight);

            // Check whether this is not in maze mode, then apply events 
            if (ed != null)
            {
                this.arcButton.MouseEnter += ed.Arc_Enter;
                this.arcButton.MouseLeave += ed.Arc_Leave;
                this.arcButton.Checked += ed.Arc_Checked;
                this.arcButton.Unchecked += ed.Arc_Unchecked;
            }

            Canvas.SetZIndex(arcButton, 100);
            Canvas.SetZIndex(weightLabel, 300);
        }

        public void ChangeZIndex(int value)
        {
            Canvas.SetZIndex(arcButton, value);
        }


        public void ChangeStyle(Style style)
        {
            this.arcButton.Style = style;
        }

        private void ConnectLine(Node node1, Node node2, uint weight = 1)
        {
            this.weight = weight;
            connections[0] = node1;
            connections[1] = node2;

            UpdateArc(weight);
        }

        public void Uncheck()
        {
            this.arcButton.IsChecked = false;
        }

        public void Check()
        {
            this.arcButton.IsChecked = true;
        }

        public void ChangeFontSize(float size)
        {
            weightLabel.FontSize = size;
            SetLabelPosition();
        }

        void SetLabelPosition()
        {
            Line arcLine = (Line)arcButton.Template.FindName("arcLine", arcButton);

            double x_raw = (arcLine.X2 - arcLine.X1) * 0.5 + arcLine.X1;
            double y_raw = (arcLine.Y2 - arcLine.Y1) * 0.5 + arcLine.Y1;

            double x = x_raw - weightLabel.ActualWidth * 0.5;
            double y = y_raw - weightLabel.ActualHeight * 0.5;
            Canvas.SetLeft(weightLabel, x);
            Canvas.SetTop(weightLabel, y);
        }

        public void ChangeColour(Brush brush)
        {
            arcButton.Foreground = brush;
        }

        public void RenderWeights(Canvas canvas)
        {
            canvas.Children.Add(weightLabel);
            weightLabel.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            weightLabel.Arrange(new Rect(weightLabel.DesiredSize));
            SetLabelPosition();
        }

        public void DerenderWeights(Canvas canvas)
        {
            canvas.Children.Remove(weightLabel);
        }

        public void AddToCanvas(Canvas canvas, bool? renderWeight = true)
        {
            if (canvas.Children.Contains(arcButton)) return;
            canvas.Children.Add(arcButton);

            if (renderWeight == true)
                RenderWeights(canvas);
        }

        public void RemoveFromCanvas(Canvas canvas)
        {
            if (!canvas.Children.Contains(arcButton)) return;
            canvas.Children.Remove(arcButton);
            DerenderWeights(canvas);
        }

        public void UpdateArc(uint weight)
        {
            this.weight = weight;

            Line arcLine = (Line)arcButton.Template.FindName("arcLine", arcButton);
            // Assigning the position of first node

            arcLine.X1 = connections[0].GetLocation().X;
            arcLine.Y1 = connections[0].GetLocation().Y;

            // Assigning the position of second node
            arcLine.X2 = connections[1].GetLocation().X;
            arcLine.Y2 = connections[1].GetLocation().Y;

            // Adding weight label relative to the line position
            weightLabel.Content = weight.ToString();
            SetLabelPosition();
        }

        public Node GetConnectedNode(Node currentNode)
        {
            return currentNode == connections[0] ? connections[1] : connections[0];
        }

        public static Arc? GetConnectingArc(Node node1, Node node2)
        {
            return node1.arcConnections.Find(a => node2.arcConnections.Contains(a));
        }
    }
}
