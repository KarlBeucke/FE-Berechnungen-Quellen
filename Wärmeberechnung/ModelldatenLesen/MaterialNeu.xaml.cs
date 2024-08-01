using FE_Berechnungen.Wärmeberechnung.Modelldaten;
using FEBibliothek.Modell;
using FEBibliothek.Modell.abstrakte_Klassen;
using System.Diagnostics;
using System.Globalization;
using System.Windows;

namespace FE_Berechnungen.Wärmeberechnung.ModelldatenLesen;

public partial class MaterialNeu
{
    private readonly MaterialKeys materialKeys;
    private readonly FeModell modell;
    private AbstraktMaterial material, vorhandenesMaterial;

    public MaterialNeu(FeModell modell)
    {
        this.modell = modell;
        InitializeComponent();
        materialKeys = new MaterialKeys(modell);
        materialKeys.Show();
        Show();
    }

    private void BtnDialogOk_Click(object sender, RoutedEventArgs e)
    {
        var materialId = MaterialId.Text;
        if (materialId == "")
        {
            _ = MessageBox.Show("Material Id muss definiert sein", "neues Material");
            return;
        }

        var leitfähigkeit = new double[3];
        double dichteLeitfähigkeit = 0;
        // vorhandenes Material
        if (modell.Material.Keys.Contains(MaterialId.Text))
        {
            modell.Material.TryGetValue(materialId, out vorhandenesMaterial);
            Debug.Assert(vorhandenesMaterial != null, nameof(vorhandenesMaterial) + " != null");

            if (LeitfähigkeitX.Text.Length > 0)
                vorhandenesMaterial.MaterialWerte[0] = double.Parse(LeitfähigkeitX.Text);
            if (LeitfähigkeitY.Text.Length > 0)
                vorhandenesMaterial.MaterialWerte[1] = double.Parse(LeitfähigkeitY.Text);
            if (LeitfähigkeitZ.Text.Length > 0)
                vorhandenesMaterial.MaterialWerte[2] = double.Parse(LeitfähigkeitZ.Text);
            if (DichteLeitfähigkeit.Text.Length > 0)
                vorhandenesMaterial.MaterialWerte[3] = double.Parse(DichteLeitfähigkeit.Text);
        }
        // neues Material
        else
        {
            if (LeitfähigkeitX.Text.Length > 0)
                leitfähigkeit[0] = double.Parse(LeitfähigkeitX.Text);
            if (LeitfähigkeitY.Text.Length > 0)
                leitfähigkeit[1] = double.Parse(LeitfähigkeitY.Text);
            if (LeitfähigkeitZ.Text.Length > 0)
                leitfähigkeit[2] = double.Parse(LeitfähigkeitZ.Text);
            if (DichteLeitfähigkeit.Text.Length > 0)
                dichteLeitfähigkeit = double.Parse(DichteLeitfähigkeit.Text);
            material = new Material(materialId, leitfähigkeit, dichteLeitfähigkeit);
            modell.Material.Add(materialId, material);
        }

        materialKeys?.Close();
        Close();
    }

    private void BtnDialogCancel_Click(object sender, RoutedEventArgs e)
    {
        materialKeys?.Close();
        Close();
    }

    private void MaterialIdLostFocus(object sender, RoutedEventArgs e)
    {
        if (!modell.Material.ContainsKey(MaterialId.Text))
        {
            LeitfähigkeitX.Text = "";
            LeitfähigkeitY.Text = "";
            LeitfähigkeitZ.Text = "";
            DichteLeitfähigkeit.Text = "";
            return;
        }

        // vorhandene Materialdefinition
        modell.Material.TryGetValue(MaterialId.Text, out vorhandenesMaterial);
        Debug.Assert(vorhandenesMaterial != null, nameof(vorhandenesMaterial) + " != null");
        MaterialId.Text = "";

        MaterialId.Text = vorhandenesMaterial.MaterialId;

        LeitfähigkeitX.Text = vorhandenesMaterial.MaterialWerte[0].ToString("G3", CultureInfo.CurrentCulture);
        LeitfähigkeitY.Text = vorhandenesMaterial.MaterialWerte[1].ToString("G3", CultureInfo.CurrentCulture);
        LeitfähigkeitZ.Text = vorhandenesMaterial.MaterialWerte[2].ToString("G3", CultureInfo.CurrentCulture);
        DichteLeitfähigkeit.Text = vorhandenesMaterial.MaterialWerte[3].ToString("G3", CultureInfo.CurrentCulture);
    }

    private void BtnLöschen_Click(object sender, RoutedEventArgs e)
    {
        if (vorhandenesMaterial != null) modell.Material.Remove(vorhandenesMaterial.MaterialId);
        materialKeys.Close();
        Close();
    }
}