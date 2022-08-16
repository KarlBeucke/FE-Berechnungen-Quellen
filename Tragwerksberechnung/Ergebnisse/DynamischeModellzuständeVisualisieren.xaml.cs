using FEBibliothek.Modell;
using FEBibliothek.Modell.abstrakte_Klassen;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Shapes;
using static System.Windows.Controls.Canvas;
using static System.Windows.Media.Brushes;

namespace FE_Berechnungen.Tragwerksberechnung.Ergebnisse
{
    public partial class DynamischeModellzuständeVisualisieren
    {
        private readonly FeModell modell;
        private readonly double dt;
        private readonly int nSteps;
        private int dropDownIndex, index, indexN, indexQ, indexM;

        private readonly Darstellung darstellung;

        private bool elementTexteAn = true, knotenTexteAn = true,
                     verformungenAn, normalkräfteAn, querkräfteAn, momenteAn;
        private double maxNormalkraft, maxQuerkraft, maxMoment;
        private TextBlock maximalWerte;

        public DynamischeModellzuständeVisualisieren(FeModell feModel)
        {
            Language = XmlLanguage.GetLanguage("de-DE");
            modell = feModel;

            InitializeComponent();
            Show();

            // Auswahl des Zeitschritts
            dt = modell.Zeitintegration.Dt;
            var tmax = modell.Zeitintegration.Tmax;

            // Auswahl des Zeitschritts aus Zeitraster, z.B. jeder 10.
            nSteps = (int)(tmax / dt);
            const int zeitraster = 1;
            //if (nSteps > 1000) zeitraster = 10;
            nSteps = (nSteps / zeitraster) + 1;
            var zeit = new double[nSteps];
            for (var i = 0; i < nSteps; i++) { zeit[i] = (i * dt * zeitraster); }

            darstellung = new Darstellung(modell, VisualErgebnisse);
            darstellung.UnverformteGeometrie();

            // mit Knoten und Element Ids
            darstellung.KnotenTexte();
            darstellung.ElementTexte();

            MaximalwerteGesamterZeitverlauf();
            Zeitschrittauswahl.ItemsSource = zeit;
        }

        private void DropDownZeitschrittauswahlClosed(object sender, EventArgs e)
        {
            if (Zeitschrittauswahl.SelectedIndex < 0)
            {
                _ = MessageBox.Show("kein gültiger Zeitschritt ausgewählt", "Zeitschrittauswahl");
                return;
            }
            dropDownIndex = Zeitschrittauswahl.SelectedIndex;
            index = dropDownIndex;
            foreach (var item in modell.Knoten)
                for (var i = 0; i < item.Value.AnzahlKnotenfreiheitsgrade; i++)
                    item.Value.Knotenfreiheitsgrade[i] = item.Value.KnotenVariable[i][index];

            verformungenAn = false;
            normalkräfteAn = false;
            querkräfteAn = false;
            momenteAn = false;
        }

        private void BtnVerformung_Click(object sender, RoutedEventArgs e)
        {
            if (index == 0)
            {
                _ = MessageBox.Show("Zeitschritt muss erst ausgewählt werden", "Tragwerksberechnung");
                return;
            }

            if (verformungenAn)
            {
                foreach (Shape path in darstellung.Verformungen)
                    VisualErgebnisse.Children.Remove(path);
                index++;
                AktuellerZeitschritt.Text = "aktuelle Integrationszeit = " + (index * dt).ToString(CultureInfo.InvariantCulture);
                if (index >= nSteps)
                {
                    _ = MessageBox.Show("Ende der Zeitschrittberechnung", "Tragwerksberechnung");
                    index = dropDownIndex;
                    verformungenAn = false;
                    return;
                }
            }
            else
            {
                index = dropDownIndex;
                Clean();
            }

            foreach (var item in modell.Knoten)
                for (var i = 0; i < item.Value.AnzahlKnotenfreiheitsgrade; i++)
                    item.Value.Knotenfreiheitsgrade[i] = item.Value.KnotenVariable[i][index];
            darstellung.VerformteGeometrie();
            verformungenAn = true;
            normalkräfteAn = false;
            querkräfteAn = false;
            momenteAn = false;
        }

        private void BtnNormalkraft_Click(object sender, RoutedEventArgs e)
        {
            if (index == 0)
            {
                _ = MessageBox.Show("Zeitschritt muss erst ausgewählt werden", "Tragwerksberechnung");
                return;
            }

            if (normalkräfteAn)
            {
                foreach (Shape path in darstellung.NormalkraftListe)
                    VisualErgebnisse.Children.Remove(path);
                index++;
                AktuellerZeitschritt.Text = "aktuelle Integrationszeit = " + (index * dt).ToString(CultureInfo.InvariantCulture);
                if (index >= nSteps)
                {
                    _ = MessageBox.Show("Ende der Zeitschrittberechnung", "Tragwerksberechnung");
                    index = dropDownIndex;
                    normalkräfteAn = false;
                    return;
                }
            }
            else
            {
                index = dropDownIndex;
                Clean();
            }

            foreach (var item in modell.Knoten)
                for (var i = 0; i < item.Value.AnzahlKnotenfreiheitsgrade; i++)
                    item.Value.Knotenfreiheitsgrade[i] = item.Value.KnotenVariable[i][index];
            // Skalierung der Normalkraftdarstellung und Darstellung aller Normalkraftverteilungen
            foreach (var beam in modell.Elemente.
                Select(item => item.Value).OfType<AbstraktBalken>())
            {
                _ = beam.BerechneStabendkräfte();
                darstellung.Normalkraft_Zeichnen(beam, maxNormalkraft, false);
            }
            verformungenAn = false;
            normalkräfteAn = true;
            querkräfteAn = false;
            momenteAn = false;
        }

        private void Clean()
        {
            //index = dropDownIndex;
            foreach (Shape path in darstellung.Verformungen)
                VisualErgebnisse.Children.Remove(path);
            foreach (Shape path in darstellung.NormalkraftListe)
                VisualErgebnisse.Children.Remove(path);
            foreach (Shape path in darstellung.QuerkraftListe)
                VisualErgebnisse.Children.Remove(path);
            foreach (Shape path in darstellung.MomenteListe)
                VisualErgebnisse.Children.Remove(path);
        }

        private void BtnQuerkraft_Click(object sender, RoutedEventArgs e)
        {
            if (index == 0)
            {
                _ = MessageBox.Show("Zeitschritt muss erst ausgewählt werden", "Tragwerksberechnung");
                return;
            }

            if (querkräfteAn)
            {
                foreach (Shape path in darstellung.QuerkraftListe)
                    VisualErgebnisse.Children.Remove(path);
                index++;
                AktuellerZeitschritt.Text = "aktuelle Integrationszeit = " + (index * dt).ToString(CultureInfo.InvariantCulture);
                if (index >= nSteps)
                {
                    _ = MessageBox.Show("Ende der Zeitschrittberechnung", "Tragwerksberechnung");
                    index = dropDownIndex;
                    querkräfteAn = false;
                    return;
                }
            }
            else
            {
                index = dropDownIndex;
                Clean();
            }

            foreach (var item in modell.Knoten)
                for (var i = 0; i < item.Value.AnzahlKnotenfreiheitsgrade; i++)
                    item.Value.Knotenfreiheitsgrade[i] = item.Value.KnotenVariable[i][index];
            // Skalierung der Querkraftdarstellung und Darstellung aller Querkraftverteilungen
            foreach (var beam in modell.Elemente.
                Select(item => item.Value).OfType<AbstraktBalken>())
            {
                _ = beam.BerechneStabendkräfte();
                darstellung.Querkraft_Zeichnen(beam, maxQuerkraft, false);
            }

            verformungenAn = false;
            normalkräfteAn = false;
            querkräfteAn = true;
            momenteAn = false;
        }

        private void BtnBiegemoment_Click(object sender, RoutedEventArgs e)
        {
            if (index == 0)
            {
                _ = MessageBox.Show("Zeitschritt muss erst ausgewählt werden", "Tragwerksberechnung");
                return;
            }

            if (momenteAn)
            {
                foreach (Shape path in darstellung.MomenteListe) 
                    VisualErgebnisse.Children.Remove(path);
                index++;
                AktuellerZeitschritt.Text = "aktuelle Integrationszeit = " + (index * dt).ToString(CultureInfo.InvariantCulture);
                if (index >= nSteps)
                {
                    _ = MessageBox.Show("Ende der Zeitschrittberechnung", "Tragwerksberechnung");
                    index = dropDownIndex;
                    momenteAn = false;
                    return;
                }
            }
            else
            {
                index = dropDownIndex;
                Clean();
            }

            foreach (var item in modell.Knoten)
                for (var i = 0; i < item.Value.AnzahlKnotenfreiheitsgrade; i++)
                    item.Value.Knotenfreiheitsgrade[i] = item.Value.KnotenVariable[i][index];
            // Skalierung der Momentendarstellung und Darstellung aller Momentverteilungen
            foreach (var beam in modell.Elemente.
                Select(item => item.Value).OfType<AbstraktBalken>())
            {
                _ = beam.BerechneStabendkräfte();
                darstellung.Momente_Zeichnen(beam, maxMoment, false);
            }
            verformungenAn = false;
            normalkräfteAn = false;
            querkräfteAn = false;
            momenteAn = true;
        }

        private void MaximalwerteGesamterZeitverlauf()
        {
            double maxUx = 0, minUx = 0, maxUy = 0, minUy = 0;
            string knotenUxMax ="", knotenUxMin ="", knotenUyMax = "", knotenUyMin = "";
            double maxUxZeit = 0, minUxZeit = 0, maxUyZeit = 0, minUyZeit = 0;
            var sb = new StringBuilder();
            foreach (var item in modell.Knoten)
            {
                var temp = item.Value.KnotenVariable[0].Max();
                if (maxUx < temp)
                {
                    maxUx = temp;
                    knotenUxMax = item.Value.Id;
                    maxUxZeit = dt * Array.IndexOf(item.Value.KnotenVariable[0], maxUx);
                }
                temp = item.Value.KnotenVariable[0].Min();
                if (minUx > temp)
                {
                    minUx = temp;
                    knotenUxMin = item.Value.Id;
                    minUxZeit = dt * Array.IndexOf(item.Value.KnotenVariable[0], minUx);
                }

                temp = item.Value.KnotenVariable[1].Max();
                if (maxUy < temp)
                {
                    maxUy = temp;
                    knotenUyMax = item.Value.Id;
                    maxUyZeit = dt * Array.IndexOf(item.Value.KnotenVariable[1], maxUy);
                }
                temp = item.Value.KnotenVariable[1].Min();
                if (minUy > temp)
                {
                    minUy = temp;
                    knotenUyMin = item.Value.Id;
                    minUyZeit = dt * Array.IndexOf(item.Value.KnotenVariable[1], minUy);
                }
            }

            if (knotenUxMax.Length == 0)
            {
                sb.Append("ux = " + maxUx.ToString("G4"));
                sb.Append(", max. uy = " + maxUy.ToString("G4") + ", an Knoten "
                          + knotenUyMax + " zur Zeit " + maxUyZeit.ToString("G4")
                          + ", min. uy = " + minUy.ToString("G4") + ", an Knoten "
                          + knotenUyMin + " zur Zeit " + minUyZeit.ToString("G4"));
            }
            else if (knotenUyMax.Length == 0)
            {
                sb.Append("max. ux = " + maxUx.ToString("G4") + ", an Knoten "
                          + knotenUxMax + " zur Zeit " + maxUxZeit.ToString("G4")
                          + ", min. ux = " + minUx.ToString("G4") + ", an Knoten "
                          + knotenUxMin + " zur Zeit " + minUxZeit.ToString("G4"));
                sb.Append(", uy = " + maxUy.ToString("G4"));
            }
            else
            {
                sb.Append("max. ux = " + maxUx.ToString("G4") + ", an Knoten "
                          + knotenUxMax + " zur Zeit " + maxUxZeit.ToString("G4")
                          + ", min. ux = " + minUx.ToString("G4") + ", an Knoten "
                          + knotenUxMin + " zur Zeit " + minUxZeit.ToString("G4"));
                sb.Append(", max. uy = " + maxUy.ToString("G4") + ", an Knoten "
                          + knotenUyMax + " zur Zeit " + maxUyZeit.ToString("G4")
                          + ", min. uy = " + minUy.ToString("G4") + ", an Knoten "
                          + knotenUyMin + " zur Zeit " + minUyZeit.ToString("G4"));
            }
            var maximalVerformungen = new TextBlock
            {
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                Text = sb.ToString(),
                Foreground = Red
            };
            SetTop(maximalVerformungen, 0);
            SetLeft(maximalVerformungen, 5);
            VisualErgebnisse.Children.Add(maximalVerformungen);

            // Schleife über alle Zeitschritte
            for (var i = 0; i < nSteps; i++)
            {
                foreach (var item in modell.Knoten)
                    for (var k = 0; k < item.Value.AnzahlKnotenfreiheitsgrade; k++)
                        item.Value.Knotenfreiheitsgrade[k] = item.Value.KnotenVariable[k][i];

                IEnumerable<AbstraktBalken> Beams()
                {
                    foreach (var item in modell.Elemente)
                        if (item.Value is AbstraktBalken element)
                            yield return element;
                }

                // Zustand aller Fachwerk- und Biegebalkenelemente an einem Zeitschritt
                foreach (var element in Beams())
                {
                    element.ElementZustand = element.BerechneStabendkräfte();

                    // Fachwerkstäbe
                    if (element.ElementZustand.Length == 2)
                    {
                        if (Math.Abs(element.ElementZustand[0]) > maxNormalkraft) { indexN = i; maxNormalkraft = Math.Abs(element.ElementZustand[0]); }

                        if (!(Math.Abs(element.ElementZustand[1]) > maxNormalkraft)) continue;
                        indexN = i; maxNormalkraft = Math.Abs(element.ElementZustand[1]);
                    }

                    // Biegebalken
                    else
                    {
                        if (Math.Abs(element.ElementZustand[0]) > maxNormalkraft) { indexN = i; maxNormalkraft = Math.Abs(element.ElementZustand[0]); }
                        if (Math.Abs(element.ElementZustand[3]) > maxNormalkraft) { indexN = i; maxNormalkraft = Math.Abs(element.ElementZustand[3]); }
                        if (Math.Abs(element.ElementZustand[1]) > maxQuerkraft) { indexQ = i; maxQuerkraft = Math.Abs(element.ElementZustand[1]); }
                        if (Math.Abs(element.ElementZustand[4]) > maxQuerkraft) { indexQ = i; maxQuerkraft = Math.Abs(element.ElementZustand[4]); }
                        if (Math.Abs(element.ElementZustand[2]) > maxMoment) { indexM = i; maxMoment = Math.Abs(element.ElementZustand[2]); }
                        if (Math.Abs(element.ElementZustand[5]) > maxMoment) { indexM = i; maxMoment = Math.Abs(element.ElementZustand[5]); }
                    }
                }
            }
            maximalWerte = new TextBlock
            {
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                Text = "max Normalkraft = " + maxNormalkraft.ToString("G4") + " nach Zeit = " + (indexN * dt).ToString("N2") +
                     ", max Querkraft = " + maxQuerkraft.ToString("G4") + " nach Zeit = " + (indexQ * dt).ToString("N2") +
                     " und max Moment = " + maxMoment.ToString("G4") + " nach Zeit = " + (indexM * dt).ToString("N2"),
                Foreground = Red
            };
            SetTop(maximalWerte, 20);
            SetLeft(maximalWerte, 5);
            VisualErgebnisse.Children.Add(maximalWerte);
        }

        private void BtnElementIDs_Click(object sender, RoutedEventArgs e)
        {
            if (!elementTexteAn)
            {
                darstellung.ElementTexte();
                elementTexteAn = true;
            }
            else
            {
                foreach (TextBlock id in darstellung.ElementIDs) { VisualErgebnisse.Children.Remove(id); }
                elementTexteAn = false;
            }
        }
        private void BtnKnotenIDs_Click(object sender, RoutedEventArgs e)
        {
            if (!knotenTexteAn)
            {
                darstellung.KnotenTexte();
                knotenTexteAn = true;
            }
            else
            {
                foreach (TextBlock id in darstellung.KnotenIDs) { VisualErgebnisse.Children.Remove(id); }
                knotenTexteAn = false;
            }
        }

        private void BtnVerschiebung_Click(object sender, RoutedEventArgs e)
        {
            darstellung.überhöhungVerformung = int.Parse(Verschiebung.Text);
            foreach (Shape path in darstellung.Verformungen) { VisualErgebnisse.Children.Remove(path); }
            verformungenAn = false;
            darstellung.VerformteGeometrie();
            verformungenAn = true;
        }
    }
}