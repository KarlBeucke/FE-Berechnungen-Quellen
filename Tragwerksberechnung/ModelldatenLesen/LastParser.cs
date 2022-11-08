using FE_Berechnungen.Tragwerksberechnung.Modelldaten;
using FEBibliothek.Modell;
using FEBibliothek.Modell.abstrakte_Klassen;
using System.Collections.Generic;

namespace FE_Berechnungen.Tragwerksberechnung.ModelldatenLesen;

internal class LastParser
{
    private FeModell modell;
    private string[] substrings;
    private readonly char[] delimiters = { '\t' };

    private string loadId;
    private string elementId;
    private string nodeId;
    private double[] p;
    private KnotenLast knotenLast;
    private PunktLast punktLast;
    private double offset;
    private bool inElementCoordinateSystem;

    public void ParseLasten(string[] lines, FeModell feModel)
    {
        modell = feModel;

        ParseKnotenLast(lines);
        ParsePunktLast(lines);
        ParseLinienLast(lines);
    }

    private void ParseKnotenLast(IReadOnlyList<string> lines)
    {
        for (var i = 0; i < lines.Count; i++)
        {
            if (lines[i] != "Knotenlast") continue;
            FeParser.EingabeGefunden += "\nKnotenlast";
            do
            {
                substrings = lines[i + 1].Split(delimiters);

                p = new double[3];
                switch (substrings.Length)
                {
                    case 4:
                        loadId = substrings[0];
                        nodeId = substrings[1];
                        p[0] = double.Parse(substrings[2]);
                        p[1] = double.Parse(substrings[3]);
                        knotenLast = new KnotenLast(nodeId, p[0], p[1]);
                        break;
                    case 5:
                        loadId = substrings[0];
                        nodeId = substrings[1];
                        p[0] = double.Parse(substrings[2]);
                        p[1] = double.Parse(substrings[3]);
                        p[2] = double.Parse(substrings[4]);
                        knotenLast = new KnotenLast(nodeId, p[0], p[1], p[2])
                        {
                            LastId = loadId
                        };
                        break;
                    default:
                        {
                            throw new ParseAusnahme((i + 2) + ": Fachwerk, falsche Anzahl Parameter");
                        }
                }
                modell.Lasten.Add(loadId, knotenLast);
                i++;
            } while (lines[i + 1].Length != 0);
            break;
        }
    }
    private void ParsePunktLast(IReadOnlyList<string> lines)
    {
        for (var i = 0; i < lines.Count; i++)
        {
            if (lines[i] != "Punktlast") continue;
            FeParser.EingabeGefunden += "\nPunktlast";
            do
            {
                // Punktlast durch Normalkraft, Querkraft auf Stab und prozentualem Offset zum Stabanfang
                // z.B. Element Normalkraft pN=0, Querkraft pQ=2 mit Angriff in Elementmitte offset = 0,5
                substrings = lines[i + 1].Split(delimiters);
                switch (substrings.Length)
                {
                    case 5:
                        loadId = substrings[0];
                        elementId = substrings[1];
                        p = new double[3];
                        p[0] = double.Parse(substrings[2]);
                        p[1] = double.Parse(substrings[3]);
                        offset = double.Parse(substrings[4]);

                        punktLast = new PunktLast(elementId, p[0], p[1], offset);
                        punktLast.LastId = loadId;
                        modell.PunktLasten.Add(loadId, punktLast);
                        i++;
                        break;
                    default:
                        throw new ParseAusnahme((i + 2) + ": Punktlast");
                }
            } while (lines[i + 1].Length != 0);
            break;
        }
    }
    private void ParseLinienLast(IReadOnlyList<string> lines)
    {
        for (var i = 0; i < lines.Count; i++)
        {
            if (lines[i] != "Linienlast") continue;
            FeParser.EingabeGefunden += "\nLinienlast";
            do
            {
                // Linenlast definiert durch p0, p1, p2, p3 mit optionalem inElementCoordinateSystem: default= true
                // mit lokalen Koordinaten p0N, p0Q, p1N, p1Q   für inElementCoordinateSystem = true
                // mit globalen Koordinaten p0x, p0y, p1x, p1y, inElementCoordinateSystem = false
                substrings = lines[i + 1].Split(delimiters);

                p = new double[4];
                AbstraktLinienlast linienLast;
                switch (substrings.Length)
                {
                    case 6:
                        loadId = substrings[0];
                        elementId = substrings[1];
                        p[0] = double.Parse(substrings[2]);
                        p[1] = double.Parse(substrings[3]);
                        p[2] = double.Parse(substrings[4]);
                        p[3] = double.Parse(substrings[5]);
                        linienLast = new LinienLast(elementId, p[0], p[1], p[2], p[3]); // inElementCoordinateSystem = true
                        linienLast.LastId = loadId;
                        modell.ElementLasten.Add(loadId, linienLast);
                        i++;
                        break;
                    case 7:
                        loadId = substrings[0];
                        elementId = substrings[1];
                        p[0] = double.Parse(substrings[2]);
                        p[1] = double.Parse(substrings[3]);
                        p[2] = double.Parse(substrings[4]);
                        p[3] = double.Parse(substrings[5]);
                        inElementCoordinateSystem = bool.Parse(substrings[6]);
                        linienLast = new LinienLast(elementId, p[0], p[1], p[2], p[3], inElementCoordinateSystem); //inElementCoordinateSystem = input
                        linienLast.LastId = loadId;
                        modell.ElementLasten.Add(loadId, linienLast);
                        i++;
                        break;
                    default:
                        throw new ParseAusnahme((i + 2) + ": Linienlast, falsche Anzahl Parameter");
                }
            } while (lines[i + 1].Length != 0);
            break;
        }
    }
}