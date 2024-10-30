using FE_Berechnungen.Wärmeberechnung.Ergebnisse;
using FE_Berechnungen.Wärmeberechnung.Modelldaten;
using FE_Berechnungen.Wärmeberechnung.ModelldatenLesen;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Shapes;
using Element2D3 = FE_Berechnungen.Wärmeberechnung.Modelldaten.Element2D3;
using ElementKeys = FE_Berechnungen.Wärmeberechnung.ModelldatenLesen.ElementKeys;
using ElementNeu = FE_Berechnungen.Wärmeberechnung.ModelldatenLesen.ElementNeu;
using KnotenGruppeNeu = FE_Berechnungen.Wärmeberechnung.ModelldatenLesen.KnotenGruppeNeu;
using KnotenKeys = FE_Berechnungen.Wärmeberechnung.ModelldatenLesen.KnotenKeys;
using KnotenlastNeu = FE_Berechnungen.Wärmeberechnung.ModelldatenLesen.KnotenlastNeu;
using KnotenNetzÄquidistant = FE_Berechnungen.Wärmeberechnung.ModelldatenLesen.KnotenNetzÄquidistant;
using KnotenNetzVariabel = FE_Berechnungen.Wärmeberechnung.ModelldatenLesen.KnotenNetzVariabel;
using KnotenNeu = FE_Berechnungen.Wärmeberechnung.ModelldatenLesen.KnotenNeu;
using LinienlastNeu = FE_Berechnungen.Wärmeberechnung.ModelldatenLesen.LinienlastNeu;
using MaterialNeu = FE_Berechnungen.Wärmeberechnung.ModelldatenLesen.MaterialNeu;
using ZeitintegrationNeu = FE_Berechnungen.Wärmeberechnung.ModelldatenLesen.ZeitintegrationNeu;

namespace FE_Berechnungen.Wärmeberechnung.ModelldatenAnzeigen;

public partial class WärmemodellVisualisieren
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
    private bool _knotenAn = true, _elementeAn = true, _lastenAn = true, _randbedingungAn = true;
    private KnotenNeu _knotenNeu;
    private string generatedId = "E";

    private ElementNeu _elementNeu;
    public MaterialNeu MaterialNeu;
    private KnotenlastNeu _knotenlastNeu;
    private LinienlastNeu _linienlastNeu;
    private ElementlastNeu _elementlastNeu;
    private RandbedingungNeu _randbedingungNeu;

    public bool IsKnoten, IsElement, IsKnotenlast, IsLinienlast, IsElementlast, IsRandbedingung;
    public KnotenKeys KnotenKeys;
    public ElementKeys ElementKeys;
    //public MaterialKeys MaterialKeys;
    //public WärmelastenKeys WärmelastenKeys;
    public ZeitintegrationNeu ZeitintegrationNeu;

    public WärmemodellVisualisieren(FeModell feModell)
    {
        Language = XmlLanguage.GetLanguage("de-DE");
        InitializeComponent();
        VisualWärmeModell.Children.Remove(Pilot);
        Show();
        VisualWärmeModell.Background = Brushes.Transparent;
        if(feModell == null)
        {
            _ = MessageBox.Show("WärmeModell nicht gefunden", "Wärmeberechnung");
            return;
        }
        _modell = feModell;

        try
        {
            Darstellung = new Darstellung(feModell, VisualWärmeModell);
            Darstellung.AlleElementeZeichnen();

            // mit Knoten, Element Ids, Lasten und Randbedingungen
            Darstellung.KnotenTexte();
            Darstellung.ElementTexte();
            Darstellung.KnotenlastenZeichnen();
            Darstellung.LinienlastenZeichnen();
            Darstellung.ElementlastenZeichnen();
            Darstellung.RandbedingungenZeichnen();
        }
        catch (ModellAusnahme e)
        {
            _ = MessageBox.Show(e.Message);
        }
    }

    // Modell berechnen
    private void OnBtnBerechnen_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (!_modell.Berechnet)
            {
                var modellBerechnung = new Berechnung(_modell);
                modellBerechnung.BerechneSystemMatrix();
                modellBerechnung.BerechneSystemVektor();
                modellBerechnung.LöseGleichungen();
                _modell.Berechnet = true;
            }
            var stationäreErgebnisse = new StationäreErgebnisseVisualisieren(_modell);
            stationäreErgebnisse.Show();
        }
        catch (BerechnungAusnahme e2)
        {
            _ = MessageBox.Show(e2.Message);
        }
    }

    private void InstationäreDaten(object sender, RoutedEventArgs e)
    {
        if (_modell.ZeitintegrationDaten && _modell != null)
        {
            var wärme = new InstationäreDatenAnzeigen(_modell);
            wärme.Show();
            _modell.ZeitintegrationBerechnet = false;
        }
        else
        {
            _ = MessageBox.Show("Modelldaten für Wärmeberechnung sind noch nicht spezifiziert", "Wärmeberechnung");
        }
    }

    private void InstationäreBerechnung(object sender, RoutedEventArgs e)
    {
        try
        {
            if (_modell.ZeitintegrationDaten && _modell != null)
            {
                Berechnung modellBerechnung = null;
                if (!_modell.Berechnet)
                {
                    modellBerechnung = new Berechnung(_modell);
                    modellBerechnung.BerechneSystemMatrix();
                    modellBerechnung.BerechneSystemVektor();
                    modellBerechnung.LöseGleichungen();
                    _modell.Berechnet = true;
                }

                modellBerechnung?.ZeitintegrationErsterOrdnung();
                _modell.ZeitintegrationBerechnet = true;
                _ = MessageBox.Show("Zeitintegration erfolgreich durchgeführt", "instationäre Wärmeberechnung");
            }
            else
            {
                _ = MessageBox.Show("Daten für Zeitintegration sind noch nicht spezifiziert", "Wärmeberechnung");
                const double tmax = 0;
                const double dt = 0;
                const double alfa = 0;
                if (_modell != null)
                {
                    _modell.Zeitintegration = new Wärmeberechnung.Modelldaten.Zeitintegration(tmax, dt, alfa) { VonStationär = false };
                    _modell.ZeitintegrationDaten = true;
                    var wärme = new InstationäreDatenAnzeigen(_modell);
                    wärme.Show();
                }

                _modell.ZeitintegrationBerechnet = false;
            }
        }
        catch (BerechnungAusnahme e2)
        {
            _ = MessageBox.Show(e2.Message);
        }
    }

    private void InstationäreModellzuständeVisualisieren(object sender, RoutedEventArgs e)
    {
        if (_modell.ZeitintegrationBerechnet && _modell != null)
        {
            var modellzuständeVisualisieren = new InstationäreModellzuständeVisualisieren(_modell);
            modellzuständeVisualisieren.Show();
        }
        else
        {
            _ = MessageBox.Show("Zeitintegration noch nicht ausgeführt!!", "Wärmeberechnung");
        }
    }

    private void TemperaturzeitverläufeVisualisieren(object sender, RoutedEventArgs e)
    {
        if (_modell.ZeitintegrationBerechnet && _modell != null)
        {
            var knotenzeitverläufeVisualisieren = new KnotenzeitverläufeVisualisieren(_modell);
            knotenzeitverläufeVisualisieren.Show();
        }
        else
        {
            _ = MessageBox.Show("Zeitintegration noch nicht ausgeführt!!", "Wärmeberechnung");
        }
    }

    // Modelldefinitionen neu definieren und vorhandene editieren
    private void MenuKnotenNeu(object sender, RoutedEventArgs e)
    {
        _knotenNeu = new KnotenNeu(_modell) { Topmost = true, Owner = (Window)Parent };
        KnotenKeys = new KnotenKeys(_modell) { Topmost = true, Owner = (Window)Parent };
        KnotenKeys.Show();
        _modell.Berechnet = false;
    }

    private void MenuKnotenGruppeNeu(object sender, RoutedEventArgs e)
    {
        _ = new KnotenGruppeNeu(_modell) { Topmost = true, Owner = (Window)Parent };
    }

    private void MenuKnotenNetzÄquidistant(object sender, RoutedEventArgs e)
    {
        _ = new KnotenNetzÄquidistant(_modell) { Topmost = true, Owner = (Window)Parent };
    }

    private void MenuKnotenNetzVariabel(object sender, RoutedEventArgs e)
    {
        _ = new KnotenNetzVariabel(_modell) { Topmost = true, Owner = (Window)Parent };
    }


    private void MenuElementNeu(object sender, RoutedEventArgs e)
    {
        IsElement = true;
        _elementNeu = new ElementNeu(_modell) { Topmost = true, Owner = (Window)Parent };
        ElementKeys = new ElementKeys(_modell) { Topmost = true, Owner = (Window)Parent };
        ElementKeys.Show();
        _modell.Berechnet = false;
    }

    private void MenuMaterialNeu(object sender, RoutedEventArgs e)
    {
        MaterialNeu = new MaterialNeu(_modell) { Topmost = true, Owner = (Window)Parent };
        //MaterialKeys = new MaterialKeys(_modell) { Topmost = true, Owner = (Window)Parent };
        //MaterialKeys.Show();
        _modell.Berechnet = false;
    }

    private void MenuKnotenlastNeu(object sender, RoutedEventArgs e)
    {
        IsKnotenlast = true;
        _knotenlastNeu = new KnotenlastNeu(_modell) { Topmost = true, Owner = (Window)Parent };
        //WärmelastenKeys = new WärmelastenKeys(_modell) { Topmost = true, Owner = (Window)Parent };
        //WärmelastenKeys.Show();
        _modell.Berechnet = false;
    }

    private void MenuLinienlastNeu(object sender, RoutedEventArgs e)
    {
        IsLinienlast = true;
        _linienlastNeu = new LinienlastNeu(_modell) { Topmost = true, Owner = (Window)Parent };
        //WärmelastenKeys = new WärmelastenKeys(_modell) { Topmost = true, Owner = (Window)Parent };
        //WärmelastenKeys.Show();
        _modell.Berechnet = false;
    }

    private void MenuElementlastNeu(object sender, RoutedEventArgs e)
    {
        IsElementlast = true;
        _elementlastNeu = new ElementlastNeu(_modell) { Topmost = true, Owner = (Window)Parent };
        //WärmelastenKeys = new WärmelastenKeys(_modell) { Topmost = true, Owner = (Window)Parent };
        //WärmelastenKeys.Show();
        _modell.Berechnet = false;
    }

    private void MenuZeitKnotenlastNeu(object sender, RoutedEventArgs e)
    {
        _ = new ZeitKnotentemperaturNeu(_modell);
        _modell.Berechnet = false;
    }

    private void MenuZeitElementlastNeu(object sender, RoutedEventArgs e)
    {
        _ = new ZeitElementtemperaturNeu(_modell);
        _modell.Berechnet = false;
    }

    private void MenuRandbedingungNeu(object sender, RoutedEventArgs e)
    {
        _ = new RandbedingungNeu(_modell);
        _modell.Berechnet = false;
    }

    private void MenuAnfangstemperaturNeu(object sender, RoutedEventArgs e)
    {
        _ = new ZeitAnfangstemperaturNeu(_modell);
        _modell.Berechnet = false;
    }

    private void MenuZeitRandbedingungNeu(object sender, RoutedEventArgs e)
    {
        _ = new ZeitRandtemperaturNeu(_modell);
        _modell.Berechnet = false;
    }

    // Modelldefinitionen darstellen
    private void OnBtnKnotenIDs_Click(object sender, RoutedEventArgs e)
    {
        if (!_knotenAn)
        {
            Darstellung.KnotenTexte();
            _knotenAn = true;
        }
        else
        {
            foreach (var id in Darstellung.KnotenIDs) VisualWärmeModell.Children.Remove(id);
            _knotenAn = false;
        }
    }

    private void OnBtnElementIDs_Click(object sender, RoutedEventArgs e)
    {
        if (!_elementeAn)
        {
            Darstellung.ElementTexte();
            _elementeAn = true;
        }
        else
        {
            foreach (var id in Darstellung.ElementIDs) VisualWärmeModell.Children.Remove(id);
            _elementeAn = false;
        }
    }

    private void OnBtnLasten_Click(object sender, RoutedEventArgs e)
    {
        if (!_lastenAn)
        {
            Darstellung.KnotenlastenZeichnen();
            Darstellung.LinienlastenZeichnen();
            Darstellung.ElementlastenZeichnen();
            _lastenAn = true;
        }
        else
        {
            foreach (var lastKnoten in Darstellung.LastKnoten) VisualWärmeModell.Children.Remove(lastKnoten);
            foreach (var lastLinie in Darstellung.LastLinien) VisualWärmeModell.Children.Remove(lastLinie);
            foreach (var lastElement in Darstellung.LastElemente) VisualWärmeModell.Children.Remove(lastElement);
            _lastenAn = false;
        }
    }

    private void OnBtnRandbedingung_Click(object sender, RoutedEventArgs e)
    {
        if (!_randbedingungAn)
        {
            Darstellung.RandbedingungenZeichnen();
            _randbedingungAn = true;
        }
        else
        {
            foreach (var randbedingung in Darstellung.RandKnoten)
                VisualWärmeModell.Children.Remove(randbedingung);
            _randbedingungAn = false;
        }
    }


    // KnotenNeu setzt Pilotpunkt
    // MouseDown rechte Taste "fängt" Pilotknoten, MouseMove folgt ihm, MouseUp setzt ihn neu
    private void Pilot_MouseDown(object sender, MouseButtonEventArgs e)
    {
        Pilot.CaptureMouse();
        _isDragging = true;
    }

    private void Pilot_MouseMove(object sender, MouseEventArgs e)
    {
        if (!_isDragging) return;
        var canvPosToWindow = VisualWärmeModell.TransformToAncestor(this).Transform(new Point(0, 0));

        if (sender is not Ellipse knoten) return;
        var upperlimit = canvPosToWindow.Y + knoten.Height / 2;
        var lowerlimit = canvPosToWindow.Y + VisualWärmeModell.ActualHeight - knoten.Height / 2;

        var leftlimit = canvPosToWindow.X + knoten.Width / 2;
        var rightlimit = canvPosToWindow.X + VisualWärmeModell.ActualWidth - knoten.Width / 2;


        var absmouseXpos = e.GetPosition(this).X;
        var absmouseYpos = e.GetPosition(this).Y;

        if (!(absmouseXpos > leftlimit) || !(absmouseXpos < rightlimit)
                                        || !(absmouseYpos > upperlimit) || !(absmouseYpos < lowerlimit)) return;

        _mittelpunkt = new Point(e.GetPosition(VisualWärmeModell).X, e.GetPosition(VisualWärmeModell).Y);

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
    }

    private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        MyPopup.IsOpen = false;
        _hitList.Clear();
        _hitTextBlock.Clear();
        VisualWärmeModell.Children.Remove(Pilot);
        var hitPoint = e.GetPosition(VisualWärmeModell);
        _hitArea = new EllipseGeometry(hitPoint, 1.0, 1.0);
        VisualTreeHelper.HitTest(VisualWärmeModell, null, HitTestCallBack,
            new GeometryHitTestParameters(_hitArea));

        // click auf Canvas weder Text noch Shape --> neuer Knoten wird mit Zeiger platziert und bewegt
        if (_hitList.Count == 0 && _hitTextBlock.Count == 0)
        {
            if (_knotenNeu == null) return;
            _mittelpunkt = new Point(e.GetPosition(VisualWärmeModell).X, e.GetPosition(VisualWärmeModell).Y);
            Canvas.SetLeft(Pilot, _mittelpunkt.X - Pilot.Width / 2);
            Canvas.SetTop(Pilot, _mittelpunkt.Y - Pilot.Height / 2);
            VisualWärmeModell.Children.Add(Pilot);
            IsKnoten = true;
            var koordinaten = Darstellung.TransformBildPunkt(_mittelpunkt);
            _knotenNeu.X.Text = koordinaten[0].ToString("N2", CultureInfo.CurrentCulture);
            _knotenNeu.Y.Text = koordinaten[1].ToString("N2", CultureInfo.CurrentCulture);
            MyPopup.IsOpen = false;
            return;
        }

        var sb = new StringBuilder();
        // click auf Shape Darstellungen
        foreach (var item in _hitList)
        {
            if (IsKnoten | item is not Path) return;
            if (item.Name == null) continue;

            // Elemente
            if (_modell.Elemente.TryGetValue(item.Name, out var abstractElement))
            {
                MyPopup.IsOpen = true;
                sb.Append("Element " + abstractElement.ElementId + ": ");
                switch (abstractElement)
                {
                    case Element2D2:
                    {
                        sb.Append("Knoten 1 = " + abstractElement.KnotenIds[0]);
                        sb.Append("Knoten 2 = " + abstractElement.KnotenIds[1]);
                        if (_modell.Material.TryGetValue(abstractElement.ElementMaterialId, out var material))
                            sb.Append("\nLeitfähigkeit = " + material.MaterialWerte[0].ToString("g3"));

                        break;
                    }
                    case Element2D3:
                    {
                        sb.Append("\nKnoten 1 = " + abstractElement.KnotenIds[0]);
                        sb.Append("\nKnoten 2 = " + abstractElement.KnotenIds[1]);
                        sb.Append("\nKnoten 3 = " + abstractElement.KnotenIds[2]);
                        if (_modell.Material.TryGetValue(abstractElement.ElementMaterialId, out var material))
                            sb.Append("\nLeitfähigkeit = " + material.MaterialWerte[0].ToString("g3"));

                        break;
                    }
                    case Element2D4:
                    {
                        sb.Append("Knoten 1 = " + abstractElement.KnotenIds[0]);
                        sb.Append("Knoten 2 = " + abstractElement.KnotenIds[1]);
                        sb.Append("Knoten 3 = " + abstractElement.KnotenIds[2]);
                        sb.Append("Knoten 4 = " + abstractElement.KnotenIds[3]);
                        if (_modell.Material.TryGetValue(abstractElement.ElementMaterialId, out var material))
                            sb.Append("\nLeitfähigkeit = " + material.MaterialWerte[0].ToString("g3"));

                        break;
                    }
                }

                sb.Append('\n');
            }

            // Lasten
            // Linienlasten
            else if (_modell.LinienLasten.TryGetValue(item.Name, out var last))
            {
                sb.Append("Linienlast = " + last.LastId);
                sb.Append("\nStartknoten " + last.StartKnotenId + "\t= " + last.Lastwerte[0].ToString("g2"));
                sb.Append("\nEndknoten " + last.EndKnotenId + "\t= " + last.Lastwerte[1].ToString("g2"));
                sb.Append('\n');
            }

            // Elementlasten
            else if (_modell.ElementLasten.TryGetValue(item.Name, out var elementLast))
            {
                _modell.Elemente.TryGetValue(elementLast.ElementId, out abstractElement);
                if (abstractElement == null) continue;
                switch (elementLast)
                {
                    case ElementLast3:
                        sb.Append("Elementlast = " + elementLast.LastId + "\n"
                                  + abstractElement.KnotenIds[0] + " = " + elementLast.Lastwerte[0].ToString("g2") +
                                  ", "
                                  + abstractElement.KnotenIds[1] + " = " + elementLast.Lastwerte[1].ToString("g2") +
                                  ", "
                                  + abstractElement.KnotenIds[2] + " = " + elementLast.Lastwerte[2].ToString("g2"));
                        sb.Append('\n');
                        break;
                    case ElementLast4:
                        sb.Append("\nElementlast = " + elementLast.LastId + "\n"
                                  + abstractElement.KnotenIds[0] + " = " + elementLast.Lastwerte[0].ToString("g2") +
                                  ", "
                                  + abstractElement.KnotenIds[1] + " = " + elementLast.Lastwerte[1].ToString("g2") +
                                  ", "
                                  + abstractElement.KnotenIds[2] + " = " + elementLast.Lastwerte[2].ToString("g2") +
                                  ", "
                                  + abstractElement.KnotenIds[3] + " = " + elementLast.Lastwerte[3].ToString("g2"));
                        sb.Append('\n');
                        break;
                }
            }

            // zeitabhängige Elementlasten
            else if (_modell.ZeitabhängigeElementLasten.TryGetValue(item.Name, out var zeitElementLast))
            {
                _modell.Elemente.TryGetValue(zeitElementLast.ElementId, out abstractElement);
                if (abstractElement == null) continue;

                sb.Append("zeitabhängige Elementlast = " + zeitElementLast.LastId + "\n"
                          + abstractElement.KnotenIds[0] + " = " + zeitElementLast.P[0].ToString("g2") +
                          ", "
                          + abstractElement.KnotenIds[1] + " = " + zeitElementLast.P[1].ToString("g2") +
                          ", "
                          + abstractElement.KnotenIds[2] + " = " + zeitElementLast.P[2].ToString("g2"));
                sb.Append('\n');
            }

            sb.Append('\n');
            MyPopupText.Text = sb.ToString();
        }

        // click auf Knotentext --> Eigenschaften eines vorhandenen Knotens werden interaktiv verändert
        foreach (var item in _hitTextBlock)
        {
            if (!_modell.Knoten.TryGetValue(item.Text, out var knoten)) continue;

            if (item.Text != knoten.Id) _ = MessageBox.Show("Knoten Id kann hier nicht verändert werden", "Knotentext");
            _knotenNeu = new KnotenNeu(_modell)
            {
                KnotenId = { Text = item.Text },
                X = { Text = knoten.Koordinaten[0].ToString("N2", CultureInfo.CurrentCulture) },
                Y = { Text = knoten.Koordinaten[1].ToString("N2", CultureInfo.CurrentCulture) }
            };

            _mittelpunkt = new Point(knoten.Koordinaten[0] * Darstellung.Auflösung + Darstellung.RandLinks,
                (-knoten.Koordinaten[1] + Darstellung.MaxY) * Darstellung.Auflösung + Darstellung.RandOben);
            Canvas.SetLeft(Pilot, _mittelpunkt.X - Pilot.Width / 2);
            Canvas.SetTop(Pilot, _mittelpunkt.Y - Pilot.Height / 2);
            VisualWärmeModell.Children.Add(Pilot);
            IsKnoten = true;
            MyPopup.IsOpen = false;
        }

        MyPopupText.Text = sb.ToString();

        // click auf Textdarstellungen - ausser Knotentexte (werden oben gesondert behandelt)
        foreach (var item in _hitTextBlock.Where(item => item != null))
        {
            // Textdarstellung ist ein Knoten
            if (_modell.Knoten.TryGetValue(item.Text, out var knoten))
            {
                IsKnoten = true;
                KnotenClick(knoten);
            }

            // Textdarstellung ist Element
            else if (_modell.Elemente.TryGetValue(item.Text, out var element))
            {
                // bei der Definition eines neuen Lagers ist Elementeingabe ungültig
                if (IsRandbedingung)
                {
                    _ = MessageBox.Show("Elementeingabe ungültig bei Definition eines neuen Lagers", "neue Linienlast");
                    return;
                }
                // bei der Definition einer neuen Knotenlast ist Elementeingabe ungültig
                else if (IsKnotenlast)
                {
                    _ = MessageBox.Show("Elementeingabe ungültig bei Definition einer neuen Knotenlast",
                        "neue Linienlast");
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
                //KnotenlastNeu(knotenlast);
            }

            // Textdarstellung ist eine Elementlast (Linienlast)
            else if (_modell.ElementLasten.TryGetValue(item.Text, out var linienlast))
            {
                //if (_linienlastNeu == null) LinienlastNeu(linienlast);
            }

            // Textdarstellung ist eine Punktlast
            else if (_modell.PunktLasten.TryGetValue(item.Text, out var punktlast))
            {
                //PunktlastNeu(punktlast);
            }
            // Textdarstellung ist eine Randbedingung
            else if (_modell.Randbedingungen.TryGetValue(item.Text, out var randbedingung))
            {
                RandbedingungNeu(randbedingung);
            }
        }
    }

    private void KnotenClick(Knoten knoten)
    {
        // Knotentexte angeklickt bei Definition eines neuen Elementes
        if (IsElement)
        {
            if (_elementNeu.Knoten1Id.Text == string.Empty)
            {
                generatedId += knoten.Id;
                _elementNeu.Knoten1Id.Text = knoten.Id; _elementNeu.ElementId.Text = generatedId;
            }
            else if (_elementNeu.Knoten2Id.Text == string.Empty)
            {
                generatedId += knoten.Id;
                _elementNeu.Knoten2Id.Text = knoten.Id; _elementNeu.ElementId.Text = generatedId;
            }
            else if (_elementNeu.Knoten3Id.Text == string.Empty)
            {
                generatedId += knoten.Id;
                _elementNeu.Knoten3Id.Text = knoten.Id; _elementNeu.ElementId.Text = generatedId;
            }
            else if (_elementNeu.Knoten4Id.Text == string.Empty)
            {
                generatedId += knoten.Id;
                _elementNeu.Knoten4Id.Text = knoten.Id; _elementNeu.ElementId.Text = generatedId;
            }
            else if (_elementNeu.Knoten5Id.Text == string.Empty) _elementNeu.Knoten5Id.Text = knoten.Id;
            else if (_elementNeu.Knoten6Id.Text == string.Empty) _elementNeu.Knoten6Id.Text = knoten.Id;
            else if (_elementNeu.Knoten7Id.Text == string.Empty) _elementNeu.Knoten7Id.Text = knoten.Id;
            else if (_elementNeu.Knoten8Id.Text == string.Empty)
            {
                generatedId += knoten.Id;
                _elementNeu.Knoten8Id.Text = knoten.Id; _elementNeu.ElementId.Text = generatedId;
            }

            _elementNeu.Show();
            _knotenNeu.Close();
            return;
        }

        // Knotentext angeklickt bei Definition einer neuen Knotenlast
        else if (IsKnotenlast)
        {
            _knotenlastNeu.KnotenId.Text = knoten.Id;
            _knotenlastNeu.KnotenlastId.Text = "KL_" + knoten.Id;
            _knotenlastNeu.Show();
            return;
        }
        // Knotentext angeklickt bei Definition einer neuen Elementlast
        else if (IsLinienlast)
        {
            _ = MessageBox.Show("Knoteneingabe ungültig bei Definition einer neuen Elementlast", "neue Linienlast");
            return;
        }
        // Knotentext angeklickt bei Definition einer neuen Elementlast
        else if (IsElementlast)
        {
            _ = MessageBox.Show("Knoteneingabe ungültig bei Definition einer neuen Elementlast", "neue Elementlast");
            return;
        }

        // Knotentext angeklickt bei Definition eines neuen Lagers
        else if (IsRandbedingung)
        {
            _randbedingungNeu.KnotenId.Text = knoten.Id;
            if (_randbedingungNeu.RandbedingungId.Text == string.Empty) _randbedingungNeu.RandbedingungId.Text = "L_" + knoten.Id;
            _randbedingungNeu.Show();
            return;
        }

        // Knotentext angeklickt, um vorhandenen Knoten zu editieren
        _knotenNeu = new KnotenNeu(_modell)
        {
            Topmost = true,
            Owner = (Window)Parent,
            KnotenId = { Text = knoten.Id },
            AnzahlDof = { Text = knoten.AnzahlKnotenfreiheitsgrade.ToString("N0", CultureInfo.CurrentCulture) },
            X = { Text = knoten.Koordinaten[0].ToString("N2", CultureInfo.CurrentCulture) },
            Y = { Text = knoten.Koordinaten[1].ToString("N2", CultureInfo.CurrentCulture) }
        };

        _mittelpunkt = new Point(knoten.Koordinaten[0] * Darstellung.Auflösung + Darstellung.RandLinks,
            (-knoten.Koordinaten[1] + Darstellung.MaxY) * Darstellung.Auflösung + Darstellung.RandOben);
        Canvas.SetLeft(Pilot, _mittelpunkt.X - Pilot.Width / 2);
        Canvas.SetTop(Pilot, _mittelpunkt.Y - Pilot.Height / 2);
        VisualWärmeModell.Children.Add(Pilot);
    }
    private void ElementNeu(AbstraktElement element)
    {
        // anderer Elementtext angeklickt beim Erstellen eines neuen Elementes
        // Material- und Querschnitteigenschaften werden übernommen
        if (IsElement)
        {
            _elementNeu.MaterialId.Text = element.ElementMaterialId;
            //_elementNeu.Show();
            IsElement = false;
            return;
        }

        // Elementtext angeklickt bei Definition einer neuen Linienlast
        if (IsLinienlast)
        {
            _linienlastNeu.LinienlastId.Text = "ll" + element.KnotenIds[0] + element.KnotenIds[1];
            _linienlastNeu.StartknotenId.Text = element.KnotenIds[0];
            _linienlastNeu.EndknotenId.Text = element.KnotenIds[1];
            _linienlastNeu.Show();
            return;
        }

        // Elementeigenschaften können editiert werden
        _elementNeu = element switch
        {
            Element2D2 => new ElementNeu(_modell)
            {
                Topmost = true,
                Owner = (Window)Parent,
                Element2D2 = { IsChecked = true },
                Element2D3 = { IsChecked = false },
                Element2D4 = { IsChecked = false },
                Element3D8 = { IsChecked = false },
                ElementId = { Text = element.ElementId },
                Knoten1Id = { Text = element.KnotenIds[0] },
                Knoten2Id = { Text = element.KnotenIds[1] },
                MaterialId = { Text = element.ElementMaterialId }
            },
            Element2D3 => new ElementNeu(_modell)
            {
                Topmost = true,
                Owner = (Window)Parent,
                Element2D2 = { IsChecked = false },
                Element2D3 = { IsChecked = true },
                Element2D4 = { IsChecked = false },
                Element3D8 = { IsChecked = false },
                ElementId = { Text = element.ElementId },
                Knoten1Id = { Text = element.KnotenIds[0] },
                Knoten2Id = { Text = element.KnotenIds[1] },
                Knoten3Id = { Text = element.KnotenIds[2] },
                MaterialId = { Text = element.ElementMaterialId }
            },
            Element2D4 => new ElementNeu(_modell)
            {
                Topmost = true,
                Owner = (Window)Parent,
                Element2D2 = { IsChecked = false },
                Element2D3 = { IsChecked = false },
                Element2D4 = { IsChecked = true },
                Element3D8 = { IsChecked = false },
                ElementId = { Text = element.ElementId },
                Knoten1Id = { Text = element.KnotenIds[0] },
                Knoten2Id = { Text = element.KnotenIds[1] },
                Knoten3Id = { Text = element.KnotenIds[2] },
                Knoten4Id = { Text = element.KnotenIds[3] },
                MaterialId = { Text = element.ElementMaterialId }
            },
            Element3D8 => new ElementNeu(_modell)
            {
                Topmost = true,
                Owner = (Window)Parent,
                Element2D2 = { IsChecked = false },
                Element2D3 = { IsChecked = false },
                Element2D4 = { IsChecked = false },
                Element3D8 = { IsChecked = true },
                ElementId = { Text = element.ElementId },
                Knoten1Id = { Text = element.KnotenIds[0] },
                Knoten2Id = { Text = element.KnotenIds[1] },
                Knoten3Id = { Text = element.KnotenIds[2] },
                Knoten4Id = { Text = element.KnotenIds[3] },
                Knoten5Id = { Text = element.KnotenIds[4] },
                Knoten6Id = { Text = element.KnotenIds[5] },
                Knoten7Id = { Text = element.KnotenIds[6] },
                Knoten8Id = { Text = element.KnotenIds[7] },
                MaterialId = { Text = element.ElementMaterialId }
            },
            _ => _elementNeu
        };
        IsElement = true;
    }

    //private void KnotenlastNeu(AbstraktLast knotenlast)
    //{
    //    _knotenlastNeu = new KnotenlastNeu(_modell)
    //    {
    //        Topmost = true,
    //        Owner = (Window)Parent,
    //        LastId = { Text = knotenlast.LastId },
    //        KnotenId = { Text = knotenlast.KnotenId.ToString(CultureInfo.CurrentCulture) },
    //        Px = { Text = knotenlast.Lastwerte[0].ToString(CultureInfo.CurrentCulture) },
    //        Py = { Text = knotenlast.Lastwerte[1].ToString(CultureInfo.CurrentCulture) }
    //    };
    //    if (knotenlast.Lastwerte.Length > 2)
    //        _knotenlastNeu.M.Text = knotenlast.Lastwerte[2].ToString(CultureInfo.CurrentCulture);
    //    IsKnotenlast = true;
    //}

    //private void PunktlastNeu(AbstraktElementLast punktLast)
    //{
    //    var punktlast = (PunktLast)punktLast;
    //    _punktlastNeu = new PunktlastNeu(_modell)
    //    {
    //        Topmost = true,
    //        Owner = (Window)Parent,
    //        LastId = { Text = punktlast.LastId },
    //        ElementId = { Text = punktlast.ElementId.ToString(CultureInfo.CurrentCulture) },
    //        Px = { Text = punktlast.Lastwerte[0].ToString(CultureInfo.CurrentCulture) },
    //        Py = { Text = punktlast.Lastwerte[1].ToString(CultureInfo.CurrentCulture) },
    //        Offset = { Text = punktlast.Offset.ToString(CultureInfo.CurrentCulture) }
    //    };
    //    IsPunktlast = true;
    //}

    //private void LinienlastNeu(AbstraktElementLast linienlast)
    //{
    //    _linienlastNeu = new LinienlastNeu(_modell)
    //    {
    //        Topmost = true,
    //        Owner = (Window)Parent,
    //        LastId = { Text = linienlast.LastId },
    //        ElementId = { Text = linienlast.ElementId.ToString(CultureInfo.CurrentCulture) },
    //        Pxa = { Text = linienlast.Lastwerte[0].ToString(CultureInfo.CurrentCulture) },
    //        Pya = { Text = linienlast.Lastwerte[1].ToString(CultureInfo.CurrentCulture) },
    //        Pxb = { Text = linienlast.Lastwerte[2].ToString(CultureInfo.CurrentCulture) },
    //        Pyb = { Text = linienlast.Lastwerte[3].ToString(CultureInfo.CurrentCulture) },
    //        InElement = { IsChecked = linienlast.InElementKoordinatenSystem }
    //    };
    //    IsLinienlast = true;
    //}

    private void RandbedingungNeu(AbstraktRandbedingung randbedingung)
    {
        _randbedingungNeu = new RandbedingungNeu(_modell)
        {
            Topmost = true,
            Owner = (Window)Parent,
            RandbedingungId = { Text = randbedingung.RandbedingungId },
            KnotenId = { Text = randbedingung.KnotenId.ToString(CultureInfo.CurrentCulture) }
            //Xfest = { IsChecked = (randbedingung.Typ == 1) | (randbedingung.Typ == 3) | (randbedingung.Typ == 7) },
            //Yfest = { IsChecked = (randbedingung.Typ == 2) | (randbedingung.Typ == 3) | (randbedingung.Typ == 7) },
            //Rfest = { IsChecked = (randbedingung.Typ == 4) | (randbedingung.Typ == 7) }
        };
        //if ((bool)_randbedingungNeu.Xfest.IsChecked) _randbedingungNeu.VorX.Text = randbedingung.Vordefiniert[0].ToString("0.00");
        //if ((bool)_randbedingungNeu.Yfest.IsChecked) _randbedingungNeu.VorY.Text = randbedingung.Vordefiniert[1].ToString("0.00");
        //if ((bool)_randbedingungNeu.Rfest.IsChecked) _randbedingungNeu.VorRot.Text = randbedingung.Vordefiniert[2].ToString("0.00");
        IsRandbedingung = true;
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


    //else if (_modell.Elemente.TryGetValue(item.Text, out var element))
    //        {
    //            switch (element)
    //            {
    //                case Element2D2:
    //                    _ = new ElementNeu(_modell)
    //                    {
    //                        Element2D2 = { IsChecked = true },
    //                        ElementId = { Text = element.ElementId },
    //                        Knoten1Id = { Text = element.KnotenIds[0] },
    //                        Knoten2Id = { Text = element.KnotenIds[1] },
    //                        MaterialId = { Text = element.ElementMaterialId }
    //                    };
    //                    break;
    //                case Element2D3:
    //                    _ = new ElementNeu(_modell)
    //                    {
    //                        Element2D3 = { IsChecked = true },
    //                        ElementId = { Text = element.ElementId },
    //                        Knoten1Id = { Text = element.KnotenIds[0] },
    //                        Knoten2Id = { Text = element.KnotenIds[1] },
    //                        Knoten3Id = { Text = element.KnotenIds[2] },
    //                        MaterialId = { Text = element.ElementMaterialId }
    //                    };
    //                    break;
    //                case Element2D4:
    //                    _ = new ElementNeu(_modell)
    //                    {
    //                        Element2D4 = { IsChecked = true },
    //                        ElementId = { Text = element.ElementId },
    //                        Knoten1Id = { Text = element.KnotenIds[0] },
    //                        Knoten2Id = { Text = element.KnotenIds[1] },
    //                        Knoten3Id = { Text = element.KnotenIds[2] },
    //                        Knoten4Id = { Text = element.KnotenIds[3] },
    //                        MaterialId = { Text = element.ElementMaterialId }
    //                    };
    //                    break;
    //                case Element3D8:
    //                    _ = new ElementNeu(_modell)
    //                    {
    //                        Element3D8 = { IsChecked = true },
    //                        ElementId = { Text = element.ElementId },
    //                        Knoten1Id = { Text = element.KnotenIds[0] },
    //                        Knoten2Id = { Text = element.KnotenIds[1] },
    //                        Knoten3Id = { Text = element.KnotenIds[2] },
    //                        Knoten4Id = { Text = element.KnotenIds[3] },
    //                        Knoten5Id = { Text = element.KnotenIds[4] },
    //                        Knoten6Id = { Text = element.KnotenIds[5] },
    //                        Knoten7Id = { Text = element.KnotenIds[6] },
    //                        Knoten8Id = { Text = element.KnotenIds[7] },
    //                        MaterialId = { Text = element.ElementMaterialId }
    //                    };
    //                    break;
    //            }
    //        }

    //        // Textdarstellung ist Knotenlast
    //        else if (_modell.Lasten.TryGetValue(item.Uid, out var knotenlast))
    //        {
    //            _ = new RandbdingungNeu(_modell)
    //            {
    //                RandbedingungId = { Text = knotenlast.LastId },
    //                KnotenId = { Text = knotenlast.KnotenId },
    //                Temperatur = { Text = knotenlast.Lastwerte[0].ToString("g3") }
    //            };
    //        }
    //        // Textdarstellung ist zeitabhängige Knotenlast
    //        else if (_modell.ZeitabhängigeKnotenLasten.TryGetValue(item.Uid, out var zeitKnotenlast))
    //        {
    //            var zeitKnotentemperatur = new ZeitKnotentemperaturNeu(_modell)
    //            {
    //                LastId = { Text = zeitKnotenlast.LastId },
    //                KnotenId = { Text = zeitKnotenlast.KnotenId }
    //            };
    //            switch (zeitKnotenlast.VariationsTyp)
    //            {
    //                case 0:
    //                    zeitKnotentemperatur.Datei.IsChecked = true;
    //                    break;
    //                case 1:
    //                    zeitKnotentemperatur.Konstant.Text = zeitKnotenlast.KonstanteTemperatur.ToString("g3");
    //                    break;
    //                case 2:
    //                    zeitKnotentemperatur.Konstant.Text = zeitKnotenlast.KonstanteTemperatur.ToString("g3");
    //                    zeitKnotentemperatur.Amplitude.Text = zeitKnotenlast.Amplitude.ToString("g3");
    //                    zeitKnotentemperatur.Frequenz.Text = zeitKnotenlast.Frequenz.ToString("g3");
    //                    zeitKnotentemperatur.Winkel.Text = zeitKnotenlast.PhasenWinkel.ToString("g3");
    //                    break;
    //                case 3:
    //                    var intervall = zeitKnotenlast.Intervall;
    //                    for (var i = 0; i < intervall.Length; i += 2)
    //                    {
    //                        sb.Append(intervall[i].ToString("N0"));
    //                        sb.Append(';');
    //                        sb.Append(intervall[i + 1].ToString("N0"));
    //                        sb.Append(' ');
    //                    }

    //                    zeitKnotentemperatur.Linear.Text = sb.ToString();
    //                    break;
    //            }
    //        }
    //        // Textdarstellung ist Elementlast
    //        else if (_modell.ElementLasten.TryGetValue(item.Uid, out var elementLast))
    //        {
    //            var elementlast = new ElementlastNeu(_modell)
    //            {
    //                ElementlastId = { Text = elementLast.LastId },
    //                ElementId = { Text = elementLast.ElementId },
    //                Knoten1 = { Text = elementLast.Lastwerte[0].ToString(CultureInfo.CurrentCulture) },
    //                Knoten2 = { Text = elementLast.Lastwerte[1].ToString(CultureInfo.CurrentCulture) }
    //            };
    //            for (var i = 0; i < elementLast.Lastwerte.Length; i++)
    //                switch (i)
    //                {
    //                    case 3:
    //                        elementlast.Knoten3.Text = elementLast.Lastwerte[2].ToString(CultureInfo.CurrentCulture);
    //                        break;
    //                    case 4:
    //                        elementlast.Knoten3.Text = elementLast.Lastwerte[2].ToString(CultureInfo.CurrentCulture);
    //                        elementlast.Knoten4.Text = elementLast.Lastwerte[3].ToString(CultureInfo.CurrentCulture);
    //                        break;
    //                    case 5:
    //                        elementlast.Knoten3.Text = elementLast.Lastwerte[2].ToString(CultureInfo.CurrentCulture);
    //                        elementlast.Knoten4.Text = elementLast.Lastwerte[3].ToString(CultureInfo.CurrentCulture);
    //                        elementlast.Knoten5.Text = elementLast.Lastwerte[4].ToString(CultureInfo.CurrentCulture);
    //                        break;
    //                    case 6:
    //                        elementlast.Knoten3.Text = elementLast.Lastwerte[2].ToString(CultureInfo.CurrentCulture);
    //                        elementlast.Knoten4.Text = elementLast.Lastwerte[3].ToString(CultureInfo.CurrentCulture);
    //                        elementlast.Knoten5.Text = elementLast.Lastwerte[4].ToString(CultureInfo.CurrentCulture);
    //                        elementlast.Knoten6.Text = elementLast.Lastwerte[5].ToString(CultureInfo.CurrentCulture);
    //                        break;
    //                    case 7:
    //                        elementlast.Knoten3.Text = elementLast.Lastwerte[2].ToString(CultureInfo.CurrentCulture);
    //                        elementlast.Knoten4.Text = elementLast.Lastwerte[3].ToString(CultureInfo.CurrentCulture);
    //                        elementlast.Knoten5.Text = elementLast.Lastwerte[4].ToString(CultureInfo.CurrentCulture);
    //                        elementlast.Knoten6.Text = elementLast.Lastwerte[5].ToString(CultureInfo.CurrentCulture);
    //                        elementlast.Knoten7.Text = elementLast.Lastwerte[6].ToString(CultureInfo.CurrentCulture);
    //                        break;
    //                    case 8:
    //                        elementlast.Knoten3.Text = elementLast.Lastwerte[2].ToString(CultureInfo.CurrentCulture);
    //                        elementlast.Knoten4.Text = elementLast.Lastwerte[3].ToString(CultureInfo.CurrentCulture);
    //                        elementlast.Knoten5.Text = elementLast.Lastwerte[4].ToString(CultureInfo.CurrentCulture);
    //                        elementlast.Knoten6.Text = elementLast.Lastwerte[5].ToString(CultureInfo.CurrentCulture);
    //                        elementlast.Knoten7.Text = elementLast.Lastwerte[6].ToString(CultureInfo.CurrentCulture);
    //                        elementlast.Knoten8.Text = elementLast.Lastwerte[7].ToString(CultureInfo.CurrentCulture);
    //                        break;
    //                }
    //        }
    //        // Textdarstellung ist zeitabhängige Elementlast
    //        else if (_modell.ZeitabhängigeElementLasten.TryGetValue(item.Uid, out var zeitElementlast))
    //        {
    //            var elementlast = new ZeitElementtemperaturNeu(_modell)
    //            {
    //                LastId = { Text = zeitElementlast.LastId },
    //                ElementId = { Text = zeitElementlast.ElementId },
    //                P0 = { Text = zeitElementlast.P[0].ToString("G2") },
    //                P1 = { Text = zeitElementlast.P[1].ToString("G2") }
    //            };
    //            switch (zeitElementlast.P.Length)
    //            {
    //                case 3:
    //                    elementlast.P2.Text = zeitElementlast.P[2].ToString("G2");
    //                    break;
    //                case 4:
    //                    elementlast.P2.Text = zeitElementlast.P[2].ToString("G2");
    //                    elementlast.P3.Text = zeitElementlast.P[3].ToString("G2");
    //                    break;
    //            }
    //        }

    //        // Textdarstellung ist Randtemperatur
    //        else if (_modell.Randbedingungen.TryGetValue(item.Uid, out var randbedingung))
    //        {
    //            _ = new RandbdingungNeu(_modell)
    //            {
    //                RandbedingungId = { Text = randbedingung.RandbedingungId },
    //                KnotenId = { Text = randbedingung.KnotenId },
    //                Temperatur = { Text = randbedingung.Vordefiniert[0].ToString("g3") }
    //            };
    //        }
    //        // Textdarstellung ist zeitabhängige Randtemperatur
    //        else if (_modell.ZeitabhängigeRandbedingung.TryGetValue(item.Uid, out var zeitRandbedingung))
    //        {
    //            var rand = new ZeitRandtemperaturNeu(_modell)
    //            {
    //                RandbedingungId = { Text = zeitRandbedingung.RandbedingungId },
    //                KnotenId = { Text = zeitRandbedingung.KnotenId }
    //            };
    //            switch (zeitRandbedingung.VariationsTyp)
    //            {
    //                case 0:
    //                    rand.Datei.IsChecked = true;
    //                    break;
    //                case 1:
    //                    rand.Konstant.Text = zeitRandbedingung.KonstanteTemperatur.ToString("g3");
    //                    break;
    //                case 2:
    //                    rand.Konstant.Text = zeitRandbedingung.KonstanteTemperatur.ToString("g3");
    //                    rand.Amplitude.Text = zeitRandbedingung.Amplitude.ToString("g3");
    //                    rand.Frequenz.Text = zeitRandbedingung.Frequenz.ToString("g3");
    //                    rand.Winkel.Text = zeitRandbedingung.PhasenWinkel.ToString("g3");
    //                    break;
    //                case 3:
    //                    var intervall = zeitRandbedingung.Intervall;
    //                    for (var i = 0; i < intervall.Length; i += 2)
    //                    {
    //                        sb.Append(intervall[i].ToString("N0"));
    //                        sb.Append(';');
    //                        sb.Append(intervall[i + 1].ToString("N0"));
    //                        sb.Append(' ');
    //                    }

    //                    rand.Linear.Text = sb.ToString();
    //                    break;
    //            }
    //}
    //}
    //}
    //private HitTestResultBehavior HitTestCallBack(HitTestResult result)
    //{
    //    var intersectionDetail = ((GeometryHitTestResult)result).IntersectionDetail;

    //    switch (intersectionDetail)
    //    {
    //        case IntersectionDetail.Empty:
    //            return HitTestResultBehavior.Continue;
    //        case IntersectionDetail.FullyContains:
    //            switch (result.VisualHit)
    //            {
    //                case Shape hit:
    //                    _hitList.Add(hit);
    //                    break;
    //                case TextBlock hit:
    //                    _hitTextBlock.Add(hit);
    //                    break;
    //            }

    //            return HitTestResultBehavior.Continue;
    //        case IntersectionDetail.FullyInside:
    //            return HitTestResultBehavior.Continue;
    //        case IntersectionDetail.Intersects:
    //            switch (result.VisualHit)
    //            {
    //                case Shape hit:
    //                    _hitList.Add(hit);
    //                    break;
    //            }

    //            return HitTestResultBehavior.Continue;
    //        case IntersectionDetail.NotCalculated:
    //            return HitTestResultBehavior.Continue;
    //        default:
    //            return HitTestResultBehavior.Stop;
    //    }
    //}
}