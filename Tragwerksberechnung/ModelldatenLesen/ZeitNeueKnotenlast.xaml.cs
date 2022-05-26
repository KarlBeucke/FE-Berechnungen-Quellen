using FE_Berechnungen.Tragwerksberechnung.Modelldaten;
using FEBibliothek.Modell;
using FEBibliothek.Modell.abstrakte_Klassen;
using System;
using System.Windows;

namespace FE_Berechnungen.Tragwerksberechnung.ModelldatenLesen
{
    public partial class ZeitNeueKnotenlast
    {
        private readonly FEModell modell;
        public ZeitNeueKnotenlast(FEModell modell)
        {
            InitializeComponent();
            this.modell = modell;
            LoadId.Text = "";
            KnotenId.Text = "";
            KnotenDof.Text = "";
            Datei.IsChecked = false;
            Amplitude.Text = "";
            Frequenz.Text = "";
            Winkel.Text = "";
            Linear.Text = "";
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
                Amplitude.Text = "";
                Frequenz.Text = "";
                Winkel.Text = "";
                Linear.Text = "";
                zeitabhängigeKnotenlast.Datei = true;
                zeitabhängigeKnotenlast.VariationsTyp = 0;
                var last = (AbstraktZeitabhängigeKnotenlast)zeitabhängigeKnotenlast;
                modell.ZeitabhängigeKnotenLasten.Add(loadId, last);

            }
            else if ((Amplitude.Text.Length & Frequenz.Text.Length & Winkel.Text.Length) != 0)
            {
                Linear.Text = "";
                Datei.IsChecked = false;
                zeitabhängigeKnotenlast.VariationsTyp = 2;
                zeitabhängigeKnotenlast.Amplitude = double.Parse(Amplitude.Text);
                zeitabhängigeKnotenlast.Frequenz = 2 * Math.PI * double.Parse(Frequenz.Text);
                zeitabhängigeKnotenlast.PhasenWinkel = Math.PI / 180 * double.Parse(Winkel.Text);
            }
            else if (Linear.Text.Length != 0)
            {
                Amplitude.Text = "";
                Frequenz.Text = "";
                Winkel.Text = "";
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
            modell.ZeitabhängigeKnotenLasten.Add(loadId, zeitabhängigeKnotenlast);
            Close();
        }

        private void BtnDialogCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}