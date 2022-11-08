using FE_Berechnungen.Tragwerksberechnung.Modelldaten;
using FEBibliothek.Modell;
using System.Diagnostics;
using System.Windows;

namespace FE_Berechnungen.Tragwerksberechnung.ModelldatenLesen;

public partial class FederelementNeu
{
    private readonly FeModell modell;

    public FederelementNeu(FeModell modell)
    {
        InitializeComponent();
        Show();
        this.modell = modell;
        //var elementKeys = new ElementKeys(modell) { Owner = this };
        //elementKeys.Show();
    }

    private void BtnDialogOk_Click(object sender, RoutedEventArgs e)
    {
        var elementId = ElementId.Text;
        if (elementId == "")
        {
            _ = MessageBox.Show("Element Id muss definiert sein", "neues Federelement");
            return;
        }

        if (modell.Elemente.ContainsKey(elementId))
        {
            modell.Elemente.TryGetValue(elementId, out var vorhandenesElement);
            Debug.Assert(vorhandenesElement != null, nameof(vorhandenesElement) + " != null");
            if (KnotenId.Text.Length > 0) vorhandenesElement.KnotenIds[0] = KnotenId.Text;
            if (MaterialId.Text.Length > 0) vorhandenesElement.ElementMaterialId = MaterialId.Text;
        }
        else
        {
            var nodeIds = new string[2];
            nodeIds[0] = KnotenId.Text;
            var materialId = "";
            if (MaterialId.Text.Length > 0) materialId = MaterialId.Text;
            var federLager = new FederElement(nodeIds, materialId, modell)
            {
                ElementId = ElementId.Text
            };
            modell.Elemente.Add(ElementId.Text, federLager);
        }
        StartFenster.tragwerksModell.Close();
        Close();
    }

    private void BtnDialogCancel_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void KnotenIdLostFocus(object sender, RoutedEventArgs e)
    {
        if (!modell.Elemente.ContainsKey(ElementId.Text))
        {
            KnotenId.Text = "";
            MaterialId.Text = "";
            return;
        }
        modell.Elemente.TryGetValue(ElementId.Text, out var vorhandenesElement);
        Debug.Assert(vorhandenesElement != null, nameof(vorhandenesElement) + " != null");
        KnotenId.Text = vorhandenesElement.KnotenIds[0];
        MaterialId.Text = vorhandenesElement.ElementMaterialId;
    }
}