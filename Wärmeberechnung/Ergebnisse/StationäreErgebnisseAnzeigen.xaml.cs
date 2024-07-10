using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Shapes;
using FE_Berechnungen.Wärmeberechnung.Modelldaten;
using FEBibliothek.Modell;
using FEBibliothek.Modell.abstrakte_Klassen;

namespace FE_Berechnungen.Wärmeberechnung.Ergebnisse;

public partial class StationäreErgebnisseAnzeigen
{
    private readonly FeModell modell;
    private Shape letzterKnoten;
    private Shape letztesElement;

    public StationäreErgebnisseAnzeigen(FeModell feModell)
    {
        Language = XmlLanguage.GetLanguage("de-DE");
        modell = feModell;
        InitializeComponent();
    }

    private void Knoten_Loaded(object sender, RoutedEventArgs e)
    {
        KnotenGrid = sender as DataGrid;
        if (KnotenGrid != null) KnotenGrid.ItemsSource = modell.Knoten;
    }

    //SelectionChanged
    private void KnotenZeileSelected(object sender, SelectionChangedEventArgs e)
    {
        if (KnotenGrid.SelectedCells.Count <= 0) return;
        var cellInfo = KnotenGrid.SelectedCells[0];
        var cell = (KeyValuePair<string, Knoten>)cellInfo.Item;
        var knoten = cell.Value;
        if (letzterKnoten != null)
            StartFenster.StationäreErgebnisse.VisualWärmeErgebnisse.Children.Remove(letzterKnoten);
        letzterKnoten = StartFenster.StationäreErgebnisse.darstellung.KnotenZeigen(knoten, Brushes.Green, 1);
    }

    //LostFocus
    private void KeinKnotenSelected(object sender, RoutedEventArgs e)
    {
        StartFenster.StationäreErgebnisse.VisualWärmeErgebnisse.Children.Remove(letzterKnoten);
        letztesElement = null;
    }

    private void WärmeflussVektoren_Loaded(object sender, RoutedEventArgs e)
    {
        WärmeflussVektorGrid = sender as DataGrid;
        foreach (var item in modell.Elemente)
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
                    element3d8.WärmeStatus = element3d8.BerechneElementZustand(0, 0, 0);
                    break;
                }
            }

        if (WärmeflussVektorGrid != null) WärmeflussVektorGrid.ItemsSource = modell.Elemente;
    }

    //SelectionChanged
    private void ElementZeileSelected(object sender, SelectionChangedEventArgs e)
    {
        if (WärmeflussVektorGrid.SelectedCells.Count <= 0) return;
        var cellInfo = WärmeflussVektorGrid.SelectedCells[0];
        var cell = (KeyValuePair<string, AbstraktElement>)cellInfo.Item;
        var element = cell.Value;
        if (letztesElement != null)
            StartFenster.StationäreErgebnisse.VisualWärmeErgebnisse.Children.Remove(letztesElement);
        letztesElement = StartFenster.StationäreErgebnisse.darstellung.ElementFillZeichnen((Abstrakt2D)element,
            Brushes.Black, Colors.Green, .2, 2);
    }

    //LostFocus
    private void KeinElementSelected(object sender, RoutedEventArgs e)
    {
        StartFenster.StationäreErgebnisse.VisualWärmeErgebnisse.Children.Remove(letztesElement);
        letzterKnoten = null;
    }

    private void Wärmefluss_Loaded(object sender, RoutedEventArgs e)
    {
        WärmeflussGrid = sender as DataGrid;
        if (WärmeflussGrid != null) WärmeflussGrid.ItemsSource = modell.Randbedingungen;
    }
}