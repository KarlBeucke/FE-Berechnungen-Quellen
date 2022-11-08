using FEBibliothek.Modell;
using System.Linq;

namespace FE_Berechnungen.Tragwerksberechnung.ModelldatenLesen;

public partial class LagerKeys
{
    public LagerKeys(FeModell modell)
    {
        InitializeComponent();
        this.Left = 2 * this.Width;
        this.Top = this.Height;
        var lager = modell.Randbedingungen.Select(item => item.Value).ToList();
        LagerKey.ItemsSource = lager;
    }
}