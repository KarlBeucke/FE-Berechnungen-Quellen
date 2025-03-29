using FE_Berechnungen.Tragwerksberechnung.Modelldaten;
using FE_Berechnungen.Tragwerksberechnung.ModelldatenLesen;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace FE_Berechnungen.Tragwerksberechnung.ModelldatenAnzeigen;

public partial class TragwerkdatenAnzeigen
{
    private readonly FeModell _modell;
    private Shape _letzterKnoten;
    private Shape _letztesElement;
    private string _removeKey;

    public TragwerkdatenAnzeigen(FeModell feModell)
    {
        Language = XmlLanguage.GetLanguage("de-DE");
        _modell = feModell;
        InitializeComponent();
        _letzterKnoten = null;
        _letztesElement = null;
    }

    // Loaded
    private void Knoten_Loaded(object sender, RoutedEventArgs e)
    {
        var knoten = _modell.Knoten.Select(item => item.Value).ToList();
        KnotenGrid = sender as DataGrid;
        if (KnotenGrid != null) KnotenGrid.ItemsSource = knoten;
    }

    // MouseDoubleClick
    private void NeuerKnoten(object sender, MouseButtonEventArgs e)
    {
        _ = new KnotenNeu(_modell);
        _modell.Berechnet = false;
        Close();
    }

    // UnloadingRow
    private void KnotenZeileLöschen(object sender, DataGridRowEventArgs e)
    {
        if (_removeKey == null) return;
        _modell.Knoten.Remove(_removeKey);
        _modell.Berechnet = false;
        Close();

        var tragwerk = new TragwerkdatenAnzeigen(_modell);
        tragwerk.Show();
    }

    // SelectionChanged
    private void KnotenZeileSelected(object sender, SelectionChangedEventArgs e)
    {
        if (KnotenGrid.SelectedCells.Count <= 0) return;
        var cellInfo = KnotenGrid.SelectedCells[0];
        var knoten = (Knoten)cellInfo.Item;
        _removeKey = knoten.Id;
        if (_letzterKnoten != null) StartFenster.TragwerkVisual.VisualTragwerkModel.Children.Remove(_letzterKnoten);
        _letzterKnoten =
            StartFenster.TragwerkVisual.Darstellung.KnotenZeigen(knoten, Brushes.Green, 1);
    }

    // LostFocus
    private void KeinKnotenSelected(object sender, RoutedEventArgs e)
    {
        StartFenster.TragwerkVisual.VisualTragwerkModel.Children.Remove(_letzterKnoten);
    }

    // Loaded
    private void ElementeGrid_Loaded(object sender, RoutedEventArgs e)
    {
        var elemente = _modell.Elemente.Select(item => item.Value).ToList();
        ElementGrid = sender as DataGrid;
        if (ElementGrid != null) ElementGrid.ItemsSource = elemente;
    }

    // MouseDoubleClick
    private void NeuesElement(object sender, MouseButtonEventArgs e)
    {
        _ = new ElementNeu(_modell);
        _modell.Berechnet = false;
        Close();
    }

    // UnloadingRow
    private void ElementZeileLöschen(object sender, DataGridRowEventArgs e)
    {
        if (_removeKey == null) return;
        _modell.Elemente.Remove(_removeKey);
        _modell.Berechnet = false;
        Close();

        var tragwerk = new TragwerkdatenAnzeigen(_modell);
        tragwerk.Show();
    }

    // SelectionChanged
    private void ElementZeileSelected(object sender, SelectionChangedEventArgs e)
    {
        if (ElementGrid.SelectedCells.Count <= 0) return;
        var cellInfo = ElementGrid.SelectedCells[0];
        var element = (Abstrakt2D)cellInfo.Item;
        _removeKey = element.ElementId;
        if (_letztesElement != null) StartFenster.TragwerkVisual.VisualTragwerkModel.Children.Remove(_letztesElement);
        _letztesElement = StartFenster.TragwerkVisual.Darstellung.ElementZeichnen(element, Brushes.Green, 5);
    }

    // LostFocus
    private void KeinElementSelected(object sender, RoutedEventArgs e)
    {
        StartFenster.TragwerkVisual.VisualTragwerkModel.Children.Remove(_letztesElement);
    }

    // Loaded
    private void Material_Loaded(object sender, RoutedEventArgs e)
    {
        var material = _modell.Material.Select(item => item.Value).ToList();
        MaterialGrid = sender as DataGrid;
        if (MaterialGrid != null) MaterialGrid.ItemsSource = material;
    }

    // MouseDoubleClick
    private void NeuesMaterial(object sender, MouseButtonEventArgs e)
    {
        _ = new MaterialNeu(_modell);
        Close();
    }

    // UnloadingRow
    private void MaterialZeileLöschen(object sender, DataGridRowEventArgs e)
    {
        if (_removeKey == null) return;
        _modell.Material.Remove(_removeKey);
        _modell.Berechnet = false;
        Close();

        var tragwerk = new TragwerkdatenAnzeigen(_modell);
        tragwerk.Show();
    }

    // SelectionChanged
    private void MaterialZeileSelected(object sender, SelectionChangedEventArgs e)
    {
        if (MaterialGrid.SelectedCells.Count <= 0) return;
        var cellInfo = MaterialGrid.SelectedCells[0];
        var material = (Material)cellInfo.Item;
        _removeKey = material.MaterialId;
    }

    // Loaded
    private void Querschnitt_Loaded(object sender, RoutedEventArgs e)
    {
        var querschnitt = _modell.Querschnitt.Select(item => item.Value).ToList();
        QuerschnittGrid = sender as DataGrid;
        if (QuerschnittGrid != null) QuerschnittGrid.ItemsSource = querschnitt;
    }

    // MouseDoubleClick
    private void NeuerQuerschnitt(object sender, MouseButtonEventArgs e)
    {
        _ = new QuerschnittNeu(_modell);
        Close();
    }

    // UnloadingRow
    private void QuerschnittZeileLöschen(object sender, DataGridRowEventArgs e)
    {
        if (_removeKey == null) return;
        _modell.Querschnitt.Remove(_removeKey);
        _modell.Berechnet = false;
        Close();

        var tragwerk = new TragwerkdatenAnzeigen(_modell);
        tragwerk.Show();
    }

    // SelectionChanged
    private void QuerschnittZeileSelected(object sender, SelectionChangedEventArgs e)
    {
        if (QuerschnittGrid.SelectedCells.Count <= 0) return;
        var cellInfo = QuerschnittGrid.SelectedCells[0];
        var querschnitt = (Querschnitt)cellInfo.Item;
        _removeKey = querschnitt.QuerschnittId;
    }

    // Loaded
    private void Lager_Loaded(object sender, RoutedEventArgs e)
    {
        var lager = new List<AbstraktRandbedingung>();
        foreach (var item in _modell.Randbedingungen)
        {
            for (var i = 0; i < item.Value.Vordefiniert.Length; i++)
                if (!item.Value.Festgehalten[i])
                    item.Value.Vordefiniert[i] = double.PositiveInfinity;
            lager.Add(item.Value);
        }

        LagerGrid = sender as DataGrid;
        if (LagerGrid != null) LagerGrid.ItemsSource = lager;
    }

    // MouseDoubleClick
    private void NeuesLager(object sender, MouseButtonEventArgs e)
    {
        const double vorX = 0, vorY = 0, vorRot = 0;
        _ = new LagerNeu(_modell, vorX, vorY, vorRot);
        _modell.Berechnet = false;
        Close();
    }

    // UnloadingRow
    private void LagerZeileLöschen(object sender, DataGridRowEventArgs e)
    {
        if (_removeKey == null) return;
        _modell.Randbedingungen.Remove(_removeKey);
        _modell.Berechnet = false;
        Close();

        var tragwerk = new TragwerkdatenAnzeigen(_modell);
        tragwerk.Show();
    }

    // SelectionChanged
    private void LagerZeileSelected(object sender, SelectionChangedEventArgs e)
    {
        if (LagerGrid.SelectedCells.Count <= 0) return;
        var cellInfo = LagerGrid.SelectedCells[0];
        var lager = (Lager)cellInfo.Item;
        _removeKey = lager.RandbedingungId;
    }

    // Loaded
    private void Knotenlast_Loaded(object sender, RoutedEventArgs e)
    {
        var lasten = _modell.Lasten.Select(item => item.Value).ToList();
        KnotenlastGrid = sender as DataGrid;
        if (KnotenlastGrid != null) KnotenlastGrid.ItemsSource = lasten;
    }

    // MouseDoubleClick
    private void NeueKnotenlast(object sender, MouseButtonEventArgs e)
    {
        _ = new KnotenlastNeu(_modell, string.Empty, string.Empty, 0, 0, 0);
        _modell.Berechnet = false;
        Close();
    }

    // UnloadingRow
    private void KnotenlastZeileLöschen(object sender, DataGridRowEventArgs e)
    {
        if (_removeKey == null) return;
        _modell.Lasten.Remove(_removeKey);
        _modell.Berechnet = false;
        Close();

        var tragwerk = new TragwerkdatenAnzeigen(_modell);
        tragwerk.Show();
    }

    // SelectionChanged
    private void KnotenlastZeileSelected(object sender, SelectionChangedEventArgs e)
    {
        if (KnotenlastGrid.SelectedCells.Count <= 0) return;
        var cellInfo = KnotenlastGrid.SelectedCells[0];
        var knotenlast = (AbstraktLast)cellInfo.Item;
        _removeKey = knotenlast.LastId;
    }

    // Loaded
    private void Punktlast_Loaded(object sender, RoutedEventArgs e)
    {
        var lasten = _modell.PunktLasten.Select(item => item.Value).ToList();
        PunktlastGrid = sender as DataGrid;
        if (PunktlastGrid != null) PunktlastGrid.ItemsSource = lasten;
    }

    // MouseDoubleClick
    private void NeuePunktlast(object sender, MouseButtonEventArgs e)
    {
        _ = new PunktlastNeu(_modell, string.Empty, string.Empty, 0, 0, 0);
        _modell.Berechnet = false;
        Close();
    }

    // UnloadingRow
    private void PunktlastZeileLöschen(object sender, DataGridRowEventArgs e)
    {
        if (_removeKey == null) return;
        _modell.PunktLasten.Remove(_removeKey);
        _modell.Berechnet = false;
        Close();

        var tragwerk = new TragwerkdatenAnzeigen(_modell);
        tragwerk.Show();
    }

    // SelectionChanged
    private void PunktlastZeileSelected(object sender, SelectionChangedEventArgs e)
    {
        if (PunktlastGrid.SelectedCells.Count <= 0) return;
        var cellInfo = PunktlastGrid.SelectedCells[0];
        var punktlast = (AbstraktElementLast)cellInfo.Item;
        _removeKey = punktlast.LastId;
    }

    // Loaded
    private void Linienlast_Loaded(object sender, RoutedEventArgs e)
    {
        var lasten = _modell.ElementLasten.Select(item => item.Value).ToList();
        LinienlastGrid = sender as DataGrid;
        if (LinienlastGrid != null) LinienlastGrid.ItemsSource = lasten;
    }

    // MouseDoubleClick
    private void NeueLinienlast(object sender, MouseButtonEventArgs e)
    {
        _ = new LinienlastNeu(_modell, string.Empty, string.Empty, 0, 0, 0, 0, true);
        _modell.Berechnet = false;
        Close();
    }

    // UnloadingRow
    private void LinienlastZeileLöschen(object sender, DataGridRowEventArgs e)
    {
        if (_removeKey == null) return;
        _modell.ElementLasten.Remove(_removeKey);
        _modell.Berechnet = false;
        Close();

        var tragwerk = new TragwerkdatenAnzeigen(_modell);
        tragwerk.Show();
    }

    // SelectionChanged
    private void LinienlastZeileSelected(object sender, RoutedEventArgs e)
    {
        if (LinienlastGrid.SelectedCells.Count <= 0) return;
        var cellInfo = LinienlastGrid.SelectedCells[0];
        var linienlast = (LinienLast)cellInfo.Item;
        _removeKey = linienlast.LastId;
    }

    private void Model_Changed(object sender, DataGridCellEditEndingEventArgs e)
    {
        _modell.Berechnet = false;
    }
}