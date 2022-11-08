using FE_Berechnungen.Tragwerksberechnung.Modelldaten;
using FEBibliothek.Modell;
using FEBibliothek.Modell.abstrakte_Klassen;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Windows;

namespace FE_Berechnungen.Tragwerksberechnung.ModelldatenLesen;

public partial class MaterialNeu
{
    private readonly FeModell modell;
    private AbstraktMaterial material, vorhandenesMaterial;
    private readonly MaterialKeys materialKeys;

    public MaterialNeu(FeModell modell)
    {
        InitializeComponent();
        this.modell = modell;
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

        // vorhandenes Material
        if (modell.Material.Keys.Contains(MaterialId.Text))
        {
            modell.Material.TryGetValue(materialId, out vorhandenesMaterial);
            Debug.Assert(vorhandenesMaterial != null, nameof(vorhandenesMaterial) + " != null");

            if (EModul.Text.Length > 0) vorhandenesMaterial.MaterialWerte[0] = double.Parse(EModul.Text);
            if (Poisson.Text.Length > 0) vorhandenesMaterial.MaterialWerte[1] = double.Parse(Poisson.Text);
            if (Masse.Text.Length > 0) vorhandenesMaterial.MaterialWerte[2] = double.Parse(Masse.Text);
            if (FederX.Text.Length > 0) vorhandenesMaterial.MaterialWerte[3] = double.Parse(FederX.Text);
            if (FederY.Text.Length > 0) vorhandenesMaterial.MaterialWerte[4] = double.Parse(FederY.Text);
            if (FederPhi.Text.Length > 0) vorhandenesMaterial.MaterialWerte[5] = double.Parse(FederPhi.Text);
        }
        // neues Material
        else
        {
            if (EModul.Text.Length > 0)
            {
                var eModul = double.Parse(EModul.Text);
                double poisson=0, masse=0;
                if (Poisson.Text.Length > 0) poisson = double.Parse(Poisson.Text);
                if (Masse.Text.Length > 0) masse = double.Parse(Masse.Text);
                material = new Material(eModul, poisson, masse)
                {
                    MaterialId = materialId
                };
                modell.Material.Add(materialId, material);
                FederX.Text = "";
                FederY.Text = "";
                FederPhi.Text = "";
            }
            else if(FederX.Text.Length > 0 | FederY.Text.Length > 0 | FederPhi.Text.Length > 0)
            {
                EModul.Text = "";
                Poisson.Text = "";
                Masse.Text = "";
                double federX = 0, federY = 0, federPhi = 0;
                if (FederX.Text.Length > 0) federX = double.Parse(FederX.Text);
                if (FederY.Text.Length > 0) federY = double.Parse(FederY.Text);
                if (FederPhi.Text.Length > 0) federPhi = double.Parse(FederPhi.Text);
                material = new Material(true, federX, federY, federPhi)
                {
                    MaterialId = materialId
                };
                modell.Material.Add(materialId, material);
            }
            else
            {
                _ = MessageBox.Show("entweder E-Modul oder 1 Federsteifigkeit müssen definiert sein", "neues Material");
                return;
            }
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
            EModul.Text = "";
            Poisson.Text = "";
            Masse.Text = "";
            FederX.Text = "";
            FederY.Text = "";
            FederPhi.Text = "";
            return;
        }

        // vorhandene Materialdefinition
        modell.Material.TryGetValue(MaterialId.Text, out vorhandenesMaterial);
        Debug.Assert(vorhandenesMaterial != null, nameof(vorhandenesMaterial) + " != null"); MaterialId.Text = "";

        MaterialId.Text = vorhandenesMaterial.MaterialId;
        if (!vorhandenesMaterial.Feder)
        {
            EModul.Text = vorhandenesMaterial.MaterialWerte[0].ToString("G3", CultureInfo.CurrentCulture);
            if(Poisson.Text =="") Poisson.Text = vorhandenesMaterial.MaterialWerte[1].ToString("G3", CultureInfo.CurrentCulture);
            Masse.Text = vorhandenesMaterial.MaterialWerte[2].ToString("G3", CultureInfo.CurrentCulture);
            FederX.Text = "";
            FederY.Text = "";
            FederPhi.Text = "";
        }
        else
        {
            EModul.Text = "";
            Poisson.Text = "";
            Masse.Text = "";
            FederX.Text = vorhandenesMaterial.MaterialWerte[0].ToString("G3", CultureInfo.CurrentCulture);
            FederY.Text = vorhandenesMaterial.MaterialWerte[1].ToString("G3", CultureInfo.CurrentCulture);
            FederPhi.Text = vorhandenesMaterial.MaterialWerte[2].ToString("G3", CultureInfo.CurrentCulture);
        }
    }

    private void BtnLöschen_Click(object sender, RoutedEventArgs e)
    {
        if (vorhandenesMaterial != null) modell.Material.Remove(vorhandenesMaterial.MaterialId);
        materialKeys.Close();
        Close();
    }
}