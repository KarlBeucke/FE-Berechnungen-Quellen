using System;
using System.Globalization;
using System.Windows;
using System.Windows.Markup;
using FE_Berechnungen.Wärmeberechnung.Modelldaten;
using FEBibliothek.Modell;

namespace FE_Berechnungen.Wärmeberechnung.ModelldatenLesen;

public partial class ZeitintegrationNeu
{
    private readonly FeModell modell;
    public int aktuell;
    private ZeitAnfangstemperaturNeu anfangstemperaturenNeu;
    public ZeitintegrationNeu(FeModell modell)
    {
        Language = XmlLanguage.GetLanguage("de-DE");
        InitializeComponent();
        this.modell = modell;
        if (modell.Eigenzustand != null)
            Eigenlösung.Text = modell.Eigenzustand.AnzahlZustände.ToString(CultureInfo.CurrentCulture);
        if (modell.Zeitintegration != null)
        {
            Zeitintervall.Text = modell.Zeitintegration.Dt.ToString(CultureInfo.CurrentCulture);
            Maximalzeit.Text = modell.Zeitintegration.Tmax.ToString(CultureInfo.CurrentCulture);
            Parameter.Text = modell.Zeitintegration.Parameter1.ToString(CultureInfo.CurrentCulture);
            Gesamt.Text = modell.Zeitintegration.Anfangsbedingungen.Count.ToString(CultureInfo.CurrentCulture);
            Anfangsbedingungen.Text = modell.Zeitintegration.VonStationär ? "stationäre Lösung" : "";
        }
        Show();
    }

    private void ZeitintervallBerechnen(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        var anzahl = int.Parse(Eigenlösung.Text);
        string betaM;
        var modellBerechnung = new Berechnung(modell);
        modell.Eigenzustand ??= new Eigenzustände("neu", anzahl);

        if (modell.Eigenzustand.Eigenwerte == null)
        {
            if (!StartFenster.berechnet)
            {
                modellBerechnung.BerechneSystemMatrix();
                StartFenster.berechnet = true;
            }
            modell.Eigenzustand = new Eigenzustände("neu", anzahl);
            modellBerechnung.Eigenzustände();
        }

        var alfa = double.Parse(Parameter.Text);
        var betaMax = modell.Eigenzustand.Eigenwerte[anzahl - 1];
        if (alfa < 0.5)
        {
            var deltatkrit = 2 / (betaMax * (1 - 2 * alfa));
            Zeitintervall.Text = deltatkrit.ToString(CultureInfo.CurrentCulture);
            betaM = betaMax.ToString(CultureInfo.CurrentCulture);
        }
        else
        {
            Zeitintervall.Text = "";
            betaM = betaMax.ToString(CultureInfo.CurrentCulture);
        }

        _ = MessageBox.Show("kritischer Zeitschritt für β max = " + betaM
                            + " ist frei für Stabilität und muss gewählt werden für Genauigkeit", "Zeitintegration");
    }
    private void BtnLöschen_Click(object sender, RoutedEventArgs e)
    {
        modell.Zeitintegration = null;
        Close();
    }
    private void BtnDialogOk_Click(object sender, RoutedEventArgs e)
    {
        if (Zeitintervall.Text == "")
        {
            _ = MessageBox.Show("kritischer Zeitschritt frei wählbar für Stabilität und muss gewählt werden für Genauigkeit", "Zeitintegration");
            return;
        }

        if (modell.Zeitintegration == null)
        {
            int anzahlEigenlösungen;
            try { anzahlEigenlösungen = int.Parse(Eigenlösung.Text, CultureInfo.InvariantCulture); }
            catch (FormatException) { _ = MessageBox.Show("Anzahl Eigenlösungen hat falsches Format", "neue Zeitintegration"); return; }

            double dt;
            try { dt = double.Parse(Zeitintervall.Text, CultureInfo.InvariantCulture); }
            catch (FormatException) { _ = MessageBox.Show("Zeitinterval der Integration hat falsches Format", "neue Zeitintegration"); return; }

            double tmax;
            try { tmax = double.Parse(Maximalzeit.Text, CultureInfo.InvariantCulture); }
            catch (FormatException) { _ = MessageBox.Show("maximale Integrationszeit tmax hat falsches Format", "neue Zeitintegration"); return; }

            double alfa;
            try { alfa = double.Parse(Parameter.Text, CultureInfo.InvariantCulture); }
            catch (FormatException) { _ = MessageBox.Show("Parameter alfa hat falsches Format", "neue Zeitintegration"); return; }

            modell.Eigenzustand = new Eigenzustände("eigen", anzahlEigenlösungen);
            modell.Zeitintegration = new Zeitintegration(tmax, dt, alfa) { VonStationär = false };
            StartFenster.zeitintegrationDaten = true;
        }
        else
        {
            try { modell.Eigenzustand.AnzahlZustände = int.Parse(Eigenlösung.Text, CultureInfo.InvariantCulture); }
            catch (FormatException) { _ = MessageBox.Show("number of eigensolutions has wrong format", "neue Zeitintegration"); return; }

            try { modell.Zeitintegration.Dt = double.Parse(Zeitintervall.Text, CultureInfo.InvariantCulture); }
            catch (FormatException) { _ = MessageBox.Show("time interval of integration has wrong format", "neue Zeitintegration"); return; }

            try { modell.Zeitintegration.Tmax = double.Parse(Maximalzeit.Text, CultureInfo.InvariantCulture); }
            catch (FormatException) { _ = MessageBox.Show("maximum integration time has wrong format", "neue Zeitintegration"); return; }

            try { modell.Zeitintegration.Parameter1 = double.Parse(Parameter.Text, CultureInfo.InvariantCulture); }
            catch (FormatException) { _ = MessageBox.Show("parameter alfa has wrong format", "neue Zeitintegration"); return; }
        }
        StartFenster.wärmeVisual.darstellung.AnfangsbedingungenEntfernen();
        anfangstemperaturenNeu?.Close();
        Close();
    }

    private void BtnDialogCancel_Click(object sender, RoutedEventArgs e)
    {
        anfangstemperaturenNeu?.Close();
        Close();
    }
    private void AnfangsbedingungNext(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        aktuell++;
        anfangstemperaturenNeu ??= new ZeitAnfangstemperaturNeu(modell);
        if (modell.Zeitintegration.Anfangsbedingungen.Count < aktuell)
        {
            anfangstemperaturenNeu.KnotenId.Text = "";
            anfangstemperaturenNeu.Anfangstemperatur.Text = "";
            StartFenster.wärmeVisual.zeitintegrationNeu.Anfangsbedingungen.Text = aktuell.ToString(CultureInfo.CurrentCulture);
        }
        else
        {
            var knotenwerte = (Knotenwerte)modell.Zeitintegration.Anfangsbedingungen[aktuell-1];
            StartFenster.wärmeVisual.zeitintegrationNeu.Anfangsbedingungen.Text =
                aktuell.ToString(CultureInfo.CurrentCulture);
            StartFenster.wärmeVisual.zeitintegrationNeu.Show();
            if (modell.Zeitintegration.VonStationär)
            {
                anfangstemperaturenNeu.StationäreLösung.IsChecked = true;
            }

            else if (knotenwerte.KnotenId == "alle")
            {
                anfangstemperaturenNeu.KnotenId.Text = "alle";
                anfangstemperaturenNeu.Anfangstemperatur.Text = knotenwerte.Werte[0].ToString(CultureInfo.CurrentCulture);
            }
            else
            {
                anfangstemperaturenNeu.KnotenId.Text = knotenwerte.KnotenId;
                anfangstemperaturenNeu.Anfangstemperatur.Text = knotenwerte.Werte[0].ToString(CultureInfo.CurrentCulture);
                var anf = aktuell.ToString("D");
                StartFenster.wärmeVisual.zeitintegrationNeu.Anfangsbedingungen.Text = anf;
                StartFenster.wärmeVisual.darstellung.AnfangsbedingungenZeichnen(knotenwerte.KnotenId, knotenwerte.Werte[0], anf);
            }
        }
    }
}