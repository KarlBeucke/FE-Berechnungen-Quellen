using FE_Berechnungen.Wärmeberechnung.Modelldaten;
using System.Globalization;
using System.Text;

namespace FE_Berechnungen.Wärmeberechnung.ModelldatenLesen;

public partial class ZeitKnotentemperaturNeu
{
    private readonly FeModell _modell;
    private string lastId;
    private AbstraktZeitabhängigeKnotenlast vorhandeneKnotenlast;

    public ZeitKnotentemperaturNeu(FeModell modell)
    {
        InitializeComponent();
        _modell = modell;
        Show();
    }

    private void BtnDialogOk_Click(object sender, RoutedEventArgs e)
    {
        lastId = LastId.Text;
        if (lastId == "")
        {
            _ = MessageBox.Show("zeitabhängige Knotenlast Id muss definiert sein", "neue zeitabhängige Knotenlast");
            return;
        }

        // vorhandene Knotenlast
        if (_modell.ZeitabhängigeKnotenLasten.TryGetValue(lastId, out vorhandeneKnotenlast))
        {
            if (KnotenId.Text.Length > 0)
                vorhandeneKnotenlast.KnotenId = KnotenId.Text.ToString(CultureInfo.CurrentCulture);

            try
            {
                if (Datei.IsChecked == true)
                {
                    vorhandeneKnotenlast.VariationsTyp = 0;
                }
                else if (Konstant.Text.Length > 0)
                {
                    vorhandeneKnotenlast.VariationsTyp = 1;
                    vorhandeneKnotenlast.KonstanteTemperatur = double.Parse(Konstant.Text);
                }
                else if (Amplitude.Text.Length > 0 && Frequenz.Text.Length > 0 && Winkel.Text.Length > 0)
                {
                    vorhandeneKnotenlast.VariationsTyp = 2;
                    vorhandeneKnotenlast.Amplitude = double.Parse(Amplitude.Text);
                    vorhandeneKnotenlast.Frequenz = double.Parse(Frequenz.Text);
                    vorhandeneKnotenlast.PhasenWinkel = double.Parse(Winkel.Text);
                }
                else if (Linear.Text.Length > 0)
                {
                    vorhandeneKnotenlast.VariationsTyp = 3;
                    var delimiters = new[] { '\t' };
                    var teilStrings = Linear.Text.Split(delimiters);
                    var k = 0;
                    char[] paarDelimiter = { ';' };
                    var intervall = new double[2 * teilStrings.Length];
                    for (var i = 0; i < intervall.Length; i += 2)
                    {
                        var wertePaar = teilStrings[k].Split(paarDelimiter);
                        intervall[i] = double.Parse(wertePaar[0]);
                        intervall[i + 1] = double.Parse(wertePaar[1]);
                        k++;
                    }

                    vorhandeneKnotenlast.Intervall = intervall;
                }
            }
            catch (FormatException)
            {
                _ = MessageBox.Show("ungültiges  Eingabeformat", "neue Knotentemperaturen");
            }
        }

        // neue Knotenlast
        else
        {
            var knotenId = "";
            ZeitabhängigeKnotenLast knotenlast = null;
            if (KnotenId.Text.Length > 0) knotenId = KnotenId.Text.ToString(CultureInfo.CurrentCulture);
            if (Datei.IsChecked == true)
            {
                knotenlast = new ZeitabhängigeKnotenLast(knotenId, true)
                {
                    LastId = lastId,
                    VariationsTyp = 0
                };
            }
            else if (Konstant.Text.Length > 0)
            {
                knotenlast = new ZeitabhängigeKnotenLast(knotenId, double.Parse(Konstant.Text))
                {
                    LastId = lastId,
                    VariationsTyp = 1
                };
            }
            else if (Amplitude.Text.Length > 0 && Frequenz.Text.Length > 0 && Winkel.Text.Length > 0)
            {
                var amplitude = double.Parse(Amplitude.Text);
                var frequenz = 2 * Math.PI * double.Parse(Frequenz.Text);
                var winkel = Math.PI / 180 * double.Parse(Winkel.Text);
                knotenlast = new ZeitabhängigeKnotenLast(knotenId, amplitude, frequenz, winkel)
                {
                    LastId = lastId,
                    VariationsTyp = 2
                };
            }
            else if (Linear.Text.Length > 0)
            {
                var delimiters = new[] { '\t' };
                var teilStrings = Linear.Text.Split(delimiters);
                var k = 0;
                char[] paarDelimiter = { ';' };
                var intervall = new double[2 * teilStrings.Length];
                for (var i = 0; i < intervall.Length; i += 2)
                {
                    var wertePaar = teilStrings[k].Split(paarDelimiter);
                    intervall[i] = double.Parse(wertePaar[0]);
                    intervall[i + 1] = double.Parse(wertePaar[1]);
                    k++;
                }

                knotenlast = new ZeitabhängigeKnotenLast(knotenId, intervall) { LastId = lastId, VariationsTyp = 3 };
            }

            _modell.ZeitabhängigeKnotenLasten.Add(lastId, knotenlast);
        }

        Close();
        StartFenster.WärmeVisual.Close();
    }

    private void BtnDialogCancel_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void BtnLöschen_Click(object sender, RoutedEventArgs e)
    {
        if (!_modell.ZeitabhängigeKnotenLasten.ContainsKey(LastId.Text)) return;
        _modell.ZeitabhängigeKnotenLasten.Remove(LastId.Text);
        Close();
        StartFenster.WärmeVisual.Close();

        if (!_modell.ZeitabhängigeKnotenLasten.Keys.Contains(LastId.Text)) return;
        _modell.ZeitabhängigeKnotenLasten.Remove(LastId.Text);
        Close();
        StartFenster.WärmeVisual.Close();
    }

    private void LastIdLostFocus(object sender, RoutedEventArgs e)
    {
        // neue zeitabhängige Knotenlastdefinitionen
        if (!_modell.ZeitabhängigeKnotenLasten.ContainsKey(LastId.Text))
        {
            KnotenId.Text = "";
            Datei.IsChecked = false;
            Konstant.Text = "";
            Amplitude.Text = "";
            Frequenz.Text = "";
            Winkel.Text = "";
            Linear.Text = "";
            return;
        }

        // vorhandene zeitabhängige Knotenlastdefinitionen
        if (!_modell.ZeitabhängigeKnotenLasten.TryGetValue(LastId.Text, out vorhandeneKnotenlast)) return;
        lastId = vorhandeneKnotenlast.LastId;
        KnotenId.Text = vorhandeneKnotenlast.KnotenId;
        switch (vorhandeneKnotenlast.VariationsTyp)
        {
            case 0:
                Datei.IsChecked = true;
                break;
            case 1:
                Konstant.Text = vorhandeneKnotenlast.KonstanteTemperatur.ToString("G2");
                break;
            case 2:
                Amplitude.Text = vorhandeneKnotenlast.Amplitude.ToString("G2");
                Frequenz.Text = vorhandeneKnotenlast.Frequenz.ToString("G2");
                Winkel.Text = vorhandeneKnotenlast.PhasenWinkel.ToString("G2");
                break;
            case 3:
            {
                var intervall = vorhandeneKnotenlast.Intervall;
                var sb = new StringBuilder();
                sb.Append(intervall[0].ToString("G2") + ";");
                sb.Append(intervall[1].ToString("G2"));
                for (var i = 2; i < intervall.Length; i += 2)
                {
                    sb.Append("\t");
                    sb.Append(intervall[i].ToString("G2") + ";");
                    sb.Append(intervall[i + 1].ToString("G2"));
                }

                Linear.Text = sb.ToString();
                break;
            }
        }
    }
}