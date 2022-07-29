using FEBibliothek.Modell;
using System.Windows;
using System.Windows.Markup;

namespace FE_Berechnungen.Wärmeberechnung.Ergebnisse
{
    public partial class InstationäreModellzuständeVisualisieren
    {
        private int index;
        private readonly Darstellung darstellung;
        private bool knotenTemperaturAn, knotenGradientenAn, elementTemperaturAn;

        public InstationäreModellzuständeVisualisieren(FEModell modell)
        {
            Language = XmlLanguage.GetLanguage("de-DE");
            InitializeComponent();
            Show();

            darstellung = new Darstellung(modell, VisualErgebnisse);
            darstellung.FestlegungAuflösung();
            darstellung.AlleElementeZeichnen();

            // Auswahl des Zeitschritts
            var dt = modell.Zeitintegration.Dt;
            var tmax = modell.Zeitintegration.Tmax;
            var nSteps = (int)(tmax / dt) + 1;
            var zeit = new double[nSteps];
            for (var i = 0; i < nSteps; i++) { zeit[i] = (i * dt); }
            Zeitschrittauswahl.ItemsSource = zeit;
        }

        private void DropDownZeitschrittauswahlClosed(object sender, System.EventArgs e)
        {
            if (Zeitschrittauswahl.SelectedIndex < 0)
            {
                _ = MessageBox.Show("kein gültiger Zeitschritt ausgewählt", "TimeStepSelection");
                return;
            }
            index = Zeitschrittauswahl.SelectedIndex;

            darstellung.zeitschritt = index;
            darstellung.KnotentemperaturZeichnen(index);
        }
        private void BtnKnotenTemperaturen_Click(object sender, RoutedEventArgs e)
        {
            if (!knotenTemperaturAn)
            {
                if (index == 0)
                {
                    _ = MessageBox.Show("Zeitschritt muss erst ausgewählt werden", "instationäre Wärmeberechnung");
                }
                else
                {
                    darstellung.KnotentemperaturZeichnen(index);
                    knotenTemperaturAn = true;
                }
            }
            else
            {
                // entferne ALLE Textdarstellungen der Knotentemperaturen
                foreach (var knotenTemp in darstellung.Knotentemperaturen)
                {
                    VisualErgebnisse.Children.Remove(knotenTemp);
                }
                knotenTemperaturAn = false;
            }
        }
        private void BtnKnotenGradienten_Click(object sender, RoutedEventArgs e)
        {
            if (!knotenGradientenAn)
            {
                if (index == 0)
                {
                    _ = MessageBox.Show("Zeitschritt muss erst ausgewählt werden", "instationäre Wärmeberechnung");
                }
                else
                {
                    darstellung.KnotengradientenZeichnen(index);
                    knotenGradientenAn = true;
                }
            }
            else
            {
                // entferne ALLE Textdarstellungen der Knotentemperaturen
                foreach (var knotenGrad in darstellung.Knotengradienten)
                {
                    VisualErgebnisse.Children.Remove(knotenGrad);
                }
                knotenGradientenAn = false;
            }
        }
        private void BtnElementTemperaturen_Click(object sender, RoutedEventArgs e)
        {
            if (!elementTemperaturAn)
            {
                if (index == 0)
                {
                    _ = MessageBox.Show("Zeitschritt muss erst ausgewählt werden", "instationäre Wärmeberechnung");
                }
                else
                {
                    darstellung.ElementTemperaturZeichnen();
                    darstellung.WärmeflussvektorenZeichnen();
                    elementTemperaturAn = true;
                }
            }
            else
            {
                foreach (var path in darstellung.TemperaturElemente)
                {
                    VisualErgebnisse.Children.Remove(path);
                }
                elementTemperaturAn = false;
            }

        }
    }
}