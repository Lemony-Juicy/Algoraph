using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows;
using System.Windows.Controls.Primitives;

namespace Algoraph.Scripts
{
    internal class Arc
    {
        private static string currentName = "A1";
        public static bool displayWeight = true;
        public static float thickness = 4.45f;

        public Node[] connections = new Node[2];
        public uint weight { get; private set; }
        public string name { get; private set; }

        private ToggleButton arcButton;
        public Label weightLabel { get; private set; }

        public Arc(Editor editor, Node node1, Node node2, uint weight = 1)
        {
            object s = Application.Current.FindResource("arcUI");
            this.arcButton = new ToggleButton { Style = s as Style };
            this.arcButton.ApplyTemplate();

            this.arcButton.FontSize = thickness;
            weightLabel = new Label()
            {
                Style = (Style)Application.Current.FindResource("WeightLabel")
            };
            ConnectLine(node1, node2, weight);

            this.name = currentName;
            this.arcButton.Name = currentName;
            this.arcButton.Checked += editor.Arc_Checked;
            this.arcButton.Unchecked += editor.Arc_Unchecked;

            IncrementName();
            Canvas.SetZIndex(arcButton, 100);
            Canvas.SetZIndex(weightLabel, 300);
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

        public void ChangeThickness(double thickness)
        {
            Line arcLine = (Line)arcButton.Template.FindName("arcLine", arcButton);

            arcLine.StrokeThickness = thickness;
        }

        void SetLabelPosition()
        {
            Line arcLine = (Line)arcButton.Template.FindName("arcLine", arcButton);

            double x_raw = (arcLine.X2 - arcLine.X1) * .5 + arcLine.X1;
            double y_raw = (arcLine.Y2 - arcLine.Y1) * .5 + arcLine.Y1;

            double x = x_raw - weightLabel.ActualWidth / 2;
            double y = y_raw - weightLabel.ActualHeight / 2;
            Canvas.SetLeft(weightLabel, x);
            Canvas.SetTop(weightLabel, y);
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

        public void AddToCanvas(Canvas canvas)
        {
            if (canvas.Children.Contains(arcButton)) return;
            canvas.Children.Add(arcButton);

            if (displayWeight)
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
            arcLine.X1 = connections[0].GetLocation().X + Node.radius;
            arcLine.Y1 = connections[0].GetLocation().Y + Node.radius;

            // Assigning the position of second node
            arcLine.X2 = connections[1].GetLocation().X + Node.radius;
            arcLine.Y2 = connections[1].GetLocation().Y + Node.radius;

            // Adding weight label relative to the line position
            weightLabel.Content = weight.ToString();
            SetLabelPosition();
        }

        public Node GetConnectedNode(Node currentNode)
        {
            return currentNode == connections[0] ? connections[1] : connections[0];
        }

        static void IncrementName()
        {
            string currentCount = currentName.Substring(1).ToString();
            int num = Convert.ToInt16(currentCount) + 1;
            currentName = "A" + num.ToString();
        }

        public static void DecrementName()
        {
            string currentCount = currentName.Substring(1).ToString();
            int num = Convert.ToInt16(currentCount) - 1;
            currentName = "A" + num.ToString();
        }

        public static void ResetCurrentName()
        {
            currentName = "A1";
        }
    }
}
