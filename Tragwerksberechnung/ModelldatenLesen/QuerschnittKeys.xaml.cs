namespace FE_Berechnungen.Tragwerksberechnung.ModelldatenLesen;

public partial class QuerschnittKeys
{
    private readonly FeModell _modell;
    public string Id;
    public QuerschnittKeys(FeModell modell)
    {
        InitializeComponent();
        _modell = modell;
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
        _modell.Berechnet = false;
    }

    private void MouseDoubleClickNeuesMaterial(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        var querschnittNeu = new QuerschnittNeu(_modell) { Topmost = true, Owner = (Window)Parent };
        querschnittNeu.AktuelleId = querschnittNeu.QuerschnittId.Text;
        Close();
    }
}