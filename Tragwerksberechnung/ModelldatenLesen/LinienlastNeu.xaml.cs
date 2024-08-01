using FE_Berechnungen.Tragwerksberechnung.Modelldaten;
using FE_Berechnungen.Tragwerksberechnung.ModelldatenAnzeigen;
using FEBibliothek.Modell;
using System;
using System.Globalization;
using System.Windows;

namespace FE_Berechnungen.Tragwerksberechnung.ModelldatenLesen;

public partial class LinienlastNeu
{
    private readonly FeModell _modell;

    public LinienlastNeu(FeModell modell)
    {
        InitializeComponent();
        _modell = modell;
        Show();
    }

    public LinienlastNeu(FeModell modell, string last, string element,
        double pxa, double pya, double pxb, double pyb, bool inElement)
    {
        InitializeComponent();
        _modell = modell;
        LastId.Text = last;
        ElementId.Text = element;
        Pxa.Text = pxa.ToString("0.00");
        Pya.Text = pya.ToString("0.00");
        Pxb.Text = pxb.ToString("0.00");
        Pyb.Text = pyb.ToString("0.00");
        InElement.IsChecked = inElement;
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
        _modell.ElementLasten.TryGetValue(linienlastId, out var vorhandeneLinienlast);
        if (vorhandeneLinienlast != null)
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

        StartFenster.TragwerkVisual.Close();
        StartFenster.TragwerkVisual.TragwerkLastenKeys?.Close();
        Close();
        StartFenster.TragwerkVisual = new TragwerkmodellVisualisieren(StartFenster.TragwerksModell);
        StartFenster.TragwerkVisual.Show();
        StartFenster.Berechnet = false;
    }

    private void BtnDialogCancel_Click(object sender, RoutedEventArgs e)
    {
        StartFenster.TragwerkVisual.TragwerkLastenKeys?.Close();
        Close();
    }

    private void LastIdLostFocus(object sender, RoutedEventArgs e)
    {
        // vorhandene Linienlastdefinition
        _modell.ElementLasten.TryGetValue(LastId.Text, out var vorhandeneLinienlast);
        if (vorhandeneLinienlast == null) return;
        LastId.Text = vorhandeneLinienlast.LastId;
        ElementId.Text = vorhandeneLinienlast.ElementId;
        Pxa.Text = vorhandeneLinienlast.Lastwerte[0].ToString("G3", CultureInfo.CurrentCulture);
        Pya.Text = vorhandeneLinienlast.Lastwerte[1].ToString("G3", CultureInfo.CurrentCulture);
        Pxb.Text = vorhandeneLinienlast.Lastwerte[2].ToString("G3", CultureInfo.CurrentCulture);
        Pyb.Text = vorhandeneLinienlast.Lastwerte[3].ToString("G3", CultureInfo.CurrentCulture);
        vorhandeneLinienlast.InElementKoordinatenSystem = InElement.IsChecked != null && (bool)InElement.IsChecked;
    }

    private void ElementIdLostFocus(object sender, RoutedEventArgs e)
    {
        _modell.Elemente.TryGetValue(ElementId.Text, out var vorhandenesElement);
        if (vorhandenesElement == null)
        {
            _ = MessageBox.Show("Element nicht im Modell gefunden", "neue Linienlast");
            LastId.Text = "";
            ElementId.Text = "";
            return;
        }

        if (LastId.Text == "") LastId.Text = "LL_" + ElementId.Text;
    }

    private void BtnLöschen_Click(object sender, RoutedEventArgs e)
    {
        if (!_modell.ElementLasten.Keys.Contains(LastId.Text)) return;
        _modell.ElementLasten.Remove(LastId.Text);
        StartFenster.TragwerkVisual.TragwerkLastenKeys?.Close();
        StartFenster.TragwerkVisual.Close();
        Close();

        StartFenster.TragwerkVisual = new TragwerkmodellVisualisieren(StartFenster.TragwerksModell);
        StartFenster.TragwerkVisual.Show();
        StartFenster.Berechnet = false;
    }
}