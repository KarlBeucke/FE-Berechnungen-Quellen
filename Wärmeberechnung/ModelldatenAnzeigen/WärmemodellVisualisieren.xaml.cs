using FEBibliothek.Modell;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Shapes;

namespace FE_Berechnungen.Wärmeberechnung.ModelldatenAnzeigen;

public partial class WärmemodellVisualisieren
{
    private readonly FeModell modell;
    private bool knotenAn = true, elementeAn = true;
    private bool knotenLastAn, elementLastAn, randbedingungAn;
    public readonly Darstellung darstellung;
    private readonly List<Shape> hitList = new();
    private readonly List<TextBlock> hitTextBlock = new();
    private EllipseGeometry hitArea;

    public WärmemodellVisualisieren(FeModell feModell)
    {
        modell = feModell;
        Language = XmlLanguage.GetLanguage("de-DE");
        InitializeComponent();
        Show();

        darstellung = new Darstellung(feModell, VisualModel);
        darstellung.AlleElementeZeichnen();

        // mit Knoten und Element Ids
        darstellung.KnotenTexte();
        darstellung.ElementTexte();
    }

    private void BtnKnoten_Click(object sender, RoutedEventArgs e)
    {
        if (!knotenAn)
        {
            darstellung.KnotenTexte();
            knotenAn = true;
        }
        else
        {
            foreach (TextBlock id in darstellung.KnotenIDs) VisualModel.Children.Remove(id);
            knotenAn = false;
        }
    }
    private void BtnElemente_Click(object sender, RoutedEventArgs e)
    {
        if (!elementeAn)
        {
            darstellung.ElementTexte();
            elementeAn = true;
        }
        else
        {
            foreach (TextBlock id in darstellung.ElementIDs) VisualModel.Children.Remove(id);
            elementeAn = false;
        }
    }

    private void BtnKnotenlasten_Click(object sender, RoutedEventArgs e)
    {
        if (!knotenLastAn)
        {
            darstellung.KnotenlastenZeichnen();
            knotenLastAn = true;
        }
        else
        {
            foreach (TextBlock id in darstellung.LastKnoten) VisualModel.Children.Remove(id);
            knotenLastAn = false;
        }
    }
    private void BtnElementlasten_Click(object sender, RoutedEventArgs e)
    {
        if (!elementLastAn)
        {
            darstellung.ElementlastenZeichnen();
            elementLastAn = true;
        }
        else
        {
            foreach (Shape lastElement in darstellung.LastElemente) VisualModel.Children.Remove(lastElement);
            elementLastAn = false;
        }
    }
    private void BtnRandbedingungen_Click(object sender, RoutedEventArgs e)
    {
        if (!randbedingungAn)
        {
            darstellung.RandbedingungenZeichnen();
            randbedingungAn = true;
        }
        else
        {
            foreach (TextBlock randbedingung in darstellung.RandKnoten)
            {
                VisualModel.Children.Remove(randbedingung);
            }
            randbedingungAn = false;
        }
    }
    private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        MyPopup.IsOpen = false;
        hitList.Clear();
        hitTextBlock.Clear();
        var hitPoint = e.GetPosition(VisualModel);
        hitArea = new EllipseGeometry(hitPoint, 1.0, 1.0);
        VisualTreeHelper.HitTest(VisualModel, null, HitTestCallBack,
            new GeometryHitTestParameters(hitArea));

        var sb = new StringBuilder();
        foreach (var item in hitList)
        {
            MyPopup.IsOpen = true;

            if (modell.Elemente.TryGetValue(item.Name, out var element))
            {
                sb.Append("Element\t= " + element.ElementId);
                foreach (var id in element.KnotenIds)
                {
                    if (modell.Knoten.TryGetValue(id, out var knoten))
                    {
                        sb.Append("\nKnoten " + id + " ("
                                  + knoten.Koordinaten[0].ToString("g2") + ";"
                                  + knoten.Koordinaten[1].ToString("g2") + ")");
                    }
                }

                if (modell.Material.TryGetValue(element.ElementMaterialId, out var material))
                {
                    sb.Append("\nLeitfähigkeit = " + material.MaterialWerte[0].ToString("g3"));
                }
            }

            foreach (var last in modell.ElementLasten.
                         Where(last => last.Value.ElementId == item.Name))
            {
                sb.Append("\nElementlast = " + last.Value.Lastwerte[0].ToString("g2") + ", "
                          + last.Value.Lastwerte[1].ToString("g2") + ", "
                          + last.Value.Lastwerte[2].ToString("g2"));
            }

            sb.Append("\n");
            MyPopupText.Text = sb.ToString();
        }

        foreach (var item in hitTextBlock.Where(item => item != null))
        {
            MyPopup.IsOpen = true;
            if (item.Name == "Element")
            {
                if (modell.Elemente.TryGetValue(item.Text, out var element))
                {
                    sb.Append("Element\t= " + element.ElementId);
                    foreach (var id in element.KnotenIds)
                    {
                        if (modell.Knoten.TryGetValue(id, out var knoten))
                        {
                            sb.Append("\nKnoten " + id + " ("
                                      + knoten.Koordinaten[0].ToString("g2") + ";"
                                      + knoten.Koordinaten[1].ToString("g2") + ")");
                        }
                    }
                    if (modell.Material.TryGetValue(element.ElementMaterialId, out var material))
                    {
                        sb.Append("\nLeitfähigkeit = " + material.MaterialWerte[0].ToString("g3") + "\n");
                    }
                }

                foreach (var last in modell.ElementLasten.
                             Where(last => last.Value.ElementId == item.Text))
                {
                    sb.Append("\nElementlast = " + last.Value.Lastwerte[0].ToString("g2") + ", "
                              + last.Value.Lastwerte[1].ToString("g2") + ", "
                              + last.Value.Lastwerte[2].ToString("g2"));
                }

                MyPopupText.Text = sb.ToString();
                break;
            }

            if (item.Name == "Knoten")
            {
                if (modell.Knoten.TryGetValue(item.Text, out var knoten))
                {
                    sb.Append("Knoten\t= " + knoten.Id + " ("
                              + knoten.Koordinaten[0].ToString("g2") + ";"
                              + knoten.Koordinaten[1].ToString("g2") + ")");
                }
                foreach (var last in modell.Lasten.
                             Where(last => last.Value.KnotenId == item.Text))
                {
                    sb.Append("\nKnotenlast = " + last.Value.Lastwerte[0].ToString("g2") + "\n");
                }
                foreach (var rand in modell.Randbedingungen.
                             Where(rand => rand.Value.KnotenId == item.Text))
                {
                    sb.Append("\nvordefinierte Randtemperatur = " + rand.Value.Vordefiniert[0].ToString("g2"));
                }

                MyPopupText.Text = sb.ToString();
                break;
            }

            if (item.Name != "Support") continue;
            {
                if (modell.Randbedingungen.TryGetValue(item.Uid, out _))
                {
                    if (modell.Randbedingungen.TryGetValue(item.Uid, out var rand))
                    {
                        sb.Append("vordefinierte Temperatur " + rand.RandbedingungId + " an Knoten " + rand.KnotenId);
                    }

                    MyPopupText.Text = sb.ToString();
                }
                break;
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