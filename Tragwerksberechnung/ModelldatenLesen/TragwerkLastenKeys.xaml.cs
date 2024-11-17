using FE_Berechnungen.Tragwerksberechnung.Modelldaten;

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
            .Select(AbstraktLast (item) => item.Value).ToList();
        lasten.AddRange(linienlasten);
        var punktlasten = modell.PunktLasten.Where(item => item.Value is PunktLast)
            .Select(AbstraktLast (item) => item.Value).ToList();
        lasten.AddRange(punktlasten);
        TragwerklastenKeys.ItemsSource = lasten;
    }

    private void BtnDialogCancel_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}