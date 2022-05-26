using FEBibliothek.Modell;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using static System.Windows.Media.Brushes;
using static System.Windows.Media.Color;

namespace FE_Berechnungen.Wärmeberechnung.Ergebnisse
{
    public partial class KnotenzeitverläufeVisualisieren
    {
        private readonly FEModell modell;
        private Knoten knoten;
        private readonly double dt;
        private double zeit;
        private double maxTemperatur, minTemperatur;
        private double absMaxTemperatur;
        private double maxWärmefluss, minWärmefluss;
        private double absMaxWärmefluss;

        private readonly Darstellung darstellung;
        private Darstellungsbereich ausschnitt;
        private double ausschnittMax, ausschnittMin;
        private bool darstellungsBereichNeu;
        private bool temperaturVerlauf, wärmeflussVerlauf;
        private TextBlock maximal;

        public KnotenzeitverläufeVisualisieren(FEModell modell)
        {
            Language = XmlLanguage.GetLanguage("de-DE");
            this.modell = modell;
            InitializeComponent();
            Show();

            // Festlegung der Zeitachse
            dt = modell.Zeitintegration.Dt;
            double tmin = 0;
            var tmax = modell.Zeitintegration.Tmax;
            ausschnittMin = tmin;
            ausschnittMax = tmax;

            // Auswahl des Knotens
            Knotenauswahl.ItemsSource = modell.Knoten.Keys;

            // Initialisierung der Zeichenfläche
            darstellung = new Darstellung(modell, VisualErgebnisse);
        }

        private void DropDownKnotenauswahlClosed(object sender, EventArgs e)
        {
            if (Knotenauswahl.SelectedIndex < 0)
            {
                _ = MessageBox.Show("kein gültiger Knoten Identifikator ausgewählt", "Zeitschrittauswahl");
                return;
            }
            var knotenId = (string)Knotenauswahl.SelectedItem;
            if (modell.Knoten.TryGetValue(knotenId, out knoten)) { }
        }

        private void BtnTemperatur_Click(object sender, RoutedEventArgs e)
        {
            wärmeflussVerlauf = false;
            maxTemperatur = knoten.KnotenVariable[0].Max();
            minTemperatur = knoten.KnotenVariable[0].Min();
            if (maxTemperatur > Math.Abs(minTemperatur))
            {
                zeit = dt * Array.IndexOf(knoten.KnotenVariable[0], maxTemperatur);
                absMaxTemperatur = maxTemperatur;
                TemperaturNeuZeichnen();
            }
            else
            {
                zeit = dt * Array.IndexOf(knoten.KnotenVariable[0], minTemperatur);
                absMaxTemperatur = minTemperatur;
                TemperaturNeuZeichnen();
            }
        }
        private void TemperaturNeuZeichnen()
        {
            if (knoten == null)
            {
                _ = MessageBox.Show("Knoten muss erst ausgewählt werden", "instationäre Wärmeberechnung");
            }
            else
            {
                if (darstellungsBereichNeu)
                {
                    VisualErgebnisse.Children.Clear();
                    ausschnittMin = ausschnitt.tmin;
                    ausschnittMax = ausschnitt.tmax;
                    maxTemperatur = Math.Abs(ausschnitt.maxTemperatur);
                    minTemperatur = -maxTemperatur;
                }
                else
                {
                    VisualErgebnisse.Children.Clear();
                    maxTemperatur = Math.Abs(absMaxTemperatur);
                    minTemperatur = -maxTemperatur;
                }

                Darstellungsbereich.Text = ausschnittMin.ToString("N2") + " <= zeit <= "
                                                                        + ausschnittMax.ToString("N2");
                darstellung.Koordinatensystem(ausschnittMin, ausschnittMax, maxTemperatur, minTemperatur);

                // Textdarstellung des Maximalwertes mit Zeitpunkt
                MaximalwertText("Temperatur", absMaxTemperatur, zeit);

                darstellung.ZeitverlaufZeichnen(dt, ausschnittMin, ausschnittMax, maxTemperatur, knoten.KnotenVariable[0]);

                temperaturVerlauf = true;
                wärmeflussVerlauf = false;
                darstellungsBereichNeu = false;
            }
        }

        private void BtnWärmefluss_Click(object sender, RoutedEventArgs e)
        {
            temperaturVerlauf = false;
            maxWärmefluss = knoten.KnotenAbleitungen[0].Max();
            minWärmefluss = knoten.KnotenAbleitungen[0].Min();
            if (maxWärmefluss > Math.Abs(minWärmefluss))
            {
                zeit = dt * Array.IndexOf(knoten.KnotenAbleitungen[0], maxWärmefluss);
                absMaxWärmefluss = maxWärmefluss;
                WärmeflussVerlaufNeuZeichnen();
            }
            else
            {
                zeit = dt * Array.IndexOf(knoten.KnotenAbleitungen[0], minWärmefluss);
                absMaxWärmefluss = minWärmefluss;
                WärmeflussVerlaufNeuZeichnen();
            }
        }
        private void WärmeflussVerlaufNeuZeichnen()
        {
            const int unendlicheWärmeflussAnzeige = 100;
            if (knoten == null)
            {
                _ = MessageBox.Show("Knoten muss erst ausgewählt werden", "instationäre Wärmeberechnung");
            }
            else
            {
                if (darstellungsBereichNeu)
                {
                    VisualErgebnisse.Children.Clear();
                    ausschnittMin = ausschnitt.tmin;
                    ausschnittMax = ausschnitt.tmax;
                    maxWärmefluss = Math.Abs(ausschnitt.maxWärmefluss);
                    minWärmefluss = -maxWärmefluss;
                }
                else
                {
                    VisualErgebnisse.Children.Clear();
                    maxWärmefluss = Math.Abs(absMaxWärmefluss);
                    minWärmefluss = -maxWärmefluss;
                }

                Darstellungsbereich.Text = ausschnittMin.ToString("N2") + " <= zeit <= "
                                                                        + ausschnittMax.ToString("N2");
                if (maxWärmefluss > double.MaxValue) { maxWärmefluss = unendlicheWärmeflussAnzeige; minWärmefluss = -maxWärmefluss; }
                darstellung.Koordinatensystem(ausschnittMin, ausschnittMax, maxWärmefluss, minWärmefluss);

                // Textdarstellung des Maximalwertes mit Zeitpunkt
                VisualErgebnisse.Children.Remove(maximal);
                MaximalwertText("Wärmefluss", absMaxWärmefluss, zeit);

                darstellung.ZeitverlaufZeichnen(dt, ausschnittMin, ausschnittMax, maxWärmefluss, knoten.KnotenAbleitungen[0]);

                temperaturVerlauf = false;
                wärmeflussVerlauf = true;
                darstellungsBereichNeu = false;
            }
        }

        private void DarstellungsbereichDialog_Click(object sender, RoutedEventArgs e)
        {
            if (knoten == null)
            {
                _ = MessageBox.Show("Knoten muss erst ausgewählt werden", "instationäre Wärmeberechnung");
            }
            else
            {
                VisualErgebnisse.Children.Clear();
                ausschnitt = new Darstellungsbereich(ausschnittMin, ausschnittMax, absMaxTemperatur, absMaxWärmefluss);
                ausschnittMin = ausschnitt.tmin;
                ausschnittMax = ausschnitt.tmax;
                maxTemperatur = ausschnitt.maxTemperatur;
                maxWärmefluss = ausschnitt.maxWärmefluss;
                darstellungsBereichNeu = true;
                if (temperaturVerlauf) TemperaturNeuZeichnen();
                else if (wärmeflussVerlauf) WärmeflussVerlaufNeuZeichnen();
            }
        }

        private void MaximalwertText(string ordinate, double maxWert, double maxZeit)
        {
            var rot = FromArgb(120, 255, 0, 0);
            var myBrush = new SolidColorBrush(rot);
            var maxwert = "Maximalwert für " + ordinate + " = " + maxWert.ToString("N2") + Environment.NewLine +
                          "an Zeit = " + maxZeit.ToString("N2");
            maximal = new TextBlock
            {
                FontSize = 12,
                Background = myBrush,
                Foreground = Black,
                FontWeight = FontWeights.Bold,
                Text = maxwert
            };
            Canvas.SetTop(maximal, 10);
            Canvas.SetLeft(maximal, 20);
            VisualErgebnisse.Children.Add(maximal);
        }
    }
}
