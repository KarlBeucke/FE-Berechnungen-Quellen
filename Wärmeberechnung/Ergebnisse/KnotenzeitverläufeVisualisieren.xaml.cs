using System.Windows.Controls;
using System.Windows.Media;
using static System.Windows.Media.Brushes;
using static System.Windows.Media.Color;

namespace FE_Berechnungen.Wärmeberechnung.Ergebnisse;

public partial class KnotenzeitverläufeVisualisieren
{
    private readonly Darstellung _darstellung;
    private readonly double _dt;
    private readonly FeModell _modell;
    private double _absMaxTemperatur;
    private double _absMaxWärmefluss;
    private Darstellungsbereich _ausschnitt;
    private double _ausschnittMax, _ausschnittMin;
    private bool _darstellungsBereichNeu;
    private Knoten _knoten;
    private TextBlock _maximal;
    private double _maxTemperatur, _minTemperatur;
    private double _maxWärmefluss, _minWärmefluss;
    private bool _temperaturVerlauf, _wärmeflussVerlauf;
    private double _zeit;

    public KnotenzeitverläufeVisualisieren(FeModell modell)
    {
        Language = XmlLanguage.GetLanguage("de-DE");
        this._modell = modell;
        InitializeComponent();
        Show();

        // Festlegung der Zeitachse
        _dt = modell.Zeitintegration.Dt;
        double tmin = 0;
        var tmax = modell.Zeitintegration.Tmax;
        _ausschnittMin = tmin;
        _ausschnittMax = tmax;

        // Auswahl des Knotens
        Knotenauswahl.ItemsSource = modell.Knoten.Keys;

        // Initialisierung der Zeichenfläche
        _darstellung = new Darstellung(modell, VisualErgebnisse);
    }

    private void DropDownKnotenauswahlClosed(object sender, EventArgs e)
    {
        if (Knotenauswahl.SelectedIndex < 0)
        {
            _ = MessageBox.Show("kein gültiger Knoten Identifikator ausgewählt", "Zeitschrittauswahl");
            return;
        }

        var knotenId = (string)Knotenauswahl.SelectedItem;
        if (_modell.Knoten.TryGetValue(knotenId, out _knoten))
        {
        }
    }

    private void BtnTemperatur_Click(object sender, RoutedEventArgs e)
    {
        _wärmeflussVerlauf = false;
        _maxTemperatur = _knoten.KnotenVariable[0].Max();
        _minTemperatur = _knoten.KnotenVariable[0].Min();
        if (_maxTemperatur > Math.Abs(_minTemperatur))
        {
            _zeit = _dt * Array.IndexOf(_knoten.KnotenVariable[0], _maxTemperatur);
            _absMaxTemperatur = _maxTemperatur;
            TemperaturNeuZeichnen();
        }
        else
        {
            _zeit = _dt * Array.IndexOf(_knoten.KnotenVariable[0], _minTemperatur);
            _absMaxTemperatur = _minTemperatur;
            TemperaturNeuZeichnen();
        }
    }

    private void TemperaturNeuZeichnen()
    {
        if (_knoten == null)
        {
            _ = MessageBox.Show("Knoten muss erst ausgewählt werden", "instationäre Wärmeberechnung");
        }
        else
        {
            if (_darstellungsBereichNeu)
            {
                VisualErgebnisse.Children.Clear();
                _ausschnittMin = _ausschnitt.Tmin;
                _ausschnittMax = _ausschnitt.Tmax;
                _maxTemperatur = Math.Abs(_ausschnitt.MaxTemperatur);
                _minTemperatur = -_maxTemperatur;
            }
            else
            {
                VisualErgebnisse.Children.Clear();
                _maxTemperatur = Math.Abs(_absMaxTemperatur);
                _minTemperatur = -_maxTemperatur;
            }

            Darstellungsbereich.Text = _ausschnittMin.ToString("N2") + " <= zeit <= "
                                                                    + _ausschnittMax.ToString("N2");
            _darstellung.Koordinatensystem(_ausschnittMin, _ausschnittMax, _maxTemperatur, _minTemperatur);

            // Textdarstellung des Maximalwertes mit Zeitpunkt
            MaximalwertText("Temperatur", _absMaxTemperatur, _zeit);

            _darstellung.ZeitverlaufZeichnen(_dt, _ausschnittMin, _ausschnittMax, _maxTemperatur, _knoten.KnotenVariable[0]);

            _temperaturVerlauf = true;
            _wärmeflussVerlauf = false;
            _darstellungsBereichNeu = false;
        }
    }

    private void BtnWärmefluss_Click(object sender, RoutedEventArgs e)
    {
        _temperaturVerlauf = false;
        _maxWärmefluss = _knoten.KnotenAbleitungen[0].Max();
        _minWärmefluss = _knoten.KnotenAbleitungen[0].Min();
        if (_maxWärmefluss > Math.Abs(_minWärmefluss))
        {
            _zeit = _dt * Array.IndexOf(_knoten.KnotenAbleitungen[0], _maxWärmefluss);
            _absMaxWärmefluss = _maxWärmefluss;
            WärmeflussVerlaufNeuZeichnen();
        }
        else
        {
            _zeit = _dt * Array.IndexOf(_knoten.KnotenAbleitungen[0], _minWärmefluss);
            _absMaxWärmefluss = _minWärmefluss;
            WärmeflussVerlaufNeuZeichnen();
        }
    }

    private void WärmeflussVerlaufNeuZeichnen()
    {
        const int unendlicheWärmeflussAnzeige = 100;
        if (_knoten == null)
        {
            _ = MessageBox.Show("Knoten muss erst ausgewählt werden", "instationäre Wärmeberechnung");
        }
        else
        {
            if (_darstellungsBereichNeu)
            {
                VisualErgebnisse.Children.Clear();
                _ausschnittMin = _ausschnitt.Tmin;
                _ausschnittMax = _ausschnitt.Tmax;
                _maxWärmefluss = Math.Abs(_ausschnitt.MaxWärmefluss);
                _minWärmefluss = -_maxWärmefluss;
            }
            else
            {
                VisualErgebnisse.Children.Clear();
                _maxWärmefluss = Math.Abs(_absMaxWärmefluss);
                _minWärmefluss = -_maxWärmefluss;
            }

            Darstellungsbereich.Text = _ausschnittMin.ToString("N2") + " <= zeit <= "
                                                                    + _ausschnittMax.ToString("N2");
            if (_maxWärmefluss > double.MaxValue)
            {
                _maxWärmefluss = unendlicheWärmeflussAnzeige;
                _minWärmefluss = -_maxWärmefluss;
            }

            _darstellung.Koordinatensystem(_ausschnittMin, _ausschnittMax, _maxWärmefluss, _minWärmefluss);

            // Textdarstellung des Maximalwertes mit Zeitpunkt
            VisualErgebnisse.Children.Remove(_maximal);
            MaximalwertText("Wärmefluss", _absMaxWärmefluss, _zeit);

            _darstellung.ZeitverlaufZeichnen(_dt, _ausschnittMin, _ausschnittMax, _maxWärmefluss,
                _knoten.KnotenAbleitungen[0]);

            _temperaturVerlauf = false;
            _wärmeflussVerlauf = true;
            _darstellungsBereichNeu = false;
        }
    }

    private void DarstellungsbereichDialog_Click(object sender, RoutedEventArgs e)
    {
        if (_knoten == null)
        {
            _ = MessageBox.Show("Knoten muss erst ausgewählt werden", "instationäre Wärmeberechnung");
        }
        else
        {
            VisualErgebnisse.Children.Clear();
            _ausschnitt = new Darstellungsbereich(_ausschnittMin, _ausschnittMax, _absMaxTemperatur, _absMaxWärmefluss);
            _ausschnittMin = _ausschnitt.Tmin;
            _ausschnittMax = _ausschnitt.Tmax;
            _maxTemperatur = _ausschnitt.MaxTemperatur;
            _maxWärmefluss = _ausschnitt.MaxWärmefluss;
            _darstellungsBereichNeu = true;
            if (_temperaturVerlauf) TemperaturNeuZeichnen();
            else if (_wärmeflussVerlauf) WärmeflussVerlaufNeuZeichnen();
        }
    }

    private void MaximalwertText(string ordinate, double maxWert, double maxZeit)
    {
        var rot = FromArgb(120, 255, 0, 0);
        var myBrush = new SolidColorBrush(rot);
        var maxwert = "Maximalwert für " + ordinate + " = " + maxWert.ToString("N2") + Environment.NewLine +
                      "an Zeit = " + maxZeit.ToString("N2");
        _maximal = new TextBlock
        {
            FontSize = 12,
            Background = myBrush,
            Foreground = Black,
            FontWeight = FontWeights.Bold,
            Text = maxwert
        };
        Canvas.SetTop(_maximal, 10);
        Canvas.SetLeft(_maximal, 20);
        VisualErgebnisse.Children.Add(_maximal);
    }
}