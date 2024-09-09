using FE_Berechnungen.Wärmeberechnung.Modelldaten;
using System.Diagnostics;
using System.Globalization;

namespace FE_Berechnungen.Wärmeberechnung.ModelldatenLesen;

public partial class KnotenlastNeu
{
    private readonly WärmelastenKeys lastenKeys;
    private readonly FeModell modell;
    private AbstraktLast vorhandeneLast;

    public KnotenlastNeu()
    {
        InitializeComponent();
        Show();
    }

    public KnotenlastNeu(FeModell modell)
    {
        this.modell = modell;
        InitializeComponent();
        lastenKeys = new WärmelastenKeys(modell);
        lastenKeys.Show();
        Show();
    }

    public KnotenlastNeu(FeModell modell, string last, string knoten, double t)
    {
        InitializeComponent();
        this.modell = modell;
        KnotenlastId.Text = last;
        KnotenId.Text = knoten;
        Temperatur.Text = t.ToString("0.00");
        Show();
    }

    private void BtnDialogOk_Click(object sender, RoutedEventArgs e)
    {
        var knotenlastId = KnotenlastId.Text;
        if (knotenlastId == "")
        {
            _ = MessageBox.Show("Knotenlast Id muss definiert sein", "neue Knotenlast");
            return;
        }

        // vorhandene Knotenlast
        if (modell.Lasten.Keys.Contains(KnotenlastId.Text))
        {
            modell.Lasten.TryGetValue(knotenlastId, out vorhandeneLast);
            Debug.Assert(vorhandeneLast != null, nameof(vorhandeneLast) + " != null");

            if (KnotenId.Text.Length > 0) vorhandeneLast.KnotenId = KnotenId.Text.ToString(CultureInfo.CurrentCulture);
            if (Temperatur.Text.Length > 0) vorhandeneLast.Lastwerte[0] = double.Parse(Temperatur.Text);
        }
        // neue Knotenlast
        else
        {
            var knotenId = "";
            var t = new double[1];
            if (KnotenId.Text.Length > 0) knotenId = KnotenId.Text.ToString(CultureInfo.CurrentCulture);
            if (Temperatur.Text.Length > 0) t[0] = double.Parse(Temperatur.Text);
            var knotenlast = new KnotenLast(knotenlastId, knotenId, t);
            modell.Lasten.Add(knotenlastId, knotenlast);
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

    private void KnotenlastIdLostFocus(object sender, RoutedEventArgs e)
    {
        if (!modell.Lasten.ContainsKey(KnotenlastId.Text))
        {
            KnotenId.Text = "";
            Temperatur.Text = "";
            return;
        }

        // vorhandene Knotenlastdefinition
        modell.Lasten.TryGetValue(KnotenlastId.Text, out vorhandeneLast);
        Debug.Assert(vorhandeneLast != null, nameof(vorhandeneLast) + " != null");
        KnotenlastId.Text = "";

        KnotenlastId.Text = vorhandeneLast.LastId;

        KnotenId.Text = vorhandeneLast.KnotenId;
        Temperatur.Text = vorhandeneLast.Lastwerte[0].ToString("G3", CultureInfo.CurrentCulture);
    }

    private void BtnLöschen_Click(object sender, RoutedEventArgs e)
    {
        if (!modell.Lasten.Keys.Contains(KnotenlastId.Text)) return;
        modell.Lasten.Remove(KnotenlastId.Text);
        lastenKeys?.Close();
        Close();
        StartFenster.WärmeVisual.Close();
    }
}