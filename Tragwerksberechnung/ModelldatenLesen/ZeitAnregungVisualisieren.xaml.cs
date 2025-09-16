using System.Windows.Controls;
using System.Windows.Media;

namespace FE_Berechnungen.Tragwerksberechnung.ModelldatenLesen
{
    public partial class ZeitAnregungVisualisieren
    {
        public ZeitAnregungVisualisieren(FeModell feModell, string initialDirectory)
        {
            Language = XmlLanguage.GetLanguage("de-DE");
            InitializeComponent();
            Show();

            // Festlegung der Zeitachse
            const double tmin = 0;
            var tmax = feModell.Zeitintegration.Tmax;
            var nZeitschritte = (int)(tmax / feModell.Zeitintegration.Dt) + 1;
            var funktion = new double[nZeitschritte];

            // Initialisierung der Zeichenfläche
            var darstellung = new Darstellung(feModell, VisualAnregung);

            foreach (var item in feModell.ZeitabhängigeKnotenLasten)
            {
                switch (item.Value.VariationsTyp)
                {
                    case 0:
                        var inputDirectory = initialDirectory +
                                                    "\\Beispiele\\Tragwerksberechnung\\Dynamik\\Anregungsdateien";
                        // Ordinatenwerte im Zeitintervall dt aus Datei lesen: Schritte = (int)(Tmax/dt)+1
                        Berechnung.AusDatei(inputDirectory, -1, funktion, feModell);
                        break;
                    case 1:
                        var intervall = item.Value.Intervall;
                        Berechnung.StückweiseLinear(intervall, funktion, feModell);
                        break;
                    case 2:
                        var amplitude = item.Value.Amplitude;
                        var frequenz = item.Value.Frequenz;
                        var winkel = item.Value.PhasenWinkel;
                        Berechnung.Periodisch(amplitude, frequenz, winkel, funktion, feModell);
                        break;
                }

                if (funktion is not { Length: not 0 })
                {
                    MessageBox.Show("Keine Anregungswerte gefunden.");
                    return;
                }

                var anregungMax = funktion.Max();
                //if (anregungMax < double.Epsilon) return;
                var anregungMin = -anregungMax;

                // Textdarstellung der Anregungsdauer mit Anzahl Datenpunkten und Zeitintervall
                AnregungText(item.Value.LastId, item.Value.KnotenId, funktion.Length * feModell.Zeitintegration.Dt, funktion.Length, feModell.Zeitintegration.Dt, VisualAnregung);

                darstellung.Koordinatensystem(tmin, tmax, anregungMax, anregungMin);
                darstellung.ZeitverlaufZeichnen(feModell.Zeitintegration.Dt, tmin, tmax, anregungMax, funktion);
                break;
            }
        }

        private static void AnregungText(string id, string knoten, double dauer, int nSteps, double dt, Canvas anregung)
        {
            var anregungsWerte = "zeitabhängige Knotenlast " + id + " am Knoten " + "'" + knoten + "', "
                                        + dauer.ToString("N2") + " [s] Anregung  mit "
                                        + nSteps + " Anregungswerten im Zeitschritt dt = "
                                        + dt.ToString("N3");
            var anregungTextBlock = new TextBlock
            {
                FontSize = 12,
                Foreground = Brushes.Black,
                FontWeight = FontWeights.Bold,
                Text = anregungsWerte
            };
            Canvas.SetTop(anregungTextBlock, 10);
            Canvas.SetLeft(anregungTextBlock, 20);
            anregung.Children.Add(anregungTextBlock);
        }
    }
}
