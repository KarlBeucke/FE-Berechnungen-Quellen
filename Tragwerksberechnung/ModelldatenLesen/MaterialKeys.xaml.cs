using System.Linq;
using FEBibliothek.Modell;

namespace FE_Berechnungen.Tragwerksberechnung.ModelldatenLesen;

public partial class MaterialKeys
{
    public MaterialKeys(FeModell modell)
    {
        InitializeComponent();
        this.Left = 2 * this.Width;
        this.Top = this.Height;
        var material = modell.Material.Select(item => item.Value).ToList();
        MaterialKey.ItemsSource = material;
    }
}