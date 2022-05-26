using FEBibliothek.Modell;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace FE_Berechnungen.Tragwerksberechnung.Ergebnisse
{
    public partial class EigenlösungAnzeigen
    {
        private readonly FEModell modell;
        public EigenlösungAnzeigen(FEModell modell)
        {
            Language = XmlLanguage.GetLanguage("de-DE");
            this.modell = modell;
            InitializeComponent();
        }

        private void EigenfrequenzenGrid_Loaded(object sender, RoutedEventArgs e)
        {
            var eigenfrequenzen = new Dictionary<int, string>();
            for (var k = 0; k < modell.Eigenzustand.AnzahlZustände; k++)
            {
                var strFormat = $"{Math.Sqrt(modell.Eigenzustand.Eigenwerte[k]) / 2 / Math.PI,7:N3}";
                var sb = new StringBuilder(strFormat);
                eigenfrequenzen.Add(k, sb.ToString());
            }
            EigenfrequenzenGrid = sender as DataGrid;
            if (EigenfrequenzenGrid != null) EigenfrequenzenGrid.ItemsSource = eigenfrequenzen;
        }

        private void EigenvektorenGrid_Loaded(object sender, RoutedEventArgs e)
        {
            var eigenvektoren = new Dictionary<int, string>();
            for (var j = 0; j < modell.Eigenzustand.Eigenvektoren[0].Length; j++)
            {
                var strFormat = $"{modell.Eigenzustand.Eigenvektoren[0][j],15:N5}";
                var sb = new StringBuilder(strFormat);
                for (var k = 1; k < modell.Eigenzustand.AnzahlZustände; k++)
                {
                    strFormat = $"{modell.Eigenzustand.Eigenvektoren[k][j],15:N5}";
                    sb.Append(strFormat);
                }
                eigenvektoren.Add(j, sb.ToString());
            }
            EigenvektorenGrid = sender as DataGrid;
            if (EigenvektorenGrid != null) EigenvektorenGrid.ItemsSource = eigenvektoren;
        }
    }
}