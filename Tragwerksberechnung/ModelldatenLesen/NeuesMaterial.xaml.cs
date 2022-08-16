using FE_Berechnungen.Tragwerksberechnung.Modelldaten;
using FEBibliothek.Modell;
using FEBibliothek.Modell.abstrakte_Klassen;
using System.Windows;

namespace FE_Berechnungen.Tragwerksberechnung.ModelldatenLesen
{
    public partial class NeuesMaterial
    {
        private readonly FeModell modell;
        private AbstraktMaterial material;
        public NeuesMaterial(FeModell modell)
        {
            InitializeComponent();
            this.modell = modell;
            Show();
        }

        private void BtnDialogOk_Click(object sender, RoutedEventArgs e)
        {
            var materialId = MaterialId.Text;
            double eModul = 0, masse = 0, poisson = 0;
            if (EModul.Text != string.Empty) { eModul = double.Parse(EModul.Text); }
            if (Masse.Text != string.Empty) { masse = double.Parse(Masse.Text); }
            if (Poisson.Text != string.Empty) { poisson = double.Parse(Poisson.Text); }
            if (FederX.Text == string.Empty && FederY.Text == string.Empty && FederPhi.Text == string.Empty)
            {
                material = new Material(eModul, poisson, masse)
                {
                    MaterialId = materialId
                };
            }
            else
            {
                double federX = 0;
                if (FederX.Text != string.Empty) { federX = double.Parse(FederX.Text); }
                double federY = 0;
                if (FederY.Text != string.Empty) { federY = double.Parse(FederY.Text); }
                double federPhi = 0;
                if (FederPhi.Text != string.Empty) federPhi = double.Parse(FederPhi.Text);
                material = new Material("feder", federX, federY, federPhi)
                {
                    MaterialId = materialId
                };
            }
            modell.Material.Add(materialId, material);
            Close();
        }

        private void BtnDialogCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
