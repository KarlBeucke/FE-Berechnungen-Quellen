using FE_Berechnungen.Tragwerksberechnung.Modelldaten;
using FEBibliothek.Modell;
using System.Windows;

namespace FE_Berechnungen.Tragwerksberechnung.ModelldatenLesen
{
    public partial class NeueKnotenlast
    {
        private readonly FEModell modell;
        public NeueKnotenlast()
        {
            InitializeComponent();
            Show();
        }

        public NeueKnotenlast(FEModell modell, string last, string knoten,
            double px, double py, double m)
        {
            InitializeComponent();
            this.modell = modell;
            LastId.Text = last;
            KnotenId.Text = knoten;
            Px.Text = px.ToString("0.00");
            Py.Text = py.ToString("0.00");
            M.Text = m.ToString("0.00");
            Show();
        }

        private void BtnDialogOk_Click(object sender, RoutedEventArgs e)
        {
            var loadId = LastId.Text;
            var nodeId = KnotenId.Text;
            var p = new double[3];
            p[0] = double.Parse(Px.Text);
            p[1] = double.Parse(Py.Text);
            p[2] = double.Parse(M.Text);
            var knotenLast = new KnotenLast(nodeId, p[0], p[1], p[2])
            {
                LastId = loadId
            };
            modell.Lasten.Add(loadId, knotenLast);
            Close();
        }

        private void BtnDialogCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
