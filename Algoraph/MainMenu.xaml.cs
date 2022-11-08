using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Algoraph
{
    public partial class MainMenu : Window
    {
        public MainMenu()
        {
            InitializeComponent();
        }

        private void CreateNew_Button(object sender, RoutedEventArgs e)
        {
            Editor editor = new Editor();
            editor.Show();
            Close();
        }

        private void LoadProject_Button(object sender, RoutedEventArgs e)
        {

        }
    }
}
