using FE_Berechnungen.Tragwerksberechnung.Modelldaten;
using FEBibliothek.Modell;
using FEBibliothek.Modell.abstrakte_Klassen;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Windows;

namespace FE_Berechnungen.Tragwerksberechnung.ModelldatenLesen;

public partial class KnotenlastNeu
{
    private readonly FeModell modell;
    private AbstraktLast vorhandeneKnotenlast;
    private readonly TragwerkLastenKeys lastenKeys;
    public KnotenlastNeu()
    {
        InitializeComponent();
        Show();
    }

    public KnotenlastNeu(FeModell modell)
    {
        InitializeComponent();
        this.modell = modell;
        lastenKeys = new TragwerkLastenKeys(modell);
        lastenKeys.Show();
        Show();
    }
    public KnotenlastNeu(FeModell modell, string last, string knoten,
        double px, double py, double m)
    {
        InitializeComponent();
        this.modell = modell;
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
        if (modell.Lasten.Keys.Contains(LastId.Text))
        {
            modell.Lasten.TryGetValue(knotenlastId, out vorhandeneKnotenlast);
            Debug.Assert(vorhandeneKnotenlast != null, nameof(vorhandeneKnotenlast) + " != null");

            if (KnotenId.Text.Length > 0) vorhandeneKnotenlast.KnotenId = KnotenId.Text.ToString(CultureInfo.CurrentCulture);
            if (Px.Text.Length > 0) vorhandeneKnotenlast.Lastwerte[0] = double.Parse(Px.Text);
            if (Py.Text.Length > 0) vorhandeneKnotenlast.Lastwerte[1] = double.Parse(Py.Text);
            if (M.Text.Length > 0) vorhandeneKnotenlast.Lastwerte[2] = double.Parse(M.Text);
        }
        // neue Knotenlast
        else
        {
            string knotenId = "";
            double px = 0, py = 0, m = 0;
            if (KnotenId.Text.Length > 0) knotenId = KnotenId.Text.ToString(CultureInfo.CurrentCulture);
            if (Px.Text.Length > 0) px = double.Parse(Px.Text);
            if (Py.Text.Length > 0) py = double.Parse(Py.Text);
            if (M.Text.Length > 0) m = double.Parse(M.Text);
            var knotenlast = new KnotenLast(knotenId, px, py, m)
            {
                LastId = knotenlastId
            };
            modell.Lasten.Add(knotenlastId, knotenlast);
        }
        lastenKeys?.Close();
        Close();
        StartFenster.TragwerkVisual.Close();
    }

    private void BtnDialogCancel_Click(object sender, RoutedEventArgs e)
    {
        lastenKeys?.Close();
        Close();
    }

    private void LastIdLostFocus(object sender, RoutedEventArgs e)
    {
        if (!modell.Lasten.ContainsKey(LastId.Text))
        {
            KnotenId.Text = "";
            Px.Text = "";
            Py.Text = "";
            M.Text = "";
            return;
        }

        // vorhandene Knotenlastdefinition
        modell.Lasten.TryGetValue(LastId.Text, out vorhandeneKnotenlast);
        Debug.Assert(vorhandeneKnotenlast != null, nameof(vorhandeneKnotenlast) + " != null"); LastId.Text = "";

        LastId.Text = vorhandeneKnotenlast.LastId;

        KnotenId.Text = vorhandeneKnotenlast.KnotenId;
        Px.Text = vorhandeneKnotenlast.Lastwerte[0].ToString("G3", CultureInfo.CurrentCulture);
        Py.Text = vorhandeneKnotenlast.Lastwerte[1].ToString("G3", CultureInfo.CurrentCulture);
        M.Text = vorhandeneKnotenlast.Lastwerte[2].ToString("G3", CultureInfo.CurrentCulture);
    }
    private void BtnLöschen_Click(object sender, RoutedEventArgs e)
    {
        if (!modell.Lasten.Keys.Contains(LastId.Text)) return;
        modell.Lasten.Remove(LastId.Text);
        lastenKeys?.Close();
        Close();
        StartFenster.TragwerkVisual.Close();
    }
}