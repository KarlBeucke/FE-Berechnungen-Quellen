using FE_Berechnungen.Wärmeberechnung.Modelldaten;
using FE_Berechnungen.Wärmeberechnung.ModelldatenAnzeigen;
using System.Globalization;

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
    public ZeitElementlastNeu(FeModell modell, AbstraktZeitabhängigeElementLast zeitElementlast)
    {
        InitializeComponent();
        _modell = modell;
        LastId.Text = zeitElementlast.LastId;
        AktuelleId = zeitElementlast.LastId;
        ElementId.Text = zeitElementlast.ElementId;
        P0.Text = zeitElementlast.P[0].ToString(CultureInfo.CurrentCulture);
        P1.Text = zeitElementlast.P[1].ToString(CultureInfo.CurrentCulture);
        P2.Text = zeitElementlast.P[2].ToString(CultureInfo.CurrentCulture);
        P3.Text = zeitElementlast.P[3].ToString(CultureInfo.CurrentCulture);
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

        if (AktuelleId != LastId.Text) _modell.ZeitabhängigeElementLasten.Remove(AktuelleId);

        Close();
        StartFenster.WärmeVisual.Close();
        StartFenster.WärmeVisual = new WärmemodellVisualisieren(_modell);
        StartFenster.WärmeVisual.Show();
        _modell.Berechnet = false;
    }

    private void BtnDialogCancel_Click(object sender, RoutedEventArgs e)
    {
        StartFenster.WärmeVisual.IsZeitElementtemperatur = false;
        Close();
    }

    private void BtnLöschen_Click(object sender, RoutedEventArgs e)
    {
        if (!_modell.ZeitabhängigeElementLasten.Remove(LastId.Text)) return;
        Close();
        StartFenster.WärmeVisual.Close();
        StartFenster.WärmeVisual = new WärmemodellVisualisieren(_modell);
        StartFenster.WärmeVisual.Show();
        _modell.Berechnet = false;
    }

    private void LastIdLostFocus(object sender, RoutedEventArgs e)
    {
        if (!_modell.ZeitabhängigeElementLasten.TryGetValue(LastId.Text,
                out var vorhandeneZeitElementlast)) return;

        // vorhandene zeitabhängige Elementlastdefinition
        LastId.Text = vorhandeneZeitElementlast.LastId;
        ElementId.Text = vorhandeneZeitElementlast.ElementId;
        P0.Text = vorhandeneZeitElementlast.P[0].ToString("G2");
        P1.Text = vorhandeneZeitElementlast.P[1].ToString("G2");
        P2.Text = vorhandeneZeitElementlast.P[2].ToString("G2");
        P3.Text = vorhandeneZeitElementlast.P[3].ToString("G2");
        Show();
    }

    private void ElementIdLostFocus(object sender, RoutedEventArgs e)
    {
        if (!_modell.Knoten.TryGetValue(ElementId.Text, out var vorhandenesElement))
        {
            LastId.Text = "";
            ElementId.Text = "";
        }
        else
        {
            ElementId.Text = vorhandenesElement.Id;
            if (LastId.Text != "") return;
            LastId.Text = "zEl_" + ElementId.Text;
            AktuelleId = LastId.Text;
        }
    }
}