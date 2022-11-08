using FE_Berechnungen.Tragwerksberechnung.Modelldaten;
using FE_Berechnungen.Tragwerksberechnung.ModelldatenLesen;
using FEBibliothek.Modell;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Shapes;

namespace FE_Berechnungen.Tragwerksberechnung.ModelldatenAnzeigen;

public partial class TragwerkmodellVisualisieren
{
    private readonly FeModell modell;
    public readonly Darstellung darstellung;
    private bool lastenAn = true, lagerAn = true, knotenTexteAn = true, elementTexteAn = true;
    //alle gefundenen "Shapes" werden in dieser Liste gesammelt
    private readonly List<Shape> hitList = new();
    private readonly List<TextBlock> hitTextBlock = new();
    private EllipseGeometry hitArea;
    private KnotenNeu neuerKnoten;
    private Point mittelpunkt;
    private bool isDragging;
    public bool isKnoten;
    private bool löschFlag;

    public TragwerkmodellVisualisieren(FeModell feModell)
    {
        Language = XmlLanguage.GetLanguage("de-DE");
        InitializeComponent();
        VisualModel.Children.Remove(Knoten);
        Show();
        VisualModel.Background = Brushes.Transparent;
        modell = feModell;
        darstellung = new Darstellung(feModell, VisualModel);
        darstellung.UnverformteGeometrie();
        // mit Knoten und Element Ids
        darstellung.KnotenTexte();
        darstellung.ElementTexte();
        darstellung.LastenZeichnen();
        darstellung.LastTexte();
        darstellung.LagerZeichnen();
        darstellung.LagerTexte();
    }

    private void OnBtnKnotenIDs_Click(object sender, RoutedEventArgs e)
    {
        if (!knotenTexteAn)
        {
            darstellung.KnotenTexte();
            knotenTexteAn = true;
        }
        else
        {
            foreach (var id in darstellung.KnotenIDs.Cast<TextBlock>()) VisualModel.Children.Remove(id);
            knotenTexteAn = false;
        }
    }
    private void OnBtnElementIDs_Click(object sender, RoutedEventArgs e)
    {
        if (!elementTexteAn)
        {
            darstellung.ElementTexte();
            elementTexteAn = true;
        }
        else
        {
            foreach (var id in darstellung.ElementIDs.Cast<TextBlock>()) VisualModel.Children.Remove(id);
            elementTexteAn = false;
        }
    }
    private void OnBtnLasten_Click(object sender, RoutedEventArgs e)
    {
        if (!lastenAn)
        {
            darstellung.LastenZeichnen();
            darstellung.LastTexte();
            lastenAn = true;
        }
        else
        {
            foreach (var lasten in darstellung.LastVektoren.Cast<Shape>())
            {
                VisualModel.Children.Remove(lasten);
                foreach (var id in darstellung.LastIDs.Cast<TextBlock>()) VisualModel.Children.Remove(id);
            }
            lastenAn = false;
        }
    }
    private void OnBtnLager_Click(object sender, RoutedEventArgs e)
    {
        if (!lagerAn)
        {
            darstellung.LagerZeichnen();
            darstellung.LagerTexte();
            lagerAn = true;
        }
        else
        {
            foreach (var path in darstellung.LagerDarstellung.Cast<Shape>())
            {
                VisualModel.Children.Remove(path);
                foreach (var id in darstellung.LagerIDs.Cast<TextBlock>()) VisualModel.Children.Remove(id);
            }
            lagerAn = false;
        }
    }

    private void OnBtnKnotenNeu_Click(object sender, RoutedEventArgs e)
    {
        neuerKnoten = new KnotenNeu(modell);
        StartFenster.berechnet = false;
    }
    
    private void MenuBalkenElementNeu(object sender, RoutedEventArgs e)
    {
        _ = new BalkenElementNeu(modell);
        StartFenster.berechnet = false;
        Close();
    }
    private void MenuQuerschnittNeu(object sender, RoutedEventArgs e)
    {
        _ = new QuerschnittNeu(modell);
    }
    private void MenuFederelementNeu(object sender, RoutedEventArgs e)
    {
        _ = new FederelementNeu(modell);
        StartFenster.berechnet = false;
        Close();
    }
    private void MenuMaterialNeu(object sender, RoutedEventArgs e)
    {
        _ = new MaterialNeu(modell);
    }

    private void MenuKnotenlastNeu(object sender, RoutedEventArgs e)
    {
        _ = new KnotenlastNeu(modell);
        StartFenster.berechnet = false;
    }
    private void MenuLinienlastNeu(object sender, RoutedEventArgs e)
    {
        _ = new LinienlastNeu(modell);
        StartFenster.berechnet = false;
    }
    private void MenuPunktlastNeu(object sender, RoutedEventArgs e)
    {
        _ = new PunktlastNeu(modell);
        StartFenster.berechnet = false;
    }

    private void OnBtnFesthaltungenNeu_Click(object sender, RoutedEventArgs e)
    {
        _ = new LagerNeu(modell);
        StartFenster.berechnet = false;
    }

    private void OnBtnLöschen_Click(object sender, RoutedEventArgs e)
    {
        löschFlag = true;
        _ = new DialogLöschStrukturobjekte(löschFlag);
    }

    private void Knoten_MouseDown(object sender, MouseButtonEventArgs e)
    {
        Knoten.CaptureMouse();
        isDragging = true;
    }
    private void Knoten_MouseMove(object sender, MouseEventArgs e)
    {
        if (!isDragging) return;
        var canvPosToWindow = VisualModel.TransformToAncestor(this).Transform(new Point(0, 0));

        if (sender is not Ellipse knoten) return;
        var upperlimit = canvPosToWindow.Y + knoten.Height / 2;
        var lowerlimit = canvPosToWindow.Y + VisualModel.ActualHeight - knoten.Height / 2;

        var leftlimit = canvPosToWindow.X + knoten.Width / 2;
        var rightlimit = canvPosToWindow.X + VisualModel.ActualWidth - knoten.Width / 2;


        var absmouseXpos = e.GetPosition(this).X;
        var absmouseYpos = e.GetPosition(this).Y;

        if (!(absmouseXpos > leftlimit) || !(absmouseXpos < rightlimit)
                                        || !(absmouseYpos > upperlimit) || !(absmouseYpos < lowerlimit)) return;

        mittelpunkt = new Point(e.GetPosition(VisualModel).X, e.GetPosition(VisualModel).Y);

        Canvas.SetLeft(knoten, mittelpunkt.X - Knoten.Width / 2);
        Canvas.SetTop(knoten, mittelpunkt.Y - Knoten.Height / 2);

        var koordinaten = darstellung.TransformBildPunkt(mittelpunkt);
        neuerKnoten.X.Text = koordinaten[0].ToString("N2", CultureInfo.CurrentCulture);
        neuerKnoten.Y.Text = koordinaten[1].ToString("N2", CultureInfo.CurrentCulture);
    }
    private void Knoten_MouseUp(object sender, MouseButtonEventArgs e)
    {
        Knoten.ReleaseMouseCapture();
        isDragging = false;
    }
    private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        hitList.Clear();
        hitTextBlock.Clear();
        var hitPoint = e.GetPosition(VisualModel);
        hitArea = new EllipseGeometry(hitPoint, 1.0, 1.0);
        VisualTreeHelper.HitTest(VisualModel, null, HitTestCallBack,
            new GeometryHitTestParameters(hitArea));

        // click auf Canvas weder Text noch Shape --> neuer Knoten wird mit Zeiger plaziert und bewegt
        if (hitList.Count == 0 && hitTextBlock.Count == 0)
        {
            if (löschFlag | neuerKnoten == null) return;
            mittelpunkt = new Point(e.GetPosition(VisualModel).X, e.GetPosition(VisualModel).Y);
            Canvas.SetLeft(Knoten, mittelpunkt.X - Knoten.Width / 2);
            Canvas.SetTop(Knoten, mittelpunkt.Y - Knoten.Height / 2);
            VisualModel.Children.Add(Knoten);
            isKnoten = true;
            var koordinaten = darstellung.TransformBildPunkt(mittelpunkt);
            neuerKnoten.X.Text = koordinaten[0].ToString("N2", CultureInfo.CurrentCulture);
            neuerKnoten.Y.Text = koordinaten[1].ToString("N2", CultureInfo.CurrentCulture);
            MyPopup.IsOpen = false;
            return;
        }

        var sb = new StringBuilder();
        // click auf Shape Darstellungen
        foreach (var item in hitList)
        {
            if (isKnoten) return;
            switch (item)
            {
                case { }:
                    {
                        // Elemente
                        if (modell.Elemente.TryGetValue(item.Name, out var element))
                        {
                            if (löschFlag)
                            {
                                if (MessageBox.Show("Element " + element.ElementId + " wird gelöscht.", "Tragwerksmodell", 
                                        MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No) { }
                                else
                                {
                                    modell.Elemente.Remove(element.ElementId);
                                    StartFenster.tragwerksModell.Close();
                                }
                                return;
                            }

                            MyPopup.IsOpen = true;
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

                        // Lasten
                        if (modell.Lasten.TryGetValue(item.Name, out var knotenlast))
                        {
                            if (löschFlag)
                            {
                                if (MessageBox.Show("Knotenlast " + knotenlast.LastId + " wird gelöscht.", "Tragwerksmodell",
                                        MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No) { }
                                else
                                {
                                    modell.Lasten.Remove(knotenlast.LastId);
                                    StartFenster.tragwerksModell.Close();
                                }
                                return;
                            }
                            MyPopup.IsOpen = true;
                            sb.Append("Last\t= " + item.Name);
                            for (var i = 0; i < knotenlast.Lastwerte.Length; i++)
                            {
                                sb.Append("\nLastwert " + i + "\t= " + knotenlast.Lastwerte[i]);
                            }
                            sb.Append("\n");
                        }
                        else if (modell.PunktLasten.TryGetValue(item.Name, out var punktlast))
                        {
                            if (löschFlag)
                            {
                                modell.PunktLasten.Remove(item.Name);
                                StartFenster.tragwerksModell.Close();
                                return;
                            }
                            MyPopup.IsOpen = true;
                            sb.Append("Punktlast\t= " + item.Name);
                            for (var i = 0; i < punktlast.Lastwerte.Length; i++)
                            {
                                sb.Append("\nLastwert " + i + "\t= " + punktlast.Lastwerte[i]);
                            }
                            sb.Append("\n");
                        }
                        else if (modell.ElementLasten.TryGetValue(item.Name, out var elementlast))
                        {
                            if (löschFlag)
                            {
                                modell.ElementLasten.Remove(item.Name);
                                StartFenster.tragwerksModell.Close();
                                return;
                            }
                            MyPopup.IsOpen = true;
                            sb.Append("Last\t= " + elementlast.LastId);
                            for (var i = 0; i < elementlast.Lastwerte.Length; i++)
                            {
                                sb.Append("\nLastwert " + i + "\t= " + elementlast.Lastwerte[i]);
                            }
                            sb.Append("\n");
                        }
                    }

                    // Lager
                    if (modell.Randbedingungen.TryGetValue(item.Name, out var lager))
                    {
                        if (löschFlag)
                        {
                            if (MessageBox.Show("Lager " + lager.RandbedingungId + " wird gelöscht.", "Tragwerksmodell",
                                    MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No) { }

                            modell.Randbedingungen.Remove(lager.RandbedingungId);
                            StartFenster.tragwerksModell.Close();
                            return;
                        }
                    
                        MyPopup.IsOpen = true;
                        sb.Append("Lager\t\t= " + lager.RandbedingungId);
                        sb.Append("\festgehalten\t= " + Lagertyp(lager.Typ));
                        for (var i = 0; i < lager.Vordefiniert.Length; i++)
                        {
                            sb.Append("\nvordefinierter Randwert " + i + "\t= " + lager.Vordefiniert[i]);
                        }
                        sb.Append("\n");
                    }
                    break;
            }
        }

        // click auf Knotentext --> Eigenschaften eines vorhandenen Knotens werden interaktiv verändert
        foreach (var item in hitTextBlock)
        {
            if (!modell.Knoten.TryGetValue(item.Text, out var knoten)) continue;
            if (löschFlag)
            {
                if (MessageBox.Show("Knoten " + knoten.Id + " wird gelöscht.", "Tragwerksmodell",
                        MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No) { }
                else
                {
                    modell.Knoten.Remove(knoten.Id);
                    StartFenster.tragwerksModell.Close();
                }
                return;
            }
            if (item.Text != knoten.Id) _ = MessageBox.Show("Knoten Id kann hier nicht verändert werden", "Knotentext");
            neuerKnoten = new KnotenNeu(modell)
            {
                KnotenId = { Text = item.Text },
                AnzahlDof = { Text = knoten.AnzahlKnotenfreiheitsgrade.ToString("N0", CultureInfo.CurrentCulture) },
                X = { Text = knoten.Koordinaten[0].ToString("N2", CultureInfo.CurrentCulture) },
                Y = { Text = knoten.Koordinaten[1].ToString("N2", CultureInfo.CurrentCulture) }
            };

            mittelpunkt = new Point(knoten.Koordinaten[0] * darstellung.auflösung + darstellung.plazierungH,
                (-knoten.Koordinaten[1] + darstellung.maxY) * darstellung.auflösung + darstellung.plazierungV);
            Canvas.SetLeft(Knoten, mittelpunkt.X - Knoten.Width / 2);
            Canvas.SetTop(Knoten, mittelpunkt.Y - Knoten.Height / 2);
            VisualModel.Children.Add(Knoten);
            isKnoten = true;
            MyPopup.IsOpen = false;
        }
        MyPopupText.Text = sb.ToString();

        // click auf Textdarstellungen - ausser Knotentexte (werden oben gesondert behandelt)
        if (isKnoten) return;
        foreach (var item in hitTextBlock)
        {
            // Textdarstellung ist ein Element
            if (modell.Elemente.TryGetValue(item.Text, out var element))
            {
                if (löschFlag)
                {
                    if (MessageBox.Show("Element " + element.ElementId + " wird gelöscht.", "Tragwerksmodell",
                            MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No) { }
                    else
                    {
                        modell.Elemente.Remove(element.ElementId);
                        StartFenster.tragwerksModell.Close();
                    }
                    return;
                }

                switch (element)
                {
                    case FederElement:
                    {
                        _ = new FederelementNeu(modell)
                        {
                            ElementId = { Text = item.Text },
                            KnotenId = { Text = element.KnotenIds[0] },
                            MaterialId = { Text = element.ElementMaterialId }
                        };
                        break;
                    }
                    case Fachwerk:
                    {
                        _ = new BalkenElementNeu(modell)
                        {
                            ElementId = { Text = item.Text },
                            StartknotenId = { Text = element.KnotenIds[0] },
                            EndknotenId = { Text = element.KnotenIds[1] },
                            MaterialId = { Text = element.ElementMaterialId },
                            QuerschnittId = { Text = element.ElementQuerschnittId }
                        };
                        break;
                    }
                    case Biegebalken:
                    {
                        _ = new BalkenElementNeu(modell)
                        {
                            ElementId = { Text = item.Text },
                            StartknotenId = { Text = element.KnotenIds[0] },
                            EndknotenId = { Text = element.KnotenIds[1] },
                            MaterialId = { Text = element.ElementMaterialId },
                            QuerschnittId = { Text = element.ElementQuerschnittId },
                            Gelenk1 = { IsChecked = false },
                            Gelenk2 = { IsChecked = false }
                        };
                        break;
                    }
                    case BiegebalkenGelenk:
                    {
                        var neuesElement = new BalkenElementNeu(modell)
                        {
                            ElementId = { Text = item.Text },
                            StartknotenId = { Text = element.KnotenIds[0] },
                            EndknotenId = { Text = element.KnotenIds[1] },
                            MaterialId = { Text = element.ElementMaterialId },
                            QuerschnittId = { Text = element.ElementQuerschnittId }
                        };
                        switch (element.Typ)
                        {
                            case 1: { neuesElement.Gelenk1.IsChecked = true; break; }
                            case 2: { neuesElement.Gelenk2.IsChecked = true; break; }
                        }
                        break;
                    }
                }
            }
            // Textdarstellung ist eine Knotenlast
            else if (modell.Lasten.TryGetValue(item.Text, out var knotenlast))
            {
                if (löschFlag)
                {
                    if (MessageBox.Show("Knotenlast " + knotenlast.LastId + " wird gelöscht.", "Tragwerksmodell",
                            MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No) { }
                    else
                    {
                        modell.Elemente.Remove(knotenlast.LastId);
                        StartFenster.tragwerksModell.Close();
                    }
                    return;
                }

                _ = new KnotenlastNeu(modell)
                {
                    LastId = { Text = item.Text},
                    KnotenId = { Text = knotenlast.KnotenId.ToString(CultureInfo.CurrentCulture) },
                    Px = { Text = knotenlast.Lastwerte[0].ToString(CultureInfo.CurrentCulture) },
                    Py = { Text = knotenlast.Lastwerte[1].ToString(CultureInfo.CurrentCulture) },
                    M  = { Text = knotenlast.Lastwerte[2].ToString(CultureInfo.CurrentCulture) },
                };
            }
            // Textdarstellung ist eine Elementlast (Linienlast)
            else if (modell.ElementLasten.TryGetValue(item.Text, out var linienlast))
            {
                if (löschFlag)
                {
                    if (MessageBox.Show("Linienlast " + linienlast.LastId + " wird gelöscht.", "Tragwerksmodell",
                            MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No) { }
                    else
                    {
                        modell.LinienLasten.Remove(linienlast.LastId);
                        StartFenster.tragwerksModell.Close();
                    }
                }
                _ = new LinienlastNeu(modell)
                {
                    LastId = { Text = item.Text },
                    ElementId = { Text = linienlast.ElementId.ToString(CultureInfo.CurrentCulture) },
                    Pxa = { Text = linienlast.Lastwerte[0].ToString(CultureInfo.CurrentCulture) },
                    Pya = { Text = linienlast.Lastwerte[1].ToString(CultureInfo.CurrentCulture) },
                    Pxb = { Text = linienlast.Lastwerte[2].ToString(CultureInfo.CurrentCulture) },
                    Pyb = { Text = linienlast.Lastwerte[3].ToString(CultureInfo.CurrentCulture) },
                    InElement = { IsChecked = linienlast.InElementKoordinatenSystem }
                };
            }
            // Textdarstellung ist eine Punktlast
            else if (modell.PunktLasten.TryGetValue(item.Text, out var punktlast))
            {
                if (löschFlag)
                {
                    if (MessageBox.Show("Punktlast " + punktlast.LastId + " wird gelöscht.", "Tragwerksmodell",
                            MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No){}
                    else
                    {
                        modell.PunktLasten.Remove(punktlast.LastId);
                        StartFenster.tragwerksModell.Close();
                    }
                }

                var punktLast = (PunktLast)punktlast;
                _ = new PunktlastNeu(modell)
                {
                    LastId = { Text = item.Text },
                    ElementId = { Text = punktlast.ElementId.ToString(CultureInfo.CurrentCulture) },
                    Px = { Text = punktLast.Lastwerte[0].ToString(CultureInfo.CurrentCulture) },
                    Py = { Text = punktLast.Lastwerte[1].ToString(CultureInfo.CurrentCulture) },
                    Offset = { Text = punktLast.Offset.ToString(CultureInfo.CurrentCulture) },
                };
            }
            // Textdarstellung ist ein Lager
            else if (modell.Randbedingungen.TryGetValue(item.Text, out var lager))
            {
                if (löschFlag)
                {
                    if (MessageBox.Show("Lager " + lager.RandbedingungId + " wird gelöscht.", "Tragwerksmodell",
                            MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No) { }
                    else
                    {
                        modell.Randbedingungen.Remove(lager.RandbedingungId);
                        StartFenster.tragwerksModell.Close();
                    }
                    return;
                }

                _ = new LagerNeu(modell)
                {
                    LagerId = { Text = item.Text },
                    KnotenId = { Text = lager.KnotenId.ToString(CultureInfo.CurrentCulture) },
                    VorX = { Text = lager.Vordefiniert[0].ToString("0.00") },
                    VorY = { Text = lager.Vordefiniert[1].ToString("0.00") },
                    VorRot = { Text = lager.Vordefiniert[2].ToString("0.00") },
                    Xfest = { IsChecked = lager.Typ == 1 | lager.Typ == 3 | lager.Typ == 7 },
                    Yfest = { IsChecked = lager.Typ == 2 | lager.Typ == 3 | lager.Typ == 7 },
                    Rfest = { IsChecked = lager.Typ == 4 | lager.Typ == 7 }
                };
            }
        }
    }

    private void OnMouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
        MyPopup.IsOpen = false;
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
    
    private string Lagertyp(int typ)
    {
        var lagertyp = typ switch
        {
            1 => "x",
            2 => "y",
            3 => "xy",
            7 => "xyr",
            _ => ""
        };
        return lagertyp;
    }
}