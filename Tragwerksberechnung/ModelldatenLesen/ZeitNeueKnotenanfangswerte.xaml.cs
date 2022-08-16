using FEBibliothek.Modell;
using System.Windows;

namespace FE_Berechnungen.Tragwerksberechnung.ModelldatenLesen
{

    public partial class ZeitNeueKnotenanfangswerte
    {
        private readonly FeModell modell;
        public ZeitNeueKnotenanfangswerte(FeModell modell)
        {
            InitializeComponent();
            this.modell = modell;
            KnotenId.Text = string.Empty;
            Show();
        }

        private void BtnDialogOk_Click(object sender, RoutedEventArgs e)
        {
            var knotenId = KnotenId.Text;
            if (modell.Knoten.TryGetValue(knotenId, out var knoten))
            {
                var nodalDof = knoten.AnzahlKnotenfreiheitsgrade;
                var anfangsWerte = new double[2 * nodalDof];
                if (D0.Text != string.Empty) { anfangsWerte[0] = double.Parse(D0.Text); }
                if (V0.Text != string.Empty) { anfangsWerte[1] = double.Parse(V0.Text); }

                if (nodalDof == 2)
                {
                    if (D1.Text != string.Empty) { anfangsWerte[2] = double.Parse(D1.Text); }
                    if (V1.Text != string.Empty) { anfangsWerte[3] = double.Parse(V1.Text); }
                }
                if (nodalDof == 3)
                {
                    if (D2.Text != string.Empty) { anfangsWerte[4] = double.Parse(D2.Text); }
                    if (V2.Text != string.Empty) { anfangsWerte[5] = double.Parse(V2.Text); }
                }
                modell.Zeitintegration.Anfangsbedingungen.Add(new Knotenwerte(knotenId, anfangsWerte));
            }
            Close();
        }

        private void BtnDialogCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
