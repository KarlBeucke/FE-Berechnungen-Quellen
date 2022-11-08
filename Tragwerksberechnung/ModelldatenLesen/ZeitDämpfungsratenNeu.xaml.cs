using FEBibliothek.Modell;
using System.Windows;

namespace FE_Berechnungen.Tragwerksberechnung.ModelldatenLesen;

public partial class ZeitDämpfungsratenNeu
{
    private readonly FeModell modell;
    public ZeitDämpfungsratenNeu(FeModell modell)
    {
        InitializeComponent();
        this.modell = modell;
        Show();
    }
    private void BtnDialogOk_Click(object sender, RoutedEventArgs e)
    {
        modell.Eigenzustand.DämpfungsRaten.
            Add(new ModaleWerte(double.Parse(Xi.Text)));

        Close();
    }
    private void BtnDialogCancel_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}