using System.Diagnostics;
using FEBibliothek.Modell;
using System.Windows;
using System.Globalization;

namespace FE_Berechnungen.Tragwerksberechnung.ModelldatenLesen;

public partial class KnotenNeu
{
    private readonly FeModell modell;
    public KnotenNeu()
    {
        InitializeComponent();
    }
    public KnotenNeu(FeModell feModell)
    {
        InitializeComponent();
        modell = feModell;
        // aktiviere Ereignishandler für Canvas
        StartFenster.tragwerksModell.VisualTragwerkModel.Background = System.Windows.Media.Brushes.Transparent;
        Show();
    }

    private void BtnDialogCancel_Click(object sender, RoutedEventArgs e)
    {
        // entferne Steuerungsknoten und deaktiviere Ereignishandler für Canvas
        StartFenster.tragwerksModell.VisualTragwerkModel.Children.Remove(StartFenster.tragwerksModell.Knoten);
        StartFenster.tragwerksModell.VisualTragwerkModel.Background = null;
        StartFenster.tragwerksModell.isKnoten = false;
        Close();
    }

    private void BtnDialogOk_Click(object sender, RoutedEventArgs e)
    {
        
        var knotenId = KnotenId.Text;

        if (knotenId == "")
        {
            _ = MessageBox.Show("Knoten Id muss definiert sein", "neuer Knoten");
            return;
        }

        if (modell.Knoten.ContainsKey(knotenId))
        {
            modell.Knoten.TryGetValue(knotenId, out var vorhandenerKnoten);
            Debug.Assert(vorhandenerKnoten != null, nameof(vorhandenerKnoten) + " != null");
            if (AnzahlDof.Text.Length > 0) vorhandenerKnoten.AnzahlKnotenfreiheitsgrade = int.Parse(AnzahlDof.Text);
            if (X.Text.Length > 0) vorhandenerKnoten.Koordinaten[0] = double.Parse(X.Text);
            if (Y.Text.Length > 0) vorhandenerKnoten.Koordinaten[1] = double.Parse(Y.Text);
        }
        else
        {
            var dimension = modell.Raumdimension;
            var koordinaten = new double[dimension];
            int anzahlKnotenDof = 3;
            if (AnzahlDof.Text.Length > 0) anzahlKnotenDof = int.Parse(AnzahlDof.Text);
            if (X.Text.Length > 0) koordinaten[0] = double.Parse(X.Text);
            if (Y.Text.Length > 0) koordinaten[1] = double.Parse(Y.Text);
            var neuerKnoten = new Knoten(KnotenId.Text, koordinaten, anzahlKnotenDof, dimension);
            modell.Knoten.Add(knotenId, neuerKnoten);
        }

        // entferne Steuerungsknoten und deaktiviere Ereignishandler für Canvas
        StartFenster.tragwerksModell.VisualTragwerkModel.Children.Remove(StartFenster.tragwerksModell.Knoten);
        StartFenster.tragwerksModell.VisualTragwerkModel.Background = null;
        StartFenster.tragwerksModell.isKnoten = false;
        StartFenster.tragwerksModell.Close();
        Close();
    }

    private void KnotenIdLostFocus(object sender, RoutedEventArgs e)
    {
        if (!modell.Knoten.ContainsKey(KnotenId.Text)) return;
        modell.Knoten.TryGetValue(KnotenId.Text, out var vorhandenerKnoten);
        Debug.Assert(vorhandenerKnoten != null, nameof(vorhandenerKnoten) + " != null");
        AnzahlDof.Text = vorhandenerKnoten.AnzahlKnotenfreiheitsgrade.ToString();
        X.Text = vorhandenerKnoten.Koordinaten[0].ToString("N2", CultureInfo.CurrentCulture);
        Y.Text = vorhandenerKnoten.Koordinaten[1].ToString("N2", CultureInfo.CurrentCulture);
    }
}