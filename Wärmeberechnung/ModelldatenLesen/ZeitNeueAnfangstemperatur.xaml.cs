using FEBibliothek.Modell;
using System.Windows;

namespace FE_Berechnungen.Wärmeberechnung.ModelldatenLesen
{

    public partial class ZeitNeueAnfangstemperatur
    {
        private readonly FeModell modell;
        public ZeitNeueAnfangstemperatur(FeModell modell)
        {
            InitializeComponent();
            this.modell = modell;
            KnotenId.Text = string.Empty;
            Anfangstemperatur.Text = string.Empty;
            StationäreLösung.IsChecked = modell.Zeitintegration.VonStationär;
            Show();
        }
        private void BtnDialogOk_Click(object sender, RoutedEventArgs e)
        {
            if (StationäreLösung.IsChecked == true)
            {
                modell.Zeitintegration.VonStationär = true;
                modell.Zeitintegration.Anfangsbedingungen.Clear();
                Close();
                return;
            }

            var knotenId = KnotenId.Text;
            var anfang = new double[1];
            if (Anfangstemperatur.Text != string.Empty) { anfang[0] = double.Parse(Anfangstemperatur.Text); }
            modell.Zeitintegration.Anfangsbedingungen.Add(new Knotenwerte(knotenId, anfang));
            modell.Zeitintegration.VonStationär = false;
            Close();
        }

        private void BtnDialogCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
