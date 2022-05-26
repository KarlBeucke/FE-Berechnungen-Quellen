using FE_Berechnungen.Wärmeberechnung.Modelldaten;
using FEBibliothek.Modell;
using System.Windows;

namespace FE_Berechnungen.Wärmeberechnung.ModelldatenLesen
{
    public partial class ZeitNeueElementtemperatur
    {
        private readonly FEModell modell;
        public ZeitNeueElementtemperatur(FEModell modell)
        {
            this.modell = modell;
            InitializeComponent();
            LoadId.Text = "";
            ElementId.Text = "";
            P0.Text = "";
            P1.Text = "";
            P2.Text = "";
            P3.Text = "";
            Show();
        }

        private void BtnDialogOk_Click(object sender, RoutedEventArgs e)
        {
            var loadId = LoadId.Text;
            var elementId = ElementId.Text;

            var knotenWerte = new double[4];
            knotenWerte[0] = double.Parse(P0.Text);
            knotenWerte[1] = double.Parse(P1.Text);
            knotenWerte[2] = double.Parse(P2.Text);
            if (P3.Text.Length > 0) knotenWerte[3] = double.Parse(P3.Text);
            var zeitabhängigeElementLast = new ZeitabhängigeElementLast(elementId, knotenWerte) { LastId = loadId, VariationsTyp = 1 };

            modell.ZeitabhängigeElementLasten.Add(loadId, zeitabhängigeElementLast);
            Close();
        }

        private void BtnDialogCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
