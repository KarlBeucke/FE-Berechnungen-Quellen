using FEBibliothek.Modell;
using System.Windows;

namespace FE_Berechnungen.Tragwerksberechnung.ModelldatenLesen
{
    public partial class NeuerKnoten
    {
        private readonly FEModell modell;
        public NeuerKnoten()
        {
            InitializeComponent();
        }
        public NeuerKnoten(FEModell modell, int ndof)
        {
            InitializeComponent();
            this.modell = modell;
            KnotenId.Text = "";
            AnzahlDOF.Text = ndof.ToString("0");
            X.Text = "";
            Y.Text = "";
            Z.Text = "";
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
            int numberNodalDof = int.Parse(AnzahlDOF.Text);
            var crds = new double[dimension];
            if (X.Text.Length > 0) crds[0] = double.Parse(X.Text);
            if (Y.Text.Length > 0) crds[1] = double.Parse(Y.Text);
            if (KnotenId.Text.Length > 0)
            {
                var knoten = new Knoten(knotenId, crds, numberNodalDof, dimension);
                modell.Knoten.Add(knotenId, knoten);
            }
            Close();
        }
    }
}