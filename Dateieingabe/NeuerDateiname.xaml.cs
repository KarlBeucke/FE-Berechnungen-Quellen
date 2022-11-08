using System.Windows;

namespace FE_Berechnungen.Dateieingabe;

public partial class NeuerDateiname
{
    public string dateiName;
    public NeuerDateiname()
    {
        InitializeComponent();
    }

    private void BtnDialogOk_Click(object sender, RoutedEventArgs e)
    {
        dateiName = Dateiname.Text;
        DialogResult = true;
    }

    private void BtnDialogCancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
    }
}