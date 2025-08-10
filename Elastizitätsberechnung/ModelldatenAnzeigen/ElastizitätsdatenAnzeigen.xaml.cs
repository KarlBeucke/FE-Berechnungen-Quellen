using FE_Berechnungen.Elastizitätsberechnung.Modelldaten;
using FE_Berechnungen.Elastizitätsberechnung.ModelldatenLesen;
using System.Windows.Controls;
using System.Windows.Input;

namespace FE_Berechnungen.Elastizitätsberechnung.ModelldatenAnzeigen;

public partial class ElastizitätsdatenAnzeigen
{
    private readonly FeModell _modell;
    private string _removeKey;

    public ElastizitätsdatenAnzeigen(FeModell modell)
    {
        Language = XmlLanguage.GetLanguage("de-DE");
        this._modell = modell;
        InitializeComponent();
    }

    private void DatenLoaded(object sender, RoutedEventArgs e)
    {
        // Knoten
        var knoten = _modell.Knoten.Select(item => item.Value).ToList();
        KnotenGrid.ItemsSource = knoten;

        // Elemente
        var elemente = _modell.Elemente.Select(item => item.Value).ToList();
        ElementGrid.ItemsSource = elemente;

        // Material
        var material = _modell.Material.Select(item => item.Value).ToList();
        MaterialGrid.Items.Clear();
        MaterialGrid.ItemsSource = material;

        // Querschnitt
        var querschnitt = _modell.Querschnitt.Select(item => item.Value).ToList();
        QuerschnittGrid.Items.Clear();
        QuerschnittGrid.ItemsSource = querschnitt;

        // Lasten
        var knotenlast = _modell.Lasten.Select(item => item.Value).ToList();
        KnotenlastGrid.Items.Clear();
        KnotenlastGrid.ItemsSource = knotenlast;

        // Randbedingungen
        var rand = new Dictionary<string, Lagerbedingung>();
        foreach (var (supportName, value) in _modell.Randbedingungen)
        {
            var nodeId = value.KnotenId;
            string[] vordefiniert = ["frei", "frei", "frei"];

            switch (value.Typ)
            {
                case 1:
                    {
                        vordefiniert[0] = value.Vordefiniert[0].ToString("F4");
                        if (_modell.Raumdimension == 2) vordefiniert[2] = string.Empty;
                        break;
                    }
                case 2:
                    {
                        vordefiniert[1] = value.Vordefiniert[1].ToString("F4");
                        if (_modell.Raumdimension == 2) vordefiniert[2] = string.Empty;
                        break;
                    }
                case 3:
                    {
                        vordefiniert[0] = value.Vordefiniert[0].ToString("F4");
                        vordefiniert[1] = value.Vordefiniert[1].ToString("F4");
                        if (_modell.Raumdimension == 2) vordefiniert[2] = string.Empty;
                        break;
                    }
                case 4:
                    {
                        vordefiniert[2] = value.Vordefiniert[2].ToString("F4");
                        break;
                    }
                case 5:
                    {
                        vordefiniert[0] = value.Vordefiniert[0].ToString("F4");
                        vordefiniert[2] = value.Vordefiniert[2].ToString("F4");
                        break;
                    }
                case 6:
                    {
                        vordefiniert[1] = value.Vordefiniert[1].ToString("F4");
                        vordefiniert[2] = value.Vordefiniert[2].ToString("F4");
                        break;
                    }
                case 7:
                    {
                        vordefiniert[0] = value.Vordefiniert[0].ToString("F4");
                        vordefiniert[1] = value.Vordefiniert[1].ToString("F4");
                        vordefiniert[2] = value.Vordefiniert[2].ToString("F4");
                        break;
                    }
                default:
                    throw new ModellAusnahme("\nLagerbedingung für Lager " + supportName + " falsch definiert");
            }

            var lager = new Lagerbedingung(supportName, nodeId, vordefiniert);
            rand.Add(supportName, lager);
        }

        var randbedingung = rand.Select(item => item.Value).ToList();
        RandGrid.Items.Clear();
        RandGrid.ItemsSource = randbedingung;
    }

    // Knoten
    private void NeuerKnoten(object sender, MouseButtonEventArgs e)
    {
        const int anzahlKnotenfreiheitsgrade = 3;
        _ = new NeuerKnoten(_modell, anzahlKnotenfreiheitsgrade);
        _modell.Berechnet = false;
        Close();
    }

    //UnloadingRow
    private void KnotenZeileLöschen(object sender, DataGridRowEventArgs e)
    {
        if (_removeKey == null) return;
        _modell.Knoten.Remove(_removeKey);
        _modell.Berechnet = false;
        Close();

        var tragwerk = new ElastizitätsdatenAnzeigen(_modell);
        tragwerk.Show();
    }

    //SelectionChanged
    private void KnotenZeileSelected(object sender, SelectionChangedEventArgs e)
    {
        if (KnotenGrid.SelectedCells.Count <= 0) return;
        var cellInfo = KnotenGrid.SelectedCells[0];
        var knoten = (Knoten)cellInfo.Item;
        _removeKey = knoten.Id;
    }

    // Elemente
    private void NeuesElement(object sender, MouseButtonEventArgs e)
    {
        _ = new NeuesElement(_modell);
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

        var tragwerk = new ElastizitätsdatenAnzeigen(_modell);
        tragwerk.Show();
    }

    //SelectionChanged
    private void ElementZeileSelected(object sender, SelectionChangedEventArgs e)
    {
        if (ElementGrid.SelectedCells.Count <= 0) return;
        var cellInfo = ElementGrid.SelectedCells[0];
        var element = (AbstraktElement)cellInfo.Item;
        _removeKey = element.ElementId;
    }

    // Material
    private void NeuesMaterial(object sender, MouseButtonEventArgs e)
    {
        _ = new NeuesMaterial(_modell);
        Close();
    }

    //UnloadingRow
    private void MaterialZeileLöschen(object sender, DataGridRowEventArgs e)
    {
        if (_removeKey == null) return;
        _modell.Material.Remove(_removeKey);
        _modell.Berechnet = false;
        Close();

        var tragwerk = new ElastizitätsdatenAnzeigen(_modell);
        tragwerk.Show();
    }

    //SelectionChanged
    private void MaterialZeileSelected(object sender, SelectionChangedEventArgs e)
    {
        if (MaterialGrid.SelectedCells.Count <= 0) return;
        var cellInfo = MaterialGrid.SelectedCells[0];
        var material = (Material)cellInfo.Item;
        _removeKey = material.MaterialId;
    }

    // Querschnitt
    private void NeuerQuerschnitt(object sender, MouseButtonEventArgs e)
    {
        _ = new NeuerQuerschnitt(_modell);
        Close();
    }

    //UnloadingRow
    private void QuerschnittZeileLöschen(object sender, DataGridRowEventArgs e)
    {
        if (_removeKey == null) return;
        _modell.Querschnitt.Remove(_removeKey);
        _modell.Berechnet = false;
        Close();

        var tragwerk = new ElastizitätsdatenAnzeigen(_modell);
        tragwerk.Show();
    }

    //SelectionChanged
    private void QuerschnittZeileSelected(object sender, SelectionChangedEventArgs e)
    {
        if (QuerschnittGrid.SelectedCells.Count <= 0) return;
        var cellInfo = QuerschnittGrid.SelectedCells[0];
        var querschnitt = (Querschnitt)cellInfo.Item;
        _removeKey = querschnitt.QuerschnittId;
    }

    // Lasten
    private void NeueKnotenlast(object sender, MouseButtonEventArgs e)
    {
        _ = new NeueKnotenlast(_modell, string.Empty, string.Empty, 0, 0, 0);
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

        var tragwerk = new ElastizitätsdatenAnzeigen(_modell);
        tragwerk.Show();
    }

    //SelectionChanged
    private void KnotenlastZeileSelected(object sender, SelectionChangedEventArgs e)
    {
        if (KnotenlastGrid.SelectedCells.Count <= 0) return;
        var cellInfo = KnotenlastGrid.SelectedCells[0];
        var knotenlast = (AbstraktLast)cellInfo.Item;
        _removeKey = knotenlast.LastId;
    }

    // Randbedingungen
    private void NeuesLager(object sender, MouseButtonEventArgs e)
    {
        _ = new LagerNeu(_modell);
        _modell.Berechnet = false;
        Close();
    }

    //UnloadingRow.
    private void RandbedingungZeileLöschen(object sender, DataGridRowEventArgs e)
    {
        if (_removeKey == null) return;
        _modell.Randbedingungen.Remove(_removeKey);
        _modell.Berechnet = false;
        Close();

        var tragwerk = new ElastizitätsdatenAnzeigen(_modell);
        tragwerk.Show();
    }

    //SelectionChanged
    private void RandbedingungZeileSelected(object sender, SelectionChangedEventArgs e)
    {
        if (RandGrid.SelectedCells.Count <= 0) return;
        var name = (Lagerbedingung)RandGrid.SelectedCells[0].Item;
        _removeKey = name.LagerId;
    }

    private void Model_Changed(object sender, DataGridCellEditEndingEventArgs e)
    {
        _modell.Berechnet = false;
    }
}