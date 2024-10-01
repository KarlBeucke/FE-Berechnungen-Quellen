using FE_Berechnungen.Wärmeberechnung.Modelldaten;
using FE_Berechnungen.Wärmeberechnung.ModelldatenAnzeigen;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Shapes;

namespace FE_Berechnungen.Wärmeberechnung.Ergebnisse;

public partial class InstationäreErgebnisseAnzeigen
{
    private readonly FeModell modell;
    private readonly WärmemodellVisualisieren wärmeModell;
    private readonly double[] zeit;
    private Knoten knoten;
    private Shape letzterKnoten;
    private Shape letztesElement;

    public InstationäreErgebnisseAnzeigen(FeModell modell)
    {
        Language = XmlLanguage.GetLanguage("de-DE");
        this.modell = modell;
        DataContext = this;
        wärmeModell = new WärmemodellVisualisieren(modell);
        wärmeModell.Show();
        InitializeComponent();
        Show();

        Knotenauswahl.ItemsSource = this.modell.Knoten.Keys;

        Dt = this.modell.Zeitintegration.Dt;
        var tmax = this.modell.Zeitintegration.Tmax;
        NSteps = (int)(tmax / Dt) + 1;
        zeit = new double[NSteps];
        for (var i = 0; i < NSteps; i++) zeit[i] = i * Dt;
        Zeitschrittauswahl.ItemsSource = zeit;
    }

    private double Dt { get; }
    private int NSteps { get; }
    private int Index { get; set; }

    //KnotentemperaturGrid
    private void DropDownKnotenauswahlClosed(object sender, EventArgs e)
    {
        if (Knotenauswahl.SelectedIndex < 0)
        {
            _ = MessageBox.Show("kein gültiger Knoten Identifikator ausgewählt", "Zeitschrittauswahl");
            return;
        }

        var knotenId = (string)Knotenauswahl.SelectedItem;
        if (modell.Knoten.TryGetValue(knotenId, out knoten))
        {
        }

        if (knoten != null)
        {
            var maxTemperatur = knoten.KnotenVariable[0].Max();
            var maxZeit = Dt * Array.IndexOf(knoten.KnotenVariable[0], maxTemperatur);
            var maxGradient = knoten.KnotenAbleitungen[0].Max();
            var maxZeitGradient = Dt * Array.IndexOf(knoten.KnotenAbleitungen[0], maxGradient);
            var maxText = "max. Temperatur = " + maxTemperatur.ToString("N4") + ", an Zeit =" + maxZeit.ToString("N2")
                          + "\nmax. Gradient      = " + maxGradient.ToString("N4") + ", an Zeit =" +
                          maxZeitGradient.ToString("N2");
            MaxText.Text = maxText;
        }

        KnotentemperaturGrid_Anzeigen();
    }

    private void KnotentemperaturGrid_Anzeigen()
    {
        if (knoten == null) return;
        var knotentemperaturen = new Dictionary<int, double[]>();
        for (var i = 0; i < NSteps; i++)
        {
            var zustand = new double[3];
            zustand[0] = zeit[i];
            zustand[1] = knoten.KnotenVariable[0][i];
            zustand[2] = knoten.KnotenAbleitungen[0][i];
            knotentemperaturen.Add(i, zustand);
        }

        KnotentemperaturGrid.ItemsSource = knotentemperaturen;

        if (letzterKnoten != null) wärmeModell.VisualWärmeModell.Children.Remove(letzterKnoten);
        letzterKnoten = wärmeModell.darstellung.KnotenZeigen(knoten, Brushes.Green, 1);
    }

    //KontenwerteGrid
    private void DropDownZeitschrittauswahlClosed(object sender, EventArgs e)
    {
        if (Zeitschrittauswahl.SelectedIndex < 0)
        {
            _ = MessageBox.Show("kein gültiger Zeitschritt ausgewählt", "Zeitschrittauswahl");
            return;
        }

        Index = Zeitschrittauswahl.SelectedIndex;
        Integrationsschritt.Text = "Modellzustand  an Zeitschritt  " + Index;

        foreach (var item in modell.Knoten) item.Value.Knotenfreiheitsgrade[0] = item.Value.KnotenVariable[0][Index];

        KnotenwerteGrid_Anzeigen();
        WärmeflussVektorenGrid_Anzeigen();
    }

    private void KnotenwerteGrid_Anzeigen()
    {
        var zeitschritt = new Dictionary<string, double[]>();
        foreach (var item in modell.Knoten)
        {
            var zustand = new double[2];
            zustand[0] = item.Value.KnotenVariable[0][Index];
            zustand[1] = item.Value.KnotenAbleitungen[0][Index];
            zeitschritt.Add(item.Key, zustand);
        }

        KnotenwerteGrid.ItemsSource = zeitschritt;
    }

    //SelectionChanged
    private void KnotenwerteZeileSelected(object sender, SelectionChangedEventArgs e)
    {
        if (KnotenwerteGrid.SelectedCells.Count <= 0) return;
        var cellInfo = KnotenwerteGrid.SelectedCells[0];
        var cell = (KeyValuePair<string, double[]>)cellInfo.Item;
        var knotenId = cell.Key;
        if (modell.Knoten.TryGetValue(knotenId, out knoten))
        {
        }

        if (letzterKnoten != null) wärmeModell.VisualWärmeModell.Children.Remove(letzterKnoten);
        letzterKnoten = wärmeModell.darstellung.KnotenZeigen(knoten, Brushes.Green, 1);
    }

    //LostFocus
    private void KeineKnotenwerteZeileSelected(object sender, RoutedEventArgs e)
    {
        wärmeModell.VisualWärmeModell.Children.Remove(letzterKnoten);
    }

    //WärmeflussvektorenGrid
    private void WärmeflussVektorenGrid_Anzeigen()
    {
        //var zeitschritt = new Dictionary<string, double[]>();
        foreach (var item in modell.Elemente)
            switch (item.Value)
            {
                case Abstrakt2D value:
                    {
                        value.ElementZustand = value.BerechneElementZustand(0, 0);
                        break;
                    }
                case Element3D8 value:
                    {
                        value.ElementZustand = value.BerechneElementZustand(0, 0, 0);
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
        if (letztesElement != null) wärmeModell.VisualWärmeModell.Children.Remove(letztesElement);
        letztesElement = wärmeModell.darstellung.ElementFillZeichnen((Abstrakt2D)element,
            Brushes.Black, Colors.Green, .2, 2);
    }

    //LostFocus
    private void KeinElementSelected(object sender, RoutedEventArgs e)
    {
        wärmeModell.VisualWärmeModell.Children.Remove(letztesElement);
        letzterKnoten = null;
    }

    //Unloaded
    private void ModellSchliessen(object sender, RoutedEventArgs e)
    {
        wärmeModell.Close();
    }
}