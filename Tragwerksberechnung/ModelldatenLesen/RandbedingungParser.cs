using FE_Berechnungen.Tragwerksberechnung.Modelldaten;
using FEBibliothek.Modell;

namespace FE_Berechnungen.Tragwerksberechnung.ModelldatenLesen;

public class RandbedingungParser
{
    private readonly char[] _delimiters = ['\t'];
    private string _knotenId;
    private Lager _lager;
    private string _lagerId;
    private FeModell _modell;
    private string[] _substrings;

    public void ParseRandbedingungen(string[] lines, FeModell feModell)
    {
        _modell = feModell;

        for (var i = 0; i < lines.Length; i++)
        {
            if (lines[i] != "Lager") continue;
            FeParser.EingabeGefunden += "\nLager";
            do
            {
                _substrings = lines[i + 1].Split(_delimiters);
                if (_substrings.Length < 7)
                {
                    _lagerId = _substrings[0];
                    _knotenId = _substrings[1];
                    var lagerTyp = 0;
                    var typ = _substrings[2];
                    for (var k = 0; k < typ.Length; k++)
                    {
                        var subTyp = typ.Substring(k, 1);
                        switch (subTyp)
                        {
                            case "x":
                                lagerTyp += Lager.XFixed;
                                break;
                            case "y":
                                lagerTyp += Lager.YFixed;
                                break;
                            case "r":
                                lagerTyp += Lager.RFixed;
                                break;
                        }
                    }

                    var vordefiniert = new double[3];
                    if (_substrings.Length > 3) vordefiniert[0] = double.Parse(_substrings[3]);
                    if (_substrings.Length > 4) vordefiniert[1] = double.Parse(_substrings[4]);
                    if (_substrings.Length > 5) vordefiniert[2] = double.Parse(_substrings[5]);
                    _lager = new Lager(_knotenId, lagerTyp, vordefiniert, _modell) { RandbedingungId = _lagerId };
                    _modell.Randbedingungen.Add(_lagerId, _lager);
                    i++;
                }
                else
                {
                    throw new ParseAusnahme(i + 2 + ":\nLager" + _lagerId);
                }
            } while (lines[i + 1].Length != 0);

            break;
        }
    }
}