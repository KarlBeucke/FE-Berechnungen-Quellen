using FE_Berechnungen.Elastizitätsberechnung.Modelldaten;
using FE_Berechnungen.Elastizitätsberechnung.ModelldatenLesen;
using FEBibliothek.Modell;
using FEBibliothek.Modell.abstrakte_Klassen;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;

namespace FE_Berechnungen.Elastizitätsberechnung.ModelldatenAnzeigen;

public partial class ElastizitätsdatenAnzeigen
{
    private readonly FeModell modell;
    private string removeKey;

    public ElastizitätsdatenAnzeigen(FeModell modell)
    {
        Language = XmlLanguage.GetLanguage("de-DE");
        this.modell = modell;
        InitializeComponent();
    }

    private void DatenLoaded(object sender, RoutedEventArgs e)
    {
        // Knoten
        var knoten = modell.Knoten.Select(item => item.Value).ToList();
        KnotenGrid.ItemsSource = knoten;

        // Elemente
        var elemente = modell.Elemente.Select(item => item.Value).ToList();
        ElementGrid.ItemsSource = elemente;

        // Material
        var material = modell.Material.Select(item => item.Value).ToList();
        MaterialGrid.Items.Clear();
        MaterialGrid.ItemsSource = material;

        // Querschnitt
        var querschnitt = modell.Querschnitt.Select(item => item.Value).ToList();
        QuerschnittGrid.Items.Clear();
        QuerschnittGrid.ItemsSource = querschnitt;

        // Lasten
        var knotenlast = modell.Lasten.Select(item => item.Value).ToList();
        KnotenlastGrid.Items.Clear();
        KnotenlastGrid.ItemsSource = knotenlast;

        // Randbedingungen
        var rand = new Dictionary<string, Lagerbedingung>();
        foreach (var item in modell.Randbedingungen)
        {
            var nodeId = item.Value.KnotenId;
            var supportName = item.Value.RandbedingungId;
            string[] vordefiniert = { "frei", "frei", "frei" };

            switch (item.Value.Typ)
            {
                case 1:
                    {
                        if (item.Value.Festgehalten[0]) vordefiniert[0] = item.Value.Vordefiniert[0].ToString("F4");
                        if (modell.Raumdimension == 2) vordefiniert[2] = string.Empty;
                        break;
                    }
                case 2:
                    {
                        if (item.Value.Festgehalten[1]) vordefiniert[1] = item.Value.Vordefiniert[1].ToString("F4");
                        if (modell.Raumdimension == 2) vordefiniert[2] = string.Empty;
                        break;
                    }
                case 3:
                    {
                        if (item.Value.Festgehalten[0]) vordefiniert[0] = item.Value.Vordefiniert[0].ToString("F4");
                        if (item.Value.Festgehalten[1]) vordefiniert[1] = item.Value.Vordefiniert[1].ToString("F4");
                        if (modell.Raumdimension == 2) vordefiniert[2] = string.Empty;
                        break;
                    }
                case 4:
                    {
                        if (item.Value.Festgehalten[2]) vordefiniert[2] = item.Value.Vordefiniert[2].ToString("F4");
                        break;
                    }
                case 5:
                    {
                        if (item.Value.Festgehalten[0]) vordefiniert[0] = item.Value.Vordefiniert[0].ToString("F4");
                        if (item.Value.Festgehalten[2]) vordefiniert[2] = item.Value.Vordefiniert[2].ToString("F4");
                        break;
                    }
                case 6:
                    {
                        if (item.Value.Festgehalten[1]) vordefiniert[1] = item.Value.Vordefiniert[1].ToString("F4");
                        if (item.Value.Festgehalten[2]) vordefiniert[2] = item.Value.Vordefiniert[2].ToString("F4");
                        break;
                    }
                case 7:
                    {
                        if (item.Value.Festgehalten[0]) vordefiniert[0] = item.Value.Vordefiniert[0].ToString("F4");
                        if (item.Value.Festgehalten[1]) vordefiniert[1] = item.Value.Vordefiniert[1].ToString("F4");
                        if (item.Value.Festgehalten[2]) vordefiniert[2] = item.Value.Vordefiniert[2].ToString("F4");
                        break;
                    }
                default:
                    throw new ModellAusnahme("Lagerbedingung für Lager " + supportName + " falsch definiert");
            }

            var lager = new Lagerbedingung(item.Key, nodeId, vordefiniert);
            rand.Add(item.Key, lager);
        }
        var randbedingung = rand.Select(item => item.Value).ToList();
        RandGrid.Items.Clear();
        RandGrid.ItemsSource = randbedingung;
    }

    // Knoten
    private void NeuerKnoten(object sender, MouseButtonEventArgs e)
    {
        const int anzahlKnotenfreitsgrade = 3;
        _ = new NeuerKnoten(modell, anzahlKnotenfreitsgrade);
        StartFenster.Berechnet = false;
        Close();
    }
    //UnloadingRow
    private void KnotenZeileLoeschen(object sender, DataGridRowEventArgs e)
    {
        if (removeKey == null) return;
        modell.Knoten.Remove(removeKey);
        StartFenster.Berechnet = false;
        Close();

        var tragwerk = new ElastizitätsdatenAnzeigen(modell);
        tragwerk.Show();
    }
    //SelectionChanged
    private void KnotenZeileSelected(object sender, SelectionChangedEventArgs e)
    {
        if (KnotenGrid.SelectedCells.Count <= 0) return;
        var cellInfo = KnotenGrid.SelectedCells[0];
        var knoten = (Knoten)cellInfo.Item;
        removeKey = knoten.Id;
    }

    // Elemente
    private void NeuesElement(object sender, MouseButtonEventArgs e)
    {
        _ = new NeuesElement(modell);
        StartFenster.Berechnet = false;
        Close();
    }
    //UnloadingRow
    private void ElementZeileLoeschen(object sender, DataGridRowEventArgs e)
    {
        if (removeKey == null) return;
        modell.Elemente.Remove(removeKey);
        StartFenster.Berechnet = false;
        Close();

        var tragwerk = new ElastizitätsdatenAnzeigen(modell);
        tragwerk.Show();
    }
    //SelectionChanged
    private void ElementZeileSelected(object sender, SelectionChangedEventArgs e)
    {
        if (ElementGrid.SelectedCells.Count <= 0) return;
        var cellInfo = ElementGrid.SelectedCells[0];
        var element = (AbstraktElement)cellInfo.Item;
        removeKey = element.ElementId;
    }

    // Material
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
        StartFenster.Berechnet = false;
        Close();

        var tragwerk = new ElastizitätsdatenAnzeigen(modell);
        tragwerk.Show();
    }
    //SelectionChanged
    private void MaterialZeileSelected(object sender, SelectionChangedEventArgs e)
    {
        if (MaterialGrid.SelectedCells.Count <= 0) return;
        var cellInfo = MaterialGrid.SelectedCells[0];
        var material = (Material)cellInfo.Item;
        removeKey = material.MaterialId;
    }

    // Querschnitt
    private void NeuerQuerschnitt(object sender, MouseButtonEventArgs e)
    {
        _ = new NeuerQuerschnitt(modell);
        Close();
    }
    //UnloadingRow
    private void QuerschnittZeileLoeschen(object sender, DataGridRowEventArgs e)
    {
        if (removeKey == null) return;
        modell.Querschnitt.Remove(removeKey);
        StartFenster.Berechnet = false;
        Close();

        var tragwerk = new ElastizitätsdatenAnzeigen(modell);
        tragwerk.Show();
    }
    //SelectionChanged
    private void QuerschnittZeileSelected(object sender, SelectionChangedEventArgs e)
    {
        if (QuerschnittGrid.SelectedCells.Count <= 0) return;
        var cellInfo = QuerschnittGrid.SelectedCells[0];
        var querschnitt = (Querschnitt)cellInfo.Item;
        removeKey = querschnitt.QuerschnittId;
    }

    // Lasten
    private void NeueKnotenlast(object sender, MouseButtonEventArgs e)
    {
        _ = new NeueKnotenlast(modell, string.Empty, string.Empty, 0, 0, 0);
        StartFenster.Berechnet = false;
        Close();
    }
    //UnloadingRow
    private void KnotenlastZeileLoeschen(object sender, DataGridRowEventArgs e)
    {
        if (removeKey == null) return;
        modell.Lasten.Remove(removeKey);
        StartFenster.Berechnet = false;
        Close();

        var tragwerk = new ElastizitätsdatenAnzeigen(modell);
        tragwerk.Show();
    }
    //SelectionChanged
    private void KnotenlastZeileSelected(object sender, SelectionChangedEventArgs e)
    {
        if (KnotenlastGrid.SelectedCells.Count <= 0) return;
        var cellInfo = KnotenlastGrid.SelectedCells[0];
        var knotenlast = (AbstraktLast)cellInfo.Item;
        removeKey = knotenlast.LastId;
    }

    // Randbedingungen
    private void NeueRandbedingung(object sender, MouseButtonEventArgs e)
    {
        _ = new NeuesLager(modell);
        StartFenster.Berechnet = false;
        Close();
    }
    //UnloadingRow.
    private void RandbedingungZeileLoeschen(object sender, DataGridRowEventArgs e)
    {
        if (removeKey == null) return;
        modell.Randbedingungen.Remove(removeKey);
        StartFenster.Berechnet = false;
        Close();

        var tragwerk = new ElastizitätsdatenAnzeigen(modell);
        tragwerk.Show();
    }
    //SelectionChanged
    private void RandbedingungZeileSelected(object sender, SelectionChangedEventArgs e)
    {
        if (RandGrid.SelectedCells.Count <= 0) return;
        var name = (Lagerbedingung)RandGrid.SelectedCells[0].Item;
        removeKey = name.LagerId;
    }

    private void Model_Changed(object sender, DataGridCellEditEndingEventArgs e)
    {
        StartFenster.Berechnet = false;
    }
}