using FE_Berechnungen.Tragwerksberechnung.Modelldaten;
using System.Globalization;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Shapes;
using static System.Windows.Controls.Canvas;
using static System.Windows.Media.Brushes;

namespace FE_Berechnungen.Tragwerksberechnung.Ergebnisse;

public partial class EigenlösungVisualisieren
{
    private const int RandOben = 60;
    private const int RandLinks = 60;
    private readonly Darstellung _darstellung;
    private readonly double _maxY, _auflösung;
    private readonly FeModell _modell;
    private double _eigenformSkalierung;
    private int _index;
    private Knoten _knoten;
    private bool _verformungenAn;
    public double ScreenH, ScreenV;

    public EigenlösungVisualisieren(FeModell feModel)
    {
        Language = XmlLanguage.GetLanguage("de-DE");
        _modell = feModel;
        InitializeComponent();
        Verformungen = [];
        Eigenfrequenzen = [];
        Show();

        // Auswahl der Eigenlösung
        var anzahlEigenformen = _modell.Eigenzustand.AnzahlZustände;
        var eigenformNr = new int[anzahlEigenformen];
        for (var i = 0; i < anzahlEigenformen; i++) eigenformNr[i] = i + 1;
        _darstellung = new Darstellung(_modell, VisualErgebnisse);
        _darstellung.FestlegungAuflösung();
        _maxY = _darstellung.MaxY;
        _auflösung = _darstellung.Auflösung;
        _darstellung.UnverformteGeometrie();
        Eigenlösungauswahl.ItemsSource = eigenformNr;

        _eigenformSkalierung = double.Parse("10");
        TxtSkalierung.Text = _eigenformSkalierung.ToString(CultureInfo.CurrentCulture);
    }

    public List<object> Verformungen { get; set; }
    public List<object> Eigenfrequenzen { get; set; }

    // ComboBox
    private void DropDownEigenformauswahlClosed(object sender, EventArgs e)
    {
        _index = Eigenlösungauswahl.SelectedIndex;
    }

    // Button events
    private void BtnGeometrie_Click(object sender, RoutedEventArgs e)
    {
        _darstellung.UnverformteGeometrie();
    }

    private void BtnEigenform_Click(object sender, RoutedEventArgs e)
    {
        Toggle_Eigenform();
    }

    private void OnKeyDownHandler(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Return) return;
        _eigenformSkalierung = double.Parse(TxtSkalierung.Text);
        Toggle_Eigenform();
        Toggle_Eigenform();
    }

    private void Toggle_Eigenform()
    {
        if (!_verformungenAn)
        {
            var pathGeometry = Eigenform_Zeichnen(_modell.Eigenzustand.Eigenvektoren[_index]);

            Shape path = new Path
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
            _verformungenAn = true;

            var value = Math.Sqrt(_modell.Eigenzustand.Eigenwerte[_index]) / 2 / Math.PI;
            var eigenfrequenz = new TextBlock
            {
                FontSize = 14,
                Text = "Eigenfrequenz Nr. " + (_index + 1) + " = " + value.ToString("N2"),
                Foreground = Blue
            };
            SetTop(eigenfrequenz, -RandOben + SteuerLeiste.Height);
            SetLeft(eigenfrequenz, RandLinks);
            VisualErgebnisse.Children.Add(eigenfrequenz);
            Eigenfrequenzen.Add(eigenfrequenz);
        }
        else
        {
            foreach (var path in Verformungen.Cast<Shape>()) VisualErgebnisse.Children.Remove(path);
            foreach (var eigenfrequenz in Eigenfrequenzen.Cast<TextBlock>())
                VisualErgebnisse.Children.Remove(eigenfrequenz);

            _verformungenAn = false;
        }
    }

    private PathGeometry Eigenform_Zeichnen(double[] zustand)
    {
        var pathGeometry = new PathGeometry();

        foreach (var element in Beams())
        {
            var pathFigure = new PathFigure();
            Point start, end;
            double startWinkel, endWinkel;

            switch (element)
            {
                case Fachwerk _:
                    {
                        if (_modell.Knoten.TryGetValue(element.KnotenIds[0], out _knoten))
                        {
                        }

                        start = TransformKnoten(_knoten, zustand, _auflösung, _maxY);
                        pathFigure.StartPoint = start;

                        for (var i = 1; i < element.KnotenIds.Length; i++)
                        {
                            if (_modell.Knoten.TryGetValue(element.KnotenIds[i], out _knoten))
                            {
                            }

                            end = TransformKnoten(_knoten, zustand, _auflösung, _maxY);
                            pathFigure.Segments.Add(new LineSegment(end, true));
                        }

                        break;
                    }
                case Biegebalken _:
                    {
                        if (_modell.Knoten.TryGetValue(element.KnotenIds[0], out _knoten))
                        {
                        }

                        start = TransformKnoten(_knoten, zustand, _auflösung, _maxY);
                        pathFigure.StartPoint = start;
                        if (_knoten != null)
                        {
                            startWinkel = -zustand[_knoten.SystemIndizes[2]] * 180 / Math.PI;

                            for (var i = 1; i < element.KnotenIds.Length; i++)
                            {
                                if (_modell.Knoten.TryGetValue(element.KnotenIds[i], out _knoten))
                                {
                                }

                                end = TransformKnoten(_knoten, zustand, _auflösung, _maxY);
                                var richtung = end - start;
                                richtung.Normalize();

                                richtung = RotateVectorScreen(richtung, startWinkel);
                                var control1 = start + richtung * element.BalkenLänge / 4 * _auflösung;
                                richtung = start - end;
                                richtung.Normalize();

                                if (_knoten == null) continue;
                                endWinkel = -zustand[_knoten.SystemIndizes[2]] * 180 / Math.PI;
                                richtung = RotateVectorScreen(richtung, endWinkel);
                                var control2 = end + richtung * element.BalkenLänge / 4 * _auflösung;
                                pathFigure.Segments.Add(new BezierSegment(control1, control2, end, true));
                            }
                        }

                        break;
                    }
                case BiegebalkenGelenk _:
                    {
                        if (_modell.Knoten.TryGetValue(element.KnotenIds[0], out _knoten))
                        {
                        }

                        start = TransformKnoten(_knoten, zustand, _auflösung, _maxY);
                        pathFigure.StartPoint = start;
                        if (_knoten != null)
                        {
                            startWinkel = -zustand[_knoten.SystemIndizes[2]] * 180 / Math.PI;

                            var control = start;
                            for (var i = 1; i < element.KnotenIds.Length; i++)
                            {
                                if (_modell.Knoten.TryGetValue(element.KnotenIds[i], out _knoten))
                                {
                                }

                                if (_knoten == null) continue;
                                end = TransformKnoten(_knoten, zustand, _auflösung, _maxY);
                                endWinkel = -zustand[_knoten.SystemIndizes[2]] * 180 / Math.PI;

                                Vector richtung;
                                switch (element.Typ)
                                {
                                    case 1:
                                        richtung = start - end;
                                        richtung.Normalize();
                                        richtung = RotateVectorScreen(richtung, endWinkel);
                                        control = end + richtung * element.BalkenLänge / 4 * _auflösung;
                                        break;
                                    case 2:
                                        richtung = end - start;
                                        richtung.Normalize();
                                        richtung = RotateVectorScreen(richtung, startWinkel);
                                        control = start + richtung * element.BalkenLänge / 4 * _auflösung;
                                        break;
                                }

                                pathFigure.Segments.Add(new QuadraticBezierSegment(control, end, true));
                            }
                        }

                        break;
                    }
            }

            if (element.KnotenIds.Length > 2) pathFigure.IsClosed = true;
            pathGeometry.Figures.Add(pathFigure);
        }

        return pathGeometry;

        IEnumerable<AbstraktBalken> Beams()
        {
            foreach (var item in _modell.Elemente)
                if (item.Value is AbstraktBalken element)
                    yield return element;
        }
    }

    private Point TransformKnoten(Knoten modellKnoten, double[] zustand, double resolution, double max)
    {
        var fensterKnoten = new int[2];
        fensterKnoten[0] = (int)(modellKnoten.Koordinaten[0] * resolution +
                                 zustand[modellKnoten.SystemIndizes[0]] * _eigenformSkalierung);
        fensterKnoten[1] = (int)((-modellKnoten.Koordinaten[1] + max) * resolution -
                                 zustand[modellKnoten.SystemIndizes[1]] * _eigenformSkalierung);
        var punkt = new Point(fensterKnoten[0], fensterKnoten[1]);
        return punkt;
    }

    private static Vector RotateVectorScreen(Vector vec, double winkel) // clockwise in degree
    {
        var vector = vec;
        var angle = winkel * Math.PI / 180;
        var rotated = new Vector(vector.X * Math.Cos(angle) - vector.Y * Math.Sin(angle),
            vector.X * Math.Sin(angle) + vector.Y * Math.Cos(angle));
        return rotated;
    }
}