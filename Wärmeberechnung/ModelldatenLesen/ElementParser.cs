using FE_Berechnungen.Wärmeberechnung.Modelldaten;
using FEBibliothek.Modell;
using FEBibliothek.Modell.abstrakte_Klassen;
using System.Collections.Generic;

namespace FE_Berechnungen.Wärmeberechnung.ModelldatenLesen
{
    public class ElementParser : FEParser
    {
        private string[] substrings;
        private int nodesPerElement;
        private string elementId;
        private string[] nodeIds;
        private AbstraktElement element;
        private string materialId;
        private FEModell modell;

        // parsing a new model to be read from file
        public void ParseWärmeElements(string[] lines, FEModell feModell)
        {
            modell = feModell;
            ParseElement2D2(lines);
            ParseElement2D3(lines);
            ParseElement2D4(lines);
            ParseElement3D8(lines);
        }

        private void ParseElement2D2(IReadOnlyList<string> lines)
        {
            nodesPerElement = 2;
            var delimiters = new[] { '\t' };

            for (var i = 0; i < lines.Count; i++)
            {
                if (lines[i] != "Elemente2D2Knoten") continue;
                EingabeGefunden += "\nElemente2D2Knoten";
                do
                {
                    substrings = lines[i + 1].Split(delimiters);
                    switch (substrings.Length)
                    {
                        case 4:
                            {
                                elementId = substrings[0];
                                nodeIds = new string[8];
                                for (var k = 0; k < nodesPerElement; k++)
                                {
                                    nodeIds[k] = substrings[k + 1];
                                }

                                materialId = substrings[3];
                                element = new Element2D2(elementId, nodeIds, materialId, modell);
                                modell.Elemente.Add(elementId, element);
                                i++;
                                break;
                            }
                        default:
                            throw new ParseAusnahme((i + 2) + ": Elemente2D2Knoten, falsche Anzahl Parameter");
                    }
                } while (lines[i + 1].Length != 0);
                break;
            }
        }
        private void ParseElement2D3(IReadOnlyList<string> lines)
        {
            nodesPerElement = 3;
            var delimiters = new[] { '\t' };

            for (var i = 0; i < lines.Count; i++)
            {
                if (lines[i] != "Elemente2D3Knoten") continue;
                EingabeGefunden += "\nElemente2D3Knoten";
                do
                {
                    substrings = lines[i + 1].Split(delimiters);
                    switch (substrings.Length)
                    {
                        case 5:
                            {
                                elementId = substrings[0];
                                nodeIds = new string[8];
                                for (var k = 0; k < nodesPerElement; k++)
                                {
                                    nodeIds[k] = substrings[k + 1];
                                }

                                materialId = substrings[4];

                                element = new Element2D3(elementId, nodeIds, materialId, modell);
                                modell.Elemente.Add(elementId, element);
                                i++;
                                break;
                            }
                        default:
                            throw new ParseAusnahme((i + 2) + ": Elemente2D3Knoten, falsche Anzahl Parameter");
                    }
                } while (lines[i + 1].Length != 0);
                break;
            }
        }
        private void ParseElement2D4(IReadOnlyList<string> lines)
        {
            nodesPerElement = 4;
            var delimiters = new[] { '\t' };

            for (var i = 0; i < lines.Count; i++)
            {
                if (lines[i] != "Elemente2D4Knoten") continue;
                EingabeGefunden += "\nElemente2D4Knoten";
                do
                {
                    substrings = lines[i + 1].Split(delimiters);
                    switch (substrings.Length)
                    {
                        case 6:
                            {
                                elementId = substrings[0];
                                nodeIds = new string[8];
                                for (var k = 0; k < nodesPerElement; k++)
                                {
                                    nodeIds[k] = substrings[k + 1];
                                }

                                materialId = substrings[5];

                                element = new Element2D4(elementId, nodeIds, materialId, modell);
                                modell.Elemente.Add(elementId, element);
                                i++;
                                break;
                            }
                        default:
                            throw new ParseAusnahme((i + 2) + ": Elemente2D4Knoten, falsche Anzahl Parameter");
                    }
                } while (lines[i + 1].Length != 0);
                break;
            }
        }
        private void ParseElement3D8(IReadOnlyList<string> lines)
        {
            nodesPerElement = 8;
            var delimiters = new[] { '\t' };

            for (var i = 0; i < lines.Count; i++)
            {
                if (lines[i] != "Elemente3D8Knoten") continue;
                EingabeGefunden += "\nElemente3D8Knoten";
                do
                {
                    substrings = lines[i + 1].Split(delimiters);
                    switch (substrings.Length)
                    {
                        case 10:
                            {
                                elementId = substrings[0];
                                nodeIds = new string[8];
                                for (var k = 0; k < nodesPerElement; k++)
                                {
                                    nodeIds[k] = substrings[k + 1];
                                }

                                materialId = substrings[9];

                                element = new Element3D8(elementId, nodeIds, materialId, modell);
                                modell.Elemente.Add(elementId, element);
                                i++;
                                break;
                            }
                        default:
                            throw new ParseAusnahme((i + 2) + ": Elemente3D8Knoten, falsche Anzahl Parameter");
                    }
                } while (lines[i + 1].Length != 0);
                break;
            }
        }
    }
}