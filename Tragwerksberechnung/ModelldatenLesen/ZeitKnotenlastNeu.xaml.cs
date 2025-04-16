using System.Globalization;
using FE_Berechnungen.Tragwerksberechnung.Modelldaten;
using FE_Berechnungen.Tragwerksberechnung.ModelldatenAnzeigen;

namespace FE_Berechnungen.Tragwerksberechnung.ModelldatenLesen;

public partial class ZeitKnotenlastNeu
{
    private readonly FeModell _modell;
    public string AktuelleId;

    public ZeitKnotenlastNeu(FeModell modell)
    {
        InitializeComponent();
        _modell = modell;
        LastId.Text = string.Empty;
        KnotenId.Text = string.Empty;
        KnotenDof.Text = string.Empty;
        Bodenanregung.IsChecked = false;
        Datei.IsChecked = false;
        Amplitude.Text = string.Empty;
        Frequenz.Text = string.Empty;
        Winkel.Text = string.Empty;
        Linear.Text = string.Empty;
        Show();
    }

    private void BtnDialogOk_Click(object sender, RoutedEventArgs e)
    {
        var knotenlastId = LastId.Text;
        if (knotenlastId == "")
        {
            _ = MessageBox.Show("ZeitKnotenlast Id muss definiert sein", "neue ZeitKnotenlast");
            return;
        }

        // vorhandene zeitabhängige Knotenlast
        if (_modell.ZeitabhängigeKnotenLasten.TryGetValue(knotenlastId, out var vorhandeneZeitKnotenlast))
        {
            if (KnotenId.Text.Length > 0)
                vorhandeneZeitKnotenlast.KnotenId = KnotenId.Text.ToString(CultureInfo.CurrentCulture);
            try
            {
                if (KnotenDof.Text.Length > 0) vorhandeneZeitKnotenlast.KnotenFreiheitsgrad = int.Parse(KnotenDof.Text);
                if (Bodenanregung.IsChecked == true) vorhandeneZeitKnotenlast.Bodenanregung = true;
                if (Datei.IsChecked == true) vorhandeneZeitKnotenlast.Datei = true;
                if (Amplitude.Text.Length > 0) vorhandeneZeitKnotenlast.Amplitude = double.Parse(Amplitude.Text);
                if (Frequenz.Text.Length > 0) vorhandeneZeitKnotenlast.Frequenz = double.Parse(Frequenz.Text);
                if (Winkel.Text.Length > 0) vorhandeneZeitKnotenlast.PhasenWinkel = double.Parse(Winkel.Text);
                if (Linear.Text.Length <= 0) return;
                var knotenlinear = Linear.Text;
                char[] delimiters = [' ', ';'];
                var substrings = knotenlinear.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
                var interval = new double[substrings.Length];
                for (var j = 0; j < substrings.Length; j += 2)
                {
                    interval[j] = double.Parse(substrings[j]);
                    interval[j + 1] = double.Parse(substrings[j + 1]);
                }
                vorhandeneZeitKnotenlast.Intervall = interval;
            }
            catch (FormatException)
            {
                _ = MessageBox.Show("ungültiges Format in der Eingabe", "Neue zeitabhängige Knotenlast");
                return;
            }
        }

        // neue zeitabhängige Knotenlast
        else
        {
            var knotenId = "";
            var knotenDof=0;
            bool boden=false, datei=false;
            double amplitude=0, frequenz=0, phasenWinkel = 0;
            double[] intervall;

            if (KnotenId.Text.Length > 0) knotenId = KnotenId.Text.ToString(CultureInfo.CurrentCulture);
            if (!_modell.Knoten.TryGetValue(knotenId, out _))
                throw new ModellAusnahme("Lastknoten im Modell nicht vorhanden");

            try
            {
                if (KnotenDof.Text.Length > 0) knotenDof = int.Parse(KnotenDof.Text);
                if (Bodenanregung.IsChecked == true) boden = true;
                if (Datei.IsChecked == true) datei = true;
                if (Amplitude.Text.Length > 0) amplitude = double.Parse(Amplitude.Text);
                if (Frequenz.Text.Length > 0) frequenz = double.Parse(Frequenz.Text);
                if (Winkel.Text.Length > 0) phasenWinkel = double.Parse(Winkel.Text);
                if (Linear.Text.Length <= 0) return;
                var knotenlinear = Linear.Text;
                char[] delimiters = [' ', ';'];
                var substrings = knotenlinear.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
                intervall = new double[substrings.Length];
                for (var j = 0; j < substrings.Length; j += 2)
                {
                    intervall[j] = double.Parse(substrings[j]);
                    intervall[j + 1] = double.Parse(substrings[j + 1]);
                }
            }
            catch (FormatException)
            {
                _ = MessageBox.Show("ungültiges Format in der Eingabe", "neue zeitabhängige Knotenlast");
                return;
            }

            var zeitKnotenlast = new ZeitabhängigeKnotenLast(LastId.Text, knotenId, knotenDof, datei, boden)
            {
                Amplitude = amplitude,
                Frequenz = frequenz,
                PhasenWinkel = phasenWinkel,
                Intervall = intervall,
                VariationsTyp = 1
            };
            _modell.ZeitabhängigeKnotenLasten.Add(knotenlastId, zeitKnotenlast);
        }

        if (AktuelleId != LastId.Text) _modell.ZeitabhängigeKnotenLasten.Remove(AktuelleId);

        Close();
        StartFenster.TragwerkVisual.Close();
        StartFenster.TragwerkVisual = new TragwerkmodellVisualisieren(_modell);
        StartFenster.TragwerkVisual.Show();
        _modell.Berechnet = false;
    }

    private void BtnDialogCancel_Click(object sender, RoutedEventArgs e)
    {
        StartFenster.TragwerkVisual.IsZeitKnotenlast = false;
        Close();
    }

    private void LastIdLostFocus(object sender, RoutedEventArgs e)
    {
        if (!_modell.ZeitabhängigeKnotenLasten.TryGetValue(LastId.Text, out var vorhandeneZeitKnotenlast)) return;

        // vorhandene zeitabhängige Knotenlastdefinition
        LastId.Text = vorhandeneZeitKnotenlast.LastId;
        KnotenId.Text = vorhandeneZeitKnotenlast.KnotenId;
        KnotenDof.Text = vorhandeneZeitKnotenlast.KnotenFreiheitsgrad.ToString(CultureInfo.CurrentCulture);
        if (vorhandeneZeitKnotenlast.Bodenanregung.Equals(true)) Bodenanregung.IsChecked = true;
        switch (vorhandeneZeitKnotenlast.VariationsTyp)
        {
            case 0:
                Datei.IsChecked = true;
                break;
            case 1:
            {
                if (vorhandeneZeitKnotenlast.Intervall != null)
                {
                    var knotenlinear = "";
                    for (var i = 0; i < vorhandeneZeitKnotenlast.Intervall.Length; i += 2)
                    {
                        knotenlinear += vorhandeneZeitKnotenlast.Intervall[i].ToString("G2") + ";";
                        knotenlinear += vorhandeneZeitKnotenlast.Intervall[i + 1].ToString("G2") + "  ";
                    }
                    Linear.Text = knotenlinear;
                }
                break;
            }
            case 2:
                Amplitude.Text = vorhandeneZeitKnotenlast.Amplitude.ToString("G2");
                Frequenz.Text = (vorhandeneZeitKnotenlast.Frequenz / (2 * Math.PI)).ToString("G2");
                Winkel.Text = (vorhandeneZeitKnotenlast.PhasenWinkel * 180 / Math.PI).ToString("G2");
                break;
        }
        Show();
    }
    private void KnotenIdLostFocus(object sender, RoutedEventArgs e)
    {
        if (!_modell.Knoten.TryGetValue(KnotenId.Text, out var vorhandenerKnoten))
        {
            if (KnotenId.Text == "boden") return;
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

    private void BtnLöschen_Click(object sender, RoutedEventArgs e)
    {
        if (!_modell.ZeitabhängigeKnotenLasten.Remove(LastId.Text, out _)) return;
        Close();
        StartFenster.TragwerkVisual.Close();

        StartFenster.TragwerkVisual = new TragwerkmodellVisualisieren(_modell);
        StartFenster.TragwerkVisual.Show();
        _modell.Berechnet = false;
    }
}