using FEBibliothek.Modell;
using FEBibliothek.Modell.abstrakte_Klassen;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Shapes;

namespace FE_Berechnungen.Elastizitätsberechnung.Ergebnisse;

public partial class StatikErgebnisseVisualisieren
{
    private readonly FeModell _modell;
    private readonly Darstellung _darstellung;
    private bool _elementTexteAn = true, _knotenTexteAn = true, _verformungenAn, _spannungenAn, _reaktionenAn;
    private readonly List<object> _hitList = [];
    private readonly List<TextBlock> _hitTextBlock = new();
    private EllipseGeometry _hitArea;

    public StatikErgebnisseVisualisieren(FeModell feModell)
    {
        Language = XmlLanguage.GetLanguage("de-DE");
        InitializeComponent();
        Show();

        _modell = feModell;
        _darstellung = new Darstellung(feModell, VisualErgebnisse);

        // unverformte Geometrie
        _darstellung.ElementeZeichnen();

        // mit Element Ids
        _darstellung.ElementTexte();

        // mit Knoten Ids
        _darstellung.KnotenTexte();
    }

    private void BtnVerformung_Click(object sender, RoutedEventArgs e)
    {
        if (!_verformungenAn)
        {
            _darstellung.VerformteGeometrie();
            _verformungenAn = true;
        }
        else
        {
            for (var i = 0; i < _darstellung.Verformungen.Count; i++)
            {
                var path = (Shape)_darstellung.Verformungen[i];
                VisualErgebnisse.Children.Remove(path);
            }
            _verformungenAn = false;
        }
    }
    private void BtnSpannungen_Click(object sender, RoutedEventArgs e)
    {
        if (!_spannungenAn)
        {
            // zeichne Spannungsvektoren in Elementmitte
            _darstellung.SpannungenZeichnen();
            _spannungenAn = true;
        }
        else
        {
            // entferne Spannungsvektoren
            for (var i = 0; i < _darstellung.Spannungen.Count; i++)
            {
                var path = (Shape)_darstellung.Spannungen[i];
                VisualErgebnisse.Children.Remove(path);
            }
            _spannungenAn = false;
        }
    }
    private void Reaktionen_Click(object sender, RoutedEventArgs e)
    {
        if (!_reaktionenAn)
        {
            // zeichne Reaktionen an Festhaltungen
            _darstellung.ReaktionenZeichnen();
            _reaktionenAn = true;
        }
        else
        {
            // entferne Spannungsvektoren
            for (var i = 0; i < _darstellung.Reaktionen.Count; i++)
            {
                var path = (Shape)_darstellung.Reaktionen[i];
                VisualErgebnisse.Children.Remove(path);
            }
            _reaktionenAn = false;
        }
    }

    private void BtnElementIDs_Click(object sender, RoutedEventArgs e)
    {
        if (!_elementTexteAn)
        {
            _darstellung.ElementTexte();
            _elementTexteAn = true;
        }
        else
        {
            foreach (var id in _darstellung.ElementIDs.Cast<TextBlock>()) VisualErgebnisse.Children.Remove(id);
            _elementTexteAn = false;
        }
    }
    private void BtnKnotenIDs_Click(object sender, RoutedEventArgs e)
    {
        if (!_knotenTexteAn)
        {
            _darstellung.KnotenTexte();
            _knotenTexteAn = true;
        }
        else
        {
            foreach (var id in _darstellung.KnotenIDs.Cast<TextBlock>())
            {
                VisualErgebnisse.Children.Remove(id);
            }
            _knotenTexteAn = false;
        }
    }

    private void BtnÜberhöhung_Click(object sender, RoutedEventArgs e)
    {
        _darstellung.ÜberhöhungVerformung = double.Parse(Überhöhung.Text);
        foreach (var path in _darstellung.Verformungen.Cast<Shape>())
        {
            VisualErgebnisse.Children.Remove(path);
        }
        _verformungenAn = false;
        _darstellung.VerformteGeometrie();
        _verformungenAn = true;
    }
    private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        _hitList.Clear();
        _hitTextBlock.Clear();
        var hitPoint = e.GetPosition(VisualErgebnisse);
        _hitArea = new EllipseGeometry(hitPoint, 1.0, 1.0);
        VisualTreeHelper.HitTest(VisualErgebnisse, null, HitTestCallBack,
            new GeometryHitTestParameters(_hitArea));

        MyPopup.IsOpen = false;

        var sb = new StringBuilder();
        foreach (var item in _hitList.Where(item => item != null))
        {
            MyPopup.IsOpen = true;

            switch (item)
            {
                case Polygon polygon:
                {
                    sb.Clear();
                    if (_modell.Elemente.TryGetValue(polygon.Name, out AbstraktElement multiKnotenElement))
                    {
                        var element2D = (Abstrakt2D)multiKnotenElement;
                        var elementSpannungen = element2D.BerechneZustandsvektor();
                        sb.Append("Element = " + element2D.ElementId);
                        sb.Append("\nElementmitte sig-xx\t= " + elementSpannungen[0].ToString("F2"));
                        sb.Append("\nElementmitte sig-yy\t= " + elementSpannungen[1].ToString("F2"));
                        sb.Append("\nElementmitte sig-xy\t= " + elementSpannungen[2].ToString("F2"));
                    }
                    MyPopupText.Text = sb.ToString();
                    break;
                }
            }
        }

        foreach (var item in _hitTextBlock)
        {
            if (item == null) continue;
            MyPopup.IsOpen = true;
            if (_modell.Knoten.TryGetValue(item.Text, out var knoten))
            {
                sb.Append("Knoten = " + knoten.Id);
                sb.Append("\nux\t= " + knoten.Knotenfreiheitsgrade[0].ToString("F4"));
                sb.Append("\nuy\t= " + knoten.Knotenfreiheitsgrade[1].ToString("F4"));
                sb.Append("\n");
                if (knoten.Reaktionen != null)
                {
                    sb.Append("\nRx\t= " + knoten.Reaktionen[0].ToString("F4"));
                    sb.Append("\nRy\t= " + knoten.Reaktionen[1].ToString("F4"));
                }
            }
            else if (_modell.Elemente.TryGetValue(item.Text, out var element))
            {
                var element2D = (Abstrakt2D)element;
                var elementSpannungen = element2D.BerechneZustandsvektor();
                sb.Append("Element = " + element2D.ElementId);
                sb.Append("\nElementmitte sig-xx\t= " + elementSpannungen[0].ToString("F2"));
                sb.Append("\nElementmitte sig-yy\t= " + elementSpannungen[1].ToString("F2"));
                sb.Append("\nElementmitte sig-xy\t= " + elementSpannungen[2].ToString("F2"));
            }
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
                _hitList.Add(result.VisualHit as Shape);
                _hitTextBlock.Add(result.VisualHit as TextBlock);
                return HitTestResultBehavior.Continue;
            case IntersectionDetail.FullyInside:
                return HitTestResultBehavior.Continue;
            case IntersectionDetail.Intersects:
                _hitList.Add(result.VisualHit as Shape);
                _hitTextBlock.Add(result.VisualHit as TextBlock);
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

    //private void OnKeyDownHandler(object sender, KeyEventArgs e)
    //{
    //    if (e.Key == Key.Return)
    //    {
    //        überhöhung = Überhöhung.Text;
    //    }
    //}
}