using FE_Berechnungen.Tragwerksberechnung.ModelldatenAnzeigen;
using System.Windows.Controls;
using System.Windows.Media;

namespace FE_Berechnungen.Tragwerksberechnung.ModelldatenLesen
{
    public partial class ZeitAnregungVisualisierenNeu
    {
        private readonly FeModell _feModell;
        private Berechnung _modellBerechnung;
        private readonly Darstellung _darstellung;
        private readonly double _dt, _tmax, _tmin;
        //private double _anregungMax, _anregungMin;
        //private List<double> _werte;
        public ZeitAnregungVisualisierenNeu(FeModell feModell)
        {
            Language = XmlLanguage.GetLanguage("de-DE");
            InitializeComponent();
            Show();
            _feModell = feModell;

            // Festlegung der Zeitachse
            _dt = feModell.Zeitintegration.Dt;
            _tmin = 0;
            _tmax = feModell.Zeitintegration.Tmax;

            // Initialisierung der Zeichenfläche
            _darstellung = new Darstellung(feModell, VisualAnregung);
        }

        private void BtnDatei_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_feModell.ZeitintegrationDaten && _feModell != null)
                {
                    if (_feModell.ZeitabhängigeKnotenLasten.Count == 0)
                    {
                        MessageBox.Show("Keine keine zeitabhängigen Knotenlasten definiert");
                    }
                    else
                    {
                        foreach (var item in _feModell.ZeitabhängigeKnotenLasten)
                        {
                            switch (item.Value.VariationsTyp)
                            {
                                case 0:
                                    {
                                        _modellBerechnung ??= new Berechnung(_feModell);
                                        var anregung = new AnregungVisualisieren(_feModell);
                                        anregung.Show();

                                        // Festlegung der Zeitachse
                                        var dt = _feModell.Zeitintegration.Dt;
                                        double tmin = 0;
                                        var tmax = _feModell.Zeitintegration.Tmax;

                                        // Initialisierung der Zeichenfläche
                                        var darstellung = new Darstellung(_feModell, anregung.VisualAnregung);

                                        const string inputDirectory = "\\FE-Berechnungen\\input\\Tragwerksberechnung\\Dynamik\\Anregungsdateien";
                                        // Ordinatenwerte im Zeitintervall dt aus Datei lesen: Schritte = (int)(Tmax/dt)+1
                                        var werte = Berechnung.AusDatei(inputDirectory, _feModell);
                                        if (werte.Count == 0)
                                        {
                                            MessageBox.Show("Keine Anregungswerte gefunden.");
                                            return;
                                        }
                                        var anregungMax = werte.Max();
                                        var anregungMin = -anregungMax;

                                        // Textdarstellung der Anregungsdauer mit Anzahl Datenpunkten und Zeitintervall
                                        AnregungText(werte.Count * dt, werte.Count, dt, anregung);

                                        var funktion = new double[werte.Count];
                                        for (var i = 0; i < werte.Count; i++) funktion[i] = werte[i];
                                        darstellung.Koordinatensystem(tmin, tmax, anregungMax, anregungMin);
                                        darstellung.ZeitverlaufZeichnen(dt, tmin, tmax, anregungMax, funktion);
                                        break;
                                    }
                                case 2:

                                    break;
                                case 3:

                                    break;
                            }
                        }
                    }
                }
                else
                {
                    _ = MessageBox.Show("Daten für Zeitintegration sind noch nicht spezifiziert", "Tragwerksberechnung");
                }
            }
            catch (BerechnungAusnahme e2)
            {
                _ = MessageBox.Show(e2.Message);
            }
        }
        private static void AnregungText(double dauer, int nSteps, double dt, AnregungVisualisieren anregung)
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
            anregung.VisualAnregung.Children.Add(anregungTextBlock);
        }

        private void BtnHarmonisch_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BtnIntervalle_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
