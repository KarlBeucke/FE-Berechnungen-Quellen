using FE_Berechnungen.Wärmeberechnung.Modelldaten;
using FEBibliothek.Modell;
using FEBibliothek.Modell.abstrakte_Klassen;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace FE_Berechnungen.Wärmeberechnung.Ergebnisse
{
    public partial class StationäreErgebnisseAnzeigen
    {
        private readonly FEModell modell;
        public StationäreErgebnisseAnzeigen(FEModell feModell)
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
        private void TemperaturVektoren_Loaded(object sender, RoutedEventArgs e)
        {
            TemperaturVektorGrid = sender as DataGrid;
            foreach (var item in modell.Elemente)
            {
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
                            element3d8.WärmeStatus = element3d8.ComputeElementState(0, 0, 0);
                            break;
                        }
                }
            }
            if (TemperaturVektorGrid != null) TemperaturVektorGrid.ItemsSource = modell.Elemente;
        }
        private void Wärmefluss_Loaded(object sender, RoutedEventArgs e)
        {
            WärmeflussGrid = sender as DataGrid;
            if (WärmeflussGrid != null) WärmeflussGrid.ItemsSource = modell.Randbedingungen;
        }
    }
}
