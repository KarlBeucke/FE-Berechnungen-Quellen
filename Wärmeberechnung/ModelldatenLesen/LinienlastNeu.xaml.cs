using FE_Berechnungen.Wärmeberechnung.Modelldaten;
using FE_Berechnungen.Wärmeberechnung.ModelldatenAnzeigen;
using System.Globalization;

namespace FE_Berechnungen.Wärmeberechnung.ModelldatenLesen;

public partial class LinienlastNeu
{
    private readonly FeModell _modell;

    public LinienlastNeu(FeModell modell)
    {
        _modell = modell;
        InitializeComponent();
        Show();
    }

    private void BtnDialogOk_Click(object sender, RoutedEventArgs e)
    {
        var linienlastId = LinienlastId.Text;
        if (linienlastId == "")
        {
            _ = MessageBox.Show("Linienlast Id muss definiert sein", "neue Linienlast");
            return;
        }

        // vorhandene Linienlast
        if (_modell.LinienLasten.TryGetValue(linienlastId, out var vorhandeneLinienlast))
        {
            try
            {
                if (StartknotenId.Text.Length > 0)
                    vorhandeneLinienlast.StartKnotenId = StartknotenId.Text.ToString(CultureInfo.CurrentCulture);
                if (Start.Text.Length > 0) vorhandeneLinienlast.Lastwerte[0] = double.Parse(Start.Text);
                if (EndknotenId.Text.Length > 0)
                    vorhandeneLinienlast.EndKnotenId = EndknotenId.Text.ToString(CultureInfo.CurrentCulture);
                if (End.Text.Length > 0) vorhandeneLinienlast.Lastwerte[1] = double.Parse(End.Text);
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
            var startknotenId = "";
            var endknotenId = "";
            var t = new double[2];
            if (StartknotenId.Text.Length > 0) startknotenId = StartknotenId.Text.ToString(CultureInfo.CurrentCulture);
            if (EndknotenId.Text.Length > 0) endknotenId = EndknotenId.Text.ToString(CultureInfo.CurrentCulture);
            try
            {
                if (Start.Text.Length > 0) t[0] = double.Parse(Start.Text);
                if (End.Text.Length > 0) t[1] = double.Parse(End.Text);
            }
            catch (FormatException)
            {
                _ = MessageBox.Show("ungültiges Format in der Eingabe", "neue Linienlast");
                return;
            }

            var linienlast = new LinienLast(startknotenId, endknotenId, t)
            {
                LastId = linienlastId
            };
            _modell.LinienLasten.Add(linienlastId, linienlast);
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
        StartFenster.WärmeVisual.IsLinienlast = false;
    }

    private void LinienlastIdLostFocus(object sender, RoutedEventArgs e)
    {
        // vorhandene Linienlastdefinition
        if (!_modell.LinienLasten.TryGetValue(LinienlastId.Text, out var vorhandeneLinienlast))
        {
            _ = MessageBox.Show("Linienlast '" + LinienlastId.Text + "' nicht im Modell gefunden", "neue Linienlast");
            return;
        }

        LinienlastId.Text = vorhandeneLinienlast.LastId;
        StartknotenId.Text = vorhandeneLinienlast.StartKnotenId;
        EndknotenId.Text = vorhandeneLinienlast.EndKnotenId;
        try
        {
            Start.Text = vorhandeneLinienlast.Lastwerte[0].ToString("G3", CultureInfo.CurrentCulture);
            End.Text = vorhandeneLinienlast.Lastwerte[1].ToString("G3", CultureInfo.CurrentCulture);
        }
        catch (FormatException)
        {
            _ = MessageBox.Show("ungültiges Format in der Eingabe", "neue Linienlast");
            return;
        }
    }

    private void BtnLöschen_Click(object sender, RoutedEventArgs e)
    {
        _modell.LinienLasten.Remove(LinienlastId.Text);
        StartFenster.WärmeVisual.Close();
        Close();

        StartFenster.WärmeVisual = new WärmemodellVisualisieren(_modell);
        StartFenster.WärmeVisual.Show();
        _modell.Berechnet = false;
    }
}