using System.Text;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Shapes;

namespace FE_Berechnungen.Wärmeberechnung.Ergebnisse;

public partial class InstationäreModellzuständeVisualisieren
{
    private readonly Darstellung _darstellung;
    private readonly List<Shape> _hitList = [];
    private readonly FeModell _modell;
    private EllipseGeometry _hitArea;
    private int _index;
    private bool _knotenTemperaturAn, _knotenGradientenAn, _elementTemperaturAn;

    public InstationäreModellzuständeVisualisieren(FeModell modell)
    {
        _modell = modell;
        Language = XmlLanguage.GetLanguage("de-DE");
        InitializeComponent();
        Show();

        _darstellung = new Darstellung(modell, VisualErgebnisse);
        _darstellung.FestlegungAuflösung();
        _darstellung.AlleElementeZeichnen();

        // Auswahl des Zeitschritts
        var dt = modell.Zeitintegration.Dt;
        var tmax = modell.Zeitintegration.Tmax;
        var nSteps = (int)(tmax / dt) + 1;
        var zeit = new double[nSteps];
        for (var i = 0; i < nSteps; i++) zeit[i] = i * dt;
        Zeitschrittauswahl.ItemsSource = zeit;
    }

    private void DropDownZeitschrittauswahlClosed(object sender, EventArgs e)
    {
        if (Zeitschrittauswahl.SelectedIndex < 0)
        {
            _ = MessageBox.Show("kein gültiger Zeitschritt ausgewählt", "Zeitschrittauswahl");
            return;
        }

        _index = Zeitschrittauswahl.SelectedIndex;

        foreach (var item in _modell.Knoten) item.Value.Knotenfreiheitsgrade[0] = item.Value.KnotenVariable[0][_index];

        _darstellung.Zeitschritt = _index;
        KnotentemperaturenZeichnen();
        _darstellung.WärmeflussvektorenZeichnen();
        ElementTemperaturenZeichnen();
    }

    private void KnotentemperaturenZeichnen()
    {
        if (!_knotenTemperaturAn)
        {
            if (_index == 0)
            {
                _ = MessageBox.Show("Zeitschritt muss erst ausgewählt werden", "instationäre Wärmeberechnung");
            }
            else
            {
                _darstellung.KnotentemperaturZeichnen();
                _knotenTemperaturAn = true;
            }
        }
        else
        {
            // entferne ALLE Textdarstellungen der Knotentemperaturen
            foreach (var knotenTemp in _darstellung.Knotentemperaturen) VisualErgebnisse.Children.Remove(knotenTemp);
            _knotenTemperaturAn = false;
        }
    }

    private void ElementTemperaturenZeichnen()
    {
        if (!_elementTemperaturAn)
        {
            if (_index == 0)
            {
                _ = MessageBox.Show("Zeitschritt muss erst ausgewählt werden", "instationäre Wärmeberechnung");
            }
            else
            {
                _darstellung.ElementTemperaturZeichnen();
                _darstellung.WärmeflussvektorenZeichnen();
                _elementTemperaturAn = true;
            }
        }
        else
        {
            foreach (var path in _darstellung.TemperaturElemente) VisualErgebnisse.Children.Remove(path);
            _elementTemperaturAn = false;
        }
    }

    private void BtnKnotenTemperaturen_Click(object sender, RoutedEventArgs e)
    {
        KnotentemperaturenZeichnen();
    }

    private void BtnKnotenGradienten_Click(object sender, RoutedEventArgs e)
    {
        if (!_knotenGradientenAn)
        {
            if (_index == 0)
            {
                _ = MessageBox.Show("Zeitschritt muss erst ausgewählt werden", "instationäre Wärmeberechnung");
            }
            else
            {
                _darstellung.KnotengradientenZeichnen(_index);
                _knotenGradientenAn = true;
            }
        }
        else
        {
            // entferne ALLE Textdarstellungen der Knotentemperaturen
            foreach (var knotenGrad in _darstellung.Knotengradienten) VisualErgebnisse.Children.Remove(knotenGrad);
            _knotenGradientenAn = false;
        }
    }

    private void BtnElementTemperaturen_Click(object sender, RoutedEventArgs e)
    {
        ElementTemperaturenZeichnen();
    }

    private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        _hitList.Clear();
        var hitPoint = e.GetPosition(VisualErgebnisse);
        _hitArea = new EllipseGeometry(hitPoint, 0.2, 0.2);
        VisualTreeHelper.HitTest(VisualErgebnisse, null, HitTestCallBack,
            new GeometryHitTestParameters(_hitArea));

        MyPopup.IsOpen = false;

        var sb = new StringBuilder();
        foreach (var item in _hitList.Where(item => !((item == null) | (item?.Name == string.Empty))))
        {
            sb.Clear();
            MyPopup.IsOpen = true;

            if (!_modell.Elemente.TryGetValue(item.Name, out var element2D)) continue;
            sb.Clear();
            var wärmeElement = (Abstrakt2D)element2D;
            var wärmeFluss = wärmeElement.BerechneElementZustand(0, 0);

            sb.Append("Element = " + wärmeElement.ElementId);
            sb.Append("\nWärmefluss x\t= " + wärmeFluss[0].ToString("G4"));
            sb.Append("\nWärmefluss y\t= " + wärmeFluss[1].ToString("G4"));

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
                        _hitList.Add(hit);
                        break;
                        //case TextBlock hit:
                        //    hitTextBlock.Add(hit);
                        //    break;
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