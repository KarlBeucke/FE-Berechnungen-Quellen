using FE_Berechnungen.Wärmeberechnung.Modelldaten;
using FE_Berechnungen.Wärmeberechnung.ModelldatenAnzeigen;
using System.Globalization;
using System.Windows.Input;

namespace FE_Berechnungen.Wärmeberechnung.ModelldatenLesen;

public partial class ZeitintegrationNeu
{
    private readonly FeModell _modell;
    private ZeitKnotenAnfangstemperaturNeu _anfangstemperaturenNeu;
    private int _aktuell;

    public ZeitintegrationNeu(FeModell modell)
    {
        Language = XmlLanguage.GetLanguage("de-DE");
        InitializeComponent();
        _modell = modell;
        _aktuell = 1;
        if (modell.Eigenzustand == null) { modell.Eigenzustand = new Eigenzustände("eigen", 1); }
        else
        {
            Eigenlösung.Text = modell.Eigenzustand.AnzahlZustände.ToString(CultureInfo.CurrentCulture);
        }
        if (modell.Zeitintegration == null)
        {
            modell.Zeitintegration = new Zeitintegration(0, 0, 0);
        }
        else
        {
            Zeitintervall.Text = _modell.Zeitintegration.Dt.ToString(CultureInfo.CurrentCulture);
            Maximalzeit.Text = _modell.Zeitintegration.Tmax.ToString(CultureInfo.CurrentCulture);
            Parameter.Text = _modell.Zeitintegration.Parameter1.ToString(CultureInfo.CurrentCulture);
            Gesamt.Text = modell.Zeitintegration.Anfangsbedingungen.Count.ToString(CultureInfo.CurrentCulture);
        }
        Show();
    }

    private void ZeitintervallBerechnen(object sender, MouseButtonEventArgs e)
    {
        var anzahl = 0;

        try
        {
            anzahl = int.Parse(Eigenlösung.Text, CultureInfo.CurrentCulture);
        }
        catch (FormatException)
        {
            _ = MessageBox.Show("Anzahl Eigenlösungen hat falsches Format", "ZeitintegrationNeu");
        }

        var modellBerechnung = new Berechnung(_modell);
        _modell.Eigenzustand ??= new Eigenzustände("Tmin", anzahl);

        if (_modell.Eigenzustand.Eigenwerte == null)
        {
            if (!_modell.Berechnet)
            {
                modellBerechnung.BerechneSystemMatrix();
                _modell.Berechnet = true;
            }

            _modell.Eigenzustand = new Eigenzustände("Tmin", anzahl);
            modellBerechnung.Eigenzustände();
        }

        var alfa = double.Parse(Parameter.Text);
        var betaMax = _modell.Eigenzustand.Eigenwerte[anzahl - 1];
        if (betaMax == 0)
        {
            _ = MessageBox.Show("Eigenwerte = 0, Δt kritisch nicht berechenbar", "ZeitintegrationNeu");
        }
        else
        {
            if (alfa < 0.5)
            {
                var deltatkrit = 2 / (betaMax * (1 - 2 * alfa));
                var deltatkritText = deltatkrit.ToString("g4", CultureInfo.CurrentCulture);
                Zeitintervall.Text = deltatkritText;
                var betaM = betaMax.ToString("g4", CultureInfo.CurrentCulture);
                _ = MessageBox.Show("Lösung stabil für Δt < " + deltatkritText + " mit größtem Eigenwert = " + betaM, "ZeitintegrationNeu");
            }
            else
            {
                Zeitintervall.Text = _modell.Zeitintegration.Dt.ToString("g2", CultureInfo.CurrentCulture);
                _ = MessageBox.Show("Lösung ist unbedingt stabil, Δt kann gewählt werden für Genauigkeit", "ZeitintegrationNeu");
            }
        }
    }

    private void AnfangsbedingungNext(object sender, MouseButtonEventArgs e)
    {
        // Aktuell beinhaltet die aktuelle Nummer der Anfangsbedingung in Bearbeitung
        if (string.IsNullOrEmpty(Anfangsbedingungen.Text)) _aktuell = 1;
        else if (int.Parse(Anfangsbedingungen.Text) < _modell.Zeitintegration.Anfangsbedingungen.Count) _aktuell++;
        else
        {
            _ = MessageBox.Show("keine weiteren Anfangsbedingungen definiert", "ZeitintegrationNeu");
            return;
        }
        Anfangsbedingungen.Text = _aktuell.ToString();
        _anfangstemperaturenNeu = new ZeitKnotenAnfangstemperaturNeu(_modell, _aktuell, true) { Topmost = true };
        if (_aktuell > _modell.Zeitintegration.Anfangsbedingungen.Count) _aktuell = _modell.Zeitintegration.Anfangsbedingungen.Count;
        Anfangsbedingungen.Text = _aktuell.ToString();
        Gesamt.Text = (_modell.Zeitintegration.Anfangsbedingungen.Count).ToString();
        _anfangstemperaturenNeu.Close();
    }
    private void AnfangsbedingungEdit(object sender, KeyEventArgs e)
    {
        _aktuell = int.Parse(Anfangsbedingungen.Text);
        _anfangstemperaturenNeu = new ZeitKnotenAnfangstemperaturNeu(_modell, _aktuell, true) { Topmost = true };
    }

    private void BtnDialogOk_Click(object sender, RoutedEventArgs e)
    {
        if (Zeitintervall.Text == "")
        {
            _ = MessageBox.Show(
                "kritischer Zeitschritt frei wählbar für Stabilität und muss gewählt werden für Genauigkeit",
                "ZeitintegrationNeu");
            return;
        }

        if (_modell.Zeitintegration == null)
        {
            int anzahlEigenlösungen;
            double dt, tmax, alfa;

            try
            {
                dt = double.Parse(Zeitintervall.Text, CultureInfo.CurrentCulture);
            }
            catch (FormatException)
            {
                _ = MessageBox.Show("Zeitintervall der Integration Δt hat falsches Format", "ZeitintegrationNe");
                return;
            }

            try
            {
                tmax = double.Parse(Maximalzeit.Text, CultureInfo.CurrentCulture);
            }
            catch (FormatException)
            {
                _ = MessageBox.Show("maximale Integrationszeit Tmax hat falsches Format", "ZeitintegrationNeu");
                return;
            }
            try
            {
                anzahlEigenlösungen = int.Parse(Eigenlösung.Text, CultureInfo.CurrentCulture);
            }
            catch (FormatException)
            {
                _ = MessageBox.Show("Anzahl Eigenlösungen hat falsches Format", "ZeitintegrationNeu");
                return;
            }
            try
            {
                alfa = double.Parse(Parameter.Text, CultureInfo.CurrentCulture);
            }
            catch (FormatException)
            {
                _ = MessageBox.Show("Parameter alfa hat falsches Format", "ZeitintegrationNeu");
                return;
            }
            _modell.Eigenzustand = new Eigenzustände("eigen", anzahlEigenlösungen);
            _modell.Zeitintegration = new Zeitintegration(tmax, dt, alfa) { VonStationär = false };
            _modell.ZeitintegrationDaten = true;
        }

        else
        {
            try
            {
                _modell.Zeitintegration.Dt = double.Parse(Zeitintervall.Text, CultureInfo.CurrentCulture);
            }
            catch (FormatException)
            {
                _ = MessageBox.Show("Zeitschritt deltaT hat falsches Format", "ZeitintegrationNeu");
            }

            try
            {
                _modell.Zeitintegration.Tmax = double.Parse(Maximalzeit.Text);
            }
            catch (FormatException)
            {
                _ = MessageBox.Show("maximale Integrationszeit hat falsches Format", "ZeitintegrationNeu");
            }

            try
            {
                _modell.Eigenzustand.AnzahlZustände = int.Parse(Eigenlösung.Text, CultureInfo.CurrentCulture);
            }
            catch (FormatException)
            {
                _ = MessageBox.Show("Anzahl Eigenlösungen hat falsches Format", "ZeitintegrationNeu");
            }

            try
            {
                _modell.Zeitintegration.Parameter1 = int.Parse(Eigenlösung.Text, CultureInfo.CurrentCulture);
            }
            catch (FormatException)
            {
                _ = MessageBox.Show("Parameter alfa hat das falsche Format", "ZeitintegrationNeu");
            }
        }

        _anfangstemperaturenNeu?.Close();
        Close();
        StartFenster.WärmeVisual.Close();
        StartFenster.WärmeVisual = new WärmemodellVisualisieren(_modell);
        StartFenster.WärmeVisual.Show();
    }

    private void BtnDialogCancel_Click(object sender, RoutedEventArgs e)
    {
        _aktuell -= 1;
        Close();
    }
}