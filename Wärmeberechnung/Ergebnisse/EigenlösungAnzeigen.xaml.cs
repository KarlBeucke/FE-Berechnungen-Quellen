using System.Windows.Controls;

namespace FE_Berechnungen.Wärmeberechnung.Ergebnisse;

public partial class EigenlösungAnzeigen
{
    private readonly FeModell _modell;

    public EigenlösungAnzeigen(FeModell modell)
    {
        Language = XmlLanguage.GetLanguage("de-DE");
        _modell = modell;
        InitializeComponent();
    }

    private void EigenwerteGrid_Loaded(object sender, RoutedEventArgs e)
    {
        var eigenfrequenzen = new Dictionary<int, double>();
        var nStates = _modell.Eigenzustand.AnzahlZustände;
        for (var k = 0; k < nStates; k++)
        {
            var value = Math.Sqrt(_modell.Eigenzustand.Eigenwerte[k]) / 2 / Math.PI;
            eigenfrequenzen.Add(k, value);
        }

        EigenwerteGrid = sender as DataGrid;
        if (EigenwerteGrid != null) EigenwerteGrid.ItemsSource = eigenfrequenzen;
    }

    private void EigenvektorenGrid_Loaded(object sender, RoutedEventArgs e)
    {
        var eigenvektorGrid = new Dictionary<string, string>();
        var dimension = _modell.Eigenzustand.Eigenvektoren[0].Length;
        var i = 0;
        for (var j = 0; j < dimension; j++)
        {
            var line = _modell.Eigenzustand.Eigenvektoren[0][i].ToString("N5");
            for (var k = 1; k < _modell.Eigenzustand.AnzahlZustände; k++)
                line += "\t" + _modell.Eigenzustand.Eigenvektoren[k][i].ToString("N5");
            eigenvektorGrid.Add(j.ToString(), line);
            i++;
        }

        EigenvektorenGrid = sender as DataGrid;
        if (EigenvektorenGrid != null) EigenvektorenGrid.ItemsSource = eigenvektorGrid;
    }
}