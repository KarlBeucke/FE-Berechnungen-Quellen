using FEBibliothek.Modell;
using System.Windows;

namespace FE_Berechnungen.Wärmeberechnung.ModelldatenLesen
{

    public partial class NeueElementlast
    {
        private readonly FEModell modell;

        public NeueElementlast(FEModell modell)
        {
            this.modell = modell;
            InitializeComponent();
            ElementlastlastId.Text = "";
            ElementId.Text = "";
            Knoten1.Text = "";
            Knoten2.Text = "";
            Knoten3.Text = "";
            Knoten4.Text = "";
            Knoten5.Text = "";
            Knoten6.Text = "";
            Knoten7.Text = "";
            Knoten8.Text = "";
            Show();
        }

        private void BtnDialogOk_Click(object sender, RoutedEventArgs e)
        {
            var elementlastId = ElementlastlastId.Text;
            var elementId = ElementId.Text;
            double[] temperatur = new double[8];
            if (Knoten1.Text != "") { temperatur[0] = double.Parse(Knoten1.Text); }
            if (Knoten2.Text != "") { temperatur[1] = double.Parse(Knoten2.Text); }
            if (Knoten3.Text != "") { temperatur[2] = double.Parse(Knoten3.Text); }
            if (Knoten4.Text != "") { temperatur[3] = double.Parse(Knoten4.Text); }
            if (Knoten5.Text != "") { temperatur[4] = double.Parse(Knoten5.Text); }
            if (Knoten6.Text != "") { temperatur[5] = double.Parse(Knoten6.Text); }
            if (Knoten7.Text != "") { temperatur[6] = double.Parse(Knoten7.Text); }
            if (Knoten8.Text != "") { temperatur[7] = double.Parse(Knoten8.Text); }

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