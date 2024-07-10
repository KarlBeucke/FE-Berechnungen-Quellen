using System.Linq;
using FE_Berechnungen.Tragwerksberechnung.Modelldaten;
using FEBibliothek.Modell;
using FEBibliothek.Modell.abstrakte_Klassen;

namespace FE_Berechnungen.Tragwerksberechnung.ModelldatenLesen;

public partial class TragwerkLastenKeys
{
    public TragwerkLastenKeys(FeModell modell)
    {
        InitializeComponent();
        Left = 2 * Width;
        Top = Height;
        var lasten = modell.Lasten.Where(item => item.Value is KnotenLast).Select(item => item.Value).ToList();
        var linienlasten = modell.ElementLasten.Where(item => item.Value is LinienLast)
            .Select(item => (AbstraktLast)item.Value).ToList();
        lasten.AddRange(linienlasten);
        var punktlasten = modell.PunktLasten.Where(item => item.Value is PunktLast)
            .Select(item => (AbstraktLast)item.Value).ToList();
        lasten.AddRange(punktlasten);
        TragwerklastenKeys.ItemsSource = lasten;
    }
}