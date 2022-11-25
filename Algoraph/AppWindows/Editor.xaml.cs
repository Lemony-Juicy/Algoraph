using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using Algoraph.Scripts;
using Algoraph.Views;

namespace Algoraph
{
    public partial class Editor : Window
    {
        #region Initialisation

        readonly SelectedNodes selectedNodes;
        readonly SelectedArcs selectedArcs;
        readonly Grapher grapher;
        Node? nodeToMove;

        bool leftCtrlDown = false;
        bool middleMouseDown = false;

        Methods methods;
        GraphData graphData;

        public Editor()
        {
            InitializeComponent();
            grapher = new Grapher(this);

            methods = new Methods(this);
            graphData = new GraphData(this);
            sidePanel.Content = methods;

            mainPanel.Cursor = Cursors.Cross;
            selectedNodes = new SelectedNodes(this);
            selectedArcs = new SelectedArcs(this);

            this.KeyDown += MainPanel_KeyDown;
            this.KeyUp += MainPanel_KeyUp;
        }

        #endregion

        #region Graph Data Events

        public void ConnectSelectedNodes()
        {
            for (int i = 0; i < selectedNodes.nodes.Count - 1; i++)
            {
                Node node1 = selectedNodes.nodes[i];
                Node node2 = selectedNodes.nodes[i + 1];
                if (!node1.nodeConnections.Contains(node2))
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

        public void DeleteNonSelectedNodes()
        {
            if (selectedNodes.nodes.Count == 0) return;
            foreach (Node node in grapher.nodes.ToArray())
            {
                if (!selectedNodes.nodes.Contains(node))
                    grapher.RemoveNode(node);
            }
            UpdateNodePanel();
        }

        public void DeleteSelectedArcs()
        {
            foreach (Arc arc in selectedArcs.arcs)
                grapher.RemoveArc(arc);
            selectedArcs.ClearItems();
            UpdateArcPanel();
        }

        public void DeleteNonSelectedArcs()
        {
            if (selectedArcs.arcs.Count == 0) return;
            foreach (Arc arc in grapher.arcs.ToArray())
            {
                if (!selectedArcs.arcs.Contains(arc))
                    grapher.RemoveArc(arc);
            }
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
            if (selectedCount < 1) { graphData.nodePanel.Visibility = Visibility.Collapsed; return; }
            if (selectedCount == 1)
                graphData.joinNodeButton.Visibility = Visibility.Collapsed; 
            else
                graphData.joinNodeButton.Visibility = Visibility.Visible;


            Node n = selectedNodes.nodes.Last();
            graphData.nodeTitle.Text = $"Node Info ({n.name})";
            graphData.nodeInfo1.Text = "Connections:\n" + n.GetNodeConnectionNames();

            int degree = n.nodeConnections.Count();
            string plurality = degree > 1 ? "s" : "";
            graphData.nodeInfo2.Text = $"Connecting {degree} other node{plurality}";

            graphData.nodePanel.Visibility = Visibility.Visible;
        }

        public void UpdateArcPanel()
        {
            int selectedCount = selectedArcs.arcs.Count();
            if (selectedCount < 1) { graphData.arcPanel.Visibility = Visibility.Collapsed; return; }

            Arc a = selectedArcs.arcs.Last();
            graphData.arcTitle.Text = $"Arc Info ({a.name})";
            graphData.arcInfo.Text = "Weighting: " + a.weight;
            graphData.arcPanel.Visibility = Visibility.Visible;
        }

        #endregion

        #region Method Events

        public void DisplayArcWeights(bool show)
        {
            grapher.DisplayArcWeights(show);
        }

        public void CreateOrdered(int degree)
        {
            double width = mainCanvas.ActualWidth;
            double height = mainCanvas.ActualHeight;

            double size = width < height ? width*.5 : height*.5;

            grapher.RegularNodes(degree, (float)size, new Point(width*.5, height*.5), offsetAngle: -Math.PI*.5);
        }

        public void CreateRandom(int rand1, int rand2)
        {
            int iterations;
            if (rand1 > rand2)
                iterations = CustomExtentions.random.Next(rand2, rand1);
            else
                iterations = CustomExtentions.random.Next(rand1, rand2);

            for (int i = 0; i < iterations; i++)
            {
                int width = (int)mainPanel.ActualWidth;
                int height = (int)mainPanel.ActualHeight;
                grapher.AddNode(new Point(CustomExtentions.random.Next(0, width), CustomExtentions.random.Next(0, height)));
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

        public async void Prims()
        {
            if (selectedNodes.nodes.Count == 1)
            {
                bool done = await grapher.Prims(new List<Node>() { selectedNodes.nodes[0] }, selectedArcs);
                if (!done) { ShowError("Ensure the graph is fully connected"); return; }
                selectedNodes.ClearItems();
                MessageBox.Show("- The spanning tree is highlighted in Orange." +
                    "\n- The arcs NOT in Orange can be deleted" +
                    "\n- Press CTRL + DEL Keys to remove these arcs, or click away to ignore",
                    "Notice", MessageBoxButton.OK, MessageBoxImage.Information);

                UpdateArcPanel(); UpdateNodePanel();
            }
            else
            {
                ShowError("Please select one starting node to perform Prims algorithm on.");
            }
        }

        public void DijkstrasPath()
        {
            if (selectedNodes.nodes.Count != 2)
            {
                ShowError("Please select a starting node and an end node to find the shortest path.");
                return;
            }

            Node startNode = selectedNodes.nodes[0]; 
            Node endNode = selectedNodes.nodes[1];
            selectedNodes.ClearItems();

            Node[]? backTrackNodes = grapher.DijkstrasInfo(startNode, out uint[] weighting, endNode, selectedArcs);
            if (backTrackNodes == null) { ShowError("Ensure the graph is fully connected"); return; }

            int indexOfWeight = grapher.nodes.IndexOf(endNode);
            uint minTotalWeight = weighting[indexOfWeight];
            MessageBox.Show($"The path has been highlighted in purple\n" +
                $"Total weighting: {minTotalWeight}", 
                "Dijkstra's Information", 
                MessageBoxButton.OK, 
                MessageBoxImage.Information);

            UpdateNodePanel();

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
            mainPanel.Cursor = Cursors.Cross;
        }
        public void CursorArrowMode()
        {
            mainPanel.Cursor = Cursors.Arrow;
        }
        public void CursorHandMode()
        {
            mainPanel.Cursor = Cursors.Hand;
        }
        public void ShowError(string error = "Please try again with an appropriate input.")
        {
            MessageBox.Show(error, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        /// <summary>
        /// Updates the graph adjacency list table
        /// </summary>
        public void RenderTable()
        {
            graphData.table.Clear();
            DataRow row;
            foreach(Node node in grapher.nodes)
            {
                row = graphData.table.NewRow();
                row[0] = node.name;
                row[1] = string.Join(',', node.nodeConnections.Select(n => n.name));
                graphData.table.Rows.Add(row);
            }

            graphData.adjDataGrid.ItemsSource = graphData.table.DefaultView;
        }

        /// <summary>
        /// Checks whether mouse is in the main editor panel.
        /// </summary>
        /// <returns>False if mouse not in main panel.</returns>
        private bool IsMouseMainPanel()
        {
            Point pos = Mouse.GetPosition(mainPanel);
            return pos.X > 0 && pos.Y > 0 && pos.X < mainPanel.ActualWidth && pos.Y < mainPanel.ActualHeight;
        }

        /// <summary>
        /// Gets the clamped mouse position relative to the main panel, so the position is always inside the main panel.
        /// </summary>
        /// <returns>Point of the clamped mouse position.</returns>
        private Point MousePosMainPanel()
        {
            Point pos = Mouse.GetPosition(mainPanel);
            return new Point(Math.Clamp(pos.X, 0, mainPanel.ActualWidth), Math.Clamp(pos.Y, 0, mainPanel.ActualHeight));
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

        private void Main_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //if (!IsMouseMainPanel()) return;

            if (e.MiddleButton == MouseButtonState.Pressed)
            {
                this.nodeToMove = grapher.GetClosestNode(Mouse.GetPosition(mainPanel));
                this.middleMouseDown = true;
            }
        }

        private void Main_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.MiddleButton == MouseButtonState.Released)
            {
                this.middleMouseDown = false;
            }
        }

        private void MainPanel_LeftMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (mainPanel.Cursor == Cursors.Cross)
                grapher.AddNode(Mouse.GetPosition(mainCanvas));
            else if (Mouse.DirectlyOver is Border)
            {
                selectedArcs.ClearItems();
                selectedNodes.ClearItems();
                CursorCrossMode();
            }
        }

        private void Main_MouseMove(object sender, MouseEventArgs e)
        {
            if (this.middleMouseDown || this.leftCtrlDown)
                grapher.MoveNode(this.nodeToMove, MousePosMainPanel());
        }

        private void MainPanel_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.LeftCtrl && !this.leftCtrlDown)
            {
                this.nodeToMove = grapher.GetClosestNode(Mouse.GetPosition(mainCanvas));
                this.leftCtrlDown = true;
            }

            if (!IsMouseMainPanel()) return;
            switch (e.Key)
            {
                case Key.Delete:
                    if (Keyboard.IsKeyDown(Key.LeftCtrl))
                    {
                        DeleteNonSelectedArcs();
                        DeleteNonSelectedNodes();
                    }
                    else
                    {
                        DeleteSelectedNodes();
                        DeleteSelectedArcs();
                    }
                    
                    break;
                case Key.J:
                    ConnectSelectedNodes();
                    break;
                case Key.P:
                    Prims();
                    break;
            }
        }

        private void MainPanel_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.LeftCtrl)
                this.leftCtrlDown = false;
        }

        #endregion
    }
}
