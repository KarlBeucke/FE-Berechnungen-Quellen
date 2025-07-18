using FE_Berechnungen.Wärmeberechnung.Modelldaten;
using FE_Berechnungen.Wärmeberechnung.ModelldatenAnzeigen;
using System.Globalization;

namespace FE_Berechnungen.Wärmeberechnung.ModelldatenLesen;

public partial class KnotenlastNeu
{
    private readonly FeModell _modell;
    public string AktuelleId;

    public KnotenlastNeu()
    {
        InitializeComponent();
        Show();
    }

    public KnotenlastNeu(FeModell modell)
    {
        _modell = modell;
        InitializeComponent();
        Show();
    }

    public KnotenlastNeu(FeModell modell, string last, string knoten, double t)
    {
        InitializeComponent();
        _modell = modell;
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
        if (_modell.Lasten.TryGetValue(knotenlastId, out var vorhandeneLast))
        {
            if (KnotenId.Text.Length > 0)
                vorhandeneLast.KnotenId = KnotenId.Text.ToString(CultureInfo.CurrentCulture);
            try
            {
                if (Temperatur.Text.Length > 0) vorhandeneLast.Lastwerte[0] = double.Parse(Temperatur.Text);
            }
            catch (FormatException)
            {
                _ = MessageBox.Show("ungültiges Format in der Eingabe", "neue Knotenlast");
                return;
            }
        }

        // neue Knotenlast
        else
        {
            var knotenId = "";
            var t = new double[1];
            if (KnotenId.Text.Length > 0) knotenId = KnotenId.Text.ToString(CultureInfo.CurrentCulture);

            try
            {
                if (Temperatur.Text.Length > 0) t[0] = double.Parse(Temperatur.Text);
            }
            catch (FormatException)
            {
                _ = MessageBox.Show("ungültiges Format in der Eingabe", "neue Knotenlast");
                return;
            }

            var knotenlast = new KnotenLast(knotenlastId, knotenId, t);
            _modell.Lasten.Add(knotenlastId, knotenlast);
        }

        Close();
        StartFenster.WärmeVisual.Close();
        StartFenster.WärmeVisual = new WärmemodellVisualisieren(_modell);
        StartFenster.WärmeVisual.Show();
        _modell.Berechnet = false;
    }

    private void BtnDialogCancel_Click(object sender, RoutedEventArgs e)
    {
        Close();
        StartFenster.WärmeVisual.IsKnotenlast = false;
    }

    private void KnotenIdLostFocus(object sender, RoutedEventArgs e)
    {
        _modell.Knoten.TryGetValue(KnotenId.Text, out var vorhandenerKnoten);
        if (vorhandenerKnoten == null)
        {
            _ = MessageBox.Show("Knoten nicht im Modell gefunden", "neue Knotenlast");
            KnotenId.Text = "";
            KnotenlastId.Text = "";
            return;
        }

        // vorhandene Knotenlastdefinition
        if (KnotenlastId.Text == "") KnotenlastId.Text = "KL_" + KnotenId.Text;
    }

    private void BtnLöschen_Click(object sender, RoutedEventArgs e)
    {
        if (!_modell.Lasten.Remove(KnotenlastId.Text)) return;
        Close();
        StartFenster.WärmeVisual.Close();

        StartFenster.WärmeVisual = new WärmemodellVisualisieren(_modell);
        StartFenster.WärmeVisual.Show();
        _modell.Berechnet = false;
    }

    private void KnotenPositionNeu(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        _modell.Knoten.TryGetValue(KnotenId.Text, out var knoten);
        if (knoten == null) { _ = MessageBox.Show("Knoten nicht im Modell gefunden", "neue Knotenlast"); return; }
        StartFenster.WärmeVisual.KnotenClick(knoten);
        Close();
        _modell.Berechnet = false;
    }
}