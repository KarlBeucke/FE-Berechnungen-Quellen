using Element2D3 = FE_Berechnungen.Elastizitätsberechnung.Modelldaten.Element2D3;
using Element3D8 = FE_Berechnungen.Elastizitätsberechnung.Modelldaten.Element3D8;

namespace FE_Berechnungen.Elastizitätsberechnung.ModelldatenLesen;

public partial class NeuesElement
{
    private readonly FeModell _modell;

    public NeuesElement(FeModell modell)
    {
        InitializeComponent();
        _modell = modell;
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

        if (Element2D3.IsChecked != null && (bool)Element2D3.IsChecked)
        {
            var nodeIds = new string[3];
            nodeIds[0] = Knoten1Id.Text;
            nodeIds[1] = Knoten2Id.Text;
            nodeIds[2] = Knoten3Id.Text;
            elementId = ElementId.Text;
            var querschnittId = QuerschnittId.Text;
            var materialId = MaterialId.Text;
            element = new Element2D3(nodeIds, querschnittId, materialId, _modell) { ElementId = elementId };
            _modell.Elemente.Add(ElementId.Text, element);
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
            element = new Element3D8(nodeIds, materialId, _modell) { ElementId = elementId };
            _modell.Elemente.Add(ElementId.Text, element);
        }

        if (elementId != null) _modell.Elemente.Add(elementId, element);
        Close();
    }

    private void BtnDialogCancel_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}