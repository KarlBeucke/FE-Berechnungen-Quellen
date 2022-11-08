using System.Windows;

namespace FE_Berechnungen.Tragwerksberechnung.ModelldatenAnzeigen;

public partial class DialogLöschStrukturobjekte
{
    public bool löschFlag;

    public DialogLöschStrukturobjekte(bool lösch)
    {
        löschFlag = lösch;
        InitializeComponent();
        Show();
    }

    private void BtnDialogOk_Click(object sender, RoutedEventArgs e)
    {
        löschFlag = false;
        Close();
    }

    private void BtnDialogCancel_Click(object sender, RoutedEventArgs e)
    {
        löschFlag = false;
        Close();
    }
}