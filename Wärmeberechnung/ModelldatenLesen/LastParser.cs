using FE_Berechnungen.Wärmeberechnung.Modelldaten;
using FEBibliothek.Modell;

namespace FE_Berechnungen.Wärmeberechnung.ModelldatenLesen;

public class LastParser
{
    private string elementId;
    private ElementLast3 elementLast3;
    private ElementLast4 elementLast4;
    private KnotenLast knotenLast;
    private LinienLast linienLast;
    private string loadId;
    private FeModell modell;
    private string nodeId, startNodeId, endNodeId;
    private double[] p;
    private string[] substrings;

    public void ParseLasten(string[] lines, FeModell feModel)
    {
        modell = feModel;
        //p = new double[8];
        ParseKnotenLast(lines);
        ParseLinienLast(lines);
        ParseElementLast3(lines);
        ParseElementLast4(lines);
    }

    private void ParseKnotenLast(string[] lines)
    {
        var delimiters = new[] { '\t' };

        for (var i = 0; i < lines.Length; i++)
        {
            if (lines[i] != "KnotenLasten") continue;
            FeParser.EingabeGefunden += "\nKnotenLasten";
            do
            {
                substrings = lines[i + 1].Split(delimiters);
                switch (substrings.Length)
                {
                    case 3:
                        {
                            loadId = substrings[0];
                            nodeId = substrings[1];
                            p = new double[1];
                            p[0] = double.Parse(substrings[2]);
                            knotenLast = new KnotenLast(loadId, nodeId, p);
                            modell.Lasten.Add(loadId, knotenLast);
                            i++;
                            break;
                        }
                    default:
                        throw new ParseAusnahme(i + 2 + ":\nKnotenLasten, falsche Anzahl Parameter");
                }
            } while (lines[i + 1].Length != 0);

            break;
        }
    }

    private void ParseLinienLast(string[] lines)
    {
        var delimiters = new[] { '\t' };

        for (var i = 0; i < lines.Length; i++)
        {
            if (lines[i] != "LinienLasten") continue;
            FeParser.EingabeGefunden += "\nLinienLasten";
            do
            {
                substrings = lines[i + 1].Split(delimiters);
                switch (substrings.Length)
                {
                    case 5:
                        {
                            loadId = substrings[0];
                            startNodeId = substrings[1];
                            endNodeId = substrings[2];
                            p = new double[2];
                            p[0] = double.Parse(substrings[3]);
                            p[1] = double.Parse(substrings[4]);
                            linienLast = new LinienLast(loadId, startNodeId, endNodeId, p);
                            modell.LinienLasten.Add(loadId, linienLast);
                            i++;
                            break;
                        }
                    default:
                        throw new ParseAusnahme(i + 2 + ":\nLinienLasten, falsche Anzahl Parameter");
                }
            } while (lines[i + 1].Length != 0);

            break;
        }
    }

    private void ParseElementLast3(string[] lines)
    {
        var delimiters = new[] { '\t' };

        for (var i = 0; i < lines.Length; i++)
        {
            if (lines[i] != "ElementLast3") continue;
            FeParser.EingabeGefunden += "\nElementLast3";
            do
            {
                substrings = lines[i + 1].Split(delimiters);
                switch (substrings.Length)
                {
                    case 5:
                        {
                            loadId = substrings[0];
                            elementId = substrings[1];
                            p = new double[3];
                            p[0] = double.Parse(substrings[2]);
                            p[1] = double.Parse(substrings[3]);
                            p[2] = double.Parse(substrings[4]);
                            elementLast3 = new ElementLast3(loadId, elementId, p);
                            modell.ElementLasten.Add(loadId, elementLast3);
                            i++;
                            break;
                        }
                    default:
                        throw new ParseAusnahme(i + 2 + ":\nElementLast3, falsche Anzahl Parameter");
                }
            } while (lines[i + 1].Length != 0);

            break;
        }
    }

    private void ParseElementLast4(string[] lines)
    {
        var delimiters = new[] { '\t' };

        for (var i = 0; i < lines.Length; i++)
        {
            if (lines[i] != "ElementLast4") continue;
            FeParser.EingabeGefunden += "\nElementLast4";
            do
            {
                substrings = lines[i + 1].Split(delimiters);
                switch (substrings.Length)
                {
                    case 6:
                        {
                            loadId = substrings[0];
                            elementId = substrings[1];
                            p = new double[4];
                            p[0] = double.Parse(substrings[2]);
                            p[1] = double.Parse(substrings[3]);
                            p[2] = double.Parse(substrings[4]);
                            p[3] = double.Parse(substrings[5]);
                            elementLast4 = new ElementLast4(loadId, elementId, p);
                            modell.ElementLasten.Add(loadId, elementLast4);
                            i++;
                            break;
                        }
                    default:
                        throw new ParseAusnahme(i + 2 + ":\nElementLast4, falsche Anzahl Parameter");
                }
            } while (lines[i + 1].Length != 0);

            break;
        }
    }
}