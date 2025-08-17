using FE_Berechnungen.Tragwerksberechnung.Modelldaten;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace FE_Berechnungen.Tragwerksberechnung.Ergebnisse
{
    public partial class ErgebnisseVisualisieren
    {
        private readonly List<Shape> _hitList = [];
        private readonly List<TextBlock> _hitTextBlock = [];
        private readonly FeModell _modell;

        public readonly Darstellung Darstellung;

        private bool _elementTexteAn = true,
            _knotenTexteAn = true,
            _verformungenAn,
            _normalkräfteAn,
            _querkräfteAn,
            _momenteAn;

        private EllipseGeometry _hitArea;
        private bool _momentenMaxTexte;
        public ErgebnisseVisualisieren(FeModell feModell)
        {
            Language = XmlLanguage.GetLanguage("de-DE");
            _modell = feModell;
            InitializeComponent();
            Show();

            Darstellung = new Darstellung(_modell, VisualTragwerkErgebnisse);

            // unverformte Geometrie
            Darstellung.UnverformteGeometrie();

            // mit Element Ids
            Darstellung.ElementTexte();

            // mit Knoten Ids
            Darstellung.KnotenTexte();

            // Faktor für Überhöhung des Verformungszustands
            Darstellung.Überhöhung = int.Parse(Überhöhung.Text);
        }

        private void BtnVerformung_Click(object sender, RoutedEventArgs e)
        {
            if (!_verformungenAn)
            {
                Darstellung.VerformteGeometrie();
                _verformungenAn = true;
            }
            else
            {
                foreach (var path in Darstellung.Verformungen.Cast<Shape>()) VisualTragwerkErgebnisse.Children.Remove(path);
                _verformungenAn = false;
            }
        }

        private void BtnNormalkraft_Click(object sender, RoutedEventArgs e)
        {
            double maxNormalkraft = 0;
            if (_querkräfteAn)
            {
                foreach (var path in Darstellung.QuerkraftListe.Cast<Shape>())
                    VisualTragwerkErgebnisse.Children.Remove(path);
                _querkräfteAn = false;
            }

            if (_momenteAn)
            {
                foreach (var path in Darstellung.MomenteListe.Cast<Shape>()) VisualTragwerkErgebnisse.Children.Remove(path);
                VisualTragwerkErgebnisse.Children.Remove(Darstellung.MaxMomentText);
                _momenteAn = false;
            }

            if (!_normalkräfteAn)
            {
                // Bestimmung der maximalen Normalkraft
                IEnumerable<AbstraktBalken> Beams()
                {
                    foreach (var item in _modell.Elemente)
                        if (item.Value is AbstraktBalken beam)
                            yield return beam;
                }

                foreach (var beam in Beams())
                {
                    var barEndForces = beam.BerechneStabendkräfte();
                    if (Math.Abs(barEndForces[0]) > maxNormalkraft) maxNormalkraft = Math.Abs(barEndForces[0]);
                    if (barEndForces.Length > 2)
                    {
                        if (Math.Abs(barEndForces[3]) > maxNormalkraft) maxNormalkraft = Math.Abs(barEndForces[3]);
                    }
                    else
                    {
                        if (Math.Abs(barEndForces[1]) > maxNormalkraft) maxNormalkraft = Math.Abs(barEndForces[1]);
                    }
                }

                // Skalierung der Normalkraftdarstellung und Darstellung aller Normalkraftverteilungen
                foreach (var beam in Beams())
                {
                    _ = beam.BerechneStabendkräfte();
                    Darstellung.Normalkraft_Zeichnen(beam, maxNormalkraft, false);
                }

                _normalkräfteAn = true;
            }
            else
            {
                foreach (var path in Darstellung.NormalkraftListe.Cast<Shape>())
                    VisualTragwerkErgebnisse.Children.Remove(path);
                _normalkräfteAn = false;
            }
        }

        private void BtnQuerkraft_Click(object sender, RoutedEventArgs e)
        {
            double maxQuerkraft = 0;
            if (_normalkräfteAn)
            {
                foreach (var path in Darstellung.NormalkraftListe.Cast<Shape>())
                    VisualTragwerkErgebnisse.Children.Remove(path);
                _normalkräfteAn = false;
            }

            if (_momenteAn)
            {
                foreach (var path in Darstellung.MomenteListe.Cast<Shape>()) VisualTragwerkErgebnisse.Children.Remove(path);
                VisualTragwerkErgebnisse.Children.Remove(Darstellung.MaxMomentText);
                _momenteAn = false;
            }

            if (!_querkräfteAn)
            {
                // Bestimmung der maximalen Querkraft
                IEnumerable<AbstraktBalken> Beams()
                {
                    foreach (var item in _modell.Elemente)
                        if (item.Value is AbstraktBalken beam)
                            yield return beam;
                }

                foreach (var beam in Beams())
                {
                    beam.ElementZustand = beam.BerechneStabendkräfte();
                    if (beam.ElementZustand.Length <= 2) continue;

                    if (Math.Abs(beam.ElementZustand[1]) > maxQuerkraft) maxQuerkraft = Math.Abs(beam.ElementZustand[1]);

                    if (Math.Abs(beam.ElementZustand[4]) > maxQuerkraft) maxQuerkraft = Math.Abs(beam.ElementZustand[4]);
                }

                // skalierte Querkraftverläufe zeichnen
                foreach (var beam in Beams())
                {
                    var elementlast = false;
                    if (beam.ElementZustand.Length <= 2) continue;
                    if (Math.Abs(beam.ElementZustand[1] - beam.ElementZustand[4]) > double.Epsilon) elementlast = true;
                    Darstellung.Querkraft_Zeichnen(beam, maxQuerkraft, elementlast);
                }

                _querkräfteAn = true;
            }
            else
            {
                foreach (var path in Darstellung.QuerkraftListe.Cast<Shape>())
                    VisualTragwerkErgebnisse.Children.Remove(path);
                _querkräfteAn = false;
            }
        }

        private void BtnMomente_Click(object sender, RoutedEventArgs e)
        {
            double maxMoment = 0;
            if (_normalkräfteAn)
            {
                foreach (var path in Darstellung.NormalkraftListe.Cast<Shape>())
                    VisualTragwerkErgebnisse.Children.Remove(path);
                _normalkräfteAn = false;
            }

            if (_querkräfteAn)
            {
                foreach (var path in Darstellung.QuerkraftListe.Cast<Shape>())
                    VisualTragwerkErgebnisse.Children.Remove(path);
                _querkräfteAn = false;
            }

            if (!_momenteAn)
            {
                // Bestimmung des maximalen Biegemoments
                IEnumerable<AbstraktBalken> Beams()
                {
                    foreach (var item in _modell.Elemente)
                        if (item.Value is AbstraktBalken beam)
                            yield return beam;
                }

                foreach (var beam in Beams())
                {
                    beam.ElementZustand = beam.BerechneStabendkräfte();
                    if (beam.ElementZustand.Length <= 2) continue;
                    if (Math.Abs(beam.ElementZustand[2]) > maxMoment) maxMoment = Math.Abs(beam.ElementZustand[2]);
                    if (Math.Abs(beam.ElementZustand[5]) > maxMoment) maxMoment = Math.Abs(beam.ElementZustand[5]);
                }

                // falls Knotenmomente = 0, Bestimmung lokaler Elementmomente für Skalierung
                if (maxMoment < 1E-5)
                {
                    AbstraktElement element = null;
                    AbstraktBalken lastBalken;
                    double lokalesMoment;

                    IEnumerable<PunktLast> PunktLasten()
                    {
                        foreach (var last in _modell.PunktLasten.Select(item =>
                                     (PunktLast)item.Value).Where(last =>
                                     _modell.Elemente.TryGetValue(last.ElementId, out element)))
                            yield return last;
                    }

                    foreach (var last in PunktLasten())
                    {
                        lastBalken = (AbstraktBalken)element;
                        lokalesMoment = lastBalken.ElementZustand[1] * last.Offset * lastBalken.BalkenLänge;
                        if (Math.Abs(lokalesMoment) > maxMoment) maxMoment = Math.Abs(lokalesMoment);
                    }

                    IEnumerable<LinienLast> LinienLasten()
                    {
                        foreach (var last in _modell.ElementLasten.Select(item =>
                                     (LinienLast)item.Value).Where(last =>
                                     _modell.Elemente.TryGetValue(last.ElementId, out element)))
                            yield return last;
                    }

                    foreach (var last in LinienLasten())
                    {
                        lastBalken = (AbstraktBalken)element;
                        var stabEndkräfte = lastBalken.ElementZustand;
                        // für Skalierung nur Gleichlast mit max. Lastordinate betrachtet
                        var max = Math.Abs(last.Lastwerte[1]);
                        if (Math.Abs(last.Lastwerte[3]) > max) max = last.Lastwerte[3];
                        lokalesMoment = stabEndkräfte[1] * lastBalken.BalkenLänge / 2 -
                                        max * lastBalken.BalkenLänge / 2 * lastBalken.BalkenLänge / 4;
                        if (Math.Abs(lokalesMoment) > maxMoment) maxMoment = Math.Abs(lokalesMoment);
                    }
                }

                // Skalierung der Momentendarstellung und Momentenverteilung für alle Biegebalken zeichnen
                foreach (var beam in Beams())
                {
                    var elementlast = false;
                    if (beam.ElementZustand.Length <= 2) continue;
                    if (Math.Abs(beam.ElementZustand[1] - beam.ElementZustand[4]) > double.Epsilon) elementlast = true;
                    Darstellung.Momente_Zeichnen(beam, maxMoment, elementlast);
                }

                _momenteAn = true;
            }
            else
            {
                foreach (var path in Darstellung.MomenteListe.Cast<Shape>()) VisualTragwerkErgebnisse.Children.Remove(path);

                foreach (var maxWerte in Darstellung.MomentenMaxTexte.Cast<TextBlock>())
                    VisualTragwerkErgebnisse.Children.Remove(maxWerte);
                _momenteAn = false;
            }
        }

        private void BtnElementIDs_Click(object sender, RoutedEventArgs e)
        {
            if (!_elementTexteAn)
            {
                Darstellung.ElementTexte();
                _elementTexteAn = true;
            }
            else
            {
                foreach (var id in Darstellung.ElementIDs.Cast<TextBlock>()) VisualTragwerkErgebnisse.Children.Remove(id);
                _elementTexteAn = false;
            }
        }

        private void BtnKnotenIDs_Click(object sender, RoutedEventArgs e)
        {
            if (!_knotenTexteAn)
            {
                Darstellung.KnotenTexte();
                _knotenTexteAn = true;
            }
            else
            {
                foreach (var id in Darstellung.KnotenIDs.Cast<TextBlock>()) VisualTragwerkErgebnisse.Children.Remove(id);
                _knotenTexteAn = false;
            }
        }

        private void BtnÜberhöhung_Click(object sender, RoutedEventArgs e)
        {
            Darstellung.Überhöhung = int.Parse(Überhöhung.Text);
            foreach (var path in Darstellung.Verformungen.Cast<Shape>()) VisualTragwerkErgebnisse.Children.Remove(path);
            _verformungenAn = false;
            Darstellung.VerformteGeometrie();
            _verformungenAn = true;
        }

        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // In der Methode "UnverformteGeometrie" werden Elemente und Knoten als Path bzw TextBlock gezeichnet.
            // Deren IDs werden als "Name" an jeden einzelnen Path bzw als Text an jeden einzelnen TextBlock angehängt
            // Shapes und TextBlocks werden am Hit-Punkt gesammelt und nach ID ausgewertet
            _hitList.Clear();
            _hitTextBlock.Clear();
            var hitPoint = e.GetPosition(VisualTragwerkErgebnisse);
            _hitArea = new EllipseGeometry(hitPoint, 0.2, 0.2);
            VisualTreeHelper.HitTest(VisualTragwerkErgebnisse, null, HitTestCallBack,
                new GeometryHitTestParameters(_hitArea));

            var sb = new StringBuilder();

            foreach (var item in _hitTextBlock.Where(item => item != null).Where(item => item.Text != string.Empty))
            {
                sb.Clear();
                ErgebnisPopup.IsOpen = false;

                if (_modell.Knoten.TryGetValue(item.Text, out var knoten))
                {
                    sb.Append("Knoten = " + knoten.Id);
                    sb.Append("\nux\t= " + knoten.Knotenfreiheitsgrade[0].ToString("F4"));
                    sb.Append("\nuy\t= " + knoten.Knotenfreiheitsgrade[1].ToString("F4"));
                    if (knoten.Knotenfreiheitsgrade.Length == 3)
                        sb.Append("\nphi\t= " + knoten.Knotenfreiheitsgrade[2].ToString("F4"));
                    if (knoten.Reaktionen != null)
                        for (var i = 0; i < knoten.Reaktionen.Length; i++)
                            sb.Append("\nLagerreaktion " + i + "\t=" + knoten.Reaktionen[i].ToString("F2"));
                    ErgebnisPopup.IsOpen = true;
                    ErgebnisPopupText.Text = sb.ToString();
                    return;
                }

                if (!_modell.Elemente.TryGetValue(item.Text, out var linienElement)) continue;
                sb.Clear();
                if (linienElement is FederElement)
                {
                    linienElement.BerechneZustandsvektor();
                    sb.Append("Feder = " + linienElement.ElementId);
                    sb.Append("\nFx\t= " + linienElement.ElementZustand[0].ToString("F2"));
                    sb.Append("\nFy\t= " + linienElement.ElementZustand[1].ToString("F2"));
                    sb.Append("\nM\t= " + linienElement.ElementZustand[2].ToString("F2"));
                    ErgebnisPopupText.Text = sb.ToString();
                }

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
                ErgebnisPopup.IsOpen = true;
                ErgebnisPopupText.Text = sb.ToString();
                return;
            }

            foreach (var unused in _hitList.Where(item => item is { Name: "Biegemomente" }))
            {
                ErgebnisPopup.IsOpen = false;
                if (_momentenMaxTexte)
                {
                    foreach (var momentenMaxText in Darstellung.MomentenMaxTexte.Cast<TextBlock>())
                        VisualTragwerkErgebnisse.Children.Add(momentenMaxText);
                    _momentenMaxTexte = false;
                }
                else
                {
                    foreach (var momentenMaxText in Darstellung.MomentenMaxTexte.Cast<TextBlock>())
                        VisualTragwerkErgebnisse.Children.Remove(momentenMaxText);
                    _momentenMaxTexte = true;
                }
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
                            _hitList.Add(hit);
                            break;
                        case TextBlock hit:
                            _hitTextBlock.Add(hit);
                            break;
                    }

                    return HitTestResultBehavior.Continue;
                case IntersectionDetail.FullyInside:
                    return HitTestResultBehavior.Continue;
                case IntersectionDetail.Intersects:
                    switch (result.VisualHit)
                    {
                        case Shape hit:
                            _hitList.Add(hit);
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
            ErgebnisPopup.IsOpen = false;
        }
    }
}