using FE_Berechnungen.Tragwerksberechnung.Modelldaten;
using FE_Berechnungen.Tragwerksberechnung.ModelldatenAnzeigen;

namespace FE_Berechnungen.Tragwerksberechnung.ModelldatenLesen;

public partial class ZeitKnotenanfangswerteNeu
{
    private readonly FeModell _modell;
    private int _aktuell;

    public ZeitKnotenanfangswerteNeu(FeModell modell)
    {
        InitializeComponent();
        _modell = modell;
        _modell.Zeitintegration ??= new Zeitintegration(0, 0, 0);
        StartFenster.TragwerkVisual.ZeitintegrationNeu ??= new ZeitintegrationNeu(_modell);
        if (modell.Zeitintegration.Anfangsbedingungen.Count != 0)
        {
            var anfang = modell.Zeitintegration.Anfangsbedingungen[0];
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
        }
        Show();
    }
    public ZeitKnotenanfangswerteNeu(FeModell modell, int aktuell)
    {
        InitializeComponent();
        _modell = modell;
        _aktuell = aktuell;
        modell.Zeitintegration ??= new Zeitintegration(0, 0, 0);
        if (_aktuell > 0 && modell.Zeitintegration.Anfangsbedingungen.Count >= _aktuell)
        {
            var anfang = modell.Zeitintegration.Anfangsbedingungen[_aktuell - 1];
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
        }
        ShowDialog();
    }

    private void BtnDialogOk_Click(object sender, RoutedEventArgs e)
    {
        // neue Anfangsbedingung hinzufügen
        if (_aktuell > _modell.Zeitintegration.Anfangsbedingungen.Count)
        {
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
            else
            {
                _ = MessageBox.Show("Knotennummer muss definiert sein", "neue ZeitKnotenanfangswerte");
                return;
            }
        }
        // vorhandene Anfangsbedingung ändern
        else
        {
            var anfang = _modell.Zeitintegration.Anfangsbedingungen[_aktuell - 1];
            anfang.KnotenId = KnotenId.Text;
            try
            {
                if (Dof1D0.Text != string.Empty) anfang.Werte[0] = double.Parse(Dof1D0.Text);
                if (Dof1V0.Text != string.Empty) anfang.Werte[1] = double.Parse(Dof1V0.Text);
                if (Dof2D0.Text != string.Empty) anfang.Werte[2] = double.Parse(Dof2D0.Text);
                if (Dof2V0.Text != string.Empty) anfang.Werte[3] = double.Parse(Dof2V0.Text);
                if (Dof3D0.Text != string.Empty) anfang.Werte[4] = double.Parse(Dof3D0.Text);
                if (Dof3V0.Text != string.Empty) anfang.Werte[5] = double.Parse(Dof3V0.Text);
            }
            catch (FormatException)
            {
                _ = MessageBox.Show("ungültiges  Eingabeformat", "neue ZeitKnotenanfangswerte");
            }
        }
        Close();
        StartFenster.TragwerkVisual.Close();
        StartFenster.TragwerkVisual = new TragwerkmodellVisualisieren(_modell);
        StartFenster.TragwerkVisual.Show();
    }

    private void BtnDialogCancel_Click(object sender, RoutedEventArgs e)
    {
        Close();
        StartFenster.TragwerkVisual.ZeitintegrationNeu?.Close();
    }

    private void BtnLöschen_Click(object sender, RoutedEventArgs e)
    {
        _modell.Zeitintegration.Anfangsbedingungen.RemoveAt(_aktuell - 1);
        _aktuell = 0;
        if (_modell.Zeitintegration.Anfangsbedingungen.Count <= 0)
        {
            Close();
            StartFenster.TragwerkVisual.ZeitintegrationNeu?.Close();
            return;
        }

        var anfangsWerte = _modell.Zeitintegration.Anfangsbedingungen[_aktuell];
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
        StartFenster.TragwerkVisual.ZeitintegrationNeu?.Close();
    }

    private void KnotenIdLostFocus(object sender, RoutedEventArgs e)
    {
        var knotenId = KnotenId.Text;
        for (var i = 0; i < _modell.Zeitintegration.Anfangsbedingungen.Count; i++)
        {
            if (_modell.Zeitintegration.Anfangsbedingungen[i].KnotenId != knotenId) continue;
            var anfangsWerte = _modell.Zeitintegration.Anfangsbedingungen[i];
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
            _aktuell = i + 1;
            return;
        }

        _aktuell = _modell.Zeitintegration.Anfangsbedingungen.Count + 1;
        Dof1D0.Text = ""; Dof1V0.Text = ""; Dof2D0.Text = ""; Dof2V0.Text = "";
    }

    private void KnotenPositionNeu(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        _modell.Knoten.TryGetValue(KnotenId.Text, out var knoten);
        if (knoten == null) { _ = MessageBox.Show("Knoten nicht im Modell gefunden", "neue zeitabhängige Knotenlast"); return; }
        StartFenster.WärmeVisual.KnotenEdit(knoten);
        Close();
        _modell.Berechnet = false;
    }
}