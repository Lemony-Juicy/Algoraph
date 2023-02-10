using Microsoft.Win32;
using System.Windows;


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

        public static bool OpenProject()
        {
            OpenFileDialog fileDialogue = new OpenFileDialog();
            fileDialogue.Title = "Open Algoraph Project";
            fileDialogue.Multiselect = false;

            fileDialogue.DefaultExt = ".json";
            fileDialogue.Filter = "JSON files (*.json)|*.json";

            // If user clicks OK button
            if (fileDialogue.ShowDialog() == true)
            {
                Editor editor = new Editor();
                editor.path = fileDialogue.FileName;

                if (!editor.LoadState())
                {
                    editor.Close();
                    Editor.ShowError("Oh no! This file is not able to be decoded into graph data :(\n" +
                        "Ensure that the json file selected contains the right data to be loaded.");
                    return false;
                }
                else
                {
                    editor.Title = fileDialogue.SafeFileName;
                    editor.Show();
                    return true;
                }
            }
            return false;
        }

        private void LoadProject_Button(object sender, RoutedEventArgs e)
        {
            if (OpenProject() == true)
            {
                this.Close();
            }
        }
    }
}
