using FE_Berechnungen.Elastizitätsberechnung.Modelldaten;
using FEBibliothek.Modell;
using FEBibliothek.Modell.abstrakte_Klassen;
using System;
using System.Collections.Generic;

namespace FE_Berechnungen.Elastizitätsberechnung.ModelldatenLesen;

public class ElementParser
{
    private string[] substrings;
    private string elementId;
    private string[] nodeIds;
    private AbstraktElement element;
    private FeModell modell;

    // parsing a new model to be read from file
    public void ParseElements(string[] lines, FeModell feModell)
    {
        modell = feModell;
        ParseElement2D3(lines);
        ParseElement3D8(lines);
        ParseElement3D8Netz(lines);
        ParseQuerschnitte(lines);
    }

    private void ParseElement2D3(IReadOnlyList<string> lines)
    {
        const int nodesPerElement = 3;
        var delimiters = new[] { '\t' };

        for (var i = 0; i < lines.Count; i++)
        {
            if (lines[i] != "Element2D3") continue;
            FeParser.EingabeGefunden += "\nElement2D3";
            do
            {
                substrings = lines[i + 1].Split(delimiters);
                if (substrings.Length == 6)
                {
                    elementId = substrings[0];
                    nodeIds = new string[nodesPerElement];
                    for (var k = 0; k < nodesPerElement; k++)
                    {
                        nodeIds[k] = substrings[k + 1];
                    }

                    var querschnittId = substrings[4];
                    var materialId = substrings[5];
                    element = new Element2D3(nodeIds, querschnittId, materialId, modell) { ElementId = elementId };
                    modell.Elemente.Add(elementId, element);
                    i++;
                }
                else
                {
                    throw new ParseAusnahme((i + 1) + ":\nElement2D3 erfordert 6 Eingabeparameter");
                }
            } while (lines[i + 1].Length != 0);
            break;
        }
    }

    private void ParseElement3D8(IReadOnlyList<string> lines)
    {
        const int nodesPerElement = 8;
        var delimiters = new[] { '\t' };

        for (var i = 0; i < lines.Count; i++)
        {
            if (lines[i] != "Element3D8") continue;
            FeParser.EingabeGefunden += "\nElement3D8";
            do
            {
                substrings = lines[i + 1].Split(delimiters);
                if (substrings.Length == 10)
                {
                    elementId = substrings[0];
                    nodeIds = new string[nodesPerElement];
                    for (var k = 0; k < nodesPerElement; k++)
                    {
                        nodeIds[k] = substrings[k + 1];
                    }
                    var materialId = substrings[9];
                    element = new Element3D8(nodeIds, materialId, modell) { ElementId = elementId };
                    modell.Elemente.Add(elementId, element);
                    i++;
                }
                else
                {
                    throw new ParseAusnahme((i + 1) + ":\nElement3D8 erfordert 10 Eingabeparameter");
                }
            } while (lines[i + 1].Length != 0);
            break;
        }
    }

    private void ParseElement3D8Netz(string[] lines)
    {
        const int nodesPerElement = 8;
        var delimiters = new[] { '\t' };

        for (var i = 0; i < lines.Length; i++)
        {
            if (lines[i] != "3D8ElementNetz") continue;
            FeParser.EingabeGefunden += "\n3D8ElementNetz";
            do
            {
                substrings = lines[i + 1].Split(delimiters);
                if (substrings.Length != 4)
                {
                    throw new ParseAusnahme((i + 1) + ":\nfalsche Anzahl Parameter für Elementeingabe:\n"
                                                    + "muss gleich 4 sein für elementName, Knotennetzname,"
                                                    + "Anzahl der Intervalle und Elementmaterial");
                }
                var initial = substrings[0];
                var eNodeName = substrings[1];
                int nIntervals = short.Parse(substrings[2]);
                var eMaterial = substrings[3];


                for (var n = 0; n < nIntervals; n++)
                {
                    var idX = n.ToString().PadLeft(2, '0');
                    var idXp = (n + 1).ToString().PadLeft(2, '0');
                    for (var m = 0; m < nIntervals; m++)
                    {
                        var idY = m.ToString().PadLeft(2, '0');
                        var idYp = (m + 1).ToString().PadLeft(2, '0');
                        for (var k = 0; k < nIntervals; k++)
                        {
                            var idZ = k.ToString().PadLeft(2, '0');
                            var idZp = (k + 1).ToString().PadLeft(2, '0');
                            var eNode = new String[nodesPerElement];
                            var elementName = initial + idX + idY + idZ;
                            if (modell.Elemente.TryGetValue(elementName, out element))
                                throw new ParseAusnahme($"\nElement \"{elementName}\" bereits vorhanden.");
                            eNode[0] = eNodeName + idX + idY + idZ;
                            eNode[1] = eNodeName + idXp + idY + idZ;
                            eNode[2] = eNodeName + idXp + idYp + idZ;
                            eNode[3] = eNodeName + idX + idYp + idZ;
                            eNode[4] = eNodeName + idX + idY + idZp;
                            eNode[5] = eNodeName + idXp + idY + idZp;
                            eNode[6] = eNodeName + idXp + idYp + idZp;
                            eNode[7] = eNodeName + idX + idYp + idZp;
                            element = new Element3D8(eNode, eMaterial, modell) { ElementId = elementName };
                            modell.Elemente.Add(elementName, element);
                        }
                    }
                }
            } while (lines[i + 2].Length != 0);
            break;
        }
    }

    private void ParseQuerschnitte(IReadOnlyList<string> lines)
    {
        var delimiters = new[] { '\t' };

        for (var i = 0; i < lines.Count; i++)
        {
            if (lines[i] != "Querschnitt") continue;
            FeParser.EingabeGefunden += "\nQuerschnitt";
            do
            {
                substrings = lines[i + 1].Split(delimiters);
                var querschnittId = substrings[0];
                var thickness = double.Parse(substrings[1]);

                var querschnitt = new Querschnitt(thickness) { QuerschnittId = querschnittId };
                modell.Querschnitt.Add(querschnittId, querschnitt);
                i++;
            } while (lines[i + 1].Length != 0);
            break;
        }
    }
}