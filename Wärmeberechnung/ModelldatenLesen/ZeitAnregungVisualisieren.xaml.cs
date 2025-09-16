using System.Windows.Controls;
using System.Windows.Media;

namespace FE_Berechnungen.Wärmeberechnung.ModelldatenLesen
{
    public partial class ZeitAnregungVisualisieren
    {
        private static string _typ = string.Empty;
        public ZeitAnregungVisualisieren(FeModell feModell, string initialDirectory)
        {
            Language = XmlLanguage.GetLanguage("de-DE");
            InitializeComponent();
            Show();

            // Festlegung der Zeitachse
            var dt = feModell.Zeitintegration.Dt;
            const double tmin = 0;
            var tmax = feModell.Zeitintegration.Tmax;
            var nZeitschritte = (int)(tmax / dt) + 1;

            // Initialisierung der Zeichenfläche
            var darstellung = new Darstellung(feModell, VisualAnregung);
            var funktion = new double[nZeitschritte];

            foreach (var item in feModell.ZeitabhängigeKnotenLasten)
            {
                _typ = "zeitabhängige Knotenlast ";
                switch (item.Value.VariationsTyp)
                {
                    case 0:
                        var inputDirectory = initialDirectory +
                                             "\\Beispiele\\Wärmeberechnung\\instationär\\Anregungsdateien";
                        // Ordinatenwerte im Zeitintervall dt aus Datei lesen: Schritte = (int)(Tmax/dt)+1
                        // nur 1. Spalte lesen
                        Berechnung.AusDatei(inputDirectory, 0, funktion, feModell);
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

                if (funktion.Length == 0)
                {
                    MessageBox.Show("Keine Anregungswerte gefunden.");
                    return;
                }

                var anregungMax = funktion.Max();
                var anregungMin = -anregungMax;

                // Textdarstellung der Anregungsdauer mit Anzahl Datenpunkten und Zeitintervall
                AnregungText(item.Value.LastId, item.Value.KnotenId, funktion.Length * dt, funktion.Length, dt, VisualAnregung);

                //var funktion = new double[werte.Count];
                darstellung.Koordinatensystem(tmin, tmax, anregungMax, anregungMin);
                darstellung.ZeitverlaufZeichnen(dt, tmin, tmax, anregungMax, funktion);
                break;
            }

            //if (feModell.ZeitabhängigeElementLasten.Any())
            //{
            foreach (var item in feModell.ZeitabhängigeElementLasten)
            {
                _typ = "zeitabhängige Elementlast ";
                var inputDirectory = initialDirectory +
                                     "\\Beispiele\\Wärmeberechnung\\instationär\\Anregungsdateien";
                // Ordinatenwerte im Zeitintervall dt aus Datei lesen: Schritte = (int)(Tmax/dt)+1
                // nur 1. Spalte lesen
                Berechnung.AusDatei(inputDirectory, 0, funktion, feModell);

                if (funktion.Length == 0)
                {
                    MessageBox.Show("Keine Anregungswerte gefunden.");
                    return;
                }

                var anregungMax = funktion.Max();
                var anregungMin = -anregungMax;

                // Textdarstellung der Anregungsdauer mit Anzahl Datenpunkten und Zeitintervall
                AnregungText(item.Value.LastId, item.Value.KnotenId, funktion.Length * dt, funktion.Length, dt, VisualAnregung);

                //var funktion = new double[werte.Count];
                darstellung.Koordinatensystem(tmin, tmax, anregungMax, anregungMin);
                darstellung.ZeitverlaufZeichnen(dt, tmin, tmax, anregungMax, funktion);
                break;
            }

            foreach (var item in feModell.ZeitabhängigeRandbedingung)
            {
                _typ = "zeitabhängige Randbedingung ";
                switch (item.Value.VariationsTyp)
                {
                    case 0:
                        var inputDirectory = initialDirectory +
                                             "\\Beispiele\\Wärmeberechnung\\instationär\\Anregungsdateien";
                        // Ordinatenwerte im Zeitintervall dt aus Datei lesen: Schritte = (int)(Tmax/dt)+1
                        // nur 1. Spalte lesen
                        Berechnung.AusDatei(inputDirectory, 0, funktion, feModell);
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

                if (funktion.Length == 0)
                {
                    MessageBox.Show("Keine Anregungswerte gefunden.");
                    return;
                }

                var anregungMax = funktion.Max();
                //if (anregungMax > double.Epsilon) anregungMax = 1;
                var anregungMin = -anregungMax;

                // Textdarstellung der Anregungsdauer mit Anzahl Datenpunkten und Zeitintervall
                AnregungText(item.Value.RandbedingungId, item.Value.KnotenId, funktion.Length * dt, funktion.Length, dt, VisualAnregung);

                //var funktion = new double[werte.Count];
                darstellung.Koordinatensystem(tmin, tmax, anregungMax, anregungMin);
                darstellung.ZeitverlaufZeichnen(dt, tmin, tmax, anregungMax, funktion);
                break;
            }
        }

        private static void AnregungText(string id, string knoten, double dauer, int nSteps, double dt, Canvas anregung)
        {
            var tage = (dauer / 60 / 60 / 24).ToString("N2");
            var anregungsWerte = _typ + "'" + id + "'" + " am Knoten " + "'" + knoten + "', "
                                 + dauer.ToString("N0") + "s (" + tage + "Tage) Anregung  mit "
                                 + nSteps + " Anregungswerten im Zeitschritt dt = "
                                 + dt.ToString("N2");

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