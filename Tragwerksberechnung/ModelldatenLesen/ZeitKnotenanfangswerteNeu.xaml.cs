using FEBibliothek.Modell;
using System.Windows;

namespace FE_Berechnungen.Tragwerksberechnung.ModelldatenLesen;

public partial class ZeitKnotenanfangswerteNeu
{
    private readonly FeModell modell;
    private int aktuell;
    public ZeitKnotenanfangswerteNeu(FeModell modell)
    {
        InitializeComponent();
        this.modell = modell;
        aktuell = StartFenster.tragwerkVisual.zeitintegrationNeu.aktuell;
        var anfang = (Knotenwerte)modell.Zeitintegration.Anfangsbedingungen[aktuell];
        KnotenId.Text = anfang.KnotenId;
        Dof1D0.Text = anfang.Werte[0].ToString("G2");
        Dof1V0.Text = anfang.Werte[1].ToString("G2");
        if (anfang.Werte.Length > 2)
        {
            Dof2D0.Text = anfang.Werte[2].ToString("G2");
            Dof2V0.Text = anfang.Werte[3].ToString("G2");
        }
        if (anfang.Werte.Length > 4)
        {
            Dof3D0.Text = anfang.Werte[4].ToString("G2");
            Dof3V0.Text = anfang.Werte[5].ToString("G2");
        }
        Show();
    }

    private void BtnDialogOk_Click(object sender, RoutedEventArgs e)
    {
        if (KnotenId.Text.Length == 0) Close();

        // neue Anfangsbedingung hinzufügen
        if (StartFenster.tragwerkVisual.zeitintegrationNeu.aktuell > modell.Zeitintegration.Anfangsbedingungen.Count)
        {
            if (KnotenId.Text == "") return;
            var knotenId = KnotenId.Text;
            if (modell.Knoten.TryGetValue(knotenId, out var knoten))
            {
                var nodalDof = knoten.AnzahlKnotenfreiheitsgrade;
                var anfangsWerte = new double[2 * nodalDof];
                if (Dof1D0.Text != string.Empty) { anfangsWerte[0] = double.Parse(Dof1D0.Text); }
                if (Dof1V0.Text != string.Empty) { anfangsWerte[1] = double.Parse(Dof1V0.Text); }

                switch (nodalDof)
                {
                    case 2:
                        {
                            if (Dof2D0.Text != string.Empty) { anfangsWerte[2] = double.Parse(Dof2D0.Text); }
                            if (Dof2V0.Text != string.Empty) { anfangsWerte[3] = double.Parse(Dof2V0.Text); }
                            break;
                        }
                    case 3:
                        {
                            if (Dof3D0.Text != string.Empty) { anfangsWerte[4] = double.Parse(Dof3D0.Text); }
                            if (Dof3V0.Text != string.Empty) { anfangsWerte[5] = double.Parse(Dof3V0.Text); }
                            break;
                        }
                }
                modell.Zeitintegration.Anfangsbedingungen.Add(new Knotenwerte(KnotenId.Text, anfangsWerte));
            }
        }
        // vorhandene Anfangsbedingung ändern
        else
        {
            var anfang = (Knotenwerte)modell.Zeitintegration.Anfangsbedingungen[aktuell];
            anfang.KnotenId = KnotenId.Text;
            anfang.Werte[0] = double.Parse(Dof1D0.Text); anfang.Werte[1] = double.Parse(Dof1V0.Text);
            anfang.Werte[2] = double.Parse(Dof2D0.Text); anfang.Werte[3] = double.Parse(Dof2V0.Text);
            anfang.Werte[4] = double.Parse(Dof3D0.Text); anfang.Werte[5] = double.Parse(Dof3V0.Text);
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
        modell.Zeitintegration.Anfangsbedingungen.RemoveAt(aktuell + 1);
        aktuell = 0;
        if (modell.Zeitintegration.Anfangsbedingungen.Count <= 0)
        {
            Close();
            StartFenster.tragwerkVisual.zeitintegrationNeu.Close();
            return;
        }
        var anfangsWerte = (Knotenwerte)modell.Zeitintegration.Anfangsbedingungen[aktuell];
        KnotenId.Text = anfangsWerte.KnotenId;
        Dof1D0.Text = anfangsWerte.Werte[0].ToString("G2");
        Dof1V0.Text = anfangsWerte.Werte[1].ToString("G2");

        if (anfangsWerte.Werte.Length > 2)
        {
            Dof2D0.Text = anfangsWerte.Werte[2].ToString("G2");
            Dof2V0.Text = anfangsWerte.Werte[3].ToString("G2");
        }

        if (anfangsWerte.Werte.Length > 4)
        {
            Dof3D0.Text = anfangsWerte.Werte[4].ToString("G2");
            Dof3V0.Text = anfangsWerte.Werte[5].ToString("G2");
        }
        Close();
        StartFenster.tragwerkVisual.zeitintegrationNeu.Close();
    }
}