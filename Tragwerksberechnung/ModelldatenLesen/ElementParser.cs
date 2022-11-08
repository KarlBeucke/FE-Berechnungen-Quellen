using FE_Berechnungen.Tragwerksberechnung.Modelldaten;
using FEBibliothek.Modell;
using FEBibliothek.Modell.abstrakte_Klassen;
using System.Collections.Generic;

namespace FE_Berechnungen.Tragwerksberechnung.ModelldatenLesen;

public class ElementParser
{
    private string[] substrings;
    private int nodesPerElement;
    private string elementId;
    private string[] nodeIds;
    private AbstraktElement element;
    private FeModell modell;
    private readonly char[] delimiters = { '\t' };

    // parsing a new model to be read from file
    public void ParseElements(string[] lines, FeModell feModell)
    {
        modell = feModell;
        ParseFachwerk(lines);
        ParseBiegebalken(lines);
        ParseFederelement(lines);
        ParseBiegebalkenGelenk(lines);
        ParseQuerschnitte(lines);
    }

    private void ParseFachwerk(IReadOnlyList<string> lines)
    {
        nodesPerElement = 2;
        for (var i = 0; i < lines.Count; i++)
        {
            if (lines[i] != "Fachwerk") continue;
            FeParser.EingabeGefunden += "\nFachwerk";
            do
            {
                substrings = lines[i + 1].Split(delimiters);
                switch (substrings.Length)
                {
                    case 5:
                        {
                            elementId = substrings[0];
                            nodeIds = new string[nodesPerElement];
                            for (var k = 0; k < nodesPerElement; k++)
                            {
                                nodeIds[k] = substrings[k + 1];
                            }
                            var querschnittId = substrings[3];
                            var materialId = substrings[4];
                            element = new Fachwerk(nodeIds, querschnittId, materialId, modell)
                            {
                                ElementId = elementId
                            };
                            modell.Elemente.Add(elementId, element);
                            i++;
                            break;
                        }
                    default:
                        throw new ParseAusnahme((i + 2) + ": Fachwerk, falsche Anzahl Parameter");
                }
            } while (lines[i + 1].Length != 0);
            break;
        }
    }
    private void ParseBiegebalken(IReadOnlyList<string> lines)
    {
        nodesPerElement = 2;
        for (var i = 0; i < lines.Count; i++)
        {
            if (lines[i] != "Biegebalken") continue;
            FeParser.EingabeGefunden += "\nBiegebalken";
            do
            {
                substrings = lines[i + 1].Split(delimiters);
                switch (substrings.Length)
                {
                    case 5:
                        {
                            elementId = substrings[0];
                            nodeIds = new string[nodesPerElement];
                            for (var k = 0; k < nodesPerElement; k++)
                            {
                                nodeIds[k] = substrings[k + 1];
                            }
                            var querschnittId = substrings[3];
                            var materialId = substrings[4];
                            element = new Biegebalken(nodeIds, querschnittId, materialId, modell)
                            {
                                ElementId = elementId
                            };
                            modell.Elemente.Add(elementId, element);
                            i++;
                            break;
                        }
                    default:
                        throw new ParseAusnahme((i + 2) + ": Biegebalken, falsche Anzahl Parameter");
                }
            } while (lines[i + 1].Length != 0);
            break;
        }
    }
    private void ParseBiegebalkenGelenk(IReadOnlyList<string> lines)
    {
        nodesPerElement = 2;
        for (var i = 0; i < lines.Count; i++)
        {
            if (lines[i] != "BiegebalkenGelenk") continue;
            FeParser.EingabeGefunden += "\nBiegebalkenGelenk";
            do
            {
                substrings = lines[i + 1].Split(delimiters);

                switch (substrings.Length)
                {
                    case 6:
                        {
                            elementId = substrings[0];
                            nodeIds = new string[nodesPerElement];
                            for (var k = 0; k < nodesPerElement; k++)
                            {
                                nodeIds[k] = substrings[k + 1];
                            }
                            var querschnittId = substrings[3];
                            var materialId = substrings[4];
                            int type;
                            switch (short.Parse(substrings[5]))
                            {
                                //if (string.Equals(gelenk, "F", StringComparison.OrdinalIgnoreCase)) type = 1;
                                //else if (string.Equals(gelenk, "S", StringComparison.OrdinalIgnoreCase)) type = 2;
                                case 1:
                                    type = 1;
                                    break;
                                case 2:
                                    type = 2;
                                    break;
                                default:
                                    throw new ParseAusnahme((i + 2) + ": BiegebalkenGelenk, falscher Gelenktyp");
                            }
                            element = new BiegebalkenGelenk(nodeIds, materialId, querschnittId, modell, type)
                            {
                                ElementId = elementId
                            };
                            modell.Elemente.Add(elementId, element);
                            i++;
                            break;
                        }
                    default:
                        throw new ParseAusnahme((i + 2) + ": BiegebalkenGelenk, falsche Anzahl Parameter");
                }
            } while (lines[i + 1].Length != 0);
            break;
        }
    }

    private void ParseFederelement(IReadOnlyList<string> lines)
    {
        nodesPerElement = 1;
        for (var i = 0; i < lines.Count; i++)
        {
            if (lines[i] != "Federelement") continue;
            FeParser.EingabeGefunden += "\nFederelement";
            do
            {
                substrings = lines[i + 1].Split(delimiters);
                switch (substrings.Length)
                {
                    case 3:
                        {
                            elementId = substrings[0];
                            nodeIds = new string[nodesPerElement];
                            nodeIds[0] = substrings[1];
                            var materialId = substrings[2];
                            var federLager = new FederElement(nodeIds, materialId, modell)
                            {
                                ElementId = elementId
                            };
                            modell.Elemente.Add(elementId, federLager);
                            i++;
                            break;
                        }
                    default:
                        throw new ParseAusnahme((i + 2) + ": Federelement, falsche Anzahl Parameter");
                }
            } while (lines[i + 1].Length != 0);
            break;
        }
    }
    private void ParseQuerschnitte(IReadOnlyList<string> lines)
    {
        for (var i = 0; i < lines.Count; i++)
        {
            if (lines[i] != "Querschnitt") continue;
            FeParser.EingabeGefunden += "\nQuerschnitt";
            do
            {
                substrings = lines[i + 1].Split(delimiters);
                switch (substrings.Length)
                {
                    case 2:
                        {
                            var querschnittId = substrings[0];
                            var fläche = double.Parse(substrings[1]);
                            var querschnitt = new Querschnitt(fläche) { QuerschnittId = querschnittId };
                            modell.Querschnitt.Add(querschnittId, querschnitt);
                            i++;
                            break;
                        }
                    case 3:
                        {
                            var querschnittId = substrings[0];
                            var flaeche = double.Parse(substrings[1]);
                            var ixx = double.Parse(substrings[2]);
                            var querschnitt = new Querschnitt(flaeche, ixx) { QuerschnittId = querschnittId };
                            modell.Querschnitt.Add(querschnittId, querschnitt);
                            i++;
                            break;
                        }
                    default:
                        throw new ParseAusnahme((i + 2) + ": Querschnitt, falsche Anzahl Parameter");
                }
            } while (lines[i + 1].Length != 0);
            break;
        }
    }
}