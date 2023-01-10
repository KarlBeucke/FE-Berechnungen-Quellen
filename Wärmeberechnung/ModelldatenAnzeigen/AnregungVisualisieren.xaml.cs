using FEBibliothek.Modell;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;

namespace FE_Berechnungen.Wärmeberechnung.ModelldatenAnzeigen;

public partial class AnregungVisualisieren
{
    private readonly Darstellung darstellung;
    private readonly double dt, tmax, tmin;
    private double anregungMax, anregungMin;
    private IList<double> werte;
    //private double[] excitation;

    public AnregungVisualisieren(FeModell feModell)
    {
        Language = XmlLanguage.GetLanguage("de-DE");
        InitializeComponent();
        Show();

        // Bestimmung der Zeitachse
        dt = feModell.Zeitintegration.Dt;
        tmin = 0;
        tmax = feModell.Zeitintegration.Tmax;

        // Initialization of drawing canvas
        darstellung = new Darstellung(feModell, VisualAnregung);
    }

    private void BtnExcitation_Click(object sender, RoutedEventArgs e)
    {
        const string inputDirectory = "\\FE-Berechnungen-App\\input\\Wärmeberechnung\\instationär\\Anregungsdateien";
        // lies Ordinatenwerte im Zeitintervall dt aus Datei
        werte = new List<double>();
        StartFenster.modellBerechnung.AusDatei(inputDirectory,1,werte);
        anregungMax = werte.Max();
        anregungMin = -anregungMax;

        // Textdarstellung von Zeitdauer der Anregung mit Anzahl Datenpunkten und Zeitintervall
        AnregungsText(werte.Count * dt, werte.Count);

        var anregung = new double[werte.Count];
        for (var i = 0; i < werte.Count; i++) anregung[i] = werte[i];
        darstellung.Koordinatensystem(tmin, tmax, anregungMax, anregungMin);
        darstellung.ZeitverlaufZeichnen(dt, tmin, tmax, anregungMax, anregung);
    }
    private void AnregungsText(double duration, int nSteps)
    {
        var anregungsWerte = duration.ToString("N2") + " [s] resp. " + (duration/60/60).ToString("N0") + "[h]  Anregung mit "
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