namespace FE_Berechnungen.Wärmeberechnung.ModelldatenLesen;

public partial class ZeitAnfangstemperaturNeu
{
    private readonly FeModell _modell;
    private int aktuell;

    public ZeitAnfangstemperaturNeu(FeModell modell)
    {
        InitializeComponent();
        _modell = modell;
        aktuell = StartFenster.WärmeVisual.ZeitintegrationNeu.aktuell;
        if (modell.Zeitintegration.VonStationär)
        {
            StationäreLösung.IsChecked = true;
            KnotenId.Text = "";
            Anfangstemperatur.Text = "";
        }
        else
        {
            var anfang = (Knotenwerte)modell.Zeitintegration.Anfangsbedingungen[aktuell];
            KnotenId.Text = anfang.KnotenId;
            Anfangstemperatur.Text = anfang.Werte[0].ToString("G2");
        }

        Show();
    }

    private void StationäreLösungChecked(object sender, RoutedEventArgs e)
    {
        StationäreLösung.IsChecked = true;
        KnotenId.Text = "";
        Anfangstemperatur.Text = "";
    }

    private void BtnDialogOk_Click(object sender, RoutedEventArgs e)
    {
        if (KnotenId.Text.Length == 0) Close();
        if (StationäreLösung.IsChecked == true)
        {
            _modell.Zeitintegration.VonStationär = true;
            _modell.Zeitintegration.Anfangsbedingungen.Clear();
            Close();
            return;
        }

        // neue Anfangsbedingung hinzufügen
        if (StartFenster.WärmeVisual.ZeitintegrationNeu.aktuell > _modell.Zeitintegration.Anfangsbedingungen.Count)
        {
            if (KnotenId.Text == "") return;
            var werte = new double[1];
            try
            {
                werte[0] = double.Parse(Anfangstemperatur.Text);
            }
            catch (FormatException)
            {
                _ = MessageBox.Show("ungültiges  Eingabeformat", "neue Anfangstemperatur");
            }

            var knotenwerte = new Knotenwerte(KnotenId.Text, werte);
            _modell.Zeitintegration.Anfangsbedingungen.Add(knotenwerte);
            _modell.Zeitintegration.VonStationär = false;
        }

        // vorhandene Anfangsbedingung ändern
        else
        {
            var anfang = (Knotenwerte)_modell.Zeitintegration.Anfangsbedingungen[aktuell];
            anfang.KnotenId = KnotenId.Text;
            try
            {
                anfang.Werte[0] = double.Parse(Anfangstemperatur.Text);
            }
            catch (FormatException)
            {
                _ = MessageBox.Show("ungültiges  Eingabeformat", "neue Anfangstemperatur");
            }
        }

        Close();
    }

    private void BtnDialogCancel_Click(object sender, RoutedEventArgs e)
    {
        Close();
        StartFenster.WärmeVisual.ZeitintegrationNeu.Close();
    }

    private void BtnDelete_Click(object sender, RoutedEventArgs e)
    {
        _modell.Zeitintegration.Anfangsbedingungen.RemoveAt(aktuell + 1);
        aktuell = 0;
        if (_modell.Zeitintegration.Anfangsbedingungen.Count <= 0)
        {
            Close();
            StartFenster.WärmeVisual.ZeitintegrationNeu.Close();
            return;
        }

        var anfang = (Knotenwerte)_modell.Zeitintegration.Anfangsbedingungen[aktuell];
        KnotenId.Text = anfang.KnotenId;
        Anfangstemperatur.Text = anfang.Werte[0].ToString("G2");
        StationäreLösung.IsChecked = _modell.Zeitintegration.VonStationär;

        Close();
        StartFenster.WärmeVisual.ZeitintegrationNeu.Close();
    }
}