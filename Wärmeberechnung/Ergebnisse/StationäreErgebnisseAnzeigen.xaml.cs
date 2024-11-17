using FE_Berechnungen.Wärmeberechnung.Modelldaten;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Shapes;

namespace FE_Berechnungen.Wärmeberechnung.Ergebnisse;

public partial class StationäreErgebnisseAnzeigen
{
    private readonly FeModell _modell;
    private Shape _letzterKnoten;
    private Shape _letztesElement;

    public StationäreErgebnisseAnzeigen(FeModell feModell)
    {
        Language = XmlLanguage.GetLanguage("de-DE");
        _modell = feModell;
        InitializeComponent();
    }

    private void Knoten_Loaded(object sender, RoutedEventArgs e)
    {
        KnotenGrid = sender as DataGrid;
        if (KnotenGrid != null) KnotenGrid.ItemsSource = _modell.Knoten;
    }

    //SelectionChanged
    private void KnotenZeileSelected(object sender, SelectionChangedEventArgs e)
    {
        if (KnotenGrid.SelectedCells.Count <= 0) return;
        var cellInfo = KnotenGrid.SelectedCells[0];
        var cell = (KeyValuePair<string, Knoten>)cellInfo.Item;
        var knoten = cell.Value;
        if (_letzterKnoten != null)
            StartFenster.StationäreErgebnisse.VisualWärmeErgebnisse.Children.Remove(_letzterKnoten);
        _letzterKnoten = StartFenster.StationäreErgebnisse.Darstellung.KnotenZeigen(knoten, Brushes.Green, 1);
    }

    //LostFocus
    private void KeinKnotenSelected(object sender, RoutedEventArgs e)
    {
        StartFenster.StationäreErgebnisse.VisualWärmeErgebnisse.Children.Remove(_letzterKnoten);
        _letztesElement = null;
    }

    private void WärmeflussVektoren_Loaded(object sender, RoutedEventArgs e)
    {
        WärmeflussVektorGrid = sender as DataGrid;
        foreach (var item in _modell.Elemente)
            switch (item.Value)
            {
                case Abstrakt2D value:
                    {
                        var element = value;
                        element.ElementZustand = element.BerechneElementZustand(0, 0);
                        break;
                    }
                case Element3D8 value:
                    {
                        var element3d8 = value;
                        element3d8.BerechneElementZustand(0, 0, 0);
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
        if (_letztesElement != null)
            StartFenster.StationäreErgebnisse.VisualWärmeErgebnisse.Children.Remove(_letztesElement);
        _letztesElement = StartFenster.StationäreErgebnisse.Darstellung.ElementFillZeichnen((Abstrakt2D)element,
            Brushes.Black, Colors.Green, .2, 2);
    }

    //LostFocus
    private void KeinElementSelected(object sender, RoutedEventArgs e)
    {
        StartFenster.StationäreErgebnisse.VisualWärmeErgebnisse.Children.Remove(_letztesElement);
        _letzterKnoten = null;
    }

    private void Wärmefluss_Loaded(object sender, RoutedEventArgs e)
    {
        WärmeflussGrid = sender as DataGrid;
        if (WärmeflussGrid != null) WärmeflussGrid.ItemsSource = _modell.Randbedingungen;
    }
}