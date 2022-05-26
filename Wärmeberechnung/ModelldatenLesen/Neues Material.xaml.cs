using FEBibliothek.Modell;
using System.Windows;

namespace FE_Berechnungen.Wärmeberechnung.ModelldatenLesen
{
    public partial class NeuesMaterial
    {
        private readonly FEModell modell;

        public NeuesMaterial(FEModell modell)
        {
            this.modell = modell;
            InitializeComponent();
            MaterialId.Text = "";
            LeitfähigkeitX.Text = "";
            LeitfähigkeitY.Text = "";
            LeitfähigkeitZ.Text = "";
            DichteLeitfähigkeit.Text = "";
            Show();
        }

        private void BtnDialogOk_Click(object sender, RoutedEventArgs e)
        {
            var materialId = MaterialId.Text;
            var leitfähigkeit = new double[3];
            double dichteLeitfähigkeit = 0;
            if (LeitfähigkeitX.Text != "") { leitfähigkeit[0] = double.Parse(LeitfähigkeitX.Text); }
            if (LeitfähigkeitY.Text != "") { leitfähigkeit[1] = double.Parse(LeitfähigkeitY.Text); }
            if (LeitfähigkeitZ.Text != "") { leitfähigkeit[2] = double.Parse(LeitfähigkeitZ.Text); }
            if (DichteLeitfähigkeit.Text != "") { dichteLeitfähigkeit = double.Parse(DichteLeitfähigkeit.Text); }
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
