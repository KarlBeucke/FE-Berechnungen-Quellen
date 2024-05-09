using FEBibliothek.Modell;
using FEBibliothek.Modell.abstrakte_Klassen;
using System.Diagnostics;
using System.Linq;
using System.Windows;

namespace FE_Berechnungen.Wärmeberechnung.ModelldatenLesen;

public partial class RandbdingungNeu
{
    private readonly FeModell modell;
    private AbstraktRandbedingung vorhandeneRandbedingung;
    private readonly RandbedingungenKeys randbedingungenKeys;

    public RandbdingungNeu(FeModell modell)
    {
        this.modell = modell;
        InitializeComponent();
        randbedingungenKeys = new RandbedingungenKeys(modell);
        randbedingungenKeys.Show();
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
        if (modell.Randbedingungen.Keys.Contains(randbedingungId))
        {
            modell.Randbedingungen.TryGetValue(randbedingungId, out vorhandeneRandbedingung);
            Debug.Assert(vorhandeneRandbedingung != null, nameof(vorhandeneRandbedingung) + " != null");

            if (Temperatur.Text.Length > 0) vorhandeneRandbedingung.Vordefiniert[0] = double.Parse(Temperatur.Text);
        }
        // neues Randbedingung
        else
        {
            if (Temperatur.Text.Length > 0)
                temperatur = double.Parse(Temperatur.Text);

            var randbedingung = new Modelldaten.Randbedingung(randbedingungId, knotenId, temperatur)
            {
                RandbedingungId = randbedingungId
            };
            modell.Randbedingungen.Add(randbedingungId, randbedingung);
        }
        randbedingungenKeys?.Close();
        Close();
        StartFenster.WärmeVisual.Close();
    }

    private void BtnDialogCancel_Click(object sender, RoutedEventArgs e)
    {
        randbedingungenKeys?.Close();
        Close();
    }

    private void BtnLöschen_Click(object sender, RoutedEventArgs e)
    {
        if (!modell.Randbedingungen.Keys.Contains(RandbedingungId.Text)) return;
        modell.Randbedingungen.Remove(RandbedingungId.Text);
        randbedingungenKeys?.Close();
        Close();
        StartFenster.WärmeVisual.Close();
    }

    private void RandbedingungIdLostFocus(object sender, RoutedEventArgs e)
    {
        if (!modell.Randbedingungen.ContainsKey(RandbedingungId.Text))
        {
            KnotenId.Text = "";
            Temperatur.Text = "";
            return;
        }

        // vorhandene Randbedingungsdefinitionen
        modell.Randbedingungen.TryGetValue(RandbedingungId.Text, out vorhandeneRandbedingung);
        Debug.Assert(vorhandeneRandbedingung != null, nameof(vorhandeneRandbedingung) + " != null");
        KnotenId.Text = vorhandeneRandbedingung.KnotenId;
        Temperatur.Text = vorhandeneRandbedingung.Vordefiniert[0].ToString("G3");
    }
}