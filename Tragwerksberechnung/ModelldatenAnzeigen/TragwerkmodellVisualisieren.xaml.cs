using FE_Berechnungen.Tragwerksberechnung.Modelldaten;
using FE_Berechnungen.Tragwerksberechnung.ModelldatenLesen;
using FEBibliothek.Modell;
using FEBibliothek.Modell.abstrakte_Klassen;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Shapes;

namespace FE_Berechnungen.Tragwerksberechnung.ModelldatenAnzeigen;

public partial class TragwerkmodellVisualisieren
{
    private readonly FeModell _modell;
    public readonly Darstellung Darstellung;
    private bool _lastenAn = true, _lagerAn = true, _knotenTexteAn = true, _elementTexteAn = true;

    //alle gefundenen "Shapes" werden in dieser Liste gesammelt
    private readonly List<Shape> _hitList = [];
    //alle gefundenen "TextBlocks" werden in dieser Liste gesammelt
    private readonly List<TextBlock> _hitTextBlock = [];
    private EllipseGeometry _hitArea;

    private KnotenNeu _neuerKnoten;
    private Point _mittelpunkt;
    private bool _isDragging;
    private bool _isKnoten, _isElement, _isKnotenlast, _isLinienlast, _isPunktlast, _isLager;
    private KnotenNeu _knotenNeu;
    private ElementNeu _elementNeu;
    private KnotenlastNeu _knotenlastNeu;
    private LinienlastNeu _linienlastNeu;
    private PunktlastNeu _punktlastNeu;
    private LagerNeu _lagerNeu;
    public ZeitintegrationNeu ZeitintegrationNeu;

    public TragwerkmodellVisualisieren(FeModell feModell)
    {
        Language = XmlLanguage.GetLanguage("de-DE");
        InitializeComponent();
        VisualTragwerkModel.Children.Remove(Knoten);
        Show();
        VisualTragwerkModel.Background = Brushes.Transparent;
        _modell = feModell;
        Darstellung = new Darstellung(feModell, VisualTragwerkModel);
        Darstellung.UnverformteGeometrie();

        // mit Knoten und Element Ids
        Darstellung.KnotenTexte();
        Darstellung.ElementTexte();
        // mit Lasten und Auflagerdarstellungen und Ids
        Darstellung.LastenZeichnen();
        Darstellung.LastTexte();
        Darstellung.LagerZeichnen();
        Darstellung.LagerTexte();
    }

    private void OnBtnKnotenIDs_Click(object sender, RoutedEventArgs e)
    {
        if (!_knotenTexteAn)
        {
            Darstellung.KnotenTexte();
            _knotenTexteAn = true;
        }
        else
        {
            foreach (var id in Darstellung.KnotenIDs.Cast<TextBlock>()) VisualTragwerkModel.Children.Remove(id);
            _knotenTexteAn = false;
        }
    }
    private void OnBtnElementIDs_Click(object sender, RoutedEventArgs e)
    {
        if (!_elementTexteAn)
        {
            Darstellung.ElementTexte();
            _elementTexteAn = true;
        }
        else
        {
            foreach (var id in Darstellung.ElementIDs.Cast<TextBlock>()) VisualTragwerkModel.Children.Remove(id);
            _elementTexteAn = false;
        }
    }
    private void OnBtnLasten_Click(object sender, RoutedEventArgs e)
    {
        if (!_lastenAn)
        {
            Darstellung.LastenZeichnen();
            Darstellung.LastTexte();
            _lastenAn = true;
        }
        else
        {
            foreach (var lasten in Darstellung.LastVektoren.Cast<Shape>())
            {
                VisualTragwerkModel.Children.Remove(lasten);
                foreach (var id in Darstellung.LastIDs.Cast<TextBlock>()) VisualTragwerkModel.Children.Remove(id);
            }
            _lastenAn = false;
        }
    }
    private void OnBtnLager_Click(object sender, RoutedEventArgs e)
    {
        if (!_lagerAn)
        {
            Darstellung.LagerZeichnen();
            Darstellung.LagerTexte();
            _lagerAn = true;
        }
        else
        {
            foreach (var path in Darstellung.LagerDarstellung.Cast<Shape>())
            {
                VisualTragwerkModel.Children.Remove(path);
                foreach (var id in Darstellung.LagerIDs.Cast<TextBlock>()) VisualTragwerkModel.Children.Remove(id);
            }
            _lagerAn = false;
        }
    }

    private void MenuBalkenKnotenNeu(object sender, RoutedEventArgs e)
    {
        _neuerKnoten = new KnotenNeu(_modell) { Topmost = true, Owner = (Window)Parent };
        StartFenster.Berechnet = false;
    }
    private void MenuBalkenKnotenGruppeNeu(object sender, RoutedEventArgs e)
    {
        _ = new KnotenGruppeNeu(_modell) { Topmost = true, Owner = (Window)Parent };
    }
    private void MenuBalkenKnotenNetzÄquidistant(object sender, RoutedEventArgs e)
    {
        _ = new KnotenNetzÄquidistant(_modell) { Topmost = true, Owner = (Window)Parent };
    }

    private void MenuBalkenKnotenNetzVariabel(object sender, RoutedEventArgs e)
    {
        _ = new KnotenNetzVariabel(_modell) { Topmost = true, Owner = (Window)Parent };
    }

    private void MenuBalkenElementNeu(object sender, RoutedEventArgs e)
    {
        _isElement = true;
        _elementNeu = new ElementNeu(_modell) { Topmost = true, Owner = (Window)Parent };
        StartFenster.Berechnet = false;
    }
    private void MenuQuerschnittNeu(object sender, RoutedEventArgs e)
    {
        _ = new QuerschnittNeu(_modell) { Topmost = true, Owner = (Window)Parent };
    }
    private void MenuMaterialNeu(object sender, RoutedEventArgs e)
    {
        _ = new MaterialNeu(_modell) { Topmost = true, Owner = (Window)Parent };
    }

    private void MenuKnotenlastNeu(object sender, RoutedEventArgs e)
    {
        _isKnotenlast = true;
        _knotenlastNeu = new KnotenlastNeu(_modell) { Topmost = true, Owner = (Window)Parent };
        StartFenster.Berechnet = false;
    }
    private void MenuLinienlastNeu(object sender, RoutedEventArgs e)
    {
        _isLinienlast = true;
        _linienlastNeu = new LinienlastNeu(_modell) { Topmost = true, Owner = (Window)Parent };
        StartFenster.Berechnet = false;
    }
    private void MenuPunktlastNeu(object sender, RoutedEventArgs e)
    {
        _isPunktlast = true;
        _punktlastNeu = new PunktlastNeu(_modell) { Topmost = true, Owner = (Window)Parent };
        StartFenster.Berechnet = false;
    }

    private void OnBtnLagerNeu_Click(object sender, RoutedEventArgs e)
    {
        _isLager = true;
        _lagerNeu = new LagerNeu(_modell) { Topmost = true, Owner = (Window)Parent };
        StartFenster.Berechnet = false;
    }

    private void OnBtnZeitintegrationNew_Click(object sender, RoutedEventArgs e)
    {
        ZeitintegrationNeu = new ZeitintegrationNeu(_modell) { Topmost = true };
    }

    private void Knoten_MouseDown(object sender, MouseButtonEventArgs e)
    {
        Knoten.CaptureMouse();
        _isDragging = true;
    }
    private void Knoten_MouseMove(object sender, MouseEventArgs e)
    {
        if (!_isDragging) return;
        var canvPosToWindow = VisualTragwerkModel.TransformToAncestor(this).Transform(new Point(0, 0));

        if (sender is not Ellipse knoten) return;
        var upperlimit = canvPosToWindow.Y + knoten.Height / 2;
        var lowerlimit = canvPosToWindow.Y + VisualTragwerkModel.ActualHeight - knoten.Height / 2;

        var leftlimit = canvPosToWindow.X + knoten.Width / 2;
        var rightlimit = canvPosToWindow.X + VisualTragwerkModel.ActualWidth - knoten.Width / 2;


        var absmouseXpos = e.GetPosition(this).X;
        var absmouseYpos = e.GetPosition(this).Y;

        if (!(absmouseXpos > leftlimit) || !(absmouseXpos < rightlimit)
                                        || !(absmouseYpos > upperlimit) || !(absmouseYpos < lowerlimit)) return;

        _mittelpunkt = new Point(e.GetPosition(VisualTragwerkModel).X, e.GetPosition(VisualTragwerkModel).Y);

        Canvas.SetLeft(knoten, _mittelpunkt.X - Knoten.Width / 2);
        Canvas.SetTop(knoten, _mittelpunkt.Y - Knoten.Height / 2);

        var koordinaten = Darstellung.TransformBildPunkt(_mittelpunkt);
        _neuerKnoten.X.Text = koordinaten[0].ToString("N2", CultureInfo.CurrentCulture);
        _neuerKnoten.Y.Text = koordinaten[1].ToString("N2", CultureInfo.CurrentCulture);
    }
    private void Knoten_MouseUp(object sender, MouseButtonEventArgs e)
    {
        Knoten.ReleaseMouseCapture();
        _isDragging = false;
        _isKnoten = false;
    }

    private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        _hitList.Clear();
        _hitTextBlock.Clear();
        var hitPoint = e.GetPosition(VisualTragwerkModel);
        _hitArea = new EllipseGeometry(hitPoint, 1.0, 1.0);
        VisualTreeHelper.HitTest(VisualTragwerkModel, null, HitTestCallBack,
            new GeometryHitTestParameters(_hitArea));

        // click auf Canvas weder Text noch Shape ⇾ neuer Knoten wird mit Zeiger platziert und bewegt
        if (_hitList.Count == 0 && _hitTextBlock.Count == 0)
        {
            if (_neuerKnoten == null) return;
            _mittelpunkt = new Point(e.GetPosition(VisualTragwerkModel).X, e.GetPosition(VisualTragwerkModel).Y);
            Canvas.SetLeft(Knoten, _mittelpunkt.X - Knoten.Width / 2);
            Canvas.SetTop(Knoten, _mittelpunkt.Y - Knoten.Height / 2);
            VisualTragwerkModel.Children.Add(Knoten);
            var koordinaten = Darstellung.TransformBildPunkt(_mittelpunkt);
            _neuerKnoten.X.Text = koordinaten[0].ToString("N2", CultureInfo.CurrentCulture);
            _neuerKnoten.Y.Text = koordinaten[1].ToString("N2", CultureInfo.CurrentCulture);
            return;
        }

        // click auf Shape Darstellungen
        foreach (var item in _hitList.TakeWhile(item => !_isKnoten && !_isElement && !_isKnotenlast).
                     Where(item => item.Name != null))
        {
            // Elemente
            if (_modell.Elemente.TryGetValue(item.Name, out var element))
                ElementNeu(element);

            // Lasten
            if (_modell.Lasten.TryGetValue(item.Name, out var knotenlast))
                KnotenlastNeu(knotenlast);
            else if (_modell.PunktLasten.TryGetValue(item.Name, out var punktlast))
                PunktlastNeu(punktlast);
            else if (_modell.ElementLasten.TryGetValue(item.Name, out var elementlast))
                LinienlastNeu(elementlast);

            // Lager
            else if (_modell.Randbedingungen.TryGetValue(item.Name, out var lager))
                LagerNeu(lager);
        }

        // click auf Textdarstellungen
        foreach (var item in _hitTextBlock)
        {
            // Textdarstellung ist ein Knoten
            if (_modell.Knoten.TryGetValue(item.Text, out var knoten))
            {
                _isKnoten = true;
                KnotenNeu(knoten);
            }

            // Textdarstellung ist ein Element
            if (_modell.Elemente.TryGetValue(item.Text, out var element))
                ElementNeu(element);

            // Textdarstellung ist eine Knotenlast
            else if (_modell.Lasten.TryGetValue(item.Text, out var knotenlast))
                KnotenlastNeu(knotenlast);

            // Textdarstellung ist eine Elementlast (Linienlast)
            else if (_modell.ElementLasten.TryGetValue(item.Text, out var linienlast))
            {
                LinienlastNeu(linienlast);
            }

            // Textdarstellung ist eine Punktlast
            else if (_modell.PunktLasten.TryGetValue(item.Text, out var punktlast))
            {
                PunktlastNeu(punktlast);
            }
            // Textdarstellung ist ein Lager
            else if (_modell.Randbedingungen.TryGetValue(item.Text, out var lager))
            {
                LagerNeu(lager);
            }
        }
    }

    private void KnotenNeu(Knoten knoten)
    {
        // Knotentext angeklickt bei Definition eines neuen Elementes
        if (_isElement)
        {
            if (_elementNeu.StartknotenId.Text == string.Empty) _elementNeu.StartknotenId.Text = knoten.Id;
            else
            {
                _elementNeu.EndknotenId.Text = knoten.Id;
                _elementNeu.ElementId.Text = _elementNeu.StartknotenId.Text + knoten.Id;
            }
            _elementNeu.Show();
            return;
        }

        // Knotentext angeklickt bei Definition einer neuen Knotenlast
        if (_isKnotenlast)
        {
            _knotenlastNeu.KnotenId.Text = knoten.Id;
            _knotenlastNeu.LastId.Text = "kl" + knoten.Id;
            _knotenlastNeu.Show();
            return;
        }

        // Knotentext angeklickt bei Definition eines neuen Lagers
        if (_isLager)
        {
            _lagerNeu.KnotenId.Text = knoten.Id;
            _lagerNeu.LagerId.Text = "kl" + knoten.Id;
            _lagerNeu.Show();
            return;
        }

        _neuerKnoten = new KnotenNeu(_modell)
        {
            Topmost = true,
            Owner = (Window)Parent,
            KnotenId = { Text = knoten.Id },
            AnzahlDof = { Text = knoten.AnzahlKnotenfreiheitsgrade.ToString("N0", CultureInfo.CurrentCulture) },
            X = { Text = knoten.Koordinaten[0].ToString("N2", CultureInfo.CurrentCulture) },
            Y = { Text = knoten.Koordinaten[1].ToString("N2", CultureInfo.CurrentCulture) }
        };

        _mittelpunkt = new Point(knoten.Koordinaten[0] * Darstellung.Auflösung + Darstellung.PlatzierungH,
            (-knoten.Koordinaten[1] + Darstellung.MaxY) * Darstellung.Auflösung + Darstellung.PlatzierungV);
        Canvas.SetLeft(Knoten, _mittelpunkt.X - Knoten.Width / 2);
        Canvas.SetTop(Knoten, _mittelpunkt.Y - Knoten.Height / 2);
        VisualTragwerkModel.Children.Add(Knoten);
    }

    private void ElementNeu(AbstraktElement element)
    {
        // anderer Elementtext angeklickt beim Erstellen eines neuen Elementes
        // Material- und Querschnitteigenschaften werden übernommen
        if (_isElement)
        {
            _elementNeu.MaterialId.Text = element.ElementMaterialId;
            _elementNeu.QuerschnittId.Text = element.ElementQuerschnittId;
            _elementNeu.Show();
            return;
        }

        // Elementtext angeklickt bei Definition einer neuen Linienlast
        if (_isLinienlast)
        {
            _linienlastNeu.ElementId.Text = element.ElementId;
            _linienlastNeu.LastId.Text = "ll" + element.ElementId;
            _linienlastNeu.Show();
            return;
        }

        // Elementtext angeklickt bei Definition einer neuen Punktlast
        if (_isPunktlast)
        {
            _punktlastNeu.ElementId.Text = element.ElementId;
            _punktlastNeu.LastId.Text = "pl" + element.ElementId;
            _punktlastNeu.Show();
            return;
        }

        // Elementeigenschaften können editiert werden
        switch (element)
        {
            case FederElement:
                {
                    _ = new ElementNeu(_modell)
                    {
                        FederCheck = { IsChecked = true },
                        ElementId = { Text = element.ElementId },
                        StartknotenId = { Text = element.KnotenIds[0] },
                        MaterialId = { Text = element.ElementMaterialId }
                    };
                    break;
                }
            case Fachwerk:
                {
                    if (_modell.Material.TryGetValue(element.ElementMaterialId, out var material)) { }
                    var emodul = element.E > 0 ? element.E : material!.MaterialWerte[0];
                    var masse = element.M > 0 ? element.M : material!.MaterialWerte[2];

                    if (_modell.Querschnitt.TryGetValue(element.ElementQuerschnittId, out var querschnitt)) { }
                    var fläche = element.A > 0 ? element.A : querschnitt!.QuerschnittsWerte[0];

                    _ = new ElementNeu(_modell)
                    {
                        FachwerkCheck = { IsChecked = true },
                        ElementId = { Text = element.ElementId },
                        StartknotenId = { Text = element.KnotenIds[0] },
                        EndknotenId = { Text = element.KnotenIds[1] },
                        MaterialId = { Text = element.ElementMaterialId },
                        EModul = { Text = emodul.ToString("E2", CultureInfo.CurrentCulture) },
                        Masse = { Text = masse.ToString("E2", CultureInfo.CurrentCulture) },
                        QuerschnittId = { Text = element.ElementQuerschnittId },
                        Fläche = { Text = fläche.ToString("E2", CultureInfo.CurrentCulture) },
                        Gelenk1 = { IsChecked = true },
                        Gelenk2 = { IsChecked = true }
                    };
                    break;
                }
            case Biegebalken:
                {
                    if (_modell.Material.TryGetValue(element.ElementMaterialId, out var material)) { }
                    var emodul = element.E > 0 ? element.E : material!.MaterialWerte[0];
                    var masse = element.M > 0 ? element.M : material!.MaterialWerte[2];

                    if (_modell.Querschnitt.TryGetValue(element.ElementQuerschnittId, out var querschnitt)) { }
                    var fläche = element.A > 0 ? element.A : querschnitt!.QuerschnittsWerte[0];
                    var trägheitsmoment = element.I > 0 ? element.I : querschnitt!.QuerschnittsWerte[1];

                    _ = new ElementNeu(_modell)
                    {
                        BalkenCheck = { IsChecked = true },
                        ElementId = { Text = element.ElementId },
                        StartknotenId = { Text = element.KnotenIds[0] },
                        EndknotenId = { Text = element.KnotenIds[1] },
                        MaterialId = { Text = element.ElementMaterialId },
                        EModul = { Text = emodul.ToString("E2", CultureInfo.CurrentCulture) },
                        Masse = { Text = masse.ToString("E2", CultureInfo.CurrentCulture) },
                        QuerschnittId = { Text = element.ElementQuerschnittId },
                        Fläche = { Text = fläche.ToString("E2", CultureInfo.CurrentCulture) },
                        Trägheitsmoment = { Text = trägheitsmoment.ToString("E2", CultureInfo.CurrentCulture) },
                        Gelenk1 = { IsChecked = false },
                        Gelenk2 = { IsChecked = false }
                    };
                    break;
                }
            case BiegebalkenGelenk:
                {
                    if (_modell.Material.TryGetValue(element.ElementMaterialId, out var material)) { }
                    var emodul = element.E > 0 ? element.E : material!.MaterialWerte[0];
                    var masse = element.M > 0 ? element.M : material!.MaterialWerte[2];

                    if (_modell.Querschnitt.TryGetValue(element.ElementQuerschnittId, out var querschnitt)) { }
                    var fläche = element.A > 0 ? element.A : querschnitt!.QuerschnittsWerte[0];
                    var trägheitsmoment = element.I > 0 ? element.I : querschnitt!.QuerschnittsWerte[1];

                    var neuesElement = new ElementNeu(_modell)
                    {
                        BalkenCheck = { IsChecked = true },
                        ElementId = { Text = element.ElementId },
                        StartknotenId = { Text = element.KnotenIds[0] },
                        EndknotenId = { Text = element.KnotenIds[1] },
                        MaterialId = { Text = element.ElementMaterialId },
                        EModul = { Text = emodul.ToString("E2", CultureInfo.CurrentCulture) },
                        Masse = { Text = masse.ToString("E2", CultureInfo.CurrentCulture) },
                        QuerschnittId = { Text = element.ElementQuerschnittId },
                        Fläche = { Text = fläche.ToString("E2", CultureInfo.CurrentCulture) },
                        Trägheitsmoment = { Text = trägheitsmoment.ToString("E2", CultureInfo.CurrentCulture) }
                    };
                    switch (element.Typ)
                    {
                        case 1: { neuesElement.Gelenk1.IsChecked = true; break; }
                        case 2: { neuesElement.Gelenk2.IsChecked = true; break; }
                    }
                    break;
                }
        }
        _isElement = false;
    }

    private void KnotenlastNeu(AbstraktLast knotenlast)
    {
        _knotenlastNeu = new KnotenlastNeu(_modell)
        {
            LastId = { Text = knotenlast.LastId },
            KnotenId = { Text = knotenlast.KnotenId.ToString(CultureInfo.CurrentCulture) },
            Px = { Text = knotenlast.Lastwerte[0].ToString(CultureInfo.CurrentCulture) },
            Py = { Text = knotenlast.Lastwerte[1].ToString(CultureInfo.CurrentCulture) }
        };
        if (knotenlast.Lastwerte.Length > 2) _knotenlastNeu.M.Text = knotenlast.Lastwerte[2].ToString(CultureInfo.CurrentCulture);
        _isKnotenlast = false;
    }

    private void PunktlastNeu(AbstraktElementLast punktLast)
    {
        var punktlast = (PunktLast)punktLast;
        _punktlastNeu = new PunktlastNeu(_modell)
        {
            LastId = { Text = punktlast.LastId },
            ElementId = { Text = punktlast.ElementId.ToString(CultureInfo.CurrentCulture) },
            Px = { Text = punktlast.Lastwerte[0].ToString(CultureInfo.CurrentCulture) },
            Py = { Text = punktlast.Lastwerte[1].ToString(CultureInfo.CurrentCulture) },
            Offset = { Text = punktlast.Offset.ToString(CultureInfo.CurrentCulture) }
        };
        _isPunktlast = false;
    }

    private void LinienlastNeu(AbstraktElementLast linienlast)
    {
        _linienlastNeu = new LinienlastNeu(_modell)
        {
            LastId = { Text = linienlast.LastId },
            ElementId = { Text = linienlast.ElementId.ToString(CultureInfo.CurrentCulture) },
            Pxa = { Text = linienlast.Lastwerte[0].ToString(CultureInfo.CurrentCulture) },
            Pya = { Text = linienlast.Lastwerte[1].ToString(CultureInfo.CurrentCulture) },
            Pxb = { Text = linienlast.Lastwerte[2].ToString(CultureInfo.CurrentCulture) },
            Pyb = { Text = linienlast.Lastwerte[3].ToString(CultureInfo.CurrentCulture) },
            InElement = { IsChecked = linienlast.InElementKoordinatenSystem }
        };
        _isLinienlast = false;
    }

    private void LagerNeu(AbstraktRandbedingung lager)
    {
        _lagerNeu = new LagerNeu(_modell)
        {
            LagerId = { Text = lager.RandbedingungId },
            KnotenId = { Text = lager.KnotenId.ToString(CultureInfo.CurrentCulture) },
            Xfest = { IsChecked = lager.Typ == 1 | lager.Typ == 3 | lager.Typ == 7 },
            Yfest = { IsChecked = lager.Typ == 2 | lager.Typ == 3 | lager.Typ == 7 },
            Rfest = { IsChecked = lager.Typ == 4 | lager.Typ == 7 }
        };
        if ((bool)_lagerNeu.Xfest.IsChecked) _lagerNeu.VorX.Text = lager.Vordefiniert[0].ToString("0.00");
        if ((bool)_lagerNeu.Yfest.IsChecked) _lagerNeu.VorY.Text = lager.Vordefiniert[1].ToString("0.00");
        if ((bool)_lagerNeu.Rfest.IsChecked) _lagerNeu.VorRot.Text = lager.Vordefiniert[2].ToString("0.00");
        _isLager = false;
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

    //private static string Lagertyp(int typ)
    //{
    //    var lagertyp = typ switch
    //    {
    //        1 => "x",
    //        2 => "y",
    //        3 => "xy",
    //        7 => "xyr",
    //        _ => ""
    //    };
    //    return lagertyp;
    //}
}