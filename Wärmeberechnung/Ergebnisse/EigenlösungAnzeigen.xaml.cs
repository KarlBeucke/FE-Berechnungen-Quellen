using FEBibliothek.Modell;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace FE_Berechnungen.Wärmeberechnung.Ergebnisse;

public partial class EigenlösungAnzeigen : Window
{
    private readonly FeModell modell;

    public EigenlösungAnzeigen(FeModell modell)
    {
        Language = XmlLanguage.GetLanguage("de-DE");
        this.modell = modell;
        InitializeComponent();
    }

    private void EigenwerteGrid_Loaded(object sender, RoutedEventArgs e)
    {
        var eigenfrequenzen = new Dictionary<int, double>();
        var nStates = modell.Eigenzustand.AnzahlZustände;
        for (var k = 0; k < nStates; k++)
        {
            var value = Math.Sqrt(modell.Eigenzustand.Eigenwerte[k]) / 2 / Math.PI;
            eigenfrequenzen.Add(k, value);
        }

        EigenwerteGrid = sender as DataGrid;
        if (EigenwerteGrid != null) EigenwerteGrid.ItemsSource = eigenfrequenzen;
    }

    private void EigenvektorenGrid_Loaded(object sender, RoutedEventArgs e)
    {
        var eienvektorGrid = new Dictionary<string, string>();
        var dimension = modell.Eigenzustand.Eigenvektoren[0].Length;
        var i = 0;
        for (var j = 0; j < dimension; j++)
        {
            var line = modell.Eigenzustand.Eigenvektoren[0][i].ToString("N5");
            for (var k = 1; k < modell.Eigenzustand.AnzahlZustände; k++)
                line += "\t" + modell.Eigenzustand.Eigenvektoren[k][i].ToString("N5");
            eienvektorGrid.Add(j.ToString(), line);
            i++;
        }

        EigenvektorenGrid = sender as DataGrid;
        if (EigenvektorenGrid != null) EigenvektorenGrid.ItemsSource = eienvektorGrid;
    }
}