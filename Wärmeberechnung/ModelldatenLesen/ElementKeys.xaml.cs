using FEBibliothek.Modell;
using System.Linq;

namespace FE_Berechnungen.Wärmeberechnung.ModelldatenLesen;

public partial class ElementKeys
{
    public ElementKeys(FeModell modell)
    {
        InitializeComponent();
        this.Left = 2 * this.Width;
        this.Top = this.Height;
        var elemente = modell.Elemente.Select(item => item.Value).ToList();
        ElementKey.ItemsSource = elemente;
    }
}