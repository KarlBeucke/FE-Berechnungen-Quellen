using System.Globalization;

namespace FE_Berechnungen.Wärmeberechnung.Ergebnisse;

public partial class Darstellungsbereich
{
    public double MaxTemperatur;
    public double MaxWärmefluss;
    public readonly double Tmin;
    public double Tmax;

    public Darstellungsbereich(double tmin, double tmax, double maxTemperatur, double maxWärmefluss)
    {
        InitializeComponent();
        this.Tmin = tmin;
        this.Tmax = tmax;
        this.MaxTemperatur = maxTemperatur;
        this.MaxWärmefluss = maxWärmefluss;
        //TxtMinZeit.Text = this.tmin.ToString(CultureInfo.CurrentCulture);
        TxtMaxZeit.Text = this.Tmax.ToString(CultureInfo.CurrentCulture);
        TxtMaxTemperatur.Text = this.MaxTemperatur.ToString("N2");
        TxtMaxWärmefluss.Text = this.MaxWärmefluss.ToString("N2");
        ShowDialog();
    }

    private void BtnDialogOk_Click(object sender, RoutedEventArgs e)
    {
        //tmin = double.Parse(TxtMinZeit.Text);
        Tmax = double.Parse(TxtMaxZeit.Text);
        MaxTemperatur = double.Parse(TxtMaxTemperatur.Text);
        MaxWärmefluss = double.Parse(TxtMaxWärmefluss.Text);
        Close();
    }

    private void BtnDialogCancel_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}