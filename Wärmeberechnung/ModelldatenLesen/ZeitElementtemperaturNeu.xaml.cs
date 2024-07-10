using System.Diagnostics;
using System.Windows;
using FE_Berechnungen.Wärmeberechnung.Modelldaten;
using FEBibliothek.Modell;
using FEBibliothek.Modell.abstrakte_Klassen;

namespace FE_Berechnungen.Wärmeberechnung.ModelldatenLesen;

public partial class ZeitElementtemperaturNeu
{
    private readonly WärmelastenKeys lastenKeys;
    private readonly FeModell modell;
    private AbstraktZeitabhängigeElementLast vorhandeneLast;

    public ZeitElementtemperaturNeu(FeModell modell)
    {
        this.modell = modell;
        InitializeComponent();
        lastenKeys = new WärmelastenKeys(modell);
        lastenKeys.Show();
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
        if (modell.ZeitabhängigeElementLasten.Keys.Contains(elementlastId))
        {
            modell.ZeitabhängigeElementLasten.TryGetValue(elementlastId, out vorhandeneLast);
            Debug.Assert(vorhandeneLast != null, nameof(vorhandeneLast) + " != null");

            if (ElementId.Text.Length > 0) vorhandeneLast.ElementId = ElementId.Text;
            if (P0.Text.Length > 0) vorhandeneLast.P[0] = double.Parse(P0.Text);
            if (P1.Text.Length > 0) vorhandeneLast.P[1] = double.Parse(P1.Text);
            if (P2.Text.Length > 0) vorhandeneLast.P[2] = double.Parse(P2.Text);
            if (P3.Text.Length > 0) vorhandeneLast.P[3] = double.Parse(P3.Text);
        }
        // neue zeitabhängige Elementlast
        else
        {
            var elementId = "";
            var p = new double[4];
            if (ElementId.Text.Length > 0) elementId = ElementId.Text;
            if (P0.Text.Length > 0) p[0] = double.Parse(P0.Text);
            if (P1.Text.Length > 0) p[1] = double.Parse(P1.Text);
            if (P2.Text.Length > 0) p[2] = double.Parse(P2.Text);
            if (P3.Text.Length > 0) p[3] = double.Parse(P3.Text);
            var zeitabhängigeElementlast = new ZeitabhängigeElementLast(elementId, p)
            {
                LastId = elementlastId
            };
            modell.ZeitabhängigeElementLasten.Add(elementlastId, zeitabhängigeElementlast);
        }

        lastenKeys?.Close();
        Close();
        StartFenster.WärmeVisual.Close();
    }

    private void BtnDialogCancel_Click(object sender, RoutedEventArgs e)
    {
        lastenKeys?.Close();
        Close();
    }

    private void BtnLöschen_Click(object sender, RoutedEventArgs e)
    {
        if (!modell.ZeitabhängigeElementLasten.Keys.Contains(LastId.Text)) return;
        modell.ZeitabhängigeElementLasten.Remove(LastId.Text);
        lastenKeys?.Close();
        Close();
        StartFenster.WärmeVisual.Close();
    }

    private void LastIdLostFocus(object sender, RoutedEventArgs e)
    {
        if (!modell.ZeitabhängigeElementLasten.ContainsKey(LastId.Text))
        {
            ElementId.Text = "";
            P0.Text = "";
            P1.Text = "";
            P2.Text = "";
            P3.Text = "";
            return;
        }

        // vorhandene zeitabhängige Elementlastdefinition
        modell.ZeitabhängigeElementLasten.TryGetValue(LastId.Text, out vorhandeneLast);
        Debug.Assert(vorhandeneLast != null, nameof(vorhandeneLast) + " != null");

        LastId.Text = vorhandeneLast.LastId;

        ElementId.Text = vorhandeneLast.ElementId;
        P0.Text = vorhandeneLast.Lastwerte[0].ToString("G2");
        P1.Text = vorhandeneLast.Lastwerte[1].ToString("G2");
        P2.Text = vorhandeneLast.Lastwerte[2].ToString("G2");
        P3.Text = vorhandeneLast.Lastwerte[3].ToString("G2");
    }
}