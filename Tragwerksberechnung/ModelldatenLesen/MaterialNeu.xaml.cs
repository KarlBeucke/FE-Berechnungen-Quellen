using FE_Berechnungen.Tragwerksberechnung.Modelldaten;
using System.Diagnostics;
using System.Globalization;
using System.Linq;

namespace FE_Berechnungen.Tragwerksberechnung.ModelldatenLesen;

public partial class MaterialNeu
{
    private readonly FeModell _modell;
    private AbstraktMaterial _material, _vorhandenesMaterial;

    public MaterialNeu(FeModell modell)
    {
        InitializeComponent();
        _modell = modell;
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

        // vorhandenes Material
        if (_modell.Material.ContainsKey(MaterialId.Text))
        {
            if (!_modell.Material.TryGetValue(materialId, out _vorhandenesMaterial))
                throw new ModellAusnahme("\nMaterial '" + materialId + "' nicht im Modell gefunden");

            try
            {
                if (EModul.Text.Length > 0) _vorhandenesMaterial.MaterialWerte[0] = double.Parse(EModul.Text);
                if (Poisson.Text.Length > 0) _vorhandenesMaterial.MaterialWerte[1] = double.Parse(Poisson.Text);
                if (Masse.Text.Length > 0) _vorhandenesMaterial.MaterialWerte[2] = double.Parse(Masse.Text);
                if (FederX.Text.Length > 0) _vorhandenesMaterial.MaterialWerte[3] = double.Parse(FederX.Text);
                if (FederY.Text.Length > 0) _vorhandenesMaterial.MaterialWerte[4] = double.Parse(FederY.Text);
                if (FederPhi.Text.Length > 0) _vorhandenesMaterial.MaterialWerte[5] = double.Parse(FederPhi.Text);
            }
            catch (FormatException)
            {
                _ = MessageBox.Show("ungültiges  Eingabeformat", "neues Material");
            }
        }
        // neues Material
        else
        {
            if (EModul.Text.Length > 0)
            {
                var eModul = double.Parse(EModul.Text);
                double poisson = 0, masse = 0;
                try
                {
                    if (Poisson.Text.Length > 0) poisson = double.Parse(Poisson.Text);
                    if (Masse.Text.Length > 0) masse = double.Parse(Masse.Text);
                }
                catch (FormatException)
                {
                    _ = MessageBox.Show("ungültiges  Eingabeformat", "neues Material");
                }
                _material = new Material(eModul, poisson, masse)
                {
                    MaterialId = materialId
                };
                _modell.Material.Add(materialId, _material);
                FederX.Text = "";
                FederY.Text = "";
                FederPhi.Text = "";
            }
            else if ((FederX.Text.Length > 0) | (FederY.Text.Length > 0) | (FederPhi.Text.Length > 0))
            {
                EModul.Text = "";
                Poisson.Text = "";
                Masse.Text = "";
                double federX = 0, federY = 0, federPhi = 0;
                try
                {
                    if (FederX.Text.Length > 0) federX = double.Parse(FederX.Text);
                    if (FederY.Text.Length > 0) federY = double.Parse(FederY.Text);
                    if (FederPhi.Text.Length > 0) federPhi = double.Parse(FederPhi.Text);
                }
                catch (FormatException)
                {
                    _ = MessageBox.Show("ungültiges  Eingabeformat", "neues Material");
                }
                _material = new Material(true, federX, federY, federPhi)
                {
                    MaterialId = materialId
                };
                _modell.Material.Add(materialId, _material);
            }
            else
            {
                _ = MessageBox.Show("entweder E-Modul oder 1 Federsteifigkeit müssen definiert sein", "neues Material");
                return;
            }
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
            EModul.Text = "";
            Poisson.Text = "";
            Masse.Text = "";
            FederX.Text = "";
            FederY.Text = "";
            FederPhi.Text = "";
            return;
        }

        // vorhandene Materialdefinition
        if(!_modell.Material.TryGetValue(MaterialId.Text, out _vorhandenesMaterial))
            throw new ModellAusnahme("\nMaterial '" + MaterialId.Text + "' nicht im Modell gefunden");
        MaterialId.Text = "";

        MaterialId.Text = _vorhandenesMaterial.MaterialId;
        if (!_vorhandenesMaterial.Feder)
        {
            EModul.Text = _vorhandenesMaterial.MaterialWerte[0].ToString("G3", CultureInfo.CurrentCulture);
            if (Poisson.Text == "")
                Poisson.Text = _vorhandenesMaterial.MaterialWerte[1].ToString("G3", CultureInfo.CurrentCulture);
            Masse.Text = _vorhandenesMaterial.MaterialWerte[2].ToString("G3", CultureInfo.CurrentCulture);
            FederX.Text = "";
            FederY.Text = "";
            FederPhi.Text = "";
        }
        else
        {
            EModul.Text = "";
            Poisson.Text = "";
            Masse.Text = "";
            FederX.Text = _vorhandenesMaterial.MaterialWerte[0].ToString("G3", CultureInfo.CurrentCulture);
            FederY.Text = _vorhandenesMaterial.MaterialWerte[1].ToString("G3", CultureInfo.CurrentCulture);
            FederPhi.Text = _vorhandenesMaterial.MaterialWerte[2].ToString("G3", CultureInfo.CurrentCulture);
        }
    }

    private void BtnLöschen_Click(object sender, RoutedEventArgs e)
    {
        if (!_modell.Material.ContainsKey(MaterialId.Text)) return;
        if (MaterialReferenziert()) return;
        _modell.Material.Remove(_vorhandenesMaterial.MaterialId);
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

        //if (_modell.Elemente.All(element => element.Value.ElementMaterialId != id)) return false;
        return false;
    }
}