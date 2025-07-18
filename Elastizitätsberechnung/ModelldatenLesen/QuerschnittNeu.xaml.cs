using System.Globalization;
using FE_Berechnungen.Elastizitätsberechnung.ModelldatenAnzeigen;

namespace FE_Berechnungen.Elastizitätsberechnung.ModelldatenLesen
{
    public partial class QuerschnittNeu
    {
        private readonly FeModell _modell;
        public string AktuelleId;


        public QuerschnittNeu(FeModell modell)
        {
            InitializeComponent();
            _modell = modell;
            AktuelleId = "";
            Show();
        }

        private void BtnDialogOk_Click(object sender, RoutedEventArgs e)
        {
            var querschnittId = QuerschnittId.Text;
            if (querschnittId == "")
            {
                _ = MessageBox.Show("Querschnitt Id muss definiert sein", "neuer Querschnitt");
                return;
            }

            // vorhandener Querschnitt
            if (_modell.Querschnitt.TryGetValue(querschnittId, out var vorhandenerQuerschnitt))
            {
                try
                {
                    if (Dicke.Text.Length > 0) vorhandenerQuerschnitt.QuerschnittsWerte[0] = double.Parse(Dicke.Text);
 }
                catch (FormatException)
                {
                    _ = MessageBox.Show("ungültiges  Eingabeformat", "neuer Querschnitt");
                }
            }
            // neuer Querschnitt
            else
            {
                if (Dicke.Text != string.Empty)
                {
                    double fläche = 0, ixx = 0;
                    try
                    {
                        fläche = double.Parse(Dicke.Text);
                    }
                    catch (FormatException)
                    {
                        _ = MessageBox.Show("ungültiges  Eingabeformat", "neuer Querschnitt");
                    }

                    var querschnitt = new Querschnitt(fläche, ixx)
                    {
                        QuerschnittId = querschnittId
                    };
                    _modell.Querschnitt.Add(querschnittId, querschnitt);
                }
                else
                {
                    _ = MessageBox.Show("Fläche muss definiert sein", "neuer Querschnitt");
                    return;
                }
            }

            if (AktuelleId != QuerschnittId.Text) _modell.Material.Remove(AktuelleId);

            Close();
            StartFenster.ElastizitätVisual.Close();
            StartFenster.ElastizitätVisual = new ElastizitätsmodellVisualisieren(_modell);
            StartFenster.ElastizitätVisual.Show();
            _modell.Berechnet = false;
        }

        private void BtnDialogCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void QuerschnittIdLostFocus(object sender, RoutedEventArgs e)
        {
            if (!_modell.Querschnitt.TryGetValue(QuerschnittId.Text, out var vorhandenerQuerschnitt)) return;

            // vorhandene Querschnittdefinition
            Dicke.Text = "";
            QuerschnittId.Text = vorhandenerQuerschnitt.QuerschnittId;
            Dicke.Text = vorhandenerQuerschnitt.QuerschnittsWerte[0].ToString("G3", CultureInfo.CurrentCulture);
        }

        private void BtnLöschen_Click(object sender, RoutedEventArgs e)
        {
            if (QuerschnittReferenziert() || !_modell.Querschnitt.Remove(QuerschnittId.Text))
            {
                Close();
                return;
            }

            Close();
            _modell.Berechnet = false;
        }

        private bool QuerschnittReferenziert()
        {
            var id = QuerschnittId.Text;
            foreach (var element in _modell.Elemente.Where(element => element.Value.ElementQuerschnittId == id))
            {
                _ = MessageBox.Show(
                    "Querschnitt referenziert durch Element " + element.Value.ElementId +
                    ", kann nicht gelöscht werden",
                    "neuer Querschnitt");
                return true;
            }

            //if (_modell.Elemente.All(element => element.Value.ElementQuerschnittId != id)) return false;
            return false;
        }
    }
}