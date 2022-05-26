using FEBibliothek.Modell;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Markup;

namespace FE_Berechnungen.Wärmeberechnung.Ergebnisse
{
    public partial class InstationäreErgebnisseAnzeigen : Window
    {
        public FEModell modell;
        private Knoten knoten;
        private readonly double[] zeit;

        public double Dt { get; }
        public int NSteps { get; }
        public int Index { get; set; }
        public string KnotenId { get; set; }

        public InstationäreErgebnisseAnzeigen(FEModell modell)
        {
            this.Language = XmlLanguage.GetLanguage("de-DE");
            this.modell = modell;
            InitializeComponent();
            Show();

            Knotenauswahl.ItemsSource = this.modell.Knoten.Keys;

            Dt = this.modell.Zeitintegration.Dt;
            var tmax = this.modell.Zeitintegration.Tmax;
            NSteps = (int)(tmax / Dt) + 1;
            zeit = new double[NSteps];
            for (var i = 0; i < NSteps; i++) { zeit[i] = (i * Dt); }
            Zeitschrittauswahl.ItemsSource = zeit;
        }

        private void DropDownKnotenauswahlClosed(object sender, System.EventArgs e)
        {
            if (Knotenauswahl.SelectedIndex < 0)
            {
                _ = MessageBox.Show("kein gültiger Knoten Identifikator ausgewählt", "Zeitschrittauswahl");
                return;
            }
            var knotenId = (string)Knotenauswahl.SelectedItem;
            if (modell.Knoten.TryGetValue(knotenId, out knoten)) { }
        }
        private void KnotentemperaturGrid_Anzeigen(object sender, RoutedEventArgs e)
        {
            if (knoten == null) return;
            var knotentemperaturen = new Dictionary<string, string>();
            var line = "Zustand des Knotens " + KnotenId;
            line += "\nZeit" + "\tTemperatur" + "\tGradient";
            knotentemperaturen.Add("Schritt", line);
            for (var i = 0; i < NSteps; i++)
            {
                line = zeit[i].ToString("N2");
                line += "\t" + knoten.KnotenVariable[0][i].ToString("N4");
                line += "\t\t" + knoten.KnotenAbleitungen[0][i].ToString("N4");
                knotentemperaturen.Add(i.ToString(), line);
            }
            KnotentemperaturGrid.ItemsSource = knotentemperaturen;
        }

        private void DropDownZeitschrittauswahlClosed(object sender, System.EventArgs e)
        {
            if (Zeitschrittauswahl.SelectedIndex < 0)
            {
                _ = MessageBox.Show("kein gültiger Zeitschritt ausgewählt", "Zeitschrittauswahl");
                return;
            }
            Index = Zeitschrittauswahl.SelectedIndex;
        }
        private void ZeitschrittGrid_Anzeigen(object sender, RoutedEventArgs e)
        {
            var zeitschritt = new Dictionary<string, string>();
            var line = "Modellzustand  an Zeitschritt  " + Index;
            line += "\nTemperatur" + "\tWärmefluss";
            zeitschritt.Add("Knoten", line);
            foreach (KeyValuePair<string, Knoten> item in modell.Knoten)
            {
                line = item.Value.KnotenVariable[0][Index].ToString("N4");
                line += "\t\t" + item.Value.KnotenAbleitungen[0][Index].ToString("N4");
                zeitschritt.Add(item.Key, line);
            }
            ZeitschrittGrid.ItemsSource = zeitschritt;
        }
    }
}
