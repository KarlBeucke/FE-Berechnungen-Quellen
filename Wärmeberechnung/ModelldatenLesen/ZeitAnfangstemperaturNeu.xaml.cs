using FEBibliothek.Modell;
using System.Windows;

namespace FE_Berechnungen.Wärmeberechnung.ModelldatenLesen;

public partial class ZeitAnfangstemperaturNeu
{
    private readonly FeModell modell;
    private int aktuell;
    public ZeitAnfangstemperaturNeu(FeModell modell)
    {
        InitializeComponent();
        this.modell = modell;
        aktuell = StartFenster.wärmeVisual.zeitintegrationNeu.aktuell;
        if (modell.Zeitintegration.VonStationär)
        {
            StationäreLösung.IsChecked = true;
            KnotenId.Text = "";
            Anfangstemperatur.Text = "";
        }
        else
        {
            var anfang = (Knotenwerte)modell.Zeitintegration.Anfangsbedingungen[aktuell];
            KnotenId.Text = anfang.KnotenId;
            Anfangstemperatur.Text = anfang.Werte[0].ToString("G2");
        }
        Show();
    }
    private void StationäreLösungChecked(object sender, RoutedEventArgs e)
    {
        StationäreLösung.IsChecked = true;
        KnotenId.Text = "";
        Anfangstemperatur.Text = "";
    }
    private void BtnDialogOk_Click(object sender, RoutedEventArgs e)
    {
        if (KnotenId.Text.Length == 0) Close();
        if (StationäreLösung.IsChecked == true)
        {
            modell.Zeitintegration.VonStationär = true;
            modell.Zeitintegration.Anfangsbedingungen.Clear();
            Close();
            return;
        }

        // neue Anfangsbedingung hinzufügen
        if (StartFenster.wärmeVisual.zeitintegrationNeu.aktuell > modell.Zeitintegration.Anfangsbedingungen.Count)
        {
            if (KnotenId.Text == "") return;
            var werte = new double[1];
            werte[0] = double.Parse(Anfangstemperatur.Text);
            var knotenwerte = new Knotenwerte(KnotenId.Text, werte);
            modell.Zeitintegration.Anfangsbedingungen.Add(knotenwerte);
            modell.Zeitintegration.VonStationär = false;
        }
        // vorhandene Anfangsbedingung ändern
        else
        {
            var anfang = (Knotenwerte)modell.Zeitintegration.Anfangsbedingungen[aktuell];
            anfang.KnotenId = KnotenId.Text;
            anfang.Werte[0] = double.Parse(Anfangstemperatur.Text);
        }

        Close();
    }

    private void BtnDialogCancel_Click(object sender, RoutedEventArgs e)
    {
        Close();
        StartFenster.wärmeVisual.zeitintegrationNeu.Close();
    }

    private void BtnDelete_Click(object sender, RoutedEventArgs e)
    {
        modell.Zeitintegration.Anfangsbedingungen.RemoveAt(aktuell+1);
        aktuell = 0;
        if (modell.Zeitintegration.Anfangsbedingungen.Count <= 0)
        {
            Close();
            StartFenster.wärmeVisual.zeitintegrationNeu.Close();
            return;
        }
        var anfang = (Knotenwerte)modell.Zeitintegration.Anfangsbedingungen[aktuell];
        KnotenId.Text = anfang.KnotenId;
        Anfangstemperatur.Text = anfang.Werte[0].ToString("G2");
        StationäreLösung.IsChecked = modell.Zeitintegration.VonStationär;
        Close();
        StartFenster.wärmeVisual.zeitintegrationNeu.Close();
    }
}