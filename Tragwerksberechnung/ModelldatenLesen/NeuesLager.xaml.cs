using FE_Berechnungen.Tragwerksberechnung.Modelldaten;
using FEBibliothek.Modell;
using System.Windows;

namespace FE_Berechnungen.Tragwerksberechnung.ModelldatenLesen
{
    public partial class NeuesLager
    {
        private readonly FEModell modell;
        public NeuesLager(FEModell modell)
        {
            InitializeComponent();
            this.modell = modell;
            Show();
        }

        public NeuesLager(FEModell modell, double vordefX, double vordefY, double vordefRot)
        {
            InitializeComponent();
            this.modell = modell;
            VorX.Text = vordefX.ToString("0.00");
            VorY.Text = vordefY.ToString("0.00");
            VorRot.Text = vordefRot.ToString("0.00");
            Show();
        }

        private void BtnDialogCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void BtnDialogOk_Click(object sender, RoutedEventArgs e)
        {
            var lagerId = LagerId.Text;
            var knotenId = KnotenId.Text;
            double[] prescribed = new double[3];
            int conditions = 0;
            var type = Fest.Text;
            for (var k = 0; k < type.Length; k++)
            {
                var subType = type.Substring(k, 1);
                switch (subType)
                {
                    case "x":
                        conditions += Lager.XFixed;
                        break;
                    case "y":
                        conditions += Lager.YFixed;
                        break;
                    case "r":
                        conditions += Lager.RFixed;
                        break;
                }
            }
            var lager = new Lager(LagerId.Text, conditions, prescribed, modell) { RandbedingungId = lagerId };
            modell.Randbedingungen.Add(lagerId, lager);
            Close();
        }
    }
}
