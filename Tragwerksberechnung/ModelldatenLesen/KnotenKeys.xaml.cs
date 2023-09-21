using FEBibliothek.Modell;
using System.Linq;

namespace FE_Berechnungen.Tragwerksberechnung.ModelldatenLesen;

public partial class KnotenKeys
{
    public KnotenKeys(FeModell modell)
    {
        InitializeComponent();
        this.Left = 2 * this.Width;
        this.Top = this.Height;
        var knoten = modell.Knoten.Select(item => item.Value).ToList();
        KnotenKey.ItemsSource = knoten;
    }
}