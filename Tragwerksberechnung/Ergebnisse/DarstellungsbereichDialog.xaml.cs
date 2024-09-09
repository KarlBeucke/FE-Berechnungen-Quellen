using System.Globalization;

namespace FE_Berechnungen.Tragwerksberechnung.Ergebnisse;

public partial class DarstellungsbereichDialog
{
    public double maxBeschleunigung;
    public double maxVerformung;
    public double tmin, tmax;

    public DarstellungsbereichDialog(double tmin, double tmax, double maxVerformung, double maxBeschleunigung)
    {
        InitializeComponent();
        this.tmin = tmin;
        this.tmax = tmax;
        this.maxVerformung = maxVerformung;
        this.maxBeschleunigung = maxBeschleunigung;
        //TxtMinZeit.Text = this.tmin.ToString(CultureInfo.CurrentCulture);
        TxtMaxZeit.Text = this.tmax.ToString(CultureInfo.CurrentCulture);
        TxtMaxVerformung.Text = this.maxVerformung.ToString("N4");
        TxtMaxBeschleunigung.Text = this.maxBeschleunigung.ToString("N4");
        ShowDialog();
    }

    private void BtnDialogOk_Click(object sender, RoutedEventArgs e)
    {
        //tmin = double.Parse(TxtMinZeit.Text);
        tmax = double.Parse(TxtMaxZeit.Text);
        maxVerformung = double.Parse(TxtMaxVerformung.Text);
        maxBeschleunigung = double.Parse(TxtMaxBeschleunigung.Text);
        Close();
    }

    private void BtnDialogCancel_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}