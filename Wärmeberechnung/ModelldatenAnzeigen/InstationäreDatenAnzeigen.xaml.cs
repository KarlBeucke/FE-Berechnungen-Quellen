using FE_Berechnungen.Wärmeberechnung.Modelldaten;
using FE_Berechnungen.Wärmeberechnung.ModelldatenLesen;
using System.Windows.Controls;
using System.Windows.Input;

namespace FE_Berechnungen.Wärmeberechnung.ModelldatenAnzeigen;

public partial class InstationäreDatenAnzeigen
{
    private readonly FeModell _modell;
    private int _removeIndex;
    private string _removeKey;

    public InstationäreDatenAnzeigen(FeModell modell)
    {
        Language = XmlLanguage.GetLanguage("de-DE");
        _modell = modell;
        InitializeComponent();
        DataContext = this._modell;
    }

    private void InstationärLoaded(object sender, RoutedEventArgs e)
    {
        // Anfangsbedingungen
        if (_modell.Zeitintegration != null) Alle.IsChecked = _modell.Zeitintegration.VonStationär;

        if (_modell.Zeitintegration != null && _modell.Zeitintegration.Anfangsbedingungen.Count > 0)
        {
            var anfangstemperaturen = _modell.Zeitintegration.Anfangsbedingungen.ToList();
            AnfangstemperaturenGrid.ItemsSource = anfangstemperaturen;
        }

        // Randbedingungen
        if (_modell.ZeitabhängigeRandbedingung.Count > 0)
        {
            var randDatei = (from item
                    in _modell.ZeitabhängigeRandbedingung
                             where item.Value.VariationsTyp == 0
                             select item.Value).ToList();
            if (randDatei.Count > 0) RandDateiGrid.ItemsSource = randDatei;

            var randKonstant = (from item
                    in _modell.ZeitabhängigeRandbedingung
                                where item.Value.VariationsTyp == 1
                                select item.Value).ToList();
            if (randKonstant.Count > 0) RandKonstantGrid.ItemsSource = randKonstant;

            var randHarmonisch = (from item
                    in _modell.ZeitabhängigeRandbedingung
                                  where item.Value.VariationsTyp == 2
                                  select item.Value).ToList();
            if (randHarmonisch.Count > 0) RandHarmonischGrid.ItemsSource = randHarmonisch;

            var randLinear = (from item
                    in _modell.ZeitabhängigeRandbedingung
                              where item.Value.VariationsTyp == 3
                              select item.Value).ToList();
            if (randLinear.Count > 0) RandLinearGrid.ItemsSource = randLinear;
        }

        // Knotentemperaturen
        if (_modell.ZeitabhängigeKnotenLasten.Count > 0)
        {
            var knotenDatei = (from item
                    in _modell.ZeitabhängigeKnotenLasten
                               where item.Value.VariationsTyp == 0
                               select item.Value).ToList();
            if (knotenDatei.Count > 0) KnotenDateiGrid.ItemsSource = knotenDatei;

            var knotenHarmonisch = (from item
                    in _modell.ZeitabhängigeKnotenLasten
                                    where item.Value.VariationsTyp == 2
                                    select item.Value).ToList();
            if (knotenHarmonisch.Count > 0) KnotenHarmonischGrid.ItemsSource = knotenHarmonisch;

            var knotenLinear = (from item
                    in _modell.ZeitabhängigeKnotenLasten
                                where item.Value.VariationsTyp == 3
                                select item.Value).ToList();
            if (knotenLinear.Count > 0) KnotenLinearGrid.ItemsSource = knotenLinear;
        }

        // Elementtemperaturen
        if (_modell.ZeitabhängigeElementLasten.Count <= 0) return;
        var elementLasten = (from item
                in _modell.ZeitabhängigeElementLasten
                             where item.Value.VariationsTyp == 1
                             select item.Value).ToList();
        if (elementLasten.Count > 0) ElementLastenGrid.ItemsSource = elementLasten;
    }

    // ************************* Anfangsbedingungen *********************************
    private void ToggleStationär(object sender, RoutedEventArgs e)
    {
        if (Alle.IsChecked != null && (bool)Alle.IsChecked)
        {
            Alle.IsChecked = true;
            _modell.Zeitintegration.VonStationär = true;
        }
        else
        {
            Alle.IsChecked = false;
            _modell.Zeitintegration.VonStationär = false;
        }
    }

    private void NeueAnfangstemperatur(object sender, MouseButtonEventArgs e)
    {
        _ = new ZeitKnotenAnfangstemperaturNeu(_modell);
        _modell.Berechnet = false;
        Close();
    }

    //UnloadingRow
    private void AnfangstemperaturZeileLöschen(object sender, DataGridRowEventArgs e)
    {
        _modell.Zeitintegration.Anfangsbedingungen.RemoveAt(_removeIndex);
        _modell.Berechnet = false;
        Close();

        var wärme = new InstationäreDatenAnzeigen(_modell);
        wärme.Show();
    }

    //SelectionChanged
    private void AnfangstemperaturZeileSelected(object sender, SelectionChangedEventArgs e)
    {
        if (AnfangstemperaturenGrid.SelectedCells.Count <= 0) return;
        var cellInfo = AnfangstemperaturenGrid.SelectedCells[0];
        _removeIndex = _modell.Zeitintegration.Anfangsbedingungen.IndexOf((Knotenwerte)cellInfo.Item);
    }

    // ************************* Zeitabhängige Randbedingungen ***********************
    private void NeueRandtemperatur(object sender, MouseButtonEventArgs e)
    {
        _ = new ZeitRandbedingungNeu(_modell);
        _modell.Berechnet = false;
        Close();
    }

    //SelectionChanged
    private void RandDateiSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (RandDateiGrid.SelectedCells.Count <= 0) return;
        var cellInfo = RandDateiGrid.SelectedCells[0];
        var randbedingung = (ZeitabhängigeRandbedingung)cellInfo.Item;
        _removeKey = randbedingung.RandbedingungId;
    }

    private void RandKonstantSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (RandKonstantGrid.SelectedCells.Count <= 0) return;
        var cellInfo = RandKonstantGrid.SelectedCells[0];
        var randbedingung = (ZeitabhängigeRandbedingung)cellInfo.Item;
        _removeKey = randbedingung.RandbedingungId;
    }

    private void RandHarmonischSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (RandHarmonischGrid.SelectedCells.Count <= 0) return;
        var cellInfo = RandHarmonischGrid.SelectedCells[0];
        var randbedingung = (ZeitabhängigeRandbedingung)cellInfo.Item;
        _removeKey = randbedingung.RandbedingungId;
    }

    private void RandLinearSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (RandLinearGrid.SelectedCells.Count <= 0) return;
        var cellInfo = RandLinearGrid.SelectedCells[0];
        var zeitRand = (ZeitabhängigeRandbedingung)cellInfo.Item;
        _removeKey = zeitRand.RandbedingungId;
    }

    //UnloadingRow
    private void RandDateiZeileLöschen(object sender, DataGridRowEventArgs e)
    {
        if (_removeKey == null) return;
        _modell.ZeitabhängigeRandbedingung.Remove(_removeKey);
        _modell.Berechnet = false;
        Close();
        var wärme = new InstationäreDatenAnzeigen(_modell);
        wärme.Show();
    }

    private void RandKonstantZeileLöschen(object sender, DataGridRowEventArgs e)
    {
        if (_removeKey == null) return;
        _modell.ZeitabhängigeRandbedingung.Remove(_removeKey);
        _modell.Berechnet = false;
        Close();
        var wärme = new InstationäreDatenAnzeigen(_modell);
        wärme.Show();
    }

    private void RandHarmonischZeileLöschen(object sender, DataGridRowEventArgs e)
    {
        if (_removeKey == null) return;
        _modell.ZeitabhängigeRandbedingung.Remove(_removeKey);
        _modell.Berechnet = false;
        Close();
        var wärme = new InstationäreDatenAnzeigen(_modell);
        wärme.Show();
    }

    private void RandLinearZeileLöschen(object sender, DataGridRowEventArgs e)
    {
        if (_removeKey == null) return;
        _modell.ZeitabhängigeRandbedingung.Remove(_removeKey);
        _modell.Berechnet = false;
        Close();
        var wärme = new InstationäreDatenAnzeigen(_modell);
        wärme.Show();
    }

    // ************************* Zeitabhängige Knotenlasten ********************************
    private void NeueKnotentemperatur(object sender, MouseButtonEventArgs e)
    {
        _ = new ZeitKnotenlastNeu(_modell);
        _modell.Berechnet = false;
        Close();
    }

    //SelectionChanged
    private void KnotenDateiSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (KnotenDateiGrid.SelectedCells.Count <= 0) return;
        var cellInfo = KnotenDateiGrid.SelectedCells[0];
        var last = (ZeitabhängigeKnotenLast)cellInfo.Item;
        _removeKey = last.LastId;
    }

    private void KnotenHarmonischSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (KnotenHarmonischGrid.SelectedCells.Count <= 0) return;
        var cellInfo = KnotenHarmonischGrid.SelectedCells[0];
        var last = (ZeitabhängigeKnotenLast)cellInfo.Item;
        _removeKey = last.LastId;
    }

    private void KnotenLinearSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (KnotenLinearGrid.SelectedCells.Count <= 0) return;
        var cellInfo = KnotenLinearGrid.SelectedCells[0];
        var last = (ZeitabhängigeKnotenLast)cellInfo.Item;
        _removeKey = last.LastId;
    }

    //UnloadingRow
    private void KnotenDateiZeileLöschen(object sender, DataGridRowEventArgs e)
    {
        if (_removeKey == null) return;
        _modell.ZeitabhängigeKnotenLasten.Remove(_removeKey);
        _modell.Berechnet = false;
        Close();
        var wärme = new InstationäreDatenAnzeigen(_modell);
        wärme.Show();
    }

    private void KnotenHarmonischZeileLöschen(object sender, DataGridRowEventArgs e)
    {
        if (_removeKey == null) return;
        _modell.ZeitabhängigeKnotenLasten.Remove(_removeKey);
        _modell.Berechnet = false;
        Close();
        var wärme = new InstationäreDatenAnzeigen(_modell);
        wärme.Show();
    }

    private void KnotenLinearZeileLöschen(object sender, DataGridRowEventArgs e)
    {
        if (_removeKey == null) return;
        _modell.ZeitabhängigeKnotenLasten.Remove(_removeKey);
        _modell.Berechnet = false;
        Close();
        var wärme = new InstationäreDatenAnzeigen(_modell);
        wärme.Show();
    }

    // ************************* Zeitabhängige Elementtemperaturen ********************************
    private void NeueElementtemperatur(object sender, MouseButtonEventArgs e)
    {
        _ = new ZeitElementlastNeu(_modell);
        _modell.Berechnet = false;
        Close();
    }

    //SelectionChanged
    private void ElementtemperaturSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_removeKey == null) return;
        _modell.ZeitabhängigeElementLasten.Remove(_removeKey);
        _modell.Berechnet = false;
        Close();

        var wärme = new InstationäreDatenAnzeigen(_modell);
        wärme.Show();
    }

    //UnloadingRow
    private void ElementtemperaturZeileLöschen(object sender, DataGridRowEventArgs e)
    {
        if (_removeKey == null) return;
        _modell.ZeitabhängigeElementLasten.Remove(_removeKey);
        _modell.Berechnet = false;
        Close();

        var wärme = new InstationäreDatenAnzeigen(_modell);
        wärme.Show();
    }

    // ************************* Model wurde verändert ********************************
    private void Model_Changed(object sender, DataGridCellEditEndingEventArgs e)
    {
        _modell.Berechnet = false;
    }
}