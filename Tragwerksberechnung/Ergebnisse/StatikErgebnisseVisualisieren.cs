using FE_Berechnungen.Tragwerksberechnung.Modelldaten;
using FEBibliothek.Modell;
using FEBibliothek.Modell.abstrakte_Klassen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Shapes;

namespace FE_Berechnungen.Tragwerksberechnung.Ergebnisse
{
    public partial class StatikErgebnisseVisualisieren
    {
        private readonly FeModell modell;
        private bool elementTexteAn = true, knotenTexteAn = true,
                     verformungenAn, normalkräfteAn, querkräfteAn, momenteAn;

        public readonly Darstellung darstellung;
        private readonly List<Shape> hitList = new List<Shape>();
        private readonly List<TextBlock> hitTextBlock = new List<TextBlock>();
        private EllipseGeometry hitArea;

        public StatikErgebnisseVisualisieren(FeModell feModell)
        {
            Language = XmlLanguage.GetLanguage("de-DE");
            modell = feModell;
            InitializeComponent();
            Show();

            darstellung = new Darstellung(modell, VisualErgebnisse);

            // unverformte Geometrie
            darstellung.UnverformteGeometrie();

            // mit Element Ids
            darstellung.ElementTexte();

            // mit Knoten Ids
            darstellung.KnotenTexte();

            // Faktor für Überhöhung des Verformungszustands
            darstellung.überhöhungVerformung = int.Parse(Verschiebung.Text);
            darstellung.überhöhungRotation = int.Parse(Rotation.Text);
        }

        private void BtnVerformung_Click(object sender, RoutedEventArgs e)
        {
            if (!verformungenAn)
            {
                darstellung.VerformteGeometrie();
                verformungenAn = true;
            }
            else
            {
                foreach (Shape path in darstellung.Verformungen)
                {
                    VisualErgebnisse.Children.Remove(path);
                }
                verformungenAn = false;
            }
        }

        private void BtnNormalkraft_Click(object sender, RoutedEventArgs e)
        {
            double maxNormalkraft = 0;
            if (querkräfteAn)
            {
                foreach (Shape path in darstellung.QuerkraftListe) { VisualErgebnisse.Children.Remove(path); }
                querkräfteAn = false;
            }
            if (momenteAn)
            {
                foreach (Shape path in darstellung.MomenteListe) { VisualErgebnisse.Children.Remove(path); }
                VisualErgebnisse.Children.Remove(darstellung.maxMomentText);
                momenteAn = false;
            }
            if (!normalkräfteAn)
            {
                // Bestimmung der maximalen Normalkraft
                IEnumerable<AbstraktBalken> Beams()
                {
                    foreach (var item in modell.Elemente)
                    {
                        if (item.Value is AbstraktBalken beam) { yield return beam; }
                    }
                }
                foreach (AbstraktBalken beam in Beams())
                {
                    var barEndForces = beam.BerechneStabendkräfte();
                    if (Math.Abs(barEndForces[0]) > maxNormalkraft) { maxNormalkraft = Math.Abs(barEndForces[0]); }
                    if (barEndForces.Length > 2)
                    {
                        if (Math.Abs(barEndForces[3]) > maxNormalkraft) { maxNormalkraft = Math.Abs(barEndForces[3]); }
                    }
                    else
                    {
                        if (Math.Abs(barEndForces[1]) > maxNormalkraft) { maxNormalkraft = Math.Abs(barEndForces[1]); }
                    }
                }

                // Skalierung der Normalkraftdarstellung und Darstellung aller Normalkraftverteilungen
                foreach (AbstraktBalken beam in Beams())
                {
                    _ = beam.BerechneStabendkräfte();
                    darstellung.Normalkraft_Zeichnen(beam, maxNormalkraft, false);
                }
                normalkräfteAn = true;
            }
            else
            {
                foreach (Shape path in darstellung.NormalkraftListe) { VisualErgebnisse.Children.Remove(path); }
                normalkräfteAn = false;
            }
        }

        private void BtnQuerkraft_Click(object sender, RoutedEventArgs e)
        {
            double maxQuerkraft = 0;
            if (normalkräfteAn)
            {
                foreach (Shape path in darstellung.NormalkraftListe) { VisualErgebnisse.Children.Remove(path); }
                normalkräfteAn = false;
            }
            if (momenteAn)
            {
                foreach (Shape path in darstellung.MomenteListe) { VisualErgebnisse.Children.Remove(path); }
                VisualErgebnisse.Children.Remove(darstellung.maxMomentText);
                momenteAn = false;
            }

            if (!querkräfteAn)
            {
                // Bestimmung der maximalen Querkraft
                IEnumerable<AbstraktBalken> Beams()
                {
                    foreach (KeyValuePair<string, AbstraktElement> item in modell.Elemente)
                    {
                        if (item.Value is AbstraktBalken beam) { yield return beam; }
                    }
                }
                foreach (AbstraktBalken beam in Beams())
                {
                    beam.ElementZustand = beam.BerechneStabendkräfte();
                    if (beam.ElementZustand.Length <= 2) { continue; }
                    if (Math.Abs(beam.ElementZustand[1]) > maxQuerkraft) { maxQuerkraft = Math.Abs(beam.ElementZustand[1]); }
                    if (Math.Abs(beam.ElementZustand[4]) > maxQuerkraft) { maxQuerkraft = Math.Abs(beam.ElementZustand[4]); }
                }

                // skalierte Querkraftverläufe zeichnen
                foreach (AbstraktBalken beam in Beams())
                {
                    var elementlast = false;
                    if (beam.ElementZustand.Length <= 2) { continue; }
                    if (Math.Abs(beam.ElementZustand[1] - beam.ElementZustand[4]) > double.Epsilon) { elementlast = true; }
                    darstellung.Querkraft_Zeichnen(beam, maxQuerkraft, elementlast);
                }
                querkräfteAn = true;
            }
            else
            {
                foreach (Shape path in darstellung.QuerkraftListe) { VisualErgebnisse.Children.Remove(path); }
                querkräfteAn = false;
            }
        }

        private void BtnMomente_Click(object sender, RoutedEventArgs e)
        {
            double maxMoment = 0;
            if (normalkräfteAn)
            {
                foreach (Shape path in darstellung.NormalkraftListe) { VisualErgebnisse.Children.Remove(path); }
                normalkräfteAn = false;
            }
            if (querkräfteAn)
            {
                foreach (Shape path in darstellung.QuerkraftListe) { VisualErgebnisse.Children.Remove(path); }
                querkräfteAn = false;
            }

            if (!momenteAn)
            {
                // Bestimmung des maximalen Biegemoments
                IEnumerable<AbstraktBalken> Beams()
                {
                    foreach (var item in modell.Elemente)
                    {
                        if (item.Value is AbstraktBalken beam) { yield return beam; }
                    }
                }
                foreach (var beam in Beams())
                {
                    beam.ElementZustand = beam.BerechneStabendkräfte();
                    if (beam.ElementZustand.Length <= 2) { continue; }
                    if (Math.Abs(beam.ElementZustand[2]) > maxMoment) { maxMoment = Math.Abs(beam.ElementZustand[2]); }
                    if (Math.Abs(beam.ElementZustand[5]) > maxMoment) { maxMoment = Math.Abs(beam.ElementZustand[5]); }
                }

                // falls Knotenmomente = 0, Bestimmung lokaler Elementmomente für Skalierung
                if (maxMoment < 1E-5)
                {
                    AbstraktElement element = null;
                    AbstraktBalken lastBalken;
                    double lokalesMoment;
                    IEnumerable<PunktLast> PunktLasten()
                    {
                        foreach (var last in modell.PunktLasten.Select(item =>
                            (PunktLast)item.Value).Where(last => modell.Elemente.TryGetValue(last.ElementId, out element)))
                        { yield return last; }
                    }
                    foreach (PunktLast last in PunktLasten())
                    {
                        lastBalken = (AbstraktBalken)element;
                        lokalesMoment = lastBalken.ElementZustand[1] * last.Offset * lastBalken.balkenLänge;
                        if (Math.Abs(lokalesMoment) > maxMoment) { maxMoment = Math.Abs(lokalesMoment); }
                    }

                    IEnumerable<LinienLast> LinienLasten()
                    {
                        foreach (LinienLast last in modell.ElementLasten.Select(item =>
                            (LinienLast)item.Value).Where(last => modell.Elemente.TryGetValue(last.ElementId, out element)))
                        { yield return last; }
                    }
                    foreach (LinienLast last in LinienLasten())
                    {
                        lastBalken = (AbstraktBalken)element;
                        var stabEndkräfte = lastBalken.ElementZustand;
                        // für Skalierung nur Gleichlast mit max. Lastordinate betrachtet
                        double max = Math.Abs(last.Lastwerte[1]);
                        if (Math.Abs(last.Lastwerte[3]) > max) max = last.Lastwerte[3];
                        lokalesMoment = stabEndkräfte[1] * lastBalken.balkenLänge / 2 -
                                         max * lastBalken.balkenLänge / 2 * lastBalken.balkenLänge / 4;
                        if (Math.Abs(lokalesMoment) > maxMoment) { maxMoment = Math.Abs(lokalesMoment); }
                    }
                }

                // Skalierung der Momentendarstellung und Momentenverteilung für alle Biegebalken zeichnen
                foreach (AbstraktBalken beam in Beams())
                {
                    bool elementlast = false;
                    if (beam.ElementZustand.Length <= 2) { continue; }
                    if (Math.Abs(beam.ElementZustand[1] - beam.ElementZustand[4]) > double.Epsilon) { elementlast = true; }
                    darstellung.Momente_Zeichnen(beam, maxMoment, elementlast);
                }
                momenteAn = true;
            }
            else
            {
                foreach (Shape path in darstellung.MomenteListe) { VisualErgebnisse.Children.Remove(path); }
                foreach (TextBlock maxWerte in darstellung.MaxTexte) { VisualErgebnisse.Children.Remove(maxWerte); }
                momenteAn = false;
            }
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

        private void BtnRotation_Click(object sender, RoutedEventArgs e)
        {
            darstellung.überhöhungRotation = int.Parse(Rotation.Text);
            foreach (Shape path in darstellung.Verformungen) { VisualErgebnisse.Children.Remove(path); }
            verformungenAn = false;
            darstellung.VerformteGeometrie();
            verformungenAn = true;
        }

        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // In der Methode "UnverformteGeometrie" werden Elemente und Knoten als Path bzw TextBlock gezeichnet.
            // Deren IDs werden als "Name" an jeden einzelnen Path bzw als Text an jeden einzelnen TextBlock angehängt
            // Shapes und TextBlocks werden am Hit-Punkt gesammelt und nach ID ausgewertet
            hitList.Clear();
            hitTextBlock.Clear();
            var hitPoint = e.GetPosition(VisualErgebnisse);
            hitArea = new EllipseGeometry(hitPoint, 0.2, 0.2);
            VisualTreeHelper.HitTest(VisualErgebnisse, null, HitTestCallBack,
                new GeometryHitTestParameters(hitArea));

            MyPopup.IsOpen = false;

            var sb = new StringBuilder();
            foreach (var item in hitList.Where(item => !(item == null | item?.Name == string.Empty)))
            {
                sb.Clear();
                MyPopup.IsOpen = true;

                if (!modell.Elemente.TryGetValue(item.Name, out var linienElement)) continue;
                sb.Clear();
                if (linienElement is FederElement) continue;
                var balken = (AbstraktBalken)linienElement;
                var balkenEndKräfte = balken.BerechneStabendkräfte();

                switch (balkenEndKräfte.Length)
                {
                    case 2:
                        sb.Append("Element = " + balken.ElementId);
                        sb.Append("\nNa\t= " + balkenEndKräfte[0].ToString("F2"));
                        sb.Append("\nNb\t= " + balkenEndKräfte[1].ToString("F2"));
                        break;
                    case 6:
                        sb.Append("Element = " + linienElement.ElementId);
                        sb.Append("\nNa\t= " + balkenEndKräfte[0].ToString("F2"));
                        sb.Append("\nQa\t= " + balkenEndKräfte[1].ToString("F2"));
                        sb.Append("\nMa\t= " + balkenEndKräfte[2].ToString("F2"));
                        sb.Append("\nNb\t= " + balkenEndKräfte[3].ToString("F2"));
                        sb.Append("\nQb\t= " + balkenEndKräfte[4].ToString("F2"));
                        sb.Append("\nMb\t= " + balkenEndKräfte[5].ToString("F2"));
                        break;
                }
                sb.Append("\n");
                MyPopupText.Text = sb.ToString();
            }

            foreach (var item in hitTextBlock)
            {
                if (item == null) { continue; }
                if (item.Text == string.Empty) { continue; }

                sb.Clear();
                MyPopup.IsOpen = true;
                if (modell.Knoten.TryGetValue(item.Text, out var knoten))
                {
                    sb.Append("Knoten = " + knoten.Id);
                    sb.Append("\nux\t= " + knoten.Knotenfreiheitsgrade[0].ToString("F4"));
                    sb.Append("\nuy\t= " + knoten.Knotenfreiheitsgrade[1].ToString("F4"));
                    if (knoten.Knotenfreiheitsgrade.Length == 3)
                        sb.Append("\nphi\t= " + knoten.Knotenfreiheitsgrade[2].ToString("F4"));
                    if (knoten.Reaktionen != null)
                    {
                        for (var i = 0; i < knoten.Reaktionen.Length; i++)
                        {
                            sb.Append("\nLagerreaktion " + i + "\t=" + knoten.Reaktionen[i].ToString("F2"));
                        }
                    }
                    MyPopupText.Text = sb.ToString();
                    break;
                }

                if (!modell.Elemente.TryGetValue(item.Text, out var linienElement)) { continue; }
                sb.Clear();
                if (linienElement is FederElement)
                {
                    linienElement.BerechneZustandsvektor();
                    sb.Append("Feder = " + linienElement.ElementId);
                    sb.Append("\nFx\t= " + linienElement.ElementZustand[0].ToString("F2"));
                    sb.Append("\nFy\t= " + linienElement.ElementZustand[1].ToString("F2"));
                    sb.Append("\nM\t= " + linienElement.ElementZustand[2].ToString("F2"));
                }
                else
                {
                    var balken = (AbstraktBalken)linienElement;
                    var balkenEndKräfte = balken.BerechneStabendkräfte();

                    switch (balkenEndKräfte.Length)
                    {
                        case 2:
                            sb.Append("Element = " + balken.ElementId);
                            sb.Append("\nNa\t= " + balkenEndKräfte[0].ToString("F2"));
                            sb.Append("\nNb\t= " + balkenEndKräfte[1].ToString("F2"));
                            break;
                        case 6:
                            sb.Append("Element = " + linienElement.ElementId);
                            sb.Append("\nNa\t= " + balkenEndKräfte[0].ToString("F2"));
                            sb.Append("\nQa\t= " + balkenEndKräfte[1].ToString("F2"));
                            sb.Append("\nMa\t= " + balkenEndKräfte[2].ToString("F2"));
                            sb.Append("\nNb\t= " + balkenEndKräfte[3].ToString("F2"));
                            sb.Append("\nQb\t= " + balkenEndKräfte[4].ToString("F2"));
                            sb.Append("\nMb\t= " + balkenEndKräfte[5].ToString("F2"));
                            break;
                    }
                }
                MyPopupText.Text = sb.ToString();
            }
        }

        private HitTestResultBehavior HitTestCallBack(HitTestResult result)
        {
            var intersectionDetail = ((GeometryHitTestResult)result).IntersectionDetail;

            switch (intersectionDetail)
            {
                case IntersectionDetail.Empty:
                    return HitTestResultBehavior.Continue;
                case IntersectionDetail.FullyContains:
                    switch (result.VisualHit)
                    {
                        case Shape hit:
                            hitList.Add(hit);
                            break;
                        case TextBlock hit:
                            hitTextBlock.Add(hit);
                            break;
                    }
                    return HitTestResultBehavior.Continue;
                case IntersectionDetail.FullyInside:
                    return HitTestResultBehavior.Continue;
                case IntersectionDetail.Intersects:
                    switch (result.VisualHit)
                    {
                        case Shape hit:
                            hitList.Add(hit);
                            break;
                    }
                    return HitTestResultBehavior.Continue;
                case IntersectionDetail.NotCalculated:
                    return HitTestResultBehavior.Continue;
                default:
                    return HitTestResultBehavior.Stop;
            }
        }
        private void OnMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            MyPopup.IsOpen = false;
        }
    }
}