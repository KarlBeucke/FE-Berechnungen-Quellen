using FE_Berechnungen.Wärmeberechnung.Modelldaten;
using FE_Berechnungen.Wärmeberechnung.ModelldatenLesen;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;

namespace FE_Berechnungen.Wärmeberechnung.ModelldatenAnzeigen;

public partial class InstationäreDatenAnzeigen
{
    private readonly FeModell modell;
    private int removeIndex;
    private string removeKey;

    public InstationäreDatenAnzeigen(FeModell modell)
    {
        Language = XmlLanguage.GetLanguage("de-DE");
        this.modell = modell;
        InitializeComponent();
        DataContext = this.modell;
    }

    private void InstationärLoaded(object sender, RoutedEventArgs e)
    {
        // Anfangsbedingungen
        if (modell.Zeitintegration != null) Alle.IsChecked = modell.Zeitintegration.VonStationär;

        if (modell.Zeitintegration != null && modell.Zeitintegration.Anfangsbedingungen.Count > 0)
        {
            var anfangstemperaturen = modell.Zeitintegration.Anfangsbedingungen.Cast<Knotenwerte>().ToList();
            AnfangstemperaturenGrid.ItemsSource = anfangstemperaturen;
        }

        // Randbedingungen
        if (modell.ZeitabhängigeRandbedingung.Count > 0)
        {
            var randDatei = (from item
                    in modell.ZeitabhängigeRandbedingung
                             where item.Value.VariationsTyp == 0
                             select item.Value).ToList();
            if (randDatei.Count > 0) RandDateiGrid.ItemsSource = randDatei;

            var randKonstant = (from item
                    in modell.ZeitabhängigeRandbedingung
                                where item.Value.VariationsTyp == 1
                                select item.Value).ToList();
            if (randKonstant.Count > 0) RandKonstantGrid.ItemsSource = randKonstant;

            var randHarmonisch = (from item
                    in modell.ZeitabhängigeRandbedingung
                                  where item.Value.VariationsTyp == 2
                                  select item.Value).ToList();
            if (randHarmonisch.Count > 0) RandHarmonischGrid.ItemsSource = randHarmonisch;

            var randLinear = (from item
                    in modell.ZeitabhängigeRandbedingung
                              where item.Value.VariationsTyp == 3
                              select item.Value).ToList();
            if (randLinear.Count > 0) RandLinearGrid.ItemsSource = randLinear;
        }

        // Knotentemperaturen
        if (modell.ZeitabhängigeKnotenLasten.Count > 0)
        {
            var knotenDatei = (from item
                    in modell.ZeitabhängigeKnotenLasten
                               where item.Value.VariationsTyp == 0
                               select item.Value).ToList();
            if (knotenDatei.Count > 0) KnotenDateiGrid.ItemsSource = knotenDatei;

            var knotenHarmonisch = (from item
                    in modell.ZeitabhängigeKnotenLasten
                                    where item.Value.VariationsTyp == 2
                                    select item.Value).ToList();
            if (knotenHarmonisch.Count > 0) KnotenHarmonischGrid.ItemsSource = knotenHarmonisch;

            var knotenLinear = (from item
                    in modell.ZeitabhängigeKnotenLasten
                                where item.Value.VariationsTyp == 3
                                select item.Value).ToList();
            if (knotenLinear.Count > 0) KnotenLinearGrid.ItemsSource = knotenLinear;
        }

        // Elementtemperaturen
        if (modell.ZeitabhängigeElementLasten.Count > 0)
        {
            var elementLasten = (from item
                    in modell.ZeitabhängigeElementLasten
                                 where item.Value.VariationsTyp == 1
                                 select item.Value).ToList();
            if (elementLasten.Count > 0) ElementLastenGrid.ItemsSource = elementLasten;
        }
    }

    // ************************* Anfangsbedingungen *********************************
    private void ToggleStationaer(object sender, RoutedEventArgs e)
    {
        if (Alle.IsChecked != null && (bool)Alle.IsChecked)
        {
            Alle.IsChecked = true;
            modell.Zeitintegration.VonStationär = true;
        }
        else
        {
            Alle.IsChecked = false;
            modell.Zeitintegration.VonStationär = false;
        }
    }

    private void NeueAnfangstemperatur(object sender, MouseButtonEventArgs e)
    {
        _ = new ZeitAnfangstemperaturNeu(modell);
        modell.Berechnet = false;
        Close();
    }

    //UnloadingRow
    private void AnfangstemperaturZeileLoeschen(object sender, DataGridRowEventArgs e)
    {
        modell.Zeitintegration.Anfangsbedingungen.RemoveAt(removeIndex);
        modell.Berechnet = false;
        Close();

        var wärme = new InstationäreDatenAnzeigen(modell);
        wärme.Show();
    }

    //SelectionChanged
    private void AnfangstemperaturZeileSelected(object sender, SelectionChangedEventArgs e)
    {
        if (AnfangstemperaturenGrid.SelectedCells.Count <= 0) return;
        var cellInfo = AnfangstemperaturenGrid.SelectedCells[0];
        removeIndex = modell.Zeitintegration.Anfangsbedingungen.IndexOf(cellInfo.Item);
    }

    // ************************* Zeitabhängige Randbedingungen ***********************
    private void NeueRandtemperatur(object sender, MouseButtonEventArgs e)
    {
        _ = new ZeitRandtemperaturNeu(modell);
        modell.Berechnet = false;
        Close();
    }

    //SelectionChanged
    private void RandDateiSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (RandDateiGrid.SelectedCells.Count <= 0) return;
        var cellInfo = RandDateiGrid.SelectedCells[0];
        var randbedingung = (ZeitabhängigeRandbedingung)cellInfo.Item;
        removeKey = randbedingung.RandbedingungId;
    }

    private void RandKonstantSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (RandKonstantGrid.SelectedCells.Count <= 0) return;
        var cellInfo = RandKonstantGrid.SelectedCells[0];
        var randbedingung = (ZeitabhängigeRandbedingung)cellInfo.Item;
        removeKey = randbedingung.RandbedingungId;
    }

    private void RandHarmonischSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (RandHarmonischGrid.SelectedCells.Count <= 0) return;
        var cellInfo = RandHarmonischGrid.SelectedCells[0];
        var randbedingung = (ZeitabhängigeRandbedingung)cellInfo.Item;
        removeKey = randbedingung.RandbedingungId;
    }

    private void RandLinearSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (RandLinearGrid.SelectedCells.Count <= 0) return;
        var cellInfo = RandLinearGrid.SelectedCells[0];
        var zeitRand = (ZeitabhängigeRandbedingung)cellInfo.Item;
        removeKey = zeitRand.RandbedingungId;
    }

    //UnloadingRow
    private void RandDateiZeileLoeschen(object sender, DataGridRowEventArgs e)
    {
        if (removeKey == null) return;
        modell.ZeitabhängigeRandbedingung.Remove(removeKey);
        modell.Berechnet = false;
        Close();
        var wärme = new InstationäreDatenAnzeigen(modell);
        wärme.Show();
    }

    private void RandKonstantZeileLoeschen(object sender, DataGridRowEventArgs e)
    {
        if (removeKey == null) return;
        modell.ZeitabhängigeRandbedingung.Remove(removeKey);
        modell.Berechnet = false;
        Close();
        var wärme = new InstationäreDatenAnzeigen(modell);
        wärme.Show();
    }

    private void RandHarmonischZeileLoeschen(object sender, DataGridRowEventArgs e)
    {
        if (removeKey == null) return;
        modell.ZeitabhängigeRandbedingung.Remove(removeKey);
        modell.Berechnet = false;
        Close();
        var wärme = new InstationäreDatenAnzeigen(modell);
        wärme.Show();
    }

    private void RandLinearZeileLoeschen(object sender, DataGridRowEventArgs e)
    {
        if (removeKey == null) return;
        modell.ZeitabhängigeRandbedingung.Remove(removeKey);
        modell.Berechnet = false;
        Close();
        var wärme = new InstationäreDatenAnzeigen(modell);
        wärme.Show();
    }

    // ************************* Zeitabhängige Knotenlasten ********************************
    private void NeueKnotentemperatur(object sender, MouseButtonEventArgs e)
    {
        _ = new ZeitKnotentemperaturNeu(modell);
        modell.Berechnet = false;
        Close();
    }

    //SelectionChanged
    private void KnotenDateiSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (KnotenDateiGrid.SelectedCells.Count <= 0) return;
        var cellInfo = KnotenDateiGrid.SelectedCells[0];
        var last = (ZeitabhängigeKnotenLast)cellInfo.Item;
        removeKey = last.LastId;
    }

    private void KnotenHarmonischSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (KnotenHarmonischGrid.SelectedCells.Count <= 0) return;
        var cellInfo = KnotenHarmonischGrid.SelectedCells[0];
        var last = (ZeitabhängigeKnotenLast)cellInfo.Item;
        removeKey = last.LastId;
    }

    private void KnotenLinearSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (KnotenLinearGrid.SelectedCells.Count <= 0) return;
        var cellInfo = KnotenLinearGrid.SelectedCells[0];
        var last = (ZeitabhängigeKnotenLast)cellInfo.Item;
        removeKey = last.LastId;
    }

    //UnloadingRow
    private void KnotenDateiZeileLoeschen(object sender, DataGridRowEventArgs e)
    {
        if (removeKey == null) return;
        modell.ZeitabhängigeKnotenLasten.Remove(removeKey);
        modell.Berechnet = false;
        Close();
        var wärme = new InstationäreDatenAnzeigen(modell);
        wärme.Show();
    }

    private void KnotenHarmonischZeileLoeschen(object sender, DataGridRowEventArgs e)
    {
        if (removeKey == null) return;
        modell.ZeitabhängigeKnotenLasten.Remove(removeKey);
        modell.Berechnet = false;
        Close();
        var wärme = new InstationäreDatenAnzeigen(modell);
        wärme.Show();
    }

    private void KnotenLinearZeileLoeschen(object sender, DataGridRowEventArgs e)
    {
        if (removeKey == null) return;
        modell.ZeitabhängigeKnotenLasten.Remove(removeKey);
        modell.Berechnet = false;
        Close();
        var wärme = new InstationäreDatenAnzeigen(modell);
        wärme.Show();
    }

    // ************************* Zeitabhängige Elementtemperaturen ********************************
    private void NeueElementtemperatur(object sender, MouseButtonEventArgs e)
    {
        _ = new ZeitElementtemperaturNeu(modell);
        modell.Berechnet = false;
        Close();
    }

    //SelectionChanged
    private void ElementtemperaturSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (removeKey == null) return;
        modell.ZeitabhängigeElementLasten.Remove(removeKey);
        modell.Berechnet = false;
        Close();

        var wärme = new InstationäreDatenAnzeigen(modell);
        wärme.Show();
    }

    //UnloadingRow
    private void ElementtemperaturZeileLoeschen(object sender, DataGridRowEventArgs e)
    {
        if (removeKey == null) return;
        modell.ZeitabhängigeElementLasten.Remove(removeKey);
        modell.Berechnet = false;
        Close();

        var wärme = new InstationäreDatenAnzeigen(modell);
        wärme.Show();
    }

    // ************************* Model wurde verändert ********************************
    private void Model_Changed(object sender, DataGridCellEditEndingEventArgs e)
    {
        modell.Berechnet = false;
    }
}