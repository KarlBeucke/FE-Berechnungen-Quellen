using FEBibliothek.Modell;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Windows;

namespace FE_Berechnungen.Wärmeberechnung.ModelldatenLesen;

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
        StartFenster.wärmeVisual.VisualWärmeModell.Background = System.Windows.Media.Brushes.Transparent;
        Show();
    }
    private void BtnDialogCancel_Click(object sender, RoutedEventArgs e)
    {
        // entferne Steuerungsknoten und deaktiviere Ereignishandler für Canvas
        StartFenster.wärmeVisual.VisualWärmeModell.Children.Remove(StartFenster.wärmeVisual.Knoten);
        StartFenster.wärmeVisual.VisualWärmeModell.Background = null;
        StartFenster.wärmeVisual.isKnoten = false;
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
            if (X.Text.Length > 0) vorhandenerKnoten.Koordinaten[0] = double.Parse(X.Text);
            if (Y.Text.Length > 0) vorhandenerKnoten.Koordinaten[1] = double.Parse(Y.Text);
        }
        else
        {
            var dimension = modell.Raumdimension;
            var koordinaten = new double[dimension];
            int anzahlKnotenDof = 1;
            if (X.Text.Length > 0) koordinaten[0] = double.Parse(X.Text);
            if (Y.Text.Length > 0) koordinaten[1] = double.Parse(Y.Text);
            var neuerKnoten = new Knoten(KnotenId.Text, koordinaten, anzahlKnotenDof, dimension);
            modell.Knoten.Add(knotenId, neuerKnoten);
        }

        // entferne Steuerungsknoten und deaktiviere Ereignishandler für Canvas
        StartFenster.wärmeVisual.VisualWärmeModell.Children.Remove(StartFenster.wärmeVisual.Knoten);
        StartFenster.wärmeVisual.VisualWärmeModell.Background = null;
        StartFenster.wärmeVisual.isKnoten = false;
        StartFenster.wärmeVisual.Close();
        Close();
    }

    private void KnotenIdLostFocus(object sender, RoutedEventArgs e)
    {
        // entferne Pilotknoten und deaktiviere Ereignishandler für Canvas
        if (!modell.Knoten.ContainsKey(KnotenId.Text)) return;
        modell.Knoten.TryGetValue(KnotenId.Text, out var vorhandenerKnoten);
        Debug.Assert(vorhandenerKnoten != null, nameof(vorhandenerKnoten) + " != null");
        X.Text = vorhandenerKnoten.Koordinaten[0].ToString("N2", CultureInfo.CurrentCulture);
        Y.Text = vorhandenerKnoten.Koordinaten[1].ToString("N2", CultureInfo.CurrentCulture);
    }

    private void BtnLöschen_Click(object sender, RoutedEventArgs e)
    {
        if (!modell.Knoten.Keys.Contains(KnotenId.Text)) return;
        modell.Knoten.Remove(KnotenId.Text);
        Close();
        StartFenster.wärmeVisual.Close();
    }
}