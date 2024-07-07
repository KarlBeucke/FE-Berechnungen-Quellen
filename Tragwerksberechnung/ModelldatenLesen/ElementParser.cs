using FE_Berechnungen.Tragwerksberechnung.Modelldaten;
using FEBibliothek.Modell;
using FEBibliothek.Modell.abstrakte_Klassen;
using System.Collections.Generic;

namespace FE_Berechnungen.Tragwerksberechnung.ModelldatenLesen;

public class ElementParser
{
    private string[] _substrings;
    private int _nodesPerElement;
    private string _elementId;
    private string[] _nodeIds;
    private AbstraktElement _element;
    private FeModell _modell;
    private readonly char[] _delimiters = ['\t'];

    // Elementdefinitionen werden aus Datei gelesen
    public void ParseElements(string[] lines, FeModell feModell)
    {
        _modell = feModell;
        ParseFachwerk(lines);
        ParseBiegebalken(lines);
        ParseFederelement(lines);
        ParseBiegebalkenGelenk(lines);
        ParseQuerschnitte(lines);
    }

    private void ParseFachwerk(IReadOnlyList<string> lines)
    {
        _nodesPerElement = 2;
        for (var i = 0; i < lines.Count; i++)
        {
            if (lines[i] != "Fachwerk") continue;
            FeParser.EingabeGefunden += "\nFachwerk";
            do
            {
                _substrings = lines[i + 1].Split(_delimiters);
                switch (_substrings.Length)
                {
                    case 5:
                        {
                            _elementId = _substrings[0];
                            _nodeIds = new string[_nodesPerElement];
                            for (var k = 0; k < _nodesPerElement; k++)
                            {
                                _nodeIds[k] = _substrings[k + 1];
                            }
                            var materialId = _substrings[3];
                            var querschnittId = _substrings[4];
                            _element = new Fachwerk(_nodeIds, materialId, querschnittId, _modell)
                            {
                                ElementId = _elementId
                            };
                            _modell.Elemente.Add(_elementId, _element);
                            i++;
                            break;
                        }
                    default:
                        throw new ParseAusnahme((i + 2) + ":\nFachwerk, falsche Anzahl Parameter");
                }
            } while (lines[i + 1].Length != 0);
            break;
        }
    }
    private void ParseBiegebalken(IReadOnlyList<string> lines)
    {
        _nodesPerElement = 2;
        for (var i = 0; i < lines.Count; i++)
        {
            if (lines[i] != "Biegebalken") continue;
            FeParser.EingabeGefunden += "\nBiegebalken";
            do
            {
                _substrings = lines[i + 1].Split(_delimiters);
                switch (_substrings.Length)
                {
                    case 5:
                        {
                            _elementId = _substrings[0];
                            _nodeIds = new string[_nodesPerElement];
                            for (var k = 0; k < _nodesPerElement; k++)
                            {
                                _nodeIds[k] = _substrings[k + 1];
                            }
                            var materialId = _substrings[3];
                            var querschnittId = _substrings[4];
                            _element = new Biegebalken(_nodeIds, materialId, querschnittId, _modell)
                            {
                                ElementId = _elementId
                            };
                            _modell.Elemente.Add(_elementId, _element);
                            i++;
                            break;
                        }
                    default:
                        throw new ParseAusnahme((i + 2) + ":\nBiegebalken, falsche Anzahl Parameter");
                }
            } while (lines[i + 1].Length != 0);
            break;
        }
    }
    private void ParseBiegebalkenGelenk(IReadOnlyList<string> lines)
    {
        _nodesPerElement = 2;
        for (var i = 0; i < lines.Count; i++)
        {
            if (lines[i] != "BiegebalkenGelenk") continue;
            FeParser.EingabeGefunden += "\nBiegebalkenGelenk";
            do
            {
                _substrings = lines[i + 1].Split(_delimiters);

                switch (_substrings.Length)
                {
                    case 6:
                        {
                            _elementId = _substrings[0];
                            _nodeIds = new string[_nodesPerElement];
                            for (var k = 0; k < _nodesPerElement; k++)
                            {
                                _nodeIds[k] = _substrings[k + 1];
                            }
                            var materialId = _substrings[3];
                            var querschnittId = _substrings[4];
                            var type = short.Parse(_substrings[5]) switch
                            {
                                1 => 1,
                                2 => 2,
                                _ => throw new ParseAusnahme((i + 2) + ":\nBiegebalkenGelenk, falscher Gelenktyp")
                            };
                            _element = new BiegebalkenGelenk(_nodeIds, materialId, querschnittId, _modell, type)
                            {
                                ElementId = _elementId
                            };
                            _modell.Elemente.Add(_elementId, _element);
                            i++;
                            break;
                        }
                    default:
                        throw new ParseAusnahme((i + 2) + ":\nBiegebalkenGelenk, falsche Anzahl Parameter");
                }
            } while (lines[i + 1].Length != 0);
            break;
        }
    }

    private void ParseFederelement(IReadOnlyList<string> lines)
    {
        _nodesPerElement = 1;
        for (var i = 0; i < lines.Count; i++)
        {
            if (lines[i] != "Federelement") continue;
            FeParser.EingabeGefunden += "\nFederelement";
            do
            {
                _substrings = lines[i + 1].Split(_delimiters);
                switch (_substrings.Length)
                {
                    case 3:
                        {
                            _elementId = _substrings[0];
                            _nodeIds = new string[_nodesPerElement];
                            _nodeIds[0] = _substrings[1];
                            var materialId = _substrings[2];
                            var federLager = new FederElement(_nodeIds, materialId, _modell)
                            {
                                ElementId = _elementId
                            };
                            _modell.Elemente.Add(_elementId, federLager);
                            i++;
                            break;
                        }
                    default:
                        throw new ParseAusnahme(i + 2 + ":\nFederelement, falsche Anzahl Parameter");
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
                _substrings = lines[i + 1].Split(_delimiters);
                switch (_substrings.Length)
                {
                    case 2:
                        {
                            var querschnittId = _substrings[0];
                            var fläche = double.Parse(_substrings[1]);
                            var querschnitt = new Querschnitt(fläche) { QuerschnittId = querschnittId };
                            _modell.Querschnitt.Add(querschnittId, querschnitt);
                            i++;
                            break;
                        }
                    case 3:
                        {
                            var querschnittId = _substrings[0];
                            var fläche = double.Parse(_substrings[1]);
                            var ixx = double.Parse(_substrings[2]);
                            var querschnitt = new Querschnitt(fläche, ixx) { QuerschnittId = querschnittId };
                            _modell.Querschnitt.Add(querschnittId, querschnitt);
                            i++;
                            break;
                        }
                    default:
                        throw new ParseAusnahme((i + 2) + ":\nQuerschnitt, falsche Anzahl Parameter");
                }
            } while (lines[i + 1].Length != 0);
            break;
        }
    }
}