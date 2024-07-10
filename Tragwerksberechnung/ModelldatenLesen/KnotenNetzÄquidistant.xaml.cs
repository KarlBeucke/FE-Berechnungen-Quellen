using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using FE_Berechnungen.Tragwerksberechnung.ModelldatenAnzeigen;
using FEBibliothek.Modell;

namespace FE_Berechnungen.Tragwerksberechnung.ModelldatenLesen;

public partial class KnotenNetzÄquidistant
{
    private readonly KnotenKeys _knotenKeys;
    private readonly ObservableCollection<Knoten> _knotenListe;
    private readonly FeModell _modell;

    public KnotenNetzÄquidistant()
    {
        InitializeComponent();
    }

    public KnotenNetzÄquidistant(FeModell feModell)
    {
        InitializeComponent();
        _modell = feModell;
        Show();
        _knotenKeys = new KnotenKeys(_modell) { Owner = this };
        _knotenKeys.Show();

        Präfix.Focus();
        //_zähler = 0;
        var ndof = _modell.AnzahlKnotenfreiheitsgrade;
        AnzahlDof.Text = ndof.ToString("N0", CultureInfo.CurrentCulture);
        _knotenListe = [];
        KnotenGrid.Items.Clear();
    }

    private void BtnTabelleneintrag(object sender, RoutedEventArgs e)
    {
        var dimension = _modell.Raumdimension;
        var koordinaten = new double[dimension];
        var knotenPräfix = "";
        var anzahlKnotenDof = 3;
        double abstandX = 0, abstandY = 0;
        int wiederholungenX = 0, wiederholungenY = 0;
        if (startY.Text.Length == 0)
        {
            if (Präfix.Text.Length > 0) knotenPräfix = Präfix.Text;
            if (AnzahlDof.Text.Length > 0) anzahlKnotenDof = int.Parse(AnzahlDof.Text);

            if (startX.Text.Length > 0) koordinaten[0] = double.Parse(startX.Text);
            if (inkrementX.Text.Length > 0) abstandX = double.Parse(inkrementX.Text);
            if (anzahlX.Text.Length > 0) wiederholungenX = int.Parse(anzahlX.Text);

            for (var k = 0; k < wiederholungenX; k++)
            {
                var knotenId = knotenPräfix + k.ToString().PadLeft(2, '0');
                var knotenKoords = new[] { koordinaten[0] };
                var neuerKnoten = new Knoten(knotenId, knotenKoords, anzahlKnotenDof, dimension);
                _knotenListe.Add(neuerKnoten);
                koordinaten[0] += abstandX;
            }
        }
        else
        {
            koordinaten = new double[dimension];
            if (Präfix.Text.Length > 0) knotenPräfix = Präfix.Text;
            if (AnzahlDof.Text.Length > 0) anzahlKnotenDof = int.Parse(AnzahlDof.Text);

            if (startX.Text.Length > 0) koordinaten[0] = double.Parse(startX.Text);
            if (inkrementX.Text.Length > 0) abstandX = double.Parse(inkrementX.Text);
            if (anzahlX.Text.Length > 0) wiederholungenX = int.Parse(anzahlX.Text);

            if (startY.Text.Length > 0) koordinaten[1] = double.Parse(startY.Text);
            if (inkrementY.Text.Length > 0) abstandY = double.Parse(inkrementY.Text);
            if (anzahlY.Text.Length > 0) wiederholungenY = int.Parse(anzahlY.Text);

            for (var k = 0; k < wiederholungenX; k++)
            {
                var temp = koordinaten[0];
                var idY = k.ToString().PadLeft(2, '0');
                for (var l = 0; l < wiederholungenY; l++)
                {
                    var idX = l.ToString().PadLeft(2, '0');
                    var knotenId = knotenPräfix + idX + idY;
                    var knotenKoords = new[] { koordinaten[0], koordinaten[1] };
                    var neuerKnoten = new Knoten(knotenId, knotenKoords, anzahlKnotenDof, dimension);
                    _knotenListe.Add(neuerKnoten);
                    koordinaten[0] += abstandX;
                }

                koordinaten[1] += abstandY;
                koordinaten[0] = temp;
            }
            //_zähler++;
        }

        if (KnotenGrid != null) KnotenGrid.ItemsSource = _knotenListe;
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
                if (startX.Text.Length > 0) vorhandenerKnoten.Koordinaten[0] = double.Parse(startX.Text);
                if (startY.Text.Length > 0) vorhandenerKnoten.Koordinaten[1] = double.Parse(startY.Text);
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