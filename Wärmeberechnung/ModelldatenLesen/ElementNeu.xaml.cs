using FE_Berechnungen.Wärmeberechnung.Modelldaten;
using FE_Berechnungen.Wärmeberechnung.ModelldatenAnzeigen;

namespace FE_Berechnungen.Wärmeberechnung.ModelldatenLesen;

public partial class ElementNeu
{
    private readonly FeModell _modell;

    public ElementNeu(FeModell modell)
    {
        InitializeComponent();
        _modell = modell;
        Show();
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
    }
    private void Element2D2Checked(object sender, RoutedEventArgs e)
    {
        Element2D2.IsChecked = true;
        Element2D3.IsChecked = false;
        Element2D4.IsChecked = false;
        Element3D8.IsChecked = false;
    }

    private void Element2D3Checked(object sender, RoutedEventArgs e)
    {
        Element2D2.IsChecked = false;
        Element2D3.IsChecked = true;
        Element2D4.IsChecked = false;
        Element3D8.IsChecked = false;
    }

    private void Element2D4Checked(object sender, RoutedEventArgs e)
    {
        Element2D2.IsChecked = false;
        Element2D3.IsChecked = false;
        Element2D4.IsChecked = true;
        Element3D8.IsChecked = false;
    }

    private void Element3D8Checked(object sender, RoutedEventArgs e)
    {
        Element2D2.IsChecked = false;
        Element2D3.IsChecked = false;
        Element2D4.IsChecked = false;
        Element3D8.IsChecked = true;
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
        _modell.Elemente.Remove(ElementId.Text);

        string[] knotenIds;
        if (Element2D2.IsChecked != null && (bool)Element2D2.IsChecked)
        {
            knotenIds = new string[2];
            knotenIds[0] = Knoten1Id.Text;
            if (Knoten2Id.Text.Length != 0) knotenIds[1] = Knoten2Id.Text;
            var element = new Element2D2(knotenIds, MaterialId.Text, _modell)
            {
                ElementId = ElementId.Text
            };
            _modell.Elemente.Add(ElementId.Text, element);
        }
        else if (Element2D3.IsChecked != null && (bool)Element2D3.IsChecked)
        {
            knotenIds = new string[3];
            knotenIds[0] = Knoten1Id.Text;
            if (Knoten2Id.Text.Length != 0) knotenIds[1] = Knoten2Id.Text;
            if (Knoten3Id.Text.Length != 0) knotenIds[2] = Knoten3Id.Text;
            var element = new Element2D3(knotenIds, MaterialId.Text, _modell)
            {
                ElementId = ElementId.Text
            };
            _modell.Elemente.Add(ElementId.Text, element);
        }
        else if (Element2D4.IsChecked != null && (bool)Element2D4.IsChecked)
        {
            knotenIds = new string[4];
            knotenIds[0] = Knoten1Id.Text;
            if (Knoten2Id.Text.Length != 0) knotenIds[1] = Knoten2Id.Text;
            if (Knoten3Id.Text.Length != 0) knotenIds[2] = Knoten3Id.Text;
            if (Knoten4Id.Text.Length != 0) knotenIds[3] = Knoten4Id.Text;
            var element = new Element2D4(knotenIds, MaterialId.Text, _modell)
            {
                ElementId = ElementId.Text
            };
            _modell.Elemente.Add(ElementId.Text, element);
        }
        else if (Element3D8.IsChecked != null && (bool)Element3D8.IsChecked)
        {
            knotenIds = new string[8];
            knotenIds[0] = Knoten1Id.Text;
            if (Knoten2Id.Text.Length != 0) knotenIds[1] = Knoten2Id.Text;
            if (Knoten3Id.Text.Length != 0) knotenIds[2] = Knoten3Id.Text;
            if (Knoten4Id.Text.Length != 0) knotenIds[3] = Knoten4Id.Text;
            if (Knoten5Id.Text.Length != 0) knotenIds[4] = Knoten5Id.Text;
            if (Knoten6Id.Text.Length != 0) knotenIds[5] = Knoten6Id.Text;
            if (Knoten7Id.Text.Length != 0) knotenIds[6] = Knoten7Id.Text;
            if (Knoten8Id.Text.Length != 0) knotenIds[7] = Knoten8Id.Text;
            var element = new Element3D8(ElementId.Text, knotenIds, MaterialId.Text, _modell);
            _modell.Elemente.Add(ElementId.Text, element);
        }

        StartFenster.WärmeVisual.Close();
        Close();

        StartFenster.WärmeVisual = new WärmemodellVisualisieren(_modell);
        StartFenster.WärmeVisual.Show();
        _modell.Berechnet = false;
    }

    private void BtnDialogCancel_Click(object sender, RoutedEventArgs e)
    {
        StartFenster.WärmeVisual.IsElement = false;
        Close();
    }

    private void BtnLöschen_Click(object sender, RoutedEventArgs e)
    {
        if (!_modell.Elemente.Keys.Contains(ElementId.Text)) return;
        _modell.Elemente.Remove(ElementId.Text);
        Close();
        StartFenster.WärmeVisual.Close();

        StartFenster.WärmeVisual = new WärmemodellVisualisieren(_modell);
        StartFenster.WärmeVisual.Show();
        _modell.Berechnet = false;
    }

    private void ElementIdLostFocus(object sender, RoutedEventArgs e)
    {
        if (!_modell.Elemente.ContainsKey(ElementId.Text))
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
        if(!_modell.Elemente.TryGetValue(ElementId.Text, out var vorhandenesElement))
        {
            throw new ModellAusnahme("\nElement '" + ElementId.Text + "' nicht im Modell gefunden");
        }
        ElementId.Text = "";

        Knoten1Id.Text = vorhandenesElement.KnotenIds[0];
        ElementId.Text = vorhandenesElement.ElementId;
        switch (vorhandenesElement)
        {
            case Modelldaten.Element2D2:
                Element2D2.IsChecked = true;
                Element2D3.IsChecked = false;
                Element2D4.IsChecked = false;
                Element3D8.IsChecked = false;
                Knoten2Id.Text = vorhandenesElement.KnotenIds[1];
                break;
            case Modelldaten.Element2D3:
                Element2D3.IsChecked = true;
                Element2D2.IsChecked = false;
                Element2D4.IsChecked = false;
                Element3D8.IsChecked = false;
                Knoten2Id.Text = vorhandenesElement.KnotenIds[1];
                Knoten3Id.Text = vorhandenesElement.KnotenIds[2];
                break;
            case Modelldaten.Element2D4:
                Element2D4.IsChecked = true;
                Element2D2.IsChecked = false;
                Element2D3.IsChecked = false;
                Element3D8.IsChecked = false;
                Knoten2Id.Text = vorhandenesElement.KnotenIds[1];
                Knoten3Id.Text = vorhandenesElement.KnotenIds[2];
                Knoten4Id.Text = vorhandenesElement.KnotenIds[3];
                break;
            case Modelldaten.Element3D8:
                Element3D8.IsChecked = true;
                Element2D2.IsChecked = false;
                Element2D3.IsChecked = false;
                Element2D4.IsChecked = false;
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