using System;
using System.Globalization;
using System.Windows;
using System.Windows.Input;
using System.Windows.Markup;
using FE_Berechnungen.Tragwerksberechnung.Modelldaten;
using FEBibliothek.Modell;

namespace FE_Berechnungen.Tragwerksberechnung.ModelldatenLesen;

public partial class ZeitintegrationNeu
{
    private readonly FeModell _modell;
    private ZeitKnotenanfangswerteNeu _anfangswerteNeu;
    public int Aktuell, EigenForm;

    public ZeitintegrationNeu(FeModell modell)
    {
        Language = XmlLanguage.GetLanguage("de-DE");
        InitializeComponent();
        _modell = modell;
        if (modell.Eigenzustand != null)
        {
            EigenForm = 1;
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
                    Wilson.IsChecked = false;
                    Taylor.IsChecked = false;
                    Beta.Text = modell.Zeitintegration.Parameter1.ToString(CultureInfo.CurrentCulture);
                    Gamma.Text = modell.Zeitintegration.Parameter2.ToString(CultureInfo.CurrentCulture);
                    Theta.Text = "";
                    Alfa.Text = "";
                    break;
                case 2:
                    Wilson.IsChecked = true;
                    Newmark.IsChecked = false;
                    Taylor.IsChecked = false;
                    Beta.Text = "";
                    Gamma.Text = "";
                    Theta.Text = modell.Zeitintegration.Parameter1.ToString(CultureInfo.CurrentCulture);
                    Alfa.Text = "";
                    break;
                case 3:
                    Taylor.IsChecked = true;
                    Newmark.IsChecked = false;
                    Wilson.IsChecked = false;
                    Beta.Text = "";
                    Gamma.Text = "";
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
        Wilson.IsChecked = false;
        Taylor.IsChecked = false;
        Beta.Text = 0.25.ToString("G5");
        Gamma.Text = 0.5.ToString("G5");
        Theta.Text = "";
        Alfa.Text = "";
    }

    private void Wilson_OnChecked(object sender, RoutedEventArgs e)
    {
        Wilson.IsChecked = true;
        Newmark.IsChecked = false;
        Taylor.IsChecked = false;
        Beta.Text = "";
        Gamma.Text = "";
        Theta.Text = 1.420815.ToString("G5");
        Alfa.Text = "";
    }

    private void Taylor_OnChecked(object sender, RoutedEventArgs e)
    {
        Taylor.IsChecked = true;
        Newmark.IsChecked = false;
        Wilson.IsChecked = false;
        Beta.Text = "";
        Gamma.Text = "";
        Theta.Text = "";
        Alfa.Text = (-0.1).ToString("G5");
    }

    private void BerechneZeitintervall(object sender, MouseButtonEventArgs e)
    {
        int anzahl=0;
        Berechnung modellBerechnung = null;

        try
        {
            anzahl = int.Parse(Eigenlösung.Text, CultureInfo.CurrentCulture);
        }
        catch (FormatException)
        {
            _ = MessageBox.Show("Anzahl Eigenlösungen hat falsches Format", "neue Zeitintegration");
        }

        if (StartFenster.ModellBerechnung == null) modellBerechnung = new Berechnung(_modell);
        _modell.Eigenzustand ??= new Eigenzustände("Tmin", anzahl);

        if (_modell.Eigenzustand.Eigenwerte == null)
        {
            if (!StartFenster.Berechnet)
            {
                modellBerechnung?.BerechneSystemMatrix();
                StartFenster.Berechnet = true;
            }

            _modell.Eigenzustand = new Eigenzustände("Tmin", anzahl);
            if ((modellBerechnung != null) | !StartFenster.ZeitintegrationBerechnet) modellBerechnung?.Eigenzustände();
        }

        var omegaMax = _modell.Eigenzustand.Eigenwerte[anzahl - 1];
        // kleinste Periode für größten Eigenwert in Lösung
        var tmin = 2 * Math.PI / Math.Sqrt(omegaMax);
        Zeitintervall.Text = tmin.ToString("F3");
    }

    private void AnfangsbedingungNext(object sender, MouseButtonEventArgs e)
    {
        Aktuell++;
        _anfangswerteNeu ??= new ZeitKnotenanfangswerteNeu(_modell);
        if (_modell.Zeitintegration.Anfangsbedingungen.Count < Aktuell)
        {
            _anfangswerteNeu.KnotenId.Text = "";
            _anfangswerteNeu.Dof1D0.Text = "";
            _anfangswerteNeu.Dof1V0.Text = "";
            _anfangswerteNeu.Dof2D0.Text = "";
            _anfangswerteNeu.Dof2V0.Text = "";
            _anfangswerteNeu.Dof3D0.Text = "";
            _anfangswerteNeu.Dof3V0.Text = "";
            StartFenster.TragwerkVisual.ZeitintegrationNeu.Anfangsbedingungen.Text =
                Aktuell.ToString(CultureInfo.CurrentCulture);
        }
        else
        {
            var knotenwerte = (Knotenwerte)_modell.Zeitintegration.Anfangsbedingungen[Aktuell - 1];
            StartFenster.TragwerkVisual.ZeitintegrationNeu.Anfangsbedingungen.Text =
                Aktuell.ToString(CultureInfo.CurrentCulture);
            StartFenster.TragwerkVisual.ZeitintegrationNeu.Show();

            _anfangswerteNeu.KnotenId.Text = knotenwerte.KnotenId;
            _anfangswerteNeu.Dof1D0.Text = knotenwerte.Werte[0].ToString(CultureInfo.CurrentCulture);
            _anfangswerteNeu.Dof1V0.Text = knotenwerte.Werte[1].ToString(CultureInfo.CurrentCulture);
            if (knotenwerte.Werte.Length > 2)
            {
                _anfangswerteNeu.Dof2D0.Text = knotenwerte.Werte[2].ToString(CultureInfo.CurrentCulture);
                _anfangswerteNeu.Dof2V0.Text = knotenwerte.Werte[3].ToString(CultureInfo.CurrentCulture);
            }

            if (knotenwerte.Werte.Length > 4)
            {
                _anfangswerteNeu.Dof3D0.Text = knotenwerte.Werte[4].ToString(CultureInfo.CurrentCulture);
                _anfangswerteNeu.Dof3V0.Text = knotenwerte.Werte[5].ToString(CultureInfo.CurrentCulture);
            }

            var anf = Aktuell.ToString("D");
            StartFenster.TragwerkVisual.ZeitintegrationNeu.Anfangsbedingungen.Text = anf;
        }
    }

    private void DämpfungsratenNext(object sender, MouseButtonEventArgs e)
    {
        EigenForm++;
        StartFenster.TragwerkVisual.ZeitintegrationNeu.Eigenform.Text =
            EigenForm.ToString(CultureInfo.CurrentCulture);
        _ = new ZeitDämpfungsratenNeu(_modell);

        var modalwerte = (ModaleWerte)_modell.Eigenzustand.DämpfungsRaten[EigenForm - 1];
        StartFenster.TragwerkVisual.ZeitintegrationNeu.Dämpfungsraten.Text =
            modalwerte.Dämpfung.ToString(CultureInfo.CurrentCulture);
    }

    private void EigenformKeyDown(object sender, KeyEventArgs e)
    {
        if (Eigenform.Text.Length == 0) return;
        try
        {
            if (int.Parse(Eigenform.Text) > _modell.Eigenzustand.AnzahlZustände) return;
            EigenForm = int.Parse(Eigenform.Text);
        }
        catch (FormatException)
        {
            _ = MessageBox.Show("Anzahl Eigenzustände", "neue Zeitintegration");
        }
        if (Dämpfungsraten.Text.Length != 0) Dämpfungsraten.Text = _modell.Eigenzustand.DämpfungsRaten[EigenForm].ToString()!;
    }

    private void EigenformGotFocus(object sender, RoutedEventArgs e)
    {
        Eigenform.Clear();
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
            _ = MessageBox.Show("kritischer Zeitschritt unbeschränkt für Stabilität, gewählt für Genauigkeit",
                "neue Zeitintegration");
            return;
        }

        if (_modell.Zeitintegration == null)
        {
            short methode;
            var anzahlEigenlösungen=0;
            double dt=0, tmax=0;

            try
            {
                dt = double.Parse(Zeitintervall.Text, CultureInfo.CurrentCulture);
            }
            catch (FormatException)
            {
                _ = MessageBox.Show("Zeitintervall deltaT hat falsches Format", "neue Zeitintegration");
            }

            try
            {
                tmax = double.Parse(MaximalZeit.Text, CultureInfo.CurrentCulture);
            }
            catch (FormatException)
            {
                _ = MessageBox.Show("maximale Integrationszeit tmax hat falsches Format", "neue Zeitintegration");
            }

            try
            {
                anzahlEigenlösungen = int.Parse(Eigenlösung.Text, CultureInfo.CurrentCulture);
            }
            catch (FormatException)
            {
                _ = MessageBox.Show("Anzahl Eigenlösungen hat falsches Format", "neue Zeitintegration");
            }

            _modell.Eigenzustand = new Eigenzustände("eigen", anzahlEigenlösungen);

            if (Newmark.IsChecked == true)
            {
                methode = 1;
                double beta=0, gamma=0;
                try
                {
                    beta = double.Parse(Beta.Text, CultureInfo.CurrentCulture);
                }
                catch (FormatException)
                {
                    _ = MessageBox.Show("Parameter Beta hat falsches Format", "neue Zeitintegration");
                }

                try
                {
                    gamma = double.Parse(Gamma.Text, CultureInfo.CurrentCulture);
                }
                catch (FormatException)
                {
                    _ = MessageBox.Show("Parameter Gamma hat falsches Format", "neue Zeitintegration");
                }

                _modell.Zeitintegration = new Zeitintegration(tmax, dt, methode, beta, gamma);
            }
            else if (Wilson.IsChecked == true)
            {
                methode = 2;
                double theta=0;
                try
                {
                    theta = double.Parse(Theta.Text, CultureInfo.CurrentCulture);
                }
                catch (FormatException)
                {
                    _ = MessageBox.Show("Parameter Theta hat falsches Format", "neue Zeitintegration");
                }

                _modell.Zeitintegration = new Zeitintegration(tmax, dt, methode, theta);
            }
            else if (Taylor.IsChecked == true)
            {
                methode = 3;
                double alfa=0;
                try
                {
                    alfa = double.Parse(Alfa.Text, CultureInfo.CurrentCulture);
                }
                catch (FormatException)
                {
                    _ = MessageBox.Show("Parameter Alfa hat falsches Format", "neue Zeitintegration");
                }

                _modell.Zeitintegration = new Zeitintegration(tmax, dt, methode, alfa);
            }

            StartFenster.ZeitintegrationDaten = true;
        }
        else
        {
            try
            {
                _modell.Eigenzustand.AnzahlZustände = int.Parse(Eigenlösung.Text, CultureInfo.CurrentCulture);
            }
            catch (FormatException)
            {
                _ = MessageBox.Show("Anzahl Eigenlösungen hat falsches Format", "neue Zeitintegration");
            }

            if (_modell.Zeitintegration == null) return;
            try
            {
                _modell.Zeitintegration.Dt = double.Parse(Zeitintervall.Text, CultureInfo.CurrentCulture);
            }
            catch (FormatException)
            {
                _ = MessageBox.Show("Zeitschritt deltaT hat falsches Format", "neue Zeitintegration");
            }

            try
            {
                _modell.Zeitintegration.Tmax = double.Parse(MaximalZeit.Text);
            }
            catch (FormatException)
            {
                _ = MessageBox.Show("maximale Integrationszeit hat falsches Format", "neue Zeitintegration");
            }

            if (Dämpfungsraten.Text.Length == 0)
            {
                if (_modell.Eigenzustand.DämpfungsRaten.Count > 0) _modell.Eigenzustand.DämpfungsRaten.Clear();
                Eigenform.Text = "";
                EigenForm = 0;
            }
            else
            {
                try
                {
                    _modell.Eigenzustand.DämpfungsRaten[0] = double.Parse(Dämpfungsraten.Text);
                }
                catch (FormatException)
                {
                    _ = MessageBox.Show("DämpfungsRaten hat falsches Format", "neue Zeitintegration");
                }
            }

            if (Newmark.IsChecked == true)
            {
                try
                {
                    _modell.Zeitintegration.Parameter1 = double.Parse(Beta.Text, CultureInfo.InvariantCulture);
                }
                catch (FormatException)
                {
                    _ = MessageBox.Show("Parameter beta hat falsches Format", "neue Zeitintegration");
                }

                try
                {
                    _modell.Zeitintegration.Parameter2 = double.Parse(Gamma.Text, CultureInfo.CurrentCulture);
                }
                catch (FormatException)
                {
                    _ = MessageBox.Show("Parameter gamma hat falsches Format", "neue Zeitintegration");
                }
            }
            else if (Wilson.IsChecked == true)
            {
                try
                {
                    _modell.Zeitintegration.Parameter1 = double.Parse(Theta.Text, CultureInfo.CurrentCulture);
                }
                catch (FormatException)
                {
                    _ = MessageBox.Show("Parameter theta hat falsches Format", "neue Zeitintegration");
                }
            }
            else if (Taylor.IsChecked == true)
            {
                try
                {
                    _modell.Zeitintegration.Parameter1 = double.Parse(Alfa.Text, CultureInfo.CurrentCulture);
                }
                catch (FormatException)
                {
                    _ = MessageBox.Show("Parameter alfa hat falsches Format", "neue Zeitintegration");
                }
            }
        }
    }

    private void BtnDialogAbbrechen_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}