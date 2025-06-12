using FE_Berechnungen.Wärmeberechnung.Modelldaten;
using FE_Berechnungen.Wärmeberechnung.ModelldatenAnzeigen;
using System.Globalization;

namespace FE_Berechnungen.Wärmeberechnung.ModelldatenLesen;

public partial class ZeitKnotenAnfangstemperaturNeu
{
    private readonly FeModell _modell;
    private int _aktuell;
    private readonly string _knotenIdSave;
    private readonly bool _knotenIdFixed;

    public ZeitKnotenAnfangstemperaturNeu(FeModell modell)
    {
        InitializeComponent();
        _modell = modell;
        _aktuell = modell.Zeitintegration.Anfangsbedingungen.Count + 1;
        ShowDialog();
    }
    public ZeitKnotenAnfangstemperaturNeu(FeModell modell, int aktuell, bool knotenIdFixed)
    {
        InitializeComponent();
        _modell = modell;
        _aktuell = aktuell;
        _knotenIdFixed = knotenIdFixed;
        modell.Zeitintegration ??= new Zeitintegration(0, 0, 0);

        if (_modell.Zeitintegration.VonStationär)
        {
            StationäreLösung.IsChecked = true;
            KnotenId.Text = "";
            Anfangstemperatur.Text = "";
        }
        else
        {
            KnotenId.Text = _modell.Zeitintegration.Anfangsbedingungen[aktuell - 1].KnotenId
                .ToString(CultureInfo.CurrentCulture);
            _knotenIdSave = KnotenId.Text;
            Anfangstemperatur.Text = _modell.Zeitintegration.Anfangsbedingungen[aktuell - 1].Werte[0]
                .ToString(CultureInfo.CurrentCulture);
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
                    StartFenster.WärmeVisual.IsAnfangsbedingung = true;
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
                var anfang = _modell.Zeitintegration.Anfangsbedingungen[_aktuell - 1];
                anfang.KnotenId = KnotenId.Text;
                try
                {
                    if (Anfangstemperatur.Text != string.Empty) anfang.Werte[0] = double.Parse(Anfangstemperatur.Text);
                    _modell.Zeitintegration.Anfangsbedingungen[_aktuell - 1] = anfang;
                }
                catch (FormatException)
                {
                    _ = MessageBox.Show("ungültiges  Eingabeformat", "neue Anfangstemperatur");
                }
            }
            StartFenster.WärmeVisual.IsAnfangsbedingung = true;
        }
        Close();
        if (_knotenIdFixed) return;
        StartFenster.WärmeVisual.Close();
        StartFenster.WärmeVisual = new WärmemodellVisualisieren(_modell);
        StartFenster.WärmeVisual.Show();
        _modell.Berechnet = false;
    }

    private void BtnDialogCancel_Click(object sender, RoutedEventArgs e)
    {
        StartFenster.WärmeVisual.ZeitintegrationNeu?.Close();
        StartFenster.WärmeVisual.IsAnfangsbedingung = false;
        Close();
    }

    private void BtnLöschen_Click(object sender, RoutedEventArgs e)
    {
        _modell.Zeitintegration.Anfangsbedingungen.RemoveAt(_aktuell);
        Close();
        StartFenster.WärmeVisual.Close();
        StartFenster.WärmeVisual = new WärmemodellVisualisieren(_modell);
        StartFenster.WärmeVisual.Show();
    }

    private void KnotenIdLostFocus(object sender, RoutedEventArgs e)
    {
        if (_knotenIdFixed)
        {
            _ = MessageBox.Show("KnotenId kann hier nicht geändert werden", "ZeitKnotenAnfangstemperaturNNeu");
            KnotenId.Text = _knotenIdSave;
            return;
        }
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

    private void KnotenPositionNeu(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        _modell.Knoten.TryGetValue(KnotenId.Text, out var knoten);
        if (knoten == null) { _ = MessageBox.Show("Knoten nicht im Modell gefunden", "neue zeitabhängige Anfangstemperatur"); return; }
        StartFenster.WärmeVisual.KnotenEdit(knoten);
        Close();
        _modell.Berechnet = false;
    }
}