using FE_Berechnungen.Tragwerksberechnung.Modelldaten;
using FE_Berechnungen.Tragwerksberechnung.ModelldatenAnzeigen;
using FEBibliothek.Modell;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Windows;

namespace FE_Berechnungen.Tragwerksberechnung.ModelldatenLesen;

public partial class PunktlastNeu
{
    private readonly FeModell _modell;
    public PunktlastNeu(FeModell modell)
    {
        InitializeComponent();
        this._modell = modell;
        Show();
    }

    public PunktlastNeu(FeModell modell, string last, string element, double px, double py, double offset)
    {
        InitializeComponent();
        this._modell = modell;
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

        // vorhandene Linienlast
        if (_modell.PunktLasten.Keys.Contains(LastId.Text))
        {
            _modell.PunktLasten.TryGetValue(punktlastId, out var last);
            Debug.Assert(last != null, nameof(last) + " != null");

            var punktlast = (PunktLast)last;
            if (ElementId.Text.Length > 0) punktlast.ElementId = ElementId.Text.ToString(CultureInfo.CurrentCulture);
            if (Px.Text.Length > 0) punktlast.Lastwerte[0] = double.Parse(Px.Text);
            if (Py.Text.Length > 0) punktlast.Lastwerte[1] = double.Parse(Py.Text);
            if (Offset.Text.Length > 0) punktlast.Offset = double.Parse(Offset.Text);
        }
        // neue Punktlast
        else
        {
            var elementId = "";
            double px = 0, py = 0, offset = 0;
            if (ElementId.Text.Length > 0) elementId = ElementId.Text.ToString(CultureInfo.CurrentCulture);
            if (Px.Text.Length > 0) px = double.Parse(Px.Text);
            if (Py.Text.Length > 0) py = double.Parse(Py.Text);
            if (Offset.Text.Length > 0) offset = double.Parse(Offset.Text);
            var punktLast = new PunktLast(elementId, px, py, offset)
            {
                LastId = punktlastId
            };
            _modell.PunktLasten.Add(punktlastId, punktLast);
        }
        StartFenster.TragwerkVisual.TragwerkLastenKeys?.Close();
        Close();
        StartFenster.TragwerkVisual.Close();

        StartFenster.TragwerkVisual = new TragwerkmodellVisualisieren(StartFenster.TragwerksModell);
        StartFenster.TragwerkVisual.Show();
    }

    private void BtnDialogCancel_Click(object sender, RoutedEventArgs e)
    {
        StartFenster.TragwerkVisual.TragwerkLastenKeys?.Close();
        Close();
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
        _modell.PunktLasten.TryGetValue(LastId.Text, out var last);
        Debug.Assert(last != null, nameof(last) + " != null");

        var punktlast = (PunktLast)last;
        LastId.Text = punktlast.LastId;

        ElementId.Text = punktlast.ElementId;
        Px.Text = punktlast.Lastwerte[0].ToString("G3", CultureInfo.CurrentCulture);
        Py.Text = punktlast.Lastwerte[1].ToString("G3", CultureInfo.CurrentCulture);
        Offset.Text = punktlast.Offset.ToString("G3", CultureInfo.CurrentCulture);
    }
    private void BtnLöschen_Click(object sender, RoutedEventArgs e)
    {
        if (!_modell.PunktLasten.Keys.Contains(LastId.Text)) return;
        _modell.PunktLasten.Remove(LastId.Text);
        StartFenster.TragwerkVisual.TragwerkLastenKeys?.Close();
        Close();
        StartFenster.TragwerkVisual.Close();

        StartFenster.TragwerkVisual = new TragwerkmodellVisualisieren(StartFenster.TragwerksModell);
        StartFenster.TragwerkVisual.Show();
    }
}