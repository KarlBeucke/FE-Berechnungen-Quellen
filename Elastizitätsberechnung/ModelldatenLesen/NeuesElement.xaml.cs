using FEBibliothek.Modell;
using FEBibliothek.Modell.abstrakte_Klassen;
using System.Windows;
using Element2D3 = FE_Berechnungen.Elastizitätsberechnung.Modelldaten.Element2D3;
using Element3D8 = FE_Berechnungen.Elastizitätsberechnung.Modelldaten.Element3D8;

namespace FE_Berechnungen.Elastizitätsberechnung.ModelldatenLesen
{
    public partial class NeuesElement : Window
    {
        private readonly FEModell modell;
        public NeuesElement(FEModell modell)
        {
            InitializeComponent();
            this.modell = modell;
            ElementId.Text = "";
            Knoten1Id.Text = "";
            Knoten2Id.Text = "";
            Knoten3Id.Text = "";
            Knoten4Id.Text = "";
            Knoten5Id.Text = "";
            Knoten6Id.Text = "";
            Knoten7Id.Text = "";
            Knoten8Id.Text = "";
            MaterialId.Text = "";
            Show();
        }

        private void BtnDialogOk_Click(object sender, RoutedEventArgs e)
        {
            AbstraktElement element = null;
            string elementId = null;

            if (Element2D3.IsChecked != null && (bool)Element2D3.IsChecked)
            {
                var nodeIds = new string[3];
                nodeIds[0] = Knoten1Id.Text;
                nodeIds[1] = Knoten2Id.Text;
                nodeIds[2] = Knoten3Id.Text;
                elementId = ElementId.Text;
                var querschnittId = QuerschnittId.Text;
                var materialId = MaterialId.Text;
                element = new Element2D3(nodeIds, querschnittId, materialId, modell) { ElementId = elementId };
                modell.Elemente.Add(ElementId.Text, element);
            }
            else if (Element3D8.IsChecked != null && (bool)Element3D8.IsChecked)
            {
                var nodeIds = new string[8];
                nodeIds[0] = Knoten1Id.Text;
                nodeIds[1] = Knoten2Id.Text;
                nodeIds[2] = Knoten3Id.Text;
                nodeIds[3] = Knoten4Id.Text;
                nodeIds[4] = Knoten5Id.Text;
                nodeIds[5] = Knoten6Id.Text;
                nodeIds[6] = Knoten7Id.Text;
                nodeIds[7] = Knoten8Id.Text;
                elementId = ElementId.Text;
                var materialId = MaterialId.Text;
                element = new Element3D8(nodeIds, materialId, modell) { ElementId = elementId };
                modell.Elemente.Add(ElementId.Text, element);
            }

            if (elementId != null) modell.Elemente.Add(elementId, element);
            Close();
        }

        private void BtnDialogCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}