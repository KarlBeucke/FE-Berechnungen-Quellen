using System.Windows.Controls;
using System.Windows.Media;

namespace FE_Berechnungen.Tragwerksberechnung.ModelldatenAnzeigen;

public partial class AnregungVisualisieren
{
    private FeModell _modell;
    private readonly Darstellung _darstellung;
    private readonly double _dt, _tmax, _tmin;
    private double _anregungMax, _anregungMin;
    private List<double> _werte;

    public AnregungVisualisieren(FeModell feModell)
    {
        Language = XmlLanguage.GetLanguage("de-DE");
        InitializeComponent();
        _modell = feModell;
        Show();

        // Festlegung der Zeitachse
        _dt = feModell.Zeitintegration.Dt;
        _tmin = 0;
        _tmax = feModell.Zeitintegration.Tmax;

        // Initialisierung der Zeichenfläche
        _darstellung = new Darstellung(feModell, VisualAnregung);
    }

    private void BtnDatei_Click(object sender, RoutedEventArgs e)
    {
        const string inputDirectory = "\\FE-Berechnungen\\input\\Tragwerksberechnung\\Dynamik\\Anregungsdateien";
        // Ordinatenwerte im Zeitintervall dt aus Datei lesen: Schritte = (int)(Tmax/dt)+1
        _werte = Berechnung.AusDatei(inputDirectory, _modell);
        if (_werte.Count == 0)
        {
            MessageBox.Show("Keine Anregungswerte gefunden.");
            return;
        }
        _anregungMax = _werte.Max();
        _anregungMin = -_anregungMax;

        // Textdarstellung der Anregungsdauer mit Anzahl Datenpunkten und Zeitintervall
        AnregungText(_werte.Count * _dt, _werte.Count);

        var anregung = new double[_werte.Count];
        for (var i = 0; i < _werte.Count; i++) anregung[i] = _werte[i];
        _darstellung.Koordinatensystem(_tmin, _tmax, _anregungMax, _anregungMin);
        _darstellung.ZeitverlaufZeichnen(_dt, _tmin, _tmax, _anregungMax, anregung);
    }
    private void AnregungText(double dauer, int nSteps)
    {
        var anregungsWerte = dauer.ToString("N2") + " [s] Anregung  mit "
                                                  + nSteps + " Anregungswerten im Zeitschritt dt = " +
                                                  _dt.ToString("N3");
        var anregungTextBlock = new TextBlock
        {
            FontSize = 12,
            Foreground = Brushes.Black,
            FontWeight = FontWeights.Bold,
            Text = anregungsWerte
        };
        Canvas.SetTop(anregungTextBlock, 10);
        Canvas.SetLeft(anregungTextBlock, 20);
        VisualAnregung.Children.Add(anregungTextBlock);
    }

    private void BtnHarmonisch_Click(object sender, RoutedEventArgs e)
    {

    }

    private void BtnIntervalle_Click(object sender, RoutedEventArgs e)
    {

    }
}