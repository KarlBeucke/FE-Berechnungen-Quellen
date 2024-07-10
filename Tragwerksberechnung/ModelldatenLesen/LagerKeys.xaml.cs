using System.Linq;
using FEBibliothek.Modell;

namespace FE_Berechnungen.Tragwerksberechnung.ModelldatenLesen;

public partial class LagerKeys
{
    public LagerKeys(FeModell modell)
    {
        InitializeComponent();
        Left = 2 * Width;
        Top = Height;
        var lager = modell.Randbedingungen.Select(item => item.Value).ToList();
        LagerKey.ItemsSource = lager;
    }
}