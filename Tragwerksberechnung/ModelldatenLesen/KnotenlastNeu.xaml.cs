using FE_Berechnungen.Tragwerksberechnung.Modelldaten;
using FE_Berechnungen.Tragwerksberechnung.ModelldatenAnzeigen;
using FEBibliothek.Modell;
using FEBibliothek.Modell.abstrakte_Klassen;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Windows;

namespace FE_Berechnungen.Tragwerksberechnung.ModelldatenLesen;

public partial class KnotenlastNeu
{
    private readonly FeModell _modell;
    private AbstraktLast _vorhandeneKnotenlast;
    public KnotenlastNeu()
    {
        InitializeComponent();
        Show();
    }

    public KnotenlastNeu(FeModell modell)
    {
        InitializeComponent();
        this._modell = modell;
        Show();
    }
    public KnotenlastNeu(FeModell modell, string last, string knoten, double px, double py, double m)
    {
        InitializeComponent();
        this._modell = modell;
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
        if (_modell.Lasten.Keys.Contains(LastId.Text))
        {
            _modell.Lasten.TryGetValue(knotenlastId, out _vorhandeneKnotenlast);
            Debug.Assert(_vorhandeneKnotenlast != null, nameof(_vorhandeneKnotenlast) + " != null");

            if (KnotenId.Text.Length > 0) _vorhandeneKnotenlast.KnotenId = KnotenId.Text.ToString(CultureInfo.CurrentCulture);
            if (Px.Text.Length > 0) _vorhandeneKnotenlast.Lastwerte[0] = double.Parse(Px.Text);
            if (Py.Text.Length > 0) _vorhandeneKnotenlast.Lastwerte[1] = double.Parse(Py.Text);
            if (M.Text.Length > 0) _vorhandeneKnotenlast.Lastwerte[2] = double.Parse(M.Text);
        }
        // neue Knotenlast
        else
        {
            var knotenId = "";
            double px = 0, py = 0, m = 0;
            if (KnotenId.Text.Length > 0) knotenId = KnotenId.Text.ToString(CultureInfo.CurrentCulture);
            if (Px.Text.Length > 0) px = double.Parse(Px.Text);
            if (Py.Text.Length > 0) py = double.Parse(Py.Text);
            if (M.Text.Length > 0) m = double.Parse(M.Text);
            var knotenlast = new KnotenLast(knotenId, px, py, m)
            {
                LastId = knotenlastId
            };
            _modell.Lasten.Add(knotenlastId, knotenlast);
        }
        StartFenster.TragwerkVisual.TragwerkLastenKeys?.Close();
        Close();
        StartFenster.TragwerkVisual.Close();
        StartFenster.TragwerkVisual = new ModelldatenAnzeigen.TragwerkmodellVisualisieren(StartFenster.TragwerksModell);
        StartFenster.TragwerkVisual.Show();
    }

    private void BtnDialogCancel_Click(object sender, RoutedEventArgs e)
    {
        StartFenster.TragwerkVisual.TragwerkLastenKeys?.Close();
        Close();
    }

    private void LastIdLostFocus(object sender, RoutedEventArgs e)
    {
        if (!_modell.Lasten.ContainsKey(LastId.Text))
        {
            KnotenId.Text = "";
            Px.Text = "";
            Py.Text = "";
            M.Text = "";
            return;
        }

        // vorhandene Knotenlastdefinition
        _modell.Lasten.TryGetValue(LastId.Text, out _vorhandeneKnotenlast);
        Debug.Assert(_vorhandeneKnotenlast != null, nameof(_vorhandeneKnotenlast) + " != null"); LastId.Text = "";

        LastId.Text = _vorhandeneKnotenlast.LastId;

        KnotenId.Text = _vorhandeneKnotenlast.KnotenId;
        Px.Text = _vorhandeneKnotenlast.Lastwerte[0].ToString("G3", CultureInfo.CurrentCulture);
        Py.Text = _vorhandeneKnotenlast.Lastwerte[1].ToString("G3", CultureInfo.CurrentCulture);
        M.Text = _vorhandeneKnotenlast.Lastwerte[2].ToString("G3", CultureInfo.CurrentCulture);
    }
    private void BtnLöschen_Click(object sender, RoutedEventArgs e)
    {
        if (!_modell.Lasten.Keys.Contains(LastId.Text)) return;
        _modell.Lasten.Remove(LastId.Text);
        StartFenster.TragwerkVisual.TragwerkLastenKeys?.Close();
        Close();
        StartFenster.TragwerkVisual.Close();

        StartFenster.TragwerkVisual = new TragwerkmodellVisualisieren(StartFenster.TragwerksModell);
        StartFenster.TragwerkVisual.Show();
    }
}