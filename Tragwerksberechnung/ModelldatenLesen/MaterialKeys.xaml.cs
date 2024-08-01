using FEBibliothek.Modell;
using System.Linq;

namespace FE_Berechnungen.Tragwerksberechnung.ModelldatenLesen;

public partial class MaterialKeys
{
    public MaterialKeys(FeModell modell)
    {
        InitializeComponent();
        Left = 2 * Width;
        Top = Height;
        var material = modell.Material.Select(item => item.Value).ToList();
        MaterialKey.ItemsSource = material;
    }
}