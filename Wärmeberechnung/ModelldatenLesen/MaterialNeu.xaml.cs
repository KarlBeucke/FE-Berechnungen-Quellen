using FE_Berechnungen.Wärmeberechnung.Modelldaten;
using System.Globalization;
using System.Linq;

namespace FE_Berechnungen.Wärmeberechnung.ModelldatenLesen;

public partial class MaterialNeu
{
    private readonly FeModell _modell;
    private AbstraktMaterial material, vorhandenesMaterial;

    public MaterialNeu(FeModell modell)
    {
        _modell = modell;
        InitializeComponent();
        Show();
    }

    private void BtnDialogOk_Click(object sender, RoutedEventArgs e)
    {
        if (_modell == null) return;
        var materialId = MaterialId.Text;
        if (materialId == "")
        {
            _ = MessageBox.Show("Material Id muss definiert sein", "neues Material");
            return;
        }

        var leitfähigkeit = new double[3];
        double dichteLeitfähigkeit = 0;
        // vorhandenes Material
        if (_modell.Material.TryGetValue(materialId, out vorhandenesMaterial))
        {
            try
            {
                if (LeitfähigkeitX.Text.Length > 0)
                    vorhandenesMaterial.MaterialWerte[0] = double.Parse(LeitfähigkeitX.Text);
                if (LeitfähigkeitY.Text.Length > 0)
                    vorhandenesMaterial.MaterialWerte[1] = double.Parse(LeitfähigkeitY.Text);
                if (LeitfähigkeitZ.Text.Length > 0)
                    vorhandenesMaterial.MaterialWerte[2] = double.Parse(LeitfähigkeitZ.Text);
                if (DichteLeitfähigkeit.Text.Length > 0)
                    vorhandenesMaterial.MaterialWerte[3] = double.Parse(DichteLeitfähigkeit.Text);
            }
            catch (FormatException)
            {
                _ = MessageBox.Show("ungültiges  Eingabeformat", "neues Material");
            }
        }
            
        // neues Material
        else
        {
            try
            {
                if (LeitfähigkeitX.Text.Length > 0)
                    leitfähigkeit[0] = double.Parse(LeitfähigkeitX.Text);
                if (LeitfähigkeitY.Text.Length > 0)
                    leitfähigkeit[1] = double.Parse(LeitfähigkeitY.Text);
                if (LeitfähigkeitZ.Text.Length > 0)
                    leitfähigkeit[2] = double.Parse(LeitfähigkeitZ.Text);
                if (DichteLeitfähigkeit.Text.Length > 0)
                    dichteLeitfähigkeit = double.Parse(DichteLeitfähigkeit.Text);
            }
            catch (FormatException)
            {
                _ = MessageBox.Show("ungültiges  Eingabeformat", "neues Material");
            }
            material = new Material(materialId, leitfähigkeit, dichteLeitfähigkeit);
            _modell.Material.Add(materialId, material);
        }
        Close();
    }

    private void BtnDialogCancel_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void MaterialIdLostFocus(object sender, RoutedEventArgs e)
    {
        if (!_modell.Material.ContainsKey(MaterialId.Text))
        {
            //LeitfähigkeitX.Text = "";
            //LeitfähigkeitY.Text = "";
            //LeitfähigkeitZ.Text = "";
            //DichteLeitfähigkeit.Text = "";
            return;
        }

        // vorhandene Materialdefinition
        if (!_modell.Material.TryGetValue(MaterialId.Text, out vorhandenesMaterial))
            throw new ModellAusnahme("\nMaterial '" + MaterialId.Text + "' nicht im Modell gefunden");
        MaterialId.Text = "";

        MaterialId.Text = vorhandenesMaterial.MaterialId;

        LeitfähigkeitX.Text = vorhandenesMaterial.MaterialWerte[0].ToString("G3", CultureInfo.CurrentCulture);
        LeitfähigkeitY.Text = vorhandenesMaterial.MaterialWerte[1].ToString("G3", CultureInfo.CurrentCulture);
        LeitfähigkeitZ.Text = vorhandenesMaterial.MaterialWerte[2].ToString("G3", CultureInfo.CurrentCulture);
        DichteLeitfähigkeit.Text = vorhandenesMaterial.MaterialWerte[3].ToString("G3", CultureInfo.CurrentCulture);
    }

    private void BtnLöschen_Click(object sender, RoutedEventArgs e)
    {
        if (!_modell.Material.ContainsKey(MaterialId.Text)) return;
        if (MaterialReferenziert()) return;
        _modell.Material.Remove(vorhandenesMaterial.MaterialId);
        Close();
    }
    private bool MaterialReferenziert()
    {
        var id = MaterialId.Text;
        foreach (var element in _modell.Elemente.Where(element => element.Value.ElementMaterialId == id))
        {
            _ = MessageBox.Show(
                "Material referenziert durch Element " + element.Value.ElementId + ", kann nicht gelöscht werden",
                "neues Material");
            return true;
        }
        return false;
    }
}