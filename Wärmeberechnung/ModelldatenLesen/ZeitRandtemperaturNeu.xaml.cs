using FE_Berechnungen.Wärmeberechnung.Modelldaten;
using FE_Berechnungen.Wärmeberechnung.ModelldatenAnzeigen;
using System.Globalization;

namespace FE_Berechnungen.Wärmeberechnung.ModelldatenLesen;

public partial class ZeitRandtemperaturNeu
{
    private readonly FeModell _modell;
    public string AktuelleId;
    public ZeitRandtemperaturNeu(FeModell modell)
    {
        _modell = modell;
        InitializeComponent();
        AktuelleId = "";
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

        // vorhandene zeitabhängige Randbedingung
        if (_modell.ZeitabhängigeRandbedingung.TryGetValue(randbedingungId, out var vorhandeneRandbedingung))
        {
            vorhandeneRandbedingung.Datei = false;
            vorhandeneRandbedingung.Amplitude = 0;
            vorhandeneRandbedingung.Frequenz = 0;
            vorhandeneRandbedingung.PhasenWinkel = 0;
            vorhandeneRandbedingung.Intervall = null;
            if (KnotenId.Text.Length > 0)
                vorhandeneRandbedingung.KnotenId = KnotenId.Text.ToString(CultureInfo.CurrentCulture);
            try
            {
                if (Datei.IsChecked == true)
                {
                    vorhandeneRandbedingung.Datei = true;
                    vorhandeneRandbedingung.VariationsTyp = 0;
                }
                else if (Konstant.Text.Length > 0)
                {
                    vorhandeneRandbedingung.VariationsTyp = 1;
                    vorhandeneRandbedingung.KonstanteTemperatur = double.Parse(Konstant.Text);
                }
                else if (Amplitude.Text.Length > 0 && Frequenz.Text.Length > 0 && Winkel.Text.Length > 0)
                {
                    vorhandeneRandbedingung.VariationsTyp = 2;
                    vorhandeneRandbedingung.Amplitude = double.Parse(Amplitude.Text);
                    vorhandeneRandbedingung.Frequenz = double.Parse(Frequenz.Text);
                    vorhandeneRandbedingung.PhasenWinkel = double.Parse(Winkel.Text);
                }
                else if (Linear.Text.Length > 0)
                {
                    vorhandeneRandbedingung.VariationsTyp = 3;
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

                    vorhandeneRandbedingung.Intervall = intervall;
                    vorhandeneRandbedingung.VariationsTyp = 3;
                }
            }
            catch (FormatException)
            {
                _ = MessageBox.Show("ungültiges Format in der Eingabe", "neue zeitabhängige Randtemperatur");
                return;
            }
        }

        // neue zeitabhängige Randbedingung
        else
        {
            var knotenId = "";
            ZeitabhängigeRandbedingung zeitRandbedingung = null;

            if (KnotenId.Text.Length > 0) knotenId = KnotenId.Text.ToString(CultureInfo.CurrentCulture);
            if (!_modell.Knoten.TryGetValue(knotenId, out _))
                throw new ModellAusnahme("Knoten für Randbedingung im Modell nicht vorhanden");

            try
            {
                if (Datei.IsChecked == true)
                {
                    zeitRandbedingung = new ZeitabhängigeRandbedingung(knotenId, true)
                    {
                        RandbedingungId = randbedingungId,
                        VariationsTyp = 0
                    };
                }
                else if (Konstant.Text.Length > 0)
                {
                    zeitRandbedingung = new ZeitabhängigeRandbedingung(knotenId, double.Parse(Konstant.Text))
                    {
                        RandbedingungId = randbedingungId,
                        VariationsTyp = 1
                    };
                }
                else if (Amplitude.Text.Length > 0 && Frequenz.Text.Length > 0 && Winkel.Text.Length > 0)
                {
                    var amplitude = double.Parse(Amplitude.Text);
                    var frequenz = double.Parse(Frequenz.Text);
                    var winkel = double.Parse(Winkel.Text);
                    zeitRandbedingung = new ZeitabhängigeRandbedingung(knotenId, amplitude, frequenz, winkel)
                    {
                        RandbedingungId = randbedingungId,
                        VariationsTyp = 2
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

                    zeitRandbedingung = new ZeitabhängigeRandbedingung(knotenId, intervall)
                    {
                        RandbedingungId = randbedingungId,
                        VariationsTyp = 3,
                    };
                }
            }
            catch (FormatException)
            {
                _ = MessageBox.Show("ungültiges Format in der Eingabe", "neue zeitabhängige Knotenlast");
                return;
            }
            
            _modell.ZeitabhängigeRandbedingung.Add(randbedingungId, zeitRandbedingung);
            StartFenster.WärmeVisual.IsZeitRandtemperatur = true;
        }

        if (AktuelleId != RandbedingungId.Text) _modell.ZeitabhängigeRandbedingung.Remove(AktuelleId);

        Close();
        StartFenster.WärmeVisual.Close();
        StartFenster.WärmeVisual = new WärmemodellVisualisieren(_modell);
        StartFenster.WärmeVisual.Show();
    }

    private void BtnDialogCancel_Click(object sender, RoutedEventArgs e)
    {
        StartFenster.WärmeVisual.IsZeitRandtemperatur = false;
        Close();
    }

    private void BtnLöschen_Click(object sender, RoutedEventArgs e)
    {
        if (!_modell.ZeitabhängigeRandbedingung.Remove(RandbedingungId.Text)) return;
        Close();
        StartFenster.WärmeVisual.Close();
        StartFenster.WärmeVisual = new WärmemodellVisualisieren(_modell);
        StartFenster.WärmeVisual.Show();
        _modell.Berechnet = false;
    }

    private void RandbedingungIdLostFocus(object sender, RoutedEventArgs e)
    {
        // neue zeitabhängige Randbedingungsdefinition
        if (!_modell.ZeitabhängigeRandbedingung.ContainsKey(RandbedingungId.Text)) return;
        
        // vorhandene zeitabhängige Randbedingungsdefinitionen
        if (!_modell.ZeitabhängigeRandbedingung.TryGetValue(RandbedingungId.Text, out var vorhandeneRandbedingung)) return;
        RandbedingungId.Text = vorhandeneRandbedingung.RandbedingungId;
        KnotenId.Text = vorhandeneRandbedingung.KnotenId;
        vorhandeneRandbedingung.Vordefiniert = new double[1];
        switch (vorhandeneRandbedingung.VariationsTyp)
        {
            case 0:
                Datei.IsChecked = true;
                break;
            case 1:
                Konstant.Text = vorhandeneRandbedingung.KonstanteTemperatur.ToString("G2");
                break;
            case 2:
                Amplitude.Text = vorhandeneRandbedingung.Amplitude.ToString("G4");
                Frequenz.Text = (vorhandeneRandbedingung.Frequenz / 2 / Math.PI).ToString("G4");
                Winkel.Text = (vorhandeneRandbedingung.PhasenWinkel * 180 / Math.PI).ToString("G4");
                break;
            case 3:
                {
                    var intervall = vorhandeneRandbedingung.Intervall;
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

    private void KnotenIdLostFocus(object sender, RoutedEventArgs e)
    {
        if (!_modell.Knoten.TryGetValue(KnotenId.Text, out var vorhandenerKnoten))
        {
            _ = MessageBox.Show("Knoten nicht im Modell gefunden", "neue zeitabhängige Knotenlast");
            RandbedingungId.Text = "";
            KnotenId.Text = "";
        }
        else
        {
            KnotenId.Text = vorhandenerKnoten.Id;
            if (RandbedingungId.Text != "") return;
            RandbedingungId.Text = "zRb_" + KnotenId.Text;
            AktuelleId = RandbedingungId.Text;
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