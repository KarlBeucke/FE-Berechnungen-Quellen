using FE_Berechnungen.Tragwerksberechnung.Modelldaten;
using FE_Berechnungen.Tragwerksberechnung.ModelldatenAnzeigen;
using System.Globalization;

namespace FE_Berechnungen.Tragwerksberechnung.ModelldatenLesen;

public partial class KnotenlastNeu
{
    private readonly FeModell _modell;

    public KnotenlastNeu()
    {
        InitializeComponent();
        Show();
    }

    public KnotenlastNeu(FeModell modell)
    {
        InitializeComponent();
        _modell = modell;
        Show();
    }

    public KnotenlastNeu(FeModell modell, string last, string knoten, double px, double py, double m)
    {
        InitializeComponent();
        _modell = modell;
        LastId.Text = last;
        KnotenId.Text = knoten;
        Px.Text = px.ToString("0.00");
        Py.Text = py.ToString("0.00");
        M.Text = m.ToString("0.00");
        Show();
    }

    private void BtnDialogOk_Click(object sender, RoutedEventArgs e)
    {
        var knotenlastId = LastId.Text;
        if (knotenlastId == "")
        {
            _ = MessageBox.Show("Knotenlast Id muss definiert sein", "neue Knotenlast");
            return;
        }

        // vorhandene Knotenlast
        if (_modell.Lasten.TryGetValue(knotenlastId, out var vorhandeneKnotenlast))
        {
            if (KnotenId.Text.Length > 0)
                vorhandeneKnotenlast.KnotenId = KnotenId.Text.ToString(CultureInfo.CurrentCulture);
            try
            {
                if (Px.Text.Length > 0) vorhandeneKnotenlast.Lastwerte[0] = double.Parse(Px.Text);
                if (Py.Text.Length > 0) vorhandeneKnotenlast.Lastwerte[1] = double.Parse(Py.Text);
                if (M.Text.Length > 0) vorhandeneKnotenlast.Lastwerte[2] = double.Parse(M.Text);
            }
            catch (FormatException)
            {
                _ = MessageBox.Show("ungültiges Format in der Eingabe", "neue Knotenlast");
                return;
            }
        }
        
        // neue Knotenlast
        else
        {
            var knotenId = "";
            double px = 0, py = 0, m = 0;
            if (KnotenId.Text.Length > 0) knotenId = KnotenId.Text.ToString(CultureInfo.CurrentCulture);
            if(!_modell.Knoten.TryGetValue(knotenId, out var knoten))
                throw new ModellAusnahme("Lastknoten im Modell nicht vorhanden");

            try
            {
                if (Px.Text.Length > 0) px = double.Parse(Px.Text);
                if (Py.Text.Length > 0) py = double.Parse(Py.Text);
                if (M.Text.Length > 0) m = double.Parse(M.Text);
            }
            catch (FormatException)
            {
                _ = MessageBox.Show("ungültiges Format in der Eingabe", "neue Knotenlast");
                return;
            }

            var knotenlast = knoten.AnzahlKnotenfreiheitsgrade switch
            {
                3 => new KnotenLast(knotenId, px, py, m),
                2 => new KnotenLast(knotenId, px, py),
                _ => throw new ModellAusnahme("Lastzuweisung an ungültigen Freiheitsgrad")
            };

            knotenlast.LastId = knotenlastId;
            _modell.Lasten.Add(knotenlastId, knotenlast);
        }

        Close();
        StartFenster.TragwerkVisual.Close();
        StartFenster.TragwerkVisual = new TragwerkmodellVisualisieren(_modell);
        StartFenster.TragwerkVisual.Show();
        _modell.Berechnet = false;
    }

    private void BtnDialogCancel_Click(object sender, RoutedEventArgs e)
    {
        Close();
        StartFenster.TragwerkVisual.IsKnotenlast = false;
    }

    private void LastIdLostFocus(object sender, RoutedEventArgs e)
    {
        if (!_modell.Lasten.ContainsKey(LastId.Text))
        {
            KnotenId.Text = "";
            return;
        }

        // vorhandene Knotenlastdefinition
        if (!_modell.Lasten.TryGetValue(LastId.Text, out var vorhandeneKnotenlast))
           throw new ModellAusnahme("\nKnotenlast '" + LastId.Text + "' nicht im Modell gefunden");
       
        LastId.Text = vorhandeneKnotenlast.LastId;
        KnotenId.Text = vorhandeneKnotenlast.KnotenId;
        Px.Text = vorhandeneKnotenlast.Lastwerte[0].ToString("G3", CultureInfo.CurrentCulture);
        Py.Text = vorhandeneKnotenlast.Lastwerte[1].ToString("G3", CultureInfo.CurrentCulture);
        if (vorhandeneKnotenlast.Lastwerte.Length > 2)
            M.Text = vorhandeneKnotenlast.Lastwerte[2].ToString("G3", CultureInfo.CurrentCulture);
    }

    private void KnotenIdLostFocus(object sender, RoutedEventArgs e)
    {
        _modell.Knoten.TryGetValue(KnotenId.Text, out var vorhandenerKnoten);
        if (vorhandenerKnoten == null)
        {
            _ = MessageBox.Show("Knoten nicht im Modell gefunden", "neue Knotenlast");
            LastId.Text = "";
            KnotenId.Text = "";
            return;
        }

        if (LastId.Text == "") LastId.Text = "KL_" + KnotenId.Text;
    }

    private void BtnLöschen_Click(object sender, RoutedEventArgs e)
    {
        if (!_modell.Lasten.ContainsKey(LastId.Text)) return;
        _modell.Lasten.Remove(LastId.Text);
        StartFenster.TragwerkVisual.TragwerkLastenKeys?.Close();
        Close();
        StartFenster.TragwerkVisual.Close();

        StartFenster.TragwerkVisual = new TragwerkmodellVisualisieren(_modell);
        StartFenster.TragwerkVisual.Show();
        _modell.Berechnet = false;
    }

    private void KnotenPositionNeu(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        _modell.Knoten.TryGetValue(KnotenId.Text, out var knoten);
        if (knoten == null) { _ = MessageBox.Show("Knoten nicht im Modell gefunden", "neue Knotenlast"); return; }
        StartFenster.TragwerkVisual.KnotenClick(knoten);
        Close();
        _modell.Berechnet = false;
    }
}