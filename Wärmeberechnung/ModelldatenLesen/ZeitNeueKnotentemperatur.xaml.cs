using FE_Berechnungen.Wärmeberechnung.Modelldaten;
using FEBibliothek.Modell;
using System;
using System.Windows;

namespace FE_Berechnungen.Wärmeberechnung.ModelldatenLesen
{
    public partial class ZeitNeueKnotentemperatur
    {
        private readonly FEModell modell;

        public ZeitNeueKnotentemperatur(FEModell modell)
        {
            InitializeComponent();
            this.modell = modell;
            LoadId.Text = string.Empty;
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
            var loadId = LoadId.Text;
            var knotenId = KnotenId.Text;
            ZeitabhängigeKnotenLast zeitabhängigeKnotentemperatur = null;

            if (Datei.IsChecked == true)
            {
                Konstant.Text = string.Empty;
                Amplitude.Text = string.Empty;
                Frequenz.Text = string.Empty;
                Winkel.Text = string.Empty;
                Linear.Text = string.Empty;
                zeitabhängigeKnotentemperatur =
                    new ZeitabhängigeKnotenLast(knotenId, true) { LastId = loadId, VariationsTyp = 0 };
            }
            else if (Konstant.Text.Length != 0)
            {
                Amplitude.Text = string.Empty;
                Frequenz.Text = string.Empty;
                Winkel.Text = string.Empty;
                Linear.Text = string.Empty;
                Datei.IsChecked = false;
                var konstanteTemperatur = double.Parse(Konstant.Text);
                zeitabhängigeKnotentemperatur =
                    new ZeitabhängigeKnotenLast(knotenId, konstanteTemperatur) { LastId = loadId, VariationsTyp = 1 };
            }
            else if ((Amplitude.Text.Length & Frequenz.Text.Length & Winkel.Text.Length) != 0)
            {
                Konstant.Text = string.Empty;
                Linear.Text = string.Empty;
                Datei.IsChecked = false;
                var amplitude = double.Parse(Amplitude.Text);
                var frequency = 2 * Math.PI * double.Parse(Frequenz.Text);
                var phaseAngle = Math.PI / 180 * double.Parse(Winkel.Text);
                zeitabhängigeKnotentemperatur =
                    new ZeitabhängigeKnotenLast(knotenId, amplitude, frequency, phaseAngle) { LastId = loadId, VariationsTyp = 2 };
            }
            else if (Linear.Text.Length != 0)
            {
                Konstant.Text = string.Empty;
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

                zeitabhängigeKnotentemperatur = new ZeitabhängigeKnotenLast(knotenId, interval) { LastId = loadId, VariationsTyp = 3 };
            }

            if (zeitabhängigeKnotentemperatur != null)
            {
                modell.ZeitabhängigeKnotenLasten.Add(loadId, zeitabhängigeKnotentemperatur);
            }

            Close();
        }

        private void BtnDialogCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}