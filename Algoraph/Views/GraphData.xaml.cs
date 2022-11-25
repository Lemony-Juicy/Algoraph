using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Data;

namespace Algoraph.Views
{
    public partial class GraphData : UserControl
    {
        readonly Editor ed;
        public DataTable table { get; private set; }

        public GraphData(Editor ed)
        {
            InitializeComponent();
            this.ed = ed;

            table = new();
            CreateColumns();
            adjDataGrid.ItemsSource = table.DefaultView;
        }

        void CreateColumns()
        {
            DataColumn column = new();
            column.DataType = typeof(string);
            column.ColumnName = "Nodes";
            column.AutoIncrement = false;
            column.Unique = false;
            table.Columns.Add(column);

            column = new();
            column.DataType = typeof(string);
            column.ColumnName = "Adjacencies";
            column.AutoIncrement = false;
            column.Unique = false;
            table.Columns.Add(column);
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

            if (inputText.Text.Length > 10 || inputText.Text.Contains(" "))
            {
                ed.ShowError("Unnacceptable name, please try again with a name less than 10 characters, inculding no spaces");
                return;
            }

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
                ed.ChangeArcWeights(weight);
            }
        }

        #endregion
    }
}
