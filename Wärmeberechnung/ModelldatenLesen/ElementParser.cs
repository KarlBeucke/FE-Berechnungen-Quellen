using FE_Berechnungen.Wärmeberechnung.Modelldaten;

namespace FE_Berechnungen.Wärmeberechnung.ModelldatenLesen;

public class ElementParser : FeParser
{
    private AbstraktElement _element;
    private string _elementId;
    private string _materialId;
    private FeModell _modell;
    private string[] _nodeIds;
    private int _nodesPerElement;
    private string[] substrings;

    // parsing a new model to be read from file
    public void ParseWärmeElements(string[] lines, FeModell feModell)
    {
        _modell = feModell;
        ParseElement2D2(lines);
        ParseElement2D3(lines);
        ParseElement2D4(lines);
        ParseElement3D8(lines);
    }

    private void ParseElement2D2(IReadOnlyList<string> lines)
    {
        _nodesPerElement = 2;
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
                            _elementId = substrings[0];
                            _nodeIds = new string[_nodesPerElement];
                            for (var k = 0; k < _nodesPerElement; k++) _nodeIds[k] = substrings[k + 1];

                            _materialId = substrings[3];
                            _element = new Element2D2(_elementId, _nodeIds, _materialId, _modell);
                            _modell.Elemente.Add(_elementId, _element);
                            i++;
                            break;
                        }
                    default:
                        throw new ParseAusnahme(i + 2 + ": Elemente2D2Knoten, falsche Anzahl Parameter");
                }
            } while (lines[i + 1].Length != 0);

            break;
        }
    }

    private void ParseElement2D3(IReadOnlyList<string> lines)
    {
        _nodesPerElement = 3;
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
                            _elementId = substrings[0];
                            _nodeIds = new string[_nodesPerElement];
                            for (var k = 0; k < _nodesPerElement; k++) _nodeIds[k] = substrings[k + 1];

                            _materialId = substrings[4];

                            _element = new Element2D3(_elementId, _nodeIds, _materialId, _modell);
                            _modell.Elemente.Add(_elementId, _element);
                            i++;
                            break;
                        }
                    default:
                        throw new ParseAusnahme(i + 2 + ":\nElemente2D3Knoten, falsche Anzahl Parameter");
                }
            } while (lines[i + 1].Length != 0);

            break;
        }
    }

    private void ParseElement2D4(IReadOnlyList<string> lines)
    {
        _nodesPerElement = 4;
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
                            _elementId = substrings[0];
                            _nodeIds = new string[_nodesPerElement];
                            for (var k = 0; k < _nodesPerElement; k++) _nodeIds[k] = substrings[k + 1];

                            _materialId = substrings[5];

                            _element = new Element2D4(_elementId, _nodeIds, _materialId, _modell);
                            _modell.Elemente.Add(_elementId, _element);
                            i++;
                            break;
                        }
                    default:
                        throw new ParseAusnahme(i + 2 + ":\nElemente2D4Knoten, falsche Anzahl Parameter");
                }
            } while (lines[i + 1].Length != 0);

            break;
        }
    }

    private void ParseElement3D8(IReadOnlyList<string> lines)
    {
        _nodesPerElement = 8;
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
                            _elementId = substrings[0];
                            _nodeIds = new string[_nodesPerElement];
                            for (var k = 0; k < _nodesPerElement; k++) _nodeIds[k] = substrings[k + 1];

                            _materialId = substrings[9];

                            _element = new Element3D8(_elementId, _nodeIds, _materialId, _modell);
                            _modell.Elemente.Add(_elementId, _element);
                            i++;
                            break;
                        }
                    default:
                        throw new ParseAusnahme(i + 2 + ":\nElemente3D8Knoten, falsche Anzahl Parameter");
                }
            } while (lines[i + 1].Length != 0);

            break;
        }
    }
}