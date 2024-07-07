using FE_Berechnungen.Tragwerksberechnung.Modelldaten;
using FEBibliothek.Modell;
using FEBibliothek.Modell.abstrakte_Klassen;

namespace FE_Berechnungen.Tragwerksberechnung.ModelldatenLesen;

internal class TransientParser
{
    private string[] substrings;
    private readonly char[] delimiters = { '\t', ';' };
    public bool ZeitintegrationDaten;

    public void ParseZeitintegration(string[] lines, FeModell feModell)
    {
        var modell = feModell;

        // suche "Eigenlösungen"
        for (var i = 0; i < lines.Length; i++)
        {
            if (lines[i] != "Eigenlösungen") continue;
            FeParser.EingabeGefunden += "\nEigenlösungen";

            substrings = lines[i + 1].Split(delimiters);
            if (substrings.Length == 2)
            {
                var id = substrings[0];
                int numberOfStates = short.Parse(substrings[1]);
                modell.Eigenzustand = new Eigenzustände(id, numberOfStates);
                break;
            }
            else
            {
                throw new ParseAusnahme((i + 2) + ":\nEigenlösungen, falsche Anzahl Parameter");
            }
        }

        // suche "Zeitintegration"
        for (var i = 0; i < lines.Length; i++)
        {
            if (lines[i] != "Zeitintegration") continue;
            FeParser.EingabeGefunden += "\nZeitintegration";
            //id, tmax, dt, method, parameter1, parameter2
            //method=1:beta,gamma  method=2:theta  method=3: alfa
            substrings = lines[i + 1].Split(delimiters);
            switch (substrings.Length)
            {
                case 5:
                    var tmax = double.Parse(substrings[1]);
                    var dt = double.Parse(substrings[2]);
                    var method = short.Parse(substrings[3]);
                    var parameter1 = double.Parse(substrings[4]);
                    modell.Zeitintegration =
                        new Zeitintegration(tmax, dt, method, parameter1);
                    break;
                case 6:
                    tmax = double.Parse(substrings[1]);
                    dt = double.Parse(substrings[2]);
                    method = short.Parse(substrings[3]);
                    parameter1 = double.Parse(substrings[4]);
                    var parameter2 = double.Parse(substrings[5]);
                    modell.Zeitintegration =
                        new Zeitintegration(tmax, dt, method, parameter1, parameter2);
                    break;
                default:
                    throw new ParseAusnahme((i + 2) + ":\nZeitintegration, falsche Anzahl Parameter");
            }
            ZeitintegrationDaten = true;
        }

        // suche "Dämpfung"
        for (var i = 0; i < lines.Length; i++)
        {
            if (lines[i] != "Dämpfung") continue;
            FeParser.EingabeGefunden += "\nDämpfung";
            do
            {
                substrings = lines[i + 1].Split(delimiters);
                foreach (var rate in substrings)
                {
                    modell.Eigenzustand.DämpfungsRaten.
                        Add(new ModaleWerte(double.Parse(rate)));
                }
                i++;
            } while (lines[i + 1].Length != 0);
            break;
        }

        // suche "Anfangsbedingungen"
        for (var i = 0; i < lines.Length; i++)
        {
            if (lines[i] != "Anfangsbedingungen") continue;
            FeParser.EingabeGefunden += "\nAnfangsbedingungen";
            do
            {
                substrings = lines[i + 1].Split(delimiters);
                var anfangsKnotenId = substrings[0];
                // Anfangsverformungen und Geschwindigkeiten
                int nodalDof;
                switch (substrings.Length)
                {
                    case 3:
                        nodalDof = 1;
                        break;
                    case 5:
                        nodalDof = 2;
                        break;
                    case 7:
                        nodalDof = 3;
                        break;
                    default:
                        throw new ParseAusnahme((i + 2) + ":\nAnfangsbedingungen, falsche Anzahl Parameter");
                }
                var anfangsWerte = new double[2 * nodalDof];
                for (var k = 0; k < 2 * nodalDof; k++)
                {
                    anfangsWerte[k] = double.Parse(substrings[k + 1]);
                }
                modell.Zeitintegration.Anfangsbedingungen.Add(new Knotenwerte(anfangsKnotenId, anfangsWerte));
                i++;
            } while (lines[i + 1].Length != 0);
            break;
        }

        // suche zeitabhängige Knotenlasten
        for (var i = 0; i < lines.Length; i++)
        {
            if (lines[i] != "Zeitabhängige Knotenlast") continue;
            FeParser.EingabeGefunden += "\nZeitabhängige Knotenlast";
            var boden = false;
            i++;

            do
            {
                substrings = lines[i].Split(delimiters);
                if (substrings.Length != 3)
                    throw new ParseAusnahme((i + 2) + ":\nZeitabhängige Knotenlast, falsche Anzahl Parameter");

                var knotenLastId = substrings[0];
                var knotenId = substrings[1];
                if (knotenId == "boden") boden = true;
                var knotenFreiheitsgrad = short.Parse(substrings[2]);

                substrings = lines[i + 1].Split(delimiters);
                ZeitabhängigeKnotenLast zeitabhängigeKnotenLast;
                switch (substrings.Length)
                {
                    // 1 Wert: lies Anregung (Lastvektor) aus Datei, Variationstyp = 0
                    case 1:
                        {
                            zeitabhängigeKnotenLast = new ZeitabhängigeKnotenLast(knotenLastId, knotenId, knotenFreiheitsgrad, true, boden)
                            { VariationsTyp = 0 };
                            var last = (AbstraktZeitabhängigeKnotenlast)zeitabhängigeKnotenLast;
                            modell.ZeitabhängigeKnotenLasten.Add(knotenLastId, last);
                            break;
                        }
                    // 3 Werte: harmonische Anregung, Variationstyp = 2
                    case 3:
                        {
                            var amplitude = double.Parse(substrings[0]);
                            var circularFrequency = double.Parse(substrings[1]);
                            var phaseAngle = double.Parse(substrings[2]);
                            zeitabhängigeKnotenLast = new ZeitabhängigeKnotenLast(knotenLastId, knotenId, knotenFreiheitsgrad, false, boden)
                            { Amplitude = amplitude, Frequenz = circularFrequency, PhasenWinkel = phaseAngle, VariationsTyp = 2 };
                            modell.ZeitabhängigeKnotenLasten.Add(knotenLastId, zeitabhängigeKnotenLast);
                            break;
                        }
                    // mehr als 3 Werte: lies Zeit-/Wert-Intervalle der Anregung mit linearer Interpolation, Variationstyp = 1
                    default:
                        {
                            var interval = new double[substrings.Length];
                            for (var j = 0; j < substrings.Length; j += 2)
                            {
                                interval[j] = double.Parse(substrings[j]);
                                interval[j + 1] = double.Parse(substrings[j + 1]);
                            }
                            zeitabhängigeKnotenLast = new ZeitabhängigeKnotenLast(knotenLastId, knotenId, knotenFreiheitsgrad, false, boden)
                            { Intervall = interval, VariationsTyp = 1 };
                            modell.ZeitabhängigeKnotenLasten.Add(knotenLastId, zeitabhängigeKnotenLast);
                            break;
                        }
                }
                i += 2;
            } while (lines[i].Length != 0);
        }
    }
}