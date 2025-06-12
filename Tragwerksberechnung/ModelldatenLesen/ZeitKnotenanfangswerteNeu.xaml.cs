using FE_Berechnungen.Tragwerksberechnung.Modelldaten;
using FE_Berechnungen.Tragwerksberechnung.ModelldatenAnzeigen;

namespace FE_Berechnungen.Tragwerksberechnung.ModelldatenLesen;

public partial class ZeitKnotenanfangswerteNeu
{
    private readonly FeModell _modell;
    private int _aktuell;
    private readonly string _knotenIdSave;
    private readonly bool _knotenIdFixed;

    public ZeitKnotenanfangswerteNeu(FeModell modell)
    {
        InitializeComponent();
        _modell = modell;
        _aktuell = modell.Zeitintegration.Anfangsbedingungen.Count + 1;
        Show();
    }
    public ZeitKnotenanfangswerteNeu(FeModell modell, int aktuell, bool knotenIdFixed)
    {
        InitializeComponent();
        _modell = modell;
        _aktuell = aktuell;
        _knotenIdFixed = knotenIdFixed;
        modell.Zeitintegration ??= new Zeitintegration(0, 0, 0);

        var anfang = modell.Zeitintegration.Anfangsbedingungen[_aktuell];
        KnotenId.Text = anfang.KnotenId;
        _knotenIdSave = KnotenId.Text;
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
                StartFenster.TragwerkVisual.IsZeitAnfangsbedingung = true;
            }
            else
            {
                _ = MessageBox.Show("Knoten Id muss definiert sein", "neue ZeitKnotenanfangswerte");
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
        if (_knotenIdFixed) return;
        StartFenster.TragwerkVisual.Close();
        StartFenster.TragwerkVisual = new TragwerkmodellVisualisieren(_modell);
        StartFenster.TragwerkVisual.Show();
        _modell.Berechnet = false;
    }

    private void BtnDialogCancel_Click(object sender, RoutedEventArgs e)
    {
        StartFenster.TragwerkVisual.ZeitintegrationNeu?.Close();
        StartFenster.TragwerkVisual.IsZeitAnfangsbedingung = false;
        Close();
    }

    private void BtnLöschen_Click(object sender, RoutedEventArgs e)
    {
        _modell.Zeitintegration.Anfangsbedingungen.RemoveAt(_aktuell);
        Close();
        StartFenster.TragwerkVisual.Close();
        StartFenster.TragwerkVisual = new TragwerkmodellVisualisieren(_modell);
        StartFenster.TragwerkVisual.Show();
        _modell.Berechnet = false;
    }

    private void KnotenIdLostFocus(object sender, RoutedEventArgs e)
    {
        if (_knotenIdFixed)
        {
            _ = MessageBox.Show("KnotenId kann hier nicht geändert werden", "ZeitKnotenAnfangswerteNeu");
            KnotenId.Text = _knotenIdSave;
            return;
        }
        var knotenId = KnotenId.Text;
        if (!_modell.Knoten.TryGetValue(knotenId, out _))
        {
            _ = MessageBox.Show("Knoten nicht im Modell gefunden", "neue zeitabhängige Knotenlast");
            KnotenId.Text = "";
            return;
        }
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