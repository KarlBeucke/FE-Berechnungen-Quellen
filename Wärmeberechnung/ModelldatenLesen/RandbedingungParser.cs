using FE_Berechnungen.Wärmeberechnung.Modelldaten;

namespace FE_Berechnungen.Wärmeberechnung.ModelldatenLesen;

public class RandbedingungParser
{
    private FeModell _modell;
    private string _nodeId;
    private Randbedingung _randbedingung;
    private string[] _substrings;
    private string _supportId;

    public void ParseRandbedingungen(string[] lines, FeModell feModell)
    {
        _modell = feModell;
        var delimiters = new[] { '\t' };

        for (var i = 0; i < lines.Length; i++)
        {
            if (lines[i] != "Randbedingungen") continue;
            FeParser.EingabeGefunden += "\nRandbedingungen";
            do
            {
                _substrings = lines[i + 1].Split(delimiters);
                switch (_substrings.Length)
                {
                    case 3:
                        {
                            _supportId = _substrings[0];
                            _nodeId = _substrings[1];
                            var pre = double.Parse(_substrings[2]);
                            _randbedingung = new Randbedingung(_supportId, _nodeId, pre);
                            _modell.Randbedingungen.Add(_supportId, _randbedingung);
                            i++;
                            break;
                        }
                    default:
                        throw new ParseAusnahme(i + 2 + ":\nRandbedingungen, falsche Anzahl Parameter");
                }
            } while (lines[i + 1].Length != 0);

            break;
        }
    }
}