using FE_Berechnungen.Elastizitätsberechnung.ModelldatenAnzeigen;
using System.Collections.ObjectModel;
using System.Globalization;

namespace FE_Berechnungen.Elastizitätsberechnung.ModelldatenLesen
{
    public partial class Knoten3DNetzÄquidistant
    {
        private string _knotenId;
        private double[] _knotenKoords;
        private readonly KnotenKeys _knotenKeys;
        private readonly ObservableCollection<Knoten> _knotenListe;
        private readonly FeModell _modell;

        public Knoten3DNetzÄquidistant()
        {
            InitializeComponent();
        }

        public Knoten3DNetzÄquidistant(FeModell feModell)
        {
            InitializeComponent();
            _modell = feModell;
            Show();
            _knotenKeys = new KnotenKeys(_modell) { Owner = this };
            _knotenKeys.Show();

            Präfix.Focus();
            _knotenListe = [];
            KnotenGrid.Items.Clear();
        }

        private void BtnTabelleneintrag(object sender, RoutedEventArgs e)
        {
            var koords = new double[3];
            var dim = _modell.Raumdimension;
            var ndof = _modell.AnzahlKnotenfreiheitsgrade;
            var knotenPräfix = "";
            double abstandX = 0, abstandY = 0, abstandZ = 0;
            int wiederholungenX = 0, wiederholungenY = 0, wiederholungenZ = 0;
            
            if (Präfix.Text.Length > 0) knotenPräfix = Präfix.Text;
            try
            {
                if (StartX.Text.Length > 0) koords[0] = double.Parse(StartX.Text);
                if (InkrementX.Text.Length > 0) abstandX = double.Parse(InkrementX.Text);
                if (AnzahlX.Text.Length > 0) wiederholungenX = int.Parse(AnzahlX.Text);

                if (StartY.Text.Length > 0) koords[1] = double.Parse(StartY.Text);
                if (InkrementY.Text.Length > 0) abstandY = double.Parse(InkrementY.Text);
                if (AnzahlY.Text.Length > 0) wiederholungenY = int.Parse(AnzahlY.Text);

                if (StartZ.Text.Length > 0) koords[2] = double.Parse(StartZ.Text);
                if (InkrementZ.Text.Length > 0) abstandZ = double.Parse(InkrementZ.Text);
                if (AnzahlZ.Text.Length > 0) wiederholungenZ = int.Parse(AnzahlZ.Text);
            }
            catch (FormatException)
            {
                _ = MessageBox.Show("ungültiges  Eingabeformat", "neues Knotennetz");
            }

            for (var k = 0; k < wiederholungenZ; k++)
            {
                var temp1 = koords[1];
                var idZ = k.ToString().PadLeft(2, '0');
                for (var l = 0; l < wiederholungenY; l++)
                {
                    var temp0 = koords[0];
                    var idY = l.ToString().PadLeft(2, '0');
                    for (var m = 0; m < wiederholungenX; m++)
                    {
                        var idX = m.ToString().PadLeft(2, '0');
                        _knotenId = knotenPräfix + idX + idY + idZ;
                        _knotenKoords = [koords[0], koords[1], koords[2]];
                        var node = new Knoten(_knotenId, _knotenKoords, ndof, dim);
                        _knotenListe.Add(node);
                        koords[0] += abstandX;
                    }
                    koords[0] = temp0;
                    koords[1] += abstandY;
                }
                koords[1] = temp1;
                koords[2] += abstandZ;
            }
            if (KnotenGrid != null) KnotenGrid.ItemsSource = _knotenListe;
        }

        private void BtnDialogOk_Click(object sender, RoutedEventArgs e)
        {
            // vorhandener Knoten
            foreach (var knoten in _knotenListe)
            {
                if (_modell.Knoten.TryAdd(knoten.Id, knoten)) continue;
                _modell.Knoten.TryGetValue(knoten.Id, out var vorhandenerKnoten);
                if (vorhandenerKnoten == null) continue;
                try
                {
                    if (StartX.Text.Length > 0) vorhandenerKnoten.Koordinaten[0] = double.Parse(StartX.Text);
                    if (StartY.Text.Length > 0) vorhandenerKnoten.Koordinaten[1] = double.Parse(StartY.Text);
                    if (StartZ.Text.Length > 0) vorhandenerKnoten.Koordinaten[2] = double.Parse(StartZ.Text);
                }
                catch (FormatException)
                {
                    _ = MessageBox.Show("ungültiges  Eingabeformat", "neues Knotennetz");
                }
            }

            StartFenster.ElastizitätVisual3D.Close();
            Close();
            _knotenKeys.Close();

            StartFenster.ElastizitätVisual3D = new Elastizitätsmodell3DVisualisieren(_modell);
            StartFenster.ElastizitätVisual3D.Show();
        }

        private void BtnDialogCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
            _knotenKeys.Close();
        }
    }
}