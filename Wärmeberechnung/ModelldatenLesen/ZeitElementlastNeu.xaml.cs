using FE_Berechnungen.Wärmeberechnung.Modelldaten;
using FE_Berechnungen.Wärmeberechnung.ModelldatenAnzeigen;

namespace FE_Berechnungen.Wärmeberechnung.ModelldatenLesen;

public partial class ZeitElementlastNeu
{
    private readonly FeModell _modell;
    public string AktuelleId;
    public ZeitElementlastNeu(FeModell modell)
    {
        _modell = modell;
        InitializeComponent();
        Show();
    }

    private void BtnDialogOk_Click(object sender, RoutedEventArgs e)
    {
        var elementlastId = LastId.Text;
        if (elementlastId == "")
        {
            _ = MessageBox.Show("Elementlast Id muss definiert sein", "neue zeitabhängige Elementlast");
            return;
        }

        // vorhandene zeitabhängige Elementlast
        if (_modell.ZeitabhängigeElementLasten.TryGetValue(elementlastId, out var vorhandeneLast))
        {
            if (ElementId.Text.Length > 0) vorhandeneLast.ElementId = ElementId.Text;
            try
            {
                if (P0.Text.Length > 0) vorhandeneLast.P[0] = double.Parse(P0.Text);
                if (P1.Text.Length > 0) vorhandeneLast.P[1] = double.Parse(P1.Text);
                if (P2.Text.Length > 0) vorhandeneLast.P[2] = double.Parse(P2.Text);
                if (P3.Text.Length > 0) vorhandeneLast.P[3] = double.Parse(P3.Text);
            }
            catch (FormatException)
            {
                _ = MessageBox.Show("ungültiges  Eingabeformat", "neue Elementtemperaturen");
                return;
            }
        }

        // neue zeitabhängige Elementlast
        else
        {
            var elementId = "";
            var p = new double[4];
            if (ElementId.Text.Length > 0) elementId = ElementId.Text;
            try
            {
                if (P0.Text.Length > 0) p[0] = double.Parse(P0.Text);
                if (P1.Text.Length > 0) p[1] = double.Parse(P1.Text);
                if (P2.Text.Length > 0) p[2] = double.Parse(P2.Text);
                if (P3.Text.Length > 0) p[3] = double.Parse(P3.Text);
            }
            catch (FormatException)
            {
                _ = MessageBox.Show("ungültiges  Eingabeformat", "neue Elementtemperaturen");
                return;
            }

            var zeitabhängigeElementlast = new ZeitabhängigeElementLast(elementId, p)
            {
                LastId = elementlastId
            };
            _modell.ZeitabhängigeElementLasten.Add(elementlastId, zeitabhängigeElementlast);
            StartFenster.WärmeVisual.IsZeitElementtemperatur = true;
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
        if (!_modell.ZeitabhängigeElementLasten.Remove(LastId.Text)) return;
        Close();
        StartFenster.WärmeVisual.Close();
        StartFenster.WärmeVisual = new WärmemodellVisualisieren(_modell);
        StartFenster.WärmeVisual.Show();
    }

    private void LastIdLostFocus(object sender, RoutedEventArgs e)
    {
        if (!_modell.ZeitabhängigeElementLasten.ContainsKey(LastId.Text))
        {
            ElementId.Text = "";
            P0.Text = "";
            P1.Text = "";
            P2.Text = "";
            P3.Text = "";
            return;
        }

        // vorhandene zeitabhängige Elementlastdefinition
        if (!_modell.ZeitabhängigeElementLasten.TryGetValue(LastId.Text, out var vorhandeneLast)) return;
        LastId.Text = vorhandeneLast.LastId;

        ElementId.Text = vorhandeneLast.ElementId;
        P0.Text = vorhandeneLast.Lastwerte[0].ToString("G2");
        P1.Text = vorhandeneLast.Lastwerte[1].ToString("G2");
        P2.Text = vorhandeneLast.Lastwerte[2].ToString("G2");
        P3.Text = vorhandeneLast.Lastwerte[3].ToString("G2");
    }
}