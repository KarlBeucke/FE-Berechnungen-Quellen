using FE_Berechnungen.Tragwerksberechnung.Modelldaten;

namespace FE_Berechnungen.Tragwerksberechnung.ModelldatenLesen;

public partial class MaterialKeys
{
    public string Id;
    public MaterialKeys(FeModell modell)
    {
        InitializeComponent();
        Left = 2 * Width;
        var material = modell.Material.Select(item => item.Value).ToList();
        MaterialKey.ItemsSource = material;
    }

    private void BtnDialogCancel_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        if (MaterialKey.SelectedItems.Count <= 0) return;
        var material = (Material)MaterialKey.SelectedItem;
        if (material != null) Id = material.MaterialId;
    }
}