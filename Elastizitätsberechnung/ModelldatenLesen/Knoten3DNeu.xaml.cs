using FE_Berechnungen.Elastizitätsberechnung.ModelldatenAnzeigen;
using System.Collections.ObjectModel;
using System.Globalization;

namespace FE_Berechnungen.Elastizitätsberechnung.ModelldatenLesen
{
    public partial class Knoten3DNeu
    {
        private readonly ObservableCollection<Knoten> _knotenListe;
        private readonly FeModell _modell;
        private KnotenKeys _knotenKeys;

        public Knoten3DNeu(FeModell feModell)
        {
            InitializeComponent();
            _modell = feModell;
            Show();

            var ndof = _modell.AnzahlKnotenfreiheitsgrade;
            AnzahlDof.Text = ndof.ToString("N0", CultureInfo.CurrentCulture);
            _knotenListe = [];
            KnotenGrid.Items.Clear();
        }

        private void KnotenIdGotFocus(object sender, RoutedEventArgs e)
        {
            _knotenKeys = new KnotenKeys(_modell) { Topmost = true, Owner = (Window)Parent };
            _knotenKeys.Show();
            KnotenId.Focus();
        }

        private void KnotenIdLostFocus(object sender, RoutedEventArgs e)
        {
            if (!_modell.Knoten.TryGetValue(KnotenId.Text, out _))
            {
                _knotenKeys.Close();
                return;
            }

            _ = MessageBox.Show("neue Knoten ID " + KnotenId.Text +
                                " nicht eindeutig, schon vorhanden im Modell");
            _knotenKeys.Close();
            KnotenId.Text = string.Empty;
        }

        private void BtnDialogCancel_Click(object sender, RoutedEventArgs e)
        {
            _knotenKeys?.Close();
            Close();
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

            Close();
            _knotenKeys?.Close();

            StartFenster.ElastizitätVisual3D = new Elastizitätsmodell3DVisualisieren(_modell);
            StartFenster.ElastizitätVisual3D.Show();
            _modell.Berechnet = false;
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
            //KnotenId.Focus();
        }

        private void BtnLöschen_Click(object sender, RoutedEventArgs e)
        {
            if (KnotenReferenziert()) return;
            _modell.Knoten.Remove(KnotenId.Text);
            Close();
            StartFenster.ElastizitätVisual3D.Close();
            StartFenster.ElastizitätVisual3D = new Elastizitätsmodell3DVisualisieren(_modell);
            StartFenster.ElastizitätVisual3D.Show();
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
}