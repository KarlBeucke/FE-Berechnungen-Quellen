using FE_Berechnungen.Tragwerksberechnung.Ergebnisse;
using FE_Berechnungen.Tragwerksberechnung.Modelldaten;
using FE_Berechnungen.Tragwerksberechnung.ModelldatenLesen;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using ElementNeu = FE_Berechnungen.Tragwerksberechnung.ModelldatenLesen.ElementNeu;
using KnotenGruppeNeu = FE_Berechnungen.Tragwerksberechnung.ModelldatenLesen.KnotenGruppeNeu;
using KnotenlastNeu = FE_Berechnungen.Tragwerksberechnung.ModelldatenLesen.KnotenlastNeu;
using KnotenNetzÄquidistant = FE_Berechnungen.Tragwerksberechnung.ModelldatenLesen.KnotenNetzÄquidistant;
using KnotenNetzVariabel = FE_Berechnungen.Tragwerksberechnung.ModelldatenLesen.KnotenNetzVariabel;
using KnotenNeu = FE_Berechnungen.Tragwerksberechnung.ModelldatenLesen.KnotenNeu;
using LinienlastNeu = FE_Berechnungen.Tragwerksberechnung.ModelldatenLesen.LinienlastNeu;
using MaterialNeu = FE_Berechnungen.Tragwerksberechnung.ModelldatenLesen.MaterialNeu;
using ZeitAnregungVisualisieren = FE_Berechnungen.Tragwerksberechnung.ModelldatenLesen.ZeitAnregungVisualisieren;
using ZeitintegrationNeu = FE_Berechnungen.Tragwerksberechnung.ModelldatenLesen.ZeitintegrationNeu;
using ZeitKnotenlastNeu = FE_Berechnungen.Tragwerksberechnung.ModelldatenLesen.ZeitKnotenlastNeu;

namespace FE_Berechnungen.Tragwerksberechnung.ModelldatenAnzeigen;

public partial class TragwerkmodellVisualisieren
{
    private EllipseGeometry _hitArea;
    //alle gefundenen "Shapes" werden in dieser Liste gesammelt
    private readonly List<Shape> _hitList = [];
    //alle gefundenen "TextBlocks" werden in dieser Liste gesammelt
    private readonly List<TextBlock> _hitTextBlock = [];
    private bool _isDragging;
    private Point _mittelpunkt;

    private readonly FeModell _modell;
    public readonly Darstellung Darstellung;
    private bool _lastenAn = true, _lagerAn = true, _knotenTexteAn = true, _elementTexteAn = true;
    private bool _dynamikAn = true;
    private KnotenNeu _knotenNeu;
    private ElementNeu _elementNeu;
    private QuerschnittNeu _querschnittNeu;
    private MaterialNeu _materialNeu;
    private KnotenlastNeu _knotenlastNeu;
    private LinienlastNeu _linienlastNeu;
    private PunktlastNeu _punktlastNeu;
    private LagerNeu _lagerNeu;
    private ZeitKnotenlastNeu _zeitKnotenlastNeu;
    private ZeitKnotenanfangswerteNeu _zeitAnfangsbedingungNeu;
    public ZeitintegrationNeu ZeitintegrationNeu;
    public bool IsKnoten, IsElement, IsKnotenlast, IsLinienlast, IsPunktlast, IsLager;
    public bool IsZeitKnotenlast, IsZeitAnfangsbedingung;

    public TragwerkmodellVisualisieren(FeModell feModell)
    {
        Language = XmlLanguage.GetLanguage("de-DE");
        InitializeComponent();
        VisualTragwerkModel.Children.Remove(Pilot);
        Show();
        VisualTragwerkModel.Background = Brushes.Transparent;
        _modell = feModell;

        try
        {
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
            Darstellung.DynamikTexte();
            Darstellung.DynamikLastenZeichnen();
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
                _ = MessageBox.Show("Tragwerksdaten müssen zuerst eingelesen werden", "statische Tragwerksberechnung");
            }
        }
        catch (BerechnungAusnahme e2)
        {
            _ = MessageBox.Show(e2.Message);
        }
    }

    private void DynamischeDaten(object sender, RoutedEventArgs e)
    {
        if (_modell.ZeitintegrationDaten && _modell != null)
        {
            var tragwerk = new DynamikDatenAnzeigen(_modell);
            tragwerk.Show();
        }
        else
        {
            _ = MessageBox.Show("Daten für Zeitintegration sind noch nicht spezifiziert", "Tragwerksberechnung");
        }
    }

    private void DynamischeBerechnung(object sender, RoutedEventArgs e)
    {
        try
        {
            if (_modell.ZeitintegrationDaten && _modell != null)
            {
                var modellBerechnung = new Berechnung(_modell);
                modellBerechnung.BerechneSystemMatrix();
                modellBerechnung.BerechneSystemVektor();
                modellBerechnung.LöseGleichungen();
                _modell.Berechnet = true;

                modellBerechnung.ZeitintegrationZweiterOrdnung();
                _modell.ZeitintegrationBerechnet = true;
            }
            else
            {
                _ = MessageBox.Show("Daten für Zeitintegration sind noch nicht spezifiziert", "Tragwerksberechnung");
            }
        }
        catch (BerechnungAusnahme e2)
        {
            _ = MessageBox.Show(e2.Message);
        }
    }
    private void DynamischeModellzuständeVisualisieren(object sender, RoutedEventArgs e)
    {
        if (_modell.ZeitintegrationBerechnet && _modell != null)
        {
            var dynamikErgebnisse = new DynamischeModellzuständeVisualisieren(_modell);
            dynamikErgebnisse.Show();
        }
        else
        {
            _ = MessageBox.Show("Zeitintegration noch nicht ausgeführt!!", "dynamische Tragwerksberechnung");
        }
    }

    private void KnotenzeitverläufeTragwerkVisualisieren(object sender, RoutedEventArgs e)
    {
        if (_modell.ZeitintegrationBerechnet && _modell != null)
        {
            var knotenzeitverläufe = new KnotenzeitverläufeVisualisieren(_modell);
            knotenzeitverläufe.Show();
        }
        else
        {
            _ = MessageBox.Show("Zeitintegration noch nicht ausgeführt!!", "dynamische Tragwerksberechnung");
        }
    }


    // Modelldefinitionen neu definieren und vorhandene editieren
    private void OnBtnAbmessungen_Click(object sender, RoutedEventArgs e)
    {
        var abmessungenNeu = new AbmessungenNeu(_modell) { Topmost = true, Owner = (Window)Parent };
        abmessungenNeu.Show();
    }
    // Knoten
    private void MenuBalkenKnotenNeu(object sender, RoutedEventArgs e)
    {
        _knotenNeu = new KnotenNeu(_modell) { Topmost = true, Owner = (Window)Parent };
        _modell.Berechnet = false;
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

    //Elemente
    private void MenuBalkenElementNeu(object sender, RoutedEventArgs e)
    {
        IsElement = true;
        _elementNeu = new ElementNeu(_modell) { Topmost = true, Owner = (Window)Parent };
        _modell.Berechnet = false;
    }

    private void MenuQuerschnittNeu(object sender, RoutedEventArgs e)
    {
        _querschnittNeu = new QuerschnittNeu(_modell) { Topmost = true, Owner = (Window)Parent };
        _querschnittNeu.AktuelleId = _querschnittNeu.QuerschnittId.Text;
        _modell.Berechnet = false;
    }

    private void MenuMaterialNeu(object sender, RoutedEventArgs e)
    {
        _materialNeu = new MaterialNeu(_modell) { Topmost = true, Owner = (Window)Parent };
        _materialNeu.AktuelleId = _materialNeu.MaterialId.Text;
        _modell.Berechnet = false;
    }

    //Lasten
    private void MenuKnotenlastNeu(object sender, RoutedEventArgs e)
    {
        IsKnotenlast = true;
        _knotenlastNeu = new KnotenlastNeu(_modell) { Topmost = true, Owner = (Window)Parent };
        _knotenlastNeu.AktuelleId = _knotenlastNeu.LastId.Text;
        _modell.Berechnet = false;
    }

    private void MenuLinienlastNeu(object sender, RoutedEventArgs e)
    {
        IsLinienlast = true;
        _linienlastNeu = new LinienlastNeu(_modell) { Topmost = true, Owner = (Window)Parent };
        _linienlastNeu.AktuelleId = _linienlastNeu.LastId.Text;
        _modell.Berechnet = false;
    }

    private void MenuPunktlastNeu(object sender, RoutedEventArgs e)
    {
        IsPunktlast = true;
        _punktlastNeu = new PunktlastNeu(_modell) { Topmost = true, Owner = (Window)Parent };
        _punktlastNeu.AktuelleId = _punktlastNeu.LastId.Text;
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

    //Dynamik
    private void MenuZeitintegrationNeu(object sender, RoutedEventArgs e)
    {
        ZeitintegrationNeu = new ZeitintegrationNeu(_modell);// { Topmost = true, Owner = (Window)Parent };
    }

    private void MenuAnfangswerteNeu(object sender, RoutedEventArgs e)
    {
        IsZeitAnfangsbedingung = true;
        _zeitAnfangsbedingungNeu = new ZeitKnotenanfangswerteNeu(_modell) { Topmost = true, Owner = (Window)Parent };
        _modell.Berechnet = false;
    }

    private void MenuZeitKnotenlastNeu(object sender, RoutedEventArgs e)
    {
        IsZeitKnotenlast = true;
        _zeitKnotenlastNeu = new ZeitKnotenlastNeu(_modell) { Topmost = true, Owner = (Window)Parent };
        _modell.Berechnet = false;
    }

    private void MenuZeitAnregungNeu(object sender, RoutedEventArgs e)
    {
        _ = new ZeitAnregungVisualisieren(_modell);
        //anregung.Show();
    }

    // Modelldefinitionen darstellen
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

    private void OnBtnDynamik_Click(object sender, RoutedEventArgs e)
    {
        if (!_dynamikAn)
        {
            Darstellung.DynamikLastenZeichnen();
            Darstellung.DynamikTexte();
            _dynamikAn = true;
        }
        else
        {
            foreach (var id in Darstellung.DynamikIDs.SelectMany(_ => Darstellung.DynamikIDs.Cast<TextBlock>()))
            {
                VisualTragwerkModel.Children.Remove(id);
            }
            foreach (var lasten in Darstellung.DynamikVektoren.Cast<Shape>())
            {
                VisualTragwerkModel.Children.Remove(lasten);
                //foreach (var id in Darstellung.DynamikIDs.Cast<TextBlock>()) VisualTragwerkModel.Children.Remove(id);
            }
            _dynamikAn = false;
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

        Canvas.SetLeft(knoten, _mittelpunkt.X - Pilot.Width / 2);
        Canvas.SetTop(knoten, _mittelpunkt.Y - Pilot.Height / 2);

        var koordinaten = Darstellung.TransformBildPunkt(_mittelpunkt);
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
        var hitPoint = e.GetPosition(VisualTragwerkModel);
        _hitArea = new EllipseGeometry(hitPoint, 1.0, 1.0);
        VisualTreeHelper.HitTest(VisualTragwerkModel, null, HitTestCallBack,
            new GeometryHitTestParameters(_hitArea));

        // click auf Canvas weder Text noch Shape ⇾ neuer Knoten wird mit Zeiger platziert und bewegt
        if (_hitList.Count == 0 && _hitTextBlock.Count == 0)
        {
            if (_knotenNeu == null) return;
            _mittelpunkt = new Point(e.GetPosition(VisualTragwerkModel).X, e.GetPosition(VisualTragwerkModel).Y);
            Canvas.SetLeft(Pilot, _mittelpunkt.X - Pilot.Width / 2);
            Canvas.SetTop(Pilot, _mittelpunkt.Y - Pilot.Height / 2);
            VisualTragwerkModel.Children.Remove(Pilot);

            var koordinaten = Darstellung.TransformBildPunkt(_mittelpunkt);
            _knotenNeu.X.Text = koordinaten[0].ToString("N2", CultureInfo.CurrentCulture);
            _knotenNeu.Y.Text = koordinaten[1].ToString("N2", CultureInfo.CurrentCulture);
            return;
        }

        // click auf Shape Darstellungen
        // nur neu, falls nicht im Benutzerdialog aktiviert
        foreach (var item in _hitList.TakeWhile(_ => !IsKnoten && !IsElement && !IsKnotenlast && !IsLinienlast && !IsPunktlast)
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
            else if (_modell.ElementLasten.TryGetValue(item.Name, out var linienlast))
            {
                _linienlastNeu = new LinienlastNeu(_modell, linienlast);
                IsLinienlast = true;
            }
            else if (_modell.PunktLasten.TryGetValue(item.Name, out var last))
            {
                var punktlast = (PunktLast)last;
                _punktlastNeu = new PunktlastNeu(_modell, punktlast);
                IsLinienlast = true;
            }

            // Lager
            else if (_modell.Randbedingungen.TryGetValue(item.Name, out var lager))
            {
                _lagerNeu = new LagerNeu(_modell, lager);
                IsLager = true;
            }

            // zeitabhängige Knotenlasten
            else if (_modell.ZeitabhängigeKnotenLasten.TryGetValue(item.Name, out var zeitKnotenlast))
            {
                _zeitKnotenlastNeu = new ZeitKnotenlastNeu(_modell, zeitKnotenlast);
                IsZeitKnotenlast = true;
            }
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
                    return;
                }
                // bei der Definition einer neuen Knotenlast ist Elementeingabe ungültig
                else if (IsKnotenlast)
                {
                    _ = MessageBox.Show("Elementeingabe ungültig bei Definition einer neuen Knotenlast", "neue Linienlast");
                    return;
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
            // Textdarstellung ist eine Elementlast (Linienlast)
            else if (_modell.ElementLasten.TryGetValue(item.Text, out var linienlast))
            {
                _linienlastNeu = new LinienlastNeu(_modell, linienlast);
                IsLinienlast = true;
            }
            // Textdarstellung ist eine Punktlast
            else if (_modell.PunktLasten.TryGetValue(item.Text, out var last))
            {
                var punktlast = (PunktLast)last;
                _punktlastNeu = new PunktlastNeu(_modell, punktlast);
                IsLinienlast = true;
            }

            // Textdarstellung ist ein Lager
            else if (_modell.Randbedingungen.TryGetValue(item.Text, out var lager))
            {
                _lagerNeu = new LagerNeu(_modell, lager);
                IsLager = true;
            }

            // Textdarstellung ist eine Anfangsbedingung
            else if (item.Uid == "A")
            {
                var knotenId = item.Text[1..];
                var aktuell = _modell.Zeitintegration.Anfangsbedingungen.FindIndex((a => a.KnotenId == knotenId));

                if (aktuell < 0)
                {
                    _ = MessageBox.Show("Knoten Id für Anfangsbedingung konnte nicht gefunden werden", "neue Anfangsbedingung");
                    return;
                }
                if (_modell.Zeitintegration == null)
                {
                    _ = MessageBox.Show("Zeitintegration noch nicht definiert", "neue Anfangsbedingung");
                    return;
                }

                _zeitAnfangsbedingungNeu ??= new ZeitKnotenanfangswerteNeu(_modell, aktuell, false)
                {
                    KnotenId = { Text = knotenId }
                };

                if (aktuell >= _modell.Zeitintegration.Anfangsbedingungen.Count) aktuell = _modell.Zeitintegration.Anfangsbedingungen.Count - 1;
                _zeitAnfangsbedingungNeu.Dof1D0.Text = _modell.Zeitintegration.Anfangsbedingungen[aktuell].Werte[0]
                    .ToString("G2", CultureInfo.CurrentCulture);
                _zeitAnfangsbedingungNeu.Dof1V0.Text = _modell.Zeitintegration.Anfangsbedingungen[aktuell].Werte[1]
                    .ToString("G2", CultureInfo.CurrentCulture);
                if (_modell.Zeitintegration.Anfangsbedingungen[aktuell].Werte.Length > 2)
                {
                    _zeitAnfangsbedingungNeu.Dof2D0.Text = _modell.Zeitintegration.Anfangsbedingungen[aktuell].Werte[2]
                        .ToString("G2", CultureInfo.CurrentCulture);
                    _zeitAnfangsbedingungNeu.Dof2V0.Text = _modell.Zeitintegration.Anfangsbedingungen[aktuell].Werte[3]
                        .ToString("G2", CultureInfo.CurrentCulture);
                }
                if (_modell.Zeitintegration.Anfangsbedingungen[aktuell].Werte.Length > 4)
                {
                    _zeitAnfangsbedingungNeu.Dof3D0.Text = _modell.Zeitintegration.Anfangsbedingungen[aktuell].Werte[4]
                        .ToString("G2", CultureInfo.CurrentCulture);
                    _zeitAnfangsbedingungNeu.Dof3V0.Text = _modell.Zeitintegration.Anfangsbedingungen[aktuell].Werte[5]
                        .ToString("G2", CultureInfo.CurrentCulture);
                }
                IsZeitAnfangsbedingung = true;
            }
            // Textdarstellung ist eine zeitveränderliche Knotenlast
            else if (_modell.ZeitabhängigeKnotenLasten.TryGetValue(item.Text, out var zeitKnotenlast))
            {
                _zeitKnotenlastNeu = new ZeitKnotenlastNeu(_modell, zeitKnotenlast);
                IsZeitKnotenlast = true;
            }
        }
    }

    public void KnotenClick(Knoten knoten)
    {
        // Knotentexte angeklickt bei Definition eines neuen Elementes
        if (IsElement)
        {
            if (_elementNeu.StartknotenId.Text == string.Empty)
            {
                _elementNeu.StartknotenId.Text = knoten.Id;
            }
            else
            {
                _elementNeu.EndknotenId.Text = knoten.Id;
                _elementNeu.ElementId.Text = "e" + _elementNeu.StartknotenId.Text + knoten.Id;
            }
            _elementNeu.Show();
            return;
        }

        // Knotentext angeklickt bei Definition einer neuen Knotenlast
        if (IsKnotenlast)
        {
            _knotenlastNeu.KnotenId.Text = knoten.Id;
            _knotenlastNeu.LastId.Text = "KL_" + knoten.Id;
            _knotenlastNeu.AktuelleId = _knotenlastNeu.LastId.Text;
            _knotenlastNeu.Show();
            return;
        }

        // Knotentext angeklickt bei Definition einer neuen Linienlast
        if (IsLinienlast)
        {
            _ = MessageBox.Show("Knoteneingabe ungültig bei Definition einer neuen Elementlast", "neue Linienlast");
            return;
        }

        // Knotentext angeklickt bei Definition einer neuen Elementlast
        if (IsPunktlast)
        {
            _ = MessageBox.Show("Knoteneingabe ungültig bei Definition einer neuen Elementlast", "neue Punktlast");
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

        // Knotentext angeklickt bei Definition einer neuen Anfangsbedingung
        if (IsZeitAnfangsbedingung)
        {
            _zeitAnfangsbedingungNeu.KnotenId.Text = knoten.Id;
            _zeitAnfangsbedingungNeu.Show();
            return;
        }

        // Knotentext angeklickt bei Definition einer neuen zeitveränderlichen Knotenlast
        if (IsZeitKnotenlast)
        {
            _zeitKnotenlastNeu.KnotenId.Text = knoten.Id;
            _zeitKnotenlastNeu.LastId.Text = "zKl" + knoten.Id;
            _zeitKnotenlastNeu.AktuelleId = _zeitKnotenlastNeu.LastId.Text;
            _zeitKnotenlastNeu.Show();
            return;
        }

        // Knotentext angeklickt, um vorhandenen Knoten zu editieren
        KnotenEdit(knoten);
    }

    public void KnotenEdit(Knoten knoten)
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

        _mittelpunkt = new Point(knoten.Koordinaten[0] * Darstellung.Auflösung + Darstellung.PlatzierungH,
            (-knoten.Koordinaten[1] + Darstellung.MaxY) * Darstellung.Auflösung + Darstellung.PlatzierungV);
        Canvas.SetLeft(Pilot, _mittelpunkt.X - Pilot.Width / 2);
        Canvas.SetTop(Pilot, _mittelpunkt.Y - Pilot.Height / 2);
        VisualTragwerkModel.Children.Add(Pilot);
    }

    private void ElementNeu(AbstraktElement element)
    {
        // anderer Elementtext angeklickt beim Erstellen eines neuen Elementes
        // Material- und Querschnitteigenschaften werden übernommen
        if (IsElement)
        {
            _elementNeu.MaterialId.Text = element.ElementMaterialId;
            if (element.E > 0)
                _elementNeu.EModul.Text = element.E.ToString("E2", CultureInfo.CurrentCulture);
            if (element.M > 0)
                _elementNeu.Masse.Text = element.M.ToString("E2", CultureInfo.CurrentCulture);

            _elementNeu.QuerschnittId.Text = element.ElementQuerschnittId;
            if (element.A > 0)
                _elementNeu.Fläche.Text = element.A.ToString("E2", CultureInfo.CurrentCulture);
            if (element.I > 0)
                _elementNeu.Trägheitsmoment.Text = element.I.ToString("E2", CultureInfo.CurrentCulture);

            IsElement = false;
            return;
        }

        // Elementtext angeklickt bei Definition einer neuen Linienlast
        if (IsLinienlast)
        {
            _linienlastNeu.ElementId.Text = element.ElementId;
            _linienlastNeu.LastId.Text = "LL_" + element.ElementId;
            _linienlastNeu.AktuelleId = _linienlastNeu.LastId.Text;
            _linienlastNeu.Show();
            return;
        }

        // Elementtext angeklickt bei Definition einer neuen Punktlast
        if (IsPunktlast)
        {
            _punktlastNeu.ElementId.Text = element.ElementId;
            _punktlastNeu.LastId.Text = "PL_" + element.ElementId;
            _punktlastNeu.AktuelleId = _punktlastNeu.LastId.Text;
            _punktlastNeu.Show();
            return;
        }

        // Elementeigenschaften können editiert werden
        var emodul = element.E == 0 ? string.Empty : element.E.ToString("E2", CultureInfo.CurrentCulture);
        var masse = element.M == 0 ? string.Empty : element.M.ToString("E2", CultureInfo.CurrentCulture);
        var fläche = element.A == 0 ? string.Empty : element.A.ToString("E2", CultureInfo.CurrentCulture);
        var trägheitsmoment = element.I == 0 ? string.Empty : element.I.ToString("E2", CultureInfo.CurrentCulture);

        switch (element)
        {
            case FederElement:
                {
                    _elementNeu = new ElementNeu(_modell)
                    {
                        Topmost = true,
                        Owner = (Window)Parent,
                        FederCheck = { IsChecked = true },
                        ElementId = { Text = element.ElementId },
                        StartknotenId = { Text = element.KnotenIds[0] },
                        MaterialId = { Text = element.ElementMaterialId }
                    };
                    break;
                }
            case Fachwerk:
                {
                    _elementNeu = new ElementNeu(_modell)
                    {
                        Topmost = true,
                        Owner = (Window)Parent,
                        FachwerkCheck = { IsChecked = true },
                        ElementId = { Text = element.ElementId },
                        StartknotenId = { Text = element.KnotenIds[0] },
                        EndknotenId = { Text = element.KnotenIds[1] },
                        MaterialId = { Text = element.ElementMaterialId },
                        EModul = { Text = emodul },
                        Masse = { Text = masse },
                        QuerschnittId = { Text = element.ElementQuerschnittId },
                        Fläche = { Text = fläche },
                        Trägheitsmoment = { Text = trägheitsmoment },
                        Gelenk1 = { IsChecked = true },
                        Gelenk2 = { IsChecked = true }
                    };
                    break;
                }
            case Biegebalken:
                {
                    _elementNeu = new ElementNeu(_modell)
                    {
                        Topmost = true,
                        Owner = (Window)Parent,
                        BalkenCheck = { IsChecked = true },
                        ElementId = { Text = element.ElementId },
                        StartknotenId = { Text = element.KnotenIds[0] },
                        EndknotenId = { Text = element.KnotenIds[1] },
                        MaterialId = { Text = element.ElementMaterialId },
                        EModul = { Text = emodul },
                        Masse = { Text = masse },
                        QuerschnittId = { Text = element.ElementQuerschnittId },
                        Fläche = { Text = fläche },
                        Trägheitsmoment = { Text = trägheitsmoment },
                        Gelenk1 = { IsChecked = false },
                        Gelenk2 = { IsChecked = false }
                    };
                    break;
                }
            case BiegebalkenGelenk:
                {
                    _elementNeu = new ElementNeu(_modell)
                    {
                        Topmost = true,
                        Owner = (Window)Parent,
                        BalkenCheck = { IsChecked = true },
                        ElementId = { Text = element.ElementId },
                        StartknotenId = { Text = element.KnotenIds[0] },
                        EndknotenId = { Text = element.KnotenIds[1] },
                        MaterialId = { Text = element.ElementMaterialId },
                        EModul = { Text = emodul },
                        Masse = { Text = masse },
                        QuerschnittId = { Text = element.ElementQuerschnittId },
                        Fläche = { Text = fläche },
                        Trägheitsmoment = { Text = trägheitsmoment }
                    };
                    switch (element.Typ)
                    {
                        case 1:
                            {
                                _elementNeu.Gelenk1.IsChecked = true;
                                _elementNeu.Gelenk2.IsChecked = false;
                                break;
                            }
                        case 2:
                            {
                                _elementNeu.Gelenk1.IsChecked = false;
                                _elementNeu.Gelenk2.IsChecked = true;
                                break;
                            }
                    }
                    break;
                }
        }
        IsElement = true;
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