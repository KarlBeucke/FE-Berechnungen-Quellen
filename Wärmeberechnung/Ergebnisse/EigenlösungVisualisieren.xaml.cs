using FEBibliothek.Modell;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using static System.Windows.Controls.Canvas;
using static System.Windows.FontWeights;
using static System.Windows.Media.Brushes;

namespace FE_Berechnungen.Wärmeberechnung.Ergebnisse
{
    public partial class EigenlösungVisualisieren : Window
    {
        private readonly FEModell modell;
        private int index;
        public Darstellung darstellung;
        private double auflösung, maxY;
        public double screenH, screenV;
        private const int RandLinks = 40;
        private bool knotentemperaturenAn = false;
        public List<object> Knotentemperaturen { get; set; }
        public List<object> Eigenwerte { get; set; }

        public EigenlösungVisualisieren(FEModell modell)
        {
            this.Language = XmlLanguage.GetLanguage("de-DE");
            this.modell = modell;
            InitializeComponent();
            Knotentemperaturen = new List<object>();
            Eigenwerte = new List<object>();
        }
        private void ModelGrid_Loaded(object sender, RoutedEventArgs e)
        {
            // Auswahl der Eigenlösung
            var anzahlEigenformen = modell.Eigenzustand.AnzahlZustände;
            var eigenformNr = new int[anzahlEigenformen];
            for (var i = 0; i < anzahlEigenformen; i++) { eigenformNr[i] = i + 1; }
            Eigenlösungauswahl.ItemsSource = eigenformNr;

            darstellung = new Darstellung(modell, VisualErgebnisse);
            darstellung.FestlegungAuflösung();
            maxY = darstellung.maxY;
            auflösung = darstellung.auflösung;
            darstellung.ElementeZeichnen();
        }

        // Combobox event
        private void DropDownEigenformauswahlClosed(object sender, System.EventArgs e)
        {
            index = Eigenlösungauswahl.SelectedIndex;
        }

        // Button event
        private void BtnEigenlösung_Click(object sender, RoutedEventArgs e)
        {
            //Toggle KnotenTemperaturen
            if (!knotentemperaturenAn)
            {
                // zeichne den Wert einer jeden Randbedingung als Text an Randknoten
                Eigenzustand_Zeichnen(modell.Eigenzustand.Eigenvektoren[index]);
                knotentemperaturenAn = true;

                var eigenwert = new TextBlock
                {
                    FontSize = 14,
                    Text = "Eigenwert Nr. " + (index + 1).ToString() + " = " + modell.Eigenzustand.Eigenwerte[index].ToString("N2"),
                    Foreground = Blue
                };
                SetTop(eigenwert, -10);
                SetLeft(eigenwert, RandLinks);
                VisualErgebnisse.Children.Add(eigenwert);
                Eigenwerte.Add(eigenwert);
            }
            else
            {
                // entferne ALLE Textdarstellungen der Knotentemperaturen
                foreach (var knotenTemp in Knotentemperaturen)
                {
                    VisualErgebnisse.Children.Remove(knotenTemp as TextBlock);
                }
                foreach (TextBlock eigenwert in Eigenwerte) VisualErgebnisse.Children.Remove(eigenwert as TextBlock);
                knotentemperaturenAn = false;
            }
        }

        public void Eigenzustand_Zeichnen(double[] zustand)
        {
            double maxTemp = 0, minTemp = 100;
            foreach (var item in modell.Knoten)
            {
                var knoten = item.Value;
                var temperatur = zustand[knoten.SystemIndizes[0]].ToString("N2");
                var temp = zustand[knoten.SystemIndizes[0]];
                if (temp > maxTemp) maxTemp = temp;
                if (temp < minTemp) minTemp = temp;
                var fensterKnoten = TransformKnoten(knoten, auflösung, maxY);

                var id = new TextBlock
                {
                    FontSize = 12,
                    Background = Red,
                    FontWeight = Bold,
                    Text = temperatur
                };
                Knotentemperaturen.Add(id);
                SetTop(id, fensterKnoten[1]);
                SetLeft(id, fensterKnoten[0]);
                VisualErgebnisse.Children.Add(id);
            }
        }

        private int[] TransformKnoten(Knoten knoten, double aufl, double mY)
        {
            this.auflösung = aufl;
            this.maxY = mY;
            var fensterKnoten = new int[2];
            fensterKnoten[0] = (int)(knoten.Koordinaten[0] * auflösung);
            fensterKnoten[1] = (int)(-knoten.Koordinaten[1] * auflösung + maxY);
            return fensterKnoten;
        }
    }
}