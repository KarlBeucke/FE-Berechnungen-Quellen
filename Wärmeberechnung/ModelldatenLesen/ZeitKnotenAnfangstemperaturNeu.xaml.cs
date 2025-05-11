using FE_Berechnungen.Wärmeberechnung.Modelldaten;
using FE_Berechnungen.Wärmeberechnung.ModelldatenAnzeigen;
using System.Globalization;

namespace FE_Berechnungen.Wärmeberechnung.ModelldatenLesen;

public partial class ZeitKnotenAnfangstemperaturNeu
{
    private readonly FeModell _modell;
    private int _aktuell;

    public ZeitKnotenAnfangstemperaturNeu(FeModell modell)
    {
        InitializeComponent();
        _modell = modell;
        modell.Zeitintegration ??= new Zeitintegration(0, 0, 0);
        StartFenster.WärmeVisual.ZeitintegrationNeu ??= new ZeitintegrationNeu(_modell);

        if (_modell.Zeitintegration.VonStationär)
        {
            StationäreLösung.IsChecked = true;
            KnotenId.Text = "";
            Anfangstemperatur.Text = "";
        }

        if (_modell.Zeitintegration.Anfangsbedingungen.Count != 0)
        {
            var anfang = _modell.Zeitintegration.Anfangsbedingungen[0];
            KnotenId.Text = anfang.KnotenId;
            Anfangstemperatur.Text = anfang.Werte[0].ToString("G2", CultureInfo.CurrentCulture);
        }
        Show();
    }
    public ZeitKnotenAnfangstemperaturNeu(FeModell modell, int aktuell)
    {
        InitializeComponent();
        _modell = modell;
        _aktuell = aktuell;
        modell.Zeitintegration ??= new Zeitintegration(0, 0, 0);

        if (_modell.Zeitintegration.VonStationär)
        {
            StationäreLösung.IsChecked = true;
            KnotenId.Text = "";
            Anfangstemperatur.Text = "";
        }
        else
        {
            if (_aktuell > 0 && modell.Zeitintegration.Anfangsbedingungen.Count >= _aktuell)
            {
                var anfang = modell.Zeitintegration.Anfangsbedingungen[_aktuell - 1];
                KnotenId.Text = anfang.KnotenId;
                Anfangstemperatur.Text = anfang.Werte[0].ToString("G2");
            }
        }
        ShowDialog();
    }

    private void StationäreLösungChecked(object sender, RoutedEventArgs e)
    {
        if (StationäreLösung.IsChecked == true)
        {
            _modell.Zeitintegration.VonStationär = true;
            KnotenId.Text = "";
            Anfangstemperatur.Text = "";
        }
        else
        {
            _modell.Zeitintegration.VonStationär = false;
        }
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
            // neue Anfangsbedingung hinzufügen
            if (_aktuell > _modell.Zeitintegration.Anfangsbedingungen.Count)
            {
                var knotenId = KnotenId.Text;
                if (_modell.Knoten.TryGetValue(knotenId, out _))
                {
                    var anfangsWert = new double[1];
                    try
                    {
                        if (Anfangstemperatur.Text != string.Empty) anfangsWert[0] = double.Parse(Anfangstemperatur.Text);
                    }
                    catch (FormatException)
                    {
                        _ = MessageBox.Show("ungültiges  Eingabeformat", "neue ZeitKnotenAnfangstemperatur");
                    }
                    _modell.Zeitintegration.Anfangsbedingungen.Add(new Knotenwerte(KnotenId.Text, anfangsWert));
                    _modell.Zeitintegration.VonStationär = false;
                }
                else
                {
                    _ = MessageBox.Show("Knotennummer muss definiert sein", "neue ZeitKnotenAnfangstemperatur");
                    return;
                }
            }

            // vorhandene Anfangstemperatur ändern
            else
            {
                var anfang = _modell.Zeitintegration.Anfangsbedingungen[_aktuell];
                anfang.KnotenId = KnotenId.Text;
                try
                {
                    if (Anfangstemperatur.Text != string.Empty) anfang.Werte[0] = double.Parse(Anfangstemperatur.Text);
                    _modell.Zeitintegration.Anfangsbedingungen[_aktuell] = anfang;
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

    private void BtnLöschen_Click(object sender, RoutedEventArgs e)
    {
        _modell.Zeitintegration.Anfangsbedingungen.RemoveAt(_aktuell - 1);
        _aktuell = 0;
        if (StationäreLösung.IsChecked != null && (bool)StationäreLösung.IsChecked)
        {
            _modell.Zeitintegration.VonStationär = false;
            _modell.Zeitintegration.Anfangsbedingungen.Clear();
            StartFenster.WärmeVisual.Darstellung.AnfangsbedingungenEntfernen();
        }
        else if (_modell.Zeitintegration.Anfangsbedingungen.Count <= 0)
        {
            Close();
            StartFenster.WärmeVisual.ZeitintegrationNeu?.Close();
            return;
        }

        var anfangsWerte = _modell.Zeitintegration.Anfangsbedingungen[_aktuell];
        KnotenId.Text = anfangsWerte.KnotenId;
        Anfangstemperatur.Text = anfangsWerte.Werte[0].ToString("G2");

        Close();
        StartFenster.WärmeVisual.ZeitintegrationNeu?.Close();
    }

    private void KnotenIdLostFocus(object sender, RoutedEventArgs e)
    {
        var knotenId = KnotenId.Text;
        for (var i = 0; i < _modell.Zeitintegration.Anfangsbedingungen.Count; i++)
        {
            if (_modell.Zeitintegration.Anfangsbedingungen[i].KnotenId != knotenId) continue;
            var anfangsWerte = _modell.Zeitintegration.Anfangsbedingungen[i];
            Anfangstemperatur.Text = anfangsWerte.Werte[0].ToString("G2", CultureInfo.CurrentCulture);
            _aktuell = i + 1;
            return;
        }

        _aktuell = _modell.Zeitintegration.Anfangsbedingungen.Count + 1;
        Anfangstemperatur.Text = "";
    }
}