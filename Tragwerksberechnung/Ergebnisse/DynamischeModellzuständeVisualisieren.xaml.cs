using FEBibliothek.Modell;
using FEBibliothek.Modell.abstrakte_Klassen;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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
        private readonly FEModell modell;
        private readonly double dt;
        private readonly int nSteps;
        private int dropDownIndex, index, indexN, indexQ, indexM;

        private readonly Darstellung darstellung;

        private bool elementTexteAn = true, knotenTexteAn = true,
                     verformungenAn, normalkräfteAn, querkräfteAn, momenteAn;
        private double maxNormalkraft, maxQuerkraft, maxMoment;
        private TextBlock maximalWerte;

        public DynamischeModellzuständeVisualisieren(FEModell feModel)
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
                {
                    VisualErgebnisse.Children.Remove(path);
                }
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
            {
                for (var i = 0; i < item.Value.AnzahlKnotenfreiheitsgrade; i++)
                {
                    item.Value.Knotenfreiheitsgrade[i] = item.Value.KnotenVariable[i][index];
                }
            }
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
                {
                    VisualErgebnisse.Children.Remove(path);
                }
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
            {
                for (var i = 0; i < item.Value.AnzahlKnotenfreiheitsgrade; i++)
                {
                    item.Value.Knotenfreiheitsgrade[i] = item.Value.KnotenVariable[i][index];
                }
            }
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
            {
                VisualErgebnisse.Children.Remove(path);
            }
            foreach (Shape path in darstellung.NormalkraftListe)
            {
                VisualErgebnisse.Children.Remove(path);
            }
            foreach (Shape path in darstellung.QuerkraftListe)
            {
                VisualErgebnisse.Children.Remove(path);
            }
            foreach (Shape path in darstellung.MomenteListe)
            {
                VisualErgebnisse.Children.Remove(path);
            }
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
                {
                    VisualErgebnisse.Children.Remove(path);
                }
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
            {
                for (var i = 0; i < item.Value.AnzahlKnotenfreiheitsgrade; i++)
                {
                    item.Value.Knotenfreiheitsgrade[i] = item.Value.KnotenVariable[i][index];
                }
            }
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
                {
                    VisualErgebnisse.Children.Remove(path);
                }
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
            {
                for (var i = 0; i < item.Value.AnzahlKnotenfreiheitsgrade; i++)
                {
                    item.Value.Knotenfreiheitsgrade[i] = item.Value.KnotenVariable[i][index];
                }
            }
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

            // Schleife über alle Zeitschritte
            for (var i = 0; i < nSteps; i++)
            {
                foreach (var item in modell.Knoten)
                {
                    for (var k = 0; k < item.Value.AnzahlKnotenfreiheitsgrade; k++)
                    {
                        item.Value.Knotenfreiheitsgrade[k] = item.Value.KnotenVariable[k][i];
                    }
                }

                IEnumerable<AbstraktBalken> Beams()
                {
                    foreach (var item in modell.Elemente)
                    {
                        if (item.Value is AbstraktBalken element)
                        {
                            yield return element;
                        }
                    }
                }

                // Zustand aller Fachwerk- und Biegebalkenelemente an einem Zeitschritt
                foreach (var element in Beams())
                {
                    element.ElementZustand = element.BerechneStabendkräfte();

                    // Fachwerkstäbe
                    if (element.ElementZustand.Length == 2)
                    {
                        if (Math.Abs(element.ElementZustand[0]) > maxNormalkraft) { indexN = i; maxNormalkraft = Math.Abs(element.ElementZustand[0]); }
                        if (Math.Abs(element.ElementZustand[1]) > maxNormalkraft) { indexN = i; maxNormalkraft = Math.Abs(element.ElementZustand[1]); }
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
                FontSize = 12,
                Text = "maximale Normalkraft = " + maxNormalkraft.ToString("N0") + " nach Zeit = " + (indexN * dt).ToString("N2") +
                     ", maximaleQuerkraft = " + maxQuerkraft.ToString("N0") + " nach Zeit = " + (indexQ * dt).ToString("N2") +
                     " und maximales Moment = " + maxMoment.ToString("N0") + " nach Zeit = " + (indexM * dt).ToString("N2"),
                Foreground = Blue
            };
            SetTop(maximalWerte, 0);
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