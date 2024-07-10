using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Markup;
using FEBibliothek.Modell;

namespace FE_Berechnungen.Tragwerksberechnung.Ergebnisse;

public partial class DynamischeErgebnisseAnzeigen
{
    private readonly FeModell modell;
    private Knoten knoten;

    public DynamischeErgebnisseAnzeigen(FeModell feModell)
    {
        Language = XmlLanguage.GetLanguage("de-DE");
        modell = feModell;
        InitializeComponent();
        Show();

        Knotenauswahl.ItemsSource = modell.Knoten.Keys;

        // Auswahl des Zeitschritts aus Zeitraster, z.B. jeder 10.
        Dt = modell.Zeitintegration.Dt;
        var tmax = modell.Zeitintegration.Tmax;
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
        if (modell.Knoten.TryGetValue(knotenId, out knoten))
        {
        }

        if (knoten != null)
        {
            var maxDeltaX = knoten.KnotenVariable[0].Max();
            var maxDeltaXZeit = Dt * Array.IndexOf(knoten.KnotenVariable[0], maxDeltaX);
            var maxDeltaY = knoten.KnotenVariable[1].Max();
            var maxDeltaYZeit = Dt * Array.IndexOf(knoten.KnotenVariable[1], maxDeltaY);
            var maxAccX = knoten.KnotenAbleitungen[0].Max();
            var maxAccXZeit = Dt * Array.IndexOf(knoten.KnotenAbleitungen[0], maxAccX);
            var maxAccY = knoten.KnotenAbleitungen[1].Max();
            var maxAccYZeit = Dt * Array.IndexOf(knoten.KnotenAbleitungen[1], maxAccY);

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
        if (knoten == null) return;

        var knotenverformungen = new List<Knotenverformungen>();
        var dt = modell.Zeitintegration.Dt;
        var nSteps = knoten.KnotenVariable[0].Length;
        var zeit = new double[nSteps + 1];
        zeit[0] = 0;

        Knotenverformungen knotenverformung = null;
        for (var i = 0; i < nSteps; i++)
        {
            switch (knoten.KnotenVariable.Length)
            {
                case 2:
                    knotenverformung = new Knotenverformungen(zeit[i], knoten.KnotenVariable[0][i],
                        knoten.KnotenVariable[1][i],
                        knoten.KnotenAbleitungen[0][i], knoten.KnotenAbleitungen[1][i]);
                    break;
                case 3:
                    knotenverformung = new Knotenverformungen(zeit[i], knoten.KnotenVariable[0][i],
                        knoten.KnotenVariable[1][i], knoten.KnotenVariable[2][i],
                        knoten.KnotenAbleitungen[0][i], knoten.KnotenAbleitungen[1][i], knoten.KnotenAbleitungen[2][i]);
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
        var dt = modell.Zeitintegration.Dt;
        var tmax = modell.Zeitintegration.Tmax;
        var nSteps = (int)(tmax / dt) + 1;
        var zeit = new double[nSteps + 1];
        zeit[0] = 0;

        Knotenverformungen knotenverformung = null;
        foreach (var item in modell.Knoten)
        {
            // eingabeEinheit z.B. in m, verformungsEinheit z.B. cm, beschleunigungsEinheit z.B. cm/s/s
            const int verformungsEinheit = 1;
            knoten = item.Value;
            switch (knoten.KnotenVariable.Length)
            {
                case 2:
                    knotenverformung = new Knotenverformungen(item.Value.Id,
                        knoten.KnotenVariable[0][Index] * verformungsEinheit,
                        knoten.KnotenVariable[1][Index] * verformungsEinheit,
                        knoten.KnotenAbleitungen[0][Index] * verformungsEinheit,
                        knoten.KnotenAbleitungen[1][Index] * verformungsEinheit);
                    break;
                case 3:
                    knotenverformung = new Knotenverformungen(item.Value.Id,
                        knoten.KnotenVariable[0][Index] * verformungsEinheit,
                        knoten.KnotenVariable[1][Index] * verformungsEinheit,
                        knoten.KnotenVariable[2][Index] * verformungsEinheit,
                        knoten.KnotenAbleitungen[0][Index] * verformungsEinheit,
                        knoten.KnotenAbleitungen[1][Index] * verformungsEinheit,
                        knoten.KnotenAbleitungen[2][Index] * verformungsEinheit);
                    break;
            }

            zeitschritt.Add(knotenverformung);
        }

        ZeitschrittGrid.ItemsSource = zeitschritt;
    }
}