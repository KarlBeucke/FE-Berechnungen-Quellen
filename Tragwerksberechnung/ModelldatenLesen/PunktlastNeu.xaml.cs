using FE_Berechnungen.Tragwerksberechnung.Modelldaten;
using FE_Berechnungen.Tragwerksberechnung.ModelldatenAnzeigen;
using System.Globalization;

namespace FE_Berechnungen.Tragwerksberechnung.ModelldatenLesen;

public partial class PunktlastNeu
{
    private readonly FeModell _modell;

    public PunktlastNeu(FeModell modell)
    {
        InitializeComponent();
        _modell = modell;
        Show();
    }

    public PunktlastNeu(FeModell modell, string last, string element, double px, double py, double offset)
    {
        InitializeComponent();
        _modell = modell;
        LastId.Text = last;
        ElementId.Text = element;
        Px.Text = px.ToString("0.00");
        Py.Text = py.ToString("0.00");
        Offset.Text = offset.ToString("0.00");
        Show();
    }

    private void BtnDialogOk_Click(object sender, RoutedEventArgs e)
    {
        var punktlastId = LastId.Text;
        if (punktlastId == "")
        {
            _ = MessageBox.Show("Punktlast Id muss definiert sein", "neue Punktlast");
            return;
        }

        // vorhandene Punktlast
        if (_modell.PunktLasten.Keys.Contains(LastId.Text))
        {
            _modell.PunktLasten.TryGetValue(punktlastId, out var vorhandenePunktlast);
            if (vorhandenePunktlast != null)
            {
                if (ElementId.Text.Length > 0)
                    vorhandenePunktlast.ElementId = ElementId.Text.ToString(CultureInfo.CurrentCulture);
                try
                {
                    if (Px.Text.Length > 0) vorhandenePunktlast.Lastwerte[0] = double.Parse(Px.Text);
                    if (Py.Text.Length > 0) vorhandenePunktlast.Lastwerte[1] = double.Parse(Py.Text);
                    if (Offset.Text.Length > 0) vorhandenePunktlast.Offset = double.Parse(Offset.Text);
                }
                catch (FormatException)
                {
                    _ = MessageBox.Show("ungültiges Format in der Eingabe", "neue Punktlast");
                    return;
                }
            }
        }
        // neue Punktlast
        else
        {
            var elementId = "";
            double px = 0, py = 0, offset = 0;
            if (ElementId.Text.Length > 0) elementId = ElementId.Text.ToString(CultureInfo.CurrentCulture);
            _modell.Elemente.TryGetValue(elementId, out var element);
            if (element is Fachwerk)
                throw new ModellAusnahme(" Punktlast ungültig für Fachwerk");
            try
            {
                if (Px.Text.Length > 0) px = double.Parse(Px.Text);
                if (Py.Text.Length > 0) py = double.Parse(Py.Text);
                if (Offset.Text.Length > 0) offset = double.Parse(Offset.Text);
            }
            catch (FormatException)
            {
                _ = MessageBox.Show("ungültiges Format in der Eingabe", "neue Punktlast");
                return;
            }

            var punktLast = new PunktLast(elementId, px, py, offset)
            {
                LastId = punktlastId
            };
            _modell.PunktLasten.Add(punktlastId, punktLast);
        }

        StartFenster.TragwerkVisual.TragwerkLastenKeys?.Close();
        Close();
        StartFenster.TragwerkVisual.Close();

        StartFenster.TragwerkVisual = new TragwerkmodellVisualisieren(_modell);
        StartFenster.TragwerkVisual.Show();
        StartFenster.Berechnet = false;
    }

    private void BtnDialogCancel_Click(object sender, RoutedEventArgs e)
    {
        StartFenster.TragwerkVisual.TragwerkLastenKeys?.Close();
        Close();
        StartFenster.TragwerkVisual.IsPunktlast = false;
    }

    private void PunktlastIdLostFocus(object sender, RoutedEventArgs e)
    {
        if (!_modell.PunktLasten.ContainsKey(LastId.Text))
        {
            ElementId.Text = "";
            Px.Text = "";
            Py.Text = "";
            Offset.Text = "";
            return;
        }

        // vorhandene Punktlastdefinition
        _modell.PunktLasten.TryGetValue(LastId.Text, out var vorhandenePunktlast);
        if (vorhandenePunktlast == null) return;
        LastId.Text = vorhandenePunktlast.LastId;

        ElementId.Text = vorhandenePunktlast.ElementId;
        Px.Text = vorhandenePunktlast.Lastwerte[0].ToString("G3", CultureInfo.CurrentCulture);
        Py.Text = vorhandenePunktlast.Lastwerte[1].ToString("G3", CultureInfo.CurrentCulture);
        Offset.Text = vorhandenePunktlast.Offset.ToString("G3", CultureInfo.CurrentCulture);
    }

    private void ElementIdLostFocus(object sender, RoutedEventArgs e)
    {
        _modell.Elemente.TryGetValue(ElementId.Text, out var vorhandenesElement);
        switch (vorhandenesElement)
        {
            case null:
                _ = MessageBox.Show("Element nicht im Modell gefunden", "neue Linienlast");
                LastId.Text = "";
                ElementId.Text = "";
                return;
            case Fachwerk:
                throw new ModellAusnahme(" Punktlast ungültig für Fachwerkstab");
        }

        if (LastId.Text == "") LastId.Text = "PL_" + ElementId.Text;
    }

    private void BtnLöschen_Click(object sender, RoutedEventArgs e)
    {
        if (!_modell.PunktLasten.Keys.Contains(LastId.Text)) return;
        _modell.PunktLasten.Remove(LastId.Text);
        StartFenster.TragwerkVisual.TragwerkLastenKeys?.Close();
        Close();
        StartFenster.TragwerkVisual.Close();

        StartFenster.TragwerkVisual = new TragwerkmodellVisualisieren(_modell);
        StartFenster.TragwerkVisual.Show();
        StartFenster.Berechnet = false;
    }
}