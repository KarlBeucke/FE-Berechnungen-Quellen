using System.Linq;

namespace FE_Berechnungen.Tragwerksberechnung.ModelldatenLesen;

public partial class KnotenKeys
{
    public KnotenKeys(FeModell modell)
    {
        InitializeComponent();
        Left = 2 * Width;
        Top = Height;
        var knoten = modell.Knoten.Select(item => item.Value).ToList();
        KnotenKey.ItemsSource = knoten;
    }

    private void BtnDialogCancel_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}