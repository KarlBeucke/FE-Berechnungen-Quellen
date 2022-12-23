using System.Diagnostics;
using FE_Berechnungen.Wärmeberechnung.Modelldaten;
using FEBibliothek.Modell;
using System.Linq;
using System.Windows;

namespace FE_Berechnungen.Wärmeberechnung.ModelldatenLesen;

public partial class ElementNeu
{
    private readonly FeModell modell;
    private readonly ElementKeys elementKeys;

    public ElementNeu(FeModell modell)
    {
        InitializeComponent();
        this.modell = modell;
        Show();
        elementKeys = new ElementKeys(modell) { Owner = this };
        elementKeys.Show();
    }

    private void BtnDialogOk_Click(object sender, RoutedEventArgs e)
    {
        if (ElementId.Text == "")
        {
            _ = MessageBox.Show("Element Id muss definiert sein", "neues Element");
            return;
        }

        // vorhandenes Element wird komplett entfernt, da Elementdefinition
        // (Element2D2, Element2D3, Element2D4, Element3D8) geändert werden kann
        // neues Element wird angelegt und unter vorhandenem Key gespeichert
        if (modell.Elemente.ContainsKey(ElementId.Text))
        {
            modell.Elemente.Remove(ElementId.Text);
        }
        var knotenIds = new string[2];
        knotenIds[0] = Knoten1Id.Text;
        if (Knoten2Id.Text.Length != 0) knotenIds[1] = Knoten2Id.Text;

        if (Element2D2Check.IsChecked != null && (bool)Element2D2Check.IsChecked)
        {
            var element = new Element2D2(knotenIds, MaterialId.Text, modell)
            {
                ElementId = ElementId.Text
            };
            modell.Elemente.Add(ElementId.Text, element);
        }
        else if (Element2D3Check.IsChecked != null && (bool)Element2D3Check.IsChecked)
        {
            if (Knoten3Id.Text.Length != 0) knotenIds[2] = Knoten3Id.Text;
            var element = new Element2D3(knotenIds, MaterialId.Text, modell)
            {
                ElementId = ElementId.Text
            };
            modell.Elemente.Add(ElementId.Text, element);
        }
        else if (Element2D4Check.IsChecked != null && (bool)Element2D4Check.IsChecked)
        {
            if (Knoten3Id.Text.Length != 0) knotenIds[2] = Knoten3Id.Text;
            if (Knoten4Id.Text.Length != 0) knotenIds[3] = Knoten4Id.Text;
            var element = new Element2D4(knotenIds, MaterialId.Text, modell)
            {
                ElementId = ElementId.Text
            };
            modell.Elemente.Add(ElementId.Text, element);
        }
        else if (Element3D8Check.IsChecked != null && (bool)Element3D8Check.IsChecked)
        {
            if (Knoten3Id.Text.Length != 0) knotenIds[2] = Knoten3Id.Text;
            if (Knoten4Id.Text.Length != 0) knotenIds[3] = Knoten4Id.Text;
            if (Knoten5Id.Text.Length != 0) knotenIds[4] = Knoten5Id.Text;
            if (Knoten6Id.Text.Length != 0) knotenIds[5] = Knoten6Id.Text;
            if (Knoten7Id.Text.Length != 0) knotenIds[6] = Knoten7Id.Text;
            if (Knoten8Id.Text.Length != 0) knotenIds[7] = Knoten8Id.Text;
            var element = new Element3D8(ElementId.Text, knotenIds, MaterialId.Text, modell);
            modell.Elemente.Add(ElementId.Text, element);
        }
        Close();
        StartFenster.tragwerksModell.Close();
        elementKeys?.Close();
    }

    private void BtnDialogCancel_Click(object sender, RoutedEventArgs e)
    {
        elementKeys?.Close();
        Close();
    }

    private void BtnLöschen_Click(object sender, RoutedEventArgs e)
    {
        if (!modell.Elemente.Keys.Contains(ElementId.Text)) return;
        modell.Elemente.Remove(ElementId.Text);
        elementKeys?.Close();
        Close();
        StartFenster.wärmeModell.Close();
    }

    private void ElementIdLostFocus(object sender, RoutedEventArgs e)
    {
        if (!modell.Elemente.ContainsKey(ElementId.Text))
        {
            Knoten1Id.Text = "";
            Knoten2Id.Text = "";
            Knoten3Id.Text = "";
            Knoten4Id.Text = "";
            Knoten5Id.Text = "";
            Knoten6Id.Text = "";
            Knoten7Id.Text = "";
            Knoten8Id.Text = "";
            MaterialId.Text = "";
            return;
        }

        // vorhandene element definitionen
        modell.Elemente.TryGetValue(ElementId.Text, out var vorhandenesElement);
        Debug.Assert(vorhandenesElement != null, nameof(vorhandenesElement) + " != null"); ElementId.Text = "";

        ElementId.Text = vorhandenesElement.ElementId;
        Knoten1Id.Text = vorhandenesElement.KnotenIds[0];
        switch (vorhandenesElement)
        {
            case Element2D2:
                Element2D2Check.IsChecked = true;
                Element2D3Check.IsChecked = false; Element2D4Check.IsChecked = false; Element3D8Check.IsChecked = false;
                Knoten2Id.Text = vorhandenesElement.KnotenIds[1];
                break;
            case Element2D3:
                Element2D3Check.IsChecked = true;
                Element2D2Check.IsChecked = false; Element2D4Check.IsChecked = false; Element3D8Check.IsChecked = false;
                Knoten2Id.Text = vorhandenesElement.KnotenIds[1];
                Knoten3Id.Text = vorhandenesElement.KnotenIds[2];
                break;
            case Element2D4:
                Element2D4Check.IsChecked = true;
                Element2D2Check.IsChecked = false; Element2D3Check.IsChecked = false; Element3D8Check.IsChecked = false;
                Knoten2Id.Text = vorhandenesElement.KnotenIds[1];
                Knoten3Id.Text = vorhandenesElement.KnotenIds[2];
                Knoten4Id.Text = vorhandenesElement.KnotenIds[3];
                break;
            case Element3D8:
                Element3D8Check.IsChecked = true;
                Element2D2Check.IsChecked = false; Element2D3Check.IsChecked = false; Element2D4Check.IsChecked = false;
                Knoten2Id.Text = vorhandenesElement.KnotenIds[1];
                Knoten3Id.Text = vorhandenesElement.KnotenIds[2];
                Knoten4Id.Text = vorhandenesElement.KnotenIds[3];
                Knoten5Id.Text = vorhandenesElement.KnotenIds[4];
                Knoten6Id.Text = vorhandenesElement.KnotenIds[5];
                Knoten7Id.Text = vorhandenesElement.KnotenIds[6];
                Knoten8Id.Text = vorhandenesElement.KnotenIds[7];
                break;
        }
        MaterialId.Text = vorhandenesElement.ElementMaterialId;
    }
}