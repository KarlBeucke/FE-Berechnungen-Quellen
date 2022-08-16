using Microsoft.Win32;
using System.IO;
using System.Windows;

namespace FE_Berechnungen.Dateieingabe
{
    public partial class ModelldatenEditieren : Window
    {
        public ModelldatenEditieren()
        {
            InitializeComponent();
            OpenFileDialog openFileDialog = new OpenFileDialog { Filter = "Eingabedateien (*.inp)|*.inp" };
            if (openFileDialog.ShowDialog() == true)
                txtEditor.Text = File.ReadAllText(openFileDialog.FileName);
        }
        public ModelldatenEditieren(string path)
        {
            InitializeComponent();
            txtEditor.Text = File.ReadAllText(path);
        }
        private void BtnOpenFileClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog { Filter = "Eingabedateien (*.inp)|*.inp" };
            if (openFileDialog.ShowDialog() == true)
                txtEditor.Text = File.ReadAllText(openFileDialog.FileName);
        }
        private void BtnSaveFile_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog { Filter = "Eingabedateien (*.inp)|*.inp" };
            if (saveFileDialog.ShowDialog() == true)
                File.WriteAllText(saveFileDialog.FileName, txtEditor.Text);
        }
    }
}
