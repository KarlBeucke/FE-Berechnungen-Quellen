using FE_Berechnungen.Elastizitätsberechnung.Modelldaten;

namespace FE_Berechnungen.Elastizitätsberechnung.ModelldatenLesen;

public class ElementParser
{
    private AbstraktElement _element;
    private string _elementId;
    private FeModell _modell;
    private string[] _nodeIds;
    private string[] _substrings;

    // parsing a new model to be read from file
    public void ParseElements(string[] lines, FeModell feModell)
    {
        _modell = feModell;
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
                _substrings = lines[i + 1].Split(delimiters);
                if (_substrings.Length == 6)
                {
                    _elementId = _substrings[0];
                    _nodeIds = new string[nodesPerElement];
                    for (var k = 0; k < nodesPerElement; k++) _nodeIds[k] = _substrings[k + 1];

                    var querschnittId = _substrings[4];
                    var materialId = _substrings[5];
                    _element = new Element2D3(_nodeIds, querschnittId, materialId, _modell) { ElementId = _elementId };
                    _modell.Elemente.Add(_elementId, _element);
                    i++;
                }
                else
                {
                    throw new ParseAusnahme(i + 1 + ":\nElement2D3 erfordert 6 Eingabeparameter");
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
                _substrings = lines[i + 1].Split(delimiters);
                if (_substrings.Length == 10)
                {
                    _elementId = _substrings[0];
                    _nodeIds = new string[nodesPerElement];
                    for (var k = 0; k < nodesPerElement; k++) _nodeIds[k] = _substrings[k + 1];
                    var materialId = _substrings[9];
                    _element = new Element3D8(_nodeIds, materialId, _modell) { ElementId = _elementId };
                    _modell.Elemente.Add(_elementId, _element);
                    i++;
                }
                else
                {
                    throw new ParseAusnahme(i + 1 + ":\nElement3D8 erfordert 10 Eingabeparameter");
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
                _substrings = lines[i + 1].Split(delimiters);
                if (_substrings.Length != 4)
                    throw new ParseAusnahme(i + 1 + ":\nfalsche Anzahl Parameter für Elementeingabe:\n"
                                            + "muss gleich 4 sein für elementName, Knotennetzname,"
                                            + "Anzahl der Intervalle und Elementmaterial");
                var initial = _substrings[0];
                var eNodeName = _substrings[1];
                int nIntervals = short.Parse(_substrings[2]);
                var eMaterial = _substrings[3];

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
                            var eNode = new string[nodesPerElement];
                            var elementName = initial + idX + idY + idZ;
                            if (_modell.Elemente.TryGetValue(elementName, out _element))
                                throw new ParseAusnahme($"\nElement \"{elementName}\" bereits vorhanden.");
                            eNode[0] = eNodeName + idX + idY + idZ;
                            eNode[1] = eNodeName + idXp + idY + idZ;
                            eNode[2] = eNodeName + idXp + idYp + idZ;
                            eNode[3] = eNodeName + idX + idYp + idZ;
                            eNode[4] = eNodeName + idX + idY + idZp;
                            eNode[5] = eNodeName + idXp + idY + idZp;
                            eNode[6] = eNodeName + idXp + idYp + idZp;
                            eNode[7] = eNodeName + idX + idYp + idZp;
                            _element = new Element3D8(eNode, eMaterial, _modell) { ElementId = elementName };
                            _modell.Elemente.Add(elementName, _element);
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
                _substrings = lines[i + 1].Split(delimiters);
                var querschnittId = _substrings[0];
                var dicke = double.Parse(_substrings[1]);

                var querschnitt = new Querschnitt(dicke) { QuerschnittId = querschnittId };
                _modell.Querschnitt.Add(querschnittId, querschnitt);
                i++;
            } while (lines[i + 1].Length != 0);

            break;
        }
    }
}