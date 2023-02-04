using FE_Berechnungen.Tragwerksberechnung.Modelldaten;
using FEBibliothek.Modell;
using FEBibliothek.Modell.abstrakte_Klassen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using static System.Windows.Controls.Canvas;
using static System.Windows.Media.Brushes;
using static System.Windows.Media.Color;

namespace FE_Berechnungen.Tragwerksberechnung;

public class Darstellung
{
    private readonly FeModell modell;
    private Knoten knoten;
    public double auflösung;
    private double auflösungH, auflösungV, lastAuflösung;
    public double maxY;
    private double minX, maxX, minY;
    public double plazierungV, plazierungH;
    private double screenH, screenV;
    public int überhöhungVerformung = 1;
    public int überhöhungRotation = 1;
    private const int RandOben = 60, RandLinks = 60;
    private const int MaxNormalkraftScreen = 30;
    private const int MaxQuerkraftScreen = 30;
    private const int MaxMomentScreen = 50;
    private readonly Canvas visual;
    public TextBlock maxMomentText;
    public Point plazierungText;

    public List<object> ElementIDs { get; }
    public List<object> KnotenIDs { get; }
    public List<object> LastIDs { get; }
    public List<object> LagerIDs { get; }
    public List<object> MaxTexte { get; }
    public List<object> Verformungen { get; }
    public List<object> LastVektoren { get; }
    public List<object> LagerDarstellung { get; }
    public List<object> NormalkraftListe { get; }
    public List<object> QuerkraftListe { get; }
    public List<object> MomenteListe { get; }
    public List<TextBlock> Anfangsbedingungen { get; }


    public Darstellung(FeModell feModell, Canvas visual)
    {
        modell = feModell;
        this.visual = visual;
        ElementIDs = new List<object>();
        KnotenIDs = new List<object>();
        LastIDs = new List<object>();
        LagerIDs = new List<object>();
        Verformungen = new List<object>();
        LastVektoren = new List<object>();
        LagerDarstellung = new List<object>();
        NormalkraftListe = new List<object>();
        QuerkraftListe = new List<object>();
        MomenteListe = new List<object>();
        Anfangsbedingungen = new List<TextBlock>();
        MaxTexte = new List<object>();
        FestlegungAuflösung();
    }
    public void FestlegungAuflösung()
    {
        screenH = visual.ActualWidth;
        screenV = visual.ActualHeight;

        var x = new List<double>();
        var y = new List<double>();
        foreach (var item in modell.Knoten)
        {
            x.Add(item.Value.Koordinaten[0]);
            y.Add(item.Value.Koordinaten[1]);
        }
        maxX = x.Max(); minX = x.Min();
        maxY = y.Max(); minY = y.Min();

        // vertikales Modell
        var delta = Math.Abs(maxX - minX);
        if (delta < 1)
        {
            auflösungH = screenH - 2 * RandLinks;
            plazierungH = (int)(0.5 * screenH);
        }
        else
        {
            auflösungH = (screenH - 2 * RandLinks) / delta;
            plazierungH = RandLinks;
        }

        // horizontales Modell
        delta = Math.Abs(maxY - minY);
        if (delta < 1)
        {
            auflösung = screenV - 2 * RandOben;
            plazierungV = (int)(0.5 * screenV);
        }
        else
        {
            auflösung = (screenV - 2 * RandOben) / delta;
            plazierungV = RandOben;
        }
        if (auflösungH < auflösung) auflösung = auflösungH;

    }
    
    public void UnverformteGeometrie()
    {
        // Elementumrisse werden als Shape (PathGeometry) mit Namen hinzugefügt
        // pathGeometry enthaelt EIN spezifisches Element
        // alle Elemente werden der GeometryGroup tragwerk hinzugefügt

        foreach (var item in modell.Elemente)
        {
            ElementZeichnen(item.Value, Black, 2);
        }

        // Knotengelenke werden als EllipseGeometry der GeometryGroup tragwerk hinzugefügt
        var tragwerk = new GeometryGroup();
        foreach (var gelenk in from item in modell.Knoten
                               select item.Value into knoten
                               where knoten.AnzahlKnotenfreiheitsgrade == 2
                               select TransformKnoten(knoten, auflösung, maxY) into gelenkPunkt
                               select new EllipseGeometry(gelenkPunkt, 5, 5)) { tragwerk.Children.Add(gelenk); }
        // Knotengelenke werden gezeichnet
        Shape tragwerkPath = new Path()
        {
            Stroke = Black,
            StrokeThickness = 1,
            Data = tragwerk
        };
        SetLeft(tragwerkPath, plazierungH);
        SetTop(tragwerkPath, plazierungV);
        visual.Children.Add(tragwerkPath);
    }

    public Shape KnotenZeigen(Knoten feKnoten, Brush farbe, double wichte)
    {
        var punkt = TransformKnoten(feKnoten, auflösung, maxY);

        var knotenZeigen = new GeometryGroup();
        knotenZeigen.Children.Add(
            new EllipseGeometry(new Point(punkt.X, punkt.Y), 20, 20)
        );
        Shape knotenPath = new Path()
        {
            Stroke = farbe,
            StrokeThickness = wichte,
            Data = knotenZeigen
        };
        SetLeft(knotenPath, plazierungH);
        SetTop(knotenPath, plazierungV);
        visual.Children.Add(knotenPath);
        return knotenPath;
    }
    public Shape ElementZeichnen(AbstraktElement element, Brush farbe, double wichte)
    {
        PathGeometry pathGeometry;

        switch (element)
        {
            case FederElement _:
                {
                    pathGeometry = FederelementZeichnen(element);
                    break;
                }

            case Fachwerk _:
                {
                    // Gelenke als Halbkreise an Knoten des Fachwerkelementes zeichnen
                    pathGeometry = FachwerkelementZeichnen(element);
                    break;
                }
            case Biegebalken _:
                {
                    pathGeometry = BiegebalkenZeichnen(element);
                    break;
                }

            case BiegebalkenGelenk _:
                {
                    // Gelenk am Startknoten bzw. Endknoten des BiegebalkenGelenk zeichnen
                    pathGeometry = BiegebalkenGelenkZeichnen(element);
                    break;
                }

            // Elemente mit mehreren Knoten
            default:
                {
                    pathGeometry = MultiKnotenElementZeichnen(element);
                    break;
                }
        }
        Shape elementPath = new Path()
        {
            Name = element.ElementId,
            Stroke = farbe,
            StrokeThickness = wichte,
            Data = pathGeometry
        };
        SetLeft(elementPath, plazierungH);
        SetTop(elementPath, plazierungV);
        visual.Children.Add(elementPath);
        return elementPath;
    }
    public void VerformteGeometrie()
    {
        if (!StartFenster.berechnet)
        {
            var analysis = new Berechnung(modell);
            analysis.BerechneSystemMatrix();
            analysis.BerechneSystemVektor();
            analysis.LöseGleichungen();
            StartFenster.berechnet = true;
        }
        var pathGeometry = new PathGeometry();

        IEnumerable<AbstraktBalken> Beams()
        {
            foreach (var item in modell.Elemente)
            {
                if (item.Value is AbstraktBalken element)
                {
                    yield return element;
                }
            }
        }
        foreach (var element in Beams())
        {
            var pathFigure = new PathFigure();
            Point start;
            Point end;
            double winkel;

            switch (element)
            {
                case Fachwerk _:
                    {
                        if (modell.Knoten.TryGetValue(element.KnotenIds[0], out knoten)) { }
                        start = TransformVerformtenKnoten(knoten, auflösung, maxY);
                        pathFigure.StartPoint = start;

                        for (var i = 1; i < element.KnotenIds.Length; i++)
                        {
                            if (modell.Knoten.TryGetValue(element.KnotenIds[i], out knoten)) { }
                            end = TransformVerformtenKnoten(knoten, auflösung, maxY);
                            pathFigure.Segments.Add(new LineSegment(end, true));
                        }
                        pathGeometry.Figures.Add(pathFigure);
                        break;
                    }
                case Biegebalken _:
                    {
                        element.BerechneZustandsvektor();
                        if (modell.Knoten.TryGetValue(element.KnotenIds[0], out knoten)) { }
                        start = TransformVerformtenKnoten(knoten, auflösung, maxY);
                        pathFigure.StartPoint = start;

                        for (var i = 1; i < element.KnotenIds.Length; i++)
                        {
                            if (modell.Knoten.TryGetValue(element.KnotenIds[i], out knoten)) { }
                            end = TransformVerformtenKnoten(knoten, auflösung, maxY);
                            var richtung = end - start;
                            richtung.Normalize();
                            winkel = -element.ElementVerformungen[2] * 180 / Math.PI * überhöhungRotation;
                            richtung = RotateVectorScreen(richtung, winkel);
                            var control1 = start + richtung * element.balkenLänge / 4 * auflösung;

                            richtung = start - end;
                            richtung.Normalize();
                            winkel = -element.ElementVerformungen[5] * 180 / Math.PI * überhöhungRotation;
                            richtung = RotateVectorScreen(richtung, winkel);
                            var control2 = end + richtung * element.balkenLänge / 4 * auflösung;

                            pathFigure.Segments.Add(new BezierSegment(control1, control2, end, true));
                        }
                        pathGeometry.Figures.Add(pathFigure);
                        break;
                    }
                case BiegebalkenGelenk _:
                    {
                        if (modell.Knoten.TryGetValue(element.KnotenIds[0], out knoten)) { }
                        start = TransformVerformtenKnoten(knoten, auflösung, maxY);
                        pathFigure.StartPoint = start;

                        var control = start;
                        for (var i = 1; i < element.KnotenIds.Length; i++)
                        {
                            if (modell.Knoten.TryGetValue(element.KnotenIds[i], out knoten)) { }
                            end = TransformVerformtenKnoten(knoten, auflösung, maxY);

                            switch (element.Typ)
                            {
                                case 1:
                                    {
                                        var richtung = start - end;
                                        richtung.Normalize();
                                        winkel = element.ElementVerformungen[4] * 180 / Math.PI * überhöhungRotation;
                                        richtung = RotateVectorScreen(richtung, winkel);
                                        control = end + richtung * element.balkenLänge / 4 * auflösung;
                                        break;
                                    }
                                case 2:
                                    {
                                        var richtung = end - start;
                                        richtung.Normalize();
                                        winkel = element.ElementVerformungen[2] * 180 / Math.PI * überhöhungRotation;
                                        richtung = RotateVectorScreen(richtung, winkel);
                                        control = start + richtung * element.balkenLänge / 4 * auflösung;
                                        break;
                                    }
                            }
                            pathFigure.Segments.Add(new QuadraticBezierSegment(control, end, true));
                        }
                        pathGeometry.Figures.Add(pathFigure);
                        break;
                    }
                default:
                    {
                        if (modell.Knoten.TryGetValue(element.KnotenIds[0], out knoten)) { }
                        start = TransformVerformtenKnoten(knoten, auflösung, maxY);
                        pathFigure.StartPoint = start;

                        for (var i = 1; i < element.KnotenIds.Length; i++)
                        {
                            if (modell.Knoten.TryGetValue(element.KnotenIds[i], out knoten)) { }
                            var next = TransformVerformtenKnoten(knoten, auflösung, maxY);
                            pathFigure.Segments.Add(new LineSegment(next, true));
                        }
                        pathFigure.IsClosed = true;
                        pathGeometry.Figures.Add(pathFigure);
                        break;
                    }
            }
            Shape path = new Path()
            {
                Stroke = Red,
                StrokeThickness = 2,
                Data = pathGeometry
            };

            SetLeft(path, plazierungH);
            SetTop(path, plazierungV);
            visual.Children.Add(path);
            Verformungen.Add(path);
        }
    }

    private PathGeometry FederelementZeichnen(AbstraktElement element)
    {
        var pathGeometry = new PathGeometry();
        var pathFigure = new PathFigure();
        // Plazierungspunkt des Federelementes
        if (modell.Knoten.TryGetValue(element.KnotenIds[0], out knoten)) { }
        var startPunkt = TransformKnoten(knoten, auflösung, maxY);

        // setz Referenzen der MaterialWerte
        element.SetzElementReferenzen(modell);

        if (element.ElementMaterial.MaterialWerte.Length < 3)
        {
            _ = MessageBox.Show("Materialangabe ungültig, 3 Werte für Federsteifigkeiten erforderlich", "Federdarstellung");
            return pathGeometry;
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
        const double b = 6.0; const int h = 3;
        pathFigure.StartPoint = startPunkt;
        pathFigure.Segments.Add(
            new LineSegment(new Point(startPunkt.X, startPunkt.Y + 2 * h), true));
        pathFigure.Segments.Add(
            new LineSegment(new Point(startPunkt.X - b, startPunkt.Y + 3 * h), true));
        pathFigure.Segments.Add(
            new LineSegment(new Point(startPunkt.X + b, startPunkt.Y + 5 * h), true));
        pathFigure.Segments.Add(
            new LineSegment(new Point(startPunkt.X - b, startPunkt.Y + 7 * h), true));
        pathFigure.Segments.Add(
            new LineSegment(new Point(startPunkt.X + b, startPunkt.Y + 9 * h), true));
        pathFigure.Segments.Add(
            new LineSegment(new Point(startPunkt.X, startPunkt.Y + 10 * h), true));
        pathFigure.Segments.Add(
            new LineSegment(new Point(startPunkt.X, startPunkt.Y + 12 * h), true));
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
            new LineSegment(new Point(startPunkt.X, startPunkt.Y + 12 * h), false));
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
        zielPunkt = new Point(startPunkt.X + b, startPunkt.Y);
        pathFigure.Segments.Add(
            new ArcSegment(zielPunkt, new Size(b, b + 2), 190, false, 0, true));
    }

    private PathGeometry FachwerkelementZeichnen(AbstraktElement element)
    {
        if (modell.Knoten.TryGetValue(element.KnotenIds[0], out knoten)) { }
        var startPunkt = TransformKnoten(knoten, auflösung, maxY);
        if (modell.Knoten.TryGetValue(element.KnotenIds[1], out knoten)) { }
        var endPunkt = TransformKnoten(knoten, auflösung, maxY);

        var pathGeometry = new PathGeometry();
        var pathFigure = new PathFigure { StartPoint = startPunkt };
        pathFigure.Segments.Add(new LineSegment(endPunkt, true));

        // Gelenk als Halbkreis am Startknoten des Fachwerkelementes zeichnen
        var direction = endPunkt - startPunkt;
        var start = RotateVectorScreen(direction, 90);
        start.Normalize();
        var zielPunkt = startPunkt + (5 * start);
        pathFigure.Segments.Add(new LineSegment(zielPunkt, false));
        var ziel = RotateVectorScreen(direction, -90);
        ziel.Normalize();
        zielPunkt = startPunkt + (5 * ziel);
        // ArcSegment beginnt am letzten Punkt der pathFigure
        // Zielpunkt, Größe in x,y, Öffnungswinkel, isLargeArc, sweepDirection, isStroked
        pathFigure.Segments.Add(new ArcSegment(zielPunkt, new Size(2.5, 2.5), 180, true, 0, true));
        pathFigure.Segments.Add(new LineSegment(startPunkt, false));

        // Gelenk als Halbkreis am Endknoten des Fachwerkelementes zeichnen
        direction = startPunkt - endPunkt;
        start = RotateVectorScreen(direction, -90);
        start.Normalize();
        zielPunkt = endPunkt + (5 * start);
        pathFigure.Segments.Add(new LineSegment(zielPunkt, false));
        var end = RotateVectorScreen(direction, 90);
        end.Normalize();
        zielPunkt = endPunkt + (5 * end);
        pathFigure.Segments.Add(new ArcSegment(zielPunkt, new Size(2.5, 2.5), 180, true, (SweepDirection)1, true));
        pathGeometry.Figures.Add(pathFigure);
        return pathGeometry;
    }
    private PathGeometry BiegebalkenZeichnen(AbstraktElement element)
    {
        if (modell.Knoten.TryGetValue(element.KnotenIds[0], out knoten)) { }
        var startPunkt = TransformKnoten(knoten, auflösung, maxY);
        if (modell.Knoten.TryGetValue(element.KnotenIds[1], out knoten)) { }
        var endPunkt = TransformKnoten(knoten, auflösung, maxY);

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

        if (modell.Knoten.TryGetValue(element.KnotenIds[0], out knoten)) { }
        var startPunkt = TransformKnoten(knoten, auflösung, maxY);
        if (modell.Knoten.TryGetValue(element.KnotenIds[1], out knoten)) { }
        var endPunkt = TransformKnoten(knoten, auflösung, maxY);

        var pathGeometry = new PathGeometry();
        var pathFigure = new PathFigure { StartPoint = startPunkt };
        pathFigure.Segments.Add(new LineSegment(endPunkt, true));

        // Gelenk am 1. Knoten des Biegebalken zeichnen
        if (element is BiegebalkenGelenk && element.Typ == 1)
        {
            direction = endPunkt - startPunkt;
            start = RotateVectorScreen(direction, 90);
            start.Normalize();
            zielPunkt = startPunkt + (5 * start);
            pathFigure.Segments.Add(new LineSegment(zielPunkt, false));
            var ziel = RotateVectorScreen(direction, -90);
            ziel.Normalize();
            zielPunkt = startPunkt + (5 * ziel);
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
            zielPunkt = endPunkt + (5 * start);
            pathFigure.Segments.Add(new LineSegment(zielPunkt, false));
            var end = RotateVectorScreen(direction, 90);
            end.Normalize();
            zielPunkt = endPunkt + (5 * end);
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
        if (modell.Knoten.TryGetValue(element.KnotenIds[0], out knoten)) { }
        var startPoint = TransformKnoten(knoten, auflösung, maxY);
        pathFigure.StartPoint = startPoint;
        for (var i = 1; i < element.KnotenIds.Length; i++)
        {
            if (modell.Knoten.TryGetValue(element.KnotenIds[i], out knoten)) { }
            var nextPoint = TransformKnoten(knoten, auflösung, maxY);
            pathFigure.Segments.Add(new LineSegment(nextPoint, true));
        }
        pathFigure.IsClosed = true;
        pathGeometry.Figures.Add(pathFigure);
        return pathGeometry;
    }

    public void ElementTexte()
    {
        foreach (var item in modell.Elemente)
        {
            if (item.Value is not Abstrakt2D element) continue;
            element.SetzElementReferenzen(modell);
            var cg = element.BerechneSchwerpunkt();
            var id = new TextBlock
            {
                FontSize = 12,
                Text = item.Key,
                Foreground = Blue
            };
            SetTop(id, (-cg.Y + maxY) * auflösung + plazierungV);
            SetLeft(id, cg.X * auflösung + plazierungH);
            visual.Children.Add(id);
            ElementIDs.Add(id);
        }
    }
    public void KnotenTexte()
    {
        foreach (var item in modell.Knoten)
        {
            var id = new TextBlock
            {
                FontSize = 12,
                Text = item.Key,
                Foreground = Black
            };
            SetTop(id, (-item.Value.Koordinaten[1] + maxY) * auflösung + plazierungV);
            SetLeft(id, item.Value.Koordinaten[0] * auflösung + plazierungH);
            visual.Children.Add(id);
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
        foreach (var item in modell.Lasten)
        {
            last = item.Value;
            if (Math.Abs(last.Lastwerte[0]) > maxLastWert) maxLastWert = Math.Abs(last.Lastwerte[0]);
            if (Math.Abs(last.Lastwerte[1]) > maxLastWert) maxLastWert = Math.Abs(last.Lastwerte[1]);
        }
        foreach (var item in modell.PunktLasten)
        {
            last = item.Value;
            if (Math.Abs(last.Lastwerte[0]) > maxLastWert) maxLastWert = Math.Abs(last.Lastwerte[0]);
            if (Math.Abs(last.Lastwerte[1]) > maxLastWert) maxLastWert = Math.Abs(last.Lastwerte[1]);
        }

        maxLastWert =
            (from linienLast in modell.ElementLasten.Select(item => (AbstraktLinienlast)item.Value)
             from lastwert in linienLast.Lastwerte
             select Math.Abs(lastwert)).Prepend(maxLastWert).Max();
        lastAuflösung = maxLastScreen / maxLastWert;

        foreach (var item in modell.Lasten)
        {
            last = item.Value;
            last.LastId = item.Key;
            var pathGeometry = KnotenlastZeichnen(last);
            path = new Path()
            {
                Name = last.LastId,
                Stroke = Red,
                StrokeThickness = 3,
                Data = pathGeometry
            };
            LastVektoren.Add(path);

            SetLeft(path, plazierungH);
            SetTop(path, plazierungV);
            visual.Children.Add(path);
        }
        foreach (var item in modell.PunktLasten)
        {
            var pathGeometry = PunktlastZeichnen(item.Value);
            path = new Path()
            {
                Name = item.Key,
                Stroke = Red,
                StrokeThickness = 3,
                Data = pathGeometry
            };
            LastVektoren.Add(path);

            SetLeft(path, plazierungH);
            SetTop(path, plazierungV);
            visual.Children.Add(path);
        }
        foreach (var item in modell.ElementLasten)
        {
            var linienlast = (AbstraktLinienlast)item.Value;
            var pathGeometry = LinienlastZeichnen(linienlast);
            var rot = FromArgb(60, 255, 0, 0);
            var blau = FromArgb(60, 0, 0, 255);
            var myBrush = new SolidColorBrush(rot);
            if (linienlast.Lastwerte[1] > 0) myBrush = new SolidColorBrush(blau);
            path = new Path()
            {
                Name = linienlast.LastId,
                Fill = myBrush,
                Stroke = Red,
                StrokeThickness = 1,
                Data = pathGeometry
            };
            LastVektoren.Add(path);

            SetLeft(path, plazierungH);
            SetTop(path, plazierungV);
            visual.Children.Add(path);
        }
    }
    private PathGeometry KnotenlastZeichnen(AbstraktLast knotenlast)
    {
        var pathGeometry = new PathGeometry();
        var pathFigure = new PathFigure();
        const int lastPfeilGroesse = 10;

        if (modell.Knoten.TryGetValue(knotenlast.KnotenId, out knoten)) { }

        if (knoten != null)
        {
            var endPoint = new Point(knoten.Koordinaten[0] * auflösung - knotenlast.Lastwerte[0] * lastAuflösung,
                (-knoten.Koordinaten[1] + maxY) * auflösung + knotenlast.Lastwerte[1] * lastAuflösung);
            pathFigure.StartPoint = endPoint;

            var startPoint = TransformKnoten(knoten, auflösung, maxY);
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

            if (knotenlast.Lastwerte.Length > 2 && Math.Abs(knotenlast.Lastwerte[2]) > double.Epsilon)
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
        const int lastPfeilGroesse = 10;

        punktlast.SetzElementlastReferenzen(modell);
        if (modell.Elemente.TryGetValue(punktlast.ElementId, out var element)) { }

        if (element == null) return pathGeometry;
        if (modell.Knoten.TryGetValue(element.KnotenIds[0], out knoten)) { }
        var startPunkt = TransformKnoten(knoten, auflösung, maxY);

        // zweiter Elementknoten 
        if (modell.Knoten.TryGetValue(element.KnotenIds[1], out knoten)) { }
        var endPunkt = TransformKnoten(knoten, auflösung, maxY);

        var vector = new Vector(endPunkt.X, endPunkt.Y) - new Vector(startPunkt.X, startPunkt.Y);
        var lastPunkt = (Point)(punktlast.Offset * vector);

        lastPunkt.X = startPunkt.X + lastPunkt.X;
        lastPunkt.Y = startPunkt.Y + lastPunkt.Y;

        endPunkt = new Point(lastPunkt.X - punktlast.Lastwerte[0] * lastAuflösung,
            -lastPunkt.Y + punktlast.Lastwerte[1] * lastAuflösung);
        pathFigure.StartPoint = endPunkt;

        pathFigure.Segments.Add(new LineSegment(lastPunkt, true));

        vector = lastPunkt - endPunkt;
        vector.Normalize();
        vector *= lastPfeilGroesse;
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
        var linienLastAuflösung = linienkraftÜberhöhung * lastAuflösung;

        last.SetzElementlastReferenzen(modell);
        if (modell.Elemente.TryGetValue(linienlast.ElementId, out var element)) { }
        if (element == null) return pathGeometry;

        if (modell.Knoten.TryGetValue(element.KnotenIds[0], out knoten)) { }
        var startPunkt = TransformKnoten(knoten, auflösung, maxY);

        // zweiter Elementknoten 
        if (modell.Knoten.TryGetValue(element.KnotenIds[1], out knoten)) { }
        var endPunkt = TransformKnoten(knoten, auflösung, maxY);
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

        // schliess pathFigure zum Füllen
        pathFigure.IsClosed = true;
        pathGeometry.Figures.Add(pathFigure);
        return pathGeometry;
    }
    public void LastTexte()
    {
        foreach (var item in modell.Lasten)
        {
            if (item.Value is not { }) continue;
            var id = new TextBlock
            {
                FontSize = 12,
                Text = item.Key,
                Foreground = Red
            };
            if (modell.Knoten.TryGetValue(item.Value.KnotenId, out var lastKnoten))
            {
                plazierungText = TransformKnoten(lastKnoten, auflösung, maxY);
                const int knotenOffset = 20;
                SetTop(id, plazierungText.Y + plazierungV - knotenOffset);
                SetLeft(id, plazierungText.X + plazierungH);
                visual.Children.Add(id);
                LastIDs.Add(id);
            }
        }
        foreach (var item in modell.ElementLasten.
                     Where(item => item.Value is LinienLast))
        {
            const int elementOffset = -20;

            var id = new TextBlock
            {
                FontSize = 12,
                Text = item.Key,
                Foreground = Red
            };
            var plazierung = ((Vector)TransformKnoten(item.Value.Element.Knoten[0], auflösung, maxY)
                              +(Vector)TransformKnoten(item.Value.Element.Knoten[1], auflösung, maxY))/2;
            plazierungText = (Point)plazierung;
            SetTop(id, plazierungText.Y + plazierungV + elementOffset);
            SetLeft(id, plazierungText.X + plazierungH);
            visual.Children.Add(id);
            LastIDs.Add(id);
        }
        foreach (var item in modell.PunktLasten)
        {
            if (item.Value is not PunktLast last) continue;
            var id = new TextBlock
            {
                FontSize = 12,
                Text = item.Key,
                Foreground = Red
            };

            var startPoint = TransformKnoten(last.Element.Knoten[0], auflösung, maxY);
            var endPoint = TransformKnoten(last.Element.Knoten[1], auflösung, maxY);
            plazierungText = startPoint + (endPoint- startPoint)*last.Offset;
            const int knotenOffset = 15;
            SetTop(id, plazierungText.Y + plazierungV + knotenOffset);
            SetLeft(id, plazierungText.X + plazierungH);
            visual.Children.Add(id);
            LastIDs.Add(id);
        }
    }

    public void LagerZeichnen()
    {
        foreach (var item in modell.Randbedingungen)
        {
            var lager = item.Value;
            var pathGeometry = new PathGeometry();

            if (modell.Knoten.TryGetValue(lager.KnotenId, out var lagerKnoten)) { }
            var drehPunkt = TransformKnoten(lagerKnoten, auflösung, maxY);
            double drehWinkel = 0;
            bool links = false, unten = false, rechts = false, balken = false;

            if (lagerKnoten != null)
            {
                if (Math.Abs(lagerKnoten.Koordinaten[0] - minX) < double.Epsilon) links = true;
                else if (Math.Abs(lagerKnoten.Koordinaten[0] - maxX) < double.Epsilon) rechts = true;
                if (Math.Abs(lagerKnoten.Koordinaten[1] - minY) < double.Epsilon) unten = true;

                if (Math.Abs(maxY - minY) < double.Epsilon) balken = true;
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
            }

            Shape path = new Path()
            {
                Name = lager.RandbedingungId,
                Stroke = Green,
                StrokeThickness = 2,
                Data = pathGeometry
            };
            LagerDarstellung.Add(path);

            // setz oben/links Position zum Zeichnen auf dem Canvas
            SetLeft(path, plazierungH);
            SetTop(path, plazierungV);
            // zeichne Shape
            visual.Children.Add(path);
        }
    }
    private PathGeometry EineFesthaltungZeichnen(Knoten lagerKnoten)
    {
        var pathGeometry = new PathGeometry();
        var pathFigure = new PathFigure();
        const int lagerSymbol = 20;

        var startPoint = TransformKnoten(lagerKnoten, auflösung, maxY);
        pathFigure.StartPoint = startPoint;

        var endPoint = new Point(startPoint.X - lagerSymbol, startPoint.Y + lagerSymbol);
        pathFigure.Segments.Add(new LineSegment(endPoint, true));
        endPoint = new Point(endPoint.X + 2 * lagerSymbol, startPoint.Y + lagerSymbol);
        pathFigure.Segments.Add(new LineSegment(endPoint, true));
        pathFigure.Segments.Add(new LineSegment(startPoint, true));

        startPoint = new Point(endPoint.X + 5, endPoint.Y + 5);
        pathFigure.Segments.Add(new LineSegment(startPoint, false));
        endPoint = new Point(startPoint.X - 50, startPoint.Y);
        pathFigure.Segments.Add(new LineSegment(endPoint, true));

        pathGeometry.Figures.Add(pathFigure);
        return pathGeometry;
    }
    private PathGeometry ZweiFesthaltungenZeichnen(Knoten lagerKnoten)
    {
        var pathGeometry = new PathGeometry();
        var pathFigure = new PathFigure();
        const int lagerSymbol = 20;

        var startPoint = TransformKnoten(lagerKnoten, auflösung, maxY);
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

        pathFigure.Segments.Add(new LineSegment(new Point(startPoint.X - 10, startPoint.Y), false));
        pathFigure.Segments.Add(new LineSegment(new Point(endPoint.X - 10, endPoint.Y), true));

        pathFigure.Segments.Add(new LineSegment(new Point(startPoint.X - 20, startPoint.Y), false));
        pathFigure.Segments.Add(new LineSegment(new Point(endPoint.X - 20, endPoint.Y), true));

        pathFigure.Segments.Add(new LineSegment(new Point(startPoint.X - 30, startPoint.Y), false));
        pathFigure.Segments.Add(new LineSegment(new Point(endPoint.X - 30, endPoint.Y), true));

        pathFigure.Segments.Add(new LineSegment(new Point(startPoint.X - 40, startPoint.Y), false));
        pathFigure.Segments.Add(new LineSegment(new Point(endPoint.X - 40, endPoint.Y), true));

        pathGeometry.Figures.Add(pathFigure);
        return pathGeometry;
    }
    private PathGeometry DreiFesthaltungenZeichnen(Knoten lagerKnoten)
    {
        var pathGeometry = new PathGeometry();
        var pathFigure = new PathFigure();
        const int lagerSymbol = 20;

        var startPoint = TransformKnoten(lagerKnoten, auflösung, maxY);

        startPoint = new Point(startPoint.X - lagerSymbol, startPoint.Y);
        pathFigure.StartPoint = startPoint;
        var endPoint = new Point(startPoint.X + 2 * lagerSymbol, startPoint.Y);
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
            startPoint = new Point(startPoint.X + 10, startPoint.Y);
            pathFigure.StartPoint = startPoint;
            endPoint = new Point(startPoint.X - 10, startPoint.Y + 10);
            pathFigure.Segments.Add(new LineSegment(endPoint, true));
            pathGeometry.Figures.Add(pathFigure);
        }
        return pathGeometry;
    }
    public void LagerTexte()
    {
        foreach (var item in modell.Randbedingungen)
        {
            if (item.Value is not Lager) continue;
            var id = new TextBlock
            {
                FontSize = 12,
                Text = item.Key,
                Foreground = Green
            };
            item.Value.SetzRandbedingungenReferenzen(modell);
            plazierungText = TransformKnoten(item.Value.Knoten, auflösung, maxY);
            const int supportSymbol = 25;
            SetTop(id, plazierungText.Y + plazierungV + supportSymbol);
            SetLeft(id, plazierungText.X + plazierungH);
            visual.Children.Add(id);
            LagerIDs.Add(id);
        }
    }

    public void AnfangsbedingungenZeichnen(string knotenId, double knotenwert, string anf)
    {
        const int randOffset = 15;
        // zeichne den Wert einer Anfangsbedingung als Text an Knoten

        if (modell.Knoten.TryGetValue(knotenId, out knoten)) { }
        var fensterKnoten = TransformKnoten(knoten, auflösung, maxY);

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
        visual.Children.Add(anfangsbedingung);
        Anfangsbedingungen.Add(anfangsbedingung);
    }
    public void AnfangsbedingungenEntfernen()
    {
        foreach (var item in Anfangsbedingungen) visual.Children.Remove(item);
        Anfangsbedingungen.Clear();
    }

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
        {
            normalkraft2Skaliert = element.ElementZustand[1] / maxNormalkraft * MaxNormalkraftScreen;
        }
        else
        {
            normalkraft2Skaliert = element.ElementZustand[3] / maxNormalkraft * MaxNormalkraftScreen;
        }

        Point nextPoint;
        Vector vec, vec2;
        var rot = FromArgb(120, 255, 0, 0);
        var blau = FromArgb(120, 0, 0, 255);

        if (modell.Knoten.TryGetValue(element.KnotenIds[0], out knoten)) { }
        var startPoint = TransformKnoten(knoten, auflösung, maxY);

        if (modell.Knoten.TryGetValue(element.KnotenIds[1], out knoten)) { }
        var endPoint = TransformKnoten(knoten, auflösung, maxY);

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

            Shape path = new Path()
            {
                Fill = myBrush,
                Stroke = Black,
                StrokeThickness = 1,
                Data = pathGeometry
            };
            SetLeft(path, plazierungH);
            SetTop(path, plazierungV);
            visual.Children.Add(path);
            NormalkraftListe.Add(path);
        }
        else
        {
            // Anteil einer Punktlast
            double punktLastN = 0, punktLastO = 0;
            IEnumerable<PunktLast> PunktLasten()
            {
                foreach (var last in modell.PunktLasten.Select(item => (PunktLast)item.Value)
                             .Where(last => last.ElementId == element.ElementId))
                {
                    yield return last;
                }
            }
            foreach (var punktLast in PunktLasten())
            {
                punktLastN = punktLast.Lastwerte[0];
                punktLastO = punktLast.Offset;
            }

            // Anteil einer Linienlast
            IEnumerable<LinienLast> LinienLasten()
            {
                foreach (var item in modell.ElementLasten)
                {
                    if (item.Value is LinienLast linienLast && item.Value.ElementId == element.ElementId)
                    {
                        yield return linienLast;
                    }
                }
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
                    var konstant = na * punktLastO * element.balkenLänge;
                    var linear = (nb - na) * punktLastO / 2 * element.balkenLänge;
                    if (nb < na)
                    {
                        konstant = nb * punktLastO * element.balkenLänge;
                        linear = (na - nb) * (1 - punktLastO) / 2 * element.balkenLänge;
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

                Shape path = new Path()
                {
                    Fill = myBrush,
                    Stroke = Black,
                    StrokeThickness = 1,
                    Data = pathGeometry
                };
                SetLeft(path, plazierungH);
                SetTop(path, plazierungV);
                visual.Children.Add(path);
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

        if (modell.Knoten.TryGetValue(element.KnotenIds[0], out knoten)) { }
        var startPoint = TransformKnoten(knoten, auflösung, maxY);

        if (modell.Knoten.TryGetValue(element.KnotenIds[1], out knoten)) { }
        var endPoint = TransformKnoten(knoten, auflösung, maxY);

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

            Shape path = new Path()
            {
                Fill = myBrush,
                Stroke = Black,
                StrokeThickness = 1,
                Data = pathGeometry
            };
            SetLeft(path, plazierungH);
            SetTop(path, plazierungV);
            visual.Children.Add(path);
            QuerkraftListe.Add(path);
        }
        // Element hat 1 Punkt- und/oder 1 Linienlast
        else
        {
            // test, ob element Punktlast hat
            bool balkenPunktlast = false, balkenGleichlast = false;
            double punktLastQ = 0, punktLastO = 0;
            AbstraktElementLast linienLast = null;

            foreach (var item in modell.PunktLasten)
            {
                if (item.Value is not PunktLast last || item.Value.ElementId != element.ElementId) continue;
                balkenPunktlast = true;
                punktLastQ = last.Lastwerte[1];
                punktLastO = last.Offset;
                break;
            }

            // test, ob element Linienlast hat
            foreach (var item in modell.ElementLasten)
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
                var pathFigure = new PathFigure();

                // Querkraftlinie vom Start- bis zum Lastangriffspunkt
                myBrush = new SolidColorBrush(blau);
                if (querkraft1Skaliert < 0) myBrush = new SolidColorBrush(rot);

                pathFigure.StartPoint = startPoint;
                vec = endPoint - startPoint;
                vec.Normalize();
                vec2 = RotateVectorScreen(vec, -90);
                nextPoint = startPoint + vec2 * querkraft1Skaliert;
                pathFigure.Segments.Add(new LineSegment(nextPoint, true));

                nextPoint += punktLastO * (endPoint - startPoint);
                pathFigure.Segments.Add(new LineSegment(nextPoint, true));

                startPoint += punktLastO * (endPoint - startPoint);
                pathFigure.Segments.Add(new LineSegment(startPoint, true));
                pathFigure.IsClosed = true;
                pathGeometry.Figures.Add(pathFigure);
                Shape path = new Path()
                {
                    Fill = myBrush,
                    Stroke = Black,
                    StrokeThickness = 1,
                    Data = pathGeometry
                };
                SetLeft(path, plazierungH);
                SetTop(path, plazierungV);
                visual.Children.Add(path);
                QuerkraftListe.Add(path);

                // Querkraftlinie vom Lastangriffs- bis zum Endpunkt
                pathGeometry = new PathGeometry();
                pathFigure = new PathFigure();
                myBrush = new SolidColorBrush(blau);
                if (querkraft1Skaliert + punktLastQ / maxQuerkraft * MaxQuerkraftScreen > 0)
                {
                    myBrush = new SolidColorBrush(rot);
                }
                pathFigure.StartPoint = startPoint;
                nextPoint -= vec2 * punktLastQ / maxQuerkraft * MaxQuerkraftScreen;
                pathFigure.Segments.Add(new LineSegment(nextPoint, true));

                nextPoint = endPoint + vec2 * querkraft2Skaliert;
                pathFigure.Segments.Add(new LineSegment(nextPoint, true));

                pathFigure.Segments.Add(new LineSegment(endPoint, true));
                pathFigure.IsClosed = true;
                pathGeometry.Figures.Add(pathFigure);

                path = new Path()
                {
                    Fill = myBrush,
                    Stroke = Black,
                    StrokeThickness = 1,
                    Data = pathGeometry
                };
                SetLeft(path, plazierungH);
                SetTop(path, plazierungV);
                visual.Children.Add(path);
                QuerkraftListe.Add(path);
            }

            // Gleichlast auf dem Balken und ggf. Punktlast zusätzlich
            else if (balkenGleichlast)
            {
                var pathGeometry = new PathGeometry();
                var pathFigure = new PathFigure();

                myBrush = new SolidColorBrush(blau);
                if (querkraft1Skaliert < 0) myBrush = new SolidColorBrush(rot);

                // Querkraftlinie auf der linken Seite
                pathFigure.StartPoint = startPoint;
                vec = endPoint - startPoint;
                vec.Normalize();
                vec2 = RotateVectorScreen(vec, -90);
                nextPoint = startPoint + vec2 * querkraft1Skaliert;
                pathFigure.Segments.Add(new LineSegment(nextPoint, true));

                if (punktLastO < double.Epsilon)
                {
                    startPoint += 0.5 * (endPoint - startPoint);
                    pathFigure.Segments.Add(new LineSegment(startPoint, true));
                }
                else
                {
                    nextPoint += punktLastO * (endPoint - startPoint);
                    var lastAbstand = punktLastO * element.balkenLänge;
                    var qa = linienLast.Lastwerte[1];
                    var qb = linienLast.Lastwerte[3];
                    var konstant = qa * lastAbstand;
                    var linear = (qb - qa) * lastAbstand / 2;
                    if (qb < qa)
                    {
                        konstant = qb * lastAbstand;
                        linear = (qa - qb) * (1 - punktLastO) * element.balkenLänge / 2;
                    }
                    nextPoint -= vec2 * (konstant + linear) / maxQuerkraft * MaxQuerkraftScreen;
                    pathFigure.Segments.Add(new LineSegment(nextPoint, true));
                    startPoint += punktLastO * (endPoint - startPoint);
                    pathFigure.Segments.Add(new LineSegment(startPoint, true));
                }
                pathFigure.IsClosed = true;
                pathGeometry.Figures.Add(pathFigure);

                Shape path = new Path()
                {
                    Fill = myBrush,
                    Stroke = Black,
                    StrokeThickness = 1,
                    Data = pathGeometry
                };
                SetLeft(path, plazierungH);
                SetTop(path, plazierungV);
                visual.Children.Add(path);
                QuerkraftListe.Add(path);

                // Querkraftlinie auf der rechten Seite
                pathGeometry = new PathGeometry();
                pathFigure = new PathFigure();
                myBrush = new SolidColorBrush(blau);
                if (querkraft2Skaliert < 0) { myBrush = new SolidColorBrush(rot); }
                pathFigure.StartPoint = startPoint;

                if (punktLastO > double.Epsilon)
                {
                    nextPoint -= vec2 * punktLastQ / maxQuerkraft * MaxQuerkraftScreen;
                    pathFigure.Segments.Add(new LineSegment(nextPoint, true));
                }
                nextPoint = endPoint + vec2 * querkraft2Skaliert;
                pathFigure.Segments.Add(new LineSegment(nextPoint, true));
                pathFigure.Segments.Add(new LineSegment(endPoint, true));

                pathFigure.IsClosed = true;
                pathGeometry.Figures.Add(pathFigure);

                path = new Path()
                {
                    Fill = myBrush,
                    Stroke = Black,
                    StrokeThickness = 1,
                    Data = pathGeometry
                };
                SetLeft(path, plazierungH);
                SetTop(path, plazierungV);
                visual.Children.Add(path);
                QuerkraftListe.Add(path);
            }
        }
    }
    public void Momente_Zeichnen(AbstraktBalken element, double skalierungMoment, bool elementlast)
    {
        if (element is Fachwerk) return;
        var moment1Skaliert = element.ElementZustand[2] / skalierungMoment * MaxMomentScreen;
        var moment2Skaliert = element.ElementZustand[5] / skalierungMoment * MaxMomentScreen;

        var rot = FromArgb(120, 255, 0, 0);
        var blau = FromArgb(120, 0, 0, 255);

        if (modell.Knoten.TryGetValue(element.KnotenIds[0], out knoten)) { }
        var startPunkt = TransformKnoten(knoten, auflösung, maxY);

        if (modell.Knoten.TryGetValue(element.KnotenIds[1], out knoten)) { }
        var endPunkt = TransformKnoten(knoten, auflösung, maxY);

        double punktLastO = 0;
        var pathGeometry = new PathGeometry();
        var pathFigure = new PathFigure();

        var myBrush = new SolidColorBrush(blau);
        if ((int)moment1Skaliert < 0) { myBrush = new SolidColorBrush(rot); }
        else if ((int)moment1Skaliert == 0) { if ((int)moment2Skaliert < 0) { myBrush = new SolidColorBrush(rot); } }

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

            Shape path = new Path()
            {
                Fill = myBrush,
                Stroke = Black,
                StrokeThickness = 1,
                Data = pathGeometry
            };
            SetLeft(path, plazierungH);
            SetTop(path, plazierungV);
            visual.Children.Add(path);
            MomenteListe.Add(path);
        }

        // Elementlasten (Linienlast, Punktlast) vorhanden
        // Element hat Punkt- und/oder Linienlast
        else
        {
            bool elementHatPunktLast = false, elementHatLinienLast = false;
            LinienLast linienLast = null;

            // finde Punktlast auf Balkenelement
            foreach (var item in modell.PunktLasten)
            {
                if (item.Value is not PunktLast last || item.Value.ElementId != element.ElementId) continue;
                punktLastO = last.Offset;
                elementHatPunktLast = true;
                break;
            }

            var maxPunkt = new Point(0, 0);
            double mmax = 0;

            // finde Linienlast auf Balkenelement
            foreach (var item in modell.ElementLasten)
            {
                if (item.Value is not LinienLast last || item.Value.ElementId != element.ElementId) continue;
                linienLast = last;
                elementHatLinienLast = true;
                break;
            }

            // zeichne Momentenlinie, nur Punkt-, keine Linienlast
            if (elementHatPunktLast && !elementHatLinienLast)
            {
                // Linie von Moment1 skaliert nach Mmax skaliert
                mmax = element.ElementZustand[2] - (element.ElementZustand[1] * punktLastO * element.balkenLänge);
                var mmaxSkaliert = mmax / skalierungMoment * MaxMomentScreen;

                maxPunkt = startPunkt + (vec * punktLastO * element.balkenLänge) * auflösung + vec2 * mmaxSkaliert;
                pathFigure.Segments.Add(new LineSegment(maxPunkt, true));

                //Linie von Mmax skaliert nach Moment2 skaliert
                nächsterPunkt = endPunkt + vec2 * moment2Skaliert;
                pathFigure.Segments.Add(new LineSegment(nächsterPunkt, true));

                // Linie nach end und anschliessend pathFigure schliessen
                pathFigure.Segments.Add(new LineSegment(endPunkt, true));
            }

            // zeichne Momentenlinie unter Gleich- und/oder Dreieckslast
            else if (elementHatLinienLast)
            {
                var qa = linienLast.Lastwerte[1];
                var qb = linienLast.Lastwerte[3];
                var l = element.balkenLänge;
                const double kontrollAbstand = 1;
                double abstandMmax, konstant, linear;

                // konstante Last oder linear steigende Dreieckslast
                if (Math.Abs(qb) >= Math.Abs(qa))
                {
                    var q = qb - qa;
                    abstandMmax = l / 2;
                    if (Math.Abs(q) > double.Epsilon)
                    {
                        abstandMmax = (-qa / q + Math.Sqrt(Math.Abs(Math.Pow(qa / q, 2)
                                                                    + 2 / l / q * element.ElementZustand[1]))) * l;
                    }

                    konstant = qa * abstandMmax;
                    linear = q / l * abstandMmax * abstandMmax / 2;
                    mmax = element.ElementZustand[2] + element.ElementZustand[1] * abstandMmax
                           + konstant * abstandMmax / 2
                           + linear * abstandMmax / 3;
                }
                // linear fallende Dreieckslast
                else
                {
                    // lokale Koordinate vom Balkenende
                    var q = qa - qb;
                    abstandMmax = (-qb / q + Math.Sqrt(Math.Abs(Math.Pow(qb / q, 2)
                                                                + 2 / l / q * element.ElementZustand[4]))) * l;
                    konstant = qb * abstandMmax;
                    linear = q / l * abstandMmax * abstandMmax / 2;
                    mmax = element.ElementZustand[5] + element.ElementZustand[4] * abstandMmax
                                                     + konstant * abstandMmax / 2
                                                     + linear * abstandMmax / 3;
                    abstandMmax = l - abstandMmax;
                }

                var mmaxSkaliert = mmax / skalierungMoment * MaxMomentScreen;

                // zeichne Momentenlinie als quadratischen Bezier-Spline
                // nur Linien-, keine Punktlast
                if (!elementHatPunktLast)
                {
                    // maxPunkt an maximalem Moment ist Kontrollpunkt
                    maxPunkt = startPunkt + abstandMmax / element.balkenLänge * (endPunkt - startPunkt)
                               - vec2 * kontrollAbstand * mmaxSkaliert;
                    nächsterPunkt = endPunkt + vec2 * moment2Skaliert;
                    pathFigure.Segments.Add(new QuadraticBezierSegment(maxPunkt, nächsterPunkt, true));
                    pathFigure.Segments.Add(new LineSegment(endPunkt, true));
                    // maxPunkt des Bezier-Spline (für Text) ist etwa 1/2 des Kontrollpunkts
                    maxPunkt.Y /= 2;
                }

                // Element hat Punktlast
                else
                {
                    double m1, m2, deltaM1, deltaM2;
                    var abstandPunktlast = punktLastO * element.balkenLänge;
                    var abstand1 = abstandPunktlast / 2;
                    var abstand2 = (l - abstandPunktlast) / 2;

                    // Unstetigkeit an Punktlast, Momentenlinie durch 2 quadratrische Bezier-Segmente
                    // qa <= qb   Gleichlast oder Dreieckslast linear steigend
                    if (Math.Abs(qb) >= Math.Abs(qa))
                    {
                        var q = qb - qa;
                        konstant = qa * abstandPunktlast;
                        linear = q / l * abstandPunktlast * abstandPunktlast / 2;
                        mmax = element.ElementZustand[2] - element.ElementZustand[1] * abstandPunktlast
                               + konstant * abstandPunktlast / 2
                               + linear * abstandPunktlast / 3;

                        // Moment in Mittelpunkt des 1. Segment (links von Punktlast)
                        konstant = qa * abstand1;
                        linear = q / l * abstand1 * abstand1 / 2;
                        m1 = element.ElementZustand[2] - element.ElementZustand[1] * abstand1
                             + konstant * abstand1 / 2
                             + linear * abstand1 / 3;
                        deltaM1 = m1 - (element.ElementZustand[2] + (Math.Abs(element.ElementZustand[2]) + mmax) / 2);

                        // Moment in Mittelpunkt des 2. Segment (rechts von Punktlast)
                        var lastOrdinate = qa + q * (1 - abstand2 / l);
                        konstant = lastOrdinate * abstand2;
                        linear = (qb - lastOrdinate) * abstand2 / 2;
                        m2 = element.ElementZustand[5] + element.ElementZustand[4] * abstand2
                                                       + konstant * abstand2 / 2
                                                       + linear * abstand2 * 2 / 3;
                        deltaM2 = m2 - (element.ElementZustand[5] + (Math.Abs(element.ElementZustand[5]) + mmax) / 2);
                    }

                    // Dreieckslast linear fallend, lokale Koordinate von rechts
                    else
                    {
                        var q = qa - qb;
                        konstant = qb * abstandPunktlast;
                        linear = q / l * abstandPunktlast * abstandPunktlast / 2;
                        mmax = element.ElementZustand[5] + element.ElementZustand[4] * abstandPunktlast
                                                         + konstant * abstandPunktlast / 2
                                                         + linear * abstandPunktlast / 3;

                        // Moment in Mittelpunkt des 2. Segment (rechts von Punktlast)
                        konstant = qb * abstand2;
                        linear = q / l * abstand2 * abstand2 / 2;
                        m2 = element.ElementZustand[5] + element.ElementZustand[4] * abstand2
                                                       + konstant * abstand2 / 2
                                                       + linear * abstand2 / 3;
                        deltaM2 = m2 - (element.ElementZustand[5] - (Math.Abs(element.ElementZustand[5]) + mmax) / 2);

                        // Moment in Mittelpunkt des 1. Segment (links von Punktlast)
                        var lastOrdinate = qb + q * (1 - abstand1 / l);
                        konstant = lastOrdinate * abstand1;
                        linear = (qa - lastOrdinate) * abstand1 / 2;
                        m1 = element.ElementZustand[2] - element.ElementZustand[1] * abstand1
                             + konstant * abstand1 / 2
                             + linear * abstand1 * 2 / 3;
                        deltaM1 = m1 - (element.ElementZustand[2] + (Math.Abs(element.ElementZustand[2]) + mmax) / 2);
                    }

                    maxPunkt = startPunkt + punktLastO * (endPunkt - startPunkt)
                                          + vec2 * mmax / skalierungMoment * MaxMomentScreen;
                    var kontrollVektor = (startPunkt - maxPunkt);
                    kontrollVektor.Normalize();
                    kontrollVektor = RotateVectorScreen(kontrollVektor, -90);
                    var kontrollPunkt1 = startPunkt + (maxPunkt - startPunkt) / 2
                                                    + kontrollVektor * kontrollAbstand * deltaM1 / skalierungMoment * MaxMomentScreen;

                    kontrollVektor = (endPunkt - maxPunkt);
                    kontrollVektor.Normalize();
                    kontrollVektor = RotateVectorScreen(kontrollVektor, -90);
                    var kontrollPunkt2 = endPunkt - (endPunkt - maxPunkt) / 2
                                                  - kontrollVektor * kontrollAbstand * deltaM2 / skalierungMoment * MaxMomentScreen;

                    nächsterPunkt = endPunkt + vec2 * moment2Skaliert;
                    // Startpunkt ist Endpunkt in PathFigure
                    // Kontrollpunkt1 für Auslenkung in Mitte des 1. Segment, maxPunkt am Ende des 1. Segments,
                    // Kontrollpunkt2 für Auslenkung in Mitte des 2. Segment, Endpunkt der Kurve
                    var bezierPoints = new PointCollection(4)
                    {
                        kontrollPunkt1,
                        maxPunkt,
                        kontrollPunkt2,
                        nächsterPunkt
                    };
                    pathFigure.Segments.Add(new PolyQuadraticBezierSegment(bezierPoints, true));
                    pathFigure.Segments.Add(new LineSegment(endPunkt, true));
                }
            }

            pathFigure.IsClosed = true;
            pathGeometry.Figures.Add(pathFigure);

            Shape path = new Path()
            {
                Fill = myBrush,
                Stroke = Black,
                StrokeThickness = 1,
                Data = pathGeometry
            };
            SetLeft(path, plazierungH);
            SetTop(path, plazierungV);
            visual.Children.Add(path);
            MomenteListe.Add(path);

            maxMomentText = new TextBlock
            {
                FontSize = 12,
                Text = "Moment = " + mmax.ToString("G4"),
                Foreground = Blue
            };
            SetTop(maxMomentText, maxPunkt.Y + plazierungV);
            SetLeft(maxMomentText, maxPunkt.X);
            visual.Children.Add(maxMomentText);
            MaxTexte.Add(maxMomentText);
        }
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
            var point = new Point(dt * i * auflösungH, -ordinaten[i + start] * auflösungV);
            stützpunkte.Add(point);
        }
        zeitverlauf.Points = stützpunkte;

        // setz oben/links Position zum Zeichnen auf dem Canvas
        SetLeft(zeitverlauf, RandLinks);
        SetTop(zeitverlauf, mY * auflösungV + plazierungV);
        // zeichne Shape
        visual.Children.Add(zeitverlauf);
    }
    public void Koordinatensystem(double tmin, double tmax, double max, double min)
    {
        const int rand = 20;
        screenH = visual.ActualWidth;
        screenV = visual.ActualHeight;
        if (double.IsPositiveInfinity(max)) auflösungV = screenV - rand;
        else auflösungV = (screenV - rand) / (max - min);
        auflösungH = (screenH - rand) / (tmax - tmin);
        var xAchse = new Line
        {
            Stroke = Black,
            X1 = 0,
            Y1 = max * auflösungV + plazierungV,
            X2 = (tmax - tmin) * auflösungH + plazierungH,
            Y2 = max * auflösungV + plazierungV,
            StrokeThickness = 2
        };
        _ = visual.Children.Add(xAchse);
        var yAchse = new Line
        {
            Stroke = Black,
            X1 = RandLinks,
            Y1 = max * auflösungV - min * auflösungV + 2 * plazierungV,
            X2 = RandLinks,
            Y2 = plazierungV,
            StrokeThickness = 2
        };
        visual.Children.Add(yAchse);
    }
    private static Vector RotateVectorScreen(Vector vec, double winkel)  // clockwise in degree
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
        // eingabeEinheit z.B. in m, verformungsEinheit z.B. cm --> Überhöhung
        return new Point((verformt.Koordinaten[0] + verformt.Knotenfreiheitsgrade[0] * überhöhungVerformung) * resolution,
            (-verformt.Koordinaten[1] - verformt.Knotenfreiheitsgrade[1] * überhöhungVerformung + max) * resolution);
    }
    public double[] TransformBildPunkt(Point point)
    {
        var koordinaten = new double[2];
        koordinaten[0] = (point.X - plazierungH) / auflösung;
        koordinaten[1] = (-point.Y + plazierungV) / auflösung + maxY;
        return koordinaten;
    }
    public Point TransformKnotenBildPunkt(double[] koordinaten)
    {
        var bildPunkt = new Point
        {
            X = koordinaten[0] * auflösung + plazierungH,
            Y = (-koordinaten[1] + maxY) * auflösung + plazierungV
        };
        return bildPunkt;
    }
}