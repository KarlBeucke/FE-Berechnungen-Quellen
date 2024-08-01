using FE_Berechnungen.Tragwerksberechnung.Modelldaten;
using FEBibliothek.Modell;
using FEBibliothek.Modell.abstrakte_Klassen;
using System;
using System.Windows;

namespace FE_Berechnungen.Tragwerksberechnung.ModelldatenLesen;

public partial class ZeitKnotenlastNeu
{
    private readonly FeModell _modell;

    public ZeitKnotenlastNeu(FeModell modell)
    {
        InitializeComponent();
        _modell = modell;
        LoadId.Text = string.Empty;
        KnotenId.Text = string.Empty;
        KnotenDof.Text = string.Empty;
        Datei.IsChecked = false;
        Amplitude.Text = string.Empty;
        Frequenz.Text = string.Empty;
        Winkel.Text = string.Empty;
        Linear.Text = string.Empty;
        Show();
    }

    private void BtnDialogOk_Click(object sender, RoutedEventArgs e)
    {
        var loadId = LoadId.Text;
        var knotenId = KnotenId.Text;
        var knotenDof = int.Parse(KnotenDof.Text);
        var zeitabhängigeKnotenlast =
            new ZeitabhängigeKnotenLast(loadId, knotenId, knotenDof, false, false);

        if (Datei.IsChecked == true)
        {
            Amplitude.Text = string.Empty;
            Frequenz.Text = string.Empty;
            Winkel.Text = string.Empty;
            Linear.Text = string.Empty;
            zeitabhängigeKnotenlast.Datei = true;
            zeitabhängigeKnotenlast.VariationsTyp = 0;
            var last = (AbstraktZeitabhängigeKnotenlast)zeitabhängigeKnotenlast;
            _modell.ZeitabhängigeKnotenLasten.Add(loadId, last);
        }
        else if ((Amplitude.Text.Length & Frequenz.Text.Length & Winkel.Text.Length) != 0)
        {
            Linear.Text = string.Empty;
            Datei.IsChecked = false;
            zeitabhängigeKnotenlast.VariationsTyp = 2;
            zeitabhängigeKnotenlast.Amplitude = double.Parse(Amplitude.Text);
            zeitabhängigeKnotenlast.Frequenz = 2 * Math.PI * double.Parse(Frequenz.Text);
            zeitabhängigeKnotenlast.PhasenWinkel = Math.PI / 180 * double.Parse(Winkel.Text);
        }
        else if (Linear.Text.Length != 0)
        {
            Amplitude.Text = string.Empty;
            Frequenz.Text = string.Empty;
            Winkel.Text = string.Empty;
            Datei.IsChecked = false;
            var delimiters = new[] { '\t' };
            var teilStrings = Linear.Text.Split(delimiters);
            var k = 0;
            char[] paarDelimiter = { ';' };
            var interval = new double[2 * (teilStrings.Length - 3)];
            for (var j = 3; j < teilStrings.Length; j++)
            {
                var wertePaar = teilStrings[j].Split(paarDelimiter);
                interval[k] = double.Parse(wertePaar[0]);
                interval[k + 1] = double.Parse(wertePaar[1]);
                k += 2;
            }

            zeitabhängigeKnotenlast.VariationsTyp = 3;
            zeitabhängigeKnotenlast.Intervall = interval;
            if (Bodenanregung.IsChecked == true) zeitabhängigeKnotenlast.Bodenanregung = true;
        }

        _modell.ZeitabhängigeKnotenLasten.Add(loadId, zeitabhängigeKnotenlast);
        Close();
    }

    private void BtnDialogCancel_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}