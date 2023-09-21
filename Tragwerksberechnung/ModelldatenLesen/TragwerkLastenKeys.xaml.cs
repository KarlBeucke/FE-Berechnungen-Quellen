using FE_Berechnungen.Tragwerksberechnung.Modelldaten;
using FEBibliothek.Modell;
using FEBibliothek.Modell.abstrakte_Klassen;
using System.Linq;

namespace FE_Berechnungen.Tragwerksberechnung.ModelldatenLesen;

public partial class TragwerkLastenKeys
{
    public TragwerkLastenKeys(FeModell modell)
    {
        InitializeComponent();
        this.Left = 2 * this.Width;
        this.Top = this.Height;
        var lasten = modell.Lasten.Where(item => item.Value is KnotenLast).
            Select(item => (AbstraktLast)item.Value).ToList();
        var linienlasten = modell.ElementLasten.Where(item => item.Value is LinienLast).
            Select(item => (AbstraktLast)item.Value).ToList();
        lasten.AddRange(linienlasten);
        var punktlasten = modell.PunktLasten.Where(item => item.Value is PunktLast).
            Select(item => (AbstraktLast)item.Value).ToList();
        lasten.AddRange(punktlasten);
        TragwerklastenKeys.ItemsSource = lasten;
    }
}