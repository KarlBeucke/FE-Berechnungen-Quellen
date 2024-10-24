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

namespace FE_Berechnungen.Wärmeberechnung.ModelldatenAnzeigen;

public partial class WärmemodellVisualisieren
{
    private EllipseGeometry hitArea;
    //alle gefundenen "Shapes" werden in dieser Liste gesammelt
    private readonly List<Shape> hitList = [];
    //alle gefundenen "TextBlocks" werden in dieser Liste gesammelt
    private readonly List<TextBlock> hitTextBlock = [];
    private bool isDragging;
    private Point mittelpunkt;
    
    private readonly FeModell _modell;
    public readonly Darstellung Darstellung;
    private bool knotenAn = true, elementeAn = true, lastenAn = true, randbedingungAn = true;
    private KnotenNeu _knotenNeu;
    private ElementNeu _elementNeu;
    private KnotenlastNeu _knotenlastNeu;
    private LinienlastNeu _linienlastNeu;
    private ElementlastNeu _elementlastNeu;

    public bool IsKnoten, IsElement, IsKnotenlast, IsLinienlast, IsElementlast, IsLager;
    public KnotenKeys KnotenKeys;
    public ElementKeys ElementKeys;
    public MaterialKeys MaterialKeys;
    public WärmelastenKeys WärmelastenKeys;
    public ZeitintegrationNeu ZeitintegrationNeu;

    public WärmemodellVisualisieren(FeModell feModell)
    {
        Language = XmlLanguage.GetLanguage("de-DE");
        InitializeComponent();
        VisualWärmeModell.Children.Remove(Pilot);
        Show();
        VisualWärmeModell.Background = Brushes.Transparent;
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
        if (_modell != null)
        {
            var modellBerechnung = new Berechnung(_modell);
            modellBerechnung.BerechneSystemMatrix();
            modellBerechnung.BerechneSystemVektor();
            modellBerechnung.LöseGleichungen();
            _modell.Berechnet = true;

            var stationäreErgebnisse = new StationäreErgebnisseVisualisieren(_modell);
            stationäreErgebnisse.Show();
        }
        else
        {
            _ = MessageBox.Show("WärmeModelldaten müssen zuerst eingelesen werden", "Wärmeberechnung");
        }
    }

    private void InstationäreDaten(object sender, RoutedEventArgs e)
    {
        if (_modell != null)
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
        if (_modell.ZeitintegrationDaten && _modell != null)
        {
            Berechnung modellBerechnung=null;
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
            var knotenzeitverläufeVisualisieren =
                new KnotenzeitverläufeVisualisieren(_modell);
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
        _ = new MaterialNeu(_modell) { Topmost = true, Owner = (Window)Parent };
        MaterialKeys = new MaterialKeys(_modell) { Topmost = true, Owner = (Window)Parent };
        MaterialKeys.Show();
        _modell.Berechnet = false;
    }

    private void MenuKnotenlastNeu(object sender, RoutedEventArgs e)
    {
        IsKnotenlast = true;
        _knotenlastNeu = new KnotenlastNeu(_modell) { Topmost = true, Owner = (Window)Parent };
        WärmelastenKeys = new WärmelastenKeys(_modell) { Topmost = true, Owner = (Window)Parent };
        WärmelastenKeys.Show();
        _modell.Berechnet = false;
    }

    private void MenuLinienlastNeu(object sender, RoutedEventArgs e)
    {
        IsLinienlast = true;
        _linienlastNeu = new LinienlastNeu(_modell) { Topmost = true, Owner = (Window)Parent };
        WärmelastenKeys = new WärmelastenKeys(_modell) { Topmost = true, Owner = (Window)Parent };
        WärmelastenKeys.Show();
        _modell.Berechnet = false;
    }

    private void MenuElementlastNeu(object sender, RoutedEventArgs e)
    {
        IsElementlast = true;
        _elementlastNeu = new ElementlastNeu(_modell) { Topmost = true, Owner = (Window)Parent };
        WärmelastenKeys = new WärmelastenKeys(_modell) { Topmost = true, Owner = (Window)Parent };
        WärmelastenKeys.Show();
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
        _ = new RandbdingungNeu(_modell);
        _modell.Berechnet = false;
    }

    private void MenuAnfangstemperaturNeu(object sender, RoutedEventArgs e)
    {
        _ = new ZeitAnfangstemperaturNeu(_modell);
        _modell.Berechnet = false;
    }

    private void OnBtnRandbedingungNeu_Click(object sender, RoutedEventArgs e)
    {

    }

    private void MenuZeitRandbedingungNeu(object sender, RoutedEventArgs e)
    {
        _ = new ZeitRandtemperaturNeu(_modell);
        _modell.Berechnet = false;
    }

    // Modelldefinitionen darstellen
    private void OnBtnKnotenIDs_Click(object sender, RoutedEventArgs e)
    {
        if (!knotenAn)
        {
            Darstellung.KnotenTexte();
            knotenAn = true;
        }
        else
        {
            foreach (var id in Darstellung.KnotenIDs) VisualWärmeModell.Children.Remove(id);
            knotenAn = false;
        }
    }

    private void OnBtnElementIDs_Click(object sender, RoutedEventArgs e)
    {
        if (!elementeAn)
        {
            Darstellung.ElementTexte();
            elementeAn = true;
        }
        else
        {
            foreach (var id in Darstellung.ElementIDs) VisualWärmeModell.Children.Remove(id);
            elementeAn = false;
        }
    }

    private void OnBtnLasten_Click(object sender, RoutedEventArgs e)
    {
        if (!lastenAn)
        {
            Darstellung.KnotenlastenZeichnen();
            Darstellung.LinienlastenZeichnen();
            Darstellung.ElementlastenZeichnen();
            lastenAn = true;
        }
        else
        {
            foreach (var lastKnoten in Darstellung.LastKnoten) VisualWärmeModell.Children.Remove(lastKnoten);
            foreach (var lastLinie in Darstellung.LastLinien) VisualWärmeModell.Children.Remove(lastLinie);
            foreach (var lastElement in Darstellung.LastElemente) VisualWärmeModell.Children.Remove(lastElement);
            lastenAn = false;
        }
    }

    private void OnBtnRandbedingung_Click(object sender, RoutedEventArgs e)
    {
        if (!randbedingungAn)
        {
            Darstellung.RandbedingungenZeichnen();
            randbedingungAn = true;
        }
        else
        {
            foreach (var randbedingung in Darstellung.RandKnoten)
                VisualWärmeModell.Children.Remove(randbedingung);
            randbedingungAn = false;
        }
    }


    // KnotenNeu setzt Pilotpunkt
    // MouseDown rechte Taste "fängt" Pilotknoten, MouseMove folgt ihm, MouseUp setzt ihn neu
    private void Pilot_MouseDown(object sender, MouseButtonEventArgs e)
    {
        Pilot.CaptureMouse();
        isDragging = true;
    }

    private void Pilot_MouseMove(object sender, MouseEventArgs e)
    {
        if (!isDragging) return;
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

        mittelpunkt = new Point(e.GetPosition(VisualWärmeModell).X, e.GetPosition(VisualWärmeModell).Y);

        Canvas.SetLeft(knoten, mittelpunkt.X - Pilot.Width / 2);
        Canvas.SetTop(knoten, mittelpunkt.Y - Pilot.Height / 2);

        var koordinaten = Darstellung.TransformBildPunkt(mittelpunkt);
        _knotenNeu.X.Text = koordinaten[0].ToString("N2", CultureInfo.CurrentCulture);
        _knotenNeu.Y.Text = koordinaten[1].ToString("N2", CultureInfo.CurrentCulture);
    }

    private void Pilot_MouseUp(object sender, MouseButtonEventArgs e)
    {
        Pilot.ReleaseMouseCapture();
        isDragging = false;
    }

    private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        MyPopup.IsOpen = false;
        hitList.Clear();
        hitTextBlock.Clear();
        var hitPoint = e.GetPosition(VisualWärmeModell);
        hitArea = new EllipseGeometry(hitPoint, 1.0, 1.0);
        VisualTreeHelper.HitTest(VisualWärmeModell, null, HitTestCallBack,
            new GeometryHitTestParameters(hitArea));

        // click auf Canvas weder Text noch Shape --> neuer Knoten wird mit Zeiger plaziert und bewegt
        if (hitList.Count == 0 && hitTextBlock.Count == 0)
        {
            if (_knotenNeu == null) return;
            mittelpunkt = new Point(e.GetPosition(VisualWärmeModell).X, e.GetPosition(VisualWärmeModell).Y);
            Canvas.SetLeft(Pilot, mittelpunkt.X - Pilot.Width / 2);
            Canvas.SetTop(Pilot, mittelpunkt.Y - Pilot.Height / 2);
            VisualWärmeModell.Children.Add(Pilot);
            IsKnoten = true;
            var koordinaten = Darstellung.TransformBildPunkt(mittelpunkt);
            _knotenNeu.X.Text = koordinaten[0].ToString("N2", CultureInfo.CurrentCulture);
            _knotenNeu.Y.Text = koordinaten[1].ToString("N2", CultureInfo.CurrentCulture);
            MyPopup.IsOpen = false;
            return;
        }

        var sb = new StringBuilder();
        // click auf Shape Darstellungen
        foreach (var item in hitList)
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
        foreach (var item in hitTextBlock)
        {
            if (!_modell.Knoten.TryGetValue(item.Text, out var knoten)) continue;
            
            if (item.Text != knoten.Id) _ = MessageBox.Show("Knoten Id kann hier nicht verändert werden", "Knotentext");
            _knotenNeu = new KnotenNeu(_modell)
            {
                KnotenId = { Text = item.Text },
                X = { Text = knoten.Koordinaten[0].ToString("N2", CultureInfo.CurrentCulture) },
                Y = { Text = knoten.Koordinaten[1].ToString("N2", CultureInfo.CurrentCulture) }
            };

            mittelpunkt = new Point(knoten.Koordinaten[0] * Darstellung.Auflösung + Darstellung.RandLinks,
                (-knoten.Koordinaten[1] + Darstellung.MaxY) * Darstellung.Auflösung + Darstellung.RandOben);
            Canvas.SetLeft(Pilot, mittelpunkt.X - Pilot.Width / 2);
            Canvas.SetTop(Pilot, mittelpunkt.Y - Pilot.Height / 2);
            VisualWärmeModell.Children.Add(Pilot);
            IsKnoten = true;
            MyPopup.IsOpen = false;
        }

        MyPopupText.Text = sb.ToString();

        // click auf Textdarstellungen - ausser Knotentexte (werden oben gesondert behandelt)
        if (IsKnoten) return;
        foreach (var item in hitTextBlock.Where(item => item != null))
            // Textdarstellung ist Element
            if (_modell.Elemente.TryGetValue(item.Text, out var element))
            {
                switch (element)
                {
                    case Element2D2:
                        _ = new ElementNeu(_modell)
                        {
                            Element2D2 = { IsChecked = true },
                            ElementId = { Text = element.ElementId },
                            Knoten1Id = { Text = element.KnotenIds[0] },
                            Knoten2Id = { Text = element.KnotenIds[1] },
                            MaterialId = { Text = element.ElementMaterialId }
                        };
                        break;
                    case Element2D3:
                        _ = new ElementNeu(_modell)
                        {
                            Element2D3 = { IsChecked = true },
                            ElementId = { Text = element.ElementId },
                            Knoten1Id = { Text = element.KnotenIds[0] },
                            Knoten2Id = { Text = element.KnotenIds[1] },
                            Knoten3Id = { Text = element.KnotenIds[2] },
                            MaterialId = { Text = element.ElementMaterialId }
                        };
                        break;
                    case Element2D4:
                        _ = new ElementNeu(_modell)
                        {
                            Element2D4= { IsChecked = true },
                            ElementId = { Text = element.ElementId },
                            Knoten1Id = { Text = element.KnotenIds[0] },
                            Knoten2Id = { Text = element.KnotenIds[1] },
                            Knoten3Id = { Text = element.KnotenIds[2] },
                            Knoten4Id = { Text = element.KnotenIds[3] },
                            MaterialId = { Text = element.ElementMaterialId }
                        };
                        break;
                    case Element3D8:
                        _ = new ElementNeu(_modell)
                        {
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
                        };
                        break;
                }
            }

            // Textdarstellung ist Knotenlast
            else if (_modell.Lasten.TryGetValue(item.Uid, out var knotenlast))
            {
                _ = new RandbdingungNeu(_modell)
                {
                    RandbedingungId = { Text = knotenlast.LastId },
                    KnotenId = { Text = knotenlast.KnotenId },
                    Temperatur = { Text = knotenlast.Lastwerte[0].ToString("g3") }
                };
            }
            // Textdarstellung ist zeitabhängige Knotenlast
            else if (_modell.ZeitabhängigeKnotenLasten.TryGetValue(item.Uid, out var zeitKnotenlast))
            {
                var zeitKnotentemperatur = new ZeitKnotentemperaturNeu(_modell)
                {
                    LastId = { Text = zeitKnotenlast.LastId },
                    KnotenId = { Text = zeitKnotenlast.KnotenId }
                };
                switch (zeitKnotenlast.VariationsTyp)
                {
                    case 0:
                        zeitKnotentemperatur.Datei.IsChecked = true;
                        break;
                    case 1:
                        zeitKnotentemperatur.Konstant.Text = zeitKnotenlast.KonstanteTemperatur.ToString("g3");
                        break;
                    case 2:
                        zeitKnotentemperatur.Konstant.Text = zeitKnotenlast.KonstanteTemperatur.ToString("g3");
                        zeitKnotentemperatur.Amplitude.Text = zeitKnotenlast.Amplitude.ToString("g3");
                        zeitKnotentemperatur.Frequenz.Text = zeitKnotenlast.Frequenz.ToString("g3");
                        zeitKnotentemperatur.Winkel.Text = zeitKnotenlast.PhasenWinkel.ToString("g3");
                        break;
                    case 3:
                        var intervall = zeitKnotenlast.Intervall;
                        for (var i = 0; i < intervall.Length; i += 2)
                        {
                            sb.Append(intervall[i].ToString("N0"));
                            sb.Append(';');
                            sb.Append(intervall[i + 1].ToString("N0"));
                            sb.Append(' ');
                        }

                        zeitKnotentemperatur.Linear.Text = sb.ToString();
                        break;
                }
            }
            // Textdarstellung ist Elementlast
            else if (_modell.ElementLasten.TryGetValue(item.Uid, out var elementLast))
            {
                var elementlast = new ElementlastNeu(_modell)
                {
                    ElementlastId = { Text = elementLast.LastId },
                    ElementId = { Text = elementLast.ElementId },
                    Knoten1 = { Text = elementLast.Lastwerte[0].ToString(CultureInfo.CurrentCulture) },
                    Knoten2 = { Text = elementLast.Lastwerte[1].ToString(CultureInfo.CurrentCulture) }
                };
                for (var i = 0; i < elementLast.Lastwerte.Length; i++)
                    switch (i)
                    {
                        case 3:
                            elementlast.Knoten3.Text = elementLast.Lastwerte[2].ToString(CultureInfo.CurrentCulture);
                            break;
                        case 4:
                            elementlast.Knoten3.Text = elementLast.Lastwerte[2].ToString(CultureInfo.CurrentCulture);
                            elementlast.Knoten4.Text = elementLast.Lastwerte[3].ToString(CultureInfo.CurrentCulture);
                            break;
                        case 5:
                            elementlast.Knoten3.Text = elementLast.Lastwerte[2].ToString(CultureInfo.CurrentCulture);
                            elementlast.Knoten4.Text = elementLast.Lastwerte[3].ToString(CultureInfo.CurrentCulture);
                            elementlast.Knoten5.Text = elementLast.Lastwerte[4].ToString(CultureInfo.CurrentCulture);
                            break;
                        case 6:
                            elementlast.Knoten3.Text = elementLast.Lastwerte[2].ToString(CultureInfo.CurrentCulture);
                            elementlast.Knoten4.Text = elementLast.Lastwerte[3].ToString(CultureInfo.CurrentCulture);
                            elementlast.Knoten5.Text = elementLast.Lastwerte[4].ToString(CultureInfo.CurrentCulture);
                            elementlast.Knoten6.Text = elementLast.Lastwerte[5].ToString(CultureInfo.CurrentCulture);
                            break;
                        case 7:
                            elementlast.Knoten3.Text = elementLast.Lastwerte[2].ToString(CultureInfo.CurrentCulture);
                            elementlast.Knoten4.Text = elementLast.Lastwerte[3].ToString(CultureInfo.CurrentCulture);
                            elementlast.Knoten5.Text = elementLast.Lastwerte[4].ToString(CultureInfo.CurrentCulture);
                            elementlast.Knoten6.Text = elementLast.Lastwerte[5].ToString(CultureInfo.CurrentCulture);
                            elementlast.Knoten7.Text = elementLast.Lastwerte[6].ToString(CultureInfo.CurrentCulture);
                            break;
                        case 8:
                            elementlast.Knoten3.Text = elementLast.Lastwerte[2].ToString(CultureInfo.CurrentCulture);
                            elementlast.Knoten4.Text = elementLast.Lastwerte[3].ToString(CultureInfo.CurrentCulture);
                            elementlast.Knoten5.Text = elementLast.Lastwerte[4].ToString(CultureInfo.CurrentCulture);
                            elementlast.Knoten6.Text = elementLast.Lastwerte[5].ToString(CultureInfo.CurrentCulture);
                            elementlast.Knoten7.Text = elementLast.Lastwerte[6].ToString(CultureInfo.CurrentCulture);
                            elementlast.Knoten8.Text = elementLast.Lastwerte[7].ToString(CultureInfo.CurrentCulture);
                            break;
                    }
            }
            // Textdarstellung ist zeitabhängige Elementlast
            else if (_modell.ZeitabhängigeElementLasten.TryGetValue(item.Uid, out var zeitElementlast))
            {
                var elementlast = new ZeitElementtemperaturNeu(_modell)
                {
                    LastId = { Text = zeitElementlast.LastId },
                    ElementId = { Text = zeitElementlast.ElementId },
                    P0 = { Text = zeitElementlast.P[0].ToString("G2") },
                    P1 = { Text = zeitElementlast.P[1].ToString("G2") }
                };
                switch (zeitElementlast.P.Length)
                {
                    case 3:
                        elementlast.P2.Text = zeitElementlast.P[2].ToString("G2");
                        break;
                    case 4:
                        elementlast.P2.Text = zeitElementlast.P[2].ToString("G2");
                        elementlast.P3.Text = zeitElementlast.P[3].ToString("G2");
                        break;
                }
            }

            // Textdarstellung ist Randtemperatur
            else if (_modell.Randbedingungen.TryGetValue(item.Uid, out var randbedingung))
            {
               _ = new RandbdingungNeu(_modell)
                {
                    RandbedingungId = { Text = randbedingung.RandbedingungId },
                    KnotenId = { Text = randbedingung.KnotenId },
                    Temperatur = { Text = randbedingung.Vordefiniert[0].ToString("g3") }
                };
            }
            // Textdarstellung ist zeitabhängige Randtemperatur
            else if (_modell.ZeitabhängigeRandbedingung.TryGetValue(item.Uid, out var zeitRandbedingung))
            {
                var rand = new ZeitRandtemperaturNeu(_modell)
                {
                    RandbedingungId = { Text = zeitRandbedingung.RandbedingungId },
                    KnotenId = { Text = zeitRandbedingung.KnotenId }
                };
                switch (zeitRandbedingung.VariationsTyp)
                {
                    case 0:
                        rand.Datei.IsChecked = true;
                        break;
                    case 1:
                        rand.Konstant.Text = zeitRandbedingung.KonstanteTemperatur.ToString("g3");
                        break;
                    case 2:
                        rand.Konstant.Text = zeitRandbedingung.KonstanteTemperatur.ToString("g3");
                        rand.Amplitude.Text = zeitRandbedingung.Amplitude.ToString("g3");
                        rand.Frequenz.Text = zeitRandbedingung.Frequenz.ToString("g3");
                        rand.Winkel.Text = zeitRandbedingung.PhasenWinkel.ToString("g3");
                        break;
                    case 3:
                        var intervall = zeitRandbedingung.Intervall;
                        for (var i = 0; i < intervall.Length; i += 2)
                        {
                            sb.Append(intervall[i].ToString("N0"));
                            sb.Append(';');
                            sb.Append(intervall[i + 1].ToString("N0"));
                            sb.Append(' ');
                        }

                        rand.Linear.Text = sb.ToString();
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