using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Algoraph.Scripts;
using Algoraph.Views;

namespace Algoraph
{
    public partial class Editor : Window
    {
        #region Initialisation

        public readonly Random random = new Random();
        readonly SelectedNodes selectedNodes;
        readonly SelectedArcs selectedArcs;
        readonly Grapher grapher;
        Node? nodeToMove;

        bool leftCtrlDown = false;
        Methods methods;
        GraphData graphData;

        public Editor()
        {
            InitializeComponent();
            grapher = new Grapher(this);

            methods = new Methods(this);
            graphData = new GraphData(this);
            sidePanel.Content = methods;

            middlePanel.Cursor = Cursors.Cross;
            selectedNodes = new SelectedNodes(this);
            selectedArcs = new SelectedArcs(this);

            this.KeyDown += MiddleGrid_KeyDown;
            this.KeyUp += MiddleGrid_KeyUp;
        }

        #endregion

        #region Graph Data Events

        public void ConnectSelectedNodes()
        {
            for (int i = 0; i < selectedNodes.nodes.Count - 1; i++)
            {
                Node node1 = selectedNodes.nodes[i];
                Node node2 = selectedNodes.nodes[i + 1];
                if (node1.nodeConnections.Contains(node2))
                    grapher.Disconnect(node1, node2);
                else
                    grapher.Connect(node1, node2);
            }
        }

        public void DeleteSelectedNodes()
        {
            foreach (Node node in selectedNodes.nodes)
                grapher.RemoveNode(node);
            selectedNodes.ClearItems();
            UpdateNodePanel();
        }

        public void DeleteSelectedArcs()
        {
            foreach (Arc arc in selectedArcs.arcs)
                grapher.RemoveArc(arc);
            selectedArcs.ClearItems();
            UpdateArcPanel();
        }

        public void ChangeArcWeights(uint weight)
        {
            foreach (Arc arc in selectedArcs.arcs)
                arc.UpdateArc(weight);
        }

        public void ChangeNodeName(string newName)
        {
            if (grapher.NodeNameExists(newName))
            {
                ShowError("This name is invalid, try again.");
                return;
            }

            selectedNodes.nodes.Last().ChangeName(newName);
        }

        public void UpdateNodePanel()
        {
            int selectedCount = selectedNodes.nodes.Count();
            if (selectedCount < 1) { graphData.nodeInfo.Visibility = Visibility.Collapsed; return; }

            TextBlock title = (TextBlock)graphData.nodeInfo.Template.FindName("title", graphData.nodeInfo);
            Button joinButton = (Button)graphData.nodeInfo.Template.FindName("joinNodeButton", graphData.nodeInfo);

            if (selectedCount == 1)
                joinButton.Visibility = Visibility.Collapsed; 
            else
                joinButton.Visibility = Visibility.Visible;

            Node n = selectedNodes.nodes.Last();
            title.Text = $"Node Info ({n.name})";
            graphData.nodeInfo.Content = "Connections:\n" + n.GetNodeConnectionNames();

            int degree = n.nodeConnections.Count();
            string plurality = degree > 1 ? "s" : "";
            graphData.nodeInfo.ContentStringFormat = $"Connecting {degree} other node{plurality}";

            graphData.nodeInfo.Visibility = Visibility.Visible;
        }

        public void UpdateArcPanel()
        {
            int selectedCount = selectedArcs.arcs.Count();
            if (selectedCount < 1) { graphData.arcInfo.Visibility = Visibility.Collapsed; return; }

            TextBlock title = (TextBlock)graphData.arcInfo.Template.FindName("title", graphData.arcInfo);

            Arc a = selectedArcs.arcs.Last();
            title.Text = $"Arc Info ({a.name})";
            graphData.arcInfo.Content = "Weighting: " + a.weight;
            graphData.arcInfo.Visibility = Visibility.Visible;
        }

        #endregion

        #region Method Events

        public void CreateOrdered(int degree)
        {
            double width = mainCanvas.ActualWidth;
            double height = mainCanvas.ActualHeight;

            double size = width < height ? width*.5 : height*.5;

            grapher.RegularNodes(degree, (float)size, new Point(width*.5, height*.5));
        }

        public void CreateRandom(int rand1, int rand2)
        {
            int iterations;
            if (rand1 > rand2)
                iterations = random.Next(rand2, rand1);
            else
                iterations = random.Next(rand1, rand2);

            for (int i = 0; i < iterations; i++)
            {
                int width = (int)mainCanvas.ActualWidth;
                int height = (int)mainCanvas.ActualHeight;
                grapher.AddNode(new Point(random.Next(0, width), random.Next(0, height)));
            }
        }

        public void ConnectNodesRandomly()
        {
            grapher.RandomConnections();
        }

        public void CompleteGraph()
        {
            grapher.CompleteGraphArcs();
        }

        public void Prims()
        {
            if (selectedNodes.nodes.Count == 1)
            {
                bool done = grapher.Prims(new List<Node>() { selectedNodes.nodes[0] }, selectedArcs);
                if (!done) { ShowError("Ensure the graph is fully connected"); return; }
                selectedNodes.ClearItems();
                MessageBox.Show("- The spanning tree is highlighted in white." +
                    "\n- The arcs in purple have been selected to be deleted" +
                    "\n- Press the delete Key to remove these arcs, or click away to ignore",
                    "Notice", MessageBoxButton.OK, MessageBoxImage.Information);

                UpdateArcPanel(); UpdateNodePanel();
            }
            else
            {
                ShowError("Please select one starting node to perform Prims algorithm on.");
            }
        }

        public void ClearGraph(bool? warning=true)
        {
            MessageBoxResult result = MessageBoxResult.Yes;

            if (warning == true)
            {
                result = MessageBox.Show(
                "Are you sure you want to completely erase the current state of this graph?",
                "AlgoRaph Notice",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning,
                MessageBoxResult.No
                );
            }
            

            if (result == MessageBoxResult.Yes)
            {
                grapher.ClearArcs(); 
                grapher.ClearNodes();
            }
                
        }

        #endregion

        #region Component Events

        public void Node_Checked(object sender, RoutedEventArgs e)
        {
            Node? node = grapher.GetNodeFromName(((ToggleButton)sender).Name);
            selectedNodes.OnCheck(node, selectedArcs);
            UpdateNodePanel();
        }

        public void Node_Unchecked(object sender, RoutedEventArgs e)
        {
            Node? node = grapher.GetNodeFromName(((ToggleButton)sender).Name);
            selectedNodes.OnUncheck(node);
            UpdateNodePanel();
        }

        public void Node_Enter(object sender, MouseEventArgs e)
        {
            selectedNodes.OnEnter();
        }


        public void Node_Leave(object sender, MouseEventArgs e)
        {
            selectedNodes.OnLeave();
        }



        public void Arc_Checked(object sender, RoutedEventArgs e)
        {
            Arc? arc = grapher.GetArcFromName(((ToggleButton)sender).Name);
            selectedArcs.OnCheck(arc, selectedNodes);
            UpdateArcPanel();
        }

        public void Arc_Unchecked(object sender, RoutedEventArgs e)
        {
            Arc? arc = grapher.GetArcFromName(((ToggleButton)sender).Name);
            selectedArcs.OnUncheck(arc);
            UpdateArcPanel();
        }

        public void Arc_Enter(object sender, MouseEventArgs e)
        {
            selectedArcs.OnEnter();
        }

        public void Arc_Leave(object sender, MouseEventArgs e)
        {
            selectedArcs.OnLeave();
        }

        #endregion

        #region Short Hand Methods

        public void CursorCrossMode()
        {
            middlePanel.Cursor = Cursors.Cross;
        }
        public void CursorArrowMode()
        {
            middlePanel.Cursor = Cursors.Arrow;
        }
        public void CursorHandMode()
        {
            middlePanel.Cursor = Cursors.Hand;
        }
        public void ShowError(string error = "Please try again with an appropriate input.")
        {
            MessageBox.Show(error, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        private void MoveClosestNodeMousePos()
        {
            Point mousPos = Mouse.GetPosition(mainCanvas);
            grapher.MoveNode(nodeToMove, mousPos);
        }

        #endregion

        #region Misc

        private void ToggleView_Click(object sender, RoutedEventArgs e)
        {
            if (sidePanel is null) return;

            ToggleButton toggleButton = (ToggleButton)sender;
            if (toggleButton.Name == "graphDataButton")
                sidePanel.Content = graphData;
            else
                sidePanel.Content = methods;
        }

        private void ButtonBackToMenu(object sender, RoutedEventArgs e)
        {
            MainMenu menu = new MainMenu();
            menu.Show();
        }

        #endregion

        #region Main Panel

        private void MiddleGrid_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (middlePanel.Cursor == Cursors.Cross)
            {
                grapher.AddNode(Mouse.GetPosition(mainCanvas));
            }
            else if (Mouse.DirectlyOver is Border)
            {
                selectedArcs.ClearItems();
                selectedNodes.ClearItems();
                CursorCrossMode();
            }
        }

        private void MiddleGrid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (Mouse.MiddleButton == MouseButtonState.Pressed || this.leftCtrlDown)
                nodeToMove = grapher.GetClosestNode(Mouse.GetPosition(middlePanel));
        }

        private void MiddleGrid_MouseMove(object sender, MouseEventArgs e)
        {
            if (Mouse.MiddleButton == MouseButtonState.Pressed || this.leftCtrlDown)
                MoveClosestNodeMousePos();
        }

        private void MiddleGrid_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Delete:
                    DeleteSelectedNodes();
                    DeleteSelectedArcs();
                    break;
                case Key.J:
                    ConnectSelectedNodes();
                    break;
                case Key.W:
                    grapher.ToggleArcWeights();
                    break;
                case Key.P:
                    Prims();
                    break;
            }

            if (e.Key == Key.LeftCtrl && !this.leftCtrlDown)
            {
                this.nodeToMove = grapher.GetClosestNode(Mouse.GetPosition(mainCanvas));
                this.leftCtrlDown = true;
            }
        }

        private void MiddleGrid_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.LeftCtrl)
                this.leftCtrlDown = false;
        }

        #endregion
    }
}
