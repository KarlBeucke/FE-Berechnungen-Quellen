using FE_Berechnungen.Elastizitätsberechnung.Modelldaten;
using System.Windows.Input;

namespace FE_Berechnungen.Elastizitätsberechnung.ModelldatenLesen
{
    public partial class MaterialKeys
    {
        private readonly FeModell _modell;
        public string Id;

        public MaterialKeys(FeModell modell)
        {
            InitializeComponent();
            _modell = modell;
            Left = 2 * Width;
            var material = _modell.Material.Select(item => item.Value).ToList();
            MaterialKey.ItemsSource = material;
        }

        private void BtnDialogCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (MaterialKey.SelectedItems.Count <= 0)
            {
                return;
            }

            var material = (Material)MaterialKey.SelectedItem;
            if (material == null) return;
            Id = material.MaterialId;
            _modell.Berechnet = false;
        }

        private void MouseDoubleClickNeuesMaterial(object sender, MouseButtonEventArgs e)
        {
            var materialNeu = new MaterialNeu(_modell) { Topmost = true, Owner = (Window)Parent };
            materialNeu.AktuelleMaterialId = materialNeu.MaterialId.Text;
            Close();
        }
    }
}