namespace FEBibliothek.Modell
{
    public class FEParser
    {
        private string knotenId, knotenPrefix;
        private string[] substrings;
        private readonly char[] delimiters = { '\t' };
        private double[] koords;
        private int anzahlKnotenfreiheitsgrade, zaehler;
        private double xIntervall, yIntervall, zIntervall;
        private int nKnotenX, nKnotenY;

        public string ModellId { get; set; }
        public FEModell FeModell { get; set; }
        public int Raumdimension { get; set; }
        public static string EingabeGefunden { get; set; }

        // parsing ein neues Modell aus einer Datei
        public void ParseModell(string[] zeilen)
        {
            for (var i = 0; i < zeilen.Length; i++)
            {
                EingabeGefunden = "";
                if (zeilen[i] != "ModellName") continue;
                ModellId = zeilen[i + 1];
                EingabeGefunden = "ModellName = " + ModellId;
                break;
            }

            for (var i = 0; i < zeilen.Length; i++)
            {
                if (zeilen[i] != "Raumdimension") continue;
                EingabeGefunden += "\nRaumdimension";
                substrings = zeilen[i + 1].Split(delimiters);
                Raumdimension = int.Parse(substrings[0]);
                anzahlKnotenfreiheitsgrade = int.Parse(substrings[1]);
                break;
            }
            FeModell = new FEModell(ModellId, Raumdimension);
        }

        // KnotenId, Knotenkoordinaten
        public void ParseNodes(string[] zeilen)
        {
            for (var i = 0; i < zeilen.Length; i++)
            {
                if (zeilen[i] == "Knoten")
                {
                    EingabeGefunden += "\nKnoten";
                    do
                    {
                        substrings = zeilen[i + 1].Split(delimiters);
                        Knoten knoten;
                        koords = new double[3];
                        var dimension = FeModell.Raumdimension;
                        switch (substrings.Length)
                        {
                            case 1:
                                anzahlKnotenfreiheitsgrade = int.Parse(substrings[0]);
                                break;
                            case 2:
                                knotenId = substrings[0];
                                koords[0] = double.Parse(substrings[1]);
                                knoten = new Knoten(knotenId, koords, anzahlKnotenfreiheitsgrade, dimension);
                                FeModell.Knoten.Add(knotenId, knoten);
                                break;
                            case 3:
                                knotenId = substrings[0];
                                koords[0] = double.Parse(substrings[1]);
                                koords[1] = double.Parse(substrings[2]);
                                knoten = new Knoten(knotenId, koords, anzahlKnotenfreiheitsgrade, dimension);
                                FeModell.Knoten.Add(knotenId, knoten);
                                break;
                            case 4:
                                knotenId = substrings[0];
                                koords[0] = double.Parse(substrings[1]);
                                koords[1] = double.Parse(substrings[2]);
                                koords[2] = double.Parse(substrings[3]);
                                knoten = new Knoten(knotenId, koords, anzahlKnotenfreiheitsgrade, dimension);
                                FeModell.Knoten.Add(knotenId, knoten);
                                break;
                            default:
                                {
                                    throw new ParseAusnahme((i + 2) + ": Knoten " + knotenId + " falsche Anzahl Parameter");
                                }
                        }
                        i++;
                    } while (zeilen[i + 1].Length != 0);
                }
                //Knotengruppe
                if (zeilen[i] == "Knotengruppe")
                {
                    EingabeGefunden += "\nKnotengruppe";
                    substrings = zeilen[i + 1].Split(delimiters);
                    if (substrings.Length == 1) knotenPrefix = substrings[0];
                    else
                        throw new ParseAusnahme(i + 2 + ": Knotengruppe");
                    zaehler = 0;
                    i += 2;
                    do
                    {
                        substrings = zeilen[i].Split(delimiters);
                        koords = new double[3];
                        if (substrings.Length == Raumdimension)
                            for (var k = 0; k < Raumdimension; k++)
                                koords[k] = double.Parse(substrings[k]);
                        else
                            throw new ParseAusnahme(i + ": Knotengruppe");

                        //raumdimension += anzahlKnotenfreiheitsgrade;
                        knotenId = knotenPrefix + zaehler.ToString().PadLeft(6, '0');
                        var node = new Knoten(knotenId, koords, anzahlKnotenfreiheitsgrade, Raumdimension);
                        FeModell.Knoten.Add(knotenId, node);
                        i++;
                        zaehler++;
                    } while (zeilen[i].Length != 0);
                }

                //Äquidistantes Knotennetz in 1D
                if (zeilen[i] == "Äquidistantes Knotennetz")
                    do
                    {
                        substrings = zeilen[i + 1].Split(delimiters);
                        knotenPrefix = substrings[0];

                        //Äquidistantes Knotennetz in 1D
                        double[] knotenKoords;
                        switch (substrings.Length)
                        {
                            case 4:
                                {
                                    EingabeGefunden += "\nÄquidistantes Knotennetz in 1D";
                                    koords[0] = double.Parse(substrings[1]);
                                    xIntervall = double.Parse(substrings[2]);
                                    nKnotenX = short.Parse(substrings[3]);

                                    for (var k = 0; k < nKnotenX; k++)
                                    {
                                        knotenId = knotenPrefix + "0000" + k.ToString().PadLeft(2, '0');
                                        knotenKoords = new[] { koords[0], 0 };
                                        var node = new Knoten(knotenId, knotenKoords, anzahlKnotenfreiheitsgrade, Raumdimension);
                                        FeModell.Knoten.Add(knotenId, node);
                                        koords[0] += xIntervall;
                                    }

                                    break;
                                }
                            //Äquidistantes Knotennetz in 2D
                            case 7:
                                {
                                    EingabeGefunden += "\nÄquidistantes Knotennetz in 2D";
                                    koords = new double[3];
                                    koords[0] = double.Parse(substrings[1]);
                                    xIntervall = double.Parse(substrings[2]);
                                    nKnotenX = short.Parse(substrings[3]);
                                    koords[1] = double.Parse(substrings[4]);
                                    yIntervall = double.Parse(substrings[5]);
                                    nKnotenY = short.Parse(substrings[6]);

                                    var idZ = "00";
                                    for (var k = 0; k < nKnotenX; k++)
                                    {
                                        var temp = koords[1];
                                        var idY = k.ToString().PadLeft(2, '0');
                                        for (var l = 0; l < nKnotenY; l++)
                                        {
                                            var idX = l.ToString().PadLeft(2, '0');
                                            knotenId = knotenPrefix + idX + idY + idZ;
                                            knotenKoords = new[] { koords[0], koords[1] };
                                            var node = new Knoten(knotenId, knotenKoords, anzahlKnotenfreiheitsgrade, Raumdimension);
                                            FeModell.Knoten.Add(knotenId, node);
                                            koords[1] += yIntervall;
                                        }

                                        koords[0] += xIntervall;
                                        koords[1] = temp;
                                    }

                                    break;
                                }
                            //Äquidistantes Knotennetz in 3D
                            case 10:
                                {
                                    EingabeGefunden += "\nÄquidistantes Knotennetz in 3D";
                                    koords = new double[3];
                                    koords[0] = double.Parse(substrings[1]);
                                    xIntervall = double.Parse(substrings[2]);
                                    nKnotenX = short.Parse(substrings[3]);
                                    koords[1] = double.Parse(substrings[4]);
                                    yIntervall = double.Parse(substrings[5]);
                                    nKnotenY = short.Parse(substrings[6]);
                                    koords[2] = double.Parse(substrings[7]);
                                    zIntervall = double.Parse(substrings[8]);

                                    for (var k = 0; k < nKnotenX; k++)
                                    {
                                        var temp1 = koords[1];
                                        var idX = k.ToString().PadLeft(2, '0');
                                        for (var l = 0; l < nKnotenY; l++)
                                        {
                                            var temp2 = koords[2];
                                            var idY = l.ToString().PadLeft(2, '0');
                                            knotenId = knotenPrefix + idX + idY;
                                            for (var m = 0; m < nKnotenY; m++)
                                            {
                                                var idZ = m.ToString().PadLeft(2, '0');
                                                knotenId = knotenPrefix + idX + idY + idZ;
                                                knotenKoords = new[] { koords[0], koords[1], koords[2] };
                                                var node = new Knoten(knotenId, knotenKoords, anzahlKnotenfreiheitsgrade, Raumdimension);
                                                FeModell.Knoten.Add(knotenId, node);
                                                koords[2] += zIntervall;
                                            }

                                            koords[2] = temp2;
                                            koords[1] += yIntervall;
                                        }

                                        koords[1] = temp1;
                                        koords[0] += xIntervall;
                                    }

                                    break;
                                }
                            default:
                                {
                                    throw new ParseAusnahme(i + 3 + ": Äquidistantes Knotennetz");
                                }
                        }

                        i++;
                    } while (zeilen[i + 1].Length != 0);

                //variables Knotennetz
                if (zeilen[i] != "Variables Knotennetz") continue;
                {
                    do
                    {
                        substrings = zeilen[i + 1].Split(delimiters);
                        EingabeGefunden += "\nVariables Knotennetz";
                        substrings = zeilen[i + 1].Split(delimiters);
                        string idX, idY;
                        koords = new double[3];

                        double koord0, koord1;
                        var offset = new double[substrings.Length];
                        for (var k = 0; k < substrings.Length; k++)
                            offset[k] = double.Parse(substrings[k]);

                        substrings = zeilen[i + 2].Split(delimiters);
                        double[] knotenKoords;
                        switch (substrings.Length)
                        {
                            case 2:
                                {
                                    knotenPrefix = substrings[0];
                                    koord0 = double.Parse(substrings[1]);
                                    for (var n = 0; n < offset.Length; n++)
                                    {
                                        koords[0] = koord0 + offset[n];
                                        knotenId = knotenPrefix + "0000" + n.ToString().PadLeft(2, '0');
                                        knotenKoords = new[] { koords[0], 0 };
                                        var node = new Knoten(knotenId, knotenKoords, anzahlKnotenfreiheitsgrade, Raumdimension);
                                        FeModell.Knoten.Add(knotenId, node);
                                    }

                                    break;
                                }
                            case 3:
                                {
                                    knotenPrefix = substrings[0];
                                    var idZ = "00";
                                    koord0 = double.Parse(substrings[1]);
                                    koord1 = double.Parse(substrings[2]);
                                    for (var n = 0; n < offset.Length; n++)
                                    {
                                        idX = n.ToString().PadLeft(2, '0');
                                        koords[0] = koord0 + offset[n];
                                        for (var m = 0; m < offset.Length; m++)
                                        {
                                            idY = m.ToString().PadLeft(2, '0');
                                            koords[1] = koord1 + offset[m];
                                            knotenId = knotenPrefix + idX + idY + idZ;
                                            knotenKoords = new[] { koords[0], koords[1] };
                                            var node = new Knoten(knotenId, knotenKoords, anzahlKnotenfreiheitsgrade, Raumdimension);
                                            FeModell.Knoten.Add(knotenId, node);
                                        }
                                    }

                                    break;
                                }
                            case 4:
                                {
                                    knotenPrefix = substrings[0];
                                    koord0 = double.Parse(substrings[1]);
                                    koord1 = double.Parse(substrings[2]);
                                    var coord2 = double.Parse(substrings[3]);
                                    for (var n = 0; n < offset.Length; n++)
                                    {
                                        idX = n.ToString().PadLeft(2, '0');
                                        var inkrement0 = koord0 + offset[n];
                                        for (var m = 0; m < offset.Length; m++)
                                        {
                                            idY = m.ToString().PadLeft(2, '0');
                                            var inkrement1 = koord1 + offset[m];
                                            for (var k = 0; k < offset.Length; k++)
                                            {
                                                koords = new double[3];
                                                koords[0] = inkrement0;
                                                koords[1] = inkrement1;
                                                var idZ = k.ToString().PadLeft(2, '0');
                                                koords[2] = coord2 + offset[k];
                                                knotenId = knotenPrefix + idX + idY + idZ;
                                                knotenKoords = new[] { koords[0], koords[1], koords[2] };
                                                var node = new Knoten(knotenId, knotenKoords, anzahlKnotenfreiheitsgrade, Raumdimension);
                                                FeModell.Knoten.Add(knotenId, node);
                                            }
                                        }
                                    }

                                    break;
                                }
                            default:
                                {
                                    throw new ParseAusnahme(i + 3 + ": Variables Knotennetz");
                                }
                        }
                    } while (zeilen[i + 3].Length != 0);
                }
            }
        }
    }
}