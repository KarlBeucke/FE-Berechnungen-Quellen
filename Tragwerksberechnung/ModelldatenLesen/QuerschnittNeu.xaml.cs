using System.Globalization;

namespace FE_Berechnungen.Tragwerksberechnung.ModelldatenLesen;

public partial class QuerschnittNeu
{
    private readonly FeModell _modell;
    private Querschnitt _querschnitt, _vorhandenerQuerschnitt;

    public QuerschnittNeu(FeModell modell)
    {
        InitializeComponent();
        _modell = modell;
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
        if (_modell.Querschnitt.Keys.Contains(QuerschnittId.Text))
        {
            _modell.Querschnitt.TryGetValue(querschnittId, out _vorhandenerQuerschnitt);
            if (_vorhandenerQuerschnitt != null)
            {
                if (Fläche.Text == string.Empty)
                {
                    _ = MessageBox.Show("mindestens Fläche muss definiert sein", "neuer Querschnitt");
                    return;
                }

                try
                {
                    _vorhandenerQuerschnitt.QuerschnittsWerte[0] = double.Parse(Fläche.Text);
                }
                catch (FormatException)
                {
                    _ = MessageBox.Show("ungültiges  Eingabeformat", "neuer Querschnitt");
                }

                if (Ixx.Text != string.Empty)
                {
                    try
                    {
                        _vorhandenerQuerschnitt.QuerschnittsWerte[1] = double.Parse(Ixx.Text);
                    }
                    catch (FormatException)
                    {
                        _ = MessageBox.Show("ungültiges  Eingabeformat", "neuer Querschnitt");
                    }
                }
            }
        }
        // neuer Querschnitt
        else
        {
            if (Fläche.Text != string.Empty)
            {
                double ixx = 0;
                var fläche = double.Parse(Fläche.Text);
                if (Ixx.Text != string.Empty) ixx = double.Parse(Ixx.Text);
                _querschnitt = new Querschnitt(fläche, ixx)
                {
                    QuerschnittId = querschnittId
                };
                _modell.Querschnitt.Add(querschnittId, _querschnitt);
            }
        }

        Close();
        StartFenster.TragwerkVisual.QuerschnittKeys?.Close();
    }

    private void BtnDialogCancel_Click(object sender, RoutedEventArgs e)
    {
        StartFenster.TragwerkVisual.QuerschnittKeys?.Close();
        Close();
    }

    private void QuerschnittIdLostFocus(object sender, RoutedEventArgs e)
    {
        if (!_modell.Querschnitt.ContainsKey(QuerschnittId.Text))
        {
            Fläche.Text = "";
            Ixx.Text = "";
            return;
        }

        // vorhandene Querschnittdefinition
        if (!_modell.Querschnitt.TryGetValue(QuerschnittId.Text, out _vorhandenerQuerschnitt)) return;
        QuerschnittId.Text = "";

        QuerschnittId.Text = _vorhandenerQuerschnitt.QuerschnittId;

        Fläche.Text = _vorhandenerQuerschnitt.QuerschnittsWerte[0].ToString("G3", CultureInfo.CurrentCulture);
        if (Ixx.Text == "")
            Ixx.Text = _vorhandenerQuerschnitt.QuerschnittsWerte[1].ToString("G3", CultureInfo.CurrentCulture);
    }

    private void BtnLöschen_Click(object sender, RoutedEventArgs e)
    {
        if (QuerschnittReferenziert()) return;

        _modell.Querschnitt.Remove(_vorhandenerQuerschnitt.QuerschnittId);
        StartFenster.TragwerkVisual.QuerschnittKeys?.Close();
        Close();
    }

    private bool QuerschnittReferenziert()
    {
        var id = QuerschnittId.Text;
        foreach (var element in _modell.Elemente.Where(element => element.Value.ElementQuerschnittId == id))
        {
            _ = MessageBox.Show(
                "Querschnitt referenziert durch Element " + element.Value.ElementId + ", kann nicht gelöscht werden",
                "neuer Querschnitt");
            return true;
        }

        //if (_modell.Elemente.All(element => element.Value.ElementQuerschnittId != id)) return false;
        return false;
    }
}