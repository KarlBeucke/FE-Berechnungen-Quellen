using FE_Berechnungen.Wärmeberechnung.Modelldaten;
using FEBibliothek.Modell;
using FEBibliothek.Modell.abstrakte_Klassen;
using System.Windows;

namespace FE_Berechnungen.Wärmeberechnung.ModelldatenLesen
{
    public partial class NeuesElement
    {
        private readonly FEModell modell;
        public NeuesElement(FEModell modell)
        {
            InitializeComponent();
            this.modell = modell;
            ElementId.Text = string.Empty;
            Knoten1Id.Text = string.Empty;
            Knoten2Id.Text = string.Empty;
            Knoten3Id.Text = string.Empty;
            Knoten4Id.Text = string.Empty;
            Knoten5Id.Text = string.Empty;
            Knoten6Id.Text = string.Empty;
            Knoten7Id.Text = string.Empty;
            Knoten8Id.Text = string.Empty;
            MaterialId.Text = string.Empty;
            Show();
        }

        private void BtnDialogOk_Click(object sender, RoutedEventArgs e)
        {
            AbstraktElement element = null;
            string elementId = null;

            if (Element2D2.IsChecked != null && (bool)Element2D2.IsChecked)
            {
                var nodeIds = new string[2];
                nodeIds[0] = Knoten1Id.Text;
                nodeIds[1] = Knoten2Id.Text;
                elementId = ElementId.Text;
                element = new Element2D2(elementId, nodeIds, MaterialId.Text, modell);
                modell.Elemente.Add(ElementId.Text, element);
            }
            else if (Element2D3.IsChecked != null && (bool)Element2D3.IsChecked)
            {
                var nodeIds = new string[3];
                nodeIds[0] = Knoten1Id.Text;
                nodeIds[1] = Knoten2Id.Text;
                nodeIds[2] = Knoten3Id.Text;
                elementId = ElementId.Text;
                element = new Element2D3(elementId, nodeIds, MaterialId.Text, modell);
                modell.Elemente.Add(ElementId.Text, element);
            }
            else if (Element2D4.IsChecked != null && (bool)Element2D4.IsChecked)
            {
                var nodeIds = new string[4];
                nodeIds[0] = Knoten1Id.Text;
                nodeIds[1] = Knoten2Id.Text;
                nodeIds[2] = Knoten3Id.Text;
                nodeIds[3] = Knoten4Id.Text;
                elementId = ElementId.Text;
                element = new Element2D4(elementId, nodeIds, MaterialId.Text, modell);
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
                element = new Element3D8(elementId, nodeIds, MaterialId.Text, modell);
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