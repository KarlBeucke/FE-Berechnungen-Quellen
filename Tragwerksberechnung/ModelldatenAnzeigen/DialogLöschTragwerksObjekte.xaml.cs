using System.Windows;

namespace FE_Berechnungen.Tragwerksberechnung.ModelldatenAnzeigen;

public partial class DialogLöschTragwerksObjekte
{
    public bool löschFlag;

    public DialogLöschTragwerksObjekte(bool delete)
    {
        löschFlag = delete;
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