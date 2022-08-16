using FEBibliothek.Modell;
using System.Windows;

namespace FE_Berechnungen.Elastizitätsberechnung.ModelldatenLesen
{
    public partial class NeuerKnoten : Window
    {
        private readonly FeModell modell;
        public NeuerKnoten()
        {
            InitializeComponent();
        }
        public NeuerKnoten(FeModell modell, int ndof)
        {
            InitializeComponent();
            this.modell = modell;
            KnotenId.Text = string.Empty;
            AnzahlDOF.Text = ndof.ToString("0");
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
