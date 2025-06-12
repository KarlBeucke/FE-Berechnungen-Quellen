namespace FE_Berechnungen.Tragwerksberechnung.ModelldatenAnzeigen;

public partial class DialogLöschTragwerksObjekte
{
    public bool LöschFlag;

    public DialogLöschTragwerksObjekte(bool delete)
    {
        LöschFlag = delete;
        InitializeComponent();
        Show();
    }

    private void BtnDialogOk_Click(object sender, RoutedEventArgs e)
    {
        LöschFlag = false;
        Close();
    }

    private void BtnDialogCancel_Click(object sender, RoutedEventArgs e)
    {
        LöschFlag = false;
        Close();
    }
}