using System.Globalization;
using FEBibliothek.Modell;
using System.Windows;

namespace FE_Berechnungen.Tragwerksberechnung.ModelldatenLesen;

public partial class ZeitDämpfungsratenNeu
{
    private readonly FeModell modell;
    private int eigenform;
    public ZeitDämpfungsratenNeu(FeModell modell)
    {
        InitializeComponent();
        this.modell = modell;
        eigenform = StartFenster.tragwerkVisual.zeitintegrationNeu.eigenForm;
        if (eigenform > modell.Eigenzustand.DämpfungsRaten.Count)
        {
            Xi.Text = "";
        }
        else
        {
            var anfang = (ModaleWerte)modell.Eigenzustand.DämpfungsRaten[eigenform - 1];
            Xi.Text = anfang.Dämpfung.ToString(CultureInfo.CurrentCulture);
        }
        ShowDialog();
    }
    private void BtnDialogOk_Click(object sender, RoutedEventArgs e)
    {
        // neues Dämpfungsmaß hinzufügen
        if (eigenform > modell.Eigenzustand.DämpfungsRaten.Count)
        {
            modell.Eigenzustand.DämpfungsRaten.Add(new ModaleWerte(double.Parse(Xi.Text)));
        }
        // vorhandenes Dämpfungsmaß ändern
        else
        {
            var anfang = (ModaleWerte)modell.Eigenzustand.DämpfungsRaten[eigenform];
            anfang.Dämpfung = double.Parse(Xi.Text);
        }
        Close();
    }
    private void BtnDialogCancel_Click(object sender, RoutedEventArgs e)
    {
        Close();
        StartFenster.tragwerkVisual.zeitintegrationNeu.Close();
    }

    private void BtnLöschen_Click(object sender, RoutedEventArgs e)
    {
        modell.Eigenzustand.DämpfungsRaten.RemoveAt(eigenform);
        eigenform = 0;
        if (modell.Eigenzustand.DämpfungsRaten.Count <= 0)
        {
            Close();
            StartFenster.tragwerkVisual.zeitintegrationNeu.Close();
            return;
        }
        var anfangsWerte = (ModaleWerte)modell.Eigenzustand.DämpfungsRaten[eigenform];
        Xi.Text = anfangsWerte.Dämpfung.ToString("G2");
        Close();
        StartFenster.tragwerkVisual.zeitintegrationNeu.Close();
    }
}