using FEBibliothek.Modell;
using FEBibliothek.Modell.abstrakte_Klassen;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace FE_Berechnungen.Tragwerksberechnung.Ergebnisse
{
    public partial class StatikErgebnisseAnzeigen
    {
        private readonly FEModell modell;
        public StatikErgebnisseAnzeigen(FEModell feModell)
        {
            Language = XmlLanguage.GetLanguage("de-DE");
            modell = feModell;
            InitializeComponent();
        }
        private void Knotenverformungen_Loaded(object sender, RoutedEventArgs e)
        {
            KnotenverformungenGrid.ItemsSource = modell.Knoten;
        }

        private void Elementendkraefte_Loaded(object sender, RoutedEventArgs e)
        {
            var elementKräfte = new List<Stabendkräfte>();
            foreach (var item in modell.Elemente)
            {
                if (!(item.Value is AbstraktBalken balken)) continue;
                double[] balkenEndKräfte = balken.BerechneStabendkräfte();
                elementKräfte.Add(new Stabendkräfte(balken.ElementId, balkenEndKräfte));
            }

            ElementendkraefteGrid.ItemsSource = elementKräfte;
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
}
