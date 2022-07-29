using FE_Berechnungen.Wärmeberechnung.Modelldaten;
using FE_Berechnungen.Wärmeberechnung.ModelldatenLesen;
using FEBibliothek.Modell;
using FEBibliothek.Modell.abstrakte_Klassen;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Shapes;

namespace FE_Berechnungen.Wärmeberechnung.ModelldatenAnzeigen
{
    public partial class WärmedatenAnzeigen
    {
        private readonly FEModell modell;
        private string removeKey;
        private Shape letztesElement;
        private Shape letzterKnoten;

        public WärmedatenAnzeigen(FEModell modell)
        {
            Language = XmlLanguage.GetLanguage("de-DE");
            this.modell = modell;
            InitializeComponent();
            DataContext = this.modell;
        }

        private void Knoten_Loaded(object sender, RoutedEventArgs e)
        {
            var knoten = modell.Knoten.Select(item => item.Value).ToList();
            KnotenGrid = sender as DataGrid;
            if (KnotenGrid != null) KnotenGrid.ItemsSource = knoten;
        }
        private void NeuerKnoten(object sender, MouseButtonEventArgs e)
        {
            _ = new NeuerKnoten(modell);
            StartFenster.berechnet = false;
            Close();
        }
        //UnloadingRow
        private void KnotenZeileLoeschen(object sender, DataGridRowEventArgs e)
        {
            if (removeKey == null) return;
            modell.Knoten.Remove(removeKey);
            StartFenster.berechnet = false;
            Close();

            var wärme = new WärmedatenAnzeigen(modell);
            wärme.Show();
        }
        //SelectionChanged
        private void KnotenZeileSelected(object sender, SelectionChangedEventArgs e)
        {
            if (KnotenGrid.SelectedCells.Count <= 0) return;
            var cellInfo = KnotenGrid.SelectedCells[0];
            var knoten = (Knoten)cellInfo.Item;
            removeKey = knoten.Id;
            if (letzterKnoten != null)
            {
                StartFenster.wärmeModell.VisualModel.Children.Remove(letzterKnoten);
            }
            letzterKnoten = StartFenster.wärmeModell.darstellung.KnotenZeigen(knoten, Brushes.Green, 1);
        }
        private void KeinKnotenSelected(object sender, RoutedEventArgs e)
        {
            StartFenster.wärmeModell.VisualModel.Children.Remove(letzterKnoten);
        }

        private void Elemente_Loaded(object sender, RoutedEventArgs e)
        {
            var elemente = modell.Elemente.Select(item => item.Value).ToList();
            ElementGrid = sender as DataGrid;
            if (ElementGrid == null) return;
            ElementGrid.Items.Clear();
            ElementGrid.ItemsSource = elemente;
        }
        private void NeuesElement(object sender, MouseButtonEventArgs e)
        {
            _ = new NeuesElement(modell);
            StartFenster.berechnet = false;
            Close();
        }
        //UnloadingRow
        private void ElementZeileLoeschen(object sender, DataGridRowEventArgs e)
        {
            if (removeKey == null) return;
            modell.Elemente.Remove(removeKey);
            StartFenster.berechnet = false;
            Close();

            var wärme = new WärmedatenAnzeigen(modell);
            wärme.Show();
        }
        //SelectionChanged
        private void ElementZeileSelected(object sender, SelectionChangedEventArgs e)
        {
            if (ElementGrid.SelectedCells.Count <= 0) return;
            var cellInfo = ElementGrid.SelectedCells[0];
            var element = (AbstraktElement)cellInfo.Item;
            removeKey = element.ElementId;
            if (letztesElement != null)
            {
                StartFenster.wärmeModell.VisualModel.Children.Remove(letztesElement);
            }
            letztesElement = StartFenster.wärmeModell.darstellung.ElementFillZeichnen((Abstrakt2D)element, 
                Brushes.Black, Colors.Green, .2, 2);
        }
        private void KeinElementSelected(object sender, RoutedEventArgs e)
        {
            StartFenster.wärmeModell.VisualModel.Children.Remove(letztesElement);
        }

        private void Material_Loaded(object sender, RoutedEventArgs e)
        {
            var material = modell.Material.Select(item => item.Value).ToList();
            MaterialGrid = sender as DataGrid;
            if (MaterialGrid == null) return;
            MaterialGrid.Items.Clear();
            MaterialGrid.ItemsSource = material;
        }
        private void NeuesMaterial(object sender, MouseButtonEventArgs e)
        {
            _ = new NeuesMaterial(modell);
            Close();
        }
        //UnloadingRow
        private void MaterialZeileLoeschen(object sender, DataGridRowEventArgs e)
        {
            if (removeKey == null) return;
            modell.Material.Remove(removeKey);
            StartFenster.berechnet = false;
            Close();

            var wärme = new WärmedatenAnzeigen(modell);
            wärme.Show();
        }
        //SelectionChanged
        private void MaterialZeileSelected(object sender, SelectionChangedEventArgs e)
        {
            if (MaterialGrid.SelectedCells.Count <= 0) return;
            var cellInfo = MaterialGrid.SelectedCells[0];
            var material = (Material)cellInfo.Item;
            removeKey = material.MaterialId;
        }

        private void Randbedingung_Loaded(object sender, RoutedEventArgs e)
        {
            var rand = modell.Randbedingungen.Select(item => item.Value).ToList();
            RandbedingungGrid = sender as DataGrid;
            if (RandbedingungGrid != null) RandbedingungGrid.ItemsSource = rand;
        }
        private void NeueRandbedingung(object sender, MouseButtonEventArgs e)
        {
            _ = new NeueRandbdingung(modell);
            StartFenster.berechnet = false;
            Close();
        }
        //UnloadingRow
        private void RandbedingungZeileLoeschen(object sender, DataGridRowEventArgs e)
        {
            if (removeKey == null) return;
            modell.Randbedingungen.Remove(removeKey);
            StartFenster.berechnet = false;
            Close();

            var wärme = new WärmedatenAnzeigen(modell);
            wärme.Show();
        }
        //SelectionChanged
        private void RandbedingungZeileSelected(object sender, SelectionChangedEventArgs e)
        {
            if (RandbedingungGrid.SelectedCells.Count <= 0) return;
            var cellInfo = RandbedingungGrid.SelectedCells[0];
            var lager = (Randbedingung)cellInfo.Item;
            removeKey = lager.RandbedingungId;
        }

        private void KnotenEinwirkungen_Loaded(object sender, RoutedEventArgs e)
        {
            var lasten = modell.Lasten.Select(item => item.Value).ToList();
            KnotenEinwirkungenGrid = sender as DataGrid;
            if (KnotenEinwirkungenGrid != null) KnotenEinwirkungenGrid.ItemsSource = lasten;
        }
        private void NeueKnotenlast(object sender, MouseButtonEventArgs e)
        {
            _ = new NeueKnotenlast(modell);
            StartFenster.berechnet = false;
            Close();
        }
        //UnloadingRow
        private void KnotenlastZeileLoeschen(object sender, DataGridRowEventArgs e)
        {
            if (removeKey == null) return;
            modell.Lasten.Remove(removeKey);
            StartFenster.berechnet = false;
            Close();

            var wärme = new WärmedatenAnzeigen(modell);
            wärme.Show();
        }
        //SelectionChanged
        private void KnotenlastZeileSelected(object sender, SelectionChangedEventArgs e)
        {
            if (KnotenEinwirkungenGrid.SelectedCells.Count <= 0) return;
            var cellInfo = KnotenEinwirkungenGrid.SelectedCells[0];
            var last = (KnotenLast)cellInfo.Item;
            removeKey = last.LastId;
        }

        private void LinienEinwirkungen_Loaded(object sender, RoutedEventArgs e)
        {
            var lasten = modell.LinienLasten.Select(item => item.Value).Cast<AbstraktLast>().ToList();
            LinienEinwirkungenGrid = sender as DataGrid;
            if (LinienEinwirkungenGrid != null) LinienEinwirkungenGrid.ItemsSource = lasten;
        }
        private void NeueLinienlast(object sender, MouseButtonEventArgs e)
        {
            _ = new NeueLinienlast(modell);
            StartFenster.berechnet = false;
            Close();
        }
        //UnloadingRow
        private void LinienlastZeileLoeschen(object sender, DataGridRowEventArgs e)
        {
            if (removeKey == null) return;
            modell.LinienLasten.Remove(removeKey);
            StartFenster.berechnet = false;
            Close();

            var wärme = new WärmedatenAnzeigen(modell);
            wärme.Show();
        }
        //SelectionChanged
        private void LinienlastZeileSelected(object sender, SelectionChangedEventArgs e)
        {
            if (LinienEinwirkungenGrid.SelectedCells.Count <= 0) return;
            var cellInfo = LinienEinwirkungenGrid.SelectedCells[0];
            var last = (LinienLast)cellInfo.Item;
            removeKey = last.LastId;
        }

        private void ElementEinwirkungen_Loaded(object sender, RoutedEventArgs e)
        {
            var lasten = modell.ElementLasten.Select(item => item.Value).Cast<AbstraktLast>().ToList();
            ElementEinwirkungenGrid = sender as DataGrid;
            if (ElementEinwirkungenGrid != null) ElementEinwirkungenGrid.ItemsSource = lasten;
        }
        private void NeueElementlast(object sender, MouseButtonEventArgs e)
        {
            _ = new NeueElementlast(modell);
            StartFenster.berechnet = false;
            Close();
        }
        //UnloadingRow
        private void ElementlastZeileLoeschen(object sender, DataGridRowEventArgs e)
        {
            if (removeKey == null) return;
            modell.ElementLasten.Remove(removeKey);
            StartFenster.berechnet = false;
            Close();

            var wärme = new WärmedatenAnzeigen(modell);
            wärme.Show();
        }
        //SelectionChanged
        private void ElementlastlastZeileSelected(object sender, SelectionChangedEventArgs e)
        {
            if (ElementEinwirkungenGrid.SelectedCells.Count <= 0) return;
            var cellInfo = ElementEinwirkungenGrid.SelectedCells[0];
            var last = (AbstraktLast)cellInfo.Item;
            removeKey = last.LastId;
        }

        private void Model_Changed(object sender, DataGridCellEditEndingEventArgs e)
        {
            StartFenster.berechnet = false;
        }

        //private void DataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        //{
        //    // ... hol die TextBox, die editiert wurde
        //    var element = e.EditingElement as TextBox;
        //    var text = element.Text;

        //    // ... pruef, ob die Textveraenderung abgelehnt werden soll
        //    // ... Ablehnung, falls der Nutzer ein ? eingibt
        //    if (text == "?")
        //    {
        //        Title = "Invalid";
        //        e.Cancel = true;
        //    }
        //    else
        //    {
        //        // ... zeige den Zellenwert im Titel
        //        Title = "Eingabe: " + text;
        //    }
        //}
    }
}