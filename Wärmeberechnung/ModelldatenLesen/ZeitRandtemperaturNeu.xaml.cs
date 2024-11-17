using FE_Berechnungen.Wärmeberechnung.Modelldaten;
using FE_Berechnungen.Wärmeberechnung.ModelldatenAnzeigen;
using System.Globalization;
using System.Text;

namespace FE_Berechnungen.Wärmeberechnung.ModelldatenLesen;

public partial class ZeitRandtemperaturNeu
{
    private readonly FeModell _modell;
    private AbstraktZeitabhängigeRandbedingung _vorhandeneRandbedingung;

    public ZeitRandtemperaturNeu(FeModell modell)
    {
        _modell = modell;
        InitializeComponent();
        Show();
    }

    private void BtnDialogOk_Click(object sender, RoutedEventArgs e)
    {
        var randbedingungId = RandbedingungId.Text;
        if (randbedingungId == "")
        {
            _ = MessageBox.Show("Randbedingung Id muss definiert sein", "neue zeitabhängige Randbedingung");
            return;
        }

        // vorhandene Randbedingung
        if (_modell.ZeitabhängigeRandbedingung.TryGetValue(randbedingungId, out _vorhandeneRandbedingung))
        {
            if (KnotenId.Text.Length > 0)
                _vorhandeneRandbedingung.KnotenId = KnotenId.Text.ToString(CultureInfo.CurrentCulture);
            try
            {
                if (Datei.IsChecked == true)
                {
                    _vorhandeneRandbedingung.VariationsTyp = 0;
                }
                else if (Konstant.Text.Length > 0)
                {
                    _vorhandeneRandbedingung.VariationsTyp = 1;
                    _vorhandeneRandbedingung.KonstanteTemperatur = double.Parse(Konstant.Text);
                }
                else if (Amplitude.Text.Length > 0 && Frequenz.Text.Length > 0 && Winkel.Text.Length > 0)
                {
                    _vorhandeneRandbedingung.VariationsTyp = 2;
                    _vorhandeneRandbedingung.Amplitude = double.Parse(Amplitude.Text);
                    _vorhandeneRandbedingung.Frequenz = double.Parse(Frequenz.Text);
                    _vorhandeneRandbedingung.PhasenWinkel = double.Parse(Winkel.Text);
                }
                else if (Linear.Text.Length > 0)
                {
                    _vorhandeneRandbedingung.VariationsTyp = 3;
                    char[] delimiters = [' ', '\t'];
                    var teilStrings = Linear.Text.Split(delimiters);

                    var k = 0;
                    char[] paarDelimiter = [';'];
                    // split teilStrings zählt auch delimiter HINTER text mit
                    var intervall = new double[2 * teilStrings.Length - 2];
                    for (var i = 0; i < intervall.Length; i += 2)
                    {
                        var wertePaar = teilStrings[k].Split(paarDelimiter);
                        intervall[i] = double.Parse(wertePaar[0]);
                        intervall[i + 1] = double.Parse(wertePaar[1]);
                        k++;
                    }

                    _vorhandeneRandbedingung.Intervall = intervall;
                }
            }
            catch (FormatException)
            {
                _ = MessageBox.Show("ungültiges Format in der Eingabe", "neue zeitabhängige Randtemperatur");
                return;
            }
        }

        // neue Randbedingung
        else
        {
            var knotenId = "";
            ZeitabhängigeRandbedingung randbedingung = null;
            if (KnotenId.Text.Length > 0) knotenId = KnotenId.Text.ToString(CultureInfo.CurrentCulture);
            if (Datei.IsChecked == true)
            {
                randbedingung = new ZeitabhängigeRandbedingung(knotenId, true)
                {
                    Typ = 0
                };
            }
            else if (Konstant.Text.Length > 0)
            {
                randbedingung = new ZeitabhängigeRandbedingung(knotenId, double.Parse(Konstant.Text))
                {
                    Typ = 1
                };
            }
            else if (Amplitude.Text.Length > 0 && Frequenz.Text.Length > 0 && Winkel.Text.Length > 0)
            {
                var amplitude = double.Parse(Amplitude.Text);
                var frequenz = double.Parse(Frequenz.Text);
                var winkel = double.Parse(Winkel.Text);
                randbedingung = new ZeitabhängigeRandbedingung(knotenId, amplitude, frequenz, winkel)
                {
                    Typ = 2
                };
            }
            else if (Linear.Text.Length > 0)
            {
                char[] delimiters = [' ', '\t'];
                var teilStrings = Linear.Text.Split(delimiters);
                var k = 0;
                char[] paarDelimiter = [';'];
                // split teilStrings zählt auch delimiter HINTER text mit
                var intervall = new double[2 * teilStrings.Length - 2];
                for (var i = 0; i < intervall.Length; i += 2)
                {
                    var wertePaar = teilStrings[k].Split(paarDelimiter);
                    intervall[i] = double.Parse(wertePaar[0]);
                    intervall[i + 1] = double.Parse(wertePaar[1]);
                    k++;
                }

                randbedingung = new ZeitabhängigeRandbedingung(knotenId, intervall)
                {
                    Typ = 3
                };
            }
            _modell.ZeitabhängigeRandbedingung.Add(randbedingungId, randbedingung);
            StartFenster.WärmeVisual.IsZeitRandtemperatur = true;
        }
        Close();
        StartFenster.WärmeVisual.Close();
        StartFenster.WärmeVisual = new WärmemodellVisualisieren(_modell);
        StartFenster.WärmeVisual.Show();
    }

    private void BtnDialogCancel_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void BtnLöschen_Click(object sender, RoutedEventArgs e)
    {
        if (!_modell.ZeitabhängigeRandbedingung.ContainsKey(RandbedingungId.Text)) return;
        _modell.ZeitabhängigeRandbedingung.Remove(RandbedingungId.Text);
        Close();
        StartFenster.WärmeVisual.Close();
        StartFenster.WärmeVisual = new WärmemodellVisualisieren(_modell);
        StartFenster.WärmeVisual.Show();
    }

    private void RandbedingungIdLostFocus(object sender, RoutedEventArgs e)
    {
        // neue zeitabhängige Randbedingungsdefinitionen
        if (!_modell.ZeitabhängigeRandbedingung.ContainsKey(RandbedingungId.Text))
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

        // vorhandene zeitabhängige Randbedingungsdefinitionen
        if (!_modell.ZeitabhängigeRandbedingung.TryGetValue(RandbedingungId.Text, out _vorhandeneRandbedingung)) return;
        RandbedingungId.Text = _vorhandeneRandbedingung.RandbedingungId;
        KnotenId.Text = _vorhandeneRandbedingung.KnotenId;
        _vorhandeneRandbedingung.Vordefiniert = new double[1];
        switch (_vorhandeneRandbedingung.VariationsTyp)
        {
            case 0:
                Datei.IsChecked = true;
                break;
            case 1:
                Konstant.Text = _vorhandeneRandbedingung.KonstanteTemperatur.ToString("G2");
                break;
            case 2:
                Amplitude.Text = _vorhandeneRandbedingung.Amplitude.ToString("G4");
                Frequenz.Text = (_vorhandeneRandbedingung.Frequenz / 2 / Math.PI).ToString("G4");
                Winkel.Text = (_vorhandeneRandbedingung.PhasenWinkel * 180 / Math.PI).ToString("G4");
                break;
            case 3:
                {
                    var intervall = _vorhandeneRandbedingung.Intervall;
                    var sb = new StringBuilder();
                    sb.Append(intervall[0].ToString("G2") + ";");
                    sb.Append(intervall[1].ToString("G2"));
                    for (var i = 2; i < intervall.Length; i += 2)
                    {
                        sb.Append('\t');
                        sb.Append(intervall[i].ToString("G2") + ";");
                        sb.Append(intervall[i + 1].ToString("G2"));
                    }

                    Linear.Text = sb.ToString();
                    break;
                }
        }
    }
}