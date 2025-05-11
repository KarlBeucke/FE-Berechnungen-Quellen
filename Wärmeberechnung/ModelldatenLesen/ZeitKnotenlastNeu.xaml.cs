using FE_Berechnungen.Wärmeberechnung.Modelldaten;
using FE_Berechnungen.Wärmeberechnung.ModelldatenAnzeigen;
using System.Globalization;

namespace FE_Berechnungen.Wärmeberechnung.ModelldatenLesen;

public partial class ZeitKnotenlastNeu
{
    private readonly FeModell _modell;
    public string AktuelleId;

    public ZeitKnotenlastNeu(FeModell modell)
    {
        InitializeComponent();
        _modell = modell;
        AktuelleId = "";
        Show();
    }

    private void BtnDialogOk_Click(object sender, RoutedEventArgs e)
    {
        var knotenlastId = LastId.Text;
        if (knotenlastId == "")
        {
            _ = MessageBox.Show("zeitabhängige Knotenlast Id muss definiert sein", "neue zeitabhängige Knotenlast");
            return;
        }

        // vorhandene zeitabhängige Knotenlast (Temperatur)
        if (_modell.ZeitabhängigeKnotenLasten.TryGetValue(knotenlastId, out var vorhandeneZeitKnotenlast))
        {
            vorhandeneZeitKnotenlast.Datei = false;
            vorhandeneZeitKnotenlast.Amplitude = 0;
            vorhandeneZeitKnotenlast.Frequenz = 0;
            vorhandeneZeitKnotenlast.PhasenWinkel = 0;
            vorhandeneZeitKnotenlast.Intervall = null;
            if (KnotenId.Text.Length > 0)
                vorhandeneZeitKnotenlast.KnotenId = KnotenId.Text.ToString(CultureInfo.CurrentCulture);

            try
            {
                if (Datei.IsChecked == true)
                {
                    vorhandeneZeitKnotenlast.Datei = true;
                    vorhandeneZeitKnotenlast.VariationsTyp = 0;
                }
                else if (Konstant.Text.Length > 0)
                {
                    vorhandeneZeitKnotenlast.VariationsTyp = 1;
                    vorhandeneZeitKnotenlast.KonstanteTemperatur = double.Parse(Konstant.Text);
                }
                else if (Amplitude.Text.Length > 0 && Frequenz.Text.Length > 0 && Winkel.Text.Length > 0)
                {
                    vorhandeneZeitKnotenlast.VariationsTyp = 2;
                    vorhandeneZeitKnotenlast.Amplitude = double.Parse(Amplitude.Text);
                    vorhandeneZeitKnotenlast.Frequenz = double.Parse(Frequenz.Text);
                    vorhandeneZeitKnotenlast.PhasenWinkel = double.Parse(Winkel.Text);
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

                    vorhandeneZeitKnotenlast.Intervall = intervall;
                    vorhandeneZeitKnotenlast.VariationsTyp = 3;

                }
            }
            catch (FormatException)
            {
                _ = MessageBox.Show("ungültiges  Eingabeformat", "neue Knotentemperaturen");
            }
        }

        // neue zeitabhängige Knotenlast (Temperatur)
        else
        {
            var knotenId = "";
            ZeitabhängigeKnotenLast zeitKnotenlast = null;

            if (KnotenId.Text.Length > 0) knotenId = KnotenId.Text.ToString(CultureInfo.CurrentCulture);
            if (!_modell.Knoten.TryGetValue(knotenId, out _))
                throw new ModellAusnahme("Lastknoten im Modell nicht vorhanden");

            try
            {
                if (Datei.IsChecked == true)
                {
                    zeitKnotenlast = new ZeitabhängigeKnotenLast(knotenId, true)
                    {
                        LastId = knotenlastId,
                        VariationsTyp = 0
                    };
                }
                else if (Konstant.Text.Length > 0)
                {
                    zeitKnotenlast = new ZeitabhängigeKnotenLast(knotenId, double.Parse(Konstant.Text))
                    {
                        LastId = knotenlastId,
                        VariationsTyp = 1
                    };
                }
                else if (Amplitude.Text.Length > 0 && Frequenz.Text.Length > 0 && Winkel.Text.Length > 0)
                {
                    var amplitude = double.Parse(Amplitude.Text);
                    var frequenz = double.Parse(Frequenz.Text);
                    var phasenWinkel = double.Parse(Winkel.Text);
                    zeitKnotenlast = new ZeitabhängigeKnotenLast(knotenId, amplitude, frequenz, phasenWinkel)
                    {
                        LastId = knotenlastId,
                        VariationsTyp = 2
                    };
                }
                else if (Linear.Text.Length > 0)
                {
                    char[] delimiters = [' ', '\t'];
                    var teilStrings = Linear.Text.Split(delimiters);
                    var k = 0;
                    char[] paarDelimiter = [';'];
                    var intervall = new double[2 * teilStrings.Length];
                    for (var i = 0; i < intervall.Length; i += 2)
                    {
                        var wertePaar = teilStrings[k].Split(paarDelimiter);
                        intervall[i] = double.Parse(wertePaar[0]);
                        intervall[i + 1] = double.Parse(wertePaar[1]);
                        k++;
                    }

                    zeitKnotenlast = new ZeitabhängigeKnotenLast(knotenId, intervall) { LastId = knotenlastId, VariationsTyp = 3 };
                }
            }
            catch (FormatException)
            {
                _ = MessageBox.Show("ungültiges Format in der Eingabe", "neue zeitabhängige Knotenlast");
                return;
            }

            _modell.ZeitabhängigeKnotenLasten.Add(knotenlastId, zeitKnotenlast);
            StartFenster.WärmeVisual.IsZeitKnotentemperatur = true;
        }

        if(AktuelleId != LastId.Text) _modell.ZeitabhängigeKnotenLasten.Remove(AktuelleId);

        Close();
        StartFenster.WärmeVisual.Close();
        StartFenster.WärmeVisual = new WärmemodellVisualisieren(_modell);
        StartFenster.WärmeVisual.Show();
    }

    private void BtnDialogCancel_Click(object sender, RoutedEventArgs e)
    {
        StartFenster.TragwerkVisual.IsZeitKnotenlast = false;
        Close();
    }

    private void BtnLöschen_Click(object sender, RoutedEventArgs e)
    {
        if (!_modell.ZeitabhängigeKnotenLasten.Remove(LastId.Text, out _)) return;
        Close();
        StartFenster.WärmeVisual.Close();
        StartFenster.WärmeVisual = new WärmemodellVisualisieren(_modell);
        StartFenster.WärmeVisual.Show();
        _modell.Berechnet = false;
    }

    private void LastIdLostFocus(object sender, RoutedEventArgs e)
    {
        if (!_modell.ZeitabhängigeKnotenLasten.TryGetValue(LastId.Text, out var vorhandeneZeitKnotenlast)) return;

        // vorhandene zeitabhängige Knotenlastdefinitionen
        LastId.Text = vorhandeneZeitKnotenlast.LastId;
        KnotenId.Text = vorhandeneZeitKnotenlast.KnotenId;
        switch (vorhandeneZeitKnotenlast.VariationsTyp)
        {
            case 0:
                Datei.IsChecked = true;
                break;
            case 1:
                Konstant.Text = vorhandeneZeitKnotenlast.KonstanteTemperatur.ToString("G2");
                break;
            case 2:
                Amplitude.Text = vorhandeneZeitKnotenlast.Amplitude.ToString("G2");
                Frequenz.Text = vorhandeneZeitKnotenlast.Frequenz.ToString("G2");
                Winkel.Text = vorhandeneZeitKnotenlast.PhasenWinkel.ToString("G2");
                break;
            case 3:
                {
                    var intervall = vorhandeneZeitKnotenlast.Intervall;
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

    private void KnotenIdLostFocus(object sender, RoutedEventArgs e)
    {
        if (!_modell.Knoten.TryGetValue(KnotenId.Text, out var vorhandenerKnoten))
        {
            _ = MessageBox.Show("Knoten nicht im Modell gefunden", "neue zeitabhängige Knotenlast");
            LastId.Text = "";
            KnotenId.Text = "";
        }
        else
        {
            KnotenId.Text = vorhandenerKnoten.Id;
            if (LastId.Text != "") return;
            LastId.Text = "zkl_" + KnotenId.Text;
            AktuelleId = LastId.Text;
        }
    }

    private void KnotenPositionNeu(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        _modell.Knoten.TryGetValue(KnotenId.Text, out var knoten);
        if (knoten == null) { _ = MessageBox.Show("Knoten nicht im Modell gefunden", "neue zeitabhängige Knotenlast"); return; }
        StartFenster.WärmeVisual.KnotenEdit(knoten);
        Close();
        _modell.Berechnet = false;
    }
}