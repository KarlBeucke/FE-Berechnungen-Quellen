using FE_Berechnungen.Wärmeberechnung.Modelldaten;
using FE_Berechnungen.Wärmeberechnung.ModelldatenAnzeigen;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace FE_Berechnungen.Wärmeberechnung.Ergebnisse;

public partial class InstationäreErgebnisseAnzeigen
{
    private readonly FeModell _modell;
    private readonly WärmemodellVisualisieren _wärmeModell;
    private readonly double[] _zeit;
    private Knoten _knoten;
    private Shape _letzterKnoten;
    private Shape _letztesElement;

    public InstationäreErgebnisseAnzeigen(FeModell modell)
    {
        Language = XmlLanguage.GetLanguage("de-DE");
        _modell = modell;
        DataContext = this;
        _wärmeModell = new WärmemodellVisualisieren(modell);
        _wärmeModell.Show();
        InitializeComponent();
        Show();

        Knotenauswahl.ItemsSource = this._modell.Knoten.Keys;

        Dt = this._modell.Zeitintegration.Dt;
        var tmax = this._modell.Zeitintegration.Tmax;
        NSteps = (int)(tmax / Dt) + 1;
        _zeit = new double[NSteps];
        for (var i = 0; i < NSteps; i++) _zeit[i] = i * Dt;
        Zeitschrittauswahl.ItemsSource = _zeit;
    }

    private double Dt { get; }
    private int NSteps { get; }
    private int Index { get; set; }

    //KnotentemperaturGrid
    private void DropDownKnotenauswahlClosed(object sender, EventArgs e)
    {
        if (Knotenauswahl.SelectedIndex < 0)
        {
            _ = MessageBox.Show("kein gültiger Knoten Identifikator ausgewählt", "Zeitschrittauswahl");
            return;
        }

        var knotenId = (string)Knotenauswahl.SelectedItem;
        if (_modell.Knoten.TryGetValue(knotenId, out _knoten))
        {
        }

        if (_knoten != null)
        {
            var maxTemperatur = _knoten.KnotenVariable[0].Max();
            var maxZeit = Dt * Array.IndexOf(_knoten.KnotenVariable[0], maxTemperatur);
            var maxGradient = _knoten.KnotenAbleitungen[0].Max();
            var maxZeitGradient = Dt * Array.IndexOf(_knoten.KnotenAbleitungen[0], maxGradient);
            var maxText = "max. Temperatur = " + maxTemperatur.ToString("N4") + ", an Zeit =" + maxZeit.ToString("N2")
                          + "\nmax. Gradient      = " + maxGradient.ToString("N4") + ", an Zeit =" +
                          maxZeitGradient.ToString("N2");
            MaxText.Text = maxText;
        }

        KnotentemperaturGrid_Anzeigen();
    }

    private void KnotentemperaturGrid_Anzeigen()
    {
        if (_knoten == null) return;
        var knotentemperaturen = new Dictionary<int, double[]>();
        for (var i = 0; i < NSteps; i++)
        {
            var zustand = new double[3];
            zustand[0] = _zeit[i];
            zustand[1] = _knoten.KnotenVariable[0][i];
            zustand[2] = _knoten.KnotenAbleitungen[0][i];
            knotentemperaturen.Add(i, zustand);
        }

        KnotentemperaturGrid.ItemsSource = knotentemperaturen;

        if (_letzterKnoten != null) _wärmeModell.VisualWärmeModell.Children.Remove(_letzterKnoten);
        _letzterKnoten = _wärmeModell.Darstellung.KnotenZeigen(_knoten, Brushes.Green, 1);
    }

    //KontenwerteGrid
    private void DropDownZeitschrittauswahlClosed(object sender, EventArgs e)
    {
        if (Zeitschrittauswahl.SelectedIndex < 0)
        {
            _ = MessageBox.Show("kein gültiger Zeitschritt ausgewählt", "Zeitschrittauswahl");
            return;
        }

        Index = Zeitschrittauswahl.SelectedIndex;
        Integrationsschritt.Text = "Modellzustand  an Zeitschritt  " + Index;

        foreach (var item in _modell.Knoten) item.Value.Knotenfreiheitsgrade[0] = item.Value.KnotenVariable[0][Index];

        KnotenwerteGrid_Anzeigen();
        WärmeflussVektorenGrid_Anzeigen();
    }

    private void KnotenwerteGrid_Anzeigen()
    {
        var zeitschritt = new Dictionary<string, double[]>();
        foreach (var item in _modell.Knoten)
        {
            var zustand = new double[2];
            zustand[0] = item.Value.KnotenVariable[0][Index];
            zustand[1] = item.Value.KnotenAbleitungen[0][Index];
            zeitschritt.Add(item.Key, zustand);
        }

        KnotenwerteGrid.ItemsSource = zeitschritt;
    }

    //SelectionChanged
    private void KnotenwerteZeileSelected(object sender, SelectionChangedEventArgs e)
    {
        if (KnotenwerteGrid.SelectedCells.Count <= 0) return;
        var cellInfo = KnotenwerteGrid.SelectedCells[0];
        var cell = (KeyValuePair<string, double[]>)cellInfo.Item;
        var knotenId = cell.Key;
        if (_modell.Knoten.TryGetValue(knotenId, out _knoten))
        {
        }

        if (_letzterKnoten != null) _wärmeModell.VisualWärmeModell.Children.Remove(_letzterKnoten);
        _letzterKnoten = _wärmeModell.Darstellung.KnotenZeigen(_knoten, Brushes.Green, 1);
    }

    //LostFocus
    private void KeineKnotenwerteZeileSelected(object sender, RoutedEventArgs e)
    {
        _wärmeModell.VisualWärmeModell.Children.Remove(_letzterKnoten);
    }

    //WärmeflussvektorenGrid
    private void WärmeflussVektorenGrid_Anzeigen()
    {
        //var zeitschritt = new Dictionary<string, double[]>();
        foreach (var item in _modell.Elemente)
            switch (item.Value)
            {
                case Abstrakt2D value:
                    {
                        value.ElementZustand = value.BerechneElementZustand(0, 0);
                        break;
                    }
                case Element3D8 value:
                    {
                        value.ElementZustand = value.BerechneElementZustand(0, 0, 0);
                        break;
                    }
            }

        if (WärmeflussVektorGrid != null) WärmeflussVektorGrid.ItemsSource = _modell.Elemente;
    }

    //SelectionChanged
    private void ElementZeileSelected(object sender, SelectionChangedEventArgs e)
    {
        if (WärmeflussVektorGrid.SelectedCells.Count <= 0) return;
        var cellInfo = WärmeflussVektorGrid.SelectedCells[0];
        var cell = (KeyValuePair<string, AbstraktElement>)cellInfo.Item;
        var element = cell.Value;
        if (_letztesElement != null) _wärmeModell.VisualWärmeModell.Children.Remove(_letztesElement);
        _letztesElement = _wärmeModell.Darstellung.ElementFillZeichnen((Abstrakt2D)element,
            Brushes.Black, Colors.Green, .2, 2);
    }

    //LostFocus
    private void KeinElementSelected(object sender, RoutedEventArgs e)
    {
        _wärmeModell.VisualWärmeModell.Children.Remove(_letztesElement);
        _letzterKnoten = null;
    }

    //Unloaded
    private void ModellSchliessen(object sender, RoutedEventArgs e)
    {
        _wärmeModell.Close();
    }
}