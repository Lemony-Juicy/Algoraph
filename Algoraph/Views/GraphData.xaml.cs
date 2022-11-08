using Algoraph.Scripts;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Algoraph.Views
{
    public partial class GraphData : UserControl
    {
        readonly Editor ed;

        public GraphData(Editor ed)
        {
            InitializeComponent();
            this.ed = ed;

            nodeInfo.ApplyTemplate();
            arcInfo.ApplyTemplate();

            ((TextBox)nodeInfo.Template.FindName("textbox", nodeInfo)).KeyDown += NodeNameTextbox_KeyDown;
            ((Button)nodeInfo.Template.FindName("joinNodeButton", nodeInfo)).Click += JoinNode_Button;
            ((Button)nodeInfo.Template.FindName("deleteNodeButton", nodeInfo)).Click += DeleteNode_Button;

            ((Button)arcInfo.Template.FindName("deleteArcButton", arcInfo)).Click += DeleteArc_Button;
            ((TextBox)arcInfo.Template.FindName("textbox", arcInfo)).KeyDown += ArcWeightTextbox_KeyDown;
        }

        private void JoinNode_Button(object sender, RoutedEventArgs e)
        {
            ed.ConnectSelectedNodes();
        }

        private void DeleteNode_Button(object sender, RoutedEventArgs e)
        {
            ed.DeleteSelectedNodes();
        }



        private void DeleteArc_Button(object sender, RoutedEventArgs e)
        {
            ed.DeleteSelectedArcs();
        }


        private void NodeNameTextbox_KeyDown(object sender, KeyEventArgs e)
        {
            if (!(e.Key == Key.Enter)) return;
            TextBox textBox = (TextBox)sender;
            textBox.ApplyTemplate();
            TextBox inputText = ((TextBox)textBox.Template.FindName("inputText", textBox));

            if (inputText.Text.Length > 10 || inputText.Text.Contains(" "))
            {
                ed.ShowError("Unnacceptable name, please try again with a name less than 10 characters, inculding no spaces");
                return;
            }

            ed.ChangeNodeName(inputText.Text);
            inputText.Text = "";
        }

        private void ArcWeightTextbox_KeyDown(object sender, KeyEventArgs e)
        {
            if (!(e.Key == Key.Enter)) return;
            TextBox textBox = (TextBox)sender;
            textBox.ApplyTemplate();
            TextBox inputText = (TextBox)textBox.Template.FindName("inputText", textBox);

            if (uint.TryParse(inputText.Text, out uint weight))
            {
                ed.ChangeArcWeights(weight);
                inputText.Text = "";
            }  
        }
    }
}
