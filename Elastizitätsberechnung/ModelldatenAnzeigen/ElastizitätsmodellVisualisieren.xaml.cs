using FE_Berechnungen.Elastizitätsberechnung.Ergebnisse;
using FE_Berechnungen.Elastizitätsberechnung.ModelldatenLesen;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using KnotenlastNeu = FE_Berechnungen.Elastizitätsberechnung.ModelldatenLesen.KnotenlastNeu;
using KnotenNetzÄquidistant = FE_Berechnungen.Elastizitätsberechnung.ModelldatenLesen.KnotenNetzÄquidistant;
using KnotenNetzVariabel = FE_Berechnungen.Elastizitätsberechnung.ModelldatenLesen.KnotenNetzVariabel;
using KnotenNeu = FE_Berechnungen.Elastizitätsberechnung.ModelldatenLesen.KnotenNeu;
using LagerNeu = FE_Berechnungen.Elastizitätsberechnung.ModelldatenLesen.LagerNeu;
using MaterialNeu = FE_Berechnungen.Elastizitätsberechnung.ModelldatenLesen.MaterialNeu;
using ModellNeu = FE_Berechnungen.Elastizitätsberechnung.ModelldatenLesen.ModellNeu;

namespace FE_Berechnungen.Elastizitätsberechnung.ModelldatenAnzeigen;

public partial class ElastizitätsmodellVisualisieren
{
    private readonly Darstellung _darstellung;
    private KnotenNeu _knotenNeu;
    private Element2D3Neu _element2D3Neu;
    private MaterialNeu _materialNeu;
    private KnotenlastNeu _knotenlastNeu;
    private LagerNeu _lagerNeu;
    private bool _lastenAn = true, _lagerAn = true, _knotenTexteAn = true, _elementTexteAn = true;
    public bool IsKnoten, IsElement, IsKnotenlast, IsLinienlast, IsLager;
    private readonly List<Shape> _hitList = [];
    private readonly List<TextBlock> _hitTextBlock = [];
    private readonly FeModell _modell;
    private EllipseGeometry _hitArea;
    private bool _isDragging;
    private Point _mittelpunkt;

    public ElastizitätsmodellVisualisieren(FeModell feModell)
    {
        Language = XmlLanguage.GetLanguage("de-DE");
        InitializeComponent();
        VisualElastizitätModel.Children.Remove(Pilot);
        Show();
        VisualElastizitätModel.Background = Brushes.Transparent;
        _modell = feModell;

        try
        {
            _darstellung = new Darstellung(feModell, VisualElastizitätModel);
            _darstellung.ElementeZeichnen();
            // mit Element und Knoten Ids
            _darstellung.KnotenTexte();
            _darstellung.ElementTexte();
            _darstellung.LastenZeichnen();
            _darstellung.LastTexte();
            _darstellung.FesthaltungenZeichnen();
            _darstellung.FesthaltungenTexte();
        }
        catch (ModellAusnahme e)
        {
            _ = MessageBox.Show(e.Message);
        }
    }

    private void OnBtnBerechnen_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (_modell != null)
            {
                if (!_modell.Berechnet)
                {
                    var modellBerechnung = new Berechnung(_modell);
                    modellBerechnung.BerechneSystemMatrix();
                    modellBerechnung.BerechneSystemVektor();
                    modellBerechnung.LöseGleichungen();
                    _modell.Berechnet = true;
                }

                var statikErgebnisse = new StatikErgebnisseVisualisieren(_modell);
                statikErgebnisse.Show();
            }
            else
            {
                _ = MessageBox.Show("Elastizitätsdaten müssen zuerst eingelesen werden", "Elastizitätsberechnung");
            }
        }
        catch (BerechnungAusnahme e2)
        {
            _ = MessageBox.Show(e2.Message);
        }
    }

    // Modelldefinitionen neu definieren und vorhandene editieren
    // Modell
    private void OnBtnModell_Click(object sender, RoutedEventArgs e)
    {
        var modellNeu = new ModellNeu(_modell) { Topmost = true, Owner = (Window)Parent };
        modellNeu.Show();
    }

    // Knoten
    private void MenuKnotenNeu(object sender, RoutedEventArgs e)
    {
        _knotenNeu = new KnotenNeu(_modell) { Topmost = true, Owner = (Window)Parent };
    }
    private void MenuKnotenNetzÄquidistant(object sender, RoutedEventArgs e)
    {
        _ = new KnotenNetzÄquidistant(_modell) { Topmost = true, Owner = (Window)Parent };
    }
    private void MenuKnotenNetzVariabel(object sender, RoutedEventArgs e)
    {
        _ = new KnotenNetzVariabel(_modell) { Topmost = true, Owner = (Window)Parent };
    }

    // Elemente
    private void MenuElement2D3Neu(object sender, RoutedEventArgs e)
    {
        IsElement = true;
        _element2D3Neu = new Element2D3Neu(_modell) { Topmost = true, Owner = (Window)Parent };
        _modell.Berechnet = false;
    }

    private void MenuMaterialNeu(object sender, RoutedEventArgs e)
    {
        _materialNeu = new MaterialNeu(_modell) { Topmost = true, Owner = (Window)Parent };
        _materialNeu.AktuelleMaterialId = _materialNeu.MaterialId.Text;
        _materialNeu.AktuelleQuerschnittId = _materialNeu.QuerschnittId.Text;
        _modell.Berechnet = false;
    }

    // Lasten
    private void MenuKnotenlastNeu(object sender, RoutedEventArgs e)
    {
        IsKnotenlast = true;
        _knotenlastNeu = new KnotenlastNeu(_modell) { Topmost = true, Owner = (Window)Parent };
        _knotenlastNeu.AktuelleId = _knotenlastNeu.LastId.Text;
        _modell.Berechnet = false;
    }

    // Lager
    private void OnBtnLagerNeu_Click(object sender, RoutedEventArgs e)
    {
        IsLager = true;
        _lagerNeu = new LagerNeu(_modell) { Topmost = true, Owner = (Window)Parent };
        _lagerNeu.AktuelleId = _lagerNeu.LagerId.Text;
        _modell.Berechnet = false;
    }

    // Modelldefinitionen darstellen
    private void OnBtnKnotenIDs_Click(object sender, RoutedEventArgs e)
    {
        if (!_knotenTexteAn)
        {
            _darstellung.KnotenTexte();
            _knotenTexteAn = true;
        }
        else
        {
            foreach (TextBlock id in _darstellung.KnotenIDs) VisualElastizitätModel.Children.Remove(id);
            _knotenTexteAn = false;
        }
    }

    private void OnBtnElementIDs_Click(object sender, RoutedEventArgs e)
    {
        if (!_elementTexteAn)
        {
            _darstellung.ElementTexte();
            _elementTexteAn = true;
        }
        else
        {
            foreach (var id in _darstellung.ElementIDs.Cast<TextBlock>()) VisualElastizitätModel.Children.Remove(id);
            _elementTexteAn = false;
        }
    }

    private void OnBtnLasten_Click(object sender, RoutedEventArgs e)
    {
        if (!_lastenAn)
        {
            _darstellung.LastenZeichnen();
            _darstellung.LastTexte();
            _lastenAn = true;
        }
        else
        {
            foreach (var lasten in _darstellung.LastVektoren.Cast<Shape>())
            {
                VisualElastizitätModel.Children.Remove(lasten);
                foreach (var id in _darstellung.LastIDs.Cast<TextBlock>()) VisualElastizitätModel.Children.Remove(id);
            }

            _lastenAn = false;
        }
    }

    private void OnBtnLager_Click(object sender, RoutedEventArgs e)
    {
        if (!_lagerAn)
        {
            _darstellung.FesthaltungenZeichnen();
            //_darstellung.LagerTexte();
            _lagerAn = true;
        }
        else
        {
            foreach (var fest in _darstellung.LagerDarstellung.Cast<Shape>()) VisualElastizitätModel.Children.Remove(fest);
            foreach (var id in _darstellung.LagerIDs.Cast<TextBlock>()) VisualElastizitätModel.Children.Remove(id);
            _lagerAn = false;
        }
    }

    // KnotenNeu setzt Pilotpunkt
    // MouseDown rechte Taste "fängt" Pilotpunkt, MouseMove folgt ihm, MouseUp setzt ihn neu
    private void Pilot_MouseDown(object sender, MouseButtonEventArgs e)
    {
        Pilot.CaptureMouse();
        _isDragging = true;
    }

    private void Pilot_MouseMove(object sender, MouseEventArgs e)
    {
        if (!_isDragging) return;
        var canvPosToWindow = VisualElastizitätModel.TransformToAncestor(this).Transform(new Point(0, 0));

        if (sender is not Ellipse knoten) return;
        var upperlimit = canvPosToWindow.Y + knoten.Height / 2;
        var lowerlimit = canvPosToWindow.Y + VisualElastizitätModel.ActualHeight - knoten.Height / 2;

        var leftlimit = canvPosToWindow.X + knoten.Width / 2;
        var rightlimit = canvPosToWindow.X + VisualElastizitätModel.ActualWidth - knoten.Width / 2;


        var absmouseXpos = e.GetPosition(this).X;
        var absmouseYpos = e.GetPosition(this).Y;

        if (!(absmouseXpos > leftlimit) || !(absmouseXpos < rightlimit)
                                        || !(absmouseYpos > upperlimit) || !(absmouseYpos < lowerlimit)) return;

        _mittelpunkt = new Point(e.GetPosition(VisualElastizitätModel).X, e.GetPosition(VisualElastizitätModel).Y);

        Canvas.SetLeft(knoten, _mittelpunkt.X - Pilot.Width / 2);
        Canvas.SetTop(knoten, _mittelpunkt.Y - Pilot.Height / 2);

        var koordinaten = _darstellung.TransformBildPunkt(_mittelpunkt);
        _knotenNeu.X.Text = koordinaten[0].ToString("N2", CultureInfo.CurrentCulture);
        _knotenNeu.Y.Text = koordinaten[1].ToString("N2", CultureInfo.CurrentCulture);
    }

    private void Pilot_MouseUp(object sender, MouseButtonEventArgs e)
    {
        Pilot.ReleaseMouseCapture();
        _isDragging = false;
        IsKnoten = false;
    }

    private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        _hitList.Clear();
        _hitTextBlock.Clear();
        var hitPoint = e.GetPosition(VisualElastizitätModel);
        _hitArea = new EllipseGeometry(hitPoint, 1.0, 1.0);
        VisualTreeHelper.HitTest(VisualElastizitätModel, null, HitTestCallBack,
            new GeometryHitTestParameters(_hitArea));

        // click auf Canvas weder Text noch Shape ⇾ neuer Knoten wird mit Zeiger platziert und bewegt
        if (_hitList.Count == 0 && _hitTextBlock.Count == 0)
        {
            if (_knotenNeu == null) return;
            _mittelpunkt = new Point(e.GetPosition(VisualElastizitätModel).X, e.GetPosition(VisualElastizitätModel).Y);
            Canvas.SetLeft(Pilot, _mittelpunkt.X - Pilot.Width / 2);
            Canvas.SetTop(Pilot, _mittelpunkt.Y - Pilot.Height / 2);
            VisualElastizitätModel.Children.Remove(Pilot);

            var koordinaten = _darstellung.TransformBildPunkt(_mittelpunkt);
            _knotenNeu.X.Text = koordinaten[0].ToString("N2", CultureInfo.CurrentCulture);
            _knotenNeu.Y.Text = koordinaten[1].ToString("N2", CultureInfo.CurrentCulture);
            return;
        }

        // click auf Textdarstellungen
        foreach (var item in _hitTextBlock)
        {
            // Textdarstellung ist ein Knoten
            if (_modell.Knoten.TryGetValue(item.Text, out var knoten))
            {
                IsKnoten = true;
                KnotenClick(knoten);
            }

            // Textdarstellung ist ein Element
            else if (_modell.Elemente.TryGetValue(item.Text, out var element))
            {
                // bei der Definition eines neuen Lagers ist Elementeingabe ungültig
                if (IsLager)
                {
                    _ = MessageBox.Show("Elementeingabe ungültig bei Definition eines neuen Lagers", "neue Linienlast");
                }
                // bei der Definition einer neuen Knotenlast ist Elementeingabe ungültig
                else if (IsKnotenlast)
                {
                    _ = MessageBox.Show("Elementeingabe ungültig bei Definition einer neuen Knotenlast", "neue Linienlast");
                }
                else
                {
                    ElementNeu(element);
                }
            }

            // Textdarstellung ist eine Knotenlast
            else if (_modell.Lasten.TryGetValue(item.Text, out var knotenlast))
            {
                _knotenlastNeu = new KnotenlastNeu(_modell, knotenlast);
                IsKnotenlast = true;
            }

            // Textdarstellung ist ein Lager
            else if (_modell.Randbedingungen.TryGetValue(item.Text, out var lager))
            {
                _lagerNeu = new LagerNeu(_modell, lager);
                IsLager = true;
            }
        }

        // click auf Shape Darstellungen
        // nur neu, falls nicht im Benutzerdialog aktiviert
        foreach (var item in _hitList.TakeWhile(_ => !IsKnoten && !IsElement && !IsKnotenlast && !IsLinienlast)
                     .Where(item => item.Name != null))
        {
            // Elemente
            if (_modell.Elemente.TryGetValue(item.Name, out var element))
                ElementNeu(element);

            // Lasten
            else if (_modell.Lasten.TryGetValue(item.Name, out var knotenlast))
            {
                _knotenlastNeu = new KnotenlastNeu(_modell, knotenlast);
                IsKnotenlast = true;
            }
            //else if (_modell.ElementLasten.TryGetValue(item.Name, out var linienlast))
            //{
            //    //_linienlastNeu = new LinienlastNeu(_modell, linienlast);
            //    //IsLinienlast = true;
            //}

            // Lager
            else if (_modell.Randbedingungen.TryGetValue(item.Name, out var lager))
            {
                _lagerNeu = new LagerNeu(_modell, lager);
                IsLager = true;
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

    private void KnotenClick(Knoten knoten)
    {
        // Knotentexte angeklickt bei Definition eines neuen Elementes
        if (IsElement)
        {
            if (_element2D3Neu.Knoten1Id.Text == string.Empty)
            {
                _element2D3Neu.Knoten1Id.Text = knoten.Id;
            }
            else if (_element2D3Neu.Knoten2Id.Text == string.Empty)
            {
                _element2D3Neu.Knoten2Id.Text = knoten.Id;
            }
            else
            {
                _element2D3Neu.Knoten3Id.Text = knoten.Id;
                _element2D3Neu.ElementId.Text = "e" + _element2D3Neu.Knoten1Id.Text + _element2D3Neu.Knoten2Id.Text + knoten.Id;
            }
            _element2D3Neu.Show();
            return;
        }

        // Knotentext angeklickt bei Definition einer neuen Knotenlast
        if (IsKnotenlast)
        {
            if (_element2D3Neu.Knoten1Id.Text == string.Empty)
            {
                _element2D3Neu.Knoten1Id.Text = knoten.Id;
            }
            else if (_element2D3Neu.Knoten2Id.Text == string.Empty)
            {
                _element2D3Neu.Knoten2Id.Text = knoten.Id;
            }
            else
            {
                _element2D3Neu.Knoten3Id.Text = knoten.Id;
                _element2D3Neu.ElementId.Text = "e" + _element2D3Neu.Knoten1Id.Text + _element2D3Neu.Knoten2Id.Text + knoten.Id;
            }
            _element2D3Neu.Show();
            return;
        }

        // Knotentext angeklickt bei Definition eines neuen Lagers
        if (IsLager)
        {
            _lagerNeu.KnotenId.Text = knoten.Id;
            if (_lagerNeu.LagerId.Text == string.Empty) _lagerNeu.LagerId.Text = "L" + knoten.Id;
            _lagerNeu.AktuelleId = _lagerNeu.LagerId.Text;
            _lagerNeu.Show();
            return;
        }

        // Knotentext angeklickt, um vorhandenen Knoten zu editieren
        KnotenEdit(knoten);
    }

    private void KnotenEdit(Knoten knoten)
    {
        _knotenNeu = new KnotenNeu(_modell)
        {
            Topmost = true,
            Owner = (Window)Parent,
            KnotenId = { Text = knoten.Id },
            AnzahlDof = { Text = knoten.AnzahlKnotenfreiheitsgrade.ToString("N0", CultureInfo.CurrentCulture) },
            X = { Text = knoten.Koordinaten[0].ToString("N2", CultureInfo.CurrentCulture) },
            Y = { Text = knoten.Koordinaten[1].ToString("N2", CultureInfo.CurrentCulture) }
        };

        _mittelpunkt = new Point(knoten.Koordinaten[0] * _darstellung.Auflösung + _darstellung.PlatzierungH,
            (-knoten.Koordinaten[1] + _darstellung.MaxY) * _darstellung.Auflösung + _darstellung.PlatzierungV);
        Canvas.SetLeft(Pilot, _mittelpunkt.X - Pilot.Width / 2);
        Canvas.SetTop(Pilot, _mittelpunkt.Y - Pilot.Height / 2);
        VisualElastizitätModel.Children.Add(Pilot);
    }

    private void ElementNeu(AbstraktElement element)
    {
        // anderer Elementtext angeklickt beim Erstellen eines neuen Elementes
        // Material- und Querschnitteigenschaften werden übernommen
        if (IsElement)
        {
            _element2D3Neu.MaterialId.Text = element.ElementMaterialId;
            if (element.E > 0)
                _element2D3Neu.EModul.Text = element.E.ToString("E2", CultureInfo.CurrentCulture);
            if (element.Nue > 0)
                _element2D3Neu.Poisson.Text = element.M.ToString("E2", CultureInfo.CurrentCulture);

            _element2D3Neu.QuerschnittId.Text = element.ElementQuerschnittId;
            if (element.Dicke > 0)
                _element2D3Neu.Dicke.Text = element.A.ToString("E2", CultureInfo.CurrentCulture);

            IsElement = false;
            return;
        }

        // Elementeigenschaften können editiert werden
        var emodul = element.E == 0 ? string.Empty : element.E.ToString("E2", CultureInfo.CurrentCulture);
        var poisson = element.Nue == 0 ? string.Empty : element.Nue.ToString("E2", CultureInfo.CurrentCulture);
        var dicke = element.Dicke == 0 ? string.Empty : element.Dicke.ToString("E2", CultureInfo.CurrentCulture);

        _element2D3Neu = new Element2D3Neu(_modell)
        {
            Topmost = true,
            Owner = (Window)Parent,
            ElementId = { Text = element.ElementId },
            Knoten1Id = { Text = element.KnotenIds[0] },
            Knoten2Id = { Text = element.KnotenIds[1] },
            Knoten3Id = { Text = element.KnotenIds[2] },
            MaterialId = { Text = element.ElementMaterialId },
            EModul = { Text = emodul },
            Poisson = { Text = poisson },
            QuerschnittId = { Text = element.ElementQuerschnittId },
            Dicke = { Text = dicke },
        };
        IsElement = true;
    }
}