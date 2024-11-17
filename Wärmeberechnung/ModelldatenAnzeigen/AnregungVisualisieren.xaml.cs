using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;

namespace FE_Berechnungen.Wärmeberechnung.ModelldatenAnzeigen;

public partial class AnregungVisualisieren
{
    private readonly double[] anregung;
    private readonly Darstellung darstellung;
    private readonly double dt, tmax, tmin;
    private double anregungMax, anregungMin;

    public AnregungVisualisieren(FeModell feModell)
    {
        Language = XmlLanguage.GetLanguage("de-DE");
        InitializeComponent();
        Show();

        // Bestimmung der Zeitachse
        dt = feModell.Zeitintegration.Dt;
        tmin = 0;
        tmax = feModell.Zeitintegration.Tmax;
        var nSteps = (int)(tmax / dt) + 1;
        anregung = new double[nSteps];

        // Initialization of drawing canvas
        darstellung = new Darstellung(feModell, VisualAnregung);
    }

    private void BtnExcitation_Click(object sender, RoutedEventArgs e)
    {
        const string inputDirectory = "\\FE-Berechnungen-App\\input\\Wärmeberechnung\\instationär\\Anregungsdateien";
        // lies Ordinatenwerte im Zeitintervall dt aus Datei
        Berechnung.AusDatei(inputDirectory, 1, anregung);
        anregungMax = anregung.Max();
        anregungMin = -anregungMax;

        // Textdarstellung von Zeitdauer der Anregung mit Anzahl Datenpunkten und Zeitintervall
        AnregungsText(anregung.Length * dt, anregung.Length);

        darstellung.Koordinatensystem(tmin, tmax, anregungMax, anregungMin);
        darstellung.ZeitverlaufZeichnen(dt, tmin, tmax, anregungMax, anregung);
    }

    private void AnregungsText(double duration, int nSteps)
    {
        var anregungsWerte = duration.ToString("N2") + " [s] resp. " + (duration / 60 / 60).ToString("N0") +
                             "[h]  Anregung mit "
                             + nSteps + " Anregungswerten im Zeitintervall dt = " + dt.ToString("N3");
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