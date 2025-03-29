using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Path = System.Windows.Shapes.Path;

namespace FE_Berechnungen.Wärmeberechnung.Ergebnisse;

public partial class StationäreErgebnisseVisualisieren
{
    private readonly List<object> _hitList = [];
    private readonly List<TextBlock> _hitTextBlock = [];
    private readonly FeModell _modell;
    public Darstellung Darstellung;
    private EllipseGeometry _hitArea;
    private bool _knotenTemperaturAn, _elementTemperaturAn, _wärmeflussAn;

    public StationäreErgebnisseVisualisieren(FeModell model)
    {
        Language = XmlLanguage.GetLanguage("de-DE");
        _modell = model;
        InitializeComponent();
    }

    private void ModelGrid_Loaded(object sender, RoutedEventArgs e)
    {
        Darstellung = new Darstellung(_modell, VisualWärmeErgebnisse);
        Darstellung.FestlegungAuflösung();
        Darstellung.AlleElementeZeichnen();
        Darstellung.KnotentemperaturZeichnen();
        _knotenTemperaturAn = true;
    }

    private void BtnKnotentemperatur_Click(object sender, RoutedEventArgs e)
    {
        if (!_knotenTemperaturAn)
        {
            // zeichne den Wert einer jeden Randbedingung als Text an Randknoten
            Darstellung.KnotentemperaturZeichnen();
            _knotenTemperaturAn = true;
        }
        else
        {
            // entferne ALLE Textdarstellungen der Knotentemperaturen
            foreach (var knotenTemp in Darstellung.Knotentemperaturen)
                VisualWärmeErgebnisse.Children.Remove(knotenTemp);
            _knotenTemperaturAn = false;
        }
    }

    private void BtnWärmefluss_Click(object sender, RoutedEventArgs e)
    {
        if (!_wärmeflussAn)
        {
            // zeichne ALLE resultierenden Wärmeflussvektoren in Elementschwerpunkten
            Darstellung.WärmeflussvektorenZeichnen();

            // zeichne den Wert einer jeden Randbedingung als Text an Randknoten
            Darstellung.RandbedingungenZeichnen();
            _wärmeflussAn = true;
        }
        else
        {
            // entferne ALLE resultierenden Wärmeflussvektoren in Elementschwerpunkten
            foreach (var path in Darstellung.WärmeVektoren) VisualWärmeErgebnisse.Children.Remove(path);

            // entferne ALLE Textdarstellungen der Randbedingungen
            foreach (var rand in Darstellung.RandKnoten) VisualWärmeErgebnisse.Children.Remove(rand);
            _wärmeflussAn = false;
        }
    }

    private void BtnElementTemperaturen_Click(object sender, RoutedEventArgs e)
    {
        if (!_elementTemperaturAn)
        {
            Darstellung.ElementTemperaturZeichnen();
            _elementTemperaturAn = true;
        }
        else
        {
            foreach (var path in Darstellung.TemperaturElemente) VisualWärmeErgebnisse.Children.Remove(path);
            _elementTemperaturAn = false;
        }
    }

    private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        _hitList.Clear();
        _hitTextBlock.Clear();
        var hitPoint = e.GetPosition(VisualWärmeErgebnisse);
        _hitArea = new EllipseGeometry(hitPoint, 1, 1);
        VisualTreeHelper.HitTest(VisualWärmeErgebnisse, null, HitTestCallBack,
            new GeometryHitTestParameters(_hitArea));

        MyPopup.IsOpen = true;

        var sb = new StringBuilder();
        var done = "";
        foreach (var item in _hitList.Where(item => item != null))
            switch (item)
            {
                case Polygon polygon:
                    {
                        MyPopup.IsOpen = true;
                        if (_modell.Elemente.TryGetValue(polygon.Name, out var multiKnotenElement))
                        {
                            var element2D = (Abstrakt2D)multiKnotenElement;
                            var elementTemperaturen = element2D.BerechneElementZustand(0, 0);
                            sb.Append("Element\t= " + element2D.ElementId);
                            sb.Append("\nElementmitte Tx\t= " + elementTemperaturen[0].ToString("F2"));
                            sb.Append("\nElementmitte Ty\t= " + elementTemperaturen[1].ToString("F2") + "\n");
                        }

                        MyPopupText.Text = sb.ToString();
                        break;
                    }
                case Path path:
                    {
                        if (path.Name == done) break;
                        MyPopup.IsOpen = true;
                        if (_modell.Elemente.TryGetValue(path.Name, out var multiKnotenElement))
                        {
                            var element2D = (Abstrakt2D)multiKnotenElement;
                            var elementTemperaturen = element2D.BerechneElementZustand(0, 0);
                            sb.Append("Element\t= " + element2D.ElementId);
                            sb.Append("\nElementmitte Tx\t= " + elementTemperaturen[0].ToString("F2"));
                            sb.Append("\nElementmitte Ty\t= " + elementTemperaturen[1].ToString("F2") + "\n");
                        }

                        MyPopupText.Text = sb.ToString();
                        done = path.Name;
                        break;
                    }
            }

        foreach (var item in _hitTextBlock.Where(item => item != null))
        {
            if (!_modell.Knoten.TryGetValue(item.Name, out var knoten)) continue;
            sb.Append("Knoten\t\t = " + knoten.Id);
            sb.Append("\nTemperatur\t= " + knoten.Knotenfreiheitsgrade[0].ToString("F2"));
            if (knoten.Reaktionen != null)
                sb.Append("\nWärmefluss\t= " + knoten.Reaktionen[0].ToString("F2"));
            MyPopupText.Text = sb.ToString();
            break;
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
        MyPopup.IsOpen = false;
    }
}