using FEBibliothek.Modell;
using System.Windows;

namespace FE_Berechnungen.Wärmeberechnung.ModelldatenLesen
{
    public partial class NeueLinienlast
    {
        private readonly FEModell modell;

        public NeueLinienlast(FEModell modell)
        {
            this.modell = modell;
            InitializeComponent();
            LinienlastId.Text = string.Empty;
            StartknotenId.Text = string.Empty;
            Start.Text = string.Empty;
            EndknotenId.Text = string.Empty;
            End.Text = string.Empty;
            Show();
        }

        private void BtnDialogOk_Click(object sender, RoutedEventArgs e)
        {
            var linienlastId = LinienlastId.Text;
            double[] temperatur = new double[2];
            var startId = StartknotenId.Text;
            if (Start.Text != string.Empty) { temperatur[0] = double.Parse(Start.Text); }
            var endId = EndknotenId.Text;
            if (End.Text != string.Empty) { temperatur[1] = double.Parse(End.Text); }

            var knotenlast = new Modelldaten.LinienLast(linienlastId, startId, endId, temperatur);

            modell.LinienLasten.Add(linienlastId, knotenlast);
            Close();
        }

        private void BtnDialogCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}