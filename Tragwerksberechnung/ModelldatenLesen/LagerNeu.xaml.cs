using FE_Berechnungen.Tragwerksberechnung.Modelldaten;
using FE_Berechnungen.Tragwerksberechnung.ModelldatenAnzeigen;
using FEBibliothek.Modell;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Windows;

namespace FE_Berechnungen.Tragwerksberechnung.ModelldatenLesen;

public partial class LagerNeu
{
    private readonly FeModell _modell;
    public LagerNeu(FeModell modell)
    {
        InitializeComponent();
        this._modell = modell;
        Show();
        LagerId.Text = string.Empty;
        KnotenId.Text = string.Empty;
        Yfest.IsChecked = true;
        VorX.Text = "0,00";
        VorY.Text = "0,00";
        VorRot.Text = "0,00";
    }

    public LagerNeu(FeModell modell, double vordefX, double vordefY, double vordefRot)
    {
        InitializeComponent();
        this._modell = modell;
        VorX.Text = vordefX.ToString("0.00");
        VorY.Text = vordefY.ToString("0.00");
        VorRot.Text = vordefRot.ToString("0.00");
        Show();
    }

    private void BtnDialogOk_Click(object sender, RoutedEventArgs e)
    {
        var lagerId = LagerId.Text;
        if (lagerId == "")
        {
            _ = MessageBox.Show("Lager Id muss definiert sein", "neues Lager");
            return;
        }

        // vorhandenes Lager
        if (_modell.Randbedingungen.Keys.Contains(lagerId))
        {
            _modell.Randbedingungen.TryGetValue(lagerId, out var lager);
            Debug.Assert(lager != null, nameof(lager) + " != null");

            if (KnotenId.Text.Length > 0) lager.KnotenId = KnotenId.Text.ToString(CultureInfo.CurrentCulture);

            lager.Festgehalten[0] = false;
            lager.Festgehalten[1] = false;
            lager.Festgehalten[2] = false;
            if (Xfest.IsChecked != null && (bool)Xfest.IsChecked) lager.Festgehalten[0] = true;
            if (Yfest.IsChecked != null && (bool)Yfest.IsChecked) lager.Festgehalten[1] = true;
            if (Rfest.IsChecked != null && (bool)Rfest.IsChecked) lager.Festgehalten[2] = true;
            lager.Typ = 0;
            if (lager.Festgehalten[0]) lager.Typ = Lager.XFixed;
            if (lager.Festgehalten[1]) lager.Typ += Lager.YFixed;
            if (lager.Festgehalten[2]) lager.Typ += Lager.RFixed;

            if (VorX.Text.Length > 0) lager.Vordefiniert[0] = double.Parse(VorX.Text);
            if (VorY.Text.Length > 0) lager.Vordefiniert[1] = double.Parse(VorY.Text);
            if (VorRot.Text.Length > 0) lager.Vordefiniert[2] = double.Parse(VorRot.Text);
        }
        // neues Lager
        else
        {
            var vordefiniert = new double[3];
            if (VorX.Text.Length > 0) vordefiniert[0] = double.Parse(VorX.Text);
            if (VorY.Text.Length > 0) vordefiniert[1] = double.Parse(VorY.Text);
            if (VorRot.Text.Length > 0) vordefiniert[2] = double.Parse(VorRot.Text);
            var typ = 0;
            if (Xfest.IsChecked != null && (bool)Xfest.IsChecked) typ = Lager.XFixed;
            if (Yfest.IsChecked != null && (bool)Yfest.IsChecked) typ += Lager.YFixed;
            if (Rfest.IsChecked != null && (bool)Rfest.IsChecked) typ += Lager.RFixed;
            var lager = new Lager(KnotenId.Text, typ, vordefiniert, _modell) { RandbedingungId = lagerId };
            _modell.Randbedingungen.Add(lagerId, lager);
        }

        Close();
        StartFenster.TragwerkVisual.Close();

        StartFenster.TragwerkVisual = new TragwerkmodellVisualisieren(StartFenster.TragwerksModell);
        StartFenster.TragwerkVisual.Show();
    }
    private void BtnDialogCancel_Click(object sender, RoutedEventArgs e)
    {
        StartFenster.TragwerkVisual.LagerKeys?.Close();
        Close();
    }

    private void LagerIdLostFocus(object sender, RoutedEventArgs e)
    {
        if (!_modell.Randbedingungen.ContainsKey(LagerId.Text))
        {
            KnotenId.Text = "";
            Xfest.IsChecked = false; Yfest.IsChecked = false; Rfest.IsChecked = false;
            return;
        }

        // vorhandene Lagerdefinition
        _modell.Randbedingungen.TryGetValue(LagerId.Text, out var lager);
        Debug.Assert(lager != null, nameof(lager) + " != null");

        LagerId.Text = lager.RandbedingungId;
        KnotenId.Text = lager.KnotenId;
        Xfest.IsChecked = false; Yfest.IsChecked = false; Rfest.IsChecked = false;
        if (lager.Festgehalten[0]) Xfest.IsChecked = true;
        if (lager.Festgehalten[1]) Yfest.IsChecked = true;
        if (lager.Festgehalten[2]) Rfest.IsChecked = true;
        VorX.Text = lager.Vordefiniert[0].ToString("N2", CultureInfo.CurrentCulture);
        VorY.Text = lager.Vordefiniert[1].ToString("N2", CultureInfo.CurrentCulture);
        VorRot.Text = lager.Vordefiniert[2].ToString("N2", CultureInfo.CurrentCulture);
    }
    private void BtnLöschen_Click(object sender, RoutedEventArgs e)
    {
        if (!_modell.Randbedingungen.Keys.Contains(LagerId.Text)) return;
        _modell.Randbedingungen.Remove(LagerId.Text);
        Close();
        StartFenster.TragwerkVisual.Close();

        StartFenster.TragwerkVisual = new TragwerkmodellVisualisieren(StartFenster.TragwerksModell);
        StartFenster.TragwerkVisual.Show();
    }

    private void KnotenPositionNeu(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        _modell.Knoten.TryGetValue(KnotenId.Text, out var knoten);
        Debug.Assert(knoten != null, nameof(knoten) + " != null");
        StartFenster.TragwerkVisual.KnotenNeu(knoten);
        Close();
    }
}