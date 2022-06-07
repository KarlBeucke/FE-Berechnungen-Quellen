using FEBibliothek.Modell;
using System.Windows;

namespace FE_Berechnungen.Tragwerksberechnung.ModelldatenLesen
{
    public partial class NeuerQuerschnitt : Window
    {
        private readonly FEModell modell;
        public NeuerQuerschnitt(FEModell modell)
        {
            InitializeComponent();
            this.modell = modell;
            Show();
        }

        private void BtnDialogOk_Click(object sender, RoutedEventArgs e)
        {
            var querschnittId = QuerschnittId.Text;
            double fläche = 0, ixx = 0;
            if (Fläche.Text != string.Empty) { fläche = double.Parse(Fläche.Text); }
            if (Ixx.Text != string.Empty) { ixx = double.Parse(Ixx.Text); }
            {
                var querschnitt = new Querschnitt(fläche, ixx) { QuerschnittId = querschnittId };
                modell.Querschnitt.Add(querschnittId, querschnitt);
            }
            Close();
        }

        private void BtnDialogCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
