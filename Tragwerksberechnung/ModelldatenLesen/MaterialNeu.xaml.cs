using FE_Berechnungen.Tragwerksberechnung.ModelldatenAnzeigen;
using System.Globalization;
using Material = FE_Berechnungen.Tragwerksberechnung.Modelldaten.Material;

namespace FE_Berechnungen.Tragwerksberechnung.ModelldatenLesen;

public partial class MaterialNeu
{
    private readonly FeModell _modell;
    //private MaterialKeys _materialKeys;
    public string AktuelleId;

    public MaterialNeu(FeModell modell)
    {
        InitializeComponent();
        _modell = modell;
        AktuelleId = "";
        Show();
    }
    public MaterialNeu(FeModell modell, AbstraktMaterial material)
    {
        InitializeComponent();
        _modell = modell;
        MaterialId.Text = material.MaterialId;
        AktuelleId = material.MaterialId;
        if (!material.Feder)
        {
            EModul.Text = material.MaterialWerte[0].ToString("G3", CultureInfo.CurrentCulture);
            Poisson.Text = material.MaterialWerte[1].ToString("G3", CultureInfo.CurrentCulture);
            Masse.Text = material.MaterialWerte[2].ToString("G3", CultureInfo.CurrentCulture);
            FederX.Text = "";
            FederY.Text = "";
            FederPhi.Text = "";
        }
        else
        {
            EModul.Text = "";
            Poisson.Text = "";
            Masse.Text = "";
            FederX.Text = material.MaterialWerte[0].ToString("G3", CultureInfo.CurrentCulture);
            FederY.Text = material.MaterialWerte[1].ToString("G3", CultureInfo.CurrentCulture);
            FederPhi.Text = material.MaterialWerte[2].ToString("G3", CultureInfo.CurrentCulture);
        }
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
        if (_modell.Material.TryGetValue(materialId, out var vorhandenesMaterial))
        {
            try
            {
                if (EModul.Text.Length > 0) vorhandenesMaterial.MaterialWerte[0] = double.Parse(EModul.Text);
                if (Poisson.Text.Length > 0) vorhandenesMaterial.MaterialWerte[1] = double.Parse(Poisson.Text);
                if (Masse.Text.Length > 0) vorhandenesMaterial.MaterialWerte[2] = double.Parse(Masse.Text);
                if (FederX.Text.Length > 0) vorhandenesMaterial.MaterialWerte[0] = double.Parse(FederX.Text);
                if (FederY.Text.Length > 0) vorhandenesMaterial.MaterialWerte[1] = double.Parse(FederY.Text);
                if (FederPhi.Text.Length > 0) vorhandenesMaterial.MaterialWerte[2] = double.Parse(FederPhi.Text);
            }
            catch (FormatException)
            {
                _ = MessageBox.Show("ungültiges  Eingabeformat", "neues Material");
                return;
            }
        }
        // neues Material
        else
        {
            if (EModul.Text.Length > 0)
            {
                double eModul = 0, poisson = 0, masse = 0;
                try
                {
                    eModul = double.Parse(EModul.Text);
                    if (Poisson.Text.Length > 0) poisson = double.Parse(Poisson.Text);
                    if (Masse.Text.Length > 0) masse = double.Parse(Masse.Text);
                }
                catch (FormatException)
                {
                    _ = MessageBox.Show("ungültiges  Eingabeformat", "neues Material");
                }
                var material = new Material(eModul, poisson, masse)
                {
                    MaterialId = materialId
                };
                _modell.Material.Add(materialId, material);
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
                var material = new Material(true, federX, federY, federPhi)
                {
                    MaterialId = materialId
                };
                _modell.Material.Add(materialId, material);
            }
            else
            {
                _ = MessageBox.Show("entweder E-Modul oder 1 Federsteifigkeit müssen definiert sein", "neues Material");
                return;
            }
        }
        if (AktuelleId != MaterialId.Text) _modell.Material.Remove(AktuelleId);

        Close();
        StartFenster.TragwerkVisual.Close();
        StartFenster.TragwerkVisual = new TragwerkmodellVisualisieren(_modell);
        StartFenster.TragwerkVisual.Show();
        _modell.Berechnet = false;
    }

    private void BtnDialogCancel_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }


    //private void MaterialIdGotFocus(object sender, RoutedEventArgs e)
    //{
    //    _materialKeys = new MaterialKeys(_modell) { Topmost = true, Owner = (Window)Parent };
    //    _materialKeys.Show();
    //    MaterialId.Focus();
    //}
    private void MaterialIdLostFocus(object sender, RoutedEventArgs e)
    {
        //_materialKeys?.Close();
        if (!_modell.Material.TryGetValue(MaterialId.Text, out var vorhandenesMaterial)) return;

        // vorhandene Materialdefinition
        MaterialId.Text = vorhandenesMaterial.MaterialId;
        if (!vorhandenesMaterial.Feder)
        {
            EModul.Text = vorhandenesMaterial.MaterialWerte[0].ToString("G3", CultureInfo.CurrentCulture);
            Poisson.Text = vorhandenesMaterial.MaterialWerte[1].ToString("G3", CultureInfo.CurrentCulture);
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
            FederX.Text = vorhandenesMaterial.MaterialWerte[3].ToString("G3", CultureInfo.CurrentCulture);
            FederY.Text = vorhandenesMaterial.MaterialWerte[4].ToString("G3", CultureInfo.CurrentCulture);
            FederPhi.Text = vorhandenesMaterial.MaterialWerte[5].ToString("G3", CultureInfo.CurrentCulture);
        }
    }

    private void BtnLöschen_Click(object sender, RoutedEventArgs e)
    {
        if (MaterialReferenziert() || !_modell.Material.Remove(MaterialId.Text))
        {
            Close(); return;
        }
        Close();
        _modell.Berechnet = false;
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