using FEBibliothek.Modell;
using System.Windows;

namespace FE_Berechnungen.Wärmeberechnung.ModelldatenLesen
{
    public partial class NeuerKnoten
    {
        private readonly FeModell modell;
        public NeuerKnoten(FeModell modell)
        {
            this.modell = modell;
            InitializeComponent();
            KnotenId.Text = string.Empty;
            AnzahlDOF.Text = "1";
            X.Text = string.Empty;
            Y.Text = string.Empty;
            Z.Text = string.Empty;
            Show();
        }

        private void BtnDialogCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void BtnDialogOk_Click(object sender, RoutedEventArgs e)
        {
            var dimension = modell.Raumdimension;
            var knotenId = KnotenId.Text;
            const int numberNodalDof = 1;
            var crds = new double[dimension];
            if (X.Text.Length > 0) crds[0] = double.Parse(X.Text);
            if (Y.Text.Length > 0) crds[1] = double.Parse(Y.Text);
            if (Z.Text.Length > 0) crds[2] = double.Parse(Z.Text);
            if (KnotenId.Text.Length > 0)
            {
                var knoten = new Knoten(knotenId, crds, numberNodalDof, dimension);
                modell.Knoten.Add(knotenId, knoten);
            }
            Close();
        }
    }
}
