using FE_Berechnungen.Tragwerksberechnung.Modelldaten;
using FE_Berechnungen.Tragwerksberechnung.ModelldatenAnzeigen;
using System.Globalization;

namespace FE_Berechnungen.Tragwerksberechnung.ModelldatenLesen;

public partial class LinienlastNeu
{
    private readonly FeModell _modell;
    private TragwerkLastenKeys _lastenKeys;
    public string AktuelleId;

    public LinienlastNeu(FeModell modell)
    {
        InitializeComponent();
        _modell = modell;
        AktuelleId = "";
        Show();
    }

    public LinienlastNeu(FeModell modell, AbstraktElementLast linienlast)
    {
        InitializeComponent();
        _modell = modell;
        LastId.Text = linienlast.LastId;
        AktuelleId = linienlast.LastId;
        ElementId.Text = linienlast.ElementId;
        Pxa.Text = linienlast.Lastwerte[0].ToString("0.00");
        Pya.Text = linienlast.Lastwerte[1].ToString("0.00");
        Pxb.Text = linienlast.Lastwerte[2].ToString("0.00");
        Pyb.Text = linienlast.Lastwerte[3].ToString("0.00");
        if (linienlast.InElementKoordinatenSystem) InElement.IsChecked = true;
        Show();
    }

    private void BtnDialogOk_Click(object sender, RoutedEventArgs e)
    {
        var linienlastId = LastId.Text;
        if (linienlastId == "")
        {
            _ = MessageBox.Show("Linienlast Id muss definiert sein", "neue Linienlast");
            return;
        }

        // vorhandene Linienlast
        if (_modell.ElementLasten.TryGetValue(linienlastId, out var vorhandeneLinienlast))
        {
            if (ElementId.Text.Length > 0)
                vorhandeneLinienlast.ElementId = ElementId.Text.ToString(CultureInfo.CurrentCulture);
            vorhandeneLinienlast.InElementKoordinatenSystem = InElement.IsChecked != null && (bool)InElement.IsChecked;
            try
            {
                if (Pxa.Text.Length > 0) vorhandeneLinienlast.Lastwerte[0] = double.Parse(Pxa.Text);
                if (Pya.Text.Length > 0) vorhandeneLinienlast.Lastwerte[1] = double.Parse(Pya.Text);
                if (Pxb.Text.Length > 0) vorhandeneLinienlast.Lastwerte[2] = double.Parse(Pxb.Text);
                if (Pyb.Text.Length > 0) vorhandeneLinienlast.Lastwerte[3] = double.Parse(Pyb.Text);
            }
            catch (FormatException)
            {
                _ = MessageBox.Show("ungültiges Format in der Eingabe", "neue Linienlast");
                return;
            }
        }

        // neue Linienlast
        else
        {
            var inElement = false;
            var elementId = "";
            double pxa = 0, pxb = 0, pya = 0, pyb = 0;
            if (ElementId.Text.Length > 0) elementId = ElementId.Text.ToString(CultureInfo.CurrentCulture);
            _modell.Elemente.TryGetValue(elementId, out var element);
            if (element is Fachwerk)
                throw new ModellAusnahme("Linienlast ungültig für Fachwerk");

            if (InElement.IsChecked != null && (bool)InElement.IsChecked) inElement = true;
            try
            {
                if (Pxa.Text.Length > 0) pxa = double.Parse(Pxa.Text);
                if (Pya.Text.Length > 0) pya = double.Parse(Pya.Text);
                if (Pxb.Text.Length > 0) pxb = double.Parse(Pxb.Text);
                if (Pyb.Text.Length > 0) pyb = double.Parse(Pyb.Text);
            }
            catch (FormatException)
            {
                _ = MessageBox.Show("ungültiges Format in der Eingabe", "neue Linienlast");
                return;
            }

            var linienlast = new LinienLast(elementId, pxa, pya, pxb, pyb, inElement)
            {
                LastId = linienlastId
            };
            _modell.ElementLasten.Add(linienlastId, linienlast);
        }
        if (AktuelleId != LastId.Text) _modell.ElementLasten.Remove(AktuelleId);

        Close();
        StartFenster.TragwerkVisual.Close();
        StartFenster.TragwerkVisual = new TragwerkmodellVisualisieren(_modell);
        StartFenster.TragwerkVisual.Show();
        _modell.Berechnet = false;
    }

    private void BtnDialogCancel_Click(object sender, RoutedEventArgs e)
    {
        Close();
        StartFenster.TragwerkVisual.IsLinienlast = false;
    }


    private void LastIdGotFocus(object sender, RoutedEventArgs e)
    {
        _lastenKeys = new TragwerkLastenKeys(_modell) { Topmost = true, Owner = (Window)Parent };
        _lastenKeys.Show();
        _lastenKeys.Focus();
    }
    private void LastIdLostFocus(object sender, RoutedEventArgs e)
    {
        _lastenKeys?.Close();
        if (!_modell.ElementLasten.TryGetValue(LastId.Text, out var vorhandeneLinienlast)) return;

        // vorhandene Linienlastdefinition
        LastId.Text = vorhandeneLinienlast.LastId;
        ElementId.Text = vorhandeneLinienlast.ElementId;
        Pxa.Text = vorhandeneLinienlast.Lastwerte[0].ToString("G3", CultureInfo.CurrentCulture);
        Pya.Text = vorhandeneLinienlast.Lastwerte[1].ToString("G3", CultureInfo.CurrentCulture);
        Pxb.Text = vorhandeneLinienlast.Lastwerte[2].ToString("G3", CultureInfo.CurrentCulture);
        Pyb.Text = vorhandeneLinienlast.Lastwerte[3].ToString("G3", CultureInfo.CurrentCulture);
        vorhandeneLinienlast.InElementKoordinatenSystem = InElement.IsChecked != null && (bool)InElement.IsChecked;

        if (AktuelleId != LastId.Text) _modell.ElementLasten.Remove(LastId.Text);

    }

    private void ElementIdLostFocus(object sender, RoutedEventArgs e)
    {
        if (!_modell.Elemente.TryGetValue(ElementId.Text, out var vorhandenesElement))
        {
            _ = MessageBox.Show("Element nicht im Modell gefunden", "neue Linienlast");
            LastId.Text = "";
            ElementId.Text = "";
        }

        else
        {
            if (vorhandenesElement is Fachwerk)
                throw new ModellAusnahme("Linienlast ungültig für Fachwerkstab");
            ElementId.Text = vorhandenesElement.ElementId;
            if (LastId.Text != "") return;
            LastId.Text = "LL_" + ElementId.Text;
            AktuelleId = LastId.Text;
        }
    }

    private void BtnLöschen_Click(object sender, RoutedEventArgs e)
    {
        if (!_modell.ElementLasten.Remove(LastId.Text, out _)) return;
        Close();
        StartFenster.TragwerkVisual.Close();

        StartFenster.TragwerkVisual = new TragwerkmodellVisualisieren(_modell);
        StartFenster.TragwerkVisual.Show();
        _modell.Berechnet = false;
    }
}