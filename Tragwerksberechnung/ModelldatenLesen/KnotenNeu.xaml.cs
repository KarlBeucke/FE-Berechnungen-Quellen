using FE_Berechnungen.Tragwerksberechnung.ModelldatenAnzeigen;
using FEBibliothek.Modell;
using System.Globalization;
using System.Windows;

namespace FE_Berechnungen.Tragwerksberechnung.ModelldatenLesen;

public partial class KnotenNeu
{
    private readonly FeModell _modell;
    private KnotenKeys knotenKeys;

    public KnotenNeu()
    {
        InitializeComponent();
    }
    public KnotenNeu(FeModell feModell)
    {
        InitializeComponent();
        _modell = feModell;
        // aktiviere Ereignishandler für Canvas
        StartFenster.TragwerkVisual.VisualTragwerkModel.Background = System.Windows.Media.Brushes.Transparent;
        Show();
        knotenKeys = new KnotenKeys(_modell) { Owner = this };
        knotenKeys.Show();
    }

    private void BtnDialogCancel_Click(object sender, RoutedEventArgs e)
    {
        // entferne Steuerungsknoten und deaktiviere Ereignishandler für Canvas
        StartFenster.TragwerkVisual.VisualTragwerkModel.Children.Remove(StartFenster.TragwerkVisual.Knoten);
        StartFenster.TragwerkVisual.VisualTragwerkModel.Background = null;
        Close();
        knotenKeys.Close();
    }

    private void BtnDialogOk_Click(object sender, RoutedEventArgs e)
    {

        var knotenId = KnotenId.Text;

        if (knotenId == "")
        {
            _ = MessageBox.Show("Knoten Id muss definiert sein", "neuer Knoten");
            return;
        }

        if (_modell.Knoten.ContainsKey(knotenId))
        {
            _modell.Knoten.TryGetValue(knotenId, out var vorhandenerKnoten);
            if (vorhandenerKnoten != null)
            {
                if (AnzahlDof.Text.Length > 0) vorhandenerKnoten.AnzahlKnotenfreiheitsgrade = int.Parse(AnzahlDof.Text);
                if (X.Text.Length > 0) vorhandenerKnoten.Koordinaten[0] = double.Parse(X.Text);
                if (Y.Text.Length > 0) vorhandenerKnoten.Koordinaten[1] = double.Parse(Y.Text);
            }
        }
        else
        {
            var dimension = _modell.Raumdimension;
            var koordinaten = new double[dimension];
            var anzahlKnotenDof = 3;
            if (AnzahlDof.Text.Length > 0) anzahlKnotenDof = int.Parse(AnzahlDof.Text);
            if (X.Text.Length > 0) koordinaten[0] = double.Parse(X.Text);
            if (Y.Text.Length > 0) koordinaten[1] = double.Parse(Y.Text);
            var neuerKnoten = new Knoten(KnotenId.Text, koordinaten, anzahlKnotenDof, dimension);
            _modell.Knoten.Add(knotenId, neuerKnoten);
        }

        // entferne Steuerungsknoten und deaktiviere Ereignishandler für Canvas
        StartFenster.TragwerkVisual.VisualTragwerkModel.Children.Remove(StartFenster.TragwerkVisual.Knoten);
        StartFenster.TragwerkVisual.VisualTragwerkModel.Background = null;
        StartFenster.TragwerkVisual.Close();
        Close();
        knotenKeys.Close();

        StartFenster.TragwerkVisual = new TragwerkmodellVisualisieren(StartFenster.TragwerksModell);
        StartFenster.TragwerkVisual.Show();
    }

    private void KnotenIdLostFocus(object sender, RoutedEventArgs e)
    {
        if (!_modell.Knoten.ContainsKey(KnotenId.Text)) return;
        _modell.Knoten.TryGetValue(KnotenId.Text, out var vorhandenerKnoten);
        if (vorhandenerKnoten == null) return;
        AnzahlDof.Text = vorhandenerKnoten.AnzahlKnotenfreiheitsgrade.ToString();
        X.Text = vorhandenerKnoten.Koordinaten[0].ToString("N2", CultureInfo.CurrentCulture);
        Y.Text = vorhandenerKnoten.Koordinaten[1].ToString("N2", CultureInfo.CurrentCulture);
    }
}