using FE_Berechnungen.Tragwerksberechnung.Modelldaten;
using FEBibliothek.Modell;

namespace FE_Berechnungen.Tragwerksberechnung.ModelldatenLesen
{
    public class RandbedingungParser
    {
        private FEModell modell;
        private readonly char[] delimiters = { '\t' };
        private string[] substrings;
        private string lagerId;
        private string knotenId;
        private Lager lager;

        public void ParseRandbedingungen(string[] lines, FEModell feModell)
        {
            modell = feModell;

            for (var i = 0; i < lines.Length; i++)
            {
                if (lines[i] != "Lager") continue;
                FeParser.EingabeGefunden += "\nLager";
                do
                {
                    substrings = lines[i + 1].Split(delimiters);
                    if (substrings.Length < 7)
                    {
                        lagerId = substrings[0];
                        knotenId = substrings[1];
                        var lagerTyp = 0;
                        var typ = substrings[2];
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
                        if (substrings.Length > 3) vordefiniert[0] = double.Parse(substrings[3]);
                        if (substrings.Length > 4) vordefiniert[1] = double.Parse(substrings[4]);
                        if (substrings.Length > 5) vordefiniert[2] = double.Parse(substrings[5]);
                        lager = new Lager(knotenId, lagerTyp, vordefiniert, modell) { RandbedingungId = lagerId };
                        modell.Randbedingungen.Add(lagerId, lager);
                        i++;
                    }
                    else
                    {
                        throw new ParseAusnahme((i + 2) + ": Lager" + lagerId);
                    }
                } while (lines[i + 1].Length != 0);
                break;
            }
        }
    }
}
