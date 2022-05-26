using System.Globalization;
using System.Windows;

namespace FE_Berechnungen.Wärmeberechnung.Ergebnisse
{
    public partial class Darstellungsbereich
    {
        public double tmin, tmax;
        public double maxTemperatur;
        public double maxWärmefluss;

        public Darstellungsbereich(double tmin, double tmax, double maxTemperatur, double maxWärmefluss)
        {
            InitializeComponent();
            this.tmin = tmin;
            this.tmax = tmax;
            this.maxTemperatur = maxTemperatur;
            this.maxWärmefluss = maxWärmefluss;
            //TxtMinZeit.Text = this.tmin.ToString(CultureInfo.CurrentCulture);
            TxtMaxZeit.Text = this.tmax.ToString(CultureInfo.CurrentCulture);
            TxtMaxTemperatur.Text = this.maxTemperatur.ToString("N2");
            TxtMaxWärmefluss.Text = this.maxWärmefluss.ToString("N2");
            ShowDialog();
        }

        private void BtnDialogOk_Click(object sender, RoutedEventArgs e)
        {
            //tmin = double.Parse(TxtMinZeit.Text);
            tmax = double.Parse(TxtMaxZeit.Text);
            maxTemperatur = double.Parse(TxtMaxTemperatur.Text);
            maxWärmefluss = double.Parse(TxtMaxWärmefluss.Text);
            Close();
        }
        private void BtnDialogCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
