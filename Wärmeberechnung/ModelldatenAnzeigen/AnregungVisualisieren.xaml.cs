using System.Windows.Controls;
using System.Windows.Media;

namespace FE_Berechnungen.Wärmeberechnung.ModelldatenAnzeigen;

public partial class AnregungVisualisieren
{
    private readonly double[] _anregung;
    private readonly Darstellung _darstellung;
    private readonly double _dt, _tmax, _tmin;
    private double _anregungMax, _anregungMin;
    private readonly FeModell _modell;

    public AnregungVisualisieren(FeModell feModell)
    {
        Language = XmlLanguage.GetLanguage("de-DE");
        InitializeComponent();
        _modell = feModell;
        Show();

        // Bestimmung der Zeitachse
        _dt = feModell.Zeitintegration.Dt;
        _tmin = 0;
        _tmax = feModell.Zeitintegration.Tmax;
        var nSteps = (int)(_tmax / _dt) + 1;
        _anregung = new double[nSteps];

        // Initialization of drawing canvas
        _darstellung = new Darstellung(feModell, VisualAnregung);
    }

    private void BtnExcitation_Click(object sender, RoutedEventArgs e)
    {
        const string inputDirectory = "\\FE-Berechnungen-App\\input\\Wärmeberechnung\\instationär\\Anregungsdateien";
        // lies Ordinatenwerte im Zeitintervall dt aus Datei
        Berechnung.AusDatei(inputDirectory, 0, _anregung, _modell);
        _anregungMax = _anregung.Max();
        _anregungMin = -_anregungMax;

        // Textdarstellung von Zeitdauer der Anregung mit Anzahl Datenpunkten und Zeitintervall
        AnregungsText(_anregung.Length * _dt, _anregung.Length);

        _darstellung.Koordinatensystem(_tmin, _tmax, _anregungMax, _anregungMin);
        _darstellung.ZeitverlaufZeichnen(_dt, _tmin, _tmax, _anregungMax, _anregung);
    }

    private void AnregungsText(double duration, int nSteps)
    {
        var anregungsWerte = duration.ToString("N2") + " [s] resp. " + (duration / 60 / 60).ToString("N0") +
                             "[h]  Anregung mit "
                             + nSteps + " Anregungswerten im Zeitintervall dt = " + _dt.ToString("N3");
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
}