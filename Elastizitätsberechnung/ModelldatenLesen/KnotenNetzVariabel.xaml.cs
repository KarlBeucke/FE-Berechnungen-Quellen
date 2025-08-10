using FE_Berechnungen.Elastizitätsberechnung.ModelldatenAnzeigen;
using System.Collections.ObjectModel;
using System.Globalization;

namespace FE_Berechnungen.Elastizitätsberechnung.ModelldatenLesen
{
    public partial class KnotenNetzVariabel
    {
        private readonly KnotenKeys _knotenKeys;
        private readonly ObservableCollection<Knoten> _knotenListe;
        private readonly FeModell _modell;

        public KnotenNetzVariabel()
        {
            InitializeComponent();
        }

        public KnotenNetzVariabel(FeModell feModell)
        {
            InitializeComponent();
            _modell = feModell;
            Show();
            _knotenKeys = new KnotenKeys(_modell) { Owner = this };
            _knotenKeys.Show();

            Präfix.Focus();
            var ndof = _modell.AnzahlKnotenfreiheitsgrade;
            AnzahlDof.Text = ndof.ToString("N0", CultureInfo.CurrentCulture);
            _knotenListe = [];
            KnotenGrid.Items.Clear();
        }

        private void BtnTabelleneintrag(object sender, RoutedEventArgs e)
        {
            var dimension = _modell.Raumdimension;
            double startx = 0, starty = 0;
            var knotenPräfix = "";
            var anzahlKnotenDof = 3;
            char[] delimiters = [';'];

            if (Präfix.Text.Length > 0) knotenPräfix = Präfix.Text;
            if (AnzahlDof.Text.Length > 0) anzahlKnotenDof = int.Parse(AnzahlDof.Text);

            switch (InkrementsX.Text.Length)
            {
                case > 0 when InkrementsY.Text.Length == 0:
                    {
                        var knotenId = knotenPräfix + "00";
                        try
                        {
                            if (StartX.Text.Length > 0) startx = double.Parse(StartX.Text);
                            if (StartY.Text.Length > 0) starty = double.Parse(StartY.Text);
                        }
                        catch (FormatException)
                        {
                            _ = MessageBox.Show("ungültiges  Eingabeformat", "neues Knotennetz");
                        }

                        var koordinaten = new[] { startx, starty };
                        var neuerKnoten = new Knoten(knotenId, koordinaten, anzahlKnotenDof, dimension);
                        _knotenListe.Add(neuerKnoten);

                        var substrings = InkrementsX.Text.Split(delimiters);
                        var abstände = new double[substrings.Length];

                        for (var k = 0; k < abstände.Length; k++)
                        {
                            knotenId = knotenPräfix + (k + 1).ToString().PadLeft(2, '0');
                            abstände[k] = double.Parse(substrings[k]);
                            var x = koordinaten[0] + abstände[k];
                            var y = koordinaten[1];
                            koordinaten = [x, y];
                            neuerKnoten = new Knoten(knotenId, koordinaten, anzahlKnotenDof, dimension);
                            _knotenListe.Add(neuerKnoten);
                        }

                        break;
                    }
                case > 0 when InkrementsY.Text.Length > 0:
                    {
                        // Startknoten
                        var idY = "00";
                        var knotenId = knotenPräfix + "0000";
                        try
                        {
                            if (StartX.Text.Length > 0) startx = double.Parse(StartX.Text);
                            if (StartY.Text.Length > 0) starty = double.Parse(StartY.Text);
                        }
                        catch (FormatException)
                        {
                            _ = MessageBox.Show("ungültiges  Eingabeformat", "neues Knotennetz");
                        }

                        var koordinaten = new[] { startx, starty };
                        var neuerKnoten = new Knoten(knotenId, koordinaten, anzahlKnotenDof, dimension);
                        _knotenListe.Add(neuerKnoten);

                        // 1. Reihe in x-Richtung
                        var substringsY = InkrementsY.Text.Split(delimiters);
                        var abständeY = new double[substringsY.Length];
                        var substringsX = InkrementsX.Text.Split(delimiters);
                        var abständeX = new double[substringsX.Length];
                        for (var m = 0; m < abständeX.Length; m++)
                        {
                            var idX = (m + 1).ToString().PadLeft(2, '0');
                            abständeX[m] = double.Parse(substringsX[m]);
                            var x = koordinaten[0] + abständeX[m];
                            var y = koordinaten[1];
                            koordinaten = [x, y];
                            knotenId = knotenPräfix + idX + idY;
                            neuerKnoten = new Knoten(knotenId, koordinaten, anzahlKnotenDof, dimension);
                            _knotenListe.Add(neuerKnoten);
                        }

                        for (var n = 0; n < abständeY.Length; n++)
                        {
                            // 1. Knoten nächste Reihe
                            var idX = "00";
                            idY = (n + 1).ToString().PadLeft(2, '0');
                            knotenId = knotenPräfix + idX + idY;
                            abständeY[n] = double.Parse(substringsY[n]);
                            var x = startx;
                            var y = koordinaten[1] + abständeY[n];
                            koordinaten = [x, y];
                            neuerKnoten = new Knoten(knotenId, koordinaten, anzahlKnotenDof, dimension);
                            _knotenListe.Add(neuerKnoten);

                            // restliche Knoten in Reihe
                            idY = (n + 1).ToString().PadLeft(2, '0');
                            koordinaten[0] = startx;
                            for (var m = 0; m < abständeX.Length; m++)
                            {
                                idX = (m + 1).ToString().PadLeft(2, '0');
                                knotenId = knotenPräfix + idX + idY;
                                abständeX[m] = double.Parse(substringsX[m]);
                                x = koordinaten[0] + abständeX[m];
                                y = koordinaten[1];
                                koordinaten = [x, y];
                                neuerKnoten = new Knoten(knotenId, koordinaten, anzahlKnotenDof, dimension);
                                _knotenListe.Add(neuerKnoten);
                            }
                        }

                        break;
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

            StartFenster.ElastizitätVisual.Close();
            Close();
            _knotenKeys.Close();

            StartFenster.ElastizitätVisual = new ElastizitätsmodellVisualisieren(_modell);
            StartFenster.ElastizitätVisual.Show();
        }

        private void BtnDialogCancel_Click(object sender, RoutedEventArgs e)
        {
            _knotenKeys.Close();
            _knotenListe.Clear();
        }
    }
}