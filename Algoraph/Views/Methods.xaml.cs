using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

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

        #region Events for Methods.xaml

        private void TextBoxSelectAll(object sender, MouseButtonEventArgs e)
        {
            (sender as TextBox).SelectAll();
        }

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
            if (int.TryParse(randomNodesInput.Text, out int rand))
                ed.CreateRandom(rand);
            else
                Editor.ShowError("Ensure that the input is an integer");
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

        #region Maze Events

        private void CreateComplexMaze_Button(object sender, RoutedEventArgs e)
        {
            ed.CreateComplexMaze();
        }

        private void CreateSimpleMaze_Button(object sender, RoutedEventArgs e)
        {
            ed.CreateSimpleMaze();
        }

        private void DFS_Button(object sender, RoutedEventArgs e)
        {
            ed.DFS();
        }

        private void BFS_Button(object sender, RoutedEventArgs e)
        {
            ed.BFS();
        }

        private void A_Star_Button(object sender, RoutedEventArgs e)
        {
            ed.A_Star();
        }

        #endregion
    }
}
