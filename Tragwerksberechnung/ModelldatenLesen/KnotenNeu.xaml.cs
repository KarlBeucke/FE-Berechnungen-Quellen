using FE_Berechnungen.Tragwerksberechnung.ModelldatenAnzeigen;
using FEBibliothek.Modell;
using System.Globalization;
using System.Windows;
using System.Collections.ObjectModel;

namespace FE_Berechnungen.Tragwerksberechnung.ModelldatenLesen;

public partial class KnotenNeu
{
    private readonly FeModell _modell;
    private KnotenKeys _knotenKeys;
    private int ndof;
    private ObservableCollection<Knoten> _knotenListe;

    public KnotenNeu()
    {
        InitializeComponent();
    }
    public KnotenNeu(FeModell feModell)
    {
        InitializeComponent();
        _modell = feModell;
        // aktiviere Ereignishandler für Canvas
        StartFenster.TragwerkVisual.VisualTragwerkModel.Background = System.Windows.Media.Brushes.Transparent;
        Show();
        _knotenKeys = new KnotenKeys(_modell) { Owner = this };
        _knotenKeys.Show();

        KnotenId.Focus();
        ndof = _modell.AnzahlKnotenfreiheitsgrade;
        AnzahlDof.Text = ndof.ToString("N0", CultureInfo.CurrentCulture);
        _knotenListe = [];
        KnotenGrid.Items.Clear();
    }

    private void BtnDialogCancel_Click(object sender, RoutedEventArgs e)
    {
        // entferne Steuerungsknoten und deaktiviere Ereignishandler für Canvas
        StartFenster.TragwerkVisual.VisualTragwerkModel.Children.Remove(StartFenster.TragwerkVisual.Knoten);
        StartFenster.TragwerkVisual.VisualTragwerkModel.Background = null;
        Close();
        _knotenKeys.Close();
    }

    private void BtnDialogOk_Click(object sender, RoutedEventArgs e)
    {
        // kein Eintrag in Tabelle, Knotenwerte mit "Ok" bestätigt
        if (_knotenListe.Count == 0)
        {
            if (_modell.Knoten.ContainsKey(KnotenId.Text))
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
            }
            else
            {
                // neuer Knoten
                var dimension = _modell.Raumdimension;
                var koordinaten = new double[dimension];
                var anzahlKnotenDof = 3;
                if (AnzahlDof.Text.Length > 0) anzahlKnotenDof = int.Parse(AnzahlDof.Text);
                if (X.Text.Length > 0) koordinaten[0] = double.Parse(X.Text);
                if (Y.Text.Length > 0) koordinaten[1] = double.Parse(Y.Text);
                var neuerKnoten = new Knoten(KnotenId.Text, koordinaten, anzahlKnotenDof, dimension);
                _modell.Knoten.Add(KnotenId.Text, neuerKnoten);
            }
        }

        // Knoten mit "Eintrag Tabelle" in "knotenListe" gesammelt 
        foreach (var knoten in _knotenListe)
        {
            // vorhandener Knoten
            if (_modell.Knoten.ContainsKey(knoten.Id))
            {
                _modell.Knoten.TryGetValue(knoten.Id, out var vorhandenerKnoten);
                if (vorhandenerKnoten == null) continue;
                if (AnzahlDof.Text.Length > 0)
                    vorhandenerKnoten.AnzahlKnotenfreiheitsgrade = int.Parse(AnzahlDof.Text);
                if (X.Text.Length > 0) vorhandenerKnoten.Koordinaten[0] = double.Parse(X.Text);
                if (Y.Text.Length > 0) vorhandenerKnoten.Koordinaten[1] = double.Parse(Y.Text);
            }
            // neuer Knoten
            else
            {
                _modell.Knoten.Add(knoten.Id, knoten);
            }
        }

        // entferne Steuerungsknoten und deaktiviere Ereignishandler für Canvas
        StartFenster.TragwerkVisual.VisualTragwerkModel.Children.Remove(StartFenster.TragwerkVisual.Knoten);
        StartFenster.TragwerkVisual.VisualTragwerkModel.Background = null;
        StartFenster.TragwerkVisual.Close();
        Close();
        _knotenKeys.Close();

        StartFenster.TragwerkVisual = new TragwerkmodellVisualisieren(StartFenster.TragwerksModell);
        StartFenster.TragwerkVisual.Show();
    }

    private void KnotenIdLostFocus(object sender, RoutedEventArgs e)
    {
        if (!_modell.Knoten.ContainsKey(KnotenId.Text))
        {
            X.Focus();
        }
        else
        {
            _modell.Knoten.TryGetValue(KnotenId.Text, out var vorhandenerKnoten);
            if (vorhandenerKnoten == null) return;
            AnzahlDof.Text = vorhandenerKnoten.AnzahlKnotenfreiheitsgrade.ToString();
            X.Text = vorhandenerKnoten.Koordinaten[0].ToString("N2", CultureInfo.CurrentCulture);
            Y.Text = vorhandenerKnoten.Koordinaten[1].ToString("N2", CultureInfo.CurrentCulture);
        }
    }

    private void BtnTabelleneintrag(object sender, RoutedEventArgs e)
    {
        var dimension = _modell.Raumdimension;
        var koordinaten = new double[dimension];
        var anzahlKnotenDof = 3;
        if (AnzahlDof.Text.Length > 0) anzahlKnotenDof = int.Parse(AnzahlDof.Text);
        if (X.Text.Length > 0) koordinaten[0] = double.Parse(X.Text);
        if (Y.Text.Length > 0) koordinaten[1] = double.Parse(Y.Text);
        var neuerKnoten = new Knoten(KnotenId.Text, koordinaten, anzahlKnotenDof, dimension);
        _knotenListe.Add(neuerKnoten);
        if (KnotenGrid != null) KnotenGrid.ItemsSource = _knotenListe;

        KnotenId.Text = string.Empty;
        X.Text = string.Empty;
        Y.Text = string.Empty;
        Z.Text = string.Empty;
        KnotenId.Focus();
    }
}