using FEBibliothek.Modell;
using FEBibliothek.Modell.abstrakte_Klassen;
using System.Collections.Generic;
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
    private readonly FeModell modell;
    private readonly Darstellung darstellung;
    private bool elementTexteAn = true, knotenTexteAn = true, verformungenAn, spannungenAn, reaktionenAn;
    private readonly List<object> hitList = new List<object>();
    private readonly List<TextBlock> hitTextBlock = new List<TextBlock>();
    private EllipseGeometry hitArea;

    public StatikErgebnisseVisualisieren(FeModell feModell)
    {
        Language = XmlLanguage.GetLanguage("de-DE");
        InitializeComponent();
        Show();

        modell = feModell;
        darstellung = new Darstellung(feModell, VisualErgebnisse);

        // unverformte Geometrie
        darstellung.ElementeZeichnen();

        // mit Element Ids
        darstellung.ElementTexte();

        // mit Knoten Ids
        darstellung.KnotenTexte();
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
    private void BtnSpannungen_Click(object sender, RoutedEventArgs e)
    {
        if (!spannungenAn)
        {
            // zeichne Spannungsvektoren in Elementmitte
            darstellung.SpannungenZeichnen();
            spannungenAn = true;
        }
        else
        {
            // entferne Spannungsvektoren
            foreach (Shape path in darstellung.Spannungen)
            {
                VisualErgebnisse.Children.Remove(path);
            }
            spannungenAn = false;
        }
    }
    private void Reaktionen_Click(object sender, RoutedEventArgs e)
    {
        if (!reaktionenAn)
        {
            // zeichne Reaktionen an Festhaltungen
            darstellung.ReaktionenZeichnen();
            reaktionenAn = true;
        }
        else
        {
            // entferne Spannungsvektoren
            foreach (Shape path in darstellung.Reaktionen)
            {
                VisualErgebnisse.Children.Remove(path);
            }
            reaktionenAn = false;
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
            foreach (TextBlock id in darstellung.ElementIDs) VisualErgebnisse.Children.Remove(id);
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
            foreach (TextBlock id in darstellung.KnotenIDs) VisualErgebnisse.Children.Remove(id);
            knotenTexteAn = false;
        }
    }

    private void BtnÜberhöhung_Click(object sender, RoutedEventArgs e)
    {
        darstellung.überhöhungVerformung = double.Parse(Überhöhung.Text);
        foreach (Shape path in darstellung.Verformungen)
        {
            VisualErgebnisse.Children.Remove(path);
        }
        verformungenAn = false;
        darstellung.VerformteGeometrie();
        verformungenAn = true;
    }
    private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        hitList.Clear();
        hitTextBlock.Clear();
        var hitPoint = e.GetPosition(VisualErgebnisse);
        hitArea = new EllipseGeometry(hitPoint, 1.0, 1.0);
        VisualTreeHelper.HitTest(VisualErgebnisse, null, HitTestCallBack,
            new GeometryHitTestParameters(hitArea));

        MyPopup.IsOpen = false;

        var sb = new StringBuilder();
        foreach (var item in hitList)
        {
            if (item == null) continue;
            MyPopup.IsOpen = true;

            switch (item)
            {
                case Polygon polygon:
                    {
                        sb.Clear();
                        if (modell.Elemente.TryGetValue(polygon.Name, out AbstraktElement multiKnotenElement))
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

        foreach (var item in hitTextBlock)
        {
            if (item == null) continue;
            MyPopup.IsOpen = true;
            if (modell.Knoten.TryGetValue(item.Text, out var knoten))
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
            else if (modell.Elemente.TryGetValue(item.Text, out var element))
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
                hitList.Add(result.VisualHit as Shape);
                hitTextBlock.Add(result.VisualHit as TextBlock);
                return HitTestResultBehavior.Continue;
            case IntersectionDetail.FullyInside:
                return HitTestResultBehavior.Continue;
            case IntersectionDetail.Intersects:
                hitList.Add(result.VisualHit as Shape);
                hitTextBlock.Add(result.VisualHit as TextBlock);
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