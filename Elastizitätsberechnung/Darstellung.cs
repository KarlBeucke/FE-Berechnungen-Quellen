using FE_Berechnungen.Elastizitätsberechnung.Modelldaten;
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

namespace FE_Berechnungen.Elastizitätsberechnung
{
    public class Darstellung
    {
        private readonly FEModell modell;
        private Knoten knoten;
        private readonly Canvas visualErgebnisse;
        private double screenH, screenV;
        private double minX, maxX, minY, maxY;
        private double auflösungH;
        private double lastAuflösung;
        private const double Eps = 1.0E-10;
        private int randOben = 60;
        private const int RandLinks = 60;
        private const double PlazierungText = 45;
        private double auflösung;
        public double überhöhungVerformung = 1;
        private readonly double maxScreenLength = 40;
        public List<object> ElementIDs { get; }
        public List<object> KnotenIDs { get; }
        public List<object> Verformungen { get; }
        public List<object> LastVektoren { get; }
        public List<object> LagerDarstellung { get; }
        public List<object> Spannungen { get; }
        public List<object> Reaktionen { get; }
        private double vektorskalierung;

        public Darstellung(FEModell feModell, Canvas visual)
        {
            modell = feModell;
            visualErgebnisse = visual;
            ElementIDs = new List<object>();
            KnotenIDs = new List<object>();
            Verformungen = new List<object>();
            LastVektoren = new List<object>();
            LagerDarstellung = new List<object>();
            Spannungen = new List<object>();
            Reaktionen = new List<object>();
            FestlegungAuflösung();
        }
        private void FestlegungAuflösung()
        {
            screenH = visualErgebnisse.ActualWidth;
            screenV = visualErgebnisse.ActualHeight;

            var x = new List<double>();
            var y = new List<double>();
            foreach (var item in modell.Knoten)
            {
                x.Add(item.Value.Koordinaten[0]);
                maxX = x.Max(); minX = x.Min();
                y.Add(item.Value.Koordinaten[1]);
                maxY = y.Max(); minY = y.Min();
            }

            var delta = Math.Abs(maxX - minX);
            if (delta < 1)
            {
                auflösungH = screenH - 5 * RandLinks;
            }
            else
            {
                auflösungH = (screenH - 5 * RandLinks) / delta;
            }

            delta = Math.Abs(maxY - minY);
            if (delta < 1)
            {
                auflösung = screenV - 5 * randOben;
                randOben = (int)(0.5 * screenV);
            }
            else
            {
                auflösung = (screenV - 5 * randOben) / delta;
            }
            if (auflösungH < auflösung) auflösung = auflösungH;
        }

        public void KnotenTexte()
        {
            foreach (var item in modell.Knoten)
            {
                var id = new TextBlock
                {
                    Name = item.Key,
                    FontSize = 12,
                    Text = item.Key,
                    Foreground = Red
                };
                SetTop(id, (-item.Value.Koordinaten[1] + maxY) * auflösung + PlazierungText);
                SetLeft(id, item.Value.Koordinaten[0] * auflösung + RandLinks);
                visualErgebnisse.Children.Add(id);
                KnotenIDs.Add(id);
            }
        }
        public void ElementTexte()
        {
            foreach (var item in modell.Elemente)
            {
                Abstrakt2D element = (Abstrakt2D)item.Value;
                var cg = element.BerechneSchwerpunkt();
                var id = new TextBlock
                {
                    Name = item.Key,
                    FontSize = 12,
                    Text = item.Key,
                    Foreground = Blue
                };
                SetTop(id, (-cg.Y + maxY) * auflösung + PlazierungText);
                SetLeft(id, cg.X * auflösung + RandLinks);
                visualErgebnisse.Children.Add(id);
                ElementIDs.Add(id);
            }
        }

        public void ElementeZeichnen()
        {
            foreach (var item in modell.Elemente)
            {
                AktElementZeichnen(item.Value);
            }
        }
        private void PolygonZeichnen(AbstraktElement elementMultiK, PointCollection umriss)
        {
            var elementPolygon = new Polygon
            {
                Name = elementMultiK.ElementId,
                Stroke = Black,
                Points = umriss,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
                StrokeThickness = 2
            };
            SetLeft(elementPolygon, RandLinks);
            SetTop(elementPolygon, randOben);
            visualErgebnisse.Children.Add(elementPolygon);
        }
        private void AktElementZeichnen(AbstraktElement element)
        {
            // Knoten am Elementanfang
            if (modell.Knoten.TryGetValue(element.KnotenIds[0], out var node)) { }
            var startPunkt = TransformKnoten(node, auflösung, maxY);

            switch (element)
            {
                // Elemente mit mehreren Knoten
                default:
                    {
                        // PointCollection für Polygondarstellung
                        var elementPointCollection = new PointCollection { startPunkt };
                        for (var i = 1; i < element.KnotenIds.Length; i++)
                        {
                            if (modell.Knoten.TryGetValue(element.KnotenIds[i], out node)) { }
                            var endPunkt = TransformKnoten(node, auflösung, maxY);
                            elementPointCollection.Add(endPunkt);
                        }
                        PolygonZeichnen(element, elementPointCollection);
                        return;
                    }
            }
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
            //int überhöhung = 1;
            //const int rotationÜberhöhung = 1;
            var pathGeometry = new PathGeometry();

            IEnumerable<AbstraktElement> Elements()
            {
                foreach (var item in modell.Elemente)
                {
                    if (item.Value is AbstraktElement element)
                    {
                        yield return element;
                    }
                }
            }
            foreach (var element in Elements())
            {
                //element.ElementState = element.ComputeElementState();
                var pathFigure = new PathFigure();

                switch (element)
                {
                    case Element2D3 _:
                        {
                            if (modell.Knoten.TryGetValue(element.KnotenIds[0], out knoten)) { }
                            var start = TransformVerformtenKnoten(knoten, auflösung, maxY);
                            pathFigure.StartPoint = start;

                            for (var i = 1; i < element.KnotenIds.Length; i++)
                            {
                                if (modell.Knoten.TryGetValue(element.KnotenIds[i], out knoten)) { }
                                var end = TransformVerformtenKnoten(knoten, auflösung, maxY);
                                pathFigure.Segments.Add(new LineSegment(end, true));
                            }
                            break;
                        }
                }
                if (element.KnotenIds.Length > 2) pathFigure.IsClosed = true;
                pathGeometry.Figures.Add(pathFigure);
            }

            // alle Elemente werden der GeometryGroup tragwerk hinzugefügt
            var tragwerk = new GeometryGroup();
            tragwerk.Children.Add(pathGeometry);

            Shape path = new Path()
            {
                Stroke = Red,
                StrokeThickness = 1,
                Data = tragwerk
            };
            // setz oben/links Position zum Zeichnen auf dem Canvas
            SetLeft(path, RandLinks);
            SetTop(path, randOben);
            // zeichne Shape
            visualErgebnisse.Children.Add(path);
            Verformungen.Add(path);
        }

        public void LastenZeichnen()
        {
            AbstraktLast last;
            Shape path;

            // Knotenlasten
            double maxLastWert = 1;
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
            foreach (var linienLast in modell.ElementLasten.Select(item => (AbstraktLinienlast)item.Value))
            {
                if (Math.Abs(linienLast.Lastwerte[0]) > maxLastWert) maxLastWert = Math.Abs(linienLast.Lastwerte[0]);
                if (Math.Abs(linienLast.Lastwerte[1]) > maxLastWert) maxLastWert = Math.Abs(linienLast.Lastwerte[1]);
            }
            lastAuflösung = maxLastScreen / maxLastWert;

            foreach (var item in modell.Lasten)
            {
                last = item.Value;
                var pathGeometry = KnotenlastZeichnen(last);
                path = new Path()
                {
                    Name = last.LastId,
                    Stroke = Red,
                    StrokeThickness = 3,
                    Data = pathGeometry
                };
                LastVektoren.Add(path);

                SetLeft(path, RandLinks);
                SetTop(path, randOben);
                visualErgebnisse.Children.Add(path);
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

                SetLeft(path, RandLinks);
                SetTop(path, randOben);
                visualErgebnisse.Children.Add(path);
            }
        }
        private PathGeometry KnotenlastZeichnen(AbstraktLast knotenlast)
        {
            var pathGeometry = new PathGeometry();
            var pathFigure = new PathFigure();
            const int lastPfeilGroesse = 10;

            if (modell.Knoten.TryGetValue(knotenlast.KnotenId, out var lastKnoten)) { }

            if (lastKnoten != null)
            {
                var endPoint = new Point(lastKnoten.Koordinaten[0] * auflösung - knotenlast.Lastwerte[0] * lastAuflösung,
                                         (-lastKnoten.Koordinaten[1] + maxY) * auflösung + knotenlast.Lastwerte[1] * lastAuflösung);
                pathFigure.StartPoint = endPoint;

                var startPoint = TransformKnoten(lastKnoten, auflösung, maxY);
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

                if (knotenlast.Lastwerte.Length > 2 && Math.Abs(knotenlast.Lastwerte[2]) > Eps)
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

            if (modell.Knoten.TryGetValue(element.KnotenIds[0], out var startKnoten)) { }
            var startPunkt = TransformKnoten(startKnoten, auflösung, maxY);

            // zweiter Elementknoten 
            if (modell.Knoten.TryGetValue(element.KnotenIds[1], out var endKnoten)) { }
            var endPunkt = TransformKnoten(endKnoten, auflösung, maxY);
            var vector = endPunkt - startPunkt;

            pathFigure.StartPoint = startPunkt;

            var lastVektor = RotateVectorScreen(vector, -90);
            lastVektor.Normalize();
            var vec = lastVektor * linienLastAuflösung * linienlast.Lastwerte[1];
            var nextPunkt = new Point(startPunkt.X - vec.X, startPunkt.Y - vec.Y);

            lastVektor *= lastPfeilGroesse;
            lastVektor = RotateVectorScreen(lastVektor, -150);
            var punkt = new Point(startPunkt.X - lastVektor.X, startPunkt.Y - lastVektor.Y);
            pathFigure.Segments.Add(new LineSegment(punkt, true));

            lastVektor = RotateVectorScreen(lastVektor, -60);
            punkt = new Point(startPunkt.X - lastVektor.X, startPunkt.Y - lastVektor.Y);
            pathFigure.Segments.Add(new LineSegment(punkt, false));
            pathFigure.Segments.Add(new LineSegment(startPunkt, true));
            pathFigure.Segments.Add(new LineSegment(nextPunkt, true));

            lastVektor = RotateVectorScreen(vector, 90);
            lastVektor.Normalize();
            vec = lastVektor * linienLastAuflösung * linienlast.Lastwerte[1];
            nextPunkt = new Point(endPunkt.X + vec.X, endPunkt.Y + vec.Y);
            pathFigure.Segments.Add(new LineSegment(nextPunkt, true));
            pathFigure.Segments.Add(new LineSegment(endPunkt, true));

            lastVektor *= lastPfeilGroesse;
            lastVektor = RotateVectorScreen(lastVektor, 30);
            nextPunkt = new Point(endPunkt.X - lastVektor.X, endPunkt.Y - lastVektor.Y);
            pathFigure.Segments.Add(new LineSegment(nextPunkt, true));

            lastVektor = RotateVectorScreen(lastVektor, -60);
            nextPunkt = new Point(endPunkt.X - lastVektor.X, endPunkt.Y - lastVektor.Y);
            pathFigure.Segments.Add(new LineSegment(nextPunkt, false));
            pathFigure.Segments.Add(new LineSegment(endPunkt, true));
            pathFigure.Segments.Add(new LineSegment(startPunkt, false));

            pathGeometry.Figures.Add(pathFigure);

            return pathGeometry;
        }

        public void FesthaltungenZeichnen()
        {
            foreach (var item in modell.Randbedingungen)
            {
                var lager = item.Value;
                var pathGeometry = new PathGeometry();

                if (modell.Knoten.TryGetValue(lager.KnotenId, out var lagerKnoten)) { }
                var drehPunkt = TransformKnoten(lagerKnoten, auflösung, maxY);

                switch (lager.Typ)
                {
                    // X_FIXED = 1, Y_FIXED = 2, R_FIXED = 4, XY_FIXED = 3, 
                    // XR_FIXED = 5, YR_FIXED = 6, XYR_FIXED = 7
                    case 1:
                        {
                            pathGeometry = EineFesthaltungZeichnen(lagerKnoten);
                            double drehWinkel = 45;
                            if (lagerKnoten != null && (lagerKnoten.Koordinaten[0] - minX) < (maxX - lagerKnoten.Koordinaten[0])) drehWinkel = -45;
                            pathGeometry.Transform = new RotateTransform(drehWinkel, drehPunkt.X, drehPunkt.Y);
                            break;
                        }
                    case 2:
                        pathGeometry = EineFesthaltungZeichnen(lagerKnoten);
                        break;
                    case 3:
                        pathGeometry = ZweiFesthaltungenZeichnen(lagerKnoten);
                        break;
                }
                Shape path = new Path()
                {
                    Stroke = Green,
                    StrokeThickness = 2,
                    Data = pathGeometry
                };
                LagerDarstellung.Add(path);

                // setz oben/links Position zum Zeichnen auf dem Canvas
                SetLeft(path, RandLinks);
                SetTop(path, randOben);
                // zeichne Shape
                visualErgebnisse.Children.Add(path);
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

        public void SpannungenZeichnen()
        {
            double[] elementSpannung;
            double maxVektor = 0;
            foreach (var abstract2D in modell.Elemente.Select(item => (Abstrakt2D)item.Value))
            {
                elementSpannung = abstract2D.BerechneZustandsvektor();
                maxVektor = elementSpannung.Select(Math.Abs).Prepend(maxVektor).Max();
            }
            vektorskalierung = maxScreenLength / maxVektor;

            foreach (var abstract2D in modell.Elemente.Select(item => (Abstrakt2D)item.Value))
            {
                elementSpannung = abstract2D.BerechneZustandsvektor();
                var sigxx = elementSpannung[0] * vektorskalierung;
                var sigyy = elementSpannung[1] * vektorskalierung;
                var cg = abstract2D.BerechneSchwerpunkt();
                // zeichne den resultierenden Vektor mit seinem Mittelpunkt im Elementschwerpunkt
                // füge am Endpunkt Pfeilspitzen an und füge Wärmeflusspfeil zur pathGeometry hinzu
                SpannungenElemente(cg, sigxx, sigyy);
            }
        }
        private void SpannungenElemente(Point cg, double sigxx, double sigyy)
        {
            var mittelpunkt = new Point(cg.X * auflösung, (-cg.Y + maxY) * auflösung);

            // Spannungspfeil in x-Richtung
            var farbe = Black;
            var winkel = 0.0;
            var länge = Math.Abs(sigxx);
            if (sigxx < 0)
            {
                farbe = Red;
                winkel = 180.0;
            }
            if ((int)länge > 1)
            {
                var pathGeometry = Spannungspfeil(mittelpunkt, länge, winkel);
                Shape path = new Path()
                {
                    Stroke = farbe,
                    StrokeThickness = 2,
                    Data = pathGeometry
                };
                Spannungen.Add(path);

                // setz oben/links Position zum Zeichnen auf dem Canvas
                SetLeft(path, RandLinks);
                SetTop(path, randOben);
                // zeichne Shape
                visualErgebnisse.Children.Add(path);
            }

            // Spannungspfeil in y-Richtung
            farbe = Black;
            winkel = -90.0;
            länge = Math.Abs(sigyy);
            if (sigyy < 0)
            {
                farbe = Red;
                winkel = 90.0;
            }

            if ((int)länge <= 1) return;
            {
                var pathGeometry = Spannungspfeil(mittelpunkt, sigyy, winkel);
                Shape path = new Path()
                {
                    Stroke = farbe,
                    StrokeThickness = 2,
                    Data = pathGeometry
                };
                Spannungen.Add(path);

                // setz oben/links Position zum Zeichnen auf dem Canvas
                SetLeft(path, RandLinks);
                SetTop(path, randOben);
                // zeichne Shape
                visualErgebnisse.Children.Add(path);
            }
        }
        private static PathGeometry Spannungspfeil(Point punkt, double länge, double winkel)
        {
            var spannungsPfeil = new PathGeometry();
            var pathFigure = new PathFigure { StartPoint = punkt };
            var endPunkt = new Point(punkt.X + Math.Abs(länge), punkt.Y);
            pathFigure.Segments.Add(new LineSegment(endPunkt, true));
            pathFigure.Segments.Add(new LineSegment(new Point(endPunkt.X - 3, endPunkt.Y - 2), true));
            pathFigure.Segments.Add(new LineSegment(new Point(endPunkt.X - 3, endPunkt.Y + 2), true));
            pathFigure.Segments.Add(new LineSegment(new Point(endPunkt.X, endPunkt.Y), true));

            spannungsPfeil.Figures.Add(pathFigure);
            spannungsPfeil.Transform = new RotateTransform(winkel, punkt.X, punkt.Y);
            return spannungsPfeil;
        }

        public void ReaktionenZeichnen()
        {
            double[] reaktionen;
            double maxVektor = 0;
            var knotenIds = new List<string>();
            foreach (var randbedingung in modell.Randbedingungen.Select(item => item.Value))
            {
                if (knotenIds.Contains(randbedingung.KnotenId)) break;
                knotenIds.Add(randbedingung.KnotenId);
                if (!modell.Knoten.TryGetValue(randbedingung.KnotenId, out knoten)) break;
                reaktionen = knoten.Reaktionen;
                maxVektor = reaktionen.Select(Math.Abs).Prepend(maxVektor).Max();
            }

            const double maxPfeillänge = 50;
            vektorskalierung = maxPfeillänge / maxVektor;

            foreach (var randbedingung in modell.Randbedingungen.Select(item => item.Value))
            {
                if (!modell.Knoten.TryGetValue(randbedingung.KnotenId, out knoten)) break;
                reaktionen = knoten.Reaktionen;
                var kx = reaktionen[0] * vektorskalierung;
                var ky = reaktionen[1] * vektorskalierung;
                knoten = randbedingung.Knoten;
                KnotenReaktionen(knoten, kx, ky);
            }
        }
        private void KnotenReaktionen(Knoten lagerKnoten, double kx, double ky)
        {
            var punkt = new Point(lagerKnoten.Koordinaten[0] * auflösung, (-lagerKnoten.Koordinaten[1] + maxY) * auflösung);
            var farbe = Black;

            // Reaktionspfeil in x-Richtung
            if (Math.Abs(kx) > 5)
            {
                var reaktionspfeil = Reaktionspfeil(punkt, Math.Abs(kx));
                if (kx < 0)
                {
                    reaktionspfeil.Transform = new RotateTransform(180, punkt.X + kx / 2, punkt.Y);
                    farbe = Red;
                }
                Shape path = new Path()
                {
                    Stroke = farbe,
                    StrokeThickness = 3,
                    Data = reaktionspfeil
                };
                Reaktionen.Add(path);

                // setz oben/links Position zum Zeichnen auf dem Canvas
                SetLeft(path, RandLinks);
                SetTop(path, randOben);
                // zeichne Shape
                visualErgebnisse.Children.Add(path);
            }

            // Reaktionspfeil in y-Richtung
            if (!(Math.Abs(ky) > 5)) return;
            {
                var reaktionspfeil = Reaktionspfeil(punkt, ky);
                if (ky > 0)
                {
                    reaktionspfeil.Transform = new RotateTransform(-90, punkt.X, punkt.Y);
                    farbe = Black;
                }
                else
                {
                    reaktionspfeil.Transform = new RotateTransform(90, punkt.X, punkt.Y);
                    farbe = Red;
                }
                Shape path = new Path()
                {
                    Stroke = farbe,
                    StrokeThickness = 4,
                    Data = reaktionspfeil
                };
                Reaktionen.Add(path);

                // setz oben/links Position zum Zeichnen auf dem Canvas
                SetLeft(path, RandLinks);
                SetTop(path, randOben);
                // zeichne Shape
                visualErgebnisse.Children.Add(path);
            }
        }
        private static PathGeometry Reaktionspfeil(Point punkt, double länge)
        {
            var reaktionsPfeil = new PathGeometry();

            var pathFigure = new PathFigure { StartPoint = new Point(punkt.X - Math.Abs(länge), punkt.Y) };
            pathFigure.Segments.Add(new LineSegment(punkt, true));
            pathFigure.Segments.Add(new LineSegment(new Point(punkt.X - 3, punkt.Y - 2), true));
            pathFigure.Segments.Add(new LineSegment(new Point(punkt.X - 3, punkt.Y + 2), true));
            pathFigure.Segments.Add(new LineSegment(punkt, true));

            reaktionsPfeil.Figures.Add(pathFigure);
            return reaktionsPfeil;
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
        private Point TransformVerformtenKnoten(Knoten node, double resolution, double max)
        {
            // eingabeEinheit z.B. in m, verformungsEinheit z.B. cm --> Überhöhung
            return new Point((node.Koordinaten[0] + node.Knotenfreiheitsgrade[0] * überhöhungVerformung) * resolution,
                             (-node.Koordinaten[1] - node.Knotenfreiheitsgrade[1] * überhöhungVerformung + max) * resolution);
        }
    }
}