using FE_Berechnungen.Elastizitätsberechnung.Modelldaten;
using FEBibliothek.Modell;
using System.Windows;

namespace FE_Berechnungen.Elastizitätsberechnung.ModelldatenLesen;

public partial class NeueKnotenlast
{
    private readonly FeModell _modell;

    public NeueKnotenlast()
    {
        InitializeComponent();
        Show();
    }

    public NeueKnotenlast(FeModell modell, string last, string knoten,
        double px, double py, double pz)
    {
        InitializeComponent();
        _modell = modell;
        LastId.Text = last;
        KnotenId.Text = knoten;
        Px.Text = px.ToString("0.00");
        Py.Text = py.ToString("0.00");
        Pz.Text = pz.ToString("0.00");
        Show();
    }

    private void BtnDialogOk_Click(object sender, RoutedEventArgs e)
    {
        var loadId = LastId.Text;
        var nodeId = KnotenId.Text;
        var p = new double[3];
        p[0] = double.Parse(Px.Text);
        p[1] = double.Parse(Py.Text);
        p[2] = double.Parse(Pz.Text);
        var knotenLast = new KnotenLast(nodeId, p[0], p[1], p[2])
        {
            LastId = loadId
        };
        _modell.Lasten.Add(loadId, knotenLast);
        Close();
    }

    private void BtnDialogCancel_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}