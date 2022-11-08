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

        private void CreateOrdered_Button(object sender, RoutedEventArgs e)
        {
            string s = orderedNodesDegreeInput.Text;
            debug.Text = s;
            if (int.TryParse(s, out int degree))
                ed.CreateOrdered(degree);
            else
                ed.ShowError("Please try again with an appropriate INTEGER input.");
        }

        private void CreateRandom_Button(object sender, RoutedEventArgs e)
        {
            if (TryParseRandNodeInp(out int[] rands))
                ed.CreateRandom(rands[0], rands[1]);
            else
                ed.ShowError("Ensure that the random range of nodes to be plotted is in the format [R1, R2] where R1 and R2 are both integers.");
        }

        private void ClearGraph_Button(object sender, RoutedEventArgs e)
        {
            ed.ClearGraph();
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


    }
}
