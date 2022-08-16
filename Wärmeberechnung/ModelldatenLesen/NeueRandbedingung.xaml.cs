using FEBibliothek.Modell;
using System.Windows;

namespace FE_Berechnungen.Wärmeberechnung.ModelldatenLesen
{
    public partial class NeueRandbdingung : Window
    {
        private readonly FeModell modell;

        public NeueRandbdingung(FeModell modell)
        {
            this.modell = modell;
            InitializeComponent();
            RandbedingungId.Text = string.Empty;
            KnotenId.Text = string.Empty;
            Temperatur.Text = string.Empty;
            Show();
        }

        private void BtnDialogOk_Click(object sender, RoutedEventArgs e)
        {
            var randbedingungId = RandbedingungId.Text;
            var knotenId = KnotenId.Text;
            double temperatur = 0;
            if (Temperatur.Text != string.Empty) { temperatur = double.Parse(Temperatur.Text); }

            var randbedingung = new Modelldaten.Randbedingung(randbedingungId, knotenId, temperatur);

            modell.Randbedingungen.Add(randbedingungId, randbedingung);
            Close();
        }

        private void BtnDialogCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
