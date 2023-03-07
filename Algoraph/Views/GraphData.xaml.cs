﻿using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Data;
using System.Windows.Data;
using System.Windows.Threading;
using Algoraph.Scripts;
using System;

namespace Algoraph.Views
{

    public partial class GraphData : UserControl
    {
        readonly Editor ed;

        public GraphData(Editor ed)
        {
            InitializeComponent();
            this.ed = ed;


        }

        #region Node Panel

        private void JoinNode_Button(object sender, RoutedEventArgs e)
        {
            ed.ConnectSelectedNodes();
        }

        private void DeleteNonSelectedNode_Button(object sender, RoutedEventArgs e)
        {
            ed.DeleteNonSelectedNodes();
        }

        private void DeleteNode_Button(object sender, RoutedEventArgs e)
        {
            ed.DeleteSelectedNodes();
        }

        private void NodeNameTextbox_KeyDown(object sender, KeyEventArgs e)
        {
            if (!(e.Key == Key.Enter)) return;
            TextBox textBox = (TextBox)sender;
            textBox.ApplyTemplate();
            TextBox inputText = ((TextBox)textBox.Template.FindName("inputText", textBox));
            ed.ChangeNodeName(inputText.Text);
        }

        #endregion

        #region Arc Panel

        private void DeleteNonSelectedArc_Button(object sender, RoutedEventArgs e)
        {
            ed.DeleteNonSelectedArcs();
        }

        private void DeleteArc_Button(object sender, RoutedEventArgs e)
        {
            ed.DeleteSelectedArcs();
        }        
        
        private void ArcWeightTextbox_KeyDown(object sender, KeyEventArgs e)
        {
            if (!(e.Key == Key.Enter)) return;
            
            TextBox textBox = (TextBox)sender;
            if (!textBox.Template.HasContent)
                textBox.ApplyTemplate();
            TextBox inputText = (TextBox)textBox.Template.FindName("inputText", textBox);

            if (uint.TryParse(inputText.Text, out uint weight))
            {
                if (weight > 0)
                    ed.ChangeArcWeights(weight);
                else
                    Editor.ShowError("You cannot have an arc weighting of 0, silly!");
            }
        }

        #endregion
    }
}
