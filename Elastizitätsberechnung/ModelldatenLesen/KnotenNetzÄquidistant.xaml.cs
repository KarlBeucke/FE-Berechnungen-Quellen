using FE_Berechnungen.Elastizitätsberechnung.ModelldatenAnzeigen;
using System.Collections.ObjectModel;
using System.Globalization;

namespace FE_Berechnungen.Elastizitätsberechnung.ModelldatenLesen
{
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
            if (StartY.Text.Length == 0)
            {
                try
                {
                    if (Präfix.Text.Length > 0) knotenPräfix = Präfix.Text;
                    if (AnzahlDof.Text.Length > 0) anzahlKnotenDof = int.Parse(AnzahlDof.Text);

                    if (StartX.Text.Length > 0) koordinaten[0] = double.Parse(StartX.Text);
                    if (InkrementX.Text.Length > 0) abstandX = double.Parse(InkrementX.Text);
                    if (AnzahlX.Text.Length > 0) wiederholungenX = int.Parse(AnzahlX.Text);
                }
                catch (FormatException)
                {
                    _ = MessageBox.Show("ungültiges  Eingabeformat", "neues Knotennetz");
                }

                for (var k = 0; k < wiederholungenX; k++)
                {
                    var knotenId = knotenPräfix + k.ToString().PadLeft(2, '0');
                    var knotenKoords = new[] { koordinaten[0], 0 };
                    var neuerKnoten = new Knoten(knotenId, knotenKoords, anzahlKnotenDof, dimension);
                    _knotenListe.Add(neuerKnoten);
                    koordinaten[0] += abstandX;
                }
            }
            else
            {
                koordinaten = new double[dimension];
                if (Präfix.Text.Length > 0) knotenPräfix = Präfix.Text;
                try
                {
                    if (AnzahlDof.Text.Length > 0) anzahlKnotenDof = int.Parse(AnzahlDof.Text);

                    if (StartX.Text.Length > 0) koordinaten[0] = double.Parse(StartX.Text);
                    if (InkrementX.Text.Length > 0) abstandX = double.Parse(InkrementX.Text);
                    if (AnzahlX.Text.Length > 0) wiederholungenX = int.Parse(AnzahlX.Text);

                    if (StartY.Text.Length > 0) koordinaten[1] = double.Parse(StartY.Text);
                    if (InkrementY.Text.Length > 0) abstandY = double.Parse(InkrementY.Text);
                    if (AnzahlY.Text.Length > 0) wiederholungenY = int.Parse(AnzahlY.Text);
                }
                catch (FormatException)
                {
                    _ = MessageBox.Show("ungültiges  Eingabeformat", "neues Knotennetz");
                }

                for (var k = 0; k < wiederholungenY; k++)
                {
                    var temp = koordinaten[0];
                    var idY = k.ToString().PadLeft(2, '0');
                    for (var l = 0; l < wiederholungenX; l++)
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
            }
            if (KnotenGrid != null) KnotenGrid.ItemsSource = _knotenListe;
        }

        private void BtnDialogOk_Click(object sender, RoutedEventArgs e)
        {
            // füg neueKnoten hinzu, ändere vorhandener Knoten
            foreach (var knoten in _knotenListe)
            {
                // neuer Knoten
                if (_modell.Knoten.TryAdd(knoten.Id, knoten)) continue;
                // vorhandener Knoten
                _modell.Knoten.TryGetValue(knoten.Id, out var vorhandenerKnoten);
                if (vorhandenerKnoten == null) continue;
                try
                {
                    if (AnzahlDof.Text.Length > 0)
                        vorhandenerKnoten.AnzahlKnotenfreiheitsgrade = int.Parse(AnzahlDof.Text);
                    if (StartX.Text.Length > 0) vorhandenerKnoten.Koordinaten[0] = double.Parse(StartX.Text);
                    if (StartY.Text.Length > 0) vorhandenerKnoten.Koordinaten[1] = double.Parse(StartY.Text);
                }
                catch (FormatException)
                {
                    _ = MessageBox.Show("ungültiges  Eingabeformat", "neues Knotennetz");
                }
            }

            StartFenster.ElastizitätVisual.Close();
            Close();
            _knotenKeys.Close();

            StartFenster.ElastizitätVisual = new ElastizitätsmodellVisualisieren(_modell);
            StartFenster.ElastizitätVisual.Show();
        }

        private void BtnDialogCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
            _knotenKeys.Close();
        }
    }
}