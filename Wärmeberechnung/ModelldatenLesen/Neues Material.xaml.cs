using FEBibliothek.Modell;
using System.Windows;

namespace FE_Berechnungen.Wärmeberechnung.ModelldatenLesen
{
    public partial class NeuesMaterial
    {
        private readonly FeModell modell;

        public NeuesMaterial(FeModell modell)
        {
            this.modell = modell;
            InitializeComponent();
            MaterialId.Text = string.Empty;
            LeitfähigkeitX.Text = string.Empty;
            LeitfähigkeitY.Text = string.Empty;
            LeitfähigkeitZ.Text = string.Empty;
            DichteLeitfähigkeit.Text = string.Empty;
            Show();
        }

        private void BtnDialogOk_Click(object sender, RoutedEventArgs e)
        {
            var materialId = MaterialId.Text;
            var leitfähigkeit = new double[3];
            double dichteLeitfähigkeit = 0;
            if (LeitfähigkeitX.Text != string.Empty) { leitfähigkeit[0] = double.Parse(LeitfähigkeitX.Text); }
            if (LeitfähigkeitY.Text != string.Empty) { leitfähigkeit[1] = double.Parse(LeitfähigkeitY.Text); }
            if (LeitfähigkeitZ.Text != string.Empty) { leitfähigkeit[2] = double.Parse(LeitfähigkeitZ.Text); }
            if (DichteLeitfähigkeit.Text != string.Empty) { dichteLeitfähigkeit = double.Parse(DichteLeitfähigkeit.Text); }
            var material = new Modelldaten.Material(materialId, leitfähigkeit, dichteLeitfähigkeit);

            modell.Material.Add(materialId, material);
            Close();
        }

        private void BtnDialogCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
