using System;
using System.Globalization;
using System.Windows;
using System.Windows.Markup;
using FEBibliothek.Modell;
using FE_Berechnungen.Tragwerksberechnung.Modelldaten;

namespace FE_Berechnungen.Tragwerksberechnung.ModelldatenLesen;

public partial class ZeitintegrationNeu
{
    private readonly FeModell modell;
    public int aktuell, eigenForm;
    private ZeitKnotenanfangswerteNeu anfangswerteNeu;
    public ZeitintegrationNeu(FeModell modell)
    {
        Language = XmlLanguage.GetLanguage("de-DE");
        InitializeComponent();
        this.modell = modell;
        if (modell.Eigenzustand != null)
        {
            eigenForm = 1;
            Eigenlösung.Text = modell.Eigenzustand.AnzahlZustände.ToString(CultureInfo.InvariantCulture);
        }
        if (modell.Eigenzustand != null && modell.Eigenzustand.DämpfungsRaten.Count > 0)
        {
            Eigenform.Text = "1";
            var modalwerte = (ModaleWerte)modell.Eigenzustand.DämpfungsRaten[0];
            var rate = modalwerte.Dämpfung;
            Dämpfungsraten.Text = rate.ToString(CultureInfo.CurrentCulture);
        }

        if (modell.Zeitintegration != null)
        {
            MaximalZeit.Text = modell.Zeitintegration.Tmax.ToString(CultureInfo.CurrentCulture);
            switch (modell.Zeitintegration.Methode)
            {
                case 1:
                    Newmark.IsChecked = true;
                    Wilson.IsChecked = false; Taylor.IsChecked = false;
                    Beta.Text = modell.Zeitintegration.Parameter1.ToString(CultureInfo.CurrentCulture);
                    Gamma.Text = modell.Zeitintegration.Parameter2.ToString(CultureInfo.CurrentCulture);
                    Theta.Text = "";
                    Alfa.Text = "";
                    break;
                case 2:
                    Wilson.IsChecked = true;
                    Newmark.IsChecked = false; Taylor.IsChecked = false;
                    Beta.Text = ""; Gamma.Text = "";
                    Theta.Text = modell.Zeitintegration.Parameter1.ToString(CultureInfo.CurrentCulture);
                    Alfa.Text = "";
                    break;
                case 3:
                    Taylor.IsChecked = true;
                    Newmark.IsChecked = false; Wilson.IsChecked = false;
                    Beta.Text = ""; Gamma.Text = "";
                    Theta.Text = "";
                    Alfa.Text = modell.Zeitintegration.Parameter1.ToString(CultureInfo.CurrentCulture);
                    break;
            }
            Zeitintervall.Text = modell.Zeitintegration.Dt.ToString(CultureInfo.CurrentCulture);
            Gesamt.Text = modell.Zeitintegration.Anfangsbedingungen.Count.ToString(CultureInfo.CurrentCulture);
        }
        Show();
    }

    private void Newmark_OnChecked(object sender, RoutedEventArgs e)
    {
        Newmark.IsChecked = true;
        Wilson.IsChecked = false; Taylor.IsChecked = false;
        Beta.Text = 0.25.ToString("G5");
        Gamma.Text = 0.5.ToString("G5");
        Theta.Text = "";
        Alfa.Text = "";
    }

    private void Wilson_OnChecked(object sender, RoutedEventArgs e)
    {
        Wilson.IsChecked = true;
        Newmark.IsChecked = false; Taylor.IsChecked = false;
        Beta.Text = ""; Gamma.Text = "";
        Theta.Text = 1.420815.ToString("G5");
        Alfa.Text = "";
    }

    private void Taylor_OnChecked(object sender, RoutedEventArgs e)
    {
        Taylor.IsChecked = true;
        Newmark.IsChecked = false; Wilson.IsChecked = false;
        Beta.Text = ""; Gamma.Text = "";
        Theta.Text = "";
        Alfa.Text = (-0.1).ToString("G5");
    }

    private void BerechneZeitintervall(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        int anzahl;
        Berechnung modellBerechnung = null;

        try { anzahl = int.Parse(Eigenlösung.Text, CultureInfo.CurrentCulture); }
        catch (FormatException) { _ = MessageBox.Show("Anzahl Eigenlösungen hat falsches Format", "neue Zeitintegration"); return; }

        if (StartFenster.modellBerechnung == null) modellBerechnung = new Berechnung(modell);
        modell.Eigenzustand ??= new Eigenzustände("Tmin", anzahl);

        if (modell.Eigenzustand.Eigenwerte == null)
        {
            if (!StartFenster.berechnet)
            {
                modellBerechnung?.BerechneSystemMatrix();
                StartFenster.berechnet = true;
            }
            modell.Eigenzustand = new Eigenzustände("Tmin", anzahl);
            if (modellBerechnung != null | !StartFenster.zeitintegrationBerechnet) modellBerechnung?.Eigenzustände();
        }

        var omegaMax = modell.Eigenzustand.Eigenwerte[anzahl - 1];
        // kleinste Periode für größten Eigenwert in Lösung
        var tmin = 2 * Math.PI / Math.Sqrt(omegaMax);
        Zeitintervall.Text = tmin.ToString("F3");
    }

    private void AnfangsbedingungNext(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        aktuell++;
        anfangswerteNeu ??= new ZeitKnotenanfangswerteNeu(modell);
        if (modell.Zeitintegration.Anfangsbedingungen.Count < aktuell)
        {
            anfangswerteNeu.KnotenId.Text = "";
            anfangswerteNeu.Dof1D0.Text = ""; anfangswerteNeu.Dof1V0.Text = "";
            anfangswerteNeu.Dof2D0.Text = ""; anfangswerteNeu.Dof2V0.Text = "";
            anfangswerteNeu.Dof3D0.Text = ""; anfangswerteNeu.Dof3V0.Text = "";
            StartFenster.tragwerksModell.zeitintegrationNeu.Anfangsbedingungen.Text = aktuell.ToString(CultureInfo.CurrentCulture);
        }
        else
        {
            var knotenwerte = (Knotenwerte)modell.Zeitintegration.Anfangsbedingungen[aktuell - 1];
            StartFenster.tragwerksModell.zeitintegrationNeu.Anfangsbedingungen.Text =
                aktuell.ToString(CultureInfo.CurrentCulture);
            StartFenster.tragwerksModell.zeitintegrationNeu.Show();

            anfangswerteNeu.KnotenId.Text = knotenwerte.KnotenId;
            anfangswerteNeu.Dof1D0.Text = knotenwerte.Werte[0].ToString(CultureInfo.CurrentCulture);
            anfangswerteNeu.Dof1V0.Text = knotenwerte.Werte[1].ToString(CultureInfo.CurrentCulture);
            if (knotenwerte.Werte.Length > 2)
            {
                anfangswerteNeu.Dof2D0.Text = knotenwerte.Werte[2].ToString(CultureInfo.CurrentCulture);
                anfangswerteNeu.Dof2V0.Text = knotenwerte.Werte[3].ToString(CultureInfo.CurrentCulture);
            }

            if (knotenwerte.Werte.Length > 4)
            {
                anfangswerteNeu.Dof3D0.Text = knotenwerte.Werte[4].ToString(CultureInfo.CurrentCulture);
                anfangswerteNeu.Dof3V0.Text = knotenwerte.Werte[5].ToString(CultureInfo.CurrentCulture);
            }
            var anf = aktuell.ToString("D");
            StartFenster.tragwerksModell.zeitintegrationNeu.Anfangsbedingungen.Text = anf;
        }
    }

    private void DämpfungsratenNext(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        eigenForm++;
        StartFenster.tragwerksModell.zeitintegrationNeu.Eigenform.Text =
            eigenForm.ToString(CultureInfo.CurrentCulture);
        _ = new ZeitDämpfungsratenNeu(modell);

        var modalwerte = (ModaleWerte)modell.Eigenzustand.DämpfungsRaten[eigenForm - 1];
        StartFenster.tragwerksModell.zeitintegrationNeu.Dämpfungsraten.Text = modalwerte.Dämpfung.ToString(CultureInfo.CurrentCulture);
    }

    private void EigenformKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (Eigenform.Text.Length == 0) return;
        if (int.Parse(Eigenform.Text) > modell.Eigenzustand.AnzahlZustände) return;
        eigenForm = int.Parse(Eigenform.Text);
        Dämpfungsraten.Text = modell.Eigenzustand.DämpfungsRaten[eigenForm].ToString();
    }

    private void EigenformGotFocus(object sender, RoutedEventArgs e)
    {
        Eigenform.Clear();
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
            _ = MessageBox.Show("kritischer Zeitschritt unbeschränkt für Stabilität, gewählt für Genauigkeit", "neue Zeitintegration");
            return;
        }

        if (modell.Zeitintegration == null)
        {
            short methode;
            int anzahlEigenlösungen;
            double dt, tmax;

            try { dt = double.Parse(Zeitintervall.Text, CultureInfo.CurrentCulture); }
            catch (FormatException) { _ = MessageBox.Show("Zeitintervall deltaT hat falsches Format", "neue Zeitintegration"); return; }

            try { tmax = double.Parse(MaximalZeit.Text, CultureInfo.CurrentCulture); }
            catch (FormatException) { _ = MessageBox.Show("maximale Integrationszeit tmax hat falsches Format", "neue Zeitintegration"); return; }

            try { anzahlEigenlösungen = int.Parse(Eigenlösung.Text, CultureInfo.CurrentCulture); }
            catch (FormatException) { _ = MessageBox.Show("Anzahl Eigenlösungen hat falsches Format", "neue Zeitintegration"); return; }
            modell.Eigenzustand = new Eigenzustände("eigen", anzahlEigenlösungen);

            if (Newmark.IsChecked == true)
            {
                methode = 1;
                double beta, gamma;
                try { beta = double.Parse(Beta.Text, CultureInfo.CurrentCulture); }
                catch (FormatException) { _ = MessageBox.Show("Parameter Beta hat falsches Format", "neue Zeitintegration"); return; }

                try { gamma = double.Parse(Gamma.Text, CultureInfo.CurrentCulture); }
                catch (FormatException) { _ = MessageBox.Show("Parameter Gamma hat falsches Format", "neue Zeitintegration"); return; }

                modell.Zeitintegration = new Zeitintegration(tmax, dt, methode, beta, gamma);
            }
            else if (Wilson.IsChecked == true)
            {
                methode = 2;
                double theta;
                try { theta = double.Parse(Theta.Text, CultureInfo.CurrentCulture); }
                catch (FormatException) { _ = MessageBox.Show("Parameter Theta hat falsches Format", "neue Zeitintegration"); return; }
                modell.Zeitintegration = new Zeitintegration(tmax, dt, methode, theta);
            }
            else if (Taylor.IsChecked == true)
            {
                methode = 3;
                double alfa;
                try { alfa = double.Parse(Alfa.Text, CultureInfo.CurrentCulture); }
                catch (FormatException) { _ = MessageBox.Show("Parameter Alfa hat falsches Format", "neue Zeitintegration"); return; }
                modell.Zeitintegration = new Zeitintegration(tmax, dt, methode, alfa);
            }
            StartFenster.zeitintegrationDaten = true;
        }
        else
        {
            try { modell.Eigenzustand.AnzahlZustände = int.Parse(Eigenlösung.Text, CultureInfo.CurrentCulture); }
            catch (FormatException) { _ = MessageBox.Show("Anzahl Eigenlösungen hat falsches Format", "neue Zeitintegration"); return; }

            if (modell.Zeitintegration == null) return;
            try { modell.Zeitintegration.Dt = double.Parse(Zeitintervall.Text, CultureInfo.CurrentCulture); }
            catch (FormatException) { _ = MessageBox.Show("Zeitschritt deltaT hat falsches Format", "neue Zeitintegration"); }

            try { modell.Zeitintegration.Tmax = double.Parse(MaximalZeit.Text); }
            catch (FormatException) { _ = MessageBox.Show("maximale Integrationszeit hat falsches Format", "neue Zeitintegration"); }

            if (Dämpfungsraten.Text.Length == 0)
            {
                if (modell.Eigenzustand.DämpfungsRaten.Count > 0) modell.Eigenzustand.DämpfungsRaten.Clear();
                Eigenform.Text = "";
                eigenForm = 0;
            }
            else
            {
                try { modell.Eigenzustand.DämpfungsRaten[0] = double.Parse(Dämpfungsraten.Text); }
                catch (FormatException) { _ = MessageBox.Show("DämpfungsRaten hat falsches Format", "neue Zeitintegration"); }
            }

            if (Newmark.IsChecked == true)
            {
                try { modell.Zeitintegration.Parameter1 = double.Parse(Beta.Text, CultureInfo.InvariantCulture); }
                catch (FormatException) { _ = MessageBox.Show("Parameter beta hat falsches Format", "neue Zeitintegration"); }

                try { modell.Zeitintegration.Parameter2 = double.Parse(Gamma.Text, CultureInfo.CurrentCulture); }
                catch (FormatException) { _ = MessageBox.Show("Parameter gamma hat falsches Format", "neue Zeitintegration"); }
            }
            else if (Wilson.IsChecked == true)
            {
                try { modell.Zeitintegration.Parameter1 = double.Parse(Theta.Text, CultureInfo.CurrentCulture); }
                catch (FormatException) { _ = MessageBox.Show("Parameter theta hat falsches Format", "neue Zeitintegration"); }
            }
            else if (Taylor.IsChecked == true)
            {
                try { modell.Zeitintegration.Parameter1 = double.Parse(Alfa.Text, CultureInfo.CurrentCulture); }
                catch (FormatException) { _ = MessageBox.Show("Parameter alfa hat falsches Format", "neue Zeitintegration"); }
            }
        }
    }

    private void BtnDialogAbbrechen_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}