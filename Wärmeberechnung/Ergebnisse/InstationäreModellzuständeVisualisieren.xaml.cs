using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Shapes;
using FEBibliothek.Modell;
using FEBibliothek.Modell.abstrakte_Klassen;

namespace FE_Berechnungen.Wärmeberechnung.Ergebnisse;

public partial class InstationäreModellzuständeVisualisieren
{
    private readonly Darstellung darstellung;
    private readonly List<Shape> hitList = new();
    private readonly FeModell modell;
    private EllipseGeometry hitArea;
    private int index;
    private bool knotenTemperaturAn, knotenGradientenAn, elementTemperaturAn;

    public InstationäreModellzuständeVisualisieren(FeModell modell)
    {
        this.modell = modell;
        Language = XmlLanguage.GetLanguage("de-DE");
        InitializeComponent();
        Show();

        darstellung = new Darstellung(modell, VisualErgebnisse);
        darstellung.FestlegungAuflösung();
        darstellung.AlleElementeZeichnen();

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

        index = Zeitschrittauswahl.SelectedIndex;

        foreach (var item in modell.Knoten) item.Value.Knotenfreiheitsgrade[0] = item.Value.KnotenVariable[0][index];

        darstellung.zeitschritt = index;
        KnotentemperaturenZeichnen();
        darstellung.WärmeflussvektorenZeichnen();
        ElementTemperaturenZeichnen();
    }

    private void KnotentemperaturenZeichnen()
    {
        if (!knotenTemperaturAn)
        {
            if (index == 0)
            {
                _ = MessageBox.Show("Zeitschritt muss erst ausgewählt werden", "instationäre Wärmeberechnung");
            }
            else
            {
                darstellung.KnotentemperaturZeichnen();
                knotenTemperaturAn = true;
            }
        }
        else
        {
            // entferne ALLE Textdarstellungen der Knotentemperaturen
            foreach (var knotenTemp in darstellung.Knotentemperaturen) VisualErgebnisse.Children.Remove(knotenTemp);
            knotenTemperaturAn = false;
        }
    }

    private void ElementTemperaturenZeichnen()
    {
        if (!elementTemperaturAn)
        {
            if (index == 0)
            {
                _ = MessageBox.Show("Zeitschritt muss erst ausgewählt werden", "instationäre Wärmeberechnung");
            }
            else
            {
                darstellung.ElementTemperaturZeichnen();
                darstellung.WärmeflussvektorenZeichnen();
                elementTemperaturAn = true;
            }
        }
        else
        {
            foreach (var path in darstellung.TemperaturElemente) VisualErgebnisse.Children.Remove(path);
            elementTemperaturAn = false;
        }
    }

    private void BtnKnotenTemperaturen_Click(object sender, RoutedEventArgs e)
    {
        KnotentemperaturenZeichnen();
    }

    private void BtnKnotenGradienten_Click(object sender, RoutedEventArgs e)
    {
        if (!knotenGradientenAn)
        {
            if (index == 0)
            {
                _ = MessageBox.Show("Zeitschritt muss erst ausgewählt werden", "instationäre Wärmeberechnung");
            }
            else
            {
                darstellung.KnotengradientenZeichnen(index);
                knotenGradientenAn = true;
            }
        }
        else
        {
            // entferne ALLE Textdarstellungen der Knotentemperaturen
            foreach (var knotenGrad in darstellung.Knotengradienten) VisualErgebnisse.Children.Remove(knotenGrad);
            knotenGradientenAn = false;
        }
    }

    private void BtnElementTemperaturen_Click(object sender, RoutedEventArgs e)
    {
        ElementTemperaturenZeichnen();
    }

    private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        hitList.Clear();
        var hitPoint = e.GetPosition(VisualErgebnisse);
        hitArea = new EllipseGeometry(hitPoint, 0.2, 0.2);
        VisualTreeHelper.HitTest(VisualErgebnisse, null, HitTestCallBack,
            new GeometryHitTestParameters(hitArea));

        MyPopup.IsOpen = false;

        var sb = new StringBuilder();
        foreach (var item in hitList.Where(item => !((item == null) | (item?.Name == string.Empty))))
        {
            sb.Clear();
            MyPopup.IsOpen = true;

            if (!modell.Elemente.TryGetValue(item.Name, out var element2D)) continue;
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
                        hitList.Add(hit);
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