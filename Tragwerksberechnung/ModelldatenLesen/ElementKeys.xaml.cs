namespace FE_Berechnungen.Tragwerksberechnung.ModelldatenLesen;

public partial class ElementKeys
{
    public ElementKeys(FeModell modell)
    {
        InitializeComponent();
        Left = 2 * Width;
        Top = Height;
        var elemente = modell.Elemente.Select(item => item.Value).ToList();
        ElementKey.ItemsSource = elemente;
    }

    private void BtnDialogCancel_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}