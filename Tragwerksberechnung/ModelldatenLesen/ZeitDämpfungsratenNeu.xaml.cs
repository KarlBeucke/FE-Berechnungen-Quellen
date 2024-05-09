using FEBibliothek.Modell;
using System.Globalization;
using System.Windows;

namespace FE_Berechnungen.Tragwerksberechnung.ModelldatenLesen;

public partial class ZeitDämpfungsratenNeu
{
    private readonly FeModell _modell;
    private int _eigenform;
    public ZeitDämpfungsratenNeu(FeModell modell)
    {
        InitializeComponent();
        _modell = modell;
        _eigenform = StartFenster.TragwerkVisual.ZeitintegrationNeu.EigenForm;
        if (_eigenform > modell.Eigenzustand.DämpfungsRaten.Count)
        {
            Xi.Text = "";
        }
        else
        {
            var anfang = (ModaleWerte)modell.Eigenzustand.DämpfungsRaten[_eigenform - 1];
            Xi.Text = anfang.Dämpfung.ToString(CultureInfo.CurrentCulture);
        }
        ShowDialog();
    }
    private void BtnDialogOk_Click(object sender, RoutedEventArgs e)
    {
        // neues Dämpfungsmaß hinzufügen
        if (_eigenform > _modell.Eigenzustand.DämpfungsRaten.Count)
        {
            _modell.Eigenzustand.DämpfungsRaten.Add(new ModaleWerte(double.Parse(Xi.Text)));
        }
        // vorhandenes Dämpfungsmaß ändern
        else
        {
            var anfang = (ModaleWerte)_modell.Eigenzustand.DämpfungsRaten[_eigenform];
            anfang.Dämpfung = double.Parse(Xi.Text);
        }
        Close();
    }
    private void BtnDialogCancel_Click(object sender, RoutedEventArgs e)
    {
        Close();
        StartFenster.TragwerkVisual.ZeitintegrationNeu.Close();
    }

    private void BtnLöschen_Click(object sender, RoutedEventArgs e)
    {
        _modell.Eigenzustand.DämpfungsRaten.RemoveAt(_eigenform);
        _eigenform = 0;
        if (_modell.Eigenzustand.DämpfungsRaten.Count <= 0)
        {
            Close();
            StartFenster.TragwerkVisual.ZeitintegrationNeu.Close();
            return;
        }
        var anfangsWerte = (ModaleWerte)_modell.Eigenzustand.DämpfungsRaten[_eigenform];
        Xi.Text = anfangsWerte.Dämpfung.ToString("G2");
        Close();
        StartFenster.TragwerkVisual.ZeitintegrationNeu.Close();
    }
}