using System.Globalization;
using System.Windows.Controls;
using System.Windows.Shapes;
using static System.Windows.Controls.Canvas;
using static System.Windows.Media.Brushes;

namespace FE_Berechnungen.Tragwerksberechnung.Ergebnisse;

public partial class DynamischeModellzuständeVisualisieren
{
    private readonly Darstellung _darstellung;
    private readonly double _dt;
    private readonly FeModell _modell;
    private readonly int _nSteps;
    private int _dropDownIndex, _index, _indexN, _indexQ, _indexM;

    private bool _elementTexteAn = true,
        _knotenTexteAn = true,
        _verformungenAn,
        _normalkräfteAn,
        _querkräfteAn,
        _momenteAn;

    private TextBlock _maximalWerte;
    private double _maxNormalkraft, _maxQuerkraft, _maxMoment;

    public DynamischeModellzuständeVisualisieren(FeModell feModel)
    {
        Language = XmlLanguage.GetLanguage("de-DE");
        _modell = feModel;

        InitializeComponent();
        Show();

        // Auswahl des Zeitschritts
        _dt = _modell.Zeitintegration.Dt;
        var tmax = _modell.Zeitintegration.Tmax;

        // Auswahl des Zeitschritts aus Zeitraster, z.B. jeder 10.
        _nSteps = (int)(tmax / _dt);
        const int zeitraster = 1;
        //if (nSteps > 1000) zeitraster = 10;
        _nSteps = _nSteps / zeitraster + 1;
        var zeit = new double[_nSteps];
        for (var i = 0; i < _nSteps; i++) zeit[i] = i * _dt * zeitraster;

        _darstellung = new Darstellung(_modell, VisualErgebnisse);
        _darstellung.UnverformteGeometrie();

        // mit Knoten und Element Ids
        _darstellung.KnotenTexte();
        _darstellung.ElementTexte();

        MaximalwerteGesamterZeitverlauf();
        Zeitschrittauswahl.ItemsSource = zeit;
    }

    private void DropDownZeitschrittauswahlClosed(object sender, EventArgs e)
    {
        if (Zeitschrittauswahl.SelectedIndex < 0)
        {
            _ = MessageBox.Show("kein gültiger Zeitschritt ausgewählt", "Zeitschrittauswahl");
            return;
        }

        _dropDownIndex = Zeitschrittauswahl.SelectedIndex;
        _index = _dropDownIndex;
        foreach (var item in _modell.Knoten)
            for (var i = 0; i < item.Value.AnzahlKnotenfreiheitsgrade; i++)
                item.Value.Knotenfreiheitsgrade[i] = item.Value.KnotenVariable[i][_index];

        _verformungenAn = false;
        _normalkräfteAn = false;
        _querkräfteAn = false;
        _momenteAn = false;
    }

    private void BtnVerformung_Click(object sender, RoutedEventArgs e)
    {
        if (_index == 0)
        {
            _ = MessageBox.Show("Zeitschritt muss erst ausgewählt werden", "Tragwerksberechnung");
            return;
        }

        if (_verformungenAn)
        {
            foreach (var path in _darstellung.Verformungen.Cast<Shape>()) VisualErgebnisse.Children.Remove(path);

            _index++;
            AktuellerZeitschritt.Text =
                "aktuelle Integrationszeit = " + (_index * _dt).ToString(CultureInfo.InvariantCulture);
            if (_index >= _nSteps)
            {
                _ = MessageBox.Show("Ende der Zeitschrittberechnung", "Tragwerksberechnung");
                _index = _dropDownIndex;
                _verformungenAn = false;
                return;
            }
        }
        else
        {
            _index = _dropDownIndex;
            Clean();
        }

        foreach (var item in _modell.Knoten)
            for (var i = 0; i < item.Value.AnzahlKnotenfreiheitsgrade; i++)
                item.Value.Knotenfreiheitsgrade[i] = item.Value.KnotenVariable[i][_index];
        _darstellung.VerformteGeometrie();
        _verformungenAn = true;
        _normalkräfteAn = false;
        _querkräfteAn = false;
        _momenteAn = false;
    }

    private void BtnNormalkraft_Click(object sender, RoutedEventArgs e)
    {
        if (_index == 0)
        {
            _ = MessageBox.Show("Zeitschritt muss erst ausgewählt werden", "Tragwerksberechnung");
            return;
        }

        if (_normalkräfteAn)
        {
            foreach (var path in _darstellung.NormalkraftListe.Cast<Shape>()) VisualErgebnisse.Children.Remove(path);

            _index++;
            AktuellerZeitschritt.Text =
                "aktuelle Integrationszeit = " + (_index * _dt).ToString(CultureInfo.InvariantCulture);
            if (_index >= _nSteps)
            {
                _ = MessageBox.Show("Ende der Zeitschrittberechnung", "Tragwerksberechnung");
                _index = _dropDownIndex;
                _normalkräfteAn = false;
                return;
            }
        }
        else
        {
            _index = _dropDownIndex;
            Clean();
        }

        foreach (var item in _modell.Knoten)
            for (var i = 0; i < item.Value.AnzahlKnotenfreiheitsgrade; i++)
                item.Value.Knotenfreiheitsgrade[i] = item.Value.KnotenVariable[i][_index];
        // Skalierung der Normalkraftdarstellung und Darstellung aller Normalkraftverteilungen
        foreach (var beam in _modell.Elemente.Select(item => item.Value).OfType<AbstraktBalken>())
        {
            _ = beam.BerechneStabendkräfte();
            _darstellung.Normalkraft_Zeichnen(beam, _maxNormalkraft, false);
        }

        _verformungenAn = false;
        _normalkräfteAn = true;
        _querkräfteAn = false;
        _momenteAn = false;
    }

    private void Clean()
    {
        //index = dropDownIndex;
        foreach (var path in _darstellung.Verformungen.Cast<Shape>()) VisualErgebnisse.Children.Remove(path);

        foreach (var path in _darstellung.NormalkraftListe.Cast<Shape>())
            VisualErgebnisse.Children.Remove(path);
        foreach (var path in _darstellung.QuerkraftListe.Cast<Shape>())
            VisualErgebnisse.Children.Remove(path);
        foreach (var path in _darstellung.MomenteListe.Cast<Shape>())
            VisualErgebnisse.Children.Remove(path);
    }

    private void BtnQuerkraft_Click(object sender, RoutedEventArgs e)
    {
        if (_index == 0)
        {
            _ = MessageBox.Show("Zeitschritt muss erst ausgewählt werden", "Tragwerksberechnung");
            return;
        }

        if (_querkräfteAn)
        {
            foreach (var path in _darstellung.QuerkraftListe.Cast<Shape>())
                VisualErgebnisse.Children.Remove(path);
            _index++;
            AktuellerZeitschritt.Text =
                "aktuelle Integrationszeit = " + (_index * _dt).ToString(CultureInfo.InvariantCulture);
            if (_index >= _nSteps)
            {
                _ = MessageBox.Show("Ende der Zeitschrittberechnung", "Tragwerksberechnung");
                _index = _dropDownIndex;
                _querkräfteAn = false;
                return;
            }
        }
        else
        {
            _index = _dropDownIndex;
            Clean();
        }

        foreach (var item in _modell.Knoten)
            for (var i = 0; i < item.Value.AnzahlKnotenfreiheitsgrade; i++)
                item.Value.Knotenfreiheitsgrade[i] = item.Value.KnotenVariable[i][_index];
        // Skalierung der Querkraftdarstellung und Darstellung aller Querkraftverteilungen
        foreach (var beam in _modell.Elemente.Select(item => item.Value).OfType<AbstraktBalken>())
        {
            _ = beam.BerechneStabendkräfte();
            _darstellung.Querkraft_Zeichnen(beam, _maxQuerkraft, false);
        }

        _verformungenAn = false;
        _normalkräfteAn = false;
        _querkräfteAn = true;
        _momenteAn = false;
    }

    private void BtnBiegemoment_Click(object sender, RoutedEventArgs e)
    {
        if (_index == 0)
        {
            _ = MessageBox.Show("Zeitschritt muss erst ausgewählt werden", "Tragwerksberechnung");
            return;
        }

        if (_momenteAn)
        {
            foreach (var path in _darstellung.MomenteListe.Cast<Shape>())
                VisualErgebnisse.Children.Remove(path);
            _index++;
            AktuellerZeitschritt.Text =
                "aktuelle Integrationszeit = " + (_index * _dt).ToString(CultureInfo.InvariantCulture);
            if (_index >= _nSteps)
            {
                _ = MessageBox.Show("Ende der Zeitschrittberechnung", "Tragwerksberechnung");
                _index = _dropDownIndex;
                _momenteAn = false;
                return;
            }
        }
        else
        {
            _index = _dropDownIndex;
            Clean();
        }

        foreach (var item in _modell.Knoten)
            for (var i = 0; i < item.Value.AnzahlKnotenfreiheitsgrade; i++)
                item.Value.Knotenfreiheitsgrade[i] = item.Value.KnotenVariable[i][_index];
        // Skalierung der Momentendarstellung und Darstellung aller Momentverteilungen
        foreach (var beam in _modell.Elemente.Select(item => item.Value).OfType<AbstraktBalken>())
        {
            _ = beam.BerechneStabendkräfte();
            _darstellung.Momente_Zeichnen(beam, _maxMoment, false);
        }

        _verformungenAn = false;
        _normalkräfteAn = false;
        _querkräfteAn = false;
        _momenteAn = true;
    }

    private void MaximalwerteGesamterZeitverlauf()
    {
        double maxUx = 0, minUx = 0, maxUy = 0, minUy = 0;
        string knotenUxMax = "", knotenUxMin = "", knotenUyMax = "", knotenUyMin = "";
        double maxUxZeit = 0, minUxZeit = 0, maxUyZeit = 0, minUyZeit = 0;
        var sb = new StringBuilder();
        foreach (var item in _modell.Knoten)
        {
            var temp = item.Value.KnotenVariable[0].Max();
            if (maxUx < temp)
            {
                maxUx = temp;
                knotenUxMax = item.Value.Id;
                maxUxZeit = _dt * Array.IndexOf(item.Value.KnotenVariable[0], maxUx);
            }

            temp = item.Value.KnotenVariable[0].Min();
            if (minUx > temp)
            {
                minUx = temp;
                knotenUxMin = item.Value.Id;
                minUxZeit = _dt * Array.IndexOf(item.Value.KnotenVariable[0], minUx);
            }

            temp = item.Value.KnotenVariable[1].Max();
            if (maxUy < temp)
            {
                maxUy = temp;
                knotenUyMax = item.Value.Id;
                maxUyZeit = _dt * Array.IndexOf(item.Value.KnotenVariable[1], maxUy);
            }

            temp = item.Value.KnotenVariable[1].Min();
            if (!(minUy > temp)) continue;
            minUy = temp;
            knotenUyMin = item.Value.Id;
            minUyZeit = _dt * Array.IndexOf(item.Value.KnotenVariable[1], minUy);
        }

        if (knotenUxMax.Length == 0)
        {
            sb.Append("ux = " + maxUx.ToString("G4"));
            sb.Append(", max. uy = " + maxUy.ToString("G4") + ", an Knoten "
                      + knotenUyMax + " zur Zeit " + maxUyZeit.ToString("G4")
                      + ", min. uy = " + minUy.ToString("G4") + ", an Knoten "
                      + knotenUyMin + " zur Zeit " + minUyZeit.ToString("G4"));
        }
        else if (knotenUyMax.Length == 0)
        {
            sb.Append("max. ux = " + maxUx.ToString("G4") + ", an Knoten "
                      + knotenUxMax + " zur Zeit " + maxUxZeit.ToString("G4")
                      + ", min. ux = " + minUx.ToString("G4") + ", an Knoten "
                      + knotenUxMin + " zur Zeit " + minUxZeit.ToString("G4"));
            sb.Append(", uy = " + maxUy.ToString("G4"));
        }
        else
        {
            sb.Append("max. ux = " + maxUx.ToString("G4") + ", an Knoten "
                      + knotenUxMax + " zur Zeit " + maxUxZeit.ToString("G4")
                      + ", min. ux = " + minUx.ToString("G4") + ", an Knoten "
                      + knotenUxMin + " zur Zeit " + minUxZeit.ToString("G4"));
            sb.Append(", max. uy = " + maxUy.ToString("G4") + ", an Knoten "
                      + knotenUyMax + " zur Zeit " + maxUyZeit.ToString("G4")
                      + ", min. uy = " + minUy.ToString("G4") + ", an Knoten "
                      + knotenUyMin + " zur Zeit " + minUyZeit.ToString("G4"));
        }

        var maximalVerformungen = new TextBlock
        {
            FontSize = 14,
            FontWeight = FontWeights.Bold,
            Text = sb.ToString(),
            Foreground = Red
        };
        SetTop(maximalVerformungen, 0);
        SetLeft(maximalVerformungen, 5);
        VisualErgebnisse.Children.Add(maximalVerformungen);

        // Schleife über alle Zeitschritte
        for (var i = 0; i < _nSteps; i++)
        {
            foreach (var item in _modell.Knoten)
                for (var k = 0; k < item.Value.AnzahlKnotenfreiheitsgrade; k++)
                    item.Value.Knotenfreiheitsgrade[k] = item.Value.KnotenVariable[k][i];

            // Zustand aller Fachwerk- und Biegebalkenelemente an einem Zeitschritt
            foreach (var element in Beams())
            {
                element.ElementZustand = element.BerechneStabendkräfte();

                // Fachwerkstäbe
                if (element.ElementZustand.Length == 2)
                {
                    if (Math.Abs(element.ElementZustand[0]) > _maxNormalkraft)
                    {
                        _indexN = i;
                        _maxNormalkraft = Math.Abs(element.ElementZustand[0]);
                    }

                    if (!(Math.Abs(element.ElementZustand[1]) > _maxNormalkraft)) continue;
                    _indexN = i;
                    _maxNormalkraft = Math.Abs(element.ElementZustand[1]);
                }

                // Biegebalken
                else
                {
                    if (Math.Abs(element.ElementZustand[0]) > _maxNormalkraft)
                    {
                        _indexN = i;
                        _maxNormalkraft = Math.Abs(element.ElementZustand[0]);
                    }

                    if (Math.Abs(element.ElementZustand[3]) > _maxNormalkraft)
                    {
                        _indexN = i;
                        _maxNormalkraft = Math.Abs(element.ElementZustand[3]);
                    }

                    if (Math.Abs(element.ElementZustand[1]) > _maxQuerkraft)
                    {
                        _indexQ = i;
                        _maxQuerkraft = Math.Abs(element.ElementZustand[1]);
                    }

                    if (Math.Abs(element.ElementZustand[4]) > _maxQuerkraft)
                    {
                        _indexQ = i;
                        _maxQuerkraft = Math.Abs(element.ElementZustand[4]);
                    }

                    if (Math.Abs(element.ElementZustand[2]) > _maxMoment)
                    {
                        _indexM = i;
                        _maxMoment = Math.Abs(element.ElementZustand[2]);
                    }

                    if (Math.Abs(element.ElementZustand[5]) > _maxMoment)
                    {
                        _indexM = i;
                        _maxMoment = Math.Abs(element.ElementZustand[5]);
                    }
                }
            }

            continue;

            IEnumerable<AbstraktBalken> Beams()
            {
                foreach (var item in _modell.Elemente)
                    if (item.Value is AbstraktBalken element)
                        yield return element;
            }
        }

        _maximalWerte = new TextBlock
        {
            FontSize = 14,
            FontWeight = FontWeights.Bold,
            Text = "max Normalkraft = " + _maxNormalkraft.ToString("G4") + " nach Zeit = " +
                   (_indexN * _dt).ToString("N2") +
                   ", max Querkraft = " + _maxQuerkraft.ToString("G4") + " nach Zeit = " +
                   (_indexQ * _dt).ToString("N2") +
                   " und max Moment = " + _maxMoment.ToString("G4") + " nach Zeit = " + (_indexM * _dt).ToString("N2"),
            Foreground = Red
        };
        SetTop(_maximalWerte, 20);
        SetLeft(_maximalWerte, 5);
        VisualErgebnisse.Children.Add(_maximalWerte);
    }

    private void BtnElementIDs_Click(object sender, RoutedEventArgs e)
    {
        if (!_elementTexteAn)
        {
            _darstellung.ElementTexte();
            _elementTexteAn = true;
        }
        else
        {
            foreach (var id in _darstellung.ElementIDs.Cast<TextBlock>()) VisualErgebnisse.Children.Remove(id);
            _elementTexteAn = false;
        }
    }

    private void BtnKnotenIDs_Click(object sender, RoutedEventArgs e)
    {
        if (!_knotenTexteAn)
        {
            _darstellung.KnotenTexte();
            _knotenTexteAn = true;
        }
        else
        {
            foreach (var id in _darstellung.KnotenIDs.Cast<TextBlock>()) VisualErgebnisse.Children.Remove(id);
            _knotenTexteAn = false;
        }
    }

    private void BtnVerschiebung_Click(object sender, RoutedEventArgs e)
    {
        _darstellung.ÜberhöhungVerformung = int.Parse(Verschiebung.Text);
        foreach (var path in _darstellung.Verformungen.Cast<Shape>()) VisualErgebnisse.Children.Remove(path);
        _verformungenAn = false;
        _darstellung.VerformteGeometrie();
        _verformungenAn = true;
    }
}