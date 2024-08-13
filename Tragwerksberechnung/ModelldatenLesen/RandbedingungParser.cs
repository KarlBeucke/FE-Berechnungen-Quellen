using FE_Berechnungen.Tragwerksberechnung.Modelldaten;
using FEBibliothek.Modell;
using System;

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
                    //Parameter 1 bis 3 sind LagerId, KnotenId und Lagertyp
                    _lagerId = _substrings[0];
                    _knotenId = _substrings[1];
                    var lagerTyp = 0;
                    var typ = _substrings[2];
                    for (var k = 0; k < typ.Length; k++)
                    {
                        var subTyp = typ.Substring(k, 1);
                        lagerTyp += subTyp switch
                        {
                            "x" => Lager.XFixed,
                            "y" => Lager.Yfixed,
                            "r" => Lager.Rfixed,
                            _ => throw new ParseAusnahme((i + 2) + ":\nLagerTyp muss 'xyr' sein")
                        };
                    }

                    var vordefiniert = new double[3];
                    try
                    {
                        // ab Parameter 4 folgen die vordefinierten Lagerverformungen
                        if (_substrings.Length > 3) vordefiniert[0] = double.Parse(_substrings[3]);
                        if (_substrings.Length > 4) vordefiniert[1] = double.Parse(_substrings[4]);
                        if (_substrings.Length > 5) vordefiniert[2] = double.Parse(_substrings[5]);
                    }
                    catch (FormatException)
                    {
                        throw new ParseAusnahme((i + 2) + ":\nLager vordefiniert, ungültiges  Eingabeformat");
                    }
                    _lager = new Lager(_knotenId, lagerTyp, vordefiniert, _modell) { RandbedingungId = _lagerId };
                    _modell.Randbedingungen.Add(_lagerId, _lager);
                    i++;
                }
                else
                {
                    throw new ParseAusnahme((i + 2) + ":\nLager" + _lagerId);
                }
            } while (lines[i + 1].Length != 0);

            break;
        }
    }
}