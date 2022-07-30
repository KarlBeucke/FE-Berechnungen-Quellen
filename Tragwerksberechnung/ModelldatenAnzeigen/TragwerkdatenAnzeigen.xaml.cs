using FE_Berechnungen.Tragwerksberechnung.Modelldaten;
using FE_Berechnungen.Tragwerksberechnung.ModelldatenLesen;
using FEBibliothek.Modell;
using FEBibliothek.Modell.abstrakte_Klassen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Shapes;

namespace FE_Berechnungen.Tragwerksberechnung.ModelldatenAnzeigen
{
    public partial class TragwerkdatenAnzeigen
    {
        private readonly FEModell modell;
        private string removeKey;
        private Shape letztesElement;
        private Shape letzterKnoten;

        public TragwerkdatenAnzeigen(FEModell feModell)
        {
            Language = XmlLanguage.GetLanguage("de-DE");
            modell = feModell;
            InitializeComponent();
            letzterKnoten = null;
            letztesElement = null;
        }

        private void Knoten_Loaded(object sender, RoutedEventArgs e)
        {
            var knoten = modell.Knoten.Select(item => item.Value).ToList();
            KnotenGrid = sender as DataGrid;
            if (KnotenGrid != null) KnotenGrid.ItemsSource = knoten;
        }
        private void NeuerKnoten(object sender, MouseButtonEventArgs e)
        {
            const int anzahlKnotenfreitsgrade = 3;
            _ = new NeuerKnoten(modell, anzahlKnotenfreitsgrade);
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

            var tragwerk = new TragwerkdatenAnzeigen(modell);
            tragwerk.Show();
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
                StartFenster.tragwerksModell.VisualModel.Children.Remove(letzterKnoten);
            }
            letzterKnoten =
                StartFenster.tragwerksModell.darstellung.KnotenZeigen(knoten, Brushes.Green, 1);
        }
        private void KeinKnotenSelected(object sender, RoutedEventArgs e)
        {
            StartFenster.tragwerksModell.VisualModel.Children.Remove(letzterKnoten);
        }

        private void ElementeGrid_Loaded(object sender, RoutedEventArgs e)
        {
            var elemente = modell.Elemente.Select(item => item.Value).ToList();
            ElementGrid = sender as DataGrid;
            if (ElementGrid != null) ElementGrid.ItemsSource = elemente;
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

            var tragwerk = new TragwerkdatenAnzeigen(modell);
            tragwerk.Show();
        }
        //SelectionChanged
        private void ElementZeileSelected(object sender, SelectionChangedEventArgs e)
        {
            if (ElementGrid.SelectedCells.Count <= 0) return;
            var cellInfo = ElementGrid.SelectedCells[0];
            var element = (Abstrakt2D)cellInfo.Item;
            removeKey = element.ElementId;
            if (letztesElement != null)
            {
                StartFenster.tragwerksModell.VisualModel.Children.Remove(letztesElement);
            }
            letztesElement = StartFenster.tragwerksModell.darstellung.ElementZeichnen(element, Brushes.Green, 5);
        }
        private void KeinElementSelected(object sender, RoutedEventArgs e)
        {
            StartFenster.tragwerksModell.VisualModel.Children.Remove(letztesElement);
        }

        private void Material_Loaded(object sender, RoutedEventArgs e)
        {
            var material = modell.Material.Select(item => item.Value).ToList();
            MaterialGrid = sender as DataGrid;
            if (MaterialGrid != null) MaterialGrid.ItemsSource = material;
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

            var tragwerk = new TragwerkdatenAnzeigen(modell);
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

        private void Querschnitt_Loaded(object sender, RoutedEventArgs e)
        {
            var querschnitt = modell.Querschnitt.Select(item => item.Value).ToList();
            QuerschnittGrid = sender as DataGrid;
            if (QuerschnittGrid != null) QuerschnittGrid.ItemsSource = querschnitt;
        }
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
            StartFenster.berechnet = false;
            Close();

            var tragwerk = new TragwerkdatenAnzeigen(modell);
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

        private void Lager_Loaded(object sender, RoutedEventArgs e)
        {
            var lager = new List<AbstraktRandbedingung>();
            foreach (var item in modell.Randbedingungen)
            {
                for (var i = 0; i < item.Value.Vordefiniert.Length; i++)
                {
                    //if (!item.Value.Restrained[i]) item.Value.Prescribed[i] = Double.NaN;
                    if (!item.Value.Festgehalten[i]) item.Value.Vordefiniert[i] = double.PositiveInfinity;
                }
                lager.Add(item.Value);
            }
            LagerGrid = sender as DataGrid;
            if (LagerGrid != null) LagerGrid.ItemsSource = lager;
        }
        private void NeuesLager(object sender, MouseButtonEventArgs e)
        {
            const double vorX = 0, vorY = 0, vorRot = 0;
            _ = new NeuesLager(modell, vorX, vorY, vorRot);
            StartFenster.berechnet = false;
            Close();
        }
        //UnloadingRow
        private void LagerZeileLoeschen(object sender, DataGridRowEventArgs e)
        {
            if (removeKey == null) return;
            modell.Randbedingungen.Remove(removeKey);
            StartFenster.berechnet = false;
            Close();

            var tragwerk = new TragwerkdatenAnzeigen(modell);
            tragwerk.Show();
        }
        //SelectionChanged
        private void LagerZeileSelected(object sender, SelectionChangedEventArgs e)
        {
            if (LagerGrid.SelectedCells.Count <= 0) return;
            var cellInfo = LagerGrid.SelectedCells[0];
            var lager = (Lager)cellInfo.Item;
            removeKey = lager.RandbedingungId;
        }

        private void Knotenlast_Loaded(object sender, RoutedEventArgs e)
        {
            var lasten = modell.Lasten.Select(item => item.Value).ToList();
            KnotenlastGrid = sender as DataGrid;
            if (KnotenlastGrid != null) KnotenlastGrid.ItemsSource = lasten;
        }
        private void NeueKnotenlast(object sender, MouseButtonEventArgs e)
        {
            _ = new NeueKnotenlast(modell, String.Empty, String.Empty, 0, 0, 0);
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

            var tragwerk = new TragwerkdatenAnzeigen(modell);
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

        private void Punktlast_Loaded(object sender, RoutedEventArgs e)
        {
            var lasten = modell.PunktLasten.Select(item => item.Value).ToList();
            PunktlastGrid = sender as DataGrid;
            if (PunktlastGrid != null) PunktlastGrid.ItemsSource = lasten;
        }
        private void NeuePunktlast(object sender, MouseButtonEventArgs e)
        {
            _ = new NeuePunktlast(modell, string.Empty, string.Empty, 0, 0, 0);
            StartFenster.berechnet = false;
            Close();
        }
        //UnloadingRow
        private void PunktlastZeileLoeschen(object sender, DataGridRowEventArgs e)
        {
            if (removeKey == null) return;
            modell.PunktLasten.Remove(removeKey);
            StartFenster.berechnet = false;
            Close();

            var tragwerk = new TragwerkdatenAnzeigen(modell);
            tragwerk.Show();
        }
        //SelectionChanged
        private void PunktlastZeileSelected(object sender, SelectionChangedEventArgs e)
        {
            if (PunktlastGrid.SelectedCells.Count <= 0) return;
            var cellInfo = PunktlastGrid.SelectedCells[0];
            var punktlast = (AbstraktElementLast)cellInfo.Item;
            removeKey = punktlast.LastId;
        }

        private void Linienlast_Loaded(object sender, RoutedEventArgs e)
        {
            var lasten = modell.ElementLasten.
                Select(item => item.Value).ToList();
            LinienlastGrid = sender as DataGrid;
            if (LinienlastGrid != null) LinienlastGrid.ItemsSource = lasten;
        }
        private void NeueLinienlast(object sender, MouseButtonEventArgs e)
        {
            _ = new NeueLinienlast(modell, String.Empty, String.Empty, 0, 0, 0, 0, "false");
            StartFenster.berechnet = false;
            Close();
        }
        //UnloadingRow
        private void LinienlastZeileLoeschen(object sender, DataGridRowEventArgs e)
        {
            if (removeKey == null) return;
            modell.ElementLasten.Remove(removeKey);
            StartFenster.berechnet = false;
            Close();

            var tragwerk = new TragwerkdatenAnzeigen(modell);
            tragwerk.Show();
        }
        //SelectionChanged
        private void LinienlastZeileSelected(object sender, RoutedEventArgs e)
        {
            if (LinienlastGrid.SelectedCells.Count <= 0) return;
            var cellInfo = LinienlastGrid.SelectedCells[0];
            var linienlast = (LinienLast)cellInfo.Item;
            removeKey = linienlast.LastId;
        }

        private void Model_Changed(object sender, DataGridCellEditEndingEventArgs e)
        {
            StartFenster.berechnet = false;
        }
    }
}