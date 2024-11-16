using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using static System.Globalization.CultureInfo;
using static System.Windows.Controls.Canvas;
using static System.Windows.Media.Brushes;
using static System.Windows.Media.Color;

namespace FE_Berechnungen.Wärmeberechnung;

public class Darstellung
{
    public const int RandOben = 60;
    public const int RandLinks = 60;
    private const double MaxScreenLength = 40;
    private readonly FeModell _modell;
    private readonly Canvas _visual;
    private AbstraktElement _aktElement;
    public double Auflösung;
    private double _auflösungH, _auflösungV;
    private Knoten _knoten;
    private double _maxTemp;
    private double _maxX;
    public double MaxY;
    private double _minTemp = 100;
    private double _screenH, _screenV;
    private double _temp;
    public int Zeitschritt;

    public Darstellung(FeModell feModell, Canvas visual)
    {
        _modell = feModell;
        _visual = visual;
        KnotenIDs = [];
        ElementIDs = [];
        //LastIDs = new List<TextBlock>();
        LastKnoten = [];
        LastLinien = [];
        LastElemente = [];
        Knotentemperaturen = [];
        Knotengradienten = [];
        TemperaturElemente = [];
        WärmeVektoren = [];
        RandKnoten = [];
        Anfangsbedingungen = [];
        FestlegungAuflösung();
    }

    public List<TextBlock> ElementIDs { get; }
    public List<TextBlock> KnotenIDs { get; }
    public List<TextBlock> LastKnoten { get; }
    public List<Shape> LastLinien { get; }
    public List<Shape> LastElemente { get; }
    public List<TextBlock> Knotentemperaturen { get; }
    public List<TextBlock> Knotengradienten { get; }
    public List<Shape> TemperaturElemente { get; }
    public List<Shape> WärmeVektoren { get; }
    public List<TextBlock> RandKnoten { get; }
    private List<TextBlock> Anfangsbedingungen { get; }

    public void FestlegungAuflösung()
    {
        const int rand = 100;
        _screenH = _visual.ActualWidth;
        _screenV = _visual.ActualHeight;

        foreach (var item in _modell.Knoten)
        {
            _knoten = item.Value;
            if (_knoten.Koordinaten[0] > _maxX) _maxX = _knoten.Koordinaten[0];
            if (_knoten.Koordinaten[1] > MaxY) MaxY = _knoten.Koordinaten[1];
        }

        if (_screenH / _maxX < _screenV / MaxY) Auflösung = (_screenH - rand) / _maxX;
        else Auflösung = (_screenV - rand) / MaxY;
    }

    public void KnotenTexte()
    {
        foreach (var item in _modell.Knoten)
        {
            var id = new TextBlock
            {
                Name = "Knoten",
                FontSize = 12,
                Text = item.Key,
                Foreground = Black
            };
            SetTop(id, (-item.Value.Koordinaten[1] + MaxY) * Auflösung + RandOben);
            SetLeft(id, item.Value.Koordinaten[0] * Auflösung + RandLinks);
            _visual.Children.Add(id);
            KnotenIDs.Add(id);
        }
    }

    public void AlleKnotenZeichnen()
    {
        foreach (var item in _modell.Knoten)
        {
            KnotenZeigen(item.Value, Brushes.Black, 1);
        }
    }
    public Shape KnotenZeigen(Knoten feKnoten, Brush farbe, double wichte)
    {
        var punkt = TransformKnoten(feKnoten, Auflösung, MaxY);

        // Geometrie wird einer GeometryGroup hinzugefügt, Daten der GeometryGroup einem Path übergeben
        // der Path wird auf einem Canvas dargestellt
        var knotenZeigen = new GeometryGroup();
        knotenZeigen.Children.Add(new EllipseGeometry(new Point(punkt.X, punkt.Y), 3, 3));
        Shape knotenPath = new Path
        {
            Stroke = farbe,
            Fill = farbe,
            StrokeThickness = wichte,
            Data = knotenZeigen
        };
        SetLeft(knotenPath, RandLinks);
        SetTop(knotenPath, RandOben);
        _visual.Children.Add(knotenPath);
        return knotenPath;
    }

    public void ElementTexte()
    {
        foreach (var item in _modell.Elemente)
        {
            var abstract2D = (Abstrakt2D)item.Value;
            var cg = abstract2D.BerechneSchwerpunkt();
            var id = new TextBlock
            {
                Name = "Element",
                FontSize = 12,
                Text = item.Key,
                Foreground = Blue
            };
            SetTop(id, (-cg.Y + MaxY) * Auflösung + RandOben);
            SetLeft(id, cg.X * Auflösung + RandLinks);
            _visual.Children.Add(id);
            ElementIDs.Add(id);
        }
    }

    public void AlleElementeZeichnen()
    {
        foreach (var item in _modell.Elemente) ElementZeichnen(item.Value);
        //for (var i = 0; i < _modell.Elemente.Count; i++)
        //{
        //    ElementZeichnen(_modell.Elemente.ElementAt(i).Value, Black, 2);
        //}
    }

    private void ElementZeichnen(AbstraktElement element)
    {
        _ = ElementUmrandung(element);
    }

    public Shape ElementFillZeichnen(AbstraktElement element, Brush umrissFarbe, Color füllFarbe, double transparenz, double wichte)
    {
        var pathGeometry = ElementUmrandung(element);
        var füllung = new SolidColorBrush(füllFarbe) { Opacity = .2 };

        Shape elementPath = new Path
        {
            Name = element.ElementId,
            Stroke = umrissFarbe,
            StrokeThickness = wichte,
            Fill = füllung,
            Data = pathGeometry
        };
        SetLeft(elementPath, RandLinks);
        SetTop(elementPath, RandOben);
        _visual.Children.Add(elementPath);
        return elementPath;
    }

    private PathGeometry ElementUmrandung(AbstraktElement element)
    {
        var pathFigure = new PathFigure();
        var pathGeometry = new PathGeometry();

        // Elementkanten werden in einer pathfigure gesammelt
        if (!_modell.Knoten.TryGetValue(element.KnotenIds[0], out _knoten))
        {
            throw new ModellAusnahme("\nElement Knoten '" + element.KnotenIds[0] + "' nicht im Modell gefunden");
        }
        var startPoint = TransformKnoten(_knoten, Auflösung, MaxY);
        pathFigure.StartPoint = startPoint;

        if (!_modell.Knoten.TryGetValue(element.KnotenIds[1], out _knoten))
        {
            throw new ModellAusnahme("\nElement Knoten '" + element.KnotenIds[1] + "' nicht im Modell gefunden");
        }
        var nextPoint = TransformKnoten(_knoten, Auflösung, MaxY);
        pathFigure.Segments.Add(new LineSegment(nextPoint, true));

        // Elemente mit mehr als 2 Knoten werden geschlossen
        for (var i = 2; i < element.KnotenProElement; i++)
        {
            if (!_modell.Knoten.TryGetValue(element.KnotenIds[i], out _knoten))
            {
                throw new ModellAusnahme("\nElement Knoten '" + element.KnotenIds[i] + "' nicht im Modell gefunden");
            }
            nextPoint = TransformKnoten(_knoten, Auflösung, MaxY);
            pathFigure.Segments.Add(new LineSegment(nextPoint, true));
        }
        if(element.KnotenProElement >2) pathFigure.IsClosed = true;

        // komplette Elementumrandung pathFigure wird einer pathGeometry hinzugefügt
        // pathGeometry wird auf Canvas dargestellt
        pathGeometry.Figures.Add(pathFigure);
        Shape elementPath = new Path
        {
            Stroke = Brushes.Black,
            StrokeThickness = 1,
            Data = pathGeometry
        };
        SetLeft(elementPath, RandLinks);
        SetTop(elementPath, RandOben);
        _visual.Children.Add(elementPath);
        return pathGeometry;
    }

    public void KnotenlastenZeichnen()
    {
        const int lastOffset = 10;
        foreach (var item in _modell.Lasten)
        {
            var knotenId = item.Value.KnotenId;
            var lastWert = item.Value.Lastwerte[0];
            if (!_modell.Knoten.TryGetValue(knotenId, out _knoten))
            {
                throw new ModellAusnahme("\nElementlast Knoten '" + item.Value.KnotenId + "' nicht im Modell gefunden");
            }

            var lastPunkt = TransformKnoten(_knoten, Auflösung, MaxY);
            var knotenLast = new TextBlock
            {
                FontSize = 12,
                Text = lastWert.ToString(CurrentCulture),
                Foreground = Red
            };
            SetTop(knotenLast, lastPunkt.Y + RandOben + lastOffset);
            SetLeft(knotenLast, lastPunkt.X + RandLinks);


            LastKnoten.Add(knotenLast);
            _visual.Children.Add(knotenLast);
        }

        // zeitabhängige Knotenlasten
        foreach (var zeitKnotenlast in from item
                     in _modell.ZeitabhängigeKnotenLasten
                                       let zeitKnotenlast = new TextBlock
                                       {
                                           Name = "ZeitKnotenLast",
                                           Uid = item.Key,
                                           FontSize = 12,
                                           Text = item.Key,
                                           Foreground = DarkViolet
                                       }
                                       where _modell.Knoten.TryGetValue(item.Value.KnotenId, out _knoten)
                                       where _knoten != null
                                       select zeitKnotenlast)
        {
            if (_knoten == null)
            {
                throw new ModellAusnahme("\nKnoten für zeitabhängige Knotenlast nicht im Modell gefunden");
            }
            SetTop(zeitKnotenlast, (-_knoten.Koordinaten[1] + MaxY) * Auflösung + RandOben + lastOffset);
            SetLeft(zeitKnotenlast, _knoten.Koordinaten[0] * Auflösung + RandLinks);
            _visual.Children.Add(zeitKnotenlast);
            LastKnoten.Add(zeitKnotenlast);
        }
    }

    public void ElementlastenZeichnen()
    {
        foreach (var item in _modell.ElementLasten)
        {
            if (!_modell.Elemente.TryGetValue(item.Value.ElementId, out var element))
            {
                throw new ModellAusnahme("\nElement '" + item.Value.ElementId + "' für Elementlast nicht im Modell gefunden");
            }

            var pathGeometry = ElementUmrandung(element);
            var füllung = new SolidColorBrush(Colors.Red) { Opacity = .2 };

            Shape elementPath = new Path
            {
                Name = item.Key,
                Stroke = Black,
                StrokeThickness = 2,
                Fill = füllung,
                Data = pathGeometry
            };
            SetLeft(elementPath, RandLinks);
            SetTop(elementPath, RandOben);
            _visual.Children.Add(elementPath);

            var abstrakt2D = (Abstrakt2D)element;
            const int lastOffset = -15;
            if (abstrakt2D != null)
            {
                var cg = abstrakt2D.BerechneSchwerpunkt();
                var id = new TextBlock
                {
                    Name = "Last",
                    Uid = item.Value.LastId,
                    FontSize = 12,
                    Text = item.Key,
                    Foreground = Red
                };
                SetTop(id, (-cg.Y + MaxY) * Auflösung + RandOben + lastOffset);
                SetLeft(id, cg.X * Auflösung + RandLinks);
                _visual.Children.Add(id);
                LastKnoten.Add(id);
            }

            LastElemente.Add(elementPath);
        }

        // zeitabhängige Elementlasten
        foreach (var item in _modell.ZeitabhängigeElementLasten)
        {
            if (!_modell.Elemente.TryGetValue(item.Value.ElementId, out var element))
            {
                throw new ModellAusnahme("\nElement '" + item.Value.ElementId + "' für Elementlast nicht im Modell gefunden");
            }

            var pathGeometry = ElementUmrandung(element);
            var füllung = new SolidColorBrush(Colors.Violet) { Opacity = .2 };

            Shape elementPath = new Path
            {
                Name = item.Key,
                Stroke = Black,
                StrokeThickness = 2,
                Fill = füllung,
                Data = pathGeometry
            };
            SetLeft(elementPath, RandLinks);
            SetTop(elementPath, RandOben);
            _visual.Children.Add(elementPath);

            var abstrakt2D = (Abstrakt2D)element;
            const int lastOffset = -15;
            if (abstrakt2D != null)
            {
                var cg = abstrakt2D.BerechneSchwerpunkt();
                var id = new TextBlock
                {
                    Name = "ZeitElementLast",
                    Uid = item.Value.LastId,
                    FontSize = 12,
                    Text = item.Key,
                    Foreground = DarkViolet
                };
                SetTop(id, (-cg.Y + MaxY) * Auflösung + RandOben + lastOffset);
                SetLeft(id, cg.X * Auflösung + RandLinks);
                _visual.Children.Add(id);
                LastKnoten.Add(id);
            }

            LastElemente.Add(elementPath);
        }
    }

    public void LinienlastenZeichnen()
    {
        foreach (var item in _modell.LinienLasten)
        {
            var linienlast = item.Value;
            var pathFigure = new PathFigure();
            var pathGeometry = new PathGeometry();

            if (!_modell.Knoten.TryGetValue(linienlast.StartKnotenId, out _knoten))
            {
                throw new ModellAusnahme("\nStartknoten '" + linienlast.StartKnotenId + "' für Linienlast nicht im Modell gefunden");
            }

            var startPoint = TransformKnoten(_knoten, Auflösung, MaxY);
            pathFigure.StartPoint = startPoint;
            if (!_modell.Knoten.TryGetValue(linienlast.EndKnotenId, out _knoten))
            {
                throw new ModellAusnahme("\nEndknoten '" + linienlast.EndKnotenId + "' für Linienlast nicht im Modell gefunden");
            }

            var endPoint = TransformKnoten(_knoten, Auflösung, MaxY);
            pathFigure.StartPoint = startPoint;
            pathFigure.Segments.Add(new LineSegment(endPoint, true));

            pathGeometry.Figures.Add(pathFigure);

            Shape linienPath = new Path
            {
                Name = linienlast.LastId,
                Stroke = Red,
                StrokeThickness = 5,
                Data = pathGeometry
            };
            SetLeft(linienPath, RandLinks);
            SetTop(linienPath, RandOben);
            _visual.Children.Add(linienPath);
            LastLinien.Add(linienPath);
        }
    }

    public void RandbedingungenZeichnen()
    {
        const int randOffset = 15;
        // zeichne den Wert einer jeden Randbedingung als Text an Randknoten
        foreach (var item in _modell.Randbedingungen)
        {
            var knotenId = item.Value.KnotenId;
            if (!_modell.Knoten.TryGetValue(knotenId, out _knoten))
            {
                throw new ModellAusnahme("\nKnoten '" + item.Value.KnotenId + "' für Randbedingung nicht im Modell gefunden");
            }

            var fensterKnoten = TransformKnoten(_knoten, Auflösung, MaxY);

            var randWert = item.Value.Vordefiniert[0];
            var randbedingung = new TextBlock
            {
                Name = "Support",
                Uid = item.Value.RandbedingungId,
                FontSize = 12,
                Text = randWert.ToString("N2"),
                Foreground = DarkOliveGreen,
                Background = LightGreen
            };
            RandKnoten.Add(randbedingung);
            SetTop(randbedingung, fensterKnoten.Y + RandOben + randOffset);
            SetLeft(randbedingung, fensterKnoten.X + RandLinks);
            _visual.Children.Add(randbedingung);

            randbedingung.Text = string.Empty;
        }

        // zeitabhängige Randknoten
        foreach (var item in _modell.ZeitabhängigeRandbedingung)
        {
            var knotenId = item.Value.KnotenId;
            if (!_modell.Knoten.TryGetValue(knotenId, out _knoten))
            {
                throw new ModellAusnahme("\nRandknoten '" + item.Value.KnotenId + "' nicht im Modell gefunden");
            }

            var fensterKnoten = TransformKnoten(_knoten, Auflösung, MaxY);

            var randbedingung = new TextBlock
            {
                Name = "ZeitRandbedingung",
                Uid = item.Value.RandbedingungId,
                FontSize = 12,
                Text = item.Key,
                Foreground = DarkGreen,
                Background = LawnGreen
            };
            RandKnoten.Add(randbedingung);
            SetTop(randbedingung, fensterKnoten.Y + RandOben + 15);
            SetLeft(randbedingung, fensterKnoten.X + RandLinks);
            _visual.Children.Add(randbedingung);
        }
    }

    public void KnotentemperaturZeichnen()
    {
        foreach (var item in _modell.Knoten)
        {
            _knoten = item.Value;
            var temperatur = _knoten.Knotenfreiheitsgrade[0].ToString("N2");
            _temp = _knoten.Knotenfreiheitsgrade[0];
            if (_temp > _maxTemp) _maxTemp = _temp;
            if (_temp < _minTemp) _minTemp = _temp;
            var fensterKnoten = TransformKnoten(_knoten, Auflösung, MaxY);

            var id = new TextBlock
            {
                Name = item.Key,
                FontSize = 12,
                Background = LightGray,
                FontWeight = FontWeights.Bold,
                Text = temperatur
            };
            Knotentemperaturen.Add(id);
            SetTop(id, fensterKnoten.Y + RandOben);
            SetLeft(id, fensterKnoten.X + RandLinks);
            _visual.Children.Add(id);
        }
    }

    public void AnfangsbedingungenZeichnen()
    {
        if (_modell.Zeitintegration == null) return;
 
        const int anfangOffset = 30;
        // zeichne Wert einer jeden Anfangsbedingung als Text an Knoten

        foreach (var item in _modell.Zeitintegration.Anfangsbedingungen)
        {
            var knotenId = item.KnotenId;
            if (knotenId == "alle") continue;

            if (!_modell.Knoten.TryGetValue(knotenId, out _knoten))
            {
                throw new ModellAusnahme("\nKnoten '" + knotenId + "' für Anfangsbedingung nicht im Modell gefunden");
            }

            var fensterKnoten = TransformKnoten(_knoten, Auflösung, MaxY);

            var anfangsbedingung = new TextBlock
            {
                Name = knotenId,
                Uid = "A",
                FontSize = 12,
                Text = "A"+item.Werte[0].ToString("N2"),
                Foreground = Black,
                Background = Turquoise
            };
            SetTop(anfangsbedingung, fensterKnoten.Y + RandOben + anfangOffset);
            SetLeft(anfangsbedingung, fensterKnoten.X + RandLinks);
            _visual.Children.Add(anfangsbedingung);
            Anfangsbedingungen.Add(anfangsbedingung);
        }
    }

    public void AnfangsbedingungenZeichnen(string knotenId, double knotenwert, string anf)
    {
        const int randOffset = 15;
        // zeichne den Wert einer Anfangsbedingung als Text an Knoten

        if (!_modell.Knoten.TryGetValue(knotenId, out _knoten))
        {
            throw new ModellAusnahme("\nKnoten '" + knotenId + "' für Anfangsbedingung nicht im Modell gefunden");
        }

        var fensterKnoten = TransformKnoten(_knoten, Auflösung, MaxY);

        var anfangsbedingung = new TextBlock
        {
            Name = "Anfangsbedingung",
            Uid = anf,
            FontSize = 12,
            Text = knotenwert.ToString("N2"),
            Foreground = Black,
            Background = Turquoise
        };
        SetTop(anfangsbedingung, fensterKnoten.Y + RandOben + randOffset);
        SetLeft(anfangsbedingung, fensterKnoten.X + RandLinks);
        _visual.Children.Add(anfangsbedingung);
        Anfangsbedingungen.Add(anfangsbedingung);
    }

    public void AnfangsbedingungenEntfernen()
    {
        foreach (var item in Anfangsbedingungen) _visual.Children.Remove(item);
        Anfangsbedingungen.Clear();
    }

    public void ElementTemperaturZeichnen()
    {
        foreach (var path in TemperaturElemente) _visual.Children.Remove(path);
        TemperaturElemente.Clear();
        foreach (var item in _modell.Knoten)
        {
            _knoten = item.Value;
            _temp = _knoten.Knotenfreiheitsgrade[0];
            if (_temp > _maxTemp) _maxTemp = _temp;
            if (_temp < _minTemp) _minTemp = _temp;
        }

        foreach (var item in _modell.Elemente)
        {
            _aktElement = item.Value;
            var pathGeometry = ElementUmrandung((Abstrakt2D)_aktElement);
            //var elementTemperature = aktElement.KnotenIds.Where(knotenId
            //    => _modell.Knoten.TryGetValue(knotenId, out knoten)).Sum(knotenId => knoten.Knotenfreiheitsgrade[0]);
            double elementTemperatur = 0;
            for (var i = 0; i < _aktElement.KnotenProElement; i++)
            {
                if (!_modell.Knoten.TryGetValue(_aktElement.KnotenIds[i], out _knoten))
                    throw new ModellAusnahme("\nElementknoten '" + _aktElement.KnotenIds[i] + "' nicht im Modell gefunden");
                elementTemperatur += _knoten.Knotenfreiheitsgrade[0];
            }
            elementTemperatur /= _aktElement.KnotenProElement;

            var intens = (byte)(255 * (elementTemperatur - _minTemp) / (_maxTemp - _minTemp));
            var rot = FromArgb(intens, 255, 0, 0);
            var myBrush = new SolidColorBrush(rot);

            Shape path = new Path
            {
                Stroke = Blue,
                StrokeThickness = 1,
                Name = _aktElement.ElementId,
                Opacity = 0.5,
                Fill = myBrush,
                Data = pathGeometry
            };
            TemperaturElemente.Add(path);
            // setz oben/links Position zum Zeichnen auf dem Canvas
            SetLeft(path, RandLinks);
            SetTop(path, RandOben);
            // zeichne Shape
            _visual.Children.Add(path);
        }
    }

    public void KnotengradientenZeichnen(int index)
    {
        foreach (var item in _modell.Knoten)
        {
            _knoten = item.Value;
            var gradient = _knoten.KnotenAbleitungen[0][index].ToString("N2");
            _temp = _knoten.Knotenfreiheitsgrade[0];
            if (_temp > _maxTemp) _maxTemp = _temp;
            if (_temp < _minTemp) _minTemp = _temp;
            var fensterKnoten = TransformKnoten(_knoten, Auflösung, MaxY);

            var id = new TextBlock
            {
                FontSize = 12,
                Background = LightBlue,
                FontWeight = FontWeights.Bold,
                Text = gradient
            };
            Knotengradienten.Add(id);
            SetTop(id, fensterKnoten.Y + RandOben + 15);
            SetLeft(id, fensterKnoten.X + RandLinks);
            _visual.Children.Add(id);
        }
    }

    public void WärmeflussvektorenZeichnen()
    {
        foreach (var path in WärmeVektoren) _visual.Children.Remove(path);
        WärmeVektoren.Clear();
        double maxVektor = 0;
        foreach (var abstract2D in _modell.Elemente.Select(item => (Abstrakt2D)item.Value))
        {
            abstract2D.ElementZustand = abstract2D.BerechneElementZustand(0, 0);
            var vektor = Math.Sqrt(abstract2D.ElementZustand[0] * abstract2D.ElementZustand[0] +
                                   abstract2D.ElementZustand[1] * abstract2D.ElementZustand[1]);
            if (maxVektor < vektor) maxVektor = vektor;
        }

        var vektorskalierung = MaxScreenLength / maxVektor;

        foreach (var abstrakt2D in _modell.Elemente.Select(item => (Abstrakt2D)item.Value))
        {
            abstrakt2D.ElementZustand = abstrakt2D.BerechneElementZustand(0, 0);
            var vektorLänge = Math.Sqrt(abstrakt2D.ElementZustand[0] * abstrakt2D.ElementZustand[0] +
                                        abstrakt2D.ElementZustand[1] * abstrakt2D.ElementZustand[1]) * vektorskalierung;
            var vektorWinkel = Math.Atan2(abstrakt2D.ElementZustand[1], abstrakt2D.ElementZustand[0]) * 180 / Math.PI;
            // zeichne den resultierenden Vektor mit seinem Mittelpunkt im Elementschwerpunkt
            // füge am Endpunkt Pfeilspitzen an und füge Wärmeflusspfeil als pathFigure zur pathGeometry hinzu
            var pathGeometry = WärmeflussElementmitte(abstrakt2D, vektorLänge);

            Shape path = new Path
            {
                Name = abstrakt2D.ElementId,
                Stroke = Black,
                StrokeThickness = 2,
                Data = pathGeometry
            };
            // rotiere Wärmeflusspfeil im Schwerpunkt um den Vektorwinkel
            var cg = abstrakt2D.BerechneSchwerpunkt();
            var rotateTransform = new RotateTransform(-vektorWinkel)
            {
                CenterX = (int)(cg.X * Auflösung),
                CenterY = (int)((-cg.Y + MaxY) * Auflösung)
            };
            path.RenderTransform = rotateTransform;
            // sammle alle Wärmeflusspfeile in der Liste Wärmevektoren, um deren Darstellung löschen zu können
            WärmeVektoren.Add(path);

            // setz oben/links Position zum Zeichnen auf dem Canvas
            SetLeft(path, RandLinks);
            SetTop(path, RandOben);
            // zeichne Shape
            _visual.Children.Add(path);
        }
    }

    private PathGeometry WärmeflussElementmitte(AbstraktElement abstraktElement, double length)
    {
        var abstrakt2D = (Abstrakt2D)abstraktElement;
        var pathFigure = new PathFigure();
        var pathGeometry = new PathGeometry();
        var cg = abstrakt2D.BerechneSchwerpunkt();
        int[] fensterKnoten = { (int)(cg.X * Auflösung), (int)((-cg.Y + MaxY) * Auflösung) };
        var startPoint = new Point(fensterKnoten[0] - length / 2, fensterKnoten[1]);
        var endPoint = new Point(fensterKnoten[0] + length / 2, fensterKnoten[1]);
        pathFigure.StartPoint = startPoint;
        pathFigure.Segments.Add(new LineSegment(endPoint, true));
        pathFigure.Segments.Add(new LineSegment(new Point(endPoint.X - 3, endPoint.Y - 2), true));
        pathFigure.Segments.Add(new LineSegment(new Point(endPoint.X - 3, endPoint.Y + 2), true));
        pathFigure.Segments.Add(new LineSegment(new Point(endPoint.X, endPoint.Y), true));
        pathGeometry.Figures.Add(pathFigure);
        return pathGeometry;
    }

    // Zeitverlauf wird ab tmin dargestellt
    public void ZeitverlaufZeichnen(double dt, double tmin, double tmax, double mY, double[] ordinaten)
    {
        if (ordinaten[0] > double.MaxValue) ordinaten[0] = mY;
        var zeitverlauf = new Polyline
        {
            Stroke = Red,
            StrokeThickness = 2
        };
        var stützpunkte = new PointCollection();
        var start = (int)Math.Round(tmin / dt);
        for (var i = 0; i < ordinaten.Length - start; i++)
        {
            var point = new Point(dt * i * _auflösungH, -ordinaten[i + start] * _auflösungV);
            stützpunkte.Add(point);
        }

        zeitverlauf.Points = stützpunkte;

        // setz oben/links Position zum Zeichnen auf dem Canvas
        SetLeft(zeitverlauf, RandLinks);
        SetTop(zeitverlauf, mY * _auflösungV + RandOben);
        // zeichne Shape
        _visual.Children.Add(zeitverlauf);
    }

    public void Koordinatensystem(double tmin, double tmax, double max, double min)
    {
        const int rand = 20;
        const int maxOrdinateAnzeigen = 100;
        _screenH = _visual.ActualWidth;
        _screenV = _visual.ActualHeight;
        if (double.IsNaN(max))
        {
            max = maxOrdinateAnzeigen;
            min = -max;
        }

        if (max - min < double.Epsilon) _auflösungV = _screenV - rand;
        else _auflösungV = (_screenV - rand) / (max - min);
        _auflösungH = (_screenH - rand) / (tmax - tmin);
        var xAchse = new Line
        {
            Stroke = Black,
            X1 = 0,
            Y1 = max * _auflösungV + RandOben,
            X2 = (tmax - tmin) * _auflösungH + RandLinks,
            Y2 = max * _auflösungV + RandOben,
            StrokeThickness = 2
        };
        _ = _visual.Children.Add(xAchse);
        var yAchse = new Line
        {
            Stroke = Black,
            X1 = RandLinks,
            Y1 = max * _auflösungV - min * _auflösungV + 2 * RandOben,
            X2 = RandLinks,
            Y2 = RandOben,
            StrokeThickness = 2
        };
        _visual.Children.Add(yAchse);
    }

    private static Point TransformKnoten(Knoten knoten, double auflösung, double maxY)
    {
        return new Point(knoten.Koordinaten[0] * auflösung, (-knoten.Koordinaten[1] + maxY) * auflösung);
    }

    public double[] TransformBildPunkt(Point point)
    {
        var koordinaten = new double[2];
        koordinaten[0] = (point.X - RandLinks) / Auflösung;
        koordinaten[1] = (-point.Y + RandOben) / Auflösung + MaxY;
        return koordinaten;
    }
}