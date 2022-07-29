using FE_Berechnungen.Tragwerksberechnung.Modelldaten;
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

namespace FE_Berechnungen.Tragwerksberechnung.ModelldatenAnzeigen
{
    public partial class TragwerkmodellVisualisieren
    {
        private readonly FEModell modell;
        public readonly Darstellung darstellung;
        private bool lastenAn = true, lagerAn = true, knotenTexteAn = true, elementTexteAn = true;
        //alle gefundenen "Shapes" werden in dieser Liste gesammelt
        private readonly List<Shape> hitList = new List<Shape>();
        private readonly List<TextBlock> hitTextBlock = new List<TextBlock>();
        private EllipseGeometry hitArea;

        public TragwerkmodellVisualisieren(FEModell feModell)
        {
            Language = XmlLanguage.GetLanguage("de-DE");
            InitializeComponent();
            Show();

            modell = feModell;
            darstellung = new Darstellung(feModell, VisualModel);
            darstellung.UnverformteGeometrie();

            // mit Knoten und Element Ids
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
                foreach (TextBlock id in darstellung.KnotenIDs) VisualModel.Children.Remove(id);
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
                foreach (TextBlock id in darstellung.ElementIDs) VisualModel.Children.Remove(id);
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
                foreach (Shape lasten in darstellung.LastVektoren)
                {
                    VisualModel.Children.Remove(lasten);
                }
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
                foreach (Shape path in darstellung.LagerDarstellung)
                {
                    VisualModel.Children.Remove(path);
                }
                lagerAn = false;
            }
        }

        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            hitList.Clear();
            hitTextBlock.Clear();
            var hitPoint = e.GetPosition(VisualModel);
            hitArea = new EllipseGeometry(hitPoint, 1.0, 1.0);
            VisualTreeHelper.HitTest(VisualModel, null, HitTestCallBack,
                new GeometryHitTestParameters(hitArea));

            MyPopup.IsOpen = false;

            var sb = new StringBuilder();
            foreach (var item in hitList.Where(item => item != null))
            {
                MyPopup.IsOpen = true;

                switch (item)
                {
                    case Shape path:
                        {
                            if (path.Name == null) continue;
                            if (modell.Elemente.TryGetValue(path.Name, out var element))
                            {

                                sb.Append("Element\t= " + element.ElementId);
                                if (element is FederElement)
                                {
                                    if (modell.Elemente.TryGetValue(element.ElementId, out var feder))
                                    {
                                        if (modell.Material.TryGetValue(feder.ElementMaterialId, out var material)) { }
                                        for (var i = 0; i < 3; i++)
                                        {
                                            if (material != null)
                                                sb.Append("\nFedersteifigkeit " + i + "\t= " +
                                                          material.MaterialWerte[i].ToString("g3"));
                                        }
                                    }
                                }
                                else
                                {
                                    sb.Append("\nKnoten 1\t= " + element.KnotenIds[0]);
                                    sb.Append("\nKnoten 2\t= " + element.KnotenIds[1]);
                                    if (modell.Material.TryGetValue(element.ElementMaterialId, out var material))
                                    {
                                        sb.Append("\nE-Modul\t= " + material.MaterialWerte[0].ToString("g3"));
                                    }
                                    if (modell.Querschnitt.TryGetValue(element.ElementQuerschnittId, out var querschnitt))
                                    {
                                        sb.Append("\nFläche\t= " + querschnitt.QuerschnittsWerte[0]);
                                        if (querschnitt.QuerschnittsWerte.Length > 1)
                                            sb.Append("\nIxx\t= " + querschnitt.QuerschnittsWerte[1].ToString("g3"));
                                    }
                                }
                            }

                            if (modell.Lasten.TryGetValue(path.Name, out var knotenlast))
                            {
                                sb.Append("Last\t= " + path.Name);
                                for (var i = 0; i < knotenlast.Lastwerte.Length; i++)
                                {
                                    sb.Append("\nLastwert " + i + "\t= " + knotenlast.Lastwerte[i]);
                                }
                            }

                            else if (modell.PunktLasten.TryGetValue(path.Name, out var punktlast))
                            {
                                sb.Append("Punktlast\t= " + path.Name);
                                for (var i = 0; i < punktlast.Lastwerte.Length; i++)
                                {
                                    sb.Append("\nLastwert " + i + "\t= " + punktlast.Lastwerte[i]);
                                }
                            }

                            else if (modell.ElementLasten.TryGetValue(path.Name, out var elementlast))
                            {
                                sb.Append("Last\t= " + elementlast.LastId);
                                for (var i = 0; i < elementlast.Lastwerte.Length; i++)
                                {
                                    sb.Append("\nLastwert " + i + "\t= " + elementlast.Lastwerte[i]);
                                }
                            }
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
                    sb.Append("\nx-Koordinate\t\t= " + knoten.Koordinaten[0].ToString("g3"));
                    sb.Append("\ny-Koordinate\t\t= " + knoten.Koordinaten[1].ToString("g3"));
                }

                if (modell.Elemente.TryGetValue(item.Text, out var element))
                {
                    if (element is FederElement)
                    {
                        if (modell.Material.TryGetValue(element.ElementMaterialId, out var material))
                        {
                            sb.Append("Federeigenschaften\t= " + material.MaterialId);
                            sb.Append("\nFedersteifigkeit x\t\t= " + material.MaterialWerte[0].ToString("g3"));
                            sb.Append("\nFedersteifigkeit y\t\t= " + material.MaterialWerte[1].ToString("g3"));
                            sb.Append("\nDrehfedersteifigkeit\t= " + material.MaterialWerte[2].ToString("g3"));
                        }
                    }
                    else
                    {
                        sb.Append("Element\t= " + element.ElementId);
                        sb.Append("\nKnoten 1\t= " + element.KnotenIds[0]);
                        sb.Append("\nKnoten 2\t= " + element.KnotenIds[1]);
                        if (modell.Material.TryGetValue(element.ElementMaterialId, out var material))
                        {
                            sb.Append("\nE-Modul\t= " + material.MaterialWerte[0].ToString("g3"));
                        }
                        if (modell.Querschnitt.TryGetValue(element.ElementQuerschnittId, out var querschnitt))
                        {
                            sb.Append("\nFläche\t= " + querschnitt.QuerschnittsWerte[0]);
                            if (querschnitt.QuerschnittsWerte.Length > 1)
                                sb.Append("\nIxx\t= " + querschnitt.QuerschnittsWerte[1].ToString("g3"));
                        }
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
}