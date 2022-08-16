using FE_Berechnungen.Tragwerksberechnung.Modelldaten;
using FEBibliothek.Modell;
using FEBibliothek.Modell.abstrakte_Klassen;
using System.Windows;

namespace FE_Berechnungen.Tragwerksberechnung.ModelldatenLesen
{

    public partial class NeueLinienlast
    {
        private readonly FeModell modell;
        private AbstraktLinienlast linienlast;
        public NeueLinienlast()
        {
            InitializeComponent();
            Show();
        }
        public NeueLinienlast(FeModell modell, string last, string element,
            double pxa, double pya, double pxb, double pyb, string inElement)
        {
            InitializeComponent();
            this.modell = modell;
            LastId.Text = last;
            ElementId.Text = element;
            Pxa.Text = pxa.ToString("0.00");
            Pxa.Text = pxa.ToString("0.00");
            Pya.Text = pya.ToString("0.00");
            Pxb.Text = pxb.ToString("0.00");
            Pyb.Text = pyb.ToString("0.00");
            InElement.Text = "false";
            Show();
        }

        private void BtnDialogCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void BtnDialogOk_Click(object sender, RoutedEventArgs e)
        {
            var lastId = LastId.Text;
            var elementId = ElementId.Text;
            var pxa = double.Parse(Pxa.Text);
            var pya = double.Parse(Pya.Text);
            var pxb = double.Parse(Pxb.Text);
            var pyb = double.Parse(Pyb.Text);
            var inElement = InElement.Text == "true";
            linienlast =
                new LinienLast(elementId, pxa, pya, pxb, pyb, inElement)
                {
                    LastId = lastId
                };
            modell.ElementLasten.Add(lastId, linienlast);
            Close();
        }
    }
}