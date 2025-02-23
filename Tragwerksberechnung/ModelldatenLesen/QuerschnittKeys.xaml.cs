namespace FE_Berechnungen.Tragwerksberechnung.ModelldatenLesen;

public partial class QuerschnittKeys
{
    public string Id;
    public QuerschnittKeys(FeModell modell)
    {
        InitializeComponent();
        Left = 2 * Width;
        Top = Height;
        var querschnitt = modell.Querschnitt.Select(item => item.Value).ToList();
        QuerschnittKey.ItemsSource = querschnitt;
    }

    private void BtnDialogCancel_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        if (QuerschnittKey.SelectedItems.Count <= 0) return;
        var querschnitt = (Querschnitt)QuerschnittKey.SelectedItem;
        if (querschnitt != null) Id = querschnitt.QuerschnittId;
    }
}