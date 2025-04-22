using System.Windows.Controls;
using System.Windows.Media;

namespace FE_Berechnungen.Tragwerksberechnung.ModelldatenLesen
{
    public partial class ZeitAnregungVisualisieren
    {
        public ZeitAnregungVisualisieren(FeModell feModell)
        {
            Language = XmlLanguage.GetLanguage("de-DE");
            InitializeComponent();
            Show();

            // Festlegung der Zeitachse
            var dt = feModell.Zeitintegration.Dt;
            const double tmin = 0;
            var tmax = feModell.Zeitintegration.Tmax;

            // Initialisierung der Zeichenfläche
            var darstellung = new Darstellung(feModell, VisualAnregung);

            foreach (var item in feModell.ZeitabhängigeKnotenLasten)
            {
                List<double> werte = null;
                switch (item.Value.VariationsTyp)
                {
                    case 0:
                        {
                            const string inputDirectory =
                                "\\FE-Berechnungen\\input\\Tragwerksberechnung\\Dynamik\\Anregungsdateien";
                            // Ordinatenwerte im Zeitintervall dt aus Datei lesen: Schritte = (int)(Tmax/dt)+1
                            werte = Berechnung.AusDatei(inputDirectory, feModell);
                            dt = feModell.Zeitintegration.Dt;
                            tmax = feModell.Zeitintegration.Tmax;
                            break;
                        }
                    case 1:
                        var intervall = item.Value.Intervall;
                        werte = Berechnung.StückweiseLinear(feModell.Zeitintegration.Dt, intervall, feModell);
                        break;
                    case 2:
                        werte = Berechnung.Periodisch(feModell.Zeitintegration.Dt, item.Value, feModell);
                        break;
                }

                if (werte == null || werte.Count == 0)
                {
                    MessageBox.Show("Keine Anregungswerte gefunden.");
                    return;
                }

                var anregungMax = werte.Max();
                var anregungMin = -anregungMax;

                // Textdarstellung der Anregungsdauer mit Anzahl Datenpunkten und Zeitintervall
                AnregungText(werte.Count * dt, werte.Count, dt, VisualAnregung);

                var funktion = new double[werte.Count];
                for (var i = 0; i < werte.Count; i++) funktion[i] = werte[i];
                darstellung.Koordinatensystem(tmin, tmax, anregungMax, anregungMin);
                darstellung.ZeitverlaufZeichnen(dt, tmin, tmax, anregungMax, funktion);
                break;
            }
        }

        private static void AnregungText(double dauer, int nSteps, double dt, Canvas anregung)
        {
            var anregungsWerte = dauer.ToString("N2") + " [s] Anregung  mit "
                                                      + nSteps + " Anregungswerten im Zeitschritt dt = " +
                                                      dt.ToString("N3");
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
