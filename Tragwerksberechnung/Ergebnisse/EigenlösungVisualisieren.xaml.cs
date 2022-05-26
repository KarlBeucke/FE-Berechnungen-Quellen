using FE_Berechnungen.Tragwerksberechnung.Modelldaten;
using FEBibliothek.Modell;
using FEBibliothek.Modell.abstrakte_Klassen;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Shapes;
using static System.Windows.Controls.Canvas;
using static System.Windows.Media.Brushes;

namespace FE_Berechnungen.Tragwerksberechnung.Ergebnisse
{
    public partial class EigenlösungVisualisieren
    {
        private readonly FEModell modell;
        private Knoten knoten;
        private int index;
        public Darstellung darstellung;
        public double screenH, screenV;
        private readonly double auflösung;
        private readonly double maxY;
        private bool verformungenAn;
        private const int RandOben = 60;
        private const int RandLinks = 60;
        public List<object> Verformungen { get; set; }
        public List<object> Eigenfrequenzen { get; set; }
        private double eigenformSkalierung;

        public EigenlösungVisualisieren(FEModell feModel)
        {
            Language = XmlLanguage.GetLanguage("de-DE");
            modell = feModel;
            InitializeComponent();
            Verformungen = new List<object>();
            Eigenfrequenzen = new List<object>();
            Show();

            // Auswahl der Eigenlösung
            var anzahlEigenformen = modell.Eigenzustand.AnzahlZustände;
            var eigenformNr = new int[anzahlEigenformen];
            for (var i = 0; i < anzahlEigenformen; i++) { eigenformNr[i] = i + 1; }
            darstellung = new Darstellung(modell, VisualErgebnisse);
            darstellung.FestlegungAuflösung();
            maxY = darstellung.maxY;
            auflösung = darstellung.auflösung;
            darstellung.UnverformteGeometrie();
            Eigenlösungauswahl.ItemsSource = eigenformNr;

            eigenformSkalierung = double.Parse("10");
            TxtSkalierung.Text = eigenformSkalierung.ToString(CultureInfo.CurrentCulture);
        }

        // ComboBox
        private void DropDownEigenformauswahlClosed(object sender, EventArgs e)
        {
            index = Eigenlösungauswahl.SelectedIndex;
        }

        // Button events
        private void BtnGeometrie_Click(object sender, RoutedEventArgs e)
        {
            darstellung.UnverformteGeometrie();
        }
        private void BtnEigenform_Click(object sender, RoutedEventArgs e)
        {
            Toggle_Eigenform();
        }
        private void OnKeyDownHandler(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Return) return;
            eigenformSkalierung = double.Parse(TxtSkalierung.Text);
            Toggle_Eigenform();
            Toggle_Eigenform();
        }

        public void Toggle_Eigenform()
        {
            if (!verformungenAn)
            {
                var pathGeometry = Eigenform_Zeichnen(modell.Eigenzustand.Eigenvektoren[index]);

                Shape path = new Path()
                {
                    Stroke = Red,
                    StrokeThickness = 2,
                    Data = pathGeometry
                };
                // setz oben/links Position zum Zeichnen auf dem Canvas
                SetLeft(path, RandLinks);
                SetTop(path, RandOben);
                // zeichne Shape
                VisualErgebnisse.Children.Add(path);
                Verformungen.Add(path);
                verformungenAn = true;

                var value = Math.Sqrt(modell.Eigenzustand.Eigenwerte[index]) / 2 / Math.PI;
                var eigenfrequenz = new TextBlock
                {
                    FontSize = 14,
                    Text = "Eigenfrequenz Nr. " + (index + 1).ToString() + " = " + value.ToString("N2"),
                    Foreground = Blue
                };
                SetTop(eigenfrequenz, -RandOben + SteuerLeiste.Height);
                SetLeft(eigenfrequenz, RandLinks);
                VisualErgebnisse.Children.Add(eigenfrequenz);
                Eigenfrequenzen.Add(eigenfrequenz);
            }
            else
            {
                foreach (Shape path in Verformungen)
                {
                    VisualErgebnisse.Children.Remove(path);
                }
                foreach (TextBlock eigenfrequenz in Eigenfrequenzen) VisualErgebnisse.Children.Remove(eigenfrequenz);
                verformungenAn = false;
            }
        }
        public PathGeometry Eigenform_Zeichnen(double[] zustand)
        {
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
                Point start, end;
                double startWinkel, endWinkel;

                switch (element)
                {
                    case Fachwerk _:
                        {
                            if (modell.Knoten.TryGetValue(element.KnotenIds[0], out knoten)) { }
                            start = TransformKnoten(knoten, zustand, auflösung, maxY);
                            pathFigure.StartPoint = start;

                            for (var i = 1; i < element.KnotenIds.Length; i++)
                            {
                                if (modell.Knoten.TryGetValue(element.KnotenIds[i], out knoten)) { }
                                end = TransformKnoten(knoten, zustand, auflösung, maxY);
                                pathFigure.Segments.Add(new LineSegment(end, true));
                            }

                            break;
                        }
                    case Biegebalken _:
                        {
                            if (modell.Knoten.TryGetValue(element.KnotenIds[0], out knoten)) { }
                            start = TransformKnoten(knoten, zustand, auflösung, maxY);
                            pathFigure.StartPoint = start;
                            startWinkel = -zustand[knoten.SystemIndizes[2]] * 180 / Math.PI;

                            for (var i = 1; i < element.KnotenIds.Length; i++)
                            {
                                if (modell.Knoten.TryGetValue(element.KnotenIds[i], out knoten)) { }
                                end = TransformKnoten(knoten, zustand, auflösung, maxY);
                                var richtung = end - start;
                                richtung.Normalize();

                                richtung = RotateVectorScreen(richtung, startWinkel);
                                var control1 = start + richtung * element.balkenLänge / 4 * auflösung;
                                richtung = start - end;
                                richtung.Normalize();

                                endWinkel = -zustand[knoten.SystemIndizes[2]] * 180 / Math.PI;
                                richtung = RotateVectorScreen(richtung, endWinkel);
                                var control2 = end + richtung * element.balkenLänge / 4 * auflösung;

                                pathFigure.Segments.Add(new BezierSegment(control1, control2, end, true));
                            }
                            break;
                        }
                    case BiegebalkenGelenk _:
                        {
                            if (modell.Knoten.TryGetValue(element.KnotenIds[0], out knoten)) { }
                            start = TransformKnoten(knoten, zustand, auflösung, maxY);
                            pathFigure.StartPoint = start;
                            startWinkel = -zustand[knoten.SystemIndizes[2]] * 180 / Math.PI;

                            var control = start;
                            for (var i = 1; i < element.KnotenIds.Length; i++)
                            {
                                if (modell.Knoten.TryGetValue(element.KnotenIds[i], out knoten)) { }
                                end = TransformKnoten(knoten, zustand, auflösung, maxY);
                                endWinkel = -zustand[knoten.SystemIndizes[2]] * 180 / Math.PI;

                                Vector richtung;
                                switch (element.Typ)
                                {
                                    case 1:
                                        richtung = start - end;
                                        richtung.Normalize();
                                        richtung = RotateVectorScreen(richtung, endWinkel);
                                        control = end + richtung * element.balkenLänge / 4 * auflösung;
                                        break;
                                    case 2:
                                        richtung = end - start;
                                        richtung.Normalize();
                                        richtung = RotateVectorScreen(richtung, startWinkel);
                                        control = start + richtung * element.balkenLänge / 4 * auflösung;
                                        break;
                                }
                                pathFigure.Segments.Add(new QuadraticBezierSegment(control, end, true));
                            }
                            break;
                        }
                }
                if (element.KnotenIds.Length > 2) pathFigure.IsClosed = true;
                pathGeometry.Figures.Add(pathFigure);
            }
            return pathGeometry;
        }

        public Point TransformKnoten(Knoten modellKnoten, double[] zustand, double resolution, double max)
        {
            var fensterKnoten = new int[2];
            fensterKnoten[0] = (int)(modellKnoten.Koordinaten[0] * resolution + zustand[modellKnoten.SystemIndizes[0]] * eigenformSkalierung);
            fensterKnoten[1] = (int)((-modellKnoten.Koordinaten[1] + max) * resolution - zustand[modellKnoten.SystemIndizes[1]] * eigenformSkalierung);
            var punkt = new Point(fensterKnoten[0], fensterKnoten[1]);
            return punkt;
        }
        public static Vector RotateVectorScreen(Vector vec, double winkel)  // clockwise in degree
        {
            var vector = vec;
            var angle = winkel * Math.PI / 180;
            var rotated = new Vector(vector.X * Math.Cos(angle) - vector.Y * Math.Sin(angle), vector.X * Math.Sin(angle) + vector.Y * Math.Cos(angle));
            return rotated;
        }
    }
}