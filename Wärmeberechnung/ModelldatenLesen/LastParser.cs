using FE_Berechnungen.Wärmeberechnung.Modelldaten;

namespace FE_Berechnungen.Wärmeberechnung.ModelldatenLesen;

public class LastParser
{
    private string _elementId;
    private ElementLast3 _elementLast3;
    private ElementLast4 _elementLast4;
    private KnotenLast _knotenLast;
    private LinienLast _linienLast;
    private string _loadId;
    private FeModell _modell;
    private string _nodeId, _startNodeId, _endNodeId;
    private double[] _p;
    private string[] _substrings;

    public void ParseLasten(string[] lines, FeModell feModel)
    {
        _modell = feModel;
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
                _substrings = lines[i + 1].Split(delimiters);
                switch (_substrings.Length)
                {
                    case 3:
                        {
                            _loadId = _substrings[0];
                            _nodeId = _substrings[1];
                            _p = new double[1];
                            _p[0] = double.Parse(_substrings[2]);
                            _knotenLast = new KnotenLast(_loadId, _nodeId, _p);
                            _modell.Lasten.Add(_loadId, _knotenLast);
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
                _substrings = lines[i + 1].Split(delimiters);
                switch (_substrings.Length)
                {
                    case 5:
                        {
                            _loadId = _substrings[0];
                            _startNodeId = _substrings[1];
                            _endNodeId = _substrings[2];
                            _p = new double[2];
                            _p[0] = double.Parse(_substrings[3]);
                            _p[1] = double.Parse(_substrings[4]);
                            _linienLast = new LinienLast(_loadId, _startNodeId, _endNodeId, _p);
                            _modell.LinienLasten.Add(_loadId, _linienLast);
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
                _substrings = lines[i + 1].Split(delimiters);
                switch (_substrings.Length)
                {
                    case 5:
                        {
                            _loadId = _substrings[0];
                            _elementId = _substrings[1];
                            _p = new double[3];
                            _p[0] = double.Parse(_substrings[2]);
                            _p[1] = double.Parse(_substrings[3]);
                            _p[2] = double.Parse(_substrings[4]);
                            _elementLast3 = new ElementLast3(_loadId, _elementId, _p);
                            _modell.ElementLasten.Add(_loadId, _elementLast3);
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
                _substrings = lines[i + 1].Split(delimiters);
                switch (_substrings.Length)
                {
                    case 6:
                        {
                            _loadId = _substrings[0];
                            _elementId = _substrings[1];
                            _p = new double[4];
                            _p[0] = double.Parse(_substrings[2]);
                            _p[1] = double.Parse(_substrings[3]);
                            _p[2] = double.Parse(_substrings[4]);
                            _p[3] = double.Parse(_substrings[5]);
                            _elementLast4 = new ElementLast4(_loadId, _elementId, _p);
                            _modell.ElementLasten.Add(_loadId, _elementLast4);
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