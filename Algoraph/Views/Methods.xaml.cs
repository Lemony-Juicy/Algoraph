using System;
using System.Windows;
using System.Windows.Controls;

namespace Algoraph.Views
{
    public partial class Methods : UserControl
    {
        Editor ed;

        public Methods(Editor editor)
        {
            InitializeComponent();
            this.ed = editor;
        }

        private void AutoClear()
        {
            if (autoClear.IsChecked == true)
                ed.ClearGraph(warning: false);
        }

        private bool TryParseRandNodeInp(out int[] rands)
        {
            rands = new int[2];

            string[] randNodes = randomNodesInput.Text.Split(',');
            if (randNodes.Length != 2) return false;

            if (int.TryParse(randNodes[0], out int r1) && int.TryParse(randNodes[1], out int r2))
            {
                rands[0] = r1;
                rands[1] = r2;
                return true;
            }
            return false;
        }

        #region Events for Methods.xaml

        private void AutoClear_Checked(object sender, RoutedEventArgs e)
        {
            if (autoClear.IsChecked == true)
                MessageBox.Show("When creating random nodes, or creating any other graph state, the graph will be cleared, and will be done so without warning.\n" +
                    "Remember to un-check the box if you do not want this feature.", "Algoraph Notice", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void HideWeights_Checked(object sender, RoutedEventArgs e)
        {
            ed.DisplayArcWeights(false);
        }

        private void HideWeights_Unchecked(object sender, RoutedEventArgs e)
        {
            ed.DisplayArcWeights(true);
        }

        private void CreateOrdered_Button(object sender, RoutedEventArgs e)
        {
            AutoClear();
            if (int.TryParse(orderedNodesDegreeInput.Text, out int degree))
                ed.CreateOrdered(degree);
            else
                Editor.ShowError("Please try again with an appropriate INTEGER input.");
        }

        private void CreateRandom_Button(object sender, RoutedEventArgs e)
        {
            AutoClear();
            if (TryParseRandNodeInp(out int[] rands))
                ed.CreateRandom(rands[0], rands[1]);
            else
                Editor.ShowError("Ensure that the random range of nodes to be plotted is in the format [R1, R2] where R1 and R2 are both integers.");
        }

        private void ConnectRandomly_Button(object sender, RoutedEventArgs e)
        {
            ed.ConnectNodesRandomly();
        }

        private void CreateComplete_Button(object sender, RoutedEventArgs e)
        {
            ed.CompleteGraph();
        }        
        
        private void Prims_Button(object sender, RoutedEventArgs e)
        {
            ed.Prims();
        }

        private void Kruskals_Button(object sender, RoutedEventArgs e)
        {
            ed.Kruskals();
        }

        private void Dijkstras_Button(object sender, RoutedEventArgs e)
        {
            ed.DijkstrasPath();
        }

        private void RouteInspection_Button(object sender, RoutedEventArgs e)
        {
            ed.RouteInspection();
        }


        private void CreateRandomTree(object sender, RoutedEventArgs e)
        {
            ed.CreateRandomTree();
        }


        private void ClearGraph_Button(object sender, RoutedEventArgs e)
        {
            ed.ClearGraph(warning: !autoClear.IsChecked);
        }

        #endregion
    }
}
