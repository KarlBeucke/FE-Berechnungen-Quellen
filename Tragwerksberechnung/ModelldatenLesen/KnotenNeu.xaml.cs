using FE_Berechnungen.Tragwerksberechnung.ModelldatenAnzeigen;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows.Media;

namespace FE_Berechnungen.Tragwerksberechnung.ModelldatenLesen;

public partial class KnotenNeu
{
    private readonly ObservableCollection<Knoten> _knotenListe;
    private readonly FeModell _modell;

    public KnotenNeu(FeModell feModell)
    {
        InitializeComponent();
        _modell = feModell;
        // aktiviere Ereignishandler für Canvas
        StartFenster.TragwerkVisual.VisualTragwerkModel.Background = Brushes.Transparent;
        Show();

        //KnotenId.Focus();
        var ndof = _modell.AnzahlKnotenfreiheitsgrade;
        AnzahlDof.Text = ndof.ToString("N0", CultureInfo.CurrentCulture);
        _knotenListe = [];
        KnotenGrid.Items.Clear();
    }

    private void BtnDialogCancel_Click(object sender, RoutedEventArgs e)
    {
        // entferne Steuerungsknoten und deaktiviere Ereignishandler für Canvas
        StartFenster.TragwerkVisual.VisualTragwerkModel.Children.Remove(StartFenster.TragwerkVisual.Pilot);
        StartFenster.TragwerkVisual.VisualTragwerkModel.Background = null;
        StartFenster.TragwerkVisual.KnotenKeys?.Close();
        Close();
        StartFenster.TragwerkVisual.IsKnoten = false;

    }

    private void BtnDialogOk_Click(object sender, RoutedEventArgs e)
    {
        // kein Eintrag in Tabelle, Knotenwerte mit "Ok" bestätigt
        if (_knotenListe.Count == 0)
        {
            // vorhandener Knoten
            _modell.Knoten.TryGetValue(KnotenId.Text, out var vorhandenerKnoten);
            if (vorhandenerKnoten != null)
            {
                if (AnzahlDof.Text.Length > 0)
                    vorhandenerKnoten.AnzahlKnotenfreiheitsgrade = int.Parse(AnzahlDof.Text);
                if (X.Text.Length > 0) vorhandenerKnoten.Koordinaten[0] = double.Parse(X.Text);
                if (Y.Text.Length > 0) vorhandenerKnoten.Koordinaten[1] = double.Parse(Y.Text);
            }
            else
            {
                // neuer Knoten
                var dimension = _modell.Raumdimension;
                var koordinaten = new double[dimension];
                var anzahlKnotenDof = 3;
                try
                {
                    var substrings = X.Text.Split(",");
                    var x = substrings[0];
                    substrings = Y.Text.Split(",");
                    var y = substrings[0];
                    if (KnotenId.Text.Length == 0) KnotenId.Text = "K" + x + y;
                    if (AnzahlDof.Text.Length > 0) anzahlKnotenDof = int.Parse(AnzahlDof.Text);
                    if (X.Text.Length > 0) koordinaten[0] = double.Parse(X.Text);
                    if (Y.Text.Length > 0) koordinaten[1] = double.Parse(Y.Text);
                }
                catch (FormatException)
                {
                    _ = MessageBox.Show("ungültiges  Eingabeformat", "neuer Knoten");
                }
                var neuerKnoten = new Knoten(KnotenId.Text, koordinaten, anzahlKnotenDof, dimension);
                _modell.Knoten.Add(KnotenId.Text, neuerKnoten);
            }
        }

        // Knoten mit "Eintrag Tabelle" in "knotenListe" gesammelt 
        foreach (var knoten in _knotenListe)
        {
            // vorhandener Knoten
            if (_modell.Knoten.TryAdd(knoten.Id, knoten)) continue;
            _modell.Knoten.TryGetValue(knoten.Id, out var vorhandenerKnoten);
            if (vorhandenerKnoten == null) continue;
            try
            {
                if (AnzahlDof.Text.Length > 0)
                    vorhandenerKnoten.AnzahlKnotenfreiheitsgrade = int.Parse(AnzahlDof.Text);
                if (X.Text.Length > 0) vorhandenerKnoten.Koordinaten[0] = double.Parse(X.Text);
                if (Y.Text.Length > 0) vorhandenerKnoten.Koordinaten[1] = double.Parse(Y.Text);
            }
            catch (FormatException)
            {
                _ = MessageBox.Show("ungültiges  Eingabeformat", "neuer Knoten");
            }
        }

        // entferne Steuerungsknoten und deaktiviere Ereignishandler für Canvas
        StartFenster.TragwerkVisual.VisualTragwerkModel.Children.Remove(StartFenster.TragwerkVisual.Pilot);
        StartFenster.TragwerkVisual.VisualTragwerkModel.Background = null;
        StartFenster.TragwerkVisual.Close();
        Close();
        StartFenster.TragwerkVisual.KnotenKeys?.Close();

        StartFenster.TragwerkVisual = new TragwerkmodellVisualisieren(_modell);
        StartFenster.TragwerkVisual.Show();
        _modell.Berechnet = false;
    }

    private void KnotenIdLostFocus(object sender, RoutedEventArgs e)
    {
        if (!_modell.Knoten.TryGetValue(KnotenId.Text, out var vorhandenerKnoten))
        {

        }
        if (vorhandenerKnoten == null) return;
        AnzahlDof.Text = vorhandenerKnoten.AnzahlKnotenfreiheitsgrade.ToString();
        X.Text = vorhandenerKnoten.Koordinaten[0].ToString("N2", CultureInfo.CurrentCulture);
        Y.Text = vorhandenerKnoten.Koordinaten[1].ToString("N2", CultureInfo.CurrentCulture);
    }

    private void BtnTabelleneintrag(object sender, RoutedEventArgs e)
    {
        var dimension = _modell.Raumdimension;
        var koordinaten = new double[dimension];
        var anzahlKnotenDof = 3;
        try
        {
            var substrings = X.Text.Split(",");
            var x = substrings[0];
            substrings = Y.Text.Split(",");
            var y = substrings[0];
            if (KnotenId.Text.Length == 0) KnotenId.Text = "K" + x + y;
            if (AnzahlDof.Text.Length > 0) anzahlKnotenDof = int.Parse(AnzahlDof.Text);
            if (X.Text.Length > 0) koordinaten[0] = double.Parse(X.Text);
            if (Y.Text.Length > 0) koordinaten[1] = double.Parse(Y.Text);
        }
        catch (FormatException)
        {
            _ = MessageBox.Show("ungültiges  Eingabeformat", "neuer Knoten");
        }
        var neuerKnoten = new Knoten(KnotenId.Text, koordinaten, anzahlKnotenDof, dimension);
        _knotenListe.Add(neuerKnoten);
        if (KnotenGrid != null) KnotenGrid.ItemsSource = _knotenListe;

        KnotenId.Text = string.Empty;
        X.Text = string.Empty;
        Y.Text = string.Empty;
        Z.Text = string.Empty;
        //KnotenId.Focus();
    }

    private void BtnLöschen_Click(object sender, RoutedEventArgs e)
    {
        if (KnotenReferenziert()) return;
        _modell.Knoten.Remove(KnotenId.Text);
        Close();
        StartFenster.TragwerkVisual.Close();
        StartFenster.TragwerkVisual = new TragwerkmodellVisualisieren(_modell);
        StartFenster.TragwerkVisual.Show();
        _modell.Berechnet = false;
    }

    private bool KnotenReferenziert()
    {
        var id = KnotenId.Text;
        foreach (var element in _modell.Elemente.Where(element
                     => element.Value.KnotenIds[0] == id || element.Value.KnotenIds[1] == id))
        {
            _ = MessageBox.Show(
                "Knoten referenziert durch Element " + element.Value.ElementId + ", kann nicht gelöscht werden",
                "neuer Knoten");
            return true;
        }

        foreach (var last in _modell.Lasten.Where(last => last.Value.KnotenId == id))
        {
            _ = MessageBox.Show("Knoten referenziert durch Last " + last.Key + ", kann nicht gelöscht werden",
                "neue Last");
            return true;
        }

        foreach (var lager in _modell.Randbedingungen.Where(lager => lager.Value.KnotenId == id))
        {
            _ = MessageBox.Show("Knoten referenziert durch Lager " + lager.Key + ", kann nicht gelöscht werden",
                "neues Lager");
            return true;
        }

        //if (_modell.Elemente.Any(element => element.Value.KnotenIds.Any(knoten => knoten == id)))
        //{
        //    _ = MessageBox.Show("Knoten referenziert durch ein Element, kann nicht gelöscht werden", "neuer Knoten");
        //    return true;
        //}
        return false;
    }
}