using FE_Berechnungen.Tragwerksberechnung.Modelldaten;
using FEBibliothek.Modell;
using System.Windows;

namespace FE_Berechnungen.Tragwerksberechnung.ModelldatenLesen
{
    public partial class NeuesElement
    {
        private readonly FEModell modell;
        public NeuesElement(FEModell modell)
        {
            InitializeComponent();
            this.modell = modell;
            ElementId.Text = "";
            StartknotenId.Text = "";
            EndknotenId.Text = "";
            MaterialId.Text = "";
            QuerschnittId.Text = "";
            Show();
        }

        private void BtnDialogOk_Click(object sender, RoutedEventArgs e)
        {
            var nodeIds = new string[2];
            nodeIds[0] = StartknotenId.Text;
            nodeIds[1] = EndknotenId.Text;

            if (Fachwerk.IsChecked != null && (bool)Fachwerk.IsChecked)
            {
                var element = new Fachwerk(nodeIds, QuerschnittId.Text, MaterialId.Text, modell)
                {
                    ElementId = ElementId.Text
                };
                modell.Elemente.Add(ElementId.Text, element);
            }
            else if (Balken.IsChecked != null && (bool)Balken.IsChecked)
            {
                var element = new Biegebalken(nodeIds, QuerschnittId.Text, MaterialId.Text, modell)
                {
                    ElementId = ElementId.Text
                };
                modell.Elemente.Add(ElementId.Text, element);
            }
            else if (BalkenGelenk.IsChecked != null && (bool)BalkenGelenk.IsChecked)
            {
                var typ = int.Parse(Gelenk.Text);
                var element = new BiegebalkenGelenk(nodeIds, QuerschnittId.Text, MaterialId.Text, modell, typ)
                {
                    ElementId = ElementId.Text
                };
                modell.Elemente.Add(ElementId.Text, element);
            }
            else if (Feder.IsChecked != null && (bool)Feder.IsChecked)
            {
                var federLager = new FederElement(nodeIds, MaterialId.Text, modell)
                {
                    ElementId = ElementId.Text
                };
                modell.Elemente.Add(ElementId.Text, federLager);
            }
            Close();
        }

        private void BtnDialogCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
