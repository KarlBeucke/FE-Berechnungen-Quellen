using FEBibliothek.Modell;
using FEBibliothek.Modell.abstrakte_Klassen;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Windows;
using FE_Berechnungen.Wärmeberechnung.Modelldaten;

namespace FE_Berechnungen.Wärmeberechnung.ModelldatenLesen;

public partial class LinienlastNeu
{
    private readonly FeModell modell;
    private AbstraktLinienlast vorhandeneLast;
    private readonly WärmelastenKeys lastenKeys;

    public LinienlastNeu(FeModell modell)
    {
        this.modell = modell;
        InitializeComponent();
        lastenKeys = new WärmelastenKeys(modell);
        lastenKeys.Show();
        Show();
    }

    private void BtnDialogOk_Click(object sender, RoutedEventArgs e)
    {
        var linienlastId = LinienlastId.Text;
        if (linienlastId == "")
        {
            _ = MessageBox.Show("Linienlast Id muss definiert sein", "neue Linienlast");
            return;
        }

        // vorhandene Linienlast
        if (modell.LinienLasten.Keys.Contains(linienlastId))
        {
            modell.LinienLasten.TryGetValue(linienlastId, out vorhandeneLast);
            Debug.Assert(vorhandeneLast != null, nameof(vorhandeneLast) + " != null");

            if (StartknotenId.Text.Length > 0) vorhandeneLast.StartKnotenId = StartknotenId.Text.ToString(CultureInfo.CurrentCulture);
            if (Start.Text.Length > 0) vorhandeneLast.Lastwerte[0] = double.Parse(Start.Text);
            if (EndknotenId.Text.Length > 0) vorhandeneLast.EndKnotenId = EndknotenId.Text.ToString(CultureInfo.CurrentCulture);
            if (End.Text.Length > 0) vorhandeneLast.Lastwerte[1] = double.Parse(End.Text);
        }
        // neue Linienlast
        else
        {
            var startknotenId = "";
            var endknotenId = "";
            var t = new double[2];
            if (StartknotenId.Text.Length > 0) startknotenId = StartknotenId.Text.ToString(CultureInfo.CurrentCulture);
            if (Start.Text.Length > 0) t[0] = double.Parse(Start.Text);
            if (EndknotenId.Text.Length > 0) endknotenId = EndknotenId.Text.ToString(CultureInfo.CurrentCulture);
            if (End.Text.Length > 0) t[1] = double.Parse(End.Text);
            var linienlast = new LinienLast(startknotenId, endknotenId, t)
            {
                LastId = linienlastId
            };
            modell.LinienLasten.Add(linienlastId, linienlast);
        }
        lastenKeys?.Close();
        Close();
        StartFenster.wärmeModell.Close();
    }

    private void BtnDialogCancel_Click(object sender, RoutedEventArgs e)
    {
        lastenKeys?.Close();
        Close();
    }

    private void BtnLöschen_Click(object sender, RoutedEventArgs e)
    {
        if (!modell.LinienLasten.Keys.Contains(LinienlastId.Text)) return;
        modell.LinienLasten.Remove(LinienlastId.Text);
        lastenKeys?.Close();
        Close();
        StartFenster.wärmeModell.Close();
    }

    private void LinienlastIdLostFocus(object sender, RoutedEventArgs e)
    {
        if (!modell.LinienLasten.ContainsKey(LinienlastId.Text))
        {
            StartknotenId.Text = "";
            Start.Text = "";
            EndknotenId.Text = "";
            End.Text = "";
            return;
        }

        // vorhandene Linienlastdefinition
        modell.LinienLasten.TryGetValue(LinienlastId.Text, out vorhandeneLast);
        Debug.Assert(vorhandeneLast != null, nameof(vorhandeneLast) + " != null"); 
        
        LinienlastId.Text = vorhandeneLast.LastId;
        StartknotenId.Text = vorhandeneLast.StartKnotenId;
        Start.Text = vorhandeneLast.Lastwerte[0].ToString("G3", CultureInfo.CurrentCulture);
        EndknotenId.Text = vorhandeneLast.EndKnotenId;
        End.Text = vorhandeneLast.Lastwerte[1].ToString("G3", CultureInfo.CurrentCulture);
    }
}