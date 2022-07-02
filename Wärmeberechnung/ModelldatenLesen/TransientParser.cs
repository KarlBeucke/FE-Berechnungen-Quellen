using FE_Berechnungen.Wärmeberechnung.Modelldaten;
using FEBibliothek.Modell;
using System;
using System.Runtime.Serialization;

namespace FE_Berechnungen.Wärmeberechnung.ModelldatenLesen
{
    public class TransientParser
    {
        private string[] substrings;
        public bool zeitintegrationDaten;
        public void ParseZeitintegration(string[] lines, FEModell feModell)
        {
            var delimiters = new[] { '\t' };

            //suche "Eigenlösungen"
            for (var i = 0; i < lines.Length; i++)
            {
                if (lines[i] != "Eigenlösungen") continue;
                FeParser.EingabeGefunden += "\nEigenlösungen";
                do
                {
                    substrings = lines[i + 1].Split(delimiters);
                    switch (substrings.Length)
                    {
                        case 2:
                            {
                                var id = substrings[0];
                                int numberOfStates = short.Parse(substrings[1]);
                                feModell.Eigenzustand = new Eigenzustände(id, numberOfStates);
                                i++;
                                break;
                            }
                        default:
                            throw new ParseException((i + 2) + ": Eigenlösungen, falsche Anzahl Parameter");
                    }
                } while (lines[i + 1].Length != 0);
                break;
            }

            // suche "Zeitintegration"
            for (var i = 0; i < lines.Length; i++)
            {
                if (lines[i] != "Zeitintegration") continue;
                FeParser.EingabeGefunden += "\nZeitintegration";
                var teilStrings = lines[i + 1].Split(delimiters);
                var tmax = double.Parse(teilStrings[1]);
                var dt = double.Parse(teilStrings[2]);
                var alfa = double.Parse(teilStrings[3]);
                feModell.Zeitintegration = new Zeitintegration(tmax, dt, alfa) { Id = teilStrings[0], VonStationär = false };
                zeitintegrationDaten = true;
                break;
            }

            // suche Anfangstemperaturen
            for (var i = 0; i < lines.Length; i++)
            {
                // stationäre Lösung oder nodeId (incl. "alle")
                if (lines[i] != "Anfangstemperaturen") continue;
                FeParser.EingabeGefunden += "\nAnfangstemperaturen";

                var teilStrings = lines[i + 1].Split(delimiters);
                if (teilStrings[0] == "stationäre Lösung")
                {
                    feModell.Zeitintegration.VonStationär = true;
                }
                else if (teilStrings.Length == 2) do
                    {
                        // knotenId inkl. alle
                        var knotenId = teilStrings[0];
                        var t0 = double.Parse(teilStrings[1]);
                        var initial = new double[1];
                        initial[0] = t0;
                        feModell.Zeitintegration.Anfangsbedingungen.Add(new Knotenwerte(knotenId, initial));
                        i++;
                        teilStrings = lines[i + 1].Split(delimiters);
                    } while (lines[i + 1].Length != 0);
                else
                {
                    break;
                }
            }

            // suche zeitabhängige Randtemperaturen, eingeprägte Temperatur am Rand
            //  5:Name,NodeId,Amplitude,Frequenz,Phase 
            // >7:Name,NodeId,Wertepaare für stückweise linearen Verlauf
            for (var i = 0; i < lines.Length; i++)
            {
                if (lines[i] != "Zeitabhängige Randbedingungen") continue;
                FeParser.EingabeGefunden += "\nZeitabhängige Randbedingungen";
                do
                {
                    var teilStrings = lines[i + 1].Split(delimiters);
                    var supportId = teilStrings[0];
                    var nodeId = teilStrings[1];

                    ZeitabhängigeRandbedingung zeitabhängigeRandbedingung = null;
                    if (teilStrings.Length < 3)
                    {
                        throw new ParseAusnahme((i + 2) + ": Zeitabhängige Randbedingungen, falsche Anzahl Parameter");
                    }
                    switch (teilStrings[2])
                    {
                        case "datei":
                            {
                                const bool datei = true;
                                zeitabhängigeRandbedingung =
                                    new ZeitabhängigeRandbedingung(nodeId, datei)
                                    { RandbedingungId = supportId, VariationsTyp = 0, Vordefiniert = new double[1] };
                                break;
                            }
                        case "konstant":
                            {
                                if (teilStrings.Length != 4)
                                {
                                    throw new ParseAusnahme((i + 2) + ": Zeitabhängige Randbedingungen konstant, falsche Anzahl Parameter");
                                }
                                var konstanteTemperatur = double.Parse(teilStrings[3]);
                                zeitabhängigeRandbedingung =
                                    new ZeitabhängigeRandbedingung(nodeId, konstanteTemperatur)
                                    { RandbedingungId = supportId, VariationsTyp = 1, Vordefiniert = new double[1] };
                                break;
                            }
                        case "harmonisch":
                            {
                                if (teilStrings.Length != 6)
                                {
                                    throw new ParseAusnahme((i + 2) + ": Zeitabhängige Randbedingungen harmonisch, falsche Anzahl Parameter");
                                }
                                var amplitude = double.Parse(teilStrings[3]);
                                var frequenz = double.Parse(teilStrings[4]);
                                var phasenWinkel = double.Parse(teilStrings[5]);
                                zeitabhängigeRandbedingung =
                                    new ZeitabhängigeRandbedingung(nodeId, amplitude, frequenz, phasenWinkel)
                                    { RandbedingungId = supportId, VariationsTyp = 2, Vordefiniert = new double[1] };
                                break;
                            }
                        case "linear":
                            {
                                if (teilStrings.Length < 5)
                                {
                                    throw new ParseAusnahme((i + 2) + ": Zeitabhängige Randbedingungen linear, falsche Anzahl Parameter");
                                }
                                var k = 0;
                                char[] paarDelimiter = { ';' };
                                var interval = new double[2 * (teilStrings.Length - 3)];

                                for (var j = 3; j < teilStrings.Length; j++)
                                {
                                    var wertePaar = teilStrings[j].Split(paarDelimiter);
                                    interval[k] = double.Parse(wertePaar[0]);
                                    interval[k + 1] = double.Parse(wertePaar[1]);
                                    k += 2;
                                }

                                zeitabhängigeRandbedingung = new ZeitabhängigeRandbedingung(nodeId, interval)
                                { RandbedingungId = supportId, VariationsTyp = 3, Vordefiniert = new double[1] };
                                break;
                            }
                    }
                    feModell.ZeitabhängigeRandbedingung.Add(supportId, zeitabhängigeRandbedingung);
                    i++;
                } while (lines[i + 1].Length != 0);
                break;
            }

            // suche zeitabhängige Knotenlast (Temperaturen) Knotentemperaturen
            //  5:Name,NodeId,Amplitude,Frequenz,Phase 
            // >7:Name,NodeId,Wertepaare für stückweise linearen Verlauf
            for (var i = 0; i < lines.Length; i++)
            {
                if (lines[i] != "Zeitabhängige Knotenlasten") continue;
                FeParser.EingabeGefunden += "\nZeitabhängige Knotenlasten";

                do
                {
                    var teilStrings = lines[i + 1].Split(delimiters);
                    if (teilStrings.Length < 3)
                    {
                        throw new ParseAusnahme((i + 2) + ": Zeitabhängige Knotenlast, falsche Anzahl Parameter");
                    }
                    var lastId = teilStrings[0];
                    var knotenId = teilStrings[1];

                    ZeitabhängigeKnotenLast zeitabhängigeKnotenLast = null;

                    switch (teilStrings[2])
                    {
                        case "datei":
                            {
                                zeitabhängigeKnotenLast =
                                    new ZeitabhängigeKnotenLast(knotenId, true) { LastId = lastId };
                                break;
                            }
                        case "harmonisch":
                            {
                                if (teilStrings.Length != 6)
                                {
                                    throw new ParseAusnahme((i + 2) + ": Zeitabhängige Knotenlast harmonisch, falsche Anzahl Parameter");
                                }
                                var amplitude = double.Parse(teilStrings[3]);
                                var frequenz = double.Parse(teilStrings[4]);
                                var phasenWinkel = double.Parse(teilStrings[5]);
                                zeitabhängigeKnotenLast =
                                    new ZeitabhängigeKnotenLast(knotenId, amplitude, frequenz, phasenWinkel)
                                    { LastId = lastId };
                                break;
                            }
                        case "linear":
                            {
                                if (teilStrings.Length < 5)
                                {
                                    throw new ParseAusnahme((i + 2) + ": Zeitabhängige Knotenlast linear, falsche Anzahl Parameter");
                                }
                                var k = 0;
                                char[] paarDelimiter = { ';' };
                                var intervall = new double[2 * (teilStrings.Length - 3)];
                                for (var j = 3; j < teilStrings.Length; j++)
                                {
                                    var wertePaar = teilStrings[j].Split(paarDelimiter);
                                    intervall[k] = double.Parse(wertePaar[0]);
                                    intervall[k + 1] = double.Parse(wertePaar[1]);
                                    k += 2;
                                }
                                zeitabhängigeKnotenLast =
                                    new ZeitabhängigeKnotenLast(knotenId, intervall) { LastId = lastId };
                                break;
                            }
                    }
                    feModell.ZeitabhängigeKnotenLasten.Add(lastId, zeitabhängigeKnotenLast);
                    i++;
                } while (lines[i + 1].Length != 0);
                break;
            }

            // suche zeitabhängigeElementLast auf Dreieckselementen
            //  5:Name,ElementId,Knotenwert1, Knotenwert2, Knotenwert3 
            for (var i = 0; i < lines.Length; i++)
            {
                if (lines[i] != "Zeitabhängige Elementlasten") continue;
                FeParser.EingabeGefunden += "\nZeitabhängige Elementlasten";
                var knotenWerte = new double[3];
                do
                {
                    var teilStrings = lines[i + 1].Split(delimiters);
                    var loadId = teilStrings[0];
                    var elementId = teilStrings[1];
                    ZeitabhängigeElementLast zeitabhängigeElementLast = null;
                    switch (teilStrings[2])
                    {
                        case "konstant":
                            {
                                for (var k = 3; k < teilStrings.Length; k++) knotenWerte[k - 3] =
                                    double.Parse(teilStrings[k]);
                                zeitabhängigeElementLast =
                                    new ZeitabhängigeElementLast(elementId, knotenWerte) { LastId = loadId, VariationsTyp = 1 };
                                break;
                            }
                    }

                    feModell.ZeitabhängigeElementLasten.Add(loadId, zeitabhängigeElementLast);
                    i++;
                } while (lines[i + 1].Length != 0);
                break;
            }
        }
        [Serializable]
        private class ParseException : Exception
        {
            public ParseException() { }
            public ParseException(string message) : base(message) { }
            public ParseException(string message, Exception innerException) : base(message, innerException) { }
            protected ParseException(SerializationInfo info, StreamingContext context) : base(info, context) { }
        }
    }
}
