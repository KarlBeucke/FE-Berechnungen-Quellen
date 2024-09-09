namespace FE_Berechnungen.Wärmeberechnung.ModelldatenAnzeigen;

public partial class DialogLöschWärmemodellobjekt
{
    private bool löschFlag;

    public DialogLöschWärmemodellobjekt(bool delete)
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