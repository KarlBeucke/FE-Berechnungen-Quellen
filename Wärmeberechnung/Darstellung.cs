using FEBibliothek.Modell;
using FEBibliothek.Modell.abstrakte_Klassen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using static System.Globalization.CultureInfo;
using static System.Windows.Controls.Canvas;
using static System.Windows.Media.Brushes;
using static System.Windows.Media.Color;

namespace FE_Berechnungen.Wärmeberechnung
{
    public class Darstellung
    {
        private readonly FeModell modell;
        private AbstraktElement aktElement;
        private Knoten knoten;
        private readonly Canvas visualErgebnisse;
        public int zeitschritt;
        private double maxX;
        private double screenH, screenV;
        private readonly double maxScreenLength = 40;
        public double auflösung;
        private double auflösungH, auflösungV;
        public double maxY;
        private double temp;
        private double minTemp = 100;
        private double maxTemp;
        private const int RandOben = 60;
        private const int RandLinks = 60;

        public List<TextBlock> ElementIDs { get; }
        public List<TextBlock> KnotenIDs { get; }
        public List<TextBlock> LastKnoten { get; }
        public List<Shape> LastElemente { get; }
        public List<TextBlock> Knotentemperaturen { get; }
        public List<TextBlock> Knotengradienten { get; }
        public List<Shape> TemperaturElemente { get; }
        public List<Shape> WärmeVektoren { get; }
        public List<TextBlock> RandKnoten { get; }

        public Darstellung(FeModell feModell, Canvas visual)
        {
            modell = feModell;
            visualErgebnisse = visual;
            KnotenIDs = new List<TextBlock>();
            ElementIDs = new List<TextBlock>();
            LastKnoten = new List<TextBlock>();
            LastElemente = new List<Shape>();
            Knotentemperaturen = new List<TextBlock>();
            Knotengradienten = new List<TextBlock>();
            TemperaturElemente = new List<Shape>();
            WärmeVektoren = new List<Shape>();
            RandKnoten = new List<TextBlock>();
            FestlegungAuflösung();
        }

        public void FestlegungAuflösung()
        {
            const int rand = 100;
            screenH = visualErgebnisse.ActualWidth;
            screenV = visualErgebnisse.ActualHeight;

            foreach (var item in modell.Knoten)
            {
                knoten = item.Value;
                if (knoten.Koordinaten[0] > maxX) maxX = knoten.Koordinaten[0];
                if (knoten.Koordinaten[1] > maxY) maxY = knoten.Koordinaten[1];
            }
            if (screenH / maxX < screenV / maxY) auflösung = (screenH - rand) / maxX;
            else auflösung = (screenV - rand) / maxY;
        }
        public void KnotenTexte()
        {
            foreach (var item in modell.Knoten)
            {
                var id = new TextBlock
                {
                    Name = "Knoten",
                    FontSize = 12,
                    Text = item.Key,
                    Foreground = Red
                };
                SetTop(id, (-item.Value.Koordinaten[1] + maxY) * auflösung + RandOben);
                SetLeft(id, item.Value.Koordinaten[0] * auflösung + RandLinks);
                visualErgebnisse.Children.Add(id);
                KnotenIDs.Add(id);
            }
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
            SetLeft(knotenPath, RandLinks);
            SetTop(knotenPath, RandOben);
            visualErgebnisse.Children.Add(knotenPath);
            return knotenPath;
        }
        public void ElementTexte()
        {
            foreach (var item in modell.Elemente)
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
                SetTop(id, (-cg.Y + maxY) * auflösung + RandOben);
                SetLeft(id, cg.X * auflösung + RandLinks);
                visualErgebnisse.Children.Add(id);
                ElementIDs.Add(id);
            }
        }

        public void AlleElementeZeichnen()
        {
            foreach (var item in modell.Elemente)
            {
                ElementZeichnen(item.Value, Black, 2);
            }
        }
        private void ElementZeichnen(AbstraktElement element, Brush farbe, double wichte)
        {
            var pathGeometry = ElementUmrisse(element);
            Shape elementPath = new Path()
            {
                Name = element.ElementId,
                Stroke = farbe,
                StrokeThickness = wichte,
                Data = pathGeometry
            };
            SetLeft(elementPath, RandLinks);
            SetTop(elementPath, RandOben);
            visualErgebnisse.Children.Add(elementPath);
        }
        public Shape ElementFillZeichnen(AbstraktElement element, Brush umrissFarbe, Color füllFarbe, double transparenz, double wichte)
        {
            var pathGeometry = ElementUmrisse(element);
            var füllung = new SolidColorBrush(füllFarbe) { Opacity = .2 };

            Shape elementPath = new Path()
            {
                Name = element.ElementId,
                Stroke = umrissFarbe,
                StrokeThickness = wichte,
                Fill = füllung,
                Data = pathGeometry
            };
            SetLeft(elementPath, RandLinks);
            SetTop(elementPath, RandOben);
            visualErgebnisse.Children.Add(elementPath);
            return elementPath;
        }
        private PathGeometry ElementUmrisse(AbstraktElement element)
        {
            var pathFigure = new PathFigure();
            var pathGeometry = new PathGeometry();

            if (modell.Knoten.TryGetValue(element.KnotenIds[0], out knoten)) { }
            var startPoint = TransformKnoten(knoten, auflösung, maxY);
            pathFigure.StartPoint = startPoint;
            for (var i = 1; i < element.KnotenProElement; i++)
            {
                if (modell.Knoten.TryGetValue(element.KnotenIds[i], out knoten)) { }
                var nextPoint = TransformKnoten(knoten, auflösung, maxY);
                pathFigure.Segments.Add(new LineSegment(nextPoint, true));
            }
            pathFigure.IsClosed = true;
            pathGeometry.Figures.Add(pathFigure);
            return pathGeometry;
        }

        public void KnotenlastenZeichnen()
        {
            foreach (var item in modell.Lasten)
            {
                var knotenId = item.Value.KnotenId;
                var lastWert = item.Value.Lastwerte[0];
                if (modell.Knoten.TryGetValue(knotenId, out knoten)) { }
                var lastPunkt = TransformKnoten(knoten, auflösung, maxY);
                var knotenLast = new TextBlock
                {
                    FontSize = 12,
                    Text = lastWert.ToString(CurrentCulture),
                    Foreground = Red
                };
                SetTop(knotenLast, lastPunkt.Y + RandOben + 10);
                SetLeft(knotenLast, lastPunkt.X + RandLinks);

                LastKnoten.Add(knotenLast);
                visualErgebnisse.Children.Add(knotenLast);
            }
        }
        public void ElementlastenZeichnen()
        {
            foreach (var item in modell.ElementLasten)
            {
                if (modell.Elemente.TryGetValue(item.Value.ElementId, out var element)) { }
                var elementLast = ElementFillZeichnen((Abstrakt2D)element, Black, Colors.Red, .2, 1);
                LastElemente.Add(elementLast);
            }
        }
        public void RandbedingungenZeichnen()
        {
            // zeichne den Wert einer jeden Randbedingung als Text an Randknoten
            foreach (var item in modell.Randbedingungen)
            {
                var knotenId = item.Value.KnotenId;
                if (modell.Knoten.TryGetValue(knotenId, out knoten)) { }
                var fensterKnoten = TransformKnoten(knoten, auflösung, maxY);

                var randWert = item.Value.Knoten.Reaktionen[0];
                var randbedingung = new TextBlock
                {
                    Name = "Support",
                    Uid = item.Value.RandbedingungId,
                    FontSize = 12,
                    Text = randWert.ToString("N2"),
                    //Foreground = Brushes.DarkOliveGreen
                    Background = LightBlue
                };
                RandKnoten.Add(randbedingung);
                SetTop(randbedingung, fensterKnoten.Y + RandOben + 15);
                SetLeft(randbedingung, fensterKnoten.X + RandLinks);
                visualErgebnisse.Children.Add(randbedingung);
            }
        }
        public void KnotentemperaturZeichnen()
        {
            foreach (var item in modell.Knoten)
            {
                knoten = item.Value;
                var temperatur = knoten.Knotenfreiheitsgrade[0].ToString("N2");
                temp = knoten.Knotenfreiheitsgrade[0];
                if (temp > maxTemp) maxTemp = temp;
                if (temp < minTemp) minTemp = temp;
                var fensterKnoten = TransformKnoten(knoten, auflösung, maxY);

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
                visualErgebnisse.Children.Add(id);
            }
        }

        public void ElementTemperaturZeichnen()
        {
            foreach (var path in TemperaturElemente)
            {
                visualErgebnisse.Children.Remove(path);
            }
            TemperaturElemente.Clear();
            foreach (var item in modell.Knoten)
            {
                knoten = item.Value;
                temp = knoten.Knotenfreiheitsgrade[0];
                if (temp > maxTemp) maxTemp = temp;
                if (temp < minTemp) minTemp = temp;
            }

            foreach (var item in modell.Elemente)
            {
                aktElement = item.Value;
                var pathGeometry = ElementUmrisse((Abstrakt2D)aktElement);
                //var elementTemperature = aktElement.KnotenIds.Where(knotenId
                //    => modell.Knoten.TryGetValue(knotenId, out knoten)).Sum(knotenId => knoten.Knotenfreiheitsgrade[0]);
                double elementTemperatur = 0;
                for (var i = 0; i < aktElement.KnotenProElement; i++)
                {
                    if (modell.Knoten.TryGetValue(aktElement.KnotenIds[i], out knoten))
                    {
                        elementTemperatur += knoten.Knotenfreiheitsgrade[0];
                    }
                }
                elementTemperatur /= aktElement.KnotenProElement;

                var intens = (byte)(255 * (elementTemperatur - minTemp) / (maxTemp - minTemp));
                var rot = FromArgb(intens, 255, 0, 0);
                var myBrush = new SolidColorBrush(rot);


                Shape path = new Path()
                {
                    Stroke = Blue,
                    StrokeThickness = 1,
                    Opacity = 0.5,
                    Fill = myBrush,
                    Data = pathGeometry
                };
                TemperaturElemente.Add(path);
                // setz oben/links Position zum Zeichnen auf dem Canvas
                SetLeft(path, RandLinks);
                SetTop(path, RandOben);
                // zeichne Shape
                visualErgebnisse.Children.Add(path);
            }
        }
        public void KnotengradientenZeichnen(int index)
        {
            foreach (var item in modell.Knoten)
            {
                knoten = item.Value;
                var gradient = knoten.KnotenAbleitungen[0][index].ToString("N2");
                temp = knoten.Knotenfreiheitsgrade[0];
                if (temp > maxTemp) maxTemp = temp;
                if (temp < minTemp) minTemp = temp;
                var fensterKnoten = TransformKnoten(knoten, auflösung, maxY);

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
                visualErgebnisse.Children.Add(id);
            }
        }
        public void WärmeflussvektorenZeichnen()
        {
            foreach (var path in WärmeVektoren)
            {
                visualErgebnisse.Children.Remove(path);
            }
            WärmeVektoren.Clear();
            double maxVektor = 0;
            foreach (var abstract2D in modell.Elemente.Select(item => (Abstrakt2D)item.Value))
            {
                abstract2D.ElementZustand = abstract2D.BerechneElementZustand(0, 0);
                var vektor = Math.Sqrt(abstract2D.ElementZustand[0] * abstract2D.ElementZustand[0] +
                                             abstract2D.ElementZustand[1] * abstract2D.ElementZustand[1]);
                if (maxVektor < vektor) maxVektor = vektor;
            }
            var vektorskalierung = maxScreenLength / maxVektor;

            foreach (var abstrakt2D in modell.Elemente.Select(item => (Abstrakt2D)item.Value))
            {
                abstrakt2D.ElementZustand = abstrakt2D.BerechneElementZustand(0, 0);
                var vektorLänge = (Math.Sqrt(abstrakt2D.ElementZustand[0] * abstrakt2D.ElementZustand[0] +
                                         abstrakt2D.ElementZustand[1] * abstrakt2D.ElementZustand[1])) * vektorskalierung;
                var vektorWinkel = Math.Atan2(abstrakt2D.ElementZustand[1], abstrakt2D.ElementZustand[0]) * 180 / Math.PI;
                // zeichne den resultierenden Vektor mit seinem Mittelpunkt im Elementschwerpunkt
                // füge am Endpunkt Pfeilspitzen an und füge Wärmeflusspfeil als pathFigure zur pathGeometry hinzu
                var pathGeometry = WärmeflussElementmitte(abstrakt2D, vektorLänge);

                Shape path = new Path()
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
                    CenterX = (int)(cg.X * auflösung),
                    CenterY = (int)((-cg.Y + maxY) * auflösung)
                };
                path.RenderTransform = rotateTransform;
                // sammle alle Wärmeflusspfeile in der Liste Wärmevektoren, um deren Darstellung löschen zu können
                WärmeVektoren.Add(path);

                // setz oben/links Position zum Zeichnen auf dem Canvas
                SetLeft(path, RandLinks);
                SetTop(path, RandOben);
                // zeichne Shape
                visualErgebnisse.Children.Add(path);
            }
        }

        private PathGeometry WärmeflussElementmitte(AbstraktElement abstraktElement, double length)
        {
            var abstrakt2D = (Abstrakt2D)abstraktElement;
            var pathFigure = new PathFigure();
            var pathGeometry = new PathGeometry();
            var cg = abstrakt2D.BerechneSchwerpunkt();
            int[] fensterKnoten = { (int)(cg.X * auflösung), (int)((-cg.Y + maxY) * auflösung) };
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
                var point = new Point(dt * i * auflösungH, -ordinaten[i + start] * auflösungV);
                stützpunkte.Add(point);
            }
            zeitverlauf.Points = stützpunkte;

            // setz oben/links Position zum Zeichnen auf dem Canvas
            SetLeft(zeitverlauf, RandLinks);
            SetTop(zeitverlauf, mY * auflösungV + RandOben);
            // zeichne Shape
            visualErgebnisse.Children.Add(zeitverlauf);
        }
        public void Koordinatensystem(double tmin, double tmax, double max, double min)
        {
            const int rand = 20;
            const int maxOrdinateAnzeigen = 100;
            screenH = visualErgebnisse.ActualWidth;
            screenV = visualErgebnisse.ActualHeight;
            if (double.IsNaN(max)) { max = maxOrdinateAnzeigen; min = -max; }
            if ((max - min) < double.Epsilon) auflösungV = screenV - rand;
            else auflösungV = (screenV - rand) / (max - min);
            auflösungH = (screenH - rand) / (tmax - tmin);
            var xAchse = new Line
            {
                Stroke = Black,
                X1 = 0,
                Y1 = max * auflösungV + RandOben,
                X2 = (tmax - tmin) * auflösungH + RandLinks,
                Y2 = max * auflösungV + RandOben,
                StrokeThickness = 2
            };
            _ = visualErgebnisse.Children.Add(xAchse);
            var yAchse = new Line
            {
                Stroke = Black,
                X1 = RandLinks,
                Y1 = max * auflösungV - min * auflösungV + 2 * RandOben,
                X2 = RandLinks,
                Y2 = RandOben,
                StrokeThickness = 2
            };
            visualErgebnisse.Children.Add(yAchse);
        }

        private static Point TransformKnoten(Knoten knoten, double auflösung, double maxY)
        {
            return new Point(knoten.Koordinaten[0] * auflösung, (-knoten.Koordinaten[1] + maxY) * auflösung);
        }
    }
}