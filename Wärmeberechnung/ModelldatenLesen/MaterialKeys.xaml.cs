using System.Linq;
using FEBibliothek.Modell;

namespace FE_Berechnungen.Wärmeberechnung.ModelldatenLesen;

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