using FE_Berechnungen.Wärmeberechnung.Modelldaten;
using System.Collections.Generic;
using System.Linq;

namespace FE_Berechnungen.Wärmeberechnung.ModelldatenLesen;

public partial class WärmelastenKeys
{
    public WärmelastenKeys(FeModell modell)
    {
        InitializeComponent();
        Left = 2 * Width;
        Top = Height;

        var lasten = new List<AbstraktLast>();
        var knotenlasten = modell.Lasten.Where(item => item.Value is KnotenLast).Select(item => item.Value).ToList();
        lasten.AddRange(knotenlasten);
        var linienlasten = modell.LinienLasten.Where(item => item.Value is LinienLast)
            .Select(item => (AbstraktLast)item.Value).ToList();
        lasten.AddRange(linienlasten);
        var elementlasten = modell.ElementLasten.Where(item => item.Value is ElementLast3)
            .Select(item => (AbstraktLast)item.Value).ToList();
        lasten.AddRange(elementlasten);
        elementlasten = modell.ElementLasten.Where(item => item.Value is ElementLast4)
            .Select(item => (AbstraktLast)item.Value).ToList();
        lasten.AddRange(elementlasten);
        var zeitKnotenlasten = modell.ZeitabhängigeKnotenLasten.Select(item => (AbstraktLast)item.Value).ToList();
        lasten.AddRange(zeitKnotenlasten);
        var zeitElementlasten = modell.ZeitabhängigeElementLasten.Select(item => (AbstraktLast)item.Value).ToList();
        lasten.AddRange(zeitElementlasten);
        WärmelastenKey.ItemsSource = lasten;
    }
}