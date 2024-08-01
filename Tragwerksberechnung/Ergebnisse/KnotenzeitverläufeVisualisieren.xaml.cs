using FEBibliothek.Modell;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using static System.Windows.Controls.Canvas;
using static System.Windows.Media.Brushes;
using static System.Windows.Media.Color;

namespace FE_Berechnungen.Tragwerksberechnung.Ergebnisse;

public partial class KnotenzeitverläufeVisualisieren
{
    private readonly Darstellung darstellung;
    private readonly double dt;
    private readonly FeModell modell;
    private double absMaxBeschleunigung;
    private double absMaxVerformung;
    private bool accXVerlauf, accYVerlauf;
    private DarstellungsbereichDialog ausschnitt;
    private double ausschnittMax, ausschnittMin;
    private bool darstellungsBereichNeu;
    private bool deltaXVerlauf, deltaYVerlauf;
    private Knoten knoten;
    private double maxBeschleunigung, minBeschleunigung;
    private TextBlock maximal;
    private double maxVerformung, minVerformung;
    private double zeit;

    public KnotenzeitverläufeVisualisieren(FeModell feModell)
    {
        Language = XmlLanguage.GetLanguage("de-DE");
        modell = feModell;
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
            _ = MessageBox.Show("kein gültiger Knotenidentifikator ausgewählt", "Zeitschrittauswahl");
            return;
        }

        var knotenId = (string)Knotenauswahl.SelectedItem;
        if (modell.Knoten.TryGetValue(knotenId, out knoten))
        {
        }
    }

    private void BtnDeltaX_Click(object sender, RoutedEventArgs e)
    {
        deltaYVerlauf = false;
        accXVerlauf = false;
        accYVerlauf = false;
        maxVerformung = knoten.KnotenVariable[0].Max();
        minVerformung = knoten.KnotenVariable[0].Min();
        if (maxVerformung > Math.Abs(minVerformung))
        {
            zeit = dt * Array.IndexOf(knoten.KnotenVariable[0], maxVerformung);
            absMaxVerformung = maxVerformung;
            DeltaXNeuZeichnen();
        }
        else
        {
            zeit = dt * Array.IndexOf(knoten.KnotenVariable[0], minVerformung);
            absMaxVerformung = minVerformung;
            DeltaXNeuZeichnen();
        }
    }

    private void DeltaXNeuZeichnen()
    {
        if (knoten == null)
        {
            _ = MessageBox.Show("Knoten muss erst ausgewählt werden", "dynamische Tragwerksberechnung");
        }
        else
        {
            if (darstellungsBereichNeu)
            {
                VisualErgebnisse.Children.Clear();
                ausschnittMin = ausschnitt.tmin;
                ausschnittMax = ausschnitt.tmax;
                maxVerformung = Math.Abs(ausschnitt.maxVerformung);
                minVerformung = -maxVerformung;
            }
            else
            {
                VisualErgebnisse.Children.Clear();
                maxVerformung = Math.Abs(absMaxVerformung);
                minVerformung = -maxVerformung;
            }

            Darstellungsbereich.Text = ausschnittMin.ToString("N2") + " <= zeit <= "
                                                                    + ausschnittMax.ToString("N2");
            if (maxVerformung < double.Epsilon) return;
            darstellung.Koordinatensystem(ausschnittMin, ausschnittMax, maxVerformung, minVerformung);

            // Textdarstellung des Maximalwertes mit Zeitpunkt
            MaximalwertText("Verformung x", absMaxVerformung, zeit);

            darstellung.ZeitverlaufZeichnen(dt, ausschnittMin, ausschnittMax, maxVerformung, knoten.KnotenVariable[0]);

            deltaXVerlauf = true;
            deltaYVerlauf = false;
            accXVerlauf = false;
            accYVerlauf = false;
            darstellungsBereichNeu = false;
        }
    }

    private void BtnDeltaY_Click(object sender, RoutedEventArgs e)
    {
        deltaXVerlauf = false;
        accXVerlauf = false;
        accYVerlauf = false;
        maxVerformung = knoten.KnotenVariable[1].Max();
        minVerformung = knoten.KnotenVariable[1].Min();
        if (maxVerformung > Math.Abs(minVerformung))
        {
            zeit = dt * Array.IndexOf(knoten.KnotenVariable[1], maxVerformung);
            absMaxVerformung = maxVerformung;
            DeltaYNeuZeichnen();
        }
        else
        {
            zeit = dt * Array.IndexOf(knoten.KnotenVariable[1], minVerformung);
            absMaxVerformung = minVerformung;
            DeltaYNeuZeichnen();
        }
    }

    private void DeltaYNeuZeichnen()
    {
        if (knoten == null)
        {
            _ = MessageBox.Show("Knoten muss erst ausgewählt werden", "dynamische Tragwerksberechnung");
        }
        else
        {
            if (darstellungsBereichNeu)
            {
                VisualErgebnisse.Children.Clear();
                ausschnittMin = ausschnitt.tmin;
                ausschnittMax = ausschnitt.tmax;
                maxVerformung = Math.Abs(ausschnitt.maxVerformung);
                minVerformung = -maxVerformung;
            }
            else
            {
                VisualErgebnisse.Children.Clear();
                maxVerformung = Math.Abs(absMaxVerformung);
                minVerformung = -maxVerformung;
            }

            Darstellungsbereich.Text = ausschnittMin.ToString("N2") + " <= zeit <= "
                                                                    + ausschnittMax.ToString("N2");
            if (maxVerformung < double.Epsilon) return;
            darstellung.Koordinatensystem(ausschnittMin, ausschnittMax, maxVerformung, minVerformung);

            // Textdarstellung des Maximalwertes mit Zeitpunkt
            MaximalwertText("Verformung y", absMaxVerformung, zeit);

            darstellung.ZeitverlaufZeichnen(dt, ausschnittMin, ausschnittMax, maxVerformung, knoten.KnotenVariable[1]);

            deltaXVerlauf = false;
            deltaYVerlauf = true;
            accXVerlauf = false;
            accYVerlauf = false;
            darstellungsBereichNeu = false;
        }
    }

    private void BtnAccX_Click(object sender, RoutedEventArgs e)
    {
        deltaXVerlauf = false;
        deltaYVerlauf = false;
        accYVerlauf = false;
        maxBeschleunigung = knoten.KnotenAbleitungen[0].Max();
        minBeschleunigung = knoten.KnotenAbleitungen[0].Min();
        if (maxBeschleunigung > Math.Abs(minBeschleunigung))
        {
            zeit = dt * Array.IndexOf(knoten.KnotenAbleitungen[0], maxBeschleunigung);
            absMaxBeschleunigung = maxBeschleunigung;
            AccXNeuZeichnen();
        }
        else
        {
            zeit = dt * Array.IndexOf(knoten.KnotenAbleitungen[0], minBeschleunigung);
            absMaxBeschleunigung = minBeschleunigung;
            AccXNeuZeichnen();
        }
    }

    private void AccXNeuZeichnen()
    {
        if (knoten == null)
        {
            _ = MessageBox.Show("Knoten muss erst ausgewählt werden", "dynamische Tragwerksberechnung");
        }
        else
        {
            if (darstellungsBereichNeu)
            {
                VisualErgebnisse.Children.Clear();
                ausschnittMin = ausschnitt.tmin;
                ausschnittMax = ausschnitt.tmax;
                maxBeschleunigung = Math.Abs(ausschnitt.maxBeschleunigung);
                minBeschleunigung = -maxBeschleunigung;
            }
            else
            {
                VisualErgebnisse.Children.Clear();
                maxBeschleunigung = Math.Abs(absMaxBeschleunigung);
                minBeschleunigung = -maxBeschleunigung;
            }

            Darstellungsbereich.Text = ausschnittMin.ToString("N2") + " <= zeit <= "
                                                                    + ausschnittMax.ToString("N2");
            if (maxBeschleunigung < double.Epsilon) return;
            darstellung.Koordinatensystem(ausschnittMin, ausschnittMax, maxBeschleunigung, minBeschleunigung);

            // Textdarstellung des Maximalwertes mit Zeitpunkt
            MaximalwertText("Beschleunigung x", absMaxBeschleunigung, zeit);

            darstellung.ZeitverlaufZeichnen(dt, ausschnittMin, ausschnittMax, maxBeschleunigung,
                knoten.KnotenAbleitungen[0]);

            deltaXVerlauf = false;
            deltaYVerlauf = false;
            accXVerlauf = true;
            accYVerlauf = false;
            darstellungsBereichNeu = false;
        }
    }

    private void BtnAccY_Click(object sender, RoutedEventArgs e)
    {
        deltaXVerlauf = false;
        deltaYVerlauf = false;
        accXVerlauf = false;
        maxBeschleunigung = knoten.KnotenAbleitungen[1].Max();
        minBeschleunigung = knoten.KnotenAbleitungen[1].Min();
        if (maxBeschleunigung > Math.Abs(minBeschleunigung))
        {
            zeit = dt * Array.IndexOf(knoten.KnotenAbleitungen[1], maxBeschleunigung);
            absMaxBeschleunigung = maxBeschleunigung;
            AccYNeuZeichnen();
        }
        else
        {
            zeit = dt * Array.IndexOf(knoten.KnotenAbleitungen[1], minBeschleunigung);
            absMaxBeschleunigung = minBeschleunigung;
            AccYNeuZeichnen();
        }
    }

    private void AccYNeuZeichnen()
    {
        if (knoten == null)
        {
            _ = MessageBox.Show("Knoten muss erst ausgewählt werden", "dynamische Tragwerksberechnung");
        }
        else
        {
            if (darstellungsBereichNeu)
            {
                VisualErgebnisse.Children.Clear();
                ausschnittMin = ausschnitt.tmin;
                ausschnittMax = ausschnitt.tmax;
                maxBeschleunigung = Math.Abs(ausschnitt.maxBeschleunigung);
                minBeschleunigung = -maxBeschleunigung;
            }
            else
            {
                VisualErgebnisse.Children.Clear();
                maxBeschleunigung = Math.Abs(absMaxBeschleunigung);
                minBeschleunigung = -maxBeschleunigung;
            }

            Darstellungsbereich.Text = ausschnittMin.ToString("N2") + " <= zeit <= "
                                                                    + ausschnittMax.ToString("N2");
            if (maxBeschleunigung < double.Epsilon) return;
            darstellung.Koordinatensystem(ausschnittMin, ausschnittMax, maxBeschleunigung, minBeschleunigung);

            // Textdarstellung des Maximalwertes mit Zeitpunkt
            MaximalwertText("Beschleunigung y", absMaxBeschleunigung, zeit);

            darstellung.ZeitverlaufZeichnen(dt, ausschnittMin, ausschnittMax, maxBeschleunigung,
                knoten.KnotenAbleitungen[1]);

            deltaXVerlauf = false;
            deltaYVerlauf = false;
            accXVerlauf = false;
            accYVerlauf = true;
            darstellungsBereichNeu = false;
        }
    }

    private void DarstellungsbereichÄndern_Click(object sender, RoutedEventArgs e)
    {
        if (knoten == null)
        {
            _ = MessageBox.Show("Knoten muss erst ausgewählt werden", "dynamische Tragwerksberechnung");
        }
        else
        {
            VisualErgebnisse.Children.Clear();
            ausschnitt =
                new DarstellungsbereichDialog(ausschnittMin, ausschnittMax, absMaxVerformung, absMaxBeschleunigung);
            ausschnittMin = ausschnitt.tmin;
            ausschnittMax = ausschnitt.tmax;
            maxVerformung = ausschnitt.maxVerformung;
            maxBeschleunigung = ausschnitt.maxBeschleunigung;
            darstellungsBereichNeu = true;
            if (deltaXVerlauf) DeltaXNeuZeichnen();
            else if (deltaYVerlauf) DeltaYNeuZeichnen();
            else if (accXVerlauf) AccXNeuZeichnen();
            else if (accYVerlauf) AccYNeuZeichnen();
        }
    }

    private void MaximalwertText(string ordinate, double maxWert, double maxZeit)
    {
        var rot = FromArgb(120, 255, 0, 0);
        var myBrush = new SolidColorBrush(rot);
        var maxwert = "Maximalwert für " + ordinate + " = " + maxWert.ToString("N4") + Environment.NewLine +
                      "an Zeit = " + maxZeit.ToString("N2");
        maximal = new TextBlock
        {
            FontSize = 12,
            Background = myBrush,
            Foreground = Black,
            FontWeight = FontWeights.Bold,
            Text = maxwert
        };
        SetTop(maximal, 10);
        SetLeft(maximal, 20);
        VisualErgebnisse.Children.Add(maximal);
    }
}