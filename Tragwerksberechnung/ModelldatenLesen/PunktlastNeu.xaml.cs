using FE_Berechnungen.Tragwerksberechnung.Modelldaten;
using FEBibliothek.Modell;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Windows;

namespace FE_Berechnungen.Tragwerksberechnung.ModelldatenLesen;

public partial class PunktlastNeu
{
    private readonly FeModell modell;
    private readonly PunktlastKeys punktlastKeys; 
    public PunktlastNeu(FeModell modell)
    {
        InitializeComponent();
        this.modell = modell;
        punktlastKeys = new PunktlastKeys(modell);
        punktlastKeys.Show();
        Show();
    }

    public PunktlastNeu(FeModell modell, string last, string element, double px, double py, double offset)
    {
        InitializeComponent();
        this.modell = modell;
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
        if (modell.PunktLasten.Keys.Contains(LastId.Text))
        {
            modell.PunktLasten.TryGetValue(punktlastId, out var last);
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
            modell.PunktLasten.Add(punktlastId, punktLast);
        }
        punktlastKeys?.Close();
        Close();
        StartFenster.tragwerksModell.Close();
    }

    private void BtnDialogCancel_Click(object sender, RoutedEventArgs e)
    {
        punktlastKeys?.Close();
        Close();
    }
    private void PunktlastIdLostFocus(object sender, RoutedEventArgs e)
    {
        if (!modell.PunktLasten.ContainsKey(LastId.Text))
        {
            ElementId.Text = "";
            Px.Text = "";
            Py.Text = "";
            Offset.Text = "";
            return;
        }

        // vorhandene Punktlastdefinition
        modell.PunktLasten.TryGetValue(LastId.Text, out var last);
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
        if (!modell.PunktLasten.Keys.Contains(LastId.Text)) return;
        modell.PunktLasten.Remove(LastId.Text);
        punktlastKeys?.Close();
        Close();
        StartFenster.tragwerksModell.Close();
        punktlastKeys?.Close();
    }
}