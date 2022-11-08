using FE_Berechnungen.Tragwerksberechnung.Modelldaten;
using FEBibliothek.Modell;
using System.Diagnostics;
using System.Linq;
using System.Windows;

namespace FE_Berechnungen.Tragwerksberechnung.ModelldatenLesen;

public partial class BalkenElementNeu
{
    private readonly FeModell modell;
    private readonly ElementKeys elementKeys;
    public BalkenElementNeu(FeModell modell)
    {
        InitializeComponent();
        this.modell = modell;
        elementKeys = new ElementKeys(modell);
        elementKeys.Show();
        Show();
    }

    private void BtnDialogOk_Click(object sender, RoutedEventArgs e)
    {
        var elementId = ElementId.Text;
        if (elementId == "")
        {
            _ = MessageBox.Show("Element Id muss definiert sein", "neues Balkenelement");
            return;
        }

        // vorhandenes Element
        if (modell.Elemente.Keys.Contains(ElementId.Text))
        {
            modell.Elemente.TryGetValue(elementId, out var vorhandenesElement);
            Debug.Assert(vorhandenesElement != null, nameof(vorhandenesElement) + " != null");
            vorhandenesElement.Typ = 0;
            if (Gelenk1.IsChecked != null && (bool)Gelenk1.IsChecked) vorhandenesElement.Typ = 1;
            if (Gelenk2.IsChecked != null && (bool)Gelenk2.IsChecked) vorhandenesElement.Typ = 2;
            if (Gelenk1.IsChecked != null && Gelenk2.IsChecked != null && (bool)Gelenk1.IsChecked 
                                          && (bool)Gelenk2.IsChecked) vorhandenesElement.Typ = 3;
            if (StartknotenId.Text.Length > 0) vorhandenesElement.KnotenIds[0] = StartknotenId.Text;
            if (EndknotenId.Text.Length > 0) vorhandenesElement.KnotenIds[1] = EndknotenId.Text;
            if (MaterialId.Text.Length > 0) vorhandenesElement.ElementMaterialId = MaterialId.Text;
            if (QuerschnittId.Text.Length > 0) vorhandenesElement.ElementQuerschnittId = QuerschnittId.Text;
            if (vorhandenesElement.Typ == 0)
            {
                modell.Elemente.Remove(elementId);
                var nodeIds = vorhandenesElement.KnotenIds;
                var biegebalken = new Biegebalken(nodeIds, QuerschnittId.Text, MaterialId.Text, modell)
                {
                    ElementId = ElementId.Text
                };
                modell.Elemente.Add(ElementId.Text, biegebalken);
            }
            else if (vorhandenesElement.Typ is 1 | vorhandenesElement.Typ is 2)
            {
                {
                    modell.Elemente.Remove(elementId);
                    var nodeIds = vorhandenesElement.KnotenIds;
                    var biegebalkenGelenk = new BiegebalkenGelenk(nodeIds, MaterialId.Text, QuerschnittId.Text,
                        modell, vorhandenesElement.Typ)
                    {
                        ElementId = ElementId.Text
                    };
                    modell.Elemente.Add(ElementId.Text, biegebalkenGelenk);
                }
            }
            else if (vorhandenesElement.Typ is 3)
            {
                {
                    modell.Elemente.Remove(elementId);
                    var nodeIds = vorhandenesElement.KnotenIds;
                    var fachwerk = new Fachwerk(nodeIds, QuerschnittId.Text, MaterialId.Text, modell)
                    {
                        ElementId = ElementId.Text
                    };
                    modell.Elemente.Add(ElementId.Text, fachwerk);
                }
            }
        }
        // neues Element
        else
        {
            if (ElementId.Text == "" | StartknotenId.Text == "" | EndknotenId.Text == "" 
                | MaterialId.Text == "" | QuerschnittId.Text == "")
            {
                _ = MessageBox.Show("die Eingabewerte müssen vollständig definiert sein", "neues Balkenelement");
                return;
            }

            var typ = 0;
            if (Gelenk1.IsChecked != null && (bool)Gelenk1.IsChecked) typ = 1;
            if (Gelenk2.IsChecked != null && (bool)Gelenk2.IsChecked) typ = 2;
            if (Gelenk1.IsChecked != null && Gelenk2.IsChecked != null 
              && (bool)Gelenk1.IsChecked && (bool)Gelenk2.IsChecked) typ = 3;

            var nodeIds = new string[2];
            nodeIds[0] = StartknotenId.Text;
            nodeIds[1] = EndknotenId.Text;

            if (typ == 0)
            {
                var biegebalken = new Biegebalken(nodeIds, QuerschnittId.Text, MaterialId.Text, modell)
                {
                    ElementId = ElementId.Text
                };
                modell.Elemente.Add(ElementId.Text, biegebalken);
            }
            else if (typ == 1 | typ ==2)
            {
                var biegebalkenGelenk = new BiegebalkenGelenk(nodeIds, MaterialId.Text, QuerschnittId.Text, modell, typ)
                {
                    ElementId = ElementId.Text
                };
                modell.Elemente.Add(ElementId.Text, biegebalkenGelenk);
            }
            else if (typ == 3)
            {
                var fachwerk = new Fachwerk(nodeIds, QuerschnittId.Text, MaterialId.Text, modell)
                {
                    ElementId = ElementId.Text
                };
                modell.Elemente.Add(ElementId.Text, fachwerk);
            }
        }
        elementKeys.Close();
        StartFenster.tragwerksModell.Close();
        Close();
    }

    private void BtnDialogCancel_Click(object sender, RoutedEventArgs e)
    {
        elementKeys.Close();
        Close();
    }

    private void ElementIdLostFocus(object sender, RoutedEventArgs e)
    {
        if (!modell.Elemente.ContainsKey(ElementId.Text)) return;
        modell.Elemente.TryGetValue(ElementId.Text, out var vorhandenesElement);
        Debug.Assert(vorhandenesElement != null, nameof(vorhandenesElement) + " != null"); ElementId.Text = "";
        if (vorhandenesElement is Fachwerk) { Gelenk1.IsChecked = true; Gelenk2.IsChecked = true; }
        else if (vorhandenesElement is BiegebalkenGelenk)
        {
            if (vorhandenesElement.Typ == 1) Gelenk1.IsChecked = true;
            if (vorhandenesElement.Typ == 2) Gelenk2.IsChecked = true;
        }

        ElementId.Text = vorhandenesElement.ElementId;
        StartknotenId.Text = vorhandenesElement.KnotenIds[0];
        EndknotenId.Text = vorhandenesElement.KnotenIds[1];
        MaterialId.Text = vorhandenesElement.ElementMaterialId;
        QuerschnittId.Text = vorhandenesElement.ElementQuerschnittId;
    }
}