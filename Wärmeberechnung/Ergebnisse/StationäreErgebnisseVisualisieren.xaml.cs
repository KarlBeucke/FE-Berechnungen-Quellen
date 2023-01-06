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

namespace FE_Berechnungen.Wärmeberechnung.Ergebnisse;

public partial class StationäreErgebnisseVisualisieren
{
    private readonly FeModell modell;
    public Darstellung darstellung;
    private bool knotenTemperaturAn, elementTemperaturAn, wärmeflussAn;
    private readonly List<object> hitList = new();
    private readonly List<TextBlock> hitTextBlock = new();
    private EllipseGeometry hitArea;

    public StationäreErgebnisseVisualisieren(FeModell model)
    {
        Language = XmlLanguage.GetLanguage("de-DE");
        modell = model;
        InitializeComponent();
    }
    private void ModelGrid_Loaded(object sender, RoutedEventArgs e)
    {
        darstellung = new Darstellung(modell, VisualWärmeErgebnisse);
        darstellung.FestlegungAuflösung();
        darstellung.AlleElementeZeichnen();
        darstellung.KnotentemperaturZeichnen();
        knotenTemperaturAn = true;
    }

    private void BtnKnotentemperatur_Click(object sender, RoutedEventArgs e)
    {
        if (!knotenTemperaturAn)
        {
            // zeichne den Wert einer jeden Randbedingung als Text an Randknoten
            darstellung.KnotentemperaturZeichnen();
            knotenTemperaturAn = true;
        }
        else
        {
            // entferne ALLE Textdarstellungen der Knotentemperaturen
            foreach (var knotenTemp in darstellung.Knotentemperaturen)
            {
                VisualWärmeErgebnisse.Children.Remove(knotenTemp);
            }
            knotenTemperaturAn = false;
        }
    }

    private void BtnWärmefluss_Click(object sender, RoutedEventArgs e)
    {
        if (!wärmeflussAn)
        {
            // zeichne ALLE resultierenden Wärmeflussvektoren in Elementschwerpunkten
            darstellung.WärmeflussvektorenZeichnen();

            // zeichne den Wert einer jeden Randbedingung als Text an Randknoten
            darstellung.RandbedingungenZeichnen();
            wärmeflussAn = true;
        }
        else
        {
            // entferne ALLE resultierenden Wärmeflussvektoren in Elementschwerpunkten
            foreach (Shape path in darstellung.WärmeVektoren)
            {
                VisualWärmeErgebnisse.Children.Remove(path);
            }

            // entferne ALLE Textdarstellungen der Randbedingungen
            foreach (var rand in darstellung.RandKnoten)
            {
                VisualWärmeErgebnisse.Children.Remove(rand);
            }
            wärmeflussAn = false;
        }
    }

    private void BtnElementTemperaturen_Click(object sender, RoutedEventArgs e)
    {
        if (!elementTemperaturAn)
        {
            darstellung.ElementTemperaturZeichnen();
            elementTemperaturAn = true;
        }
        else
        {
            foreach (var path in darstellung.TemperaturElemente)
            {
                VisualWärmeErgebnisse.Children.Remove(path);
            }
            elementTemperaturAn = false;
        }
    }

    private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        hitList.Clear();
        hitTextBlock.Clear();
        var hitPoint = e.GetPosition(VisualWärmeErgebnisse);
        hitArea = new EllipseGeometry(hitPoint, 1, 1);
        VisualTreeHelper.HitTest(VisualWärmeErgebnisse, null, HitTestCallBack,
            new GeometryHitTestParameters(hitArea));

        MyPopup.IsOpen = true;

        var sb = new StringBuilder();
        string done="";
        foreach (var item in hitList.Where(item => item != null))
        {
            switch (item)
            {
                case Polygon polygon:
                {
                    MyPopup.IsOpen = true;
                    if (modell.Elemente.TryGetValue(polygon.Name, out var multiKnotenElement))
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
                    if (modell.Elemente.TryGetValue(path.Name, out var multiKnotenElement))
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
        }

        foreach (var item in hitTextBlock.Where(item => item != null))
        {
            if (!modell.Knoten.TryGetValue(item.Name, out var knoten)) continue;
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