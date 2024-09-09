using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Shapes;

namespace FE_Berechnungen.Tragwerksberechnung.Ergebnisse;

public partial class StatikErgebnisseAnzeigen
{
    private readonly FeModell _modell;
    private Shape _letztesElement, _letzterKnoten;

    public StatikErgebnisseAnzeigen(FeModell feModell)
    {
        Language = XmlLanguage.GetLanguage("de-DE");
        _modell = feModell;
        InitializeComponent();
    }

    private void Knotenverformungen_Loaded(object sender, RoutedEventArgs e)
    {
        KnotenverformungenGrid.ItemsSource = _modell.Knoten;
    }

    //SelectionChanged
    private void KnotenZeileSelected(object sender, SelectionChangedEventArgs e)
    {
        if (KnotenverformungenGrid.SelectedCells.Count <= 0) return;
        var cellInfo = KnotenverformungenGrid.SelectedCells[0];
        var cell = (KeyValuePair<string, Knoten>)cellInfo.Item;
        var knoten = cell.Value;
        if (_letzterKnoten != null)
            StartFenster.StatikErgebnisse.VisualTragwerkErgebnisse.Children.Remove(_letzterKnoten);
        _letzterKnoten = StartFenster.StatikErgebnisse.Darstellung.KnotenZeigen(knoten, Brushes.Green, 1);
    }

    //LostFocus
    private void KeinKnotenSelected(object sender, RoutedEventArgs e)
    {
        StartFenster.StatikErgebnisse.VisualTragwerkErgebnisse.Children.Remove(_letzterKnoten);
    }

    private void Elementendkräfte_Loaded(object sender, RoutedEventArgs e)
    {
        var elementKräfte = new List<Stabendkräfte>();
        foreach (var item in _modell.Elemente)
        {
            if (item.Value is not AbstraktBalken balken) continue;
            var balkenEndKräfte = balken.BerechneStabendkräfte();
            elementKräfte.Add(new Stabendkräfte(balken.ElementId, balkenEndKräfte));
        }

        ElementendkräfteGrid.ItemsSource = elementKräfte;
    }

    // SelectionChanged
    private void ElementZeileSelected(object sender, SelectionChangedEventArgs e)
    {
        if (ElementendkräfteGrid.SelectedCells.Count <= 0) return;
        var cellInfo = ElementendkräfteGrid.SelectedCells[0];
        var stabendKräfte = (Stabendkräfte)cellInfo.Item;
        if (!_modell.Elemente.TryGetValue(stabendKräfte.ElementId, out var element)) return;
        if (_letztesElement != null)
            StartFenster.StatikErgebnisse.VisualTragwerkErgebnisse.Children.Remove(_letztesElement);
        _letztesElement = StartFenster.StatikErgebnisse.Darstellung.ElementZeichnen(element, Brushes.Green, 5);
    }

    //LostFocus
    private void KeinElementSelected(object sender, RoutedEventArgs e)
    {
        StartFenster.StatikErgebnisse.VisualTragwerkErgebnisse.Children.Remove(_letztesElement);
    }

    private void Lagerreaktionen_Loaded(object sender, RoutedEventArgs e)
    {
        var knotenReaktionen = new Dictionary<string, KnotenReaktion>();
        foreach (var knotenId in _modell.Randbedingungen.Select(item => item.Value.KnotenId))
        {
            if (!_modell.Knoten.TryGetValue(knotenId, out var knoten)) break;
            var knotenReaktion = new KnotenReaktion(knoten.Reaktionen);
            knotenReaktionen.Add(knotenId, knotenReaktion);
        }

        LagerreaktionenGrid = sender as DataGrid;
        if (LagerreaktionenGrid != null) LagerreaktionenGrid.ItemsSource = knotenReaktionen;
    }

    internal class KnotenReaktion(double[] reaktionen)
    {
        public double[] Reaktionen { get; } = reaktionen;
    }
}