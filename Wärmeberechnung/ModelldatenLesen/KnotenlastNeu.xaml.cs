using FE_Berechnungen.Wärmeberechnung.Modelldaten;
using System.Globalization;
using FE_Berechnungen.Wärmeberechnung.ModelldatenAnzeigen;

namespace FE_Berechnungen.Wärmeberechnung.ModelldatenLesen;

public partial class KnotenlastNeu
{
    private readonly FeModell _modell;

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
        this._modell = modell;
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
            try
            {
                if (KnotenId.Text.Length > 0)
                    vorhandeneLast.KnotenId = KnotenId.Text.ToString(CultureInfo.CurrentCulture);
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
            try
            {
                if (KnotenId.Text.Length > 0) knotenId = KnotenId.Text.ToString(CultureInfo.CurrentCulture);
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

    private void KnotenlastIdLostFocus(object sender, RoutedEventArgs e)
    {
        if (!_modell.Lasten.ContainsKey(KnotenlastId.Text))
        {
            KnotenId.Text = "";
            Temperatur.Text = "";
            return;
        }

        // vorhandene Knotenlastdefinition
        if (!_modell.Lasten.TryGetValue(KnotenlastId.Text, out var vorhandeneLast))
            throw new ModellAusnahme("\nKnotenlast '" + KnotenlastId.Text + "' nicht im Modell gefunden");

        KnotenlastId.Text = vorhandeneLast.LastId;
        KnotenId.Text = vorhandeneLast.KnotenId;
        Temperatur.Text = vorhandeneLast.Lastwerte[0].ToString("G3", CultureInfo.CurrentCulture);
    }

    private void BtnLöschen_Click(object sender, RoutedEventArgs e)
    {
        if (!_modell.Lasten.ContainsKey(KnotenlastId.Text)) return;
        _modell.Lasten.Remove(KnotenlastId.Text);
        Close();
        StartFenster.WärmeVisual.Close();

        StartFenster.WärmeVisual = new WärmemodellVisualisieren(_modell);
        StartFenster.WärmeVisual.Show();
        _modell.Berechnet = false;
    }
}