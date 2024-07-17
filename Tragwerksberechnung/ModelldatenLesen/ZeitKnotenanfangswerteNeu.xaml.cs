using System;
using System.Windows;
using FEBibliothek.Modell;

namespace FE_Berechnungen.Tragwerksberechnung.ModelldatenLesen;

public partial class ZeitKnotenanfangswerteNeu
{
    private readonly FeModell _modell;
    private int _aktuell;

    public ZeitKnotenanfangswerteNeu(FeModell modell)
    {
        InitializeComponent();
        _modell = modell;
        _aktuell = StartFenster.TragwerkVisual.ZeitintegrationNeu.Aktuell;
        var anfang = (Knotenwerte)modell.Zeitintegration.Anfangsbedingungen[_aktuell];
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
        if (StartFenster.TragwerkVisual.ZeitintegrationNeu.Aktuell > _modell.Zeitintegration.Anfangsbedingungen.Count)
        {
            if (KnotenId.Text == "") return;
            var knotenId = KnotenId.Text;
            if (_modell.Knoten.TryGetValue(knotenId, out var knoten))
            {
                var nodalDof = knoten.AnzahlKnotenfreiheitsgrade;
                var anfangsWerte = new double[2 * nodalDof];
                try
                {
                    if (Dof1D0.Text != string.Empty) anfangsWerte[0] = double.Parse(Dof1D0.Text);
                    if (Dof1V0.Text != string.Empty) anfangsWerte[1] = double.Parse(Dof1V0.Text);

                    switch (nodalDof)
                    {
                        case 2:
                        {
                            if (Dof2D0.Text != string.Empty) anfangsWerte[2] = double.Parse(Dof2D0.Text);
                            if (Dof2V0.Text != string.Empty) anfangsWerte[3] = double.Parse(Dof2V0.Text);
                            break;
                        }
                        case 3:
                        {
                            if (Dof3D0.Text != string.Empty) anfangsWerte[4] = double.Parse(Dof3D0.Text);
                            if (Dof3V0.Text != string.Empty) anfangsWerte[5] = double.Parse(Dof3V0.Text);
                            break;
                        }
                    }
                }
                catch (FormatException)
                {
                    _ = MessageBox.Show("ungültiges  Eingabeformat", "neue ZeitKnotenanfangswerte");
                }
                _modell.Zeitintegration.Anfangsbedingungen.Add(new Knotenwerte(KnotenId.Text, anfangsWerte));
            }
        }
        // vorhandene Anfangsbedingung ändern
        else
        {
            var anfang = (Knotenwerte)_modell.Zeitintegration.Anfangsbedingungen[_aktuell];
            anfang.KnotenId = KnotenId.Text;
            try
            {
                anfang.Werte[0] = double.Parse(Dof1D0.Text);
                anfang.Werte[1] = double.Parse(Dof1V0.Text);
                anfang.Werte[2] = double.Parse(Dof2D0.Text);
                anfang.Werte[3] = double.Parse(Dof2V0.Text);
                anfang.Werte[4] = double.Parse(Dof3D0.Text);
                anfang.Werte[5] = double.Parse(Dof3V0.Text);
            }
            catch (FormatException)
            {
                _ = MessageBox.Show("ungültiges  Eingabeformat", "neue ZeitKnotenanfangswerte");
            }
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
        _modell.Zeitintegration.Anfangsbedingungen.RemoveAt(_aktuell + 1);
        _aktuell = 0;
        if (_modell.Zeitintegration.Anfangsbedingungen.Count <= 0)
        {
            Close();
            StartFenster.TragwerkVisual.ZeitintegrationNeu.Close();
            return;
        }

        var anfangsWerte = (Knotenwerte)_modell.Zeitintegration.Anfangsbedingungen[_aktuell];
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
        StartFenster.TragwerkVisual.ZeitintegrationNeu.Close();
    }
}