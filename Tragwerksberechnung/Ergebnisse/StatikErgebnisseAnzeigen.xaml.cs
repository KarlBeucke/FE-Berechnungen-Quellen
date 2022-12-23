using FEBibliothek.Modell;
using FEBibliothek.Modell.abstrakte_Klassen;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Shapes;

namespace FE_Berechnungen.Tragwerksberechnung.Ergebnisse;

public partial class StatikErgebnisseAnzeigen
{
    private readonly FeModell modell;
    private Shape letztesElement;
    private Shape letzterKnoten;
    public StatikErgebnisseAnzeigen(FeModell feModell)
    {
        Language = XmlLanguage.GetLanguage("de-DE");
        modell = feModell;
        InitializeComponent();
    }
    private void Knotenverformungen_Loaded(object sender, RoutedEventArgs e)
    {
        KnotenverformungenGrid.ItemsSource = modell.Knoten;
    }
    //SelectionChanged
    private void KnotenZeileSelected(object sender, SelectionChangedEventArgs e)
    {
        if (KnotenverformungenGrid.SelectedCells.Count <= 0) return;
        var cellInfo = KnotenverformungenGrid.SelectedCells[0];
        var cell = (KeyValuePair<string, Knoten>)cellInfo.Item;
        var knoten = cell.Value;
        if (letzterKnoten != null)
        {
            StartFenster.statikErgebnisse.VisualTragwerkErgebnisse.Children.Remove(letzterKnoten);
        }
        letzterKnoten = StartFenster.statikErgebnisse.darstellung.KnotenZeigen(knoten, Brushes.Green, 1);
    }
    //LostFocus
    private void KeinKnotenSelected(object sender, RoutedEventArgs e)
    {
        StartFenster.statikErgebnisse.VisualTragwerkErgebnisse.Children.Remove(letzterKnoten);
    }

    private void Elementendkraefte_Loaded(object sender, RoutedEventArgs e)
    {
        var elementKräfte = new List<Stabendkräfte>();
        foreach (var item in modell.Elemente)
        {
            if (!(item.Value is AbstraktBalken balken)) continue;
            var balkenEndKräfte = balken.BerechneStabendkräfte();
            elementKräfte.Add(new Stabendkräfte(balken.ElementId, balkenEndKräfte));
        }

        ElementendkraefteGrid.ItemsSource = elementKräfte;
    }
    // SelectionChanged
    private void ElementZeileSelected(object sender, SelectionChangedEventArgs e)
    {
        if (ElementendkraefteGrid.SelectedCells.Count <= 0) return;
        var cellInfo = ElementendkraefteGrid.SelectedCells[0];
        var stabendKräfte = (Stabendkräfte)cellInfo.Item;
        if (!modell.Elemente.TryGetValue(stabendKräfte.ElementId, out var element)) return;
        if (letztesElement != null)
        {
            StartFenster.statikErgebnisse.VisualTragwerkErgebnisse.Children.Remove(letztesElement);
        }
        letztesElement = StartFenster.statikErgebnisse.darstellung.ElementZeichnen(element, Brushes.Green, 5);
    }
    //LostFocus
    private void KeinElementSelected(object sender, RoutedEventArgs e)
    {
        StartFenster.statikErgebnisse.VisualTragwerkErgebnisse.Children.Remove(letztesElement);
    }

    private void Lagerreaktionen_Loaded(object sender, RoutedEventArgs e)
    {
        var knotenReaktionen = new Dictionary<string, KnotenReaktion>();
        foreach (var item in modell.Randbedingungen)
        {
            var knotenId = item.Value.KnotenId;
            if (!modell.Knoten.TryGetValue(knotenId, out var knoten)) break;
            var knotenReaktion = new KnotenReaktion(knoten.Reaktionen);
            knotenReaktionen.Add(knotenId, knotenReaktion);
        }
        LagerreaktionenGrid = sender as DataGrid;
        if (LagerreaktionenGrid != null) LagerreaktionenGrid.ItemsSource = knotenReaktionen;
    }

    internal class KnotenReaktion
    {
        public double[] Reaktionen { get; }

        public KnotenReaktion(double[] reaktionen)
        {
            Reaktionen = reaktionen;
        }
    }
}