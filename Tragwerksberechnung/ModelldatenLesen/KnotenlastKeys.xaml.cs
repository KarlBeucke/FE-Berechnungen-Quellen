using System.Linq;
using FEBibliothek.Modell;

namespace FE_Berechnungen.Tragwerksberechnung.ModelldatenLesen;

public partial class KnotenlastKeys
{
    public KnotenlastKeys(FeModell modell)
    {
        InitializeComponent();
        this.Left = 2 * this.Width;
        this.Top = this.Height;
        var lasten = modell.Lasten.Select(item => item.Value).ToList();
        LastKey.ItemsSource = lasten;
    }
}