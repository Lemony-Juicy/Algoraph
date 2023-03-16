using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Input.Manipulations;
using System.Windows.Media;
using System.Windows.Shapes;
using Algoraph.Scripts;
using Algoraph.Scripts.Maze_Scripts;
using Algoraph.Views;
using Microsoft.Win32;

namespace Algoraph
{
    public partial class Editor : Window
    {
        #region Initialisation

        readonly SelectedNodes selectedNodes;
        readonly SelectedArcs selectedArcs;
        readonly Grapher grapher;
        readonly MazeGraph mazeGraph;
        readonly UndoStack undoStack;

        bool Mdown = false;
        bool processing = false;
        bool middleMouseDown = false;
        Node? nodeArcDraggedFrom = null;
        Line? dragLine = null;
        TaskCompletionSource<bool>? nextStepClicked;

        int mazeWidth = 40;
        int mazeHeight = 30;

        public readonly Methods methods;
        readonly GraphData graphData;
        public string? path = null;

        public Editor()
        {
            InitializeComponent();
            grapher = new Grapher(this);
            mazeGraph = new MazeGraph(this, mazeWidth, mazeHeight);
            undoStack = new UndoStack();

            methods = new Methods(this);
            graphData = new GraphData(this);
            sidePanel.Content = methods;

            mainPanel.Cursor = Cursors.Cross;
            selectedNodes = new SelectedNodes(this);
            selectedArcs = new SelectedArcs(this);

            this.KeyDown += MainPanel_KeyDown;
            this.KeyUp += MainPanel_KeyUp;

            CursorArrowMode();
        }

        public void SetProcessing(bool processing)
        {
            this.processing = processing;
        }

        public bool CheckIsProcessing()
        {
            if (processing)
            {
                MessageBox.Show("A Process is currently being ran, please try again later.\nIf you are on stepped intervals, and want to finish this process, click the delayed interval button at the top, and set the delay on the slider to zero.");
                return true;
            }
            return false;
        }

        #endregion

        #region Graph Data Updates


        public void BeforeGraphChanged()
        {
            undoStack.Push(Saver.GetJsonData(grapher.nodes, grapher.arcs, header_item: Saver.HEADER_ITEM));
        }

        public void MarkAsChanged()
        {
            if (path != null && !this.Title.Contains('*'))
                this.Title += " *";

        }

        public void UpdateNodePanel()
        {
            int selectedCount = selectedNodes.nodes.Count;
            if (selectedCount < 1) 
            { 
                graphData.nodePanel.Visibility = Visibility.Collapsed; 
                return; 
            }

            if (selectedCount == 1)
                graphData.joinNodeButton.Visibility = Visibility.Collapsed; 
            else
                graphData.joinNodeButton.Visibility = Visibility.Visible;


            Node n = selectedNodes.nodes.Last();

            int degree = n.nodeConnections.Count;
            string plurality = degree > 1 ? "s" : "";
            graphData.nodeInfo2.Text = $"Connecting {degree} other node{plurality}";
            graphData.nodeInfo1.Text = n.name;

            graphData.nodePanel.Visibility = Visibility.Visible;
        }

        public void UpdateArcPanel()
        {
            int selectedCount = selectedArcs.arcs.Count;
            if (selectedCount < 1) { graphData.arcPanel.Visibility = Visibility.Collapsed; return; }

            Arc a = selectedArcs.arcs.Last();
            graphData.arcTitle.Text = $"Arc Info";
            graphData.arcInfo.Text = "Weighting: " + a.weight;
            graphData.arcPanel.Visibility = Visibility.Visible;
        }

        #endregion

        #region Method Events

        public void ConnectSelectedNodes()
        {
            BeforeGraphChanged();
            for (int i = 0; i < selectedNodes.nodes.Count - 1; i++)
            {
                Node node1 = selectedNodes.nodes[i];
                Node node2 = selectedNodes.nodes[i + 1];
                if (!node1.nodeConnections.Contains(node2))
                    grapher.Connect(node1, node2);
            }
            MarkAsChanged();
        }

        public void DeleteSelectedNodes()
        {
            if (selectedNodes.nodes.Count == 0) return;
            BeforeGraphChanged();
            foreach (Node node in selectedNodes.nodes)
                grapher.RemoveNode(node);
            selectedNodes.ClearItems();
            UpdateNodePanel();
            MarkAsChanged();
        }

        public void DeleteNonSelectedNodes()
        {
            BeforeGraphChanged();
            if (selectedNodes.nodes.Count == 0) return;
            foreach (Node node in grapher.nodes.ToArray())
            {
                if (!selectedNodes.nodes.Contains(node))
                    grapher.RemoveNode(node);
            }
            UpdateNodePanel();
            MarkAsChanged();
        }

        public void DeleteSelectedArcs()
        {
            if (selectedArcs.arcs.Count == 0) return;
            BeforeGraphChanged();
            foreach (Arc arc in selectedArcs.arcs)
                grapher.RemoveArc(arc);
            selectedArcs.ClearItems();
            UpdateArcPanel();
            MarkAsChanged();
        }

        public void DeleteNonSelectedArcs()
        {
            BeforeGraphChanged();
            if (selectedArcs.arcs.Count == 0) return;
            foreach (Arc arc in grapher.arcs.ToArray())
            {
                if (!selectedArcs.arcs.Contains(arc))
                    grapher.RemoveArc(arc);
            }
            UpdateArcPanel();
            MarkAsChanged();
        }

        public void ChangeArcWeights(uint weight)
        {
            BeforeGraphChanged();
            foreach (Arc arc in selectedArcs.arcs)
                arc.UpdateArc(weight);
            MarkAsChanged();
        }

        public void ChangeNodeName(string newName)
        {
            BeforeGraphChanged();
            if (!grapher.ValidNodeName(newName))
            {
                ShowError("This name is invalid, try again.");
                return;
            }

            selectedNodes.nodes.Last().ChangeName(newName);
            MarkAsChanged();
        }

        public void DisplayArcWeights(bool show)
        {
            grapher.DisplayArcWeights(show);
        }

        public void CreateOrdered(int degree)
        {
            BeforeGraphChanged();
            double width = mainCanvas.ActualWidth;
            double height = mainCanvas.ActualHeight;

            double size = width < height ? width*.5 : height*.5;

            grapher.RegularNodes(degree, (float)size - 10, new Point(width*.5, height*.5), offsetAngle: -Math.PI*.5);
            MarkAsChanged();
        }

        public void CreateRandom(int iterations)
        {
            BeforeGraphChanged();

            for (int i = 0; i < iterations; i++)
            {
                int width = (int)mainPanel.ActualWidth;
                int height = (int)mainPanel.ActualHeight;
                if (!grapher.AddNode(new Point(CustomExtentions.random.Next(0, width), CustomExtentions.random.Next(0, height))))
                    return;
            }
            MarkAsChanged();
        }

        public void CreateRandomTree()
        {
            BeforeGraphChanged();
            grapher.CreateRandomTree();
            MarkAsChanged();
        }

        public void ConnectNodesRandomly()
        {
            BeforeGraphChanged();
            grapher.RandomConnections();
            MarkAsChanged();
        }

        public void CompleteGraph()
        {
            BeforeGraphChanged();
            grapher.CompleteGraphArcs();
            MarkAsChanged();
        }

        public async void Prims()
        {
            if (CheckIsProcessing()) return;
            if (selectedNodes.nodes.Count == 1 && grapher.IsFullyConnected())
            {
                ShowStepIntervalMsgBox();
                SetProcessing(true);
                await grapher.Prims(new List<Node>() { selectedNodes.nodes[0] }, selectedArcs);
                SetProcessing(false);
                selectedNodes.ClearItems();
                MessageBox.Show("- The spanning tree is highlighted in Orange." +
                    "\n- The arcs NOT in Orange can be deleted" +
                    "\n- Press CTRL + DEL Keys to remove these arcs, or click away to ignore",
                    "Notice", MessageBoxButton.OK, MessageBoxImage.Information);

                UpdateArcPanel(); 
                UpdateNodePanel();
            }
            else
            {
                ShowError("Please select one starting node to perform Prims algorithm on, or ensure the graph is fully connected");
            }
        }

        public async void Kruskals()
        {
            if (CheckIsProcessing()) return;
            if (grapher.IsFullyConnected() && grapher.arcs.Count > 0)
            {
                ShowStepIntervalMsgBox();
                SetProcessing(true);
                await grapher.Kruskals(selectedArcs);
                SetProcessing(false);
                MessageBox.Show("- The spanning tree is highlighted in Orange." +
                    "\n- The arcs NOT in Orange can be deleted" +
                    "\n- Press CTRL + DEL Keys to remove these arcs, or click away to ignore",
                    "Notice", MessageBoxButton.OK, MessageBoxImage.Information);

                UpdateArcPanel(); UpdateNodePanel();
            }
            else
            {
                ShowError("Please ensure the graph is fully connected and that there is a graph on the screen");
            }
        }

        public async void DijkstrasPath()
        {
            if (CheckIsProcessing()) return;
            if (!grapher.IsFullyConnected() || selectedNodes.nodes.Count != 2) 
            { 
                ShowError("Please select a starting node and an end node to find the shortest path, or ensure graph is fully connected"); 
                return; 
            }

            Node startNode = selectedNodes.nodes[1]; 
            Node endNode = selectedNodes.nodes[0];
            selectedNodes.ClearItems();

            ShowStepIntervalMsgBox();
            SetProcessing(true);
            await grapher.BackTrackTrace(startNode, endNode, grapher.DijkstrasInfo(startNode, out uint[] weighting), selectedArcs);
            SetProcessing(false);
            int indexOfWeight = grapher.nodes.IndexOf(endNode);
            uint minTotalWeight = weighting[indexOfWeight];

            MessageBox.Show($"The path has been highlighted in Orange\n" +
                $"Total weighting: {minTotalWeight}", 
                "Dijkstra's Information", 
                MessageBoxButton.OK, 
                MessageBoxImage.Information);

            UpdateNodePanel();
        }

        public async void RouteInspection()
        {
            if (CheckIsProcessing()) return;
            if (!grapher.IsFullyConnected() || grapher.arcs.Count <= 1)
            {
                ShowError("Ensure graph is fully connected, and there is more than one arc, to carry out route inspection");
                return;
            }

            ShowStepIntervalMsgBox();
            SetProcessing(true);
            uint minTotalWeight = await grapher.RouteInspection(selectedArcs);
            SetProcessing(false);
            if (minTotalWeight > 0)
            {
                MessageBox.Show($"The path to be repeated has been highlighted in Orange\n" +
                "Total minimum weighting for route: " + minTotalWeight.ToString(),
                "Route Inspection Information",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
            }
            else
            {
                ShowError("This graph has too many odd nodes. Max number of odd nodes is 4");
            }
            
        }


        public bool IsFullyConnected()
        {
            return grapher.IsFullyConnected();
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
                BeforeGraphChanged();
                grapher.ClearArcs(); 
                grapher.ClearNodes();
            }
        }

        #endregion

        #region Component Events (For Node and Arcs)

        public void Node_Checked(object sender, RoutedEventArgs e)
        {
            Node? node = ((ToggleButton)sender).Tag as Node;
            selectedNodes.OnCheck(node, selectedArcs);
            UpdateNodePanel();
        }

        public void Node_Unchecked(object sender, RoutedEventArgs e)
        {
            Node? node = ((ToggleButton)sender).Tag as Node;
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
            Arc? arc = ((ToggleButton)sender).Tag as Arc;
            selectedArcs.OnCheck(arc, selectedNodes);
            UpdateArcPanel();
        }

        public void Arc_Unchecked(object sender, RoutedEventArgs e)
        {
            Arc? arc = ((ToggleButton)sender).Tag as Arc;
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
        public static void ShowError(string error = "Please try again with an appropriate input.")
        {
            MessageBox.Show(error, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        public void ShowStepIntervalMsgBox()
        {
            if (steppedIntervalButton.Visibility== Visibility.Visible) 
                MessageBox.Show("To move to next step in this algorithmic process, " +
                    "click the 'Next Step' green button at the top.", "Notice", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public async Task WaitForNextCycle()
        {
            if (delayedIntervalPanel.Visibility == Visibility.Visible)
            {
                await Task.Delay((int)(speedSlider.Value * 1000));
            }
            else
            {
                // Wait for user to click the next step button
                bool isSteppedInterval = await WaitForNextStep();
                if (!isSteppedInterval)
                {
                    await Task.Delay((int)(speedSlider.Value * 1000));
                }
            }
        }

        private async Task<bool> WaitForNextStep()
        {
            // the TaskCompletionSource is a class used to help convert things to asyncronous behaviours.
            nextStepClicked = new TaskCompletionSource<bool>();

            // This waits for the nextStepClicked.TrySetResult(VALUE) method to be done.
            return await nextStepClicked.Task;
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

        #region This Window's Events

        private void UndoButton_Click(object sender, RoutedEventArgs e)
        {
            undoStack.LoadPrevious(this);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Console.WriteLine(sender);
            if (!Title.Contains('*') || !this.IsActive) return;
            MessageBoxResult r = MessageBox.Show("Do you want to save?", "Save document", MessageBoxButton.YesNoCancel);
            if (r == MessageBoxResult.Yes)
            {
                if (!SaveState())
                    e.Cancel = true;
            }
            else if (r == MessageBoxResult.Cancel)
            {
                e.Cancel = true;
            }
        }

        private void AutoInterval_Checked(object sender, RoutedEventArgs e)
        {
            if (delayedIntervalPanel == null) return;
            delayedIntervalPanel.Visibility = Visibility.Visible;
            steppedIntervalButton.Visibility = Visibility.Collapsed;

            if (nextStepClicked == null) return;

            // Setting the result to false so it triggers the await WaitForNextStep() to return false.
            // This implies the user has switched to delay intervals from the stepped intervals, so use that instead 
            nextStepClicked.TrySetResult(false);

            // then the variable is set to null again.
            nextStepClicked = null;
        }

        private void SteppedInterval_Checked(object sender, RoutedEventArgs e)
        {
            if (delayedIntervalPanel == null) return;
            delayedIntervalPanel.Visibility = Visibility.Collapsed;
            steppedIntervalButton.Visibility = Visibility.Visible;
        }

        private void SteppedInterval_Click(object sender, RoutedEventArgs e)
        {
            if (nextStepClicked == null) return;
            
            // Setting the result to true so it triggers the await WaitForNextStep() to return true.
            nextStepClicked.TrySetResult(true);  

            // then the variable is set to null again.
            nextStepClicked = null;
            
        }

        private void SpeedSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            double delay = Math.Round(speedSlider.Value, 2);
            if (!delay.ToString().Contains('.'))
                delayTextBlock.Text = $"{delay}.0 seconds";
            else
                delayTextBlock.Text = $"{delay} seconds";
        }

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
            MainMenu menu = new();
            menu.Show();
        }

        private void SaveStateButton(object sender, RoutedEventArgs e)
        {
            SaveState();
        }

        private void NewProjectButton(object sender, RoutedEventArgs e)
        {
            Editor editor = new();
            editor.Show();
        }

        private void OpenProjectButton(object sender, RoutedEventArgs e)
        {
            MainMenu.OpenProject();
        }

        #endregion

        #region Main Panel

        private void Main_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!middleMouseDown && !(mazeCheckbox.IsChecked == true) && e.MiddleButton == MouseButtonState.Pressed)
            {
                BeforeGraphChanged();
                this.middleMouseDown = true;
            }

            // Start dragging line
            if (e.RightButton == MouseButtonState.Pressed && Mouse.DirectlyOver is Ellipse component && nodeArcDraggedFrom == null)
            {
                nodeArcDraggedFrom = (Node)component.Tag;
                dragLine = new Line()
                {
                    X1 = nodeArcDraggedFrom.GetLocation().X,
                    Y1 = nodeArcDraggedFrom.GetLocation().Y,
                    X2 = Mouse.GetPosition(mainPanel).X,
                    Y2 = Mouse.GetPosition(mainPanel).Y,
                    Stroke = Brushes.Black,
                    Opacity = 0.75,
                    StrokeThickness = 3
                };
                mainCanvas.Children.Add(dragLine);
            }
        }

        private void Main_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.MiddleButton == MouseButtonState.Released && this.middleMouseDown)
            {
                this.middleMouseDown = false;
                MarkAsChanged();
            }

            double distance = grapher.GetClosestNodeDistance(Mouse.GetPosition(mainPanel));
            Node closestNode = grapher.GetClosestNode(Mouse.GetPosition(mainPanel));
            if (e.RightButton == MouseButtonState.Released && distance < 50 && nodeArcDraggedFrom != null)
            {
                BeforeGraphChanged();
                grapher.Connect(closestNode, nodeArcDraggedFrom);
            }
            nodeArcDraggedFrom = null;
            mainCanvas.Children.Remove(dragLine);
            dragLine = null;
        }

        private void MainPanel_LeftMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (mainPanel.Cursor == Cursors.Cross)
            {
                BeforeGraphChanged();
                grapher.AddNode(Mouse.GetPosition(mainCanvas));
                MarkAsChanged();
            } 
            else if (Mouse.DirectlyOver is Border)
            {
                selectedArcs.ClearItems();
                selectedNodes.ClearItems();
                CursorCrossMode();
            }
        }

        private void Main_MouseMove(object sender, MouseEventArgs e)
        {
            // MOVE NODE 
            if (this.middleMouseDown || this.Mdown)
            {
                grapher.MoveNode(grapher.GetClosestNode(Mouse.GetPosition(mainPanel)), 
                    MousePosMainPanel());
            }

            if (dragLine != null)
            {
                dragLine.X2 = Mouse.GetPosition(mainPanel).X;
                dragLine.Y2 = Mouse.GetPosition(mainPanel).Y;
            }
        }

        private void MainPanel_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.M && !this.Mdown)
            {
                BeforeGraphChanged();
                this.Mdown = true;
            }

            if (Keyboard.IsKeyDown(Key.LeftCtrl) && Keyboard.IsKeyDown(Key.S))
            {
                SaveState();
            }

            if (Keyboard.IsKeyDown(Key.LeftCtrl) && Keyboard.IsKeyDown(Key.O))
            {
                MainMenu.OpenProject();
            }

            if (Keyboard.IsKeyDown(Key.LeftCtrl) && Keyboard.IsKeyDown(Key.Z))
            {
                undoStack.LoadPrevious(this);
            }

            if (!IsMouseMainPanel()) return;
            switch (e.Key)
            {
                case Key.Delete:
                    if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
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
                    if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                        ConnectSelectedNodes();
                    break;
            }
        }

        private void MainPanel_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.M)
            {
                this.Mdown = false;
                MarkAsChanged();
            }      
        }

        #endregion

        #region Saving and Loading

        public bool LoadState()
        {
            if (path != null && grapher.LoadState(path: path))
            {
                return true;
            }       
            return false;
        }

        public void LoadJsonData(string jsonData)
        {
            grapher.ClearArcs();
            grapher.ClearNodes();
            Console.WriteLine(jsonData);
            grapher.LoadState(jsonString: jsonData);
        }

        public bool SaveState()
        {
            if (path == null)
            {
                SaveFileDialog fileDialogue = new()
                {
                    DefaultExt = ".json",
                    Filter = "JSON files (*.json)|*.json",
                    CheckFileExists = false
                };

                // When user clicks ok button
                if (fileDialogue.ShowDialog() == true)
                {
                    path = fileDialogue.FileName;
                    grapher.SaveState(path);
                    return true;
                }
                return false;
            }

            grapher.SaveState(path);
            this.Title = path.Split('\\').Last();
            return true;
        }

        #endregion

        #region Maze

        private void MazeMode_Checked(object sender, RoutedEventArgs e)
        {
            mazeGrid.Visibility = Visibility.Visible;
            methods.mazeStackPanel.Visibility = Visibility.Visible;

            mainCanvas.Visibility = Visibility.Collapsed;
            mainPanel.Visibility = Visibility.Collapsed;
            methods.mainStackPanel.Visibility = Visibility.Collapsed;
        }

        private void MazeMode_Unchecked(object sender, RoutedEventArgs e)
        {
            mazeGrid.Visibility = Visibility.Collapsed;
            methods.mazeStackPanel.Visibility = Visibility.Collapsed;

            mainCanvas.Visibility = Visibility.Visible;
            mainPanel.Visibility = Visibility.Visible;
            methods.mainStackPanel.Visibility = Visibility.Visible;
        }


        public async void CreateSimpleMaze()
        {
            await mazeGraph.CreateSimpleMazeAnimation();
        }

        public async void CreateComplexMaze()
        {
            await mazeGraph.CreateComplexMazeAnimation();
        }

        public async void DFS()
        {
            await mazeGraph.DFS();
        }

        public async void BFS()
        {
            await mazeGraph.BFS();
        }

        public async void A_Star()
        {
            await mazeGraph.A_star();
        }



        #endregion
    }
}
