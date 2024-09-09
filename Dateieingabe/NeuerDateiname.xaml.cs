namespace FE_Berechnungen.Dateieingabe;

public partial class NeuerDateiname
{
    public string DateiName;

    public NeuerDateiname()
    {
        InitializeComponent();
    }

    private void BtnDialogOk_Click(object sender, RoutedEventArgs e)
    {
        DateiName = Dateiname.Text;
        DialogResult = true;
    }

    private void BtnDialogCancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
    }
}