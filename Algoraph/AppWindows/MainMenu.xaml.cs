using Algoraph.Scripts;
using Microsoft.Win32;
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
            Editor editor;
            OpenFileDialog fileDialogue = new OpenFileDialog();
            fileDialogue.DefaultExt = ".json";
            fileDialogue.Filter = "JSON files (*.json)|*.json";
            if (fileDialogue.ShowDialog() == true)
            {
                Saver.path = fileDialogue.FileName;
                editor = new Editor();

                if (!editor.LoadState())
                {
                    editor.Close();
                    Editor.ShowError("Oh no! This file is not able to be decoded into graph data :(\n" +
                        "Ensure that the json file selected contains the right data to be loaded.");
                }
                else
                {
                    this.Close();
                    editor.Show();
                }
            }
        }
    }
}
