using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Input;
using FE_Berechnungen.Tragwerksberechnung.Modelldaten;
using FE_Berechnungen.Tragwerksberechnung.ModelldatenAnzeigen;
using FEBibliothek.Modell;

namespace FE_Berechnungen.Tragwerksberechnung.ModelldatenLesen;

public partial class LagerNeu
{
    private readonly FeModell _modell;

    public LagerNeu(FeModell modell)
    {
        InitializeComponent();
        _modell = modell;
        Show();
        VorX.Text = "0,00";
        VorY.Text = "0,00";
        VorRot.Text = "0,00";
    }

    public LagerNeu(FeModell modell, double vordefX, double vordefY, double vordefRot)
    {
        InitializeComponent();
        _modell = modell;
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
        _modell.Randbedingungen.TryGetValue(lagerId, out var vorhandenesLager);
        if (vorhandenesLager != null)
        {
            if (KnotenId.Text.Length > 0)
                vorhandenesLager.KnotenId = KnotenId.Text.ToString(CultureInfo.CurrentCulture);
            vorhandenesLager.Festgehalten[0] = false;
            vorhandenesLager.Festgehalten[1] = false;
            vorhandenesLager.Festgehalten[2] = false;

            if (Xfest.IsChecked != null && (bool)Xfest.IsChecked) vorhandenesLager.Festgehalten[0] = true;
            if (Yfest.IsChecked != null && (bool)Yfest.IsChecked) vorhandenesLager.Festgehalten[1] = true;
            if (Rfest.IsChecked != null && (bool)Rfest.IsChecked) vorhandenesLager.Festgehalten[2] = true;
            vorhandenesLager.Typ = 0;
            if (vorhandenesLager.Festgehalten[0]) vorhandenesLager.Typ = Lager.XFixed;
            if (vorhandenesLager.Festgehalten[1]) vorhandenesLager.Typ += Lager.YFixed;
            if (vorhandenesLager.Festgehalten[2]) vorhandenesLager.Typ += Lager.RFixed;
            try
            {
                if (VorX.Text.Length > 0) vorhandenesLager.Vordefiniert[0] = double.Parse(VorX.Text);
                if (VorY.Text.Length > 0) vorhandenesLager.Vordefiniert[1] = double.Parse(VorY.Text);
                if (VorRot.Text.Length > 0) vorhandenesLager.Vordefiniert[2] = double.Parse(VorRot.Text);
            }
            catch (FormatException)
            {
                _ = MessageBox.Show("ungültiges Format in der Eingabe", "neue Knotenlast");
                return;
            }
        }
        // neues Lager
        else
        {
            var vordefiniert = new double[3];
            try
            {
                if (VorX.Text.Length > 0) vordefiniert[0] = double.Parse(VorX.Text);
                if (VorY.Text.Length > 0) vordefiniert[1] = double.Parse(VorY.Text);
                if (VorRot.Text.Length > 0) vordefiniert[2] = double.Parse(VorRot.Text);
            }
            catch (FormatException)
            {
                _ = MessageBox.Show("ungültiges Format in der Eingabe", "neue Knotenlast");
                return;
            }

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
        // vorhandene Lagerdefinition
        _modell.Randbedingungen.TryGetValue(LagerId.Text, out var vorhandenesLager);
        if (vorhandenesLager == null) return;

        LagerId.Text = vorhandenesLager.RandbedingungId;
        KnotenId.Text = vorhandenesLager.KnotenId;
        Xfest.IsChecked = false;
        Yfest.IsChecked = false;
        Rfest.IsChecked = false;
        if (vorhandenesLager.Festgehalten[0]) Xfest.IsChecked = true;
        if (vorhandenesLager.Festgehalten[1]) Yfest.IsChecked = true;
        if (vorhandenesLager.Festgehalten[2]) Rfest.IsChecked = true;
        VorX.Text = vorhandenesLager.Vordefiniert[0].ToString("N2", CultureInfo.CurrentCulture);
        VorY.Text = vorhandenesLager.Vordefiniert[1].ToString("N2", CultureInfo.CurrentCulture);
        VorRot.Text = vorhandenesLager.Vordefiniert[2].ToString("N2", CultureInfo.CurrentCulture);
    }

    private void KnotenIdLostFocus(object sender, RoutedEventArgs e)
    {
        _modell.Knoten.TryGetValue(KnotenId.Text, out var vorhandenerKnoten);
        if (vorhandenerKnoten == null)
        {
            _ = MessageBox.Show("Knoten nicht im Modell gefunden", "neue Knotenlast");
            LagerId.Text = "";
            KnotenId.Text = "";
            return;
        }

        if (LagerId.Text == "") LagerId.Text = "L_" + KnotenId.Text;
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

    private void KnotenPositionNeu(object sender, MouseButtonEventArgs e)
    {
        _modell.Knoten.TryGetValue(KnotenId.Text, out var knoten);
        Debug.Assert(knoten != null, nameof(knoten) + " != null");
        StartFenster.TragwerkVisual.KnotenClick(knoten);
        Close();
    }
}