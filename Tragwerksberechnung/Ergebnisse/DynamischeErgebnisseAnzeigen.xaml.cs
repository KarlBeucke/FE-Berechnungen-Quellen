namespace FE_Berechnungen.Tragwerksberechnung.Ergebnisse;

public partial class DynamischeErgebnisseAnzeigen
{
    private readonly FeModell _modell;
    private Knoten _knoten;

    public DynamischeErgebnisseAnzeigen(FeModell feModell)
    {
        Language = XmlLanguage.GetLanguage("de-DE");
        _modell = feModell;
        InitializeComponent();
        Show();

        Knotenauswahl.ItemsSource = _modell.Knoten.Keys;

        // Auswahl des Zeitschritts aus Zeitraster, z.B. jeder 10.
        Dt = _modell.Zeitintegration.Dt;
        var tmax = _modell.Zeitintegration.Tmax;
        NSteps = (int)(tmax / Dt);
        const int zeitraster = 1;
        //if (NSteps > 1000) zeitraster = 10;
        NSteps = NSteps / zeitraster + 1;
        var zeit = new double[NSteps];
        for (var i = 0; i < NSteps; i++) zeit[i] = i * Dt * zeitraster;

        Zeitschrittauswahl.ItemsSource = zeit;
    }

    private double Dt { get; }
    private int NSteps { get; }
    private int Index { get; set; }

    private void DropDownKnotenauswahlClosed(object sender, EventArgs e)
    {
        if (Knotenauswahl.SelectedIndex < 0)
        {
            _ = MessageBox.Show("kein gültiger Knoten Identifikator ausgewählt", "Zeitschrittauswahl");
            return;
        }

        var knotenId = (string)Knotenauswahl.SelectedItem;
        if (_modell.Knoten.TryGetValue(knotenId, out _knoten))
        {
        }

        if (_knoten != null)
        {
            var maxDeltaX = _knoten.KnotenVariable[0].Max();
            var maxDeltaXZeit = Dt * Array.IndexOf(_knoten.KnotenVariable[0], maxDeltaX);
            var maxDeltaY = _knoten.KnotenVariable[1].Max();
            var maxDeltaYZeit = Dt * Array.IndexOf(_knoten.KnotenVariable[1], maxDeltaY);
            var maxAccX = _knoten.KnotenAbleitungen[0].Max();
            var maxAccXZeit = Dt * Array.IndexOf(_knoten.KnotenAbleitungen[0], maxAccX);
            var maxAccY = _knoten.KnotenAbleitungen[1].Max();
            var maxAccYZeit = Dt * Array.IndexOf(_knoten.KnotenAbleitungen[1], maxAccY);

            var maxText = "max. DeltaX = " + maxDeltaX.ToString("G4") + ", t =" + maxDeltaXZeit.ToString("N2")
                          + ", max. DeltaY = " + maxDeltaY.ToString("G4") + ", t =" + maxDeltaYZeit.ToString("N2")
                          + "\nmax. AccX = " + maxAccX.ToString("G4") + ", t =" + maxAccXZeit.ToString("N2")
                          + ", max. AccY = " + maxAccY.ToString("G4") + ", t =" + maxAccYZeit.ToString("N2");
            MaxText.Text = maxText;
        }

        KnotenverformungenAnzeigen();
    }

    private void KnotenverformungenAnzeigen()
    {
        if (_knoten == null) return;

        var knotenverformungen = new List<Knotenverformungen>();
        var dt = _modell.Zeitintegration.Dt;
        var nSteps = _knoten.KnotenVariable[0].Length;
        var zeit = new double[nSteps + 1];
        zeit[0] = 0;

        Knotenverformungen knotenverformung = null;
        for (var i = 0; i < nSteps; i++)
        {
            switch (_knoten.KnotenVariable.Length)
            {
                case 2:
                    knotenverformung = new Knotenverformungen(zeit[i], _knoten.KnotenVariable[0][i],
                        _knoten.KnotenVariable[1][i],
                        _knoten.KnotenAbleitungen[0][i], _knoten.KnotenAbleitungen[1][i]);
                    break;
                case 3:
                    knotenverformung = new Knotenverformungen(zeit[i], _knoten.KnotenVariable[0][i],
                        _knoten.KnotenVariable[1][i], _knoten.KnotenVariable[2][i],
                        _knoten.KnotenAbleitungen[0][i], _knoten.KnotenAbleitungen[1][i], _knoten.KnotenAbleitungen[2][i]);
                    break;
            }

            knotenverformungen.Add(knotenverformung);
            zeit[i + 1] = zeit[i] + dt;
        }

        KnotenverformungenGrid.ItemsSource = knotenverformungen;
    }

    private void DropDownZeitschrittauswahlClosed(object sender, EventArgs e)
    {
        if (Zeitschrittauswahl.SelectedIndex < 0)
        {
            _ = MessageBox.Show("kein gültiger Zeitschritt ausgewählt", "Zeitschrittauswahl");
            return;
        }

        Index = Zeitschrittauswahl.SelectedIndex;
    }

    private void ZeitschrittGrid_Anzeigen(object sender, RoutedEventArgs e)
    {
        if (Index == 0) return;
        var zeitschritt = new List<Knotenverformungen>();
        //var dt = _modell.Zeitintegration.Dt;
        //var tmax = _modell.Zeitintegration.Tmax;
        //var nSteps = (int)(tmax / dt) + 1;
        //var zeit = new double[nSteps + 1];
        //zeit[0] = 0;

        Knotenverformungen knotenverformung = null;
        foreach (var item in _modell.Knoten)
        {
            // eingabeEinheit z.B. in m, verformungsEinheit z.B. cm, beschleunigungsEinheit z.B. cm/s/s
            const int verformungsEinheit = 1;
            _knoten = item.Value;
            switch (_knoten.KnotenVariable.Length)
            {
                case 2:
                    knotenverformung = new Knotenverformungen(item.Value.Id,
                        _knoten.KnotenVariable[0][Index] * verformungsEinheit,
                        _knoten.KnotenVariable[1][Index] * verformungsEinheit,
                        _knoten.KnotenAbleitungen[0][Index] * verformungsEinheit,
                        _knoten.KnotenAbleitungen[1][Index] * verformungsEinheit);
                    break;
                case 3:
                    knotenverformung = new Knotenverformungen(item.Value.Id,
                        _knoten.KnotenVariable[0][Index] * verformungsEinheit,
                        _knoten.KnotenVariable[1][Index] * verformungsEinheit,
                        _knoten.KnotenVariable[2][Index] * verformungsEinheit,
                        _knoten.KnotenAbleitungen[0][Index] * verformungsEinheit,
                        _knoten.KnotenAbleitungen[1][Index] * verformungsEinheit,
                        _knoten.KnotenAbleitungen[2][Index] * verformungsEinheit);
                    break;
            }

            zeitschritt.Add(knotenverformung);
        }

        ZeitschrittGrid.ItemsSource = zeitschritt;
    }
}