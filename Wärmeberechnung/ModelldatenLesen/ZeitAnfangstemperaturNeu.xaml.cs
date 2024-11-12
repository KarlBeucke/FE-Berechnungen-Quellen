using FE_Berechnungen.Wärmeberechnung.ModelldatenAnzeigen;
using System.Globalization;

namespace FE_Berechnungen.Wärmeberechnung.ModelldatenLesen;

public partial class ZeitAnfangstemperaturNeu
{
    private readonly FeModell _modell;
    private int aktuell;

    public ZeitAnfangstemperaturNeu(FeModell modell)
    {
        _modell = modell;
        if (_modell.Zeitintegration == null)
        {
            _ = MessageBox.Show("Zeitintegration noch nicht definiert", "neue Anfangstemperatur");
            return;
        }
        InitializeComponent();
        _modell = modell;
        
        if (_modell.Zeitintegration.VonStationär)
        {
            StationäreLösung.IsChecked = true;
            KnotenId.Text = "";
            Anfangstemperatur.Text = "";
        }
        Show();
    }

    private void StationäreLösungChecked(object sender, RoutedEventArgs e)
    {
        StationäreLösung.IsChecked = true;
        _modell.Zeitintegration.VonStationär = true;
        KnotenId.Text = "";
        Anfangstemperatur.Text = "";
    }

    private void BtnDialogOk_Click(object sender, RoutedEventArgs e)
    {
        if (StationäreLösung.IsChecked == true)
        {
            _modell.Zeitintegration.VonStationär = true;
            _modell.Zeitintegration.Anfangsbedingungen.Clear();
        }
        else
        {
            aktuell = _modell.Zeitintegration.Anfangsbedingungen.FindIndex((a => a.KnotenId == KnotenId.Text));
            // neue Anfangsbedingung hinzufügen
            var werte = new double[1];
            if (aktuell < 0)
            {
                if (KnotenId.Text != "")
                {
                    try
                    {
                        werte[0] = double.Parse(Anfangstemperatur.Text);
                    }
                    catch (FormatException)
                    {
                        _ = MessageBox.Show("ungültiges  Eingabeformat", "neue Anfangstemperatur");
                    }
                }
                else
                {
                    _ = MessageBox.Show("Knoten Id muss definiert sein für neue Anfangstemperatur", "neue Anfangstemperatur");
                    return;
                }

                var knotenwerte = new Knotenwerte(KnotenId.Text, werte);
                _modell.Zeitintegration.Anfangsbedingungen.Add(knotenwerte);
                _modell.Zeitintegration.VonStationär = false;
            }

            // vorhandene Anfangstemperatur ändern
            else
            {
                var anfang = _modell.Zeitintegration.Anfangsbedingungen[aktuell];
                anfang.KnotenId = KnotenId.Text;
                try
                {
                    anfang.Werte[0] = double.Parse(Anfangstemperatur.Text);
                    _modell.Zeitintegration.Anfangsbedingungen[aktuell] = anfang;
                }
                catch (FormatException)
                {
                    _ = MessageBox.Show("ungültiges  Eingabeformat", "neue Anfangstemperatur");
                }
            }
            StartFenster.WärmeVisual.IsAnfangsbedingung = true;
        }
        Close();
        StartFenster.WärmeVisual.Close();
        StartFenster.WärmeVisual = new WärmemodellVisualisieren(_modell);
        StartFenster.WärmeVisual.Show();
    }

    private void BtnDialogCancel_Click(object sender, RoutedEventArgs e)
    {
        Close();
        StartFenster.WärmeVisual.ZeitintegrationNeu?.Close();
    }

    private void BtnDelete_Click(object sender, RoutedEventArgs e)
    {
        if (StationäreLösung.IsChecked != null && (bool)StationäreLösung.IsChecked)
        {
            _modell.Zeitintegration.VonStationär = false;
            _modell.Zeitintegration.Anfangsbedingungen.Clear();
            StartFenster.WärmeVisual.Darstellung.AnfangsbedingungenEntfernen();
        }
        else if(aktuell >= 0)
        {
            _modell.Zeitintegration.Anfangsbedingungen.RemoveAt(aktuell);
            if (_modell.Zeitintegration.Anfangsbedingungen.Count <= 0)
            {
                Close();
                StartFenster.WärmeVisual.ZeitintegrationNeu.Close();
                return;
            }

            var anfang = _modell.Zeitintegration.Anfangsbedingungen[aktuell];
            KnotenId.Text = anfang.KnotenId;
            Anfangstemperatur.Text = anfang.Werte[0].ToString("G2");
            StationäreLösung.IsChecked = _modell.Zeitintegration.VonStationär;
        }
        Close();
        StartFenster.WärmeVisual.Close();
        StartFenster.WärmeVisual = new WärmemodellVisualisieren(_modell);
        StartFenster.WärmeVisual.Show();
    }

    private void KnotenIdLostFocus(object sender, RoutedEventArgs e)
    {
        aktuell = _modell.Zeitintegration.Anfangsbedingungen.FindIndex((a => a.KnotenId == KnotenId.Text));
        if (aktuell < 0 )
        {
            KnotenId.Text = "";
            Anfangstemperatur.Text = "";
        }

        // vorhandene Anfangstemperatur
        else
        {
            Anfangstemperatur.Text = _modell.Zeitintegration.Anfangsbedingungen[aktuell].Werte[0].ToString("G3", CultureInfo.CurrentCulture);
        }
    }
}