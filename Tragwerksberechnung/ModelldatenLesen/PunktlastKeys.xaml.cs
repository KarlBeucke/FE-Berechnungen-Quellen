using FEBibliothek.Modell;
using System.Linq;

namespace FE_Berechnungen.Tragwerksberechnung.ModelldatenLesen;

public partial class PunktlastKeys
{
    public PunktlastKeys(FeModell modell)
    {
        InitializeComponent();
        this.Left = 2 * this.Width;
        this.Top = this.Height;
        var punktlasten = modell.PunktLasten.Select(item => item.Value).ToList();
        PunktlastKey.ItemsSource = punktlasten;
    }
}