using FE_Berechnungen.Wärmeberechnung.Modelldaten;
using FE_Berechnungen.Wärmeberechnung.ModelldatenAnzeigen;

namespace FE_Berechnungen.Wärmeberechnung.ModelldatenLesen;

public partial class RandbedingungNeu
{
    private readonly FeModell _modell;

    public RandbedingungNeu(FeModell modell)
    {
        _modell = modell;
        InitializeComponent();
        Show();
    }

    private void BtnDialogOk_Click(object sender, RoutedEventArgs e)
    {
        var randbedingungId = RandbedingungId.Text;
        var knotenId = KnotenId.Text;
        double temperatur = 0;
        if (randbedingungId == "")
        {
            _ = MessageBox.Show("Randbedingung Id muss definiert sein", "neue Randbedingung");
            return;
        }

        // vorhandene Randbedingung
        if (_modell.Randbedingungen.TryGetValue(randbedingungId, out var vorhandeneRandbedingung))
        {
            try
            {
                if (Temperatur.Text.Length > 0) vorhandeneRandbedingung.Vordefiniert[0] = double.Parse(Temperatur.Text);
            }
            catch (FormatException)
            {
                _ = MessageBox.Show("ungültiges Format in der Eingabe", "neue Linienlast");
                return;
            }
        }

        // neues Randbedingung
        else
        {
            try
            {
                if (Temperatur.Text.Length > 0) temperatur = double.Parse(Temperatur.Text);
            }
            catch (FormatException)
            {
                _ = MessageBox.Show("ungültiges Format in der Eingabe", "neue Linienlast");
                return;
            }

            var randbedingung = new Randbedingung(randbedingungId, knotenId, temperatur)
            {
                RandbedingungId = randbedingungId
            };
            _modell.Randbedingungen.Add(randbedingungId, randbedingung);
        }
        StartFenster.WärmeVisual.Close();
        Close();

        StartFenster.WärmeVisual = new WärmemodellVisualisieren(_modell);
        StartFenster.WärmeVisual.Show();
        _modell.Berechnet = false;
    }
    private void BtnDialogCancel_Click(object sender, RoutedEventArgs e)
    {
        Close();
        StartFenster.WärmeVisual.IsRandbedingung = false;
    }

    private void RandbedingungIdLostFocus(object sender, RoutedEventArgs e)
    {
        if (!_modell.Randbedingungen.ContainsKey(RandbedingungId.Text))
        {
            KnotenId.Text = "";
            Temperatur.Text = "";
            return;
        }

        // vorhandene Randbedingungsdefinitionen
        if (!_modell.Randbedingungen.TryGetValue(RandbedingungId.Text, out var vorhandeneRandbedingung))
        {
            _ = MessageBox.Show("Randbedingung Id muss definiert sein", "neue Randbedingung");
            return;
        }

        KnotenId.Text = vorhandeneRandbedingung.KnotenId;
        Temperatur.Text = vorhandeneRandbedingung.Vordefiniert[0].ToString("G3");

        if (RandbedingungId.Text == "") RandbedingungId.Text = "LL_" + KnotenId.Text;
    }

    private void BtnLöschen_Click(object sender, RoutedEventArgs e)
    {
        if (!_modell.Randbedingungen.ContainsKey(RandbedingungId.Text)) return;
        _modell.Randbedingungen.Remove(RandbedingungId.Text);
        StartFenster.WärmeVisual.Close();
        Close();

        StartFenster.WärmeVisual = new WärmemodellVisualisieren(_modell);
        StartFenster.WärmeVisual.Show();
        _modell.Berechnet = false;
    }
}