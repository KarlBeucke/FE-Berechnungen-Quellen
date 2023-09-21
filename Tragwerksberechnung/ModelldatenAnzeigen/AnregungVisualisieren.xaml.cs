using FEBibliothek.Modell;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;

namespace FE_Berechnungen.Tragwerksberechnung.ModelldatenAnzeigen;

public partial class AnregungVisualisieren
{
    private readonly Darstellung darstellung;
    private readonly double dt, tmax, tmin;
    private double anregungMax, anregungMin;
    private IList<double> werte;

    public AnregungVisualisieren(FeModell feModell)
    {
        Language = XmlLanguage.GetLanguage("de-DE");
        InitializeComponent();
        Show();

        // Festlegung der Zeitachse
        dt = feModell.Zeitintegration.Dt;
        tmin = 0;
        tmax = feModell.Zeitintegration.Tmax;

        // Initialisierung der Zeichenfläche
        darstellung = new Darstellung(feModell, VisualAnregung);
    }

    private void BtnAnregung_Click(object sender, RoutedEventArgs e)
    {
        const string inputDirectory = "\\FE-Berechnungen-App\\input\\Tragwerksberechnung\\Dynamik\\Anregungsdateien";
        // Ordinatenwerte im Zeitintervall dt aus Datei lesen: Schritte = (int)(tmax/dt)+1
        werte = StartFenster.modellBerechnung.AusDatei(inputDirectory);
        anregungMax = werte.Max();
        anregungMin = -anregungMax;

        // Textdarstellung der Anregungsdauer mit Anzahl Datenpunkten und Zeitintervall
        AnregungText(werte.Count * dt, werte.Count);

        var anregung = new double[werte.Count];
        for (var i = 0; i < werte.Count; i++) anregung[i] = werte[i];
        darstellung.Koordinatensystem(tmin, tmax, anregungMax, anregungMin);
        darstellung.ZeitverlaufZeichnen(dt, tmin, tmax, anregungMax, anregung);
    }
    private void AnregungText(double dauer, int nSteps)
    {
        var anregungsWerte = dauer.ToString("N2") + " [s] Anregung  mit "
                      + nSteps + " Anregungswerten im Zeitschritt dt = " + dt.ToString("N3");
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