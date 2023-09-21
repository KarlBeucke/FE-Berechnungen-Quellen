using FEBibliothek.Modell;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Windows;

namespace FE_Berechnungen.Tragwerksberechnung.ModelldatenLesen;

public partial class QuerschnittNeu
{
    private readonly FeModell modell;
    private Querschnitt querschnitt, vorhandenerQuerschnitt;
    private readonly QuerschnittKeys querschnittKeys;
    public QuerschnittNeu(FeModell modell)
    {
        InitializeComponent();
        this.modell = modell;
        querschnittKeys = new QuerschnittKeys(modell);
        querschnittKeys.Show();
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
        if (modell.Querschnitt.Keys.Contains(QuerschnittId.Text))
        {
            modell.Querschnitt.TryGetValue(querschnittId, out vorhandenerQuerschnitt);
            Debug.Assert(vorhandenerQuerschnitt != null, nameof(vorhandenerQuerschnitt) + " != null");

            if (Fläche.Text == string.Empty)
            {
                _ = MessageBox.Show("mindestens Fläche muss definiert sein", "neuer Querschnitt");
                return;
            }
            vorhandenerQuerschnitt.QuerschnittsWerte[0] = double.Parse(Fläche.Text);

            if (Ixx.Text == string.Empty) { }
            else vorhandenerQuerschnitt.QuerschnittsWerte[1] = double.Parse(Ixx.Text);
        }
        // neuer Querschnitt
        else
        {
            if (Fläche.Text != string.Empty)
            {
                double ixx = 0;
                var fläche = double.Parse(Fläche.Text);
                if (Ixx.Text != string.Empty) ixx = double.Parse(Ixx.Text);
                querschnitt = new Querschnitt(fläche, ixx)
                {
                    QuerschnittId = querschnittId
                };
                modell.Querschnitt.Add(querschnittId, querschnitt);
            }
        }
        querschnittKeys?.Close();
        Close();
    }

    private void BtnDialogCancel_Click(object sender, RoutedEventArgs e)
    {
        querschnittKeys?.Close();
        Close();
    }

    private void QuerschnittIdLostFocus(object sender, RoutedEventArgs e)
    {
        if (!modell.Querschnitt.ContainsKey(QuerschnittId.Text))
        {
            Fläche.Text = "";
            Ixx.Text = "";
            return;
        }

        // vorhandene Querschnittdefinition
        modell.Querschnitt.TryGetValue(QuerschnittId.Text, out vorhandenerQuerschnitt);
        Debug.Assert(vorhandenerQuerschnitt != null, nameof(vorhandenerQuerschnitt) + " != null"); QuerschnittId.Text = "";

        QuerschnittId.Text = vorhandenerQuerschnitt.QuerschnittId;

        Fläche.Text = vorhandenerQuerschnitt.QuerschnittsWerte[0].ToString("G3", CultureInfo.CurrentCulture);
        if (Ixx.Text == "") Ixx.Text = vorhandenerQuerschnitt.QuerschnittsWerte[1].ToString("G3", CultureInfo.CurrentCulture);
    }
    private void BtnLöschen_Click(object sender, RoutedEventArgs e)
    {
        if (vorhandenerQuerschnitt != null) modell.Querschnitt.Remove(vorhandenerQuerschnitt.QuerschnittId);
        querschnittKeys.Close();
        Close();
    }
}