using FE_Berechnungen.Tragwerksberechnung.Modelldaten;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using static System.Windows.Controls.Canvas;
using static System.Windows.Media.Brushes;
using static System.Windows.Media.Color;
using Path = System.Windows.Shapes.Path;

namespace FE_Berechnungen.Tragwerksberechnung;

public class Darstellung
{
    private const int RandOben = 60, RandLinks = 60;
    private const int MaxNormalkraftScreen = 30, MaxQuerkraftScreen = 40, MaxMomentScreen = 50;
    private readonly FeModell _modell;
    private readonly Canvas _visual;
    private double _auflösungH, _auflösungV, _lastAuflösung;
    private Knoten _knoten;
    private double _minX, _maxX, _minY;
    private Point _platzierungText;
    private double _screenH, _screenV;
    public double Auflösung;
    public TextBlock MaxMomentText;
    public double MaxY;
    public double PlatzierungV, PlatzierungH;

    public int ÜberhöhungVerformung = 1, ÜberhöhungRotation = 1;
    //public List<TextBlock> Anfangsbedingungen { get; }


    public Darstellung(FeModell feModell, Canvas visual)
    {
        _modell = feModell;
        _visual = visual;
        ElementIDs = [];
        KnotenIDs = [];
        LastIDs = [];
        LagerIDs = [];
        Verformungen = [];
        LastVektoren = [];
        LagerDarstellung = [];
        NormalkraftListe = [];
        QuerkraftListe = [];
        MomenteListe = [];
        //Anfangsbedingungen = [];
        MomentenMaxTexte = [];
        FestlegungAuflösung();
    }

    public List<object> ElementIDs { get; }
    public List<object> KnotenIDs { get; }
    public List<object> LastIDs { get; }
    public List<object> LagerIDs { get; }
    public List<object> MomentenMaxTexte { get; }
    public List<object> Verformungen { get; }
    public List<object> LastVektoren { get; }
    public List<object> LagerDarstellung { get; }
    public List<object> NormalkraftListe { get; }
    public List<object> QuerkraftListe { get; }
    public List<object> MomenteListe { get; }

    public void FestlegungAuflösung()
    {
        _screenH = _visual.ActualWidth;
        _screenV = _visual.ActualHeight;

        if (_modell.MaxX - _modell.MinX == 0 && _modell.MaxY - _modell.MinY == 0)
        {
            var x = new List<double>();
            var y = new List<double>();

            if (_modell.Knoten.Count == 0)
            {
                throw new ModellAusnahme("\nModellabmessungen können nicht bestimmt werden, keine Knotengeometrie vorhanden");
            }

            foreach (var item in _modell.Knoten)
            {
                x.Add(item.Value.Koordinaten[0]);
                y.Add(item.Value.Koordinaten[1]);
            }
            MaxY = y.Max();
            _minY = y.Min();
            _maxX = x.Max();
            _minX = x.Min();
        }
        else
        {
            MaxY = _modell.MaxY;
            _minY = _modell.MinY;
            _maxX = _modell.MaxX;
            _minX = _modell.MinX;
        }

        // vertikales Modell
        var delta = Math.Abs(_maxX - _minX);
        if (delta < 1)
        {
            _auflösungH = _screenH - 2 * RandLinks;
            PlatzierungH = (int)(0.5 * _screenH);
        }
        else
        {
            _auflösungH = (_screenH - 2 * RandLinks) / delta;
            PlatzierungH = RandLinks;
        }

        // horizontales Modell
        delta = Math.Abs(MaxY - _minY);
        if (delta < 1)
        {
            Auflösung = _screenV - 2 * RandOben;
            PlatzierungV = (int)(0.5 * _screenV);
        }
        else
        {
            Auflösung = (_screenV - 2 * RandOben) / delta;
            PlatzierungV = RandOben;
        }

        if (_auflösungH < Auflösung) Auflösung = _auflösungH;
    }

    public void UnverformteGeometrie()
    {
        // Knoten werden als kleine schwarze Punkte gezeigt
        foreach (var item in _modell.Knoten) KnotenZeigen(item.Value, Black, 1);

        // Elementumrisse werden als Shape (PathGeometry) mit Namen hinzugefügt
        // pathGeometry enthält EIN spezifisches Element
        // alle Elemente werden der GeometryGroup tragwerk hinzugefügt

        foreach (var item in _modell.Elemente) ElementZeichnen(item.Value, Black, 2);

        // Knotengelenke werden als EllipseGeometry der GeometryGroup tragwerk hinzugefügt
        var tragwerk = new GeometryGroup();
        foreach (var gelenk in from item in _modell.Knoten
                               select item.Value
                 into knoten
                               where knoten.AnzahlKnotenfreiheitsgrade == 2
                               select TransformKnoten(knoten, Auflösung, MaxY)
                 into gelenkPunkt
                               select new EllipseGeometry(gelenkPunkt, 5, 5))
            tragwerk.Children.Add(gelenk);
        // Knotengelenke werden gezeichnet
        var tragwerkPath = new Path
        {
            Stroke = Black,
            StrokeThickness = 1,
            Data = tragwerk
        };
        SetLeft(tragwerkPath, PlatzierungH);
        SetTop(tragwerkPath, PlatzierungV);
        _visual.Children.Add(tragwerkPath);
    }

    public Shape KnotenZeigen(Knoten feKnoten, Brush farbe, double wichte)
    {
        var punkt = TransformKnoten(feKnoten, Auflösung, MaxY);

        var knotenZeigen = new GeometryGroup();
        knotenZeigen.Children.Add(
            new EllipseGeometry(new Point(punkt.X, punkt.Y), 2, 2));
        Shape knotenPath = new Path
        {
            Stroke = farbe,
            Fill = farbe,
            StrokeThickness = wichte,
            Data = knotenZeigen
        };
        SetLeft(knotenPath, PlatzierungH);
        SetTop(knotenPath, PlatzierungV);
        _visual.Children.Add(knotenPath);
        return knotenPath;
    }

    public Shape ElementZeichnen(AbstraktElement element, Brush farbe, double wichte)
    {
        var pathGeometry = element switch
        {
            FederElement => FederelementZeichnen(element),
            Fachwerk =>
                // Gelenke als Halbkreise an Knoten des Fachwerkelementes zeichnen
                FachwerkelementZeichnen(element),
            Biegebalken => BiegebalkenZeichnen(element),
            BiegebalkenGelenk =>
                // Gelenk am Startknoten bzw. Endknoten des BiegebalkenGelenk zeichnen
                BiegebalkenGelenkZeichnen(element),
            _ => MultiKnotenElementZeichnen(element)
        };
        Shape elementPath = new Path
        {
            Name = element.ElementId,
            Stroke = farbe,
            StrokeThickness = wichte,
            Data = pathGeometry
        };
        SetLeft(elementPath, PlatzierungH);
        SetTop(elementPath, PlatzierungV);
        _visual.Children.Add(elementPath);
        return elementPath;
    }

    public void VerformteGeometrie()
    {
        if (!_modell.Berechnet)
        {
            var analysis = new Berechnung(_modell);
            analysis.BerechneSystemMatrix();
            analysis.BerechneSystemVektor();
            analysis.LöseGleichungen();
            _modell.Berechnet = true;
        }

        var pathGeometry = new PathGeometry();

        foreach (var element in Beams())
        {
            var pathFigure = new PathFigure();
            Point start;
            Point end;
            double winkel;

            switch (element)
            {
                case Fachwerk:
                    {
                        if (!_modell.Knoten.TryGetValue(element.KnotenIds[0], out _knoten))
                        {
                            throw new ModellAusnahme("\nFachwerk Elementknoten '" + element.KnotenIds[0] + "' nicht im Modell gefunden");
                        }

                        start = TransformVerformtenKnoten(_knoten, Auflösung, MaxY);
                        pathFigure.StartPoint = start;

                        for (var i = 1; i < element.KnotenIds.Length; i++)
                        {
                            if (!_modell.Knoten.TryGetValue(element.KnotenIds[i], out _knoten))
                            {
                                throw new ModellAusnahme("\nFachwerk Elementknoten '" + element.KnotenIds[i] + "' nicht im Modell gefunden");
                            }

                            end = TransformVerformtenKnoten(_knoten, Auflösung, MaxY);
                            pathFigure.Segments.Add(new LineSegment(end, true));
                        }
                        break;
                    }
                case Biegebalken:
                    {
                        element.BerechneZustandsvektor();
                        if (!_modell.Knoten.TryGetValue(element.KnotenIds[0], out _knoten))
                        {
                            throw new ModellAusnahme("\nBiegebalken Elementknoten '" + element.KnotenIds[0] + "' nicht im Modell gefunden");
                        }

                        start = TransformVerformtenKnoten(_knoten, Auflösung, MaxY);
                        pathFigure.StartPoint = start;

                        for (var i = 1; i < element.KnotenIds.Length; i++)
                        {
                            if (!_modell.Knoten.TryGetValue(element.KnotenIds[i], out _knoten))
                            {
                                throw new ModellAusnahme("\nBiegebalken Elementknoten '" + element.KnotenIds[i] + "' nicht im Modell gefunden");
                            }

                            end = TransformVerformtenKnoten(_knoten, Auflösung, MaxY);
                            var richtung = end - start;
                            richtung.Normalize();
                            winkel = -element.ElementVerformungen[2] * 180 / Math.PI * ÜberhöhungRotation;
                            richtung = RotateVectorScreen(richtung, winkel);
                            var control1 = start + richtung * element.BalkenLänge / 4 * Auflösung;

                            richtung = start - end;
                            richtung.Normalize();
                            winkel = -element.ElementVerformungen[5] * 180 / Math.PI * ÜberhöhungRotation;
                            richtung = RotateVectorScreen(richtung, winkel);
                            var control2 = end + richtung * element.BalkenLänge / 4 * Auflösung;

                            pathFigure.Segments.Add(new BezierSegment(control1, control2, end, true));
                        }

                        break;
                    }
                case BiegebalkenGelenk:
                    {
                        if (!_modell.Knoten.TryGetValue(element.KnotenIds[0], out _knoten))
                        {
                            throw new ModellAusnahme("\nBiegebalkenGelenk Elementknoten '" + element.KnotenIds[0] + "' nicht im Modell gefunden");
                        }

                        start = TransformVerformtenKnoten(_knoten, Auflösung, MaxY);
                        pathFigure.StartPoint = start;

                        var control = start;
                        for (var i = 1; i < element.KnotenIds.Length; i++)
                        {
                            if (!_modell.Knoten.TryGetValue(element.KnotenIds[i], out _knoten))
                            {
                                throw new ModellAusnahme("\nBiegebalkenGelenk Elementknoten '" + element.KnotenIds[i] + "' nicht im Modell gefunden");
                            }

                            end = TransformVerformtenKnoten(_knoten, Auflösung, MaxY);

                            switch (element.Typ)
                            {
                                case 1:
                                    {
                                        var richtung = start - end;
                                        richtung.Normalize();
                                        winkel = element.ElementVerformungen[4] * 180 / Math.PI * ÜberhöhungRotation;
                                        richtung = RotateVectorScreen(richtung, winkel);
                                        control = end + richtung * element.BalkenLänge / 4 * Auflösung;
                                        break;
                                    }
                                case 2:
                                    {
                                        var richtung = end - start;
                                        richtung.Normalize();
                                        winkel = element.ElementVerformungen[2] * 180 / Math.PI * ÜberhöhungRotation;
                                        richtung = RotateVectorScreen(richtung, winkel);
                                        control = start + richtung * element.BalkenLänge / 4 * Auflösung;
                                        break;
                                    }
                            }

                            pathFigure.Segments.Add(new QuadraticBezierSegment(control, end, true));
                        }

                        break;
                    }
                default:
                    {
                        if (!_modell.Knoten.TryGetValue(element.KnotenIds[0], out _knoten))
                        {
                            throw new ModellAusnahme("\nElementknoten '" + element.KnotenIds[0] + "' nicht im Modell gefunden");
                        }

                        start = TransformVerformtenKnoten(_knoten, Auflösung, MaxY);
                        pathFigure.StartPoint = start;

                        for (var i = 1; i < element.KnotenIds.Length; i++)
                        {
                            if (!_modell.Knoten.TryGetValue(element.KnotenIds[i], out _knoten))
                            {
                                throw new ModellAusnahme("\nElementknoten '" + element.KnotenIds[i] + "' nicht im Modell gefunden");
                            }

                            var next = TransformVerformtenKnoten(_knoten, Auflösung, MaxY);
                            pathFigure.Segments.Add(new LineSegment(next, true));
                        }

                        pathFigure.IsClosed = true;
                        break;
                    }
            }

            pathGeometry.Figures.Add(pathFigure);

            Shape path = new Path
            {
                Stroke = Red,
                StrokeThickness = 2,
                Data = pathGeometry
            };

            SetLeft(path, PlatzierungH);
            SetTop(path, PlatzierungV);
            _visual.Children.Add(path);
            Verformungen.Add(path);
        }

        return;

        IEnumerable<AbstraktBalken> Beams()
        {
            foreach (var item in _modell.Elemente)
                if (item.Value is AbstraktBalken element)
                    yield return element;
        }
    }

    private PathGeometry FederelementZeichnen(AbstraktElement element)
    {
        var pathGeometry = new PathGeometry();
        var pathFigure = new PathFigure();
        // Platzierungspunkt des Federelementes
        if (!_modell.Knoten.TryGetValue(element.KnotenIds[0], out _knoten))
        {
            throw new ModellAusnahme("\nFederelement Elementknoten '" + element.KnotenIds[0] + "' nicht im Modell gefunden");
        }

        var startPunkt = TransformKnoten(_knoten, Auflösung, MaxY);

        // setz Referenzen der MaterialWerte
        element.SetzElementReferenzen(_modell);

        if (element.ElementMaterial.MaterialWerte.Length < 3)
        {
            throw new ModellAusnahme("\nFederlager '" + element.ElementMaterialId + "' 3 Werte für Federsteifigkeiten erforderlich");
        }

        // x-Feder
        if (Math.Abs(element.ElementMaterial.MaterialWerte[0]) > 0)
        {
            DehnfederZeichnen(pathFigure, startPunkt);
            pathGeometry.Figures.Add(pathFigure);
            pathGeometry.Transform = new RotateTransform(90, startPunkt.X, startPunkt.Y);
        }

        // y-Feder
        if (Math.Abs(element.ElementMaterial.MaterialWerte[1]) > 0)
        {
            DehnfederZeichnen(pathFigure, startPunkt);
            pathGeometry.Figures.Add(pathFigure);
        }

        // Drehfeder zeichnen
        if (!(Math.Abs(element.ElementMaterial.MaterialWerte[2]) > 0)) return pathGeometry;

        DrehfederZeichnen(pathFigure, startPunkt);
        pathGeometry.Figures.Add(pathFigure);
        return pathGeometry;
    }

    private static void DehnfederZeichnen(PathFigure pathFigure, Point startPunkt)
    {
        const double b = 6.0;
        const int h = 3;
        pathFigure.StartPoint = startPunkt;
        pathFigure.Segments.Add(
            new LineSegment(startPunkt with { Y = startPunkt.Y + 2 * h }, true));
        pathFigure.Segments.Add(
            new LineSegment(new Point(startPunkt.X - b, startPunkt.Y + 3 * h), true));
        pathFigure.Segments.Add(
            new LineSegment(new Point(startPunkt.X + b, startPunkt.Y + 5 * h), true));
        pathFigure.Segments.Add(
            new LineSegment(new Point(startPunkt.X - b, startPunkt.Y + 7 * h), true));
        pathFigure.Segments.Add(
            new LineSegment(new Point(startPunkt.X + b, startPunkt.Y + 9 * h), true));
        pathFigure.Segments.Add(
            new LineSegment(startPunkt with { Y = startPunkt.Y + 10 * h }, true));
        pathFigure.Segments.Add(
            new LineSegment(startPunkt with { Y = startPunkt.Y + 12 * h }, true));
        pathFigure.Segments.Add(
            new LineSegment(new Point(startPunkt.X - b, startPunkt.Y + 12 * h), false));
        pathFigure.Segments.Add(
            new LineSegment(new Point(startPunkt.X + b, startPunkt.Y + 12 * h), true));

        pathFigure.Segments.Add(
            new LineSegment(new Point(startPunkt.X + b - h, startPunkt.Y + 13 * h), true));
        pathFigure.Segments.Add(
            new LineSegment(new Point(startPunkt.X + b / 2, startPunkt.Y + 12 * h), false));
        pathFigure.Segments.Add(
            new LineSegment(new Point(startPunkt.X + b / 2 - h, startPunkt.Y + 13 * h), true));
        pathFigure.Segments.Add(
            new LineSegment(startPunkt with { Y = startPunkt.Y + 12 * h }, false));
        pathFigure.Segments.Add(
            new LineSegment(new Point(startPunkt.X - h, startPunkt.Y + 13 * h), true));
        pathFigure.Segments.Add(
            new LineSegment(new Point(startPunkt.X - b / 2, startPunkt.Y + 12 * h), false));
        pathFigure.Segments.Add(
            new LineSegment(new Point(startPunkt.X - b / 2 - h, startPunkt.Y + 13 * h), true));
        pathFigure.Segments.Add(
            new LineSegment(new Point(startPunkt.X - b, startPunkt.Y + 12 * h), false));
        pathFigure.Segments.Add(
            new LineSegment(new Point(startPunkt.X - b - h, startPunkt.Y + 13 * h), true));
    }

    private static void DrehfederZeichnen(PathFigure pathFigure, Point startPunkt)
    {
        const int b = 10;
        pathFigure.StartPoint = startPunkt;
        var zielPunkt = new Point(startPunkt.X - b, startPunkt.Y - b);
        pathFigure.Segments.Add(
            new ArcSegment(zielPunkt, new Size(b, b - 3), 200, true, 0, true));
        zielPunkt = startPunkt with { X = startPunkt.X + b };
        pathFigure.Segments.Add(
            new ArcSegment(zielPunkt, new Size(b, b + 2), 190, false, 0, true));
    }

    private PathGeometry FachwerkelementZeichnen(AbstraktElement element)
    {
        if (!_modell.Knoten.TryGetValue(element.KnotenIds[0], out _knoten))
            throw new ModellAusnahme("\nBiegebalken Elementknoten '" + element.KnotenIds[0] + "' nicht im Modell gefunden");

        var startPunkt = TransformKnoten(_knoten, Auflösung, MaxY);
        if (!_modell.Knoten.TryGetValue(element.KnotenIds[1], out _knoten))
            throw new ModellAusnahme("\nBiegebalken Elementknoten '" + element.KnotenIds[1] + "' nicht im Modell gefunden");

        var endPunkt = TransformKnoten(_knoten, Auflösung, MaxY);

        var pathGeometry = new PathGeometry();
        var pathFigure = new PathFigure { StartPoint = startPunkt };
        pathFigure.Segments.Add(new LineSegment(endPunkt, true));

        // Gelenk als Halbkreis am Startknoten des Fachwerkelementes zeichnen
        var direction = endPunkt - startPunkt;
        var start = RotateVectorScreen(direction, 90);
        start.Normalize();
        var zielPunkt = startPunkt + 5 * start;
        pathFigure.Segments.Add(new LineSegment(zielPunkt, false));
        var ziel = RotateVectorScreen(direction, -90);
        ziel.Normalize();
        zielPunkt = startPunkt + 5 * ziel;
        // ArcSegment beginnt am letzten Punkt der pathFigure
        // Zielpunkt, Größe in x,y, Öffnungswinkel, isLargeArc, sweepDirection, isStroked
        pathFigure.Segments.Add(new ArcSegment(zielPunkt, new Size(2.5, 2.5), 180, true, 0, true));
        pathFigure.Segments.Add(new LineSegment(startPunkt, false));

        // Gelenk als Halbkreis am Endknoten des Fachwerkelementes zeichnen
        direction = startPunkt - endPunkt;
        start = RotateVectorScreen(direction, -90);
        start.Normalize();
        zielPunkt = endPunkt + 5 * start;
        pathFigure.Segments.Add(new LineSegment(zielPunkt, false));
        var end = RotateVectorScreen(direction, 90);
        end.Normalize();
        zielPunkt = endPunkt + 5 * end;
        pathFigure.Segments.Add(new ArcSegment(zielPunkt, new Size(2.5, 2.5), 180, true, (SweepDirection)1, true));
        pathGeometry.Figures.Add(pathFigure);
        return pathGeometry;
    }

    private PathGeometry BiegebalkenZeichnen(AbstraktElement element)
    {
        if (!_modell.Knoten.TryGetValue(element.KnotenIds[0], out _knoten))
            throw new ModellAusnahme("\nBiegebalken Elementknoten '" + element.KnotenIds[0] + "' nicht im Modell gefunden");

        var startPunkt = TransformKnoten(_knoten, Auflösung, MaxY);
        if (!_modell.Knoten.TryGetValue(element.KnotenIds[1], out _knoten))
            throw new ModellAusnahme("\nBiegebalken Elementknoten '" + element.KnotenIds[1] + "' nicht im Modell gefunden");

        var endPunkt = TransformKnoten(_knoten, Auflösung, MaxY);

        var pathGeometry = new PathGeometry();
        var pathFigure = new PathFigure { StartPoint = startPunkt };
        pathFigure.Segments.Add(new LineSegment(endPunkt, true));

        pathGeometry.Figures.Add(pathFigure);
        return pathGeometry;
    }

    private PathGeometry BiegebalkenGelenkZeichnen(AbstraktElement element)
    {
        Vector direction, start;
        Point zielPunkt;

        if (!_modell.Knoten.TryGetValue(element.KnotenIds[0], out _knoten))
            throw new ModellAusnahme("\nBiegebalken Elementknoten '" + element.KnotenIds[0] + "' nicht im Modell gefunden");

        var startPunkt = TransformKnoten(_knoten, Auflösung, MaxY);
        if (!_modell.Knoten.TryGetValue(element.KnotenIds[1], out _knoten))
            throw new ModellAusnahme("\nBiegebalken Elementknoten '" + element.KnotenIds[1] + "' nicht im Modell gefunden");

        var endPunkt = TransformKnoten(_knoten, Auflösung, MaxY);

        var pathGeometry = new PathGeometry();
        var pathFigure = new PathFigure { StartPoint = startPunkt };
        pathFigure.Segments.Add(new LineSegment(endPunkt, true));

        // Gelenk am 1. Knoten des Biegebalken zeichnen
        if (element is BiegebalkenGelenk && element.Typ == 1)
        {
            direction = endPunkt - startPunkt;
            start = RotateVectorScreen(direction, 90);
            start.Normalize();
            zielPunkt = startPunkt + 5 * start;
            pathFigure.Segments.Add(new LineSegment(zielPunkt, false));
            var ziel = RotateVectorScreen(direction, -90);
            ziel.Normalize();
            zielPunkt = startPunkt + 5 * ziel;
            // ArcSegment beginnt am letzten Punkt der pathFigure
            // Zielpunkt, Größe in x,y, Öffnungswinkel, isLargeArc, sweepDirection, isStroked
            pathFigure.Segments.Add(new ArcSegment(zielPunkt, new Size(2.5, 2.5), 180, true, 0, true));
            pathFigure.Segments.Add(new LineSegment(startPunkt, false));
        }

        // Gelenk am 2. Knoten des Biegebalken zeichnen
        if (element is BiegebalkenGelenk && element.Typ == 2)
        {
            direction = startPunkt - endPunkt;
            start = RotateVectorScreen(direction, -90);
            start.Normalize();
            zielPunkt = endPunkt + 5 * start;
            pathFigure.Segments.Add(new LineSegment(zielPunkt, false));
            var end = RotateVectorScreen(direction, 90);
            end.Normalize();
            zielPunkt = endPunkt + 5 * end;
            pathFigure.Segments.Add(new ArcSegment(zielPunkt, new Size(2.5, 2.5), 180, true, (SweepDirection)1, true));
            pathFigure.Segments.Add(new LineSegment(endPunkt, false));
        }

        pathGeometry.Figures.Add(pathFigure);
        return pathGeometry;
    }

    private PathGeometry MultiKnotenElementZeichnen(AbstraktElement element)
    {
        var pathGeometry = new PathGeometry();
        var pathFigure = new PathFigure();
        if (!_modell.Knoten.TryGetValue(element.KnotenIds[0], out _knoten))
            throw new ModellAusnahme("\nBiegebalken Elementknoten '" + element.KnotenIds[0] + "' nicht im Modell gefunden");

        var startPoint = TransformKnoten(_knoten, Auflösung, MaxY);
        pathFigure.StartPoint = startPoint;
        for (var i = 1; i < element.KnotenIds.Length; i++)
        {
            if (!_modell.Knoten.TryGetValue(element.KnotenIds[i], out _knoten))
                throw new ModellAusnahme("\nBiegebalken Elementknoten '" + element.KnotenIds[i] + "' nicht im Modell gefunden");

            var nextPoint = TransformKnoten(_knoten, Auflösung, MaxY);
            pathFigure.Segments.Add(new LineSegment(nextPoint, true));
        }

        pathFigure.IsClosed = true;
        pathGeometry.Figures.Add(pathFigure);
        return pathGeometry;
    }

    public void ElementTexte()
    {
        foreach (var item in _modell.Elemente)
        {
            if (item.Value is not Abstrakt2D element) continue;
            element.SetzElementReferenzen(_modell);
            var cg = element.BerechneSchwerpunkt();
            var id = new TextBlock
            {
                FontSize = 12,
                Text = item.Key,
                Foreground = Blue
            };
            SetTop(id, (-cg.Y + MaxY) * Auflösung + PlatzierungV);
            SetLeft(id, cg.X * Auflösung + PlatzierungH);
            _visual.Children.Add(id);
            ElementIDs.Add(id);
        }
    }

    public void KnotenTexte()
    {
        foreach (var item in _modell.Knoten)
        {
            var id = new TextBlock
            {
                FontSize = 12,
                Text = item.Key,
                Foreground = Black
            };
            SetTop(id, (-item.Value.Koordinaten[1] + MaxY) * Auflösung + PlatzierungV);
            SetLeft(id, item.Value.Koordinaten[0] * Auflösung + PlatzierungH);
            _visual.Children.Add(id);
            KnotenIDs.Add(id);
        }
    }

    public void LastenZeichnen()
    {
        AbstraktLast last;
        Shape path;

        // Knotenlasten
        var maxLastWert = 1.0;
        const int maxLastScreen = 50;
        foreach (var item in _modell.Lasten)
        {
            last = item.Value;
            if (Math.Abs(last.Lastwerte[0]) > maxLastWert) maxLastWert = Math.Abs(last.Lastwerte[0]);
            if (Math.Abs(last.Lastwerte[1]) > maxLastWert) maxLastWert = Math.Abs(last.Lastwerte[1]);
        }

        foreach (var item in _modell.PunktLasten)
        {
            last = item.Value;
            if (Math.Abs(last.Lastwerte[0]) > maxLastWert) maxLastWert = Math.Abs(last.Lastwerte[0]);
            if (Math.Abs(last.Lastwerte[1]) > maxLastWert) maxLastWert = Math.Abs(last.Lastwerte[1]);
        }

        maxLastWert =
            (from linienLast in _modell.ElementLasten.Select(item => (AbstraktLinienlast)item.Value)
             from lastwert in linienLast.Lastwerte
             select Math.Abs(lastwert)).Prepend(maxLastWert).Max();
        _lastAuflösung = maxLastScreen / maxLastWert;

        foreach (var item in _modell.Lasten)
        {
            last = item.Value;
            last.LastId = item.Key;
            var pathGeometry = KnotenlastZeichnen(last);
            path = new Path
            {
                Name = last.LastId,
                Stroke = Red,
                StrokeThickness = 3,
                Data = pathGeometry
            };
            LastVektoren.Add(path);

            SetLeft(path, PlatzierungH);
            SetTop(path, PlatzierungV);
            _visual.Children.Add(path);
        }

        foreach (var item in _modell.PunktLasten)
        {
            var pathGeometry = PunktlastZeichnen(item.Value);
            path = new Path
            {
                Name = item.Key,
                Stroke = Red,
                StrokeThickness = 3,
                Data = pathGeometry
            };
            LastVektoren.Add(path);

            SetLeft(path, PlatzierungH);
            SetTop(path, PlatzierungV);
            _visual.Children.Add(path);
        }

        foreach (var item in _modell.ElementLasten)
        {
            var linienlast = (AbstraktLinienlast)item.Value;
            var pathGeometry = LinienlastZeichnen(linienlast);
            var rot = FromArgb(60, 255, 0, 0);
            var blau = FromArgb(60, 0, 0, 255);
            var myBrush = new SolidColorBrush(rot);
            if (linienlast.Lastwerte[1] > 0) myBrush = new SolidColorBrush(blau);
            path = new Path
            {
                Name = linienlast.LastId,
                Fill = myBrush,
                Stroke = Red,
                StrokeThickness = 1,
                Data = pathGeometry
            };
            LastVektoren.Add(path);

            SetLeft(path, PlatzierungH);
            SetTop(path, PlatzierungV);
            _visual.Children.Add(path);
        }

        foreach (var item in _modell.ZeitabhängigeKnotenLasten)
        {
            last = item.Value;
            last.LastId = item.Key;
            var test = (AbstraktKnotenlast)last;
            var dof = test.KnotenFreiheitsgrad;

            var lastwerte = new double[2];
            lastwerte[dof] = 1;
            last.Lastwerte = lastwerte;

            var pathGeometry = KnotenlastZeichnen(last);
            path = new Path
            {
                Name = last.LastId,
                Stroke = DarkRed,
                StrokeThickness = 3,
                Data = pathGeometry
            };
            LastVektoren.Add(path);

            SetLeft(path, PlatzierungH);
            SetTop(path, PlatzierungV);
            _visual.Children.Add(path);
        }
    }

    private PathGeometry KnotenlastZeichnen(AbstraktLast zeitKnotenlast)
    {
        var pathGeometry = new PathGeometry();
        var pathFigure = new PathFigure();
        const int lastPfeilGroesse = 10;

        if (zeitKnotenlast.KnotenId == "boden")
        {
            // finde Knoten mit y=0, um den Bodenknoten zu zeichnen
            foreach (var item in _modell.Knoten.
                         Where(item => item.Value.Koordinaten[1] == 0))
            {
                _knoten = item.Value;
                var knotenlast = (AbstraktKnotenlast)zeitKnotenlast;
                knotenlast.Knoten = _knoten;
                break;
            }
        }
        else if (!_modell.Knoten.TryGetValue(zeitKnotenlast.KnotenId, out _knoten))
            throw new ModellAusnahme("\nBiegebalken Knotenlast Knoten '" + zeitKnotenlast.KnotenId + "' nicht im Modell gefunden");

        if (_knoten != null)
        {
            var endPoint = new Point(_knoten.Koordinaten[0] * Auflösung - zeitKnotenlast.Lastwerte[0] * _lastAuflösung,
                (-_knoten.Koordinaten[1] + MaxY) * Auflösung + zeitKnotenlast.Lastwerte[1] * _lastAuflösung);
            pathFigure.StartPoint = endPoint;

            var startPoint = TransformKnoten(_knoten, Auflösung, MaxY);
            pathFigure.Segments.Add(new LineSegment(startPoint, true));

            var vector = startPoint - endPoint;
            vector.Normalize();
            vector *= lastPfeilGroesse;
            vector = RotateVectorScreen(vector, 30);
            endPoint = new Point(startPoint.X - vector.X, startPoint.Y - vector.Y);
            pathFigure.Segments.Add(new LineSegment(endPoint, true));

            vector = RotateVectorScreen(vector, -60);
            endPoint = new Point(startPoint.X - vector.X, startPoint.Y - vector.Y);
            pathFigure.Segments.Add(new LineSegment(endPoint, false));
            pathFigure.Segments.Add(new LineSegment(startPoint, true));

            if (zeitKnotenlast.Lastwerte.Length > 2 && Math.Abs(zeitKnotenlast.Lastwerte[2]) > double.Epsilon)
            {
                startPoint.X += 30;
                pathFigure.Segments.Add(new LineSegment(startPoint, false));
                startPoint.X -= 30;
                startPoint.Y += 30;
                pathFigure.Segments.Add(new ArcSegment
                    (startPoint, new Size(30, 30), 270, true, new SweepDirection(), true));

                vector = new Vector(1, 0);
                vector *= lastPfeilGroesse;
                vector = RotateVectorScreen(vector, 45);
                endPoint = new Point(startPoint.X - vector.X, startPoint.Y - vector.Y);
                pathFigure.Segments.Add(new LineSegment(endPoint, true));

                vector = RotateVectorScreen(vector, -60);
                endPoint = new Point(startPoint.X - vector.X, startPoint.Y - vector.Y);
                pathFigure.Segments.Add(new LineSegment(endPoint, false));
                pathFigure.Segments.Add(new LineSegment(startPoint, true));
            }
        }

        pathGeometry.Figures.Add(pathFigure);
        return pathGeometry;
    }

    private PathGeometry PunktlastZeichnen(AbstraktElementLast last)
    {
        var punktlast = (PunktLast)last;
        var pathGeometry = new PathGeometry();
        var pathFigure = new PathFigure();
        const int lastPfeilGrösse = 10;

        punktlast.SetzElementlastReferenzen(_modell);
        if (!_modell.Elemente.TryGetValue(punktlast.ElementId, out var element))
        {
            throw new ModellAusnahme("\nPunktlast Element '" + punktlast.ElementId + "' nicht im Modell gefunden");
        }

        if (element == null) return pathGeometry;
        if (!_modell.Knoten.TryGetValue(element.KnotenIds[0], out _knoten))
        {
            throw new ModellAusnahme("\nPunktlast Element Knoten '" + element.KnotenIds[0] + "' nicht im Modell gefunden");
        }

        var startPunkt = TransformKnoten(_knoten, Auflösung, MaxY);

        // zweiter Elementknoten 
        if (!_modell.Knoten.TryGetValue(element.KnotenIds[1], out _knoten))
        {
            throw new ModellAusnahme("\nPunktlast Element Knoten '" + element.KnotenIds[1] + "' nicht im Modell gefunden");
        }

        var endPunkt = TransformKnoten(_knoten, Auflösung, MaxY);

        var vector = new Vector(endPunkt.X, endPunkt.Y) - new Vector(startPunkt.X, startPunkt.Y);
        var lastPunkt = (Point)(punktlast.Offset * vector);

        lastPunkt.X = startPunkt.X + lastPunkt.X;
        lastPunkt.Y = startPunkt.Y + lastPunkt.Y;

        endPunkt = new Point(lastPunkt.X - punktlast.Lastwerte[0] * _lastAuflösung,
            lastPunkt.Y + punktlast.Lastwerte[1] * _lastAuflösung);
        pathFigure.StartPoint = endPunkt;

        pathFigure.Segments.Add(new LineSegment(lastPunkt, true));

        vector = lastPunkt - endPunkt;
        vector.Normalize();
        vector *= lastPfeilGrösse;
        vector = RotateVectorScreen(vector, 30);
        endPunkt = new Point(lastPunkt.X - vector.X, lastPunkt.Y - vector.Y);
        pathFigure.Segments.Add(new LineSegment(endPunkt, true));

        vector = RotateVectorScreen(vector, -60);
        endPunkt = new Point(lastPunkt.X - vector.X, lastPunkt.Y - vector.Y);
        pathFigure.Segments.Add(new LineSegment(endPunkt, false));
        pathFigure.Segments.Add(new LineSegment(lastPunkt, true));

        pathGeometry.Figures.Add(pathFigure);
        return pathGeometry;
    }

    private PathGeometry LinienlastZeichnen(AbstraktElementLast last)
    {
        var linienlast = (LinienLast)last;
        var pathGeometry = new PathGeometry();
        var pathFigure = new PathFigure();
        const int lastPfeilGroesse = 8;
        const int linienkraftÜberhöhung = 1;
        var linienLastAuflösung = linienkraftÜberhöhung * _lastAuflösung;

        last.SetzElementlastReferenzen(_modell);
        if (!_modell.Elemente.TryGetValue(linienlast.ElementId, out var element))
        {
            throw new ModellAusnahme("\nLinienlast Element '" + linienlast.ElementId + "' nicht im Modell gefunden");
        }

        if (!_modell.Knoten.TryGetValue(element.KnotenIds[0], out _knoten))
        {
            throw new ModellAusnahme("\nLinienlast Element Knoten '" + element.KnotenIds[0] + "' nicht im Modell gefunden");
        }

        var startPunkt = TransformKnoten(_knoten, Auflösung, MaxY);

        // zweiter Elementknoten 
        if (!_modell.Knoten.TryGetValue(element.KnotenIds[1], out _knoten))
        {
            throw new ModellAusnahme("\nLinienlast Element Knoten '" + element.KnotenIds[1] + "' nicht im Modell gefunden");
        }

        var endPunkt = TransformKnoten(_knoten, Auflösung, MaxY);
        var vector = endPunkt - startPunkt;

        // Startpunkt und Lastpunkt am Anfang
        pathFigure.StartPoint = startPunkt;
        var lastVektor = RotateVectorScreen(vector, -90);
        lastVektor.Normalize();
        var vec = lastVektor * linienLastAuflösung * linienlast.Lastwerte[1];
        var nextPunkt = new Point(startPunkt.X - vec.X, startPunkt.Y - vec.Y);

        if (Math.Abs(vec.Length) > double.Epsilon)
        {
            // Lastpfeil am Anfang
            lastVektor *= lastPfeilGroesse;
            lastVektor = RotateVectorScreen(lastVektor, -150);
            var punkt = new Point(startPunkt.X - lastVektor.X, startPunkt.Y - lastVektor.Y);
            pathFigure.Segments.Add(new LineSegment(punkt, true));
            lastVektor = RotateVectorScreen(lastVektor, -60);
            punkt = new Point(startPunkt.X - lastVektor.X, startPunkt.Y - lastVektor.Y);
            pathFigure.Segments.Add(new LineSegment(punkt, false));
            pathFigure.Segments.Add(new LineSegment(startPunkt, true));

            // Linie vom Startpunkt zum Lastanfang
            pathFigure.Segments.Add(new LineSegment(nextPunkt, true));
        }

        // Linie zum Lastende
        lastVektor = RotateVectorScreen(vector, 90);
        lastVektor.Normalize();
        vec = lastVektor * linienLastAuflösung * linienlast.Lastwerte[3];
        nextPunkt = new Point(endPunkt.X + vec.X, endPunkt.Y + vec.Y);
        pathFigure.Segments.Add(new LineSegment(nextPunkt, true));

        // Linie zum Endpunkt
        pathFigure.Segments.Add(new LineSegment(endPunkt, true));

        if (Math.Abs(vec.Length) > double.Epsilon)
        {
            // Lastpfeil am Ende
            lastVektor *= lastPfeilGroesse;
            lastVektor = RotateVectorScreen(lastVektor, 30);
            nextPunkt = new Point(endPunkt.X - lastVektor.X, endPunkt.Y - lastVektor.Y);
            pathFigure.Segments.Add(new LineSegment(nextPunkt, true));
            lastVektor = RotateVectorScreen(lastVektor, -60);
            nextPunkt = new Point(endPunkt.X - lastVektor.X, endPunkt.Y - lastVektor.Y);
            pathFigure.Segments.Add(new LineSegment(nextPunkt, false));
            pathFigure.Segments.Add(new LineSegment(endPunkt, true));
        }

        // schließ pathFigure zum Füllen
        pathFigure.IsClosed = true;
        pathGeometry.Figures.Add(pathFigure);
        return pathGeometry;
    }

    public void LastTexte()
    {
        foreach (var item in _modell.Lasten)
        {
            if (item.Value is null) continue;
            var id = new TextBlock
            {
                FontSize = 12,
                Text = item.Key,
                Foreground = Red
            };
            if (!_modell.Knoten.TryGetValue(item.Value.KnotenId, out var lastKnoten))
            {
                throw new ModellAusnahme("\nBiegebalken Lastknoten '" + item.Value.KnotenId + "' nicht im Modell gefunden");
            }
            _platzierungText = TransformKnoten(lastKnoten, Auflösung, MaxY);
            const int knotenOffset = 20;
            SetTop(id, _platzierungText.Y + PlatzierungV - knotenOffset);
            SetLeft(id, _platzierungText.X + PlatzierungH);
            _visual.Children.Add(id);
            LastIDs.Add(id);
        }

        foreach (var item in _modell.ElementLasten.Where(item => item.Value is LinienLast))
        {
            const int elementOffset = -20;

            var id = new TextBlock
            {
                FontSize = 12,
                Text = item.Key,
                Foreground = Red
            };
            var platzierung = ((Vector)TransformKnoten(item.Value.Element.Knoten[0], Auflösung, MaxY)
                              + (Vector)TransformKnoten(item.Value.Element.Knoten[1], Auflösung, MaxY)) / 2;
            _platzierungText = (Point)platzierung;
            SetTop(id, _platzierungText.Y + PlatzierungV + elementOffset);
            SetLeft(id, _platzierungText.X + PlatzierungH);
            _visual.Children.Add(id);
            LastIDs.Add(id);
        }

        foreach (var item in _modell.PunktLasten)
        {
            if (item.Value is not PunktLast last) continue;
            var id = new TextBlock
            {
                FontSize = 12,
                Text = item.Key,
                Foreground = Red
            };

            var startPoint = TransformKnoten(last.Element.Knoten[0], Auflösung, MaxY);
            var endPoint = TransformKnoten(last.Element.Knoten[1], Auflösung, MaxY);
            _platzierungText = startPoint + (endPoint - startPoint) * last.Offset;
            const int knotenOffset = 15;
            SetTop(id, _platzierungText.Y + PlatzierungV + knotenOffset);
            SetLeft(id, _platzierungText.X + PlatzierungH);
            _visual.Children.Add(id);
            LastIDs.Add(id);
        }

        foreach (var item in _modell.ZeitabhängigeKnotenLasten)
        {
            if (item.Value is null) continue;
            var id = new TextBlock
            {
                FontSize = 12,
                Text = item.Key,
                Foreground = DarkRed
            };
            Knoten lastKnoten;
            if (item.Value.KnotenId == "boden")
            {
                lastKnoten = item.Value.Knoten;
            }
            else if (!_modell.Knoten.TryGetValue(item.Value.KnotenId, out lastKnoten))
            {
                throw new ModellAusnahme("\nBiegebalken Lastknoten '" + item.Value.KnotenId + "' nicht im Modell gefunden");
            }
            _platzierungText = TransformKnoten(lastKnoten, Auflösung, MaxY);
            const int knotenOffset = 20;
            SetTop(id, _platzierungText.Y + PlatzierungV - knotenOffset);
            SetLeft(id, _platzierungText.X + PlatzierungH);
            _visual.Children.Add(id);
            LastIDs.Add(id);
        }
    }

    public void LagerZeichnen()
    {
        foreach (var item in _modell.Randbedingungen)
        {
            var lager = item.Value;
            PathGeometry pathGeometry;

            if (!_modell.Knoten.TryGetValue(lager.KnotenId, out var lagerKnoten))
            {
                throw new ModellAusnahme("\nLinienlast Element Knoten '" + lager.KnotenId + "' nicht im Modell gefunden");
            }

            var drehPunkt = TransformKnoten(lagerKnoten, Auflösung, MaxY);
            double drehWinkel = 0;
            bool links = false, unten = true, rechts = false, balken = false;

            if (lagerKnoten != null)
            {
                // check, ob das Modell ein horizontaler Balken oder Durchlaufträger ist
                if (Math.Abs(MaxY - _minY) < double.Epsilon)
                {
                    balken = true;
                    // Einspannung links
                    if (Math.Abs(lagerKnoten.Koordinaten[0] - _minX) < double.Epsilon
                        && lager.Typ > 4)
                    {
                        links = true;
                        unten = false;
                    }
                    // Einspannung rechts
                    else if (Math.Abs(lagerKnoten.Koordinaten[0] - _maxX) < double.Epsilon
                             && lager.Typ > 4)
                    {
                        rechts = true;
                        unten = false;
                    }
                }
                else
                {
                    // horizontale Festhaltungen oberer Koordinaten
                    if (Math.Abs(lagerKnoten.Koordinaten[0] - _minX) < double.Epsilon
                        && Math.Abs(lagerKnoten.Koordinaten[1] - _minY) > double.Epsilon)
                    {
                        links = true;
                        unten = false;
                    }
                    else if (Math.Abs(lagerKnoten.Koordinaten[0] - _maxX) < double.Epsilon
                             && Math.Abs(lagerKnoten.Koordinaten[1] - _minY) > double.Epsilon)
                    {
                        rechts = true;
                        unten = false;
                    }
                }

                //foreach (var element in _modell.Elemente)
                //{
                //    if (lager.KnotenId != element.Value.KnotenIds[0] &&
                //        lager.KnotenId != element.Value.KnotenIds[1]) continue;

                //    var p1 = TransformKnoten(element.Value.Knoten[0], Auflösung, MaxY);
                //    var p2 = TransformKnoten(element.Value.Knoten[1], Auflösung, MaxY);
                //    var winkel = Vector.AngleBetween(p2-p1, new Vector(1, 0));
                //    if (winkel == 0)
                //    {
                //        if (lager.Typ > 4) links = true;
                //        else unten = true;
                //    }
                //    if (winkel is > 10 and < 170) unten = true;
                //    if (winkel is > 100 and < 260) unten = true;
                //}
            }

            switch (lager.Typ)
            {
                // X_FIXED = 1, Y_FIXED = 2, XY_FIXED = 3, XYR_FIXED = 7
                // R_FIXED = 4, XR_FIXED = 5, YR_FIXED = 6 werden in Balkentheorie nicht dargestellt
                case 1:
                    {
                        pathGeometry = EineFesthaltungZeichnen(lagerKnoten);
                        if (links) drehWinkel = 90;
                        else if (rechts) drehWinkel = -90;
                        pathGeometry.Transform = new RotateTransform(drehWinkel, drehPunkt.X, drehPunkt.Y);
                        break;
                    }
                case 2:
                    pathGeometry = EineFesthaltungZeichnen(lagerKnoten);
                    break;
                case 3:
                    pathGeometry = ZweiFesthaltungenZeichnen(lagerKnoten);
                    if (links && !balken) drehWinkel = 90;
                    else if (rechts) drehWinkel = -90;
                    if (unten && !balken) drehWinkel = 0;
                    pathGeometry.Transform = new RotateTransform(drehWinkel, drehPunkt.X, drehPunkt.Y);
                    break;
                case 7:
                    {
                        pathGeometry = DreiFesthaltungenZeichnen(lagerKnoten);
                        if (links) drehWinkel = 90;
                        else if (rechts) drehWinkel = -90;
                        if (unten && !balken) drehWinkel = 0;
                        pathGeometry.Transform = new RotateTransform(drehWinkel, drehPunkt.X, drehPunkt.Y);
                        break;
                    }
                default:
                    throw new ModellAusnahme("\nungültige Lagerbedingung für Lager '" + lager.RandbedingungId);
            }

            Shape path = new Path
            {
                Name = lager.RandbedingungId,
                Stroke = Green,
                StrokeThickness = 2,
                Data = pathGeometry
            };
            LagerDarstellung.Add(path);

            // setz oben/links Position zum Zeichnen auf dem Canvas
            SetLeft(path, PlatzierungH);
            SetTop(path, PlatzierungV);
            // zeichne Shape
            _visual.Children.Add(path);
        }
    }

    private PathGeometry EineFesthaltungZeichnen(Knoten lagerKnoten)
    {
        var pathGeometry = new PathGeometry();
        var pathFigure = new PathFigure();
        const int lagerSymbol = 20;

        var startPoint = TransformKnoten(lagerKnoten, Auflösung, MaxY);
        pathFigure.StartPoint = startPoint;

        var endPoint = new Point(startPoint.X - lagerSymbol, startPoint.Y + lagerSymbol);
        pathFigure.Segments.Add(new LineSegment(endPoint, true));
        endPoint = new Point(endPoint.X + 2 * lagerSymbol, startPoint.Y + lagerSymbol);
        pathFigure.Segments.Add(new LineSegment(endPoint, true));
        pathFigure.Segments.Add(new LineSegment(startPoint, true));

        startPoint = new Point(endPoint.X + 5, endPoint.Y + 5);
        pathFigure.Segments.Add(new LineSegment(startPoint, false));
        endPoint = startPoint with { X = startPoint.X - 50 };
        pathFigure.Segments.Add(new LineSegment(endPoint, true));

        pathGeometry.Figures.Add(pathFigure);
        return pathGeometry;
    }

    private PathGeometry ZweiFesthaltungenZeichnen(Knoten lagerKnoten)
    {
        var pathGeometry = new PathGeometry();
        var pathFigure = new PathFigure();
        const int lagerSymbol = 20;

        var startPoint = TransformKnoten(lagerKnoten, Auflösung, MaxY);
        pathFigure.StartPoint = startPoint;

        var endPoint = new Point(startPoint.X - lagerSymbol, startPoint.Y + lagerSymbol);
        pathFigure.Segments.Add(new LineSegment(endPoint, true));
        endPoint = new Point(endPoint.X + 2 * lagerSymbol, startPoint.Y + lagerSymbol);
        pathFigure.Segments.Add(new LineSegment(endPoint, true));
        pathFigure.Segments.Add(new LineSegment(startPoint, true));

        startPoint = endPoint;
        pathFigure.Segments.Add(new LineSegment(startPoint, false));
        endPoint = new Point(startPoint.X - 5, startPoint.Y + 5);
        pathFigure.Segments.Add(new LineSegment(endPoint, true));

        pathFigure.Segments.Add(new LineSegment(startPoint with { X = startPoint.X - 10 }, false));
        pathFigure.Segments.Add(new LineSegment(endPoint with { X = endPoint.X - 10 }, true));

        pathFigure.Segments.Add(new LineSegment(startPoint with { X = startPoint.X - 20 }, false));
        pathFigure.Segments.Add(new LineSegment(endPoint with { X = endPoint.X - 20 }, true));

        pathFigure.Segments.Add(new LineSegment(startPoint with { X = startPoint.X - 30 }, false));
        pathFigure.Segments.Add(new LineSegment(endPoint with { X = endPoint.X - 30 }, true));

        pathFigure.Segments.Add(new LineSegment(startPoint with { X = startPoint.X - 40 }, false));
        pathFigure.Segments.Add(new LineSegment(endPoint with { X = endPoint.X - 40 }, true));

        pathGeometry.Figures.Add(pathFigure);
        return pathGeometry;
    }

    private PathGeometry DreiFesthaltungenZeichnen(Knoten lagerKnoten)
    {
        var pathGeometry = new PathGeometry();
        var pathFigure = new PathFigure();
        const int lagerSymbol = 20;

        var startPoint = TransformKnoten(lagerKnoten, Auflösung, MaxY);

        startPoint = startPoint with { X = startPoint.X - lagerSymbol };
        pathFigure.StartPoint = startPoint;
        var endPoint = startPoint with { X = startPoint.X + 2 * lagerSymbol };
        pathFigure.Segments.Add(new LineSegment(endPoint, true));
        pathGeometry.Figures.Add(pathFigure);
        pathFigure = new PathFigure
        {
            StartPoint = startPoint
        };
        endPoint = new Point(startPoint.X - 10, startPoint.Y + 10);
        pathFigure.Segments.Add(new LineSegment(endPoint, true));
        pathGeometry.Figures.Add(pathFigure);
        for (var i = 0; i < 4; i++)
        {
            pathFigure = new PathFigure();
            startPoint = startPoint with { X = startPoint.X + 10 };
            pathFigure.StartPoint = startPoint;
            endPoint = new Point(startPoint.X - 10, startPoint.Y + 10);
            pathFigure.Segments.Add(new LineSegment(endPoint, true));
            pathGeometry.Figures.Add(pathFigure);
        }

        return pathGeometry;
    }

    public void LagerTexte()
    {
        foreach (var item in _modell.Randbedingungen)
        {
            if (item.Value is not Lager) continue;
            var id = new TextBlock
            {
                FontSize = 12,
                Text = item.Key,
                Foreground = Green
            };
            item.Value.SetzRandbedingungenReferenzen(_modell);
            _platzierungText = TransformKnoten(item.Value.Knoten, Auflösung, MaxY);
            const int supportSymbol = 25;
            SetTop(id, _platzierungText.Y + PlatzierungV + supportSymbol);
            SetLeft(id, _platzierungText.X + PlatzierungH);
            _visual.Children.Add(id);
            LagerIDs.Add(id);
        }
    }

    //public void AnfangsbedingungenZeichnen(string knotenId, double knotenwert, string anf)
    //{
    //    const int randOffset = 15;
    //    // zeichne den Wert einer Anfangsbedingung als Text an Knoten

    //    if (_modell.Knoten.TryGetValue(knotenId, out _knoten)) { }
    //    var fensterKnoten = TransformKnoten(_knoten, Auflösung, MaxY);

    //    var anfangsbedingung = new TextBlock
    //    {
    //        Name = "Anfangsbedingung",
    //        Uid = anf,
    //        FontSize = 12,
    //        Text = knotenwert.ToString("N2"),
    //        Foreground = Black,
    //        Background = Turquoise
    //    };
    //    SetTop(anfangsbedingung, fensterKnoten. Y + RandOben + randOffset);
    //    SetLeft(anfangsbedingung, fensterKnoten. X + RandLinks);
    //    _visual.Children.Add(anfangsbedingung);
    //    Anfangsbedingungen.Add(anfangsbedingung);
    //}
    //public void AnfangsbedingungenEntfernen()
    //{
    //    foreach (var item in Anfangsbedingungen) _visual.Children.Remove(item);
    //    Anfangsbedingungen.Clear();
    //}

    //public void Beschleunigungen_Zeichnen()
    //{
    //    var fensterPunkt = new int[2];
    //    var beschleunigungAuflösung = 0.5;
    //    foreach (var item in modell.Knoten)
    //    {
    //        knoten = item.Value;
    //        var pathGeometry = new PathGeometry();
    //        var pathFigure = new PathFigure();
    //        var verformt = TransformVerformtenKnoten(knoten, auflösung, maxY);
    //        pathFigure.StartPoint = verformt;

    //        fensterPunkt[0] = (int)(verformt.X - item.Value.NodalDerivatives[0][zeitschritt] * beschleunigungAuflösung);
    //        fensterPunkt[1] = (int)(verformt.Y + item.Value.NodalDerivatives[1][zeitschritt] * beschleunigungAuflösung);

    //        var beschleunigung = new Point(fensterPunkt[0], fensterPunkt[1]);
    //        pathFigure.Segments.Add(new LineSegment(beschleunigung, true));

    //        pathGeometry.Figures.Add(pathFigure);
    //        Shape path = new Path()
    //        {
    //            Stroke = Blue,
    //            StrokeThickness = 2,
    //            Data = pathGeometry
    //        };
    //        SetLeft(path, randLinks);
    //        SetTop(path, randOben);
    //        visualErgebnisse.Children.Add(path);
    //        Beschleunigungen.Add(path);
    //    }
    //}

    public void Normalkraft_Zeichnen(AbstraktBalken element, double maxNormalkraft, bool elementlast)
    {
        var normalkraft1Skaliert = element.ElementZustand[0] / maxNormalkraft * MaxNormalkraftScreen;
        double normalkraft2Skaliert;
        if (element.ElementZustand.Length == 2)
            normalkraft2Skaliert = element.ElementZustand[1] / maxNormalkraft * MaxNormalkraftScreen;
        else
            normalkraft2Skaliert = element.ElementZustand[3] / maxNormalkraft * MaxNormalkraftScreen;

        Point nextPoint;
        Vector vec, vec2;
        var rot = FromArgb(120, 255, 0, 0);
        var blau = FromArgb(120, 0, 0, 255);

        if (!_modell.Knoten.TryGetValue(element.KnotenIds[0], out _knoten))
        {
            throw new ModellAusnahme("\nElement Knoten '" + element.KnotenIds[0] + "' nicht im Modell gefunden");
        }

        var startPoint = TransformKnoten(_knoten, Auflösung, MaxY);

        if (!_modell.Knoten.TryGetValue(element.KnotenIds[1], out _knoten))
        {
            throw new ModellAusnahme("\nElement Knoten '" + element.KnotenIds[1] + "' nicht im Modell gefunden");
        }

        var endPoint = TransformKnoten(_knoten, Auflösung, MaxY);

        if (!elementlast)
        {
            var pathGeometry = new PathGeometry();
            var pathFigure = new PathFigure();

            var myBrush = new SolidColorBrush(blau);
            if (normalkraft1Skaliert < 0) myBrush = new SolidColorBrush(rot);

            pathFigure.StartPoint = startPoint;
            vec = endPoint - startPoint;
            vec.Normalize();
            vec2 = RotateVectorScreen(vec, -90);
            nextPoint = startPoint + vec2 * normalkraft1Skaliert;
            pathFigure.Segments.Add(new LineSegment(nextPoint, true));
            nextPoint = endPoint + vec2 * normalkraft2Skaliert;
            pathFigure.Segments.Add(new LineSegment(nextPoint, true));
            pathFigure.Segments.Add(new LineSegment(endPoint, true));
            pathFigure.IsClosed = true;
            pathGeometry.Figures.Add(pathFigure);

            Shape path = new Path
            {
                Fill = myBrush,
                Stroke = Black,
                StrokeThickness = 1,
                Data = pathGeometry
            };
            SetLeft(path, PlatzierungH);
            SetTop(path, PlatzierungV);
            _visual.Children.Add(path);
            NormalkraftListe.Add(path);
        }
        else
        {
            // Anteil einer Punktlast
            double punktLastN = 0, punktLastO = 0;

            IEnumerable<PunktLast> PunktLasten()
            {
                foreach (var last in _modell.PunktLasten.Select(item => (PunktLast)item.Value)
                             .Where(last => last.ElementId == element.ElementId))
                    yield return last;
            }

            foreach (var punktLast in PunktLasten())
            {
                punktLastN = punktLast.Lastwerte[0];
                punktLastO = punktLast.Offset;
            }

            // Anteil einer Linienlast
            IEnumerable<LinienLast> LinienLasten()
            {
                foreach (var item in _modell.ElementLasten)
                    if (item.Value is LinienLast linienLast && item.Value.ElementId == element.ElementId)
                        yield return linienLast;
            }

            foreach (var linienLast in LinienLasten())
            {
                var pathGeometry = new PathGeometry();
                var pathFigure = new PathFigure();

                var myBrush = new SolidColorBrush(blau);
                if (normalkraft1Skaliert < 0) myBrush = new SolidColorBrush(rot);

                pathFigure.StartPoint = startPoint;
                vec = endPoint - startPoint;
                vec.Normalize();
                vec2 = RotateVectorScreen(vec, -90);
                nextPoint = startPoint + vec2 * normalkraft1Skaliert;
                pathFigure.Segments.Add(new LineSegment(nextPoint, true));

                if (punktLastO > double.Epsilon)
                {
                    nextPoint += punktLastO * (endPoint - startPoint);

                    var na = linienLast.Lastwerte[0];
                    var nb = linienLast.Lastwerte[2];
                    var konstant = na * punktLastO * element.BalkenLänge;
                    var linear = (nb - na) * punktLastO / 2 * element.BalkenLänge;
                    if (nb < na)
                    {
                        konstant = nb * punktLastO * element.BalkenLänge;
                        linear = (na - nb) * (1 - punktLastO) / 2 * element.BalkenLänge;
                    }

                    nextPoint += vec2 * (konstant + linear) / maxNormalkraft * MaxNormalkraftScreen;
                    pathFigure.Segments.Add(new LineSegment(nextPoint, true));
                    nextPoint += vec2 * punktLastN / maxNormalkraft * MaxNormalkraftScreen;
                    pathFigure.Segments.Add(new LineSegment(nextPoint, true));
                }

                nextPoint = endPoint - vec2 * normalkraft2Skaliert;
                pathFigure.Segments.Add(new LineSegment(nextPoint, true));
                pathFigure.Segments.Add(new LineSegment(endPoint, true));
                pathFigure.IsClosed = true;
                pathGeometry.Figures.Add(pathFigure);

                Shape path = new Path
                {
                    Fill = myBrush,
                    Stroke = Black,
                    StrokeThickness = 1,
                    Data = pathGeometry
                };
                SetLeft(path, PlatzierungH);
                SetTop(path, PlatzierungV);
                _visual.Children.Add(path);
                NormalkraftListe.Add(path);
            }
        }
    }

    public void Querkraft_Zeichnen(AbstraktBalken element, double maxQuerkraft, bool elementlast)
    {
        if (element is Fachwerk) return;
        var querkraft1Skaliert = element.ElementZustand[1] / maxQuerkraft * MaxQuerkraftScreen;
        var querkraft2Skaliert = element.ElementZustand[4] / maxQuerkraft * MaxQuerkraftScreen;

        Point nextPoint;
        Vector vec, vec2;
        var rot = FromArgb(120, 255, 0, 0);
        var blau = FromArgb(120, 0, 0, 255);
        SolidColorBrush myBrush;

        if (!_modell.Knoten.TryGetValue(element.KnotenIds[0], out _knoten))
        {
            throw new ModellAusnahme("\nElement Knoten '" + element.KnotenIds[0] + "' nicht im Modell gefunden");
        }
        var startPoint = TransformKnoten(_knoten, Auflösung, MaxY);

        if (!_modell.Knoten.TryGetValue(element.KnotenIds[1], out _knoten))
        {
            throw new ModellAusnahme("\nElement Knoten '" + element.KnotenIds[1] + "' nicht im Modell gefunden");
        }
        var endPoint = TransformKnoten(_knoten, Auflösung, MaxY);

        if (!elementlast)
        {
            var pathGeometry = new PathGeometry();
            var pathFigure = new PathFigure();

            myBrush = new SolidColorBrush(blau);
            if (querkraft1Skaliert < 0) myBrush = new SolidColorBrush(rot);

            pathFigure.StartPoint = startPoint;
            vec = endPoint - startPoint;
            vec.Normalize();
            vec2 = RotateVectorScreen(vec, -90);
            nextPoint = startPoint + vec2 * querkraft1Skaliert;
            pathFigure.Segments.Add(new LineSegment(nextPoint, true));
            nextPoint = endPoint + vec2 * querkraft1Skaliert;
            pathFigure.Segments.Add(new LineSegment(nextPoint, true));
            pathFigure.Segments.Add(new LineSegment(endPoint, true));
            pathFigure.IsClosed = true;
            pathGeometry.Figures.Add(pathFigure);

            Shape path = new Path
            {
                Fill = myBrush,
                Stroke = Black,
                StrokeThickness = 1,
                Data = pathGeometry
            };
            SetLeft(path, PlatzierungH);
            SetTop(path, PlatzierungV);
            _visual.Children.Add(path);
            QuerkraftListe.Add(path);
        }
        // Element hat 1 Punkt- und/oder 1 Linienlast
        else
        {
            // test, ob element Punktlast hat
            bool balkenPunktlast = false, balkenGleichlast = false;
            double punktLastQ = 0, punktLastO = 0;
            AbstraktElementLast linienLast = null;

            foreach (var item in _modell.PunktLasten)
            {
                if (item.Value is not PunktLast last || item.Value.ElementId != element.ElementId) continue;
                balkenPunktlast = true;
                punktLastQ = last.Lastwerte[1];
                punktLastO = last.Offset;
                break;
            }

            // test, ob element Linienlast hat
            foreach (var item in _modell.ElementLasten)
            {
                if (item.Value is not LinienLast last || item.Value.ElementId != element.ElementId) continue;
                balkenGleichlast = true;
                linienLast = last;
                break;
            }

            // nur Punktlast auf dem Balken und keine Gleichlast
            if (balkenPunktlast && !balkenGleichlast)
            {
                var pathGeometry = new PathGeometry();
                var pathFigure = new PathFigure { StartPoint = startPoint };

                // Querkraftlinie vom Start- bis zum Lastangriffspunkt
                myBrush = new SolidColorBrush(blau);
                if (querkraft1Skaliert < 0) myBrush = new SolidColorBrush(rot);

                vec = endPoint - startPoint;
                vec.Normalize();
                vec2 = RotateVectorScreen(vec, -90);
                nextPoint = startPoint + vec2 * querkraft1Skaliert;
                pathFigure.Segments.Add(new LineSegment(nextPoint, true));

                nextPoint += punktLastO * (endPoint - startPoint);
                pathFigure.Segments.Add(new LineSegment(nextPoint, true));

                //startPoint += punktLastO * (endPoint - startPoint);
                nextPoint = startPoint + punktLastO * (endPoint - startPoint);
                pathFigure.Segments.Add(new LineSegment(nextPoint, true));
                pathFigure.IsClosed = true;
                pathGeometry.Figures.Add(pathFigure);
                Shape path = new Path
                {
                    Fill = myBrush,
                    Stroke = Black,
                    StrokeThickness = 1,
                    Data = pathGeometry
                };
                SetLeft(path, PlatzierungH);
                SetTop(path, PlatzierungV);
                _visual.Children.Add(path);
                QuerkraftListe.Add(path);

                // Querkraftlinie vom Lastangriffs- bis zum Endpunkt
                pathGeometry = new PathGeometry();
                myBrush = new SolidColorBrush(blau);
                if (querkraft2Skaliert < 0) myBrush = new SolidColorBrush(rot);
                startPoint = startPoint + punktLastO * (endPoint - startPoint);
                pathFigure = new PathFigure
                {
                    StartPoint = startPoint,
                    IsClosed = true
                };

                nextPoint = startPoint + vec2 * querkraft2Skaliert;
                pathFigure.Segments.Add(new LineSegment(nextPoint, true));
                nextPoint = endPoint + vec2 * querkraft2Skaliert;
                pathFigure.Segments.Add(new LineSegment(nextPoint, true));
                pathFigure.Segments.Add(new LineSegment(endPoint, true));
                pathGeometry.Figures.Add(pathFigure);

                path = new Path
                {
                    Fill = myBrush,
                    Stroke = Black,
                    StrokeThickness = 1,
                    Data = pathGeometry
                };
                SetLeft(path, PlatzierungH);
                SetTop(path, PlatzierungV);
                _visual.Children.Add(path);
                QuerkraftListe.Add(path);
            }

            // Gleichlast auf dem Balken und ggf. Punktlast zusätzlich
            else if (balkenGleichlast)
            {
                var pathGeometry = new PathGeometry();
                var pathFigure = new PathFigure { StartPoint = startPoint };

                //if (querkraft1Skaliert < 0) myBrush = new SolidColorBrush(rot);
                vec = endPoint - startPoint;
                vec.Normalize();
                vec2 = RotateVectorScreen(vec, -90);
                nextPoint = startPoint + vec2 * querkraft1Skaliert;
                pathFigure.Segments.Add(new LineSegment(nextPoint, true));

                // hat keine Punktlast
                if (punktLastO < double.Epsilon)
                {
                    const double anzahlProEinheit = 5;

                    var qa = linienLast.Lastwerte[1];
                    var qb = linienLast.Lastwerte[3];
                    var l = element.BalkenLänge;

                    const double inkrement = 1 / anzahlProEinheit;
                    var anzahl = (int)(l / inkrement);
                    var polyLinePointArray = new Point[anzahl + 1];

                    if (Math.Abs(qb - qa) < double.Epsilon)
                    {
                        // lokale x-Koordinate 0 <= x <= l
                        for (var i = 0; i < anzahl; i++)
                        {
                            var x = i * inkrement;
                            var q = element.ElementZustand[1] - qb * x;
                            var qPoint = new Point(startPoint.X + x * Auflösung, -q * MaxQuerkraftScreen / maxQuerkraft);
                            polyLinePointArray[i] = qPoint;
                        }
                        polyLinePointArray[anzahl] = new Point(endPoint.X, -element.ElementZustand[4] * MaxQuerkraftScreen / maxQuerkraft);
                    }
                    else if (Math.Abs(qa) < Math.Abs(qb))
                    {
                        // Q(x) = Qa - qa*x - (qa-qb)/l/2*x*x
                        for (var i = 0; i < anzahl; i++)
                        {
                            var x = i * inkrement;
                            var q = element.ElementZustand[1] - qa * x - (qb - qa) / l / 2 * x * x;
                            var qPoint = new Point(startPoint.X + x * Auflösung, -q * MaxQuerkraftScreen / maxQuerkraft);
                            polyLinePointArray[i] = qPoint;
                        }
                        polyLinePointArray[anzahl] = new Point(endPoint.X, -element.ElementZustand[4] * MaxQuerkraftScreen / maxQuerkraft);
                    }
                    else
                    {
                        // lokale x-Koordinate von rechts l >= x >= 0
                        for (var i = 0; i < anzahl; i++)
                        {
                            var x = i * inkrement;
                            var q = element.ElementZustand[4] + qb * x + (qa - qb) / l / 2 * x * x;
                            var qPoint = new Point(endPoint.X - x * Auflösung, -q * MaxQuerkraftScreen / maxQuerkraft);
                            polyLinePointArray[anzahl - i] = qPoint;
                        }
                        polyLinePointArray[0] = new Point(startPoint.X, -element.ElementZustand[1] * MaxQuerkraftScreen / maxQuerkraft);
                    }

                    // Nulldurchgang der Querkraft
                    var nullIndex = QuerkraftNullWert(polyLinePointArray);
                    Point q0;
                    Point[] polyLine0;
                    if (nullIndex > 0)
                    {
                        var x0 = polyLinePointArray[nullIndex].X + (polyLinePointArray[nullIndex + 1].X - polyLinePointArray[nullIndex].X)
                            * (polyLinePointArray[nullIndex].Y) / (-polyLinePointArray[nullIndex + 1].Y + polyLinePointArray[nullIndex].Y);
                        q0 = new Point(x0, 0);
                        // Querkraftlinie auf der linken Seite
                        polyLine0 = new Point[nullIndex + 2];
                        Array.ConstrainedCopy(polyLinePointArray, 0, polyLine0, 0, nullIndex + 1);
                        polyLine0[nullIndex + 1] = q0;
                    }
                    else
                    {
                        // Querkraftlinie ohne Nulldurchgang
                        polyLine0 = new Point[anzahl + 2];
                        Array.ConstrainedCopy(polyLinePointArray, 0, polyLine0, 0, anzahl + 1);
                        polyLine0[anzahl + 1] = endPoint;
                    }

                    pathFigure.StartPoint = startPoint;
                    var qSegment = new PolyLineSegment { Points = new PointCollection(polyLine0) };
                    pathFigure.Segments.Add(qSegment);
                    pathFigure.IsClosed = true;
                    pathGeometry.Figures.Add(pathFigure);
                    myBrush = new SolidColorBrush(blau);
                    if (element.ElementZustand[1] < 0) myBrush = new SolidColorBrush(rot);
                    Shape path = new Path
                    {
                        Fill = myBrush,
                        Stroke = Black,
                        StrokeThickness = 1,
                        Data = pathGeometry
                    };
                    SetLeft(path, PlatzierungH);
                    SetTop(path, PlatzierungV);
                    _visual.Children.Add(path);
                    QuerkraftListe.Add(path);

                    // Querkraftlinie auf der rechten Seite, nur falls Nulldurchgang < Elementlänge
                    if (nullIndex <= 0) return;
                    var anzahlqb = anzahl - nullIndex;
                    var polyLine1 = new Point[anzahlqb + 2];
                    polyLine1[0] = q0;
                    Array.ConstrainedCopy(polyLinePointArray, nullIndex + 1, polyLine1, 1, anzahlqb);
                    polyLine1[anzahlqb + 1] = endPoint;
                    qSegment = new PolyLineSegment { Points = new PointCollection(polyLine1) };

                    pathGeometry = new PathGeometry();
                    pathFigure = new PathFigure { StartPoint = q0 };
                    pathFigure.Segments.Add(qSegment);
                    pathFigure.IsClosed = true;
                    pathGeometry.Figures.Add(pathFigure);

                    myBrush = new SolidColorBrush(blau);
                    if (element.ElementZustand[1] < 0) myBrush = new SolidColorBrush(rot);
                    path = new Path
                    {
                        Fill = myBrush,
                        Stroke = Black,
                        StrokeThickness = 1,
                        Data = pathGeometry
                    };
                    SetLeft(path, PlatzierungH);
                    SetTop(path, PlatzierungV);
                    _visual.Children.Add(path);
                    QuerkraftListe.Add(path);
                }
                // mit Punktlast
                else
                {
                    const double anzahlProEinheit = 5;
                    var qa = linienLast.Lastwerte[1];
                    var qb = linienLast.Lastwerte[3];
                    var l = element.BalkenLänge;

                    const double inkrement = 1 / anzahlProEinheit;
                    var anzahl = (int)(l / inkrement);
                    var anzahlPunktlast = (int)(l * punktLastO / inkrement);
                    var polyLinePointArray = new Point[anzahl + 2];

                    if (Math.Abs(qa) <= Math.Abs(qb))
                    {
                        // Q(x) = Qa - qa*x - (qb-qa)/l/2*x*x
                        // Q(x) vor und links am Lastangriffspunkt
                        for (var i = 0; i < anzahlPunktlast; i++)
                        {
                            var x = i * inkrement;
                            var q = element.ElementZustand[1] - qa * x - (qb - qa) / l / 2 * x * x;
                            var qPoint = new Point(startPoint.X + x * Auflösung,
                                -q * MaxQuerkraftScreen / maxQuerkraft);
                            polyLinePointArray[i] = qPoint;
                        }
                        var xP = punktLastO * l;
                        var qP = element.ElementZustand[1] - qa * xP - (qb - qa) / l / 2 * xP * xP;
                        polyLinePointArray[anzahlPunktlast] = new Point(startPoint.X + punktLastO * l * Auflösung,
                                -qP * MaxQuerkraftScreen / maxQuerkraft);

                        // Q(x) rechts am und hinter Lastangriffspunkt, Lastangriffspunkt doppelt
                        polyLinePointArray[anzahlPunktlast + 1] = new Point(startPoint.X + punktLastO * l * Auflösung,
                            -(qP - punktLastQ) * MaxQuerkraftScreen / maxQuerkraft);
                        for (var i = anzahlPunktlast + 1; i < anzahl; i++)
                        {
                            var x = i * inkrement;
                            var q = element.ElementZustand[1] - qa * x - (qb - qa) / l / 2 * x * x - punktLastQ;
                            var qPoint = new Point(startPoint.X + x * Auflösung, -q * MaxQuerkraftScreen / maxQuerkraft);
                            polyLinePointArray[i + 1] = qPoint;
                        }
                        polyLinePointArray[anzahl + 1] = new Point(endPoint.X, -element.ElementZustand[4] * MaxQuerkraftScreen / maxQuerkraft);
                    }
                    else
                    {
                        // lokale Koordinate y vom rechten Rand
                        // Q(y) = Qb + qb*y + (qa-qb)/l/2*y*y
                        for (var i = 0; i < anzahl - anzahlPunktlast; i++)
                        {
                            var y = i * inkrement;
                            var q = element.ElementZustand[4] + qb * y + (qa - qb) / l / 2 * y * y;
                            var qPoint = new Point(endPoint.X - y * Auflösung, -q * MaxQuerkraftScreen / maxQuerkraft);
                            polyLinePointArray[anzahl + 1 - i] = qPoint;
                        }
                        var yP = (1 - punktLastO) * l;
                        var qP = element.ElementZustand[4] + qb * yP + (qa - qb) / l / 2 * yP * yP;
                        polyLinePointArray[anzahlPunktlast + 1] = new Point(endPoint.X - (1 - punktLastO) * l * Auflösung,
                            -qP * MaxQuerkraftScreen / maxQuerkraft);
                        // Q(y) links am und vor Lastangriffspunkt, Lastangriffspunkt doppelt
                        polyLinePointArray[anzahlPunktlast] = new Point(endPoint.X - (1 - punktLastO) * l * Auflösung,
                            -(qP + punktLastQ) * MaxQuerkraftScreen / maxQuerkraft);
                        for (var i = anzahl - anzahlPunktlast; i < anzahl - 1; i++)
                        {
                            var y = (i + 1) * inkrement;
                            var q = element.ElementZustand[4] + qb * y + (qa - qb) / l / 2 * y * y;
                            var qPoint = new Point(endPoint.X - y * Auflösung, -(q + punktLastQ) * MaxQuerkraftScreen / maxQuerkraft);
                            polyLinePointArray[anzahl - i - 1] = qPoint;
                        }
                        polyLinePointArray[0] = new Point(startPoint.X, -element.ElementZustand[1] * MaxQuerkraftScreen / maxQuerkraft);
                    }

                    // Nulldurchgang der Querkraft
                    var nullIndex = QuerkraftNullWert(polyLinePointArray);
                    Point q0;
                    Point[] polyLine0;
                    if (nullIndex > 0)
                    {
                        var x0 = polyLinePointArray[nullIndex].X + (polyLinePointArray[nullIndex + 1].X - polyLinePointArray[nullIndex].X)
                            * (polyLinePointArray[nullIndex].Y) / (-polyLinePointArray[nullIndex + 1].Y + polyLinePointArray[nullIndex].Y);
                        q0 = new Point(x0, 0);
                        // Querkraftlinie auf der linken Seite
                        polyLine0 = new Point[nullIndex + 2];
                        Array.ConstrainedCopy(polyLinePointArray, 0, polyLine0, 0, nullIndex + 1);
                        polyLine0[nullIndex + 1] = q0;
                    }
                    else
                    {
                        // Querkraftlinie ohne Nulldurchgang
                        polyLine0 = new Point[anzahl + 2];
                        Array.ConstrainedCopy(polyLinePointArray, 0, polyLine0, 0, anzahl + 1);
                        polyLine0[anzahl + 1] = endPoint;
                    }
                    // Querkraftlinie auf der linken Seite
                    var qSegment = new PolyLineSegment { Points = new PointCollection(polyLine0) };

                    pathFigure = new PathFigure { StartPoint = startPoint };
                    pathFigure.Segments.Add(qSegment);
                    pathFigure.IsClosed = true;
                    pathGeometry.Figures.Add(pathFigure);

                    myBrush = new SolidColorBrush(blau);
                    if (element.ElementZustand[1] < 0) myBrush = new SolidColorBrush(rot);
                    Shape path = new Path
                    {
                        Fill = myBrush,
                        Stroke = Black,
                        StrokeThickness = 1,
                        Data = pathGeometry
                    };
                    SetLeft(path, PlatzierungH);
                    SetTop(path, PlatzierungV);
                    _visual.Children.Add(path);
                    QuerkraftListe.Add(path);

                    // Querkraftlinie auf der rechten Seite, nur falls Nulldurchgang < Elementlänge
                    if (nullIndex <= 0) return;
                    var anzahlRechts = polyLinePointArray.Length - (nullIndex + 1);
                    var polyLine1 = new Point[anzahlRechts + 1];
                    Array.ConstrainedCopy(polyLinePointArray, nullIndex + 1, polyLine1, 0, anzahlRechts);
                    polyLine1[anzahlRechts] = endPoint;
                    qSegment = new PolyLineSegment { Points = new PointCollection(polyLine1) };

                    pathGeometry = new PathGeometry();
                    pathFigure = new PathFigure { StartPoint = q0 };
                    pathFigure.Segments.Add(qSegment);
                    pathFigure.IsClosed = true;
                    pathGeometry.Figures.Add(pathFigure);

                    myBrush = new SolidColorBrush(blau);
                    if (element.ElementZustand[4] < 0) myBrush = new SolidColorBrush(rot);
                    path = new Path
                    {
                        Fill = myBrush,
                        Stroke = Black,
                        StrokeThickness = 1,
                        Data = pathGeometry
                    };
                    SetLeft(path, PlatzierungH);
                    SetTop(path, PlatzierungV);
                    _visual.Children.Add(path);
                    QuerkraftListe.Add(path);
                }
            }
        }
    }

    private static int QuerkraftNullWert(IReadOnlyList<Point> poly)
    {
        var index = 0;
        for (var i = 0; i < poly.Count - 1; i++)
        {
            if (Math.Sign(poly[i].Y) + Math.Sign(poly[i + 1].Y) != 0) continue;
            // Nulldurchgang zwischen i und i+1
            index = i;
            break;
        }

        return index;
    }

    public void Momente_Zeichnen(AbstraktBalken element, double skalierungMoment, bool elementlast)
    {
        if (element is Fachwerk) return;
        var moment1Skaliert = element.ElementZustand[2] / skalierungMoment * MaxMomentScreen;
        var moment2Skaliert = element.ElementZustand[5] / skalierungMoment * MaxMomentScreen;

        var rot = FromArgb(120, 255, 0, 0);
        var blau = FromArgb(120, 0, 0, 255);

        if (!_modell.Knoten.TryGetValue(element.KnotenIds[0], out _knoten))
        {
            throw new ModellAusnahme("\nElement Knoten '" + element.KnotenIds[0] + "' nicht im Modell gefunden");
        }

        var startPunkt = TransformKnoten(_knoten, Auflösung, MaxY);

        if (!_modell.Knoten.TryGetValue(element.KnotenIds[1], out _knoten))
        {
            throw new ModellAusnahme("\nElement Knoten '" + element.KnotenIds[1] + "' nicht im Modell gefunden");
        }

        var endPunkt = TransformKnoten(_knoten, Auflösung, MaxY);

        double punktLastO = 0;
        var pathGeometry = new PathGeometry();
        var pathFigure = new PathFigure();

        var myBrush = new SolidColorBrush(blau);
        switch ((int)moment1Skaliert)
        {
            case < 0:
                myBrush = new SolidColorBrush(rot);
                break;
            case 0:
                {
                    if ((int)moment2Skaliert < 0) myBrush = new SolidColorBrush(rot);
                    break;
                }
        }

        pathFigure.StartPoint = startPunkt;
        var vec = endPunkt - startPunkt;
        vec.Normalize();

        // Linie von start nach Moment1 skaliert
        var vec2 = RotateVectorScreen(vec, 90);
        var nächsterPunkt = startPunkt + vec2 * moment1Skaliert;
        pathFigure.Segments.Add(new LineSegment(nächsterPunkt, true));

        // nur Knotenlasten, keine Punkt-/Linienlasten, d.h. nur Stabendkräfte
        if (!elementlast)
        {
            //Linie von Moment1 skaliert nach Moment2 skaliert
            nächsterPunkt = endPunkt + vec2 * moment2Skaliert;
            pathFigure.Segments.Add(new LineSegment(nächsterPunkt, true));

            // Linie nach end und anschliessend pathFigure schliessen
            pathFigure.Segments.Add(new LineSegment(endPunkt, true));
            pathFigure.IsClosed = true;
            pathGeometry.Figures.Add(pathFigure);

            Shape path = new Path
            {
                Fill = myBrush,
                Stroke = Black,
                StrokeThickness = 1,
                Data = pathGeometry
            };
            SetLeft(path, PlatzierungH);
            SetTop(path, PlatzierungV);
            _visual.Children.Add(path);
            MomenteListe.Add(path);
        }

        // Elementlasten (Linienlast, Punktlast) vorhanden
        // Element hat Punkt- und/oder Linienlast
        else
        {
            bool elementHatPunktLast = false, elementHatLinienLast = false;
            LinienLast linienLast = null;
            PunktLast punktLast = null;

            // finde Punktlast auf Balkenelement
            foreach (var item in _modell.PunktLasten)
            {
                if (item.Value is not PunktLast last || item.Value.ElementId != element.ElementId) continue;
                punktLast = last;
                punktLastO = last.Offset;
                elementHatPunktLast = true;
                break;
            }

            Point maxPunkt;
            double mmax;

            // finde Linienlast auf Balkenelement
            foreach (var item in _modell.ElementLasten)
            {
                if (item.Value is not LinienLast last || item.Value.ElementId != element.ElementId) continue;
                linienLast = last;
                elementHatLinienLast = true;
                break;
            }

            // zeichne Momentenlinie, nur Punkt-, keine Linienlast
            const int anzahlProEinheit = 5;
            const double inkrement = 1.0 / anzahlProEinheit;
            if (elementHatPunktLast && !elementHatLinienLast)
            {
                // Linie von Moment1 skaliert nach Mmax skaliert
                mmax = element.ElementZustand[2] - element.ElementZustand[1] * punktLastO * element.BalkenLänge;
                var mmaxSkaliert = mmax / skalierungMoment * MaxMomentScreen;

                maxPunkt = startPunkt + vec * punktLastO * element.BalkenLänge * Auflösung + vec2 * mmaxSkaliert;
                pathFigure.Segments.Add(new LineSegment(maxPunkt, true));

                //Linie von Mmax skaliert nach Moment2 skaliert
                nächsterPunkt = endPunkt + vec2 * moment2Skaliert;
                pathFigure.Segments.Add(new LineSegment(nächsterPunkt, true));

                // Linie nach end und anschliessend pathFigure schliessen
                pathFigure.Segments.Add(new LineSegment(endPunkt, true));

                MaxMomentText = new TextBlock
                {
                    FontSize = 12,
                    Text = mmax.ToString("F2"),
                    Foreground = Blue
                };
                SetTop(MaxMomentText, maxPunkt.Y + PlatzierungV);
                SetLeft(MaxMomentText, maxPunkt.X + PlatzierungH);
                _visual.Children.Add(MaxMomentText);
                MomentenMaxTexte.Add(MaxMomentText);
            }

            // zeichne Momentenlinie unter Gleich- oder Dreieckslast
            else if (elementHatLinienLast)
            {
                var qa = linienLast.Lastwerte[1];
                var qb = linienLast.Lastwerte[3];
                var l = element.BalkenLänge;

                var anzahl = (int)(l / inkrement);
                var polyLinePointArray = new Point[anzahl + 2];

                // konstante Last oder linear steigende Dreieckslast
                if (Math.Abs(qb) >= Math.Abs(qa))
                {
                    for (var i = 0; i <= anzahl; i++)
                    {
                        // lokale x-Koordinate vom Balkenanfang 0 <= x <= Lastlänge
                        var x = i * inkrement;
                        // M(x) = Ma - Qa*x + qa*x*x/2 + (qb-qa)/l/6 *x*x*x
                        var m = element.ElementZustand[2] - element.ElementZustand[1] * x + qa / 2 * x * x
                                        + (qb - qa) / l / 6 * x * x * x;
                        polyLinePointArray[i] = new Point((element.Knoten[0].Koordinaten[0] + x) * Auflösung,
                            m / skalierungMoment * MaxMomentScreen);
                    }
                    polyLinePointArray[anzahl] = new Point(element.Knoten[1].Koordinaten[0] * Auflösung,
                        +element.ElementZustand[5] / skalierungMoment * MaxMomentScreen);
                }
                //linear fallende Dreieckslast
                else
                {
                    for (var i = 0; i < anzahl; i++)
                    {
                        // lokale x-Koordinate vom Balkenende 0 <= x <= Lastlänge
                        var x = i * inkrement;
                        // M(x) = Mb - Qb*x + qb/2 *x*x + (qa-qb)/l/6 *x*x*x
                        var m = element.ElementZustand[5] + element.ElementZustand[4] * x + qb / 2 * x * x +
                                (qa - qb) / l / 6 * x * x * x;
                        polyLinePointArray[anzahl - i] = new Point((element.Knoten[1].Koordinaten[0] - x) * Auflösung,
                            m / skalierungMoment * MaxMomentScreen);
                    }
                    polyLinePointArray[0] = new Point(element.Knoten[0].Koordinaten[0] * Auflösung,
                        element.ElementZustand[2] / skalierungMoment * MaxMomentScreen);
                }

                // schreib maximalen Momententext, nur Linien-, keine Punktlast
                if (!elementHatPunktLast)
                {
                    polyLinePointArray[anzahl + 1] = endPunkt;
                    var mSegment = new PolyLineSegment { Points = new PointCollection(polyLinePointArray) };
                    pathFigure.Segments.Add(mSegment);

                    var indexMax = MomentenMaxWert(polyLinePointArray);
                    var xMax = element.Knoten[0].Koordinaten[0] + indexMax * inkrement;
                    if (indexMax > 0 && indexMax < polyLinePointArray.Length - 2)
                    {
                        maxPunkt = new Point(xMax * Auflösung, polyLinePointArray[indexMax].Y);
                        MaxMomentText = new TextBlock
                        {
                            FontSize = 12,
                            Text = "Mmax = " + (polyLinePointArray[indexMax].Y * skalierungMoment / MaxMomentScreen)
                                .ToString("F2"),
                            Foreground = Blue
                        };
                        SetTop(MaxMomentText, maxPunkt.Y + PlatzierungV);
                        SetLeft(MaxMomentText, maxPunkt.X + PlatzierungH);

                        _visual.Children.Add(MaxMomentText);
                        MomentenMaxTexte.Add(MaxMomentText);
                    }
                }

                // schreib maximalen Momententext, Element hat Punktlast
                else
                {
                    double m;
                    var abstandPunktlast = punktLastO * element.BalkenLänge;
                    // Unstetigkeit an Punktlast
                    // qa ≤ qb Gleichlast oder Dreieckslast linear steigend
                    if (Math.Abs(qb) >= Math.Abs(qa))
                    {
                        var anzahlPunktlast = (int)(abstandPunktlast / inkrement);
                        for (var i = 0; i <= anzahlPunktlast; i++)
                        {
                            var x = i * inkrement;
                            // M(x) = Ma - Qa*x + qa/2 *x*x + (qb-qa)/l/6 *x*x*x
                            m = element.ElementZustand[2] - element.ElementZustand[1] * x + qa / 2 * x * x + (qb - qa) / l / 6 * x * x * x;
                            var mPoint = new Point((element.Knoten[0].Koordinaten[0] + x) * Auflösung,
                                m / skalierungMoment * MaxMomentScreen);
                            polyLinePointArray[i] = mPoint;
                        }

                        for (var i = anzahlPunktlast + 1; i < anzahl; i++)
                        {
                            var x = i * inkrement;
                            // M(x) = Ma - Qa*x + qa/2 *x*x + (qb-qa)/l/6 *x*x*x + P * (x-abstandPunktlast)
                            m = element.ElementZustand[2] - element.ElementZustand[1] * x + qa / 2 * x * x + (qb - qa) / l / 6 * x * x * x
                                + punktLast.Lastwerte[1] * (x - abstandPunktlast);
                            var mPoint = new Point((element.Knoten[0].Koordinaten[0] + x) * Auflösung,
                                m / skalierungMoment * MaxMomentScreen);
                            polyLinePointArray[i] = mPoint;
                        }
                        polyLinePointArray[anzahl] = new Point((element.Knoten[1].Koordinaten[0]) * Auflösung, element.ElementZustand[5] / skalierungMoment * MaxMomentScreen);
                        polyLinePointArray[anzahl + 1] = endPunkt;
                        var mSegment = new PolyLineSegment { Points = new PointCollection(polyLinePointArray) };
                        pathFigure.Segments.Add(mSegment);

                        var indexMax = MomentenMaxWert(polyLinePointArray);
                        mmax = polyLinePointArray[indexMax].Y * skalierungMoment / MaxMomentScreen;
                        maxPunkt = new Point
                        {
                            X = (element.Knoten[0].Koordinaten[0] + indexMax * inkrement) * Auflösung,
                            Y = mmax / skalierungMoment * MaxMomentScreen
                        };

                        MaxMomentText = new TextBlock
                        {
                            FontSize = 12,
                            Text = "Mmax = " + mmax.ToString("F2"),
                            Foreground = Blue
                        };
                        SetTop(MaxMomentText, maxPunkt.Y + PlatzierungV);
                        SetLeft(MaxMomentText, maxPunkt.X + PlatzierungH);
                        _visual.Children.Add(MaxMomentText);
                        MomentenMaxTexte.Add(MaxMomentText);
                    }

                    // qa > qb, Dreieckslast linear fallend, lokale x-Koordinate von rechts
                    else
                    {
                        var anzahlPunktlast = (int)((l - abstandPunktlast) / inkrement);
                        polyLinePointArray = new Point[anzahl + 2];

                        polyLinePointArray[anzahl + 1] = endPunkt;
                        polyLinePointArray[anzahl] = new Point((element.Knoten[1].Koordinaten[0]) * Auflösung, element.ElementZustand[5] / skalierungMoment * MaxMomentScreen);
                        for (var i = 1; i <= anzahlPunktlast; i++)
                        {
                            // lokale x-Koordinate vom Balkenende 0 <= x <= Lastlänge
                            var x = i * inkrement;
                            // M(x) = Mb + Qb*x + qb/2 *x*x + (qa-qb)/l/6 *x*x*x
                            m = element.ElementZustand[5] + element.ElementZustand[4] * x + qb / 2 * x * x + (qa - qb) / l / 6 * x * x * x;
                            polyLinePointArray[anzahl - i] = new Point((element.Knoten[1].Koordinaten[0] - x) * Auflösung,
                                m / skalierungMoment * MaxMomentScreen);
                        }

                        for (var i = anzahlPunktlast + 1; i < anzahl; i++)
                        {
                            // lokale x-Koordinate vom Balkenende (l-abstandPunktlast) < x <= l
                            var x = i * inkrement;
                            // M(x) = Mb + Qb*x + qb/2 *x*x + (qa-qb)/l/6 *x*x*x + P * (x-(l-abstandPunktlast))
                            m = element.ElementZustand[5] + element.ElementZustand[4] * x + qb / 2 * x * x + (qa - qb) / l / 6 * x * x * x
                                + punktLast.Lastwerte[1] * (x - (l - abstandPunktlast));
                            var mPoint = new Point((element.Knoten[1].Koordinaten[0] - x) * Auflösung,
                                m / skalierungMoment * MaxMomentScreen);
                            polyLinePointArray[anzahl - i] = mPoint;
                        }
                        polyLinePointArray[0] = new Point((element.Knoten[0].Koordinaten[0]) * Auflösung, element.ElementZustand[2] / skalierungMoment * MaxMomentScreen);


                        var mSegment = new PolyLineSegment { Points = new PointCollection(polyLinePointArray) };
                        pathFigure.Segments.Add(mSegment);

                        var indexMax = MomentenMaxWert(polyLinePointArray);
                        mmax = polyLinePointArray[indexMax].Y * skalierungMoment / MaxMomentScreen;
                        maxPunkt = new Point
                        {
                            X = (element.Knoten[0].Koordinaten[0] + indexMax * inkrement) * Auflösung,
                            Y = mmax / skalierungMoment * MaxMomentScreen
                        };

                        MaxMomentText = new TextBlock
                        {
                            FontSize = 12,
                            Text = "Mmax = " + mmax.ToString("F2"),
                            Foreground = Blue
                        };
                        SetTop(MaxMomentText, maxPunkt.Y + PlatzierungV);
                        SetLeft(MaxMomentText, maxPunkt.X + PlatzierungH);
                        _visual.Children.Add(MaxMomentText);
                        MomentenMaxTexte.Add(MaxMomentText);
                    }
                }
            }

            pathFigure.IsClosed = true;
            pathGeometry.Figures.Add(pathFigure);

            Shape path = new Path
            {
                Name = "Biegemomente",
                Fill = myBrush,
                Stroke = Black,
                StrokeThickness = 1,
                Data = pathGeometry
            };
            SetLeft(path, PlatzierungH);
            SetTop(path, PlatzierungV);
            _visual.Children.Add(path);
            MomenteListe.Add(path);
        }
    }

    private static int MomentenMaxWert(IReadOnlyList<Point> poly)
    {
        var index = 0;
        double max = 0;
        for (var i = 0; i < poly.Count - 1; i++)
        {
            if (!(poly[i].Y > max)) continue;
            max = poly[i].Y;
            index = i;
        }

        return index;
    }

    // Zeitverlauf wird ab tmin dargestellt
    public void ZeitverlaufZeichnen(double dt, double tmin, double tmax, double mY, double[] ordinaten)
    {
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
        SetTop(zeitverlauf, mY * _auflösungV + PlatzierungV);
        // zeichne Shape
        _visual.Children.Add(zeitverlauf);
    }

    public void Koordinatensystem(double tmin, double tmax, double max, double min)
    {
        const int rand = 20;
        _screenH = _visual.ActualWidth;
        _screenV = _visual.ActualHeight;
        if (double.IsPositiveInfinity(max)) _auflösungV = _screenV - rand;
        else _auflösungV = (_screenV - rand) / (max - min);
        _auflösungH = (_screenH - rand) / (tmax - tmin);
        var xAchse = new Line
        {
            Stroke = Black,
            X1 = 0,
            Y1 = max * _auflösungV + PlatzierungV,
            X2 = (tmax - tmin) * _auflösungH + PlatzierungH,
            Y2 = max * _auflösungV + PlatzierungV,
            StrokeThickness = 2
        };
        _ = _visual.Children.Add(xAchse);
        var yAchse = new Line
        {
            Stroke = Black,
            X1 = RandLinks,
            Y1 = max * _auflösungV - min * _auflösungV + 2 * PlatzierungV,
            X2 = RandLinks,
            Y2 = PlatzierungV,
            StrokeThickness = 2
        };
        _visual.Children.Add(yAchse);
    }

    private static Vector RotateVectorScreen(Vector vec, double winkel) // clockwise in degree
    {
        var vector = vec;
        var angle = winkel * Math.PI / 180;
        return new Vector(vector.X * Math.Cos(angle) - vector.Y * Math.Sin(angle),
            vector.X * Math.Sin(angle) + vector.Y * Math.Cos(angle));
    }

    private static Point TransformKnoten(Knoten knoten, double auflösung, double maxY)
    {
        return new Point(knoten.Koordinaten[0] * auflösung, (-knoten.Koordinaten[1] + maxY) * auflösung);
    }

    private Point TransformVerformtenKnoten(Knoten verformt, double resolution, double max)
    {
        // eingabeEinheit z.B. in m, verformungsEinheit z.B. cm → Überhöhung
        return new Point(
            (verformt.Koordinaten[0] + verformt.Knotenfreiheitsgrade[0] * ÜberhöhungVerformung) * resolution,
            (-verformt.Koordinaten[1] - verformt.Knotenfreiheitsgrade[1] * ÜberhöhungVerformung + max) * resolution);
    }

    public double[] TransformBildPunkt(Point point)
    {
        var koordinaten = new double[2];
        koordinaten[0] = (point.X - PlatzierungH) / Auflösung;
        koordinaten[1] = (-point.Y + PlatzierungV) / Auflösung + MaxY;
        return koordinaten;
    }
    //public Point TransformKnotenBildPunkt(double[] koordinaten)
    //{
    //    var bildPunkt = new Point
    //    {
    //        X = koordinaten[0] * Auflösung + PlatzierungH,
    //        Y = (-koordinaten[1] + MaxY) * Auflösung + PlatzierungV
    //    };
    //    return bildPunkt;
    //}
}