using System.Text;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace FE_Berechnungen.Elastizitätsberechnung.ModelldatenAnzeigen;

public partial class ElastizitätsmodellVisualisieren
{
    private readonly Darstellung darstellung;
    private readonly List<Shape> hitList = new();
    private readonly List<TextBlock> hitTextBlock = new();
    private readonly FeModell modell;
    private EllipseGeometry hitArea;
    private bool lastenAn = true, lagerAn = true, knotenTexteAn = true, elementTexteAn = true;

    public ElastizitätsmodellVisualisieren(FeModell feModell)
    {
        modell = feModell;
        InitializeComponent();
        Show();
        darstellung = new Darstellung(feModell, VisualErgebnisse);
        darstellung.ElementeZeichnen();

        // mit Element und Knoten Ids
        darstellung.KnotenTexte();
        darstellung.ElementTexte();
        darstellung.LastenZeichnen();
        darstellung.FesthaltungenZeichnen();
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

    private void BtnLasten_Click(object sender, RoutedEventArgs e)
    {
        if (!lastenAn)
        {
            darstellung.LastenZeichnen();
            lastenAn = true;
        }
        else
        {
            foreach (Shape lasten in darstellung.LastVektoren) VisualErgebnisse.Children.Remove(lasten);
            lastenAn = false;
        }
    }

    private void BtnFesthaltungen_Click(object sender, RoutedEventArgs e)
    {
        if (!lagerAn)
        {
            darstellung.FesthaltungenZeichnen();
            lagerAn = true;
        }
        else
        {
            foreach (Shape path in darstellung.LagerDarstellung) VisualErgebnisse.Children.Remove(path);
            lagerAn = false;
        }
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
        foreach (var item in hitList.Where(item => item != null))
        {
            MyPopup.IsOpen = true;

            switch (item)
            {
                case not null:
                    {
                        if (item.Name == null) continue;
                        if (modell.Elemente.TryGetValue(item.Name, out var element))
                        {
                            sb.Append("\nElement\t= " + element.ElementId);

                            foreach (var id in element.KnotenIds)
                                if (modell.Knoten.TryGetValue(id, out var knoten))
                                {
                                    sb.Append("\nKnoten " + id + "\t= " + knoten.Koordinaten[0]);
                                    for (var k = 1; k < knoten.Koordinaten.Length; k++)
                                        sb.Append(", " + knoten.Koordinaten[k]);
                                }

                            if (modell.Material.TryGetValue(element.ElementMaterialId, out var material))
                            {
                                sb.Append("\nMaterial\t= " + element.ElementMaterialId + "\t= " +
                                          material.MaterialWerte[0]);

                                for (var i = 1; i < material.MaterialWerte.Length; i++)
                                    sb.Append(", " + material.MaterialWerte[i].ToString("g3"));
                            }
                        }

                        if (modell.Lasten.TryGetValue(item.Name, out var knotenlast))
                        {
                            sb.Append("Last\t= " + item.Name);
                            for (var i = 0; i < knotenlast.Lastwerte.Length; i++)
                                sb.Append("\nLastwert " + i + "\t= " + knotenlast.Lastwerte[i]);
                        }

                        sb.Append("\n");
                    }
                    break;
            }
        }

        foreach (var item in hitTextBlock.Where(item => item != null))
        {
            sb.Clear();
            MyPopup.IsOpen = true;

            if (modell.Knoten.TryGetValue(item.Text, out var knoten))
            {
                sb.Append("Knoten\t= " + knoten.Id);
                for (var i = 0; i < knoten.Koordinaten.Length; i++)
                    sb.Append("\nKoordinate " + i + "\t= " + knoten.Koordinaten[i].ToString("g3"));
            }

            if (modell.Elemente.TryGetValue(item.Text, out var element))
            {
                sb.Append("Element\t= " + element.ElementId);
                for (var i = 0; i < element.KnotenIds.Length; i++)
                    sb.Append("\nKnoten " + i + "\t= " + element.KnotenIds[i]);
                if (modell.Material.TryGetValue(element.ElementMaterialId, out var material))
                    sb.Append("\nE-Modul\t= " + material.MaterialWerte[0].ToString("g3"));
                if (modell.Querschnitt.TryGetValue(element.ElementQuerschnittId, out var querschnitt))
                {
                    sb.Append("\nFläche\t= " + querschnitt.QuerschnittsWerte[0]);
                    if (querschnitt.QuerschnittsWerte.Length > 1)
                        sb.Append("\nIxx\t= " + querschnitt.QuerschnittsWerte[1].ToString("g3"));
                }
            }
        }

        MyPopupText.Text = sb.ToString();
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