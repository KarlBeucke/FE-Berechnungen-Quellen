using FE_Berechnungen.Tragwerksberechnung.Modelldaten;
using FEBibliothek.Modell;
using FEBibliothek.Modell.abstrakte_Klassen;
using System.Windows;

namespace FE_Berechnungen.Tragwerksberechnung.ModelldatenLesen
{
    public partial class NeuePunktlast
    {
        private readonly FeModell modell;
        private AbstraktElementLast punktlast;
        public NeuePunktlast()
        {
            InitializeComponent();
            Show();
        }

        public NeuePunktlast(FeModell modell, string last, string element, double px, double py, double offset)
        {
            InitializeComponent();
            this.modell = modell;
            LastId.Text = last;
            ElementId.Text = element;
            Px.Text = px.ToString("0.00");
            Py.Text = py.ToString("0.00");
            Offset.Text = offset.ToString("0.00");
            Show();
        }

        private void BtnDialogOk_Click(object sender, RoutedEventArgs e)
        {
            var lastId = LastId.Text;
            var elementId = ElementId.Text;
            var px = double.Parse(Px.Text);
            var py = double.Parse(Py.Text);
            var offset = double.Parse(Offset.Text);
            punktlast =
                new PunktLast(elementId, px, py, offset)
                {
                    LastId = lastId
                };
            modell.ElementLasten.Add(lastId, punktlast);
            Close();
        }

        private void BtnDialogCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
