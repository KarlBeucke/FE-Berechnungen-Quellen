using FE_Berechnungen.Wärmeberechnung.Modelldaten;
using System.Globalization;
using System.Windows.Input;
using System.Windows.Markup;

namespace FE_Berechnungen.Wärmeberechnung.ModelldatenLesen;

public partial class ZeitintegrationNeu
{
    private readonly FeModell _modell;
    private ZeitAnfangstemperaturNeu anfangstemperaturenNeu;

    public ZeitintegrationNeu(FeModell modell)
    {
        Language = XmlLanguage.GetLanguage("de-DE");
        InitializeComponent();
        _modell = modell;
        if (_modell.Eigenzustand != null)
            Eigenlösung.Text = modell.Eigenzustand.AnzahlZustände.ToString(CultureInfo.CurrentCulture);
        if (_modell.Zeitintegration != null)
        {
            Zeitintervall.Text = _modell.Zeitintegration.Dt.ToString(CultureInfo.CurrentCulture);
            Maximalzeit.Text = _modell.Zeitintegration.Tmax.ToString(CultureInfo.CurrentCulture);
            Parameter.Text = _modell.Zeitintegration.Parameter1.ToString(CultureInfo.CurrentCulture);
        }

        Show();
    }

    private void ZeitintervallBerechnen(object sender, MouseButtonEventArgs e)
    {
        var anzahl = int.Parse(Eigenlösung.Text);
        var modellBerechnung = new Berechnung(_modell);
        _modell.Eigenzustand ??= new Eigenzustände("neu", anzahl);

        if (_modell.Eigenzustand.Eigenwerte == null)
        {
            if (!_modell.Berechnet)
            {
                modellBerechnung.BerechneSystemMatrix();
                _modell.Berechnet = true;
            }

            _modell.Eigenzustand = new Eigenzustände("neu", anzahl);
            modellBerechnung.Eigenzustände();
        }

        var alfa = double.Parse(Parameter.Text);
        var betaMax = _modell.Eigenzustand.Eigenwerte[anzahl - 1];
        if (alfa < 0.5)
        {
            var deltatkrit = 2 / (betaMax * (1 - 2 * alfa));
            Zeitintervall.Text = deltatkrit.ToString(CultureInfo.CurrentCulture);
        }
        else
        {
            Zeitintervall.Text = "";
        }

        var betaM = betaMax.ToString(CultureInfo.CurrentCulture);

        _ = MessageBox.Show("kritischer Zeitschritt für β max = " + betaM
                                                                  + " ist frei für Stabilität und muss gewählt werden für Genauigkeit",
            "Zeitintegration");
    }

    private void BtnLöschen_Click(object sender, RoutedEventArgs e)
    {
        _modell.Zeitintegration = null;
        Close();
    }

    private void BtnDialogOk_Click(object sender, RoutedEventArgs e)
    {
        if (Zeitintervall.Text == "")
        {
            _ = MessageBox.Show(
                "kritischer Zeitschritt frei wählbar für Stabilität und muss gewählt werden für Genauigkeit",
                "Zeitintegration");
            return;
        }

        int anzahlEigenlösungen;
        double dt, tmax, alfa;
        try
        {
            tmax = double.Parse(Maximalzeit.Text, CultureInfo.CurrentCulture);
        }
        catch (FormatException)
        {
            _ = MessageBox.Show("maximale Integrationszeit tmax hat falsches Format", "neue Zeitintegration");
            return;
        }
        try
        {
            anzahlEigenlösungen = int.Parse(Eigenlösung.Text, CultureInfo.CurrentCulture);
        }
        catch (FormatException)
        {
            _ = MessageBox.Show("Anzahl Eigenlösungen hat falsches Format", "neue Zeitintegration");
            return;
        }
        try
        {
            alfa = double.Parse(Parameter.Text, CultureInfo.CurrentCulture);
        }
        catch (FormatException)
        {
            _ = MessageBox.Show("Parameter alfa hat falsches Format", "neue Zeitintegration");
            return;
        }
        try
        {
            dt = double.Parse(Zeitintervall.Text, CultureInfo.CurrentCulture);
        }
        catch (FormatException)
        {
            _ = MessageBox.Show("Zeitintervall der Integration Δt hat falsches Format", "neue Zeitintegration");
            return;
        }
        
        

        if (_modell.Zeitintegration == null)
        {
            _modell.Eigenzustand = new Eigenzustände("eigen", anzahlEigenlösungen);
            _modell.Zeitintegration = new Zeitintegration(tmax, dt, alfa) { VonStationär = false };
            _modell.ZeitintegrationDaten = true;
        }
        else
        {
            _modell.Eigenzustand.AnzahlZustände = anzahlEigenlösungen;
            _modell.Zeitintegration.Dt = dt;
            _modell.Zeitintegration.Tmax = tmax;
            _modell.Zeitintegration.Parameter1 = alfa;
        }

        StartFenster.WärmeVisual.Darstellung.AnfangsbedingungenEntfernen();
        anfangstemperaturenNeu?.Close();
        Close();
    }

    private void BtnDialogCancel_Click(object sender, RoutedEventArgs e)
    {
        anfangstemperaturenNeu?.Close();
        Close();
    }

    //private void AnfangsbedingungNext(object sender, MouseButtonEventArgs e)
    //{
    //    aktuell++;
    //    anfangstemperaturenNeu ??= new ZeitAnfangstemperaturNeu(_modell);
    //    if (_modell.Zeitintegration.Anfangsbedingungen.Count < aktuell)
    //    {
    //        anfangstemperaturenNeu.KnotenId.Text = "";
    //        anfangstemperaturenNeu.Anfangstemperatur.Text = "";
    //        StartFenster.WärmeVisual.ZeitintegrationNeu.Anfangsbedingungen.Text =
    //            aktuell.ToString(CultureInfo.CurrentCulture);
    //    }
    //    else
    //    {
    //        var knotenwerte = (Knotenwerte)_modell.Zeitintegration.Anfangsbedingungen[aktuell - 1];
    //        StartFenster.WärmeVisual.ZeitintegrationNeu.Anfangsbedingungen.Text =
    //            aktuell.ToString(CultureInfo.CurrentCulture);
    //        StartFenster.WärmeVisual.ZeitintegrationNeu.Show();
    //        if (_modell.Zeitintegration.VonStationär)
    //        {
    //            anfangstemperaturenNeu.StationäreLösung.IsChecked = true;
    //        }

    //        else if (knotenwerte.KnotenId == "alle")
    //        {
    //            anfangstemperaturenNeu.KnotenId.Text = "alle";
    //            anfangstemperaturenNeu.Anfangstemperatur.Text =
    //                knotenwerte.Werte[0].ToString(CultureInfo.CurrentCulture);
    //        }
    //        else
    //        {
    //            anfangstemperaturenNeu.KnotenId.Text = knotenwerte.KnotenId;
    //            anfangstemperaturenNeu.Anfangstemperatur.Text =
    //                knotenwerte.Werte[0].ToString(CultureInfo.CurrentCulture);
    //            var anf = aktuell.ToString("D");
    //            StartFenster.WärmeVisual.ZeitintegrationNeu.Anfangsbedingungen.Text = anf;
    //            StartFenster.WärmeVisual.Darstellung.AnfangsbedingungenZeichnen(knotenwerte.KnotenId,
    //                knotenwerte.Werte[0], anf);
    //        }
    //    }
    //}
}