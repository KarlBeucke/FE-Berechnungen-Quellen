using FE_Berechnungen.Wärmeberechnung.Modelldaten;
using FE_Berechnungen.Wärmeberechnung.ModelldatenLesen;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace FE_Berechnungen.Wärmeberechnung.ModelldatenAnzeigen;

public partial class WärmedatenAnzeigen
{
    private readonly FeModell _modell;
    private Shape _letzterKnoten;
    private Shape _letztesElement;
    private string _removeKey;

    public WärmedatenAnzeigen(FeModell modell)
    {
        Language = XmlLanguage.GetLanguage("de-DE");
        _modell = modell;
        InitializeComponent();
        DataContext = this._modell;
    }

    private void Knoten_Loaded(object sender, RoutedEventArgs e)
    {
        var knoten = _modell.Knoten.Select(item => item.Value).ToList();
        KnotenGrid = sender as DataGrid;
        if (KnotenGrid != null) KnotenGrid.ItemsSource = knoten;
    }

    private void NeuerKnoten(object sender, MouseButtonEventArgs e)
    {
        _ = new KnotenNeu(_modell);
        _modell.Berechnet = false;
        Close();
    }

    //UnloadingRow
    private void KnotenZeileLoeschen(object sender, DataGridRowEventArgs e)
    {
        if (_removeKey == null) return;
        _modell.Knoten.Remove(_removeKey);
        _modell.Berechnet = false;
        Close();

        var wärme = new WärmedatenAnzeigen(_modell);
        wärme.Show();
    }

    //SelectionChanged
    private void KnotenZeileSelected(object sender, SelectionChangedEventArgs e)
    {
        if (KnotenGrid.SelectedCells.Count <= 0) return;
        var cellInfo = KnotenGrid.SelectedCells[0];
        var knoten = (Knoten)cellInfo.Item;
        _removeKey = knoten.Id;
        if (_letzterKnoten != null) StartFenster.WärmeVisual.VisualWärmeModell.Children.Remove(_letzterKnoten);
        _letzterKnoten = StartFenster.WärmeVisual.Darstellung.KnotenZeigen(knoten, Brushes.Green, 1);
    }

    //LostFocus
    private void KeinKnotenSelected(object sender, RoutedEventArgs e)
    {
        StartFenster.WärmeVisual.VisualWärmeModell.Children.Remove(_letzterKnoten);
    }

    private void Elemente_Loaded(object sender, RoutedEventArgs e)
    {
        var elemente = _modell.Elemente.Select(item => item.Value).ToList();
        ElementGrid = sender as DataGrid;
        if (ElementGrid == null) return;
        ElementGrid.Items.Clear();
        ElementGrid.ItemsSource = elemente;
    }

    private void NeuesElement(object sender, MouseButtonEventArgs e)
    {
        _ = new ElementNeu(_modell);
        _modell.Berechnet = false;
        Close();
    }

    //UnloadingRow
    private void ElementZeileLöschen(object sender, DataGridRowEventArgs e)
    {
        if (_removeKey == null) return;
        _modell.Elemente.Remove(_removeKey);
        _modell.Berechnet = false;
        Close();

        var wärme = new WärmedatenAnzeigen(_modell);
        wärme.Show();
    }

    //SelectionChanged
    private void ElementZeileSelected(object sender, SelectionChangedEventArgs e)
    {
        if (ElementGrid.SelectedCells.Count <= 0) return;
        var cellInfo = ElementGrid.SelectedCells[0];
        var element = (AbstraktElement)cellInfo.Item;
        _removeKey = element.ElementId;
        if (_letztesElement != null) StartFenster.WärmeVisual.VisualWärmeModell.Children.Remove(_letztesElement);
        _letztesElement = StartFenster.WärmeVisual.Darstellung.ElementFillZeichnen((Abstrakt2D)element,
            Brushes.Black, Colors.Green, .2, 2);
    }

    private void KeinElementSelected(object sender, RoutedEventArgs e)
    {
        StartFenster.WärmeVisual.VisualWärmeModell.Children.Remove(_letztesElement);
    }

    private void Material_Loaded(object sender, RoutedEventArgs e)
    {
        var material = _modell.Material.Select(item => item.Value).ToList();
        MaterialGrid = sender as DataGrid;
        if (MaterialGrid == null) return;
        MaterialGrid.Items.Clear();
        MaterialGrid.ItemsSource = material;
    }

    private void NeuesMaterial(object sender, MouseButtonEventArgs e)
    {
        _ = new MaterialNeu(_modell);
        Close();
    }

    //UnloadingRow
    private void MaterialZeileLöschen(object sender, DataGridRowEventArgs e)
    {
        if (_removeKey == null) return;
        _modell.Material.Remove(_removeKey);
        _modell.Berechnet = false;
        Close();

        var wärme = new WärmedatenAnzeigen(_modell);
        wärme.Show();
    }

    //SelectionChanged
    private void MaterialZeileSelected(object sender, SelectionChangedEventArgs e)
    {
        if (MaterialGrid.SelectedCells.Count <= 0) return;
        var cellInfo = MaterialGrid.SelectedCells[0];
        var material = (Material)cellInfo.Item;
        _removeKey = material.MaterialId;
    }

    private void Randbedingung_Loaded(object sender, RoutedEventArgs e)
    {
        var rand = _modell.Randbedingungen.Select(item => item.Value).ToList();
        RandbedingungGrid = sender as DataGrid;
        if (RandbedingungGrid != null) RandbedingungGrid.ItemsSource = rand;
    }

    private void NeueRandbedingung(object sender, MouseButtonEventArgs e)
    {
        _ = new RandbedingungNeu(_modell);
        _modell.Berechnet = false;
        Close();
    }

    //UnloadingRow
    private void RandbedingungZeileLöschen(object sender, DataGridRowEventArgs e)
    {
        if (_removeKey == null) return;
        _modell.Randbedingungen.Remove(_removeKey);
        _modell.Berechnet = false;
        Close();

        var wärme = new WärmedatenAnzeigen(_modell);
        wärme.Show();
    }

    //SelectionChanged
    private void RandbedingungZeileSelected(object sender, SelectionChangedEventArgs e)
    {
        if (RandbedingungGrid.SelectedCells.Count <= 0) return;
        var cellInfo = RandbedingungGrid.SelectedCells[0];
        var lager = (Randbedingung)cellInfo.Item;
        _removeKey = lager.RandbedingungId;
    }

    private void KnotenEinwirkungen_Loaded(object sender, RoutedEventArgs e)
    {
        var lasten = _modell.Lasten.Select(item => item.Value).ToList();
        KnotenEinwirkungenGrid = sender as DataGrid;
        if (KnotenEinwirkungenGrid != null) KnotenEinwirkungenGrid.ItemsSource = lasten;
    }

    private void NeueKnotenlast(object sender, MouseButtonEventArgs e)
    {
        _ = new KnotenlastNeu(_modell);
        _modell.Berechnet = false;
        Close();
    }

    //UnloadingRow
    private void KnotenlastZeileLöschen(object sender, DataGridRowEventArgs e)
    {
        if (_removeKey == null) return;
        _modell.Lasten.Remove(_removeKey);
        _modell.Berechnet = false;
        Close();

        var wärme = new WärmedatenAnzeigen(_modell);
        wärme.Show();
    }

    //SelectionChanged
    private void KnotenlastZeileSelected(object sender, SelectionChangedEventArgs e)
    {
        if (KnotenEinwirkungenGrid.SelectedCells.Count <= 0) return;
        var cellInfo = KnotenEinwirkungenGrid.SelectedCells[0];
        var last = (KnotenLast)cellInfo.Item;
        _removeKey = last.LastId;
    }

    private void LinienEinwirkungen_Loaded(object sender, RoutedEventArgs e)
    {
        var lasten = _modell.LinienLasten.Select(item => item.Value).Cast<AbstraktLast>().ToList();
        LinienEinwirkungenGrid = sender as DataGrid;
        if (LinienEinwirkungenGrid != null) LinienEinwirkungenGrid.ItemsSource = lasten;
    }

    private void NeueLinienlast(object sender, MouseButtonEventArgs e)
    {
        _ = new LinienlastNeu(_modell);
        _modell.Berechnet = false;
        Close();
    }

    //UnloadingRow
    private void LinienlastZeileLöschen(object sender, DataGridRowEventArgs e)
    {
        if (_removeKey == null) return;
        _modell.LinienLasten.Remove(_removeKey);
        _modell.Berechnet = false;
        Close();

        var wärme = new WärmedatenAnzeigen(_modell);
        wärme.Show();
    }

    //SelectionChanged
    private void LinienlastZeileSelected(object sender, SelectionChangedEventArgs e)
    {
        if (LinienEinwirkungenGrid.SelectedCells.Count <= 0) return;
        var cellInfo = LinienEinwirkungenGrid.SelectedCells[0];
        var last = (LinienLast)cellInfo.Item;
        _removeKey = last.LastId;
    }

    private void ElementEinwirkungen_Loaded(object sender, RoutedEventArgs e)
    {
        var lasten = _modell.ElementLasten.Select(item => item.Value).Cast<AbstraktLast>().ToList();
        ElementEinwirkungenGrid = sender as DataGrid;
        if (ElementEinwirkungenGrid != null) ElementEinwirkungenGrid.ItemsSource = lasten;
    }

    private void NeueElementlast(object sender, MouseButtonEventArgs e)
    {
        _ = new ElementlastNeu(_modell);
        _modell.Berechnet = false;
        Close();
    }

    //UnloadingRow
    private void ElementlastZeileLöschen(object sender, DataGridRowEventArgs e)
    {
        if (_removeKey == null) return;
        _modell.ElementLasten.Remove(_removeKey);
        _modell.Berechnet = false;
        Close();

        var wärme = new WärmedatenAnzeigen(_modell);
        wärme.Show();
    }

    //SelectionChanged
    private void ElementlastlastZeileSelected(object sender, SelectionChangedEventArgs e)
    {
        if (ElementEinwirkungenGrid.SelectedCells.Count <= 0) return;
        var cellInfo = ElementEinwirkungenGrid.SelectedCells[0];
        var last = (AbstraktLast)cellInfo.Item;
        _removeKey = last.LastId;
    }

    private void Model_Changed(object sender, DataGridCellEditEndingEventArgs e)
    {
        _modell.Berechnet = false;
    }

    //private void DataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
    //{
    //    // ... hol die TextBox, die editiert wurde
    //    var element = e.EditingElement as TextBox;
    //    var text = element.Text;

    //    // ... prüf, ob die Textveränderung abgelehnt werden soll
    //    // ... Ablehnung, falls der Nutzer ein "?" eingibt
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