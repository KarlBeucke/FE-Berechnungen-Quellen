using FEBibliothek.Modell;
using System.Windows;

namespace FE_Berechnungen.Wärmeberechnung.ModelldatenLesen
{

    public partial class NeueElementlast
    {
        private readonly FeModell modell;

        public NeueElementlast(FeModell modell)
        {
            this.modell = modell;
            InitializeComponent();
            ElementlastlastId.Text = string.Empty;
            ElementId.Text = string.Empty;
            Knoten1.Text = string.Empty;
            Knoten2.Text = string.Empty;
            Knoten3.Text = string.Empty;
            Knoten4.Text = string.Empty;
            Knoten5.Text = string.Empty;
            Knoten6.Text = string.Empty;
            Knoten7.Text = string.Empty;
            Knoten8.Text = string.Empty;
            Show();
        }

        private void BtnDialogOk_Click(object sender, RoutedEventArgs e)
        {
            var elementlastId = ElementlastlastId.Text;
            var elementId = ElementId.Text;
            double[] temperatur = new double[8];
            if (Knoten1.Text != string.Empty) { temperatur[0] = double.Parse(Knoten1.Text); }
            if (Knoten2.Text != string.Empty) { temperatur[1] = double.Parse(Knoten2.Text); }
            if (Knoten3.Text != string.Empty) { temperatur[2] = double.Parse(Knoten3.Text); }
            if (Knoten4.Text != string.Empty) { temperatur[3] = double.Parse(Knoten4.Text); }
            if (Knoten5.Text != string.Empty) { temperatur[4] = double.Parse(Knoten5.Text); }
            if (Knoten6.Text != string.Empty) { temperatur[5] = double.Parse(Knoten6.Text); }
            if (Knoten7.Text != string.Empty) { temperatur[6] = double.Parse(Knoten7.Text); }
            if (Knoten8.Text != string.Empty) { temperatur[7] = double.Parse(Knoten8.Text); }

            var elementlast = new Modelldaten.ElementLast4(elementlastId, elementId, temperatur);

            modell.ElementLasten.Add(elementlastId, elementlast);
            Close();
        }

        private void BtnDialogCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}