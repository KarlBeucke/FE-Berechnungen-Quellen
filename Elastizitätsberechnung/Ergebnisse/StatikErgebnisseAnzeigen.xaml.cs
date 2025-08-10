using System.Windows.Controls;

namespace FE_Berechnungen.Elastizitätsberechnung.Ergebnisse;

public partial class StatikErgebnisseAnzeigen
{
    private readonly FeModell _modell;

    public StatikErgebnisseAnzeigen(FeModell feModell)
    {
        Language = XmlLanguage.GetLanguage("de-DE");
        _modell = feModell;
        InitializeComponent();
        DataContext = this;
    }

    private void Knotenverformungen_Loaded(object sender, RoutedEventArgs e)
    {
        KnotenverformungenGrid = sender as DataGrid;
        if (KnotenverformungenGrid != null) KnotenverformungenGrid.ItemsSource = _modell.Knoten;
    }

    private void ElementspannungenGrid_Loaded(object sender, RoutedEventArgs e)
    {
        var elementSpannungen = new Dictionary<string, ElementSpannung>();
        foreach (var item in _modell.Elemente)
        {
            var elementSpannung = new ElementSpannung(item.Value.BerechneZustandsvektor());
            elementSpannungen.Add(item.Key, elementSpannung);
        }

        ElementspannungenGrid = sender as DataGrid;
        if (ElementspannungenGrid != null) ElementspannungenGrid.ItemsSource = elementSpannungen;
    }

    private void ReaktionenGrid_Loaded(object sender, RoutedEventArgs e)
    {
        ReaktionenGrid = sender as DataGrid;
        if (ReaktionenGrid != null) ReaktionenGrid.ItemsSource = _modell.Randbedingungen;
    }

    internal class ElementSpannung(double[] spannungen)
    {
        public double[] Spannungen { get; } = spannungen;
    }
}