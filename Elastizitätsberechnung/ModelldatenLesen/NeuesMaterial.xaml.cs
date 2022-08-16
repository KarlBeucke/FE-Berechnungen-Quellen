using FEBibliothek.Modell;
using System.Windows;

namespace FE_Berechnungen.Elastizitätsberechnung.ModelldatenLesen
{
    public partial class NeuesMaterial
    {
        private readonly FeModell modell;

        public NeuesMaterial(FeModell modell)
        {
            this.modell = modell;
            InitializeComponent();
            MaterialId.Text = string.Empty;
            Elastizitätsmodul.Text = string.Empty;
            Poisson.Text = string.Empty;
            Schubmodul.Text = string.Empty;
            Show();
        }

        private void BtnDialogOk_Click(object sender, RoutedEventArgs e)
        {
            var materialId = MaterialId.Text;
            double elastizität = 0, poisson = 0, schub = 0;
            if (Elastizitätsmodul.Text != string.Empty) { elastizität = double.Parse(Elastizitätsmodul.Text); }
            if (Poisson.Text != string.Empty) { poisson = double.Parse(Poisson.Text); }
            if (Schubmodul.Text != string.Empty) { schub = double.Parse(Schubmodul.Text); }
            var material = new Modelldaten.Material(elastizität, poisson, schub)
            {
                MaterialId = materialId
            };
            modell.Material.Add(materialId, material);
            Close();
        }

        private void BtnDialogCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}