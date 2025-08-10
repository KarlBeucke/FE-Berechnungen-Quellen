using FE_Berechnungen.Elastizitätsberechnung.ModelldatenAnzeigen;
using System.Collections.ObjectModel;

namespace FE_Berechnungen.Elastizitätsberechnung.ModelldatenLesen
{
    public partial class Knoten3DNetzVariabel
    {
        private readonly KnotenKeys _knotenKeys;
        private readonly ObservableCollection<Knoten> _knotenListe;
        private readonly FeModell _modell;

        public Knoten3DNetzVariabel()
        {
            InitializeComponent();
        }

        public Knoten3DNetzVariabel(FeModell feModell)
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
            var dimension = _modell.Raumdimension;
            var ndof = _modell.AnzahlKnotenfreiheitsgrade;
            double startx = 0, starty = 0, startz = 0;
            var knotenPräfix = "";
            var inkrements = "";
            char[] delimiters = [';'];

            if (Präfix.Text.Length > 0) knotenPräfix = Präfix.Text;

            if (StartX.Text.Length > 0) startx = double.Parse(StartX.Text);
            if (StartY.Text.Length > 0) starty = double.Parse(StartY.Text);
            if (StartZ.Text.Length > 0) startz = double.Parse(StartZ.Text);
            if (Inkrements.Text.Length > 0) inkrements = Inkrements.Text;

            var abständeText = inkrements.Split(delimiters);
            var abstände = new double[abständeText.Length];
            for (var i = 0; i < abständeText.Length; i++)
                abstände[i] = double.Parse(abständeText[i]);

            for (var n = 0; n < abstände.Length; n++)
            {
                var idZ = n.ToString().PadLeft(2, '0');
                var inkrement2 = startz + abstände[n];
                for (var m = 0; m < abstände.Length; m++)
                {
                    var idY = m.ToString().PadLeft(2, '0');
                    var inkrement1 = starty + abstände[m];
                    for (var k = 0; k < abstände.Length; k++)
                    {
                        var koords = new double[3];
                        koords[1] = inkrement1;
                        koords[2] = inkrement2;
                        var idX = k.ToString().PadLeft(2, '0');
                        koords[0] = startx + abstände[k];
                        var knotenId = knotenPräfix + idX + idY + idZ;
                        double[] knotenKoords = [koords[0], koords[1], koords[2]];
                        var node = new Knoten(knotenId, knotenKoords, ndof, dimension);
                        _knotenListe.Add(node);
                    }
                }
            }

            if (KnotenGrid != null) KnotenGrid.ItemsSource = _knotenListe;
        }

        private void BtnDialogOk_Click(object sender, RoutedEventArgs e)
        {
            foreach (var knoten in _knotenListe)
            {
                // neuer Knoten
                if (_modell.Knoten.TryAdd(knoten.Id, knoten)) continue;
                // vorhandener Knoten
                _ = MessageBox.Show("Knoten " + knoten.Id + " nicht hinzugefügt, da schon vorhanden", "neues Knotennetz");
            }

            StartFenster.ElastizitätVisual3D.Close();
            Close();
            _knotenKeys.Close();

            StartFenster.ElastizitätVisual3D = new Elastizitätsmodell3DVisualisieren(_modell);
            StartFenster.ElastizitätVisual3D.Show();
        }

        private void BtnDialogCancel_Click(object sender, RoutedEventArgs e)
        {
            _knotenKeys.Close();
            _knotenListe.Clear();
        }
    }
}
