using FE_Berechnungen.Tragwerksberechnung.Modelldaten;
using System.Globalization;
using System.Windows.Input;

namespace FE_Berechnungen.Tragwerksberechnung.ModelldatenLesen;

public partial class ZeitintegrationNeu
{
    private readonly FeModell _modell;
    private ZeitKnotenanfangswerteNeu _anfangswerteNeu;
    public int Aktuell;
    private int _eigenform;

    public ZeitintegrationNeu(FeModell modell)
    {
        Language = XmlLanguage.GetLanguage("de-DE");
        InitializeComponent();
        _modell = modell;
        if (modell.Eigenzustand == null) modell.Eigenzustand = new Eigenzustände("eigen", 1);
        else 
        {
            _eigenform = 1;
            Eigenlösung.Text = modell.Eigenzustand.AnzahlZustände.ToString(CultureInfo.InvariantCulture);
        }
        
        if (modell.Eigenzustand != null && modell.Eigenzustand.DämpfungsRaten.Count > 0)
        {
            var modalwerte = (ModaleWerte)modell.Eigenzustand.DämpfungsRaten[0];
            Dämpfungsraten.Text = modalwerte.Dämpfung.ToString(CultureInfo.CurrentCulture);
            Eigen.Text = modalwerte.Text;
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
        var anzahl = 0;

        try
        {
            anzahl = int.Parse(Eigenlösung.Text, CultureInfo.CurrentCulture);
        }
        catch (FormatException)
        {
            _ = MessageBox.Show("Anzahl Eigenlösungen hat falsches Format", "neue Zeitintegration");
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
            var knotenwerte = _modell.Zeitintegration.Anfangsbedingungen[Aktuell - 1];
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
        if (Eigen.Text == "alle") return;
        _eigenform++;
        if (_eigenform > _modell.Eigenzustand.AnzahlZustände)
        {
            _ = MessageBox.Show("modale Dämpfung nur für jede Eigenlösung", "Zeitintegration neu");
            Ok.Focus();
            return;
        }

        if (_eigenform > _modell.Eigenzustand.DämpfungsRaten.Count)
        {
            var neu = _modell.Eigenzustand.DämpfungsRaten.Count + 1;
            _modell.Eigenzustand.DämpfungsRaten.Add(new ModaleWerte(0,neu.ToString()+". Eigenform"));
        }

        StartFenster.TragwerkVisual.ZeitintegrationNeu.Eigen.Text =
            _eigenform.ToString(CultureInfo.CurrentCulture);

        var modalwerte = (ModaleWerte)_modell.Eigenzustand.DämpfungsRaten[_eigenform - 1];
        StartFenster.TragwerkVisual.ZeitintegrationNeu.Dämpfungsraten.Text =
            modalwerte.Dämpfung.ToString(CultureInfo.CurrentCulture);
    }

    private void EigenformGotFocus(object sender, RoutedEventArgs e)
    {
        Eigen.Clear();
    }
    private void EigenformLostFocus(object sender, RoutedEventArgs e)
    {
        if (Eigen.Text == "alle") return;
        try
        {
            if (int.Parse(Eigen.Text) > _modell.Eigenzustand.AnzahlZustände)
            {
                _ = MessageBox.Show("gewählte Eigenform größer als Anzahl verfügbarer Eigenzustände", "neue Zeitintegration");
                return;
            }
            _eigenform = int.Parse(Eigen.Text);
        }
        catch (FormatException)
        {
            _ = MessageBox.Show("Anzahl Eigenzustände", "neue Zeitintegration");
        }
    }

    private void BtnDialogOk_Click(object sender, RoutedEventArgs e)
    {
        if (Zeitintervall.Text == "")
        {
            _ = MessageBox.Show("Zeitintervall für Integration muss definiert sein", "neue Zeitintegration");
            return;
        }

        if (_modell.Zeitintegration == null)
        {
            short methode;
            var anzahlEigenlösungen = 0;
            double dt = 0, tmax = 0;

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
                _ = MessageBox.Show("maximale Integrationszeit Tmax hat falsches Format", "neue Zeitintegration");
            }

            try
            {
                anzahlEigenlösungen = int.Parse(Eigenlösung.Text, CultureInfo.CurrentCulture);
            }
            catch (FormatException)
            {
                _ = MessageBox.Show("Anzahl Eigenlösungen hat falsches Format", "neue Zeitintegration");
            }
            if(Newmark.IsChecked == true)
            {
                var beta = double.Parse(Beta.Text);
                var gamma = double.Parse(Gamma.Text);
                _modell.Zeitintegration = new Zeitintegration(tmax, dt, 1, beta, gamma);
            }
            else if (Wilson.IsChecked == true)
            {
                var theta = double.Parse(Theta.Text);
                _modell.Zeitintegration = new Zeitintegration(tmax, dt, 2, theta);
            }
            else if (Taylor.IsChecked == true)
            {
                var alfa = double.Parse(Alfa.Text);
                _modell.Zeitintegration = new Zeitintegration(tmax, dt, 3, alfa);
            }

            _modell.Eigenzustand = new Eigenzustände("eigen", anzahlEigenlösungen);
            if (Dämpfungsraten.Text.Length == 0)
            {
                if (_modell.Eigenzustand.DämpfungsRaten.Count > 0) _modell.Eigenzustand.DämpfungsRaten.Clear();
                Eigen.Text = "";
                _eigenform = 0;
            }
            else
            {
                try
                {
                    var modalwerte = new ModaleWerte(double.Parse(Dämpfungsraten.Text, CultureInfo.CurrentCulture), Eigen.Text);
                    _modell.Eigenzustand.DämpfungsRaten.Add(modalwerte);
                }
                catch (FormatException)
                {
                    _ = MessageBox.Show("DämpfungsRaten hat falsches Format", "neue Zeitintegration");
                }
            }

            if (Newmark.IsChecked == true)
            {
                methode = 1;
                double beta = 0, gamma = 0;
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
                double theta = 0;
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
                double alfa = 0;
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

            _modell.ZeitintegrationDaten = true;
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
                Eigen.Text = "";
                _eigenform = 0;
            }
            else
            {
                try
                {
                    var modalwerte = new ModaleWerte(double.Parse(Dämpfungsraten.Text, CultureInfo.CurrentCulture),Eigen.Text);
                    if (_modell.Eigenzustand.DämpfungsRaten.Count < _eigenform)
                    {
                        _modell.Eigenzustand.DämpfungsRaten.Add(modalwerte);
                    }
                    else
                    {
                        _modell.Eigenzustand.DämpfungsRaten[_eigenform-1] = modalwerte;
                    }
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
        Close();
    }

    private void BtnDialogAbbrechen_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}