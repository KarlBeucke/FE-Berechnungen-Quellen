using FEBibliothek.Modell;
using System.Linq;
using FE_Berechnungen.Tragwerksberechnung.Modelldaten;

namespace FE_Berechnungen.Tragwerksberechnung.ModelldatenLesen;

public partial class LinienlastKeys
{
    public LinienlastKeys(FeModell modell)
    {
        InitializeComponent();
        this.Left = 2 * this.Width;
        this.Top = this.Height;
        var lasten = modell.ElementLasten.
            Where(item => item.Value is LinienLast).
            Select(item => item.Value).ToList();
        LinienlastKey.ItemsSource = lasten; 
    }
}