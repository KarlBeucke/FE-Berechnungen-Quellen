using FEBibliothek.Modell;
using System.Windows;

namespace FE_Berechnungen.Wärmeberechnung.ModelldatenLesen
{
    public partial class NeueKnotenlast
    {
        private readonly FEModell modell;

        public NeueKnotenlast(FEModell modell)
        {
            this.modell = modell;
            InitializeComponent();
            KnotenlastId.Text = "";
            KnotenId.Text = "";
            Temperatur.Text = "";
            Show();
        }

        private void BtnDialogOk_Click(object sender, RoutedEventArgs e)
        {
            var knotenlastId = KnotenlastId.Text;
            var knotenId = KnotenId.Text;
            double[] temperatur = new double[1];
            if (Temperatur.Text != "") { temperatur[0] = double.Parse(Temperatur.Text); }

            var knotenlast = new Modelldaten.KnotenLast(knotenlastId, knotenId, temperatur);

            modell.Lasten.Add(knotenlastId, knotenlast);
            Close();
        }

        private void BtnDialogCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
