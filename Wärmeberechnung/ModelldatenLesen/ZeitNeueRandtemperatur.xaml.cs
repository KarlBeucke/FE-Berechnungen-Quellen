using FE_Berechnungen.Wärmeberechnung.Modelldaten;
using FEBibliothek.Modell;
using System;
using System.Windows;

namespace FE_Berechnungen.Wärmeberechnung.ModelldatenLesen
{
    public partial class ZeitNeueRandtemperatur
    {
        private readonly FEModell modell;
        public ZeitNeueRandtemperatur(FEModell modell)
        {
            InitializeComponent();
            this.modell = modell;
            SupportId.Text = string.Empty;
            KnotenId.Text = string.Empty;
            Datei.IsChecked = false;
            Konstant.Text = string.Empty;
            Amplitude.Text = string.Empty;
            Frequenz.Text = string.Empty;
            Winkel.Text = string.Empty;
            Linear.Text = string.Empty;
            Show();
        }
        private void BtnDialogOk_Click(object sender, RoutedEventArgs e)
        {
            var supportId = SupportId.Text;
            var knotenId = KnotenId.Text;
            ZeitabhängigeRandbedingung zeitabhängigeRandbedingung = null;

            if (Datei.IsChecked == true)
            {
                Konstant.Text = string.Empty;
                Amplitude.Text = string.Empty;
                Frequenz.Text = string.Empty;
                Winkel.Text = string.Empty;
                Linear.Text = string.Empty;
                zeitabhängigeRandbedingung =
                    new ZeitabhängigeRandbedingung(knotenId, true)
                    { RandbedingungId = supportId, VariationsTyp = 0, Vordefiniert = new double[1] };
            }
            else if (Konstant.Text.Length != 0)
            {
                Datei.IsChecked = false;
                Amplitude.Text = string.Empty;
                Frequenz.Text = string.Empty;
                Winkel.Text = string.Empty;
                Linear.Text = string.Empty;
                var konstanteTemperatur = double.Parse(Konstant.Text);
                zeitabhängigeRandbedingung =
                    new ZeitabhängigeRandbedingung(knotenId, konstanteTemperatur)
                    { RandbedingungId = supportId, VariationsTyp = 1, Vordefiniert = new double[1] };
            }
            else if ((Amplitude.Text.Length & Frequenz.Text.Length & Winkel.Text.Length) != 0)
            {
                Datei.IsChecked = false;
                Konstant.Text = string.Empty;
                Linear.Text = string.Empty;
                var amplitude = double.Parse(Amplitude.Text);
                var frequency = 2 * Math.PI * double.Parse(Frequenz.Text);
                var phaseAngle = Math.PI / 180 * double.Parse(Winkel.Text);
                zeitabhängigeRandbedingung =
                    new ZeitabhängigeRandbedingung(knotenId, amplitude, frequency, phaseAngle)
                    { RandbedingungId = supportId, VariationsTyp = 2, Vordefiniert = new double[1] };
            }
            else if (Linear.Text.Length != 0)
            {
                Datei.IsChecked = false;
                Konstant.Text = string.Empty;
                Amplitude.Text = string.Empty;
                Frequenz.Text = string.Empty;
                Winkel.Text = string.Empty;

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
                zeitabhängigeRandbedingung = new ZeitabhängigeRandbedingung(knotenId, interval)
                { RandbedingungId = supportId, VariationsTyp = 3, Vordefiniert = new double[1] };
            }

            if (zeitabhängigeRandbedingung != null)
            {
                modell.ZeitabhängigeRandbedingung.Add(supportId, zeitabhängigeRandbedingung);
            }
            Close();
        }

        private void BtnDialogCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}