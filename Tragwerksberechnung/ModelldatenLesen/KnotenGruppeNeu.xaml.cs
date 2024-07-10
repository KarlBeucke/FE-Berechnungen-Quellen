using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using FE_Berechnungen.Tragwerksberechnung.ModelldatenAnzeigen;
using FEBibliothek.Modell;

namespace FE_Berechnungen.Tragwerksberechnung.ModelldatenLesen;

public partial class KnotenGruppeNeu
{
    private readonly FeModell _modell;
    private readonly KnotenKeys _knotenKeys;
    private readonly ObservableCollection<Knoten> _knotenListe;
    private readonly int _ndof;
    private int _zähler;

    public KnotenGruppeNeu()
    {
        InitializeComponent();
    }

    public KnotenGruppeNeu(FeModell feModell)
    {
        InitializeComponent();
        _modell = feModell;
        Show();
        _knotenKeys = new KnotenKeys(_modell) { Owner = this };
        _knotenKeys.Show();

        Präfix.Focus();
        _zähler = 0;
        _ndof = _modell.AnzahlKnotenfreiheitsgrade;
        AnzahlDof.Text = _ndof.ToString("N0", CultureInfo.CurrentCulture);
        _knotenListe = [];
        KnotenGrid.Items.Clear();
    }

    private void BtnTabelleneintrag(object sender, RoutedEventArgs e)
    {
        var dimension = _modell.Raumdimension;
        var koordinaten = new double[dimension];
        var anzahlKnotenDof = 3;
        if (AnzahlDof.Text.Length > 0) anzahlKnotenDof = int.Parse(AnzahlDof.Text);
        if (X.Text.Length > 0) koordinaten[0] = double.Parse(X.Text);
        if (Y.Text.Length > 0) koordinaten[1] = double.Parse(Y.Text);
        var knotenId = Präfix.Text + _zähler.ToString().PadLeft(2 * koordinaten.Length, '0');
        var neuerKnoten = new Knoten(knotenId, koordinaten, anzahlKnotenDof, dimension);
        _knotenListe.Add(neuerKnoten);
        if (KnotenGrid != null) KnotenGrid.ItemsSource = _knotenListe;

        X.Text = string.Empty;
        Y.Text = string.Empty;
        Z.Text = string.Empty;
        _zähler++;
        X.Focus();
    }

    private void KnotenIdLostFocus(object sender, RoutedEventArgs e)
    {
        X.Focus();
        //Close();
        //_knotenKeys.Close();
    }

    private void BtnDialogOk_Click(object sender, RoutedEventArgs e)
    {
        foreach (var knoten in _knotenListe)
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

        StartFenster.TragwerkVisual.Close();
        Close();
        _knotenKeys.Close();

        StartFenster.TragwerkVisual = new TragwerkmodellVisualisieren(StartFenster.TragwerksModell);
        StartFenster.TragwerkVisual.Show();
    }

    private void BtnDialogCancel_Click(object sender, RoutedEventArgs e)
    {
        Close();
        _knotenKeys.Close();
    }
}