namespace FEBibliothek.Modell
{
    public class FeParser
    {
        private string knotenId, knotenPrefix;
        private string[] substrings;
        private readonly char[] delimiters = { '\t' };
        private double[] koords;
        private int anzahlKnotenfreiheitsgrade, zaehler;
        private double xIntervall, yIntervall, zIntervall;
        private int nKnotenX, nKnotenY, nKnotenZ;

        private string ModellId { get; set; }
        public FEModell FeModell { get; private set; }
        private int Raumdimension { get; set; }
        public static string EingabeGefunden { get; set; }

        // parsing ein neues Modell aus einer Datei
        public void ParseModell(string[] zeilen)
        {
            for (var i = 0; i < zeilen.Length; i++)
            {
                EingabeGefunden = string.Empty;
                if (zeilen[i] != "ModellName") continue;
                ModellId = zeilen[i + 1];
                EingabeGefunden = "ModellName = " + ModellId;
                break;
            }

            for (var i = 0; i < zeilen.Length; i++)
            {
                if (zeilen[i] != "Raumdimension") continue;
                substrings = zeilen[i + 1].Split(delimiters);
                Raumdimension = int.Parse(substrings[0]);
                anzahlKnotenfreiheitsgrade = int.Parse(substrings[1]);
                EingabeGefunden += "\nRaumdimension = " + Raumdimension + ", Anzahl Knotenfreiheitsgrade = " + anzahlKnotenfreiheitsgrade;
                break;
            }
            FeModell = new FEModell(ModellId, Raumdimension);
        }

        // KnotenId, Knotenkoordinaten
        public void ParseNodes(string[] zeilen)
        {
            for (var i = 0; i < zeilen.Length; i++)
            {
                double[] knotenKoords;
                if (zeilen[i] == "Knoten")
                {
                    EingabeGefunden += "\nKnoten";
                    while (i + 1 <= zeilen.Length)
                    {
                        if (zeilen[i + 1] == string.Empty) break;
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
                                knotenKoords = new[] { koords[0]};
                                knoten = new Knoten(knotenId, knotenKoords, anzahlKnotenfreiheitsgrade, dimension);
                                FeModell.Knoten.Add(knotenId, knoten);
                                break;
                            case 3:
                                knotenId = substrings[0];
                                koords[0] = double.Parse(substrings[1]);
                                koords[1] = double.Parse(substrings[2]);
                                knotenKoords = new[] { koords[0], koords[1] };
                                knoten = new Knoten(knotenId, knotenKoords, anzahlKnotenfreiheitsgrade, dimension);
                                FeModell.Knoten.Add(knotenId, knoten);
                                break;
                            case 4:
                                knotenId = substrings[0];
                                koords[0] = double.Parse(substrings[1]);
                                koords[1] = double.Parse(substrings[2]);
                                koords[2] = double.Parse(substrings[3]);
                                knotenKoords = new[] { koords[0], koords[1], koords[2] };
                                knoten = new Knoten(knotenId, knotenKoords, anzahlKnotenfreiheitsgrade, dimension);
                                FeModell.Knoten.Add(knotenId, knoten);
                                break;
                            default:
                                throw new ParseAusnahme((i + 2) + ": Knoten " + knotenId + " falsche Anzahl Parameter");
                        }
                        i++;
                    }
                }
                //Knotengruppe
                if (zeilen[i] == "Knotengruppe")
                {
                    EingabeGefunden += "\nKnotengruppe";
                    i++;
                    while (i <= zeilen.Length)
                    {
                        if (zeilen[i] == string.Empty) break;
                        substrings = zeilen[i].Split(delimiters);
                        if (substrings.Length == 1) knotenPrefix = substrings[0];
                        else
                            throw new ParseAusnahme(i + 2 + ": Knotengruppe falscher Prefix");
                        zaehler = 0;
                        while (zeilen[i+1].Length > 1)
                        {
                            substrings = zeilen[i + 1].Split(delimiters);
                            knotenKoords = new double[substrings.Length];
                            for (var k = 0; k < substrings.Length; k++)
                                knotenKoords[k] = double.Parse(substrings[k]);

                            knotenId = knotenPrefix + zaehler.ToString().PadLeft(2*substrings.Length, '0');
                            var node = new Knoten(knotenId, knotenKoords, anzahlKnotenfreiheitsgrade, Raumdimension);
                            FeModell.Knoten.Add(knotenId, node);
                            zaehler++;
                            i++;
                        }
                        i++;
                    }
                }

                //Äquidistantes Knotennetz
                if (zeilen[i] == "Äquidistantes Knotennetz")
                {
                    i++;
                    while (i < zeilen.Length)
                    {
                        if (zeilen[i] == string.Empty) break;
                        substrings = zeilen[i].Split(delimiters);
                        knotenPrefix = substrings[0];

                        switch (substrings.Length)
                        {
                            //Äquidistantes Knotennetz in 1D
                            case 4:
                                EingabeGefunden += "\nÄquidistantes Knotennetz in 1D";
                                koords[0] = double.Parse(substrings[1]);
                                xIntervall = double.Parse(substrings[2]);
                                nKnotenX = short.Parse(substrings[3]);

                                for (var k = 0; k < nKnotenX; k++)
                                {
                                    knotenId = knotenPrefix + k.ToString().PadLeft(2, '0');
                                    knotenKoords = new[] { koords[0]};
                                    var node = new Knoten(knotenId, knotenKoords, anzahlKnotenfreiheitsgrade, Raumdimension);
                                    FeModell.Knoten.Add(knotenId, node);
                                    koords[0] += xIntervall;
                                }

                                i++;
                                break;
                            //Äquidistantes Knotennetz in 2D
                            case 7:
                                EingabeGefunden += "\nÄquidistantes Knotennetz in 2D";
                                koords = new double[3];
                                koords[0] = double.Parse(substrings[1]);
                                xIntervall = double.Parse(substrings[2]);
                                nKnotenX = short.Parse(substrings[3]);
                                koords[1] = double.Parse(substrings[4]);
                                yIntervall = double.Parse(substrings[5]);
                                nKnotenY = short.Parse(substrings[6]);

                                for (var k = 0; k < nKnotenX; k++)
                                {
                                    var temp = koords[0];
                                    var idY = k.ToString().PadLeft(2, '0');
                                    for (var l = 0; l < nKnotenY; l++)
                                    {
                                        var idX = l.ToString().PadLeft(2, '0');
                                        knotenId = knotenPrefix + idX + idY;
                                        knotenKoords = new[] { koords[0], koords[1] };
                                        var node = new Knoten(knotenId, knotenKoords, anzahlKnotenfreiheitsgrade,
                                            Raumdimension);
                                        FeModell.Knoten.Add(knotenId, node);
                                        koords[0] += xIntervall;
                                    }

                                    koords[1] += yIntervall;
                                    koords[0] = temp;
                                }
                                i++;
                                break;
                            //Äquidistantes Knotennetz in 3D
                            case 10:
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
                                nKnotenZ = short.Parse(substrings[9]);

                                for (var k = 0; k < nKnotenZ; k++)
                                {
                                    var temp1 = koords[1];
                                    var idZ = k.ToString().PadLeft(2, '0');
                                    for (var l = 0; l < nKnotenY; l++)
                                    {
                                        var temp0 = koords[0];
                                        var idY = l.ToString().PadLeft(2, '0');
                                        for (var m = 0; m < nKnotenX; m++)
                                        {
                                            var idX = m.ToString().PadLeft(2, '0');
                                            knotenId = knotenPrefix + idX + idY + idZ;
                                            knotenKoords = new[] { koords[0], koords[1], koords[2] };
                                            var node = new Knoten(knotenId, knotenKoords, anzahlKnotenfreiheitsgrade,
                                                Raumdimension);
                                            FeModell.Knoten.Add(knotenId, node);
                                            koords[0] += xIntervall;
                                        }

                                        koords[0] = temp0;
                                        koords[1] += yIntervall;
                                    }

                                    koords[1] = temp1;
                                    koords[2] += zIntervall;
                                }

                                i++;
                                break;
                            default:
                                throw new ParseAusnahme(i + 3 + ": Äquidistantes Knotennetz");
                        }
                    }
                }

                //variables Knotennetz
                if (zeilen[i] != "Variables Knotennetz") continue;
                {
                    if (zeilen[i] == string.Empty) break;
                    EingabeGefunden += "\nVariables Knotennetz";

                    i++;
                    while (i < zeilen.Length)
                    {
                        substrings = zeilen[i].Split(delimiters);
                        koords = new double[3];

                        var offset = new double[substrings.Length];
                        for (var k = 0; k < substrings.Length; k++)
                            offset[k] = double.Parse(substrings[k]);

                        substrings = zeilen[i+1].Split(delimiters);
                        string idX, idY;
                        double koord0, koord1;
                        switch (substrings.Length)
                        {
                            case 2:
                                knotenPrefix = substrings[0];
                                koord0 = double.Parse(substrings[1]);
                                for (var n = 0; n < offset.Length; n++)
                                {
                                    koords[0] = koord0 + offset[n];
                                    knotenId = knotenPrefix + n.ToString().PadLeft(2);
                                    knotenKoords = new[] { koords[0]};
                                    var node = new Knoten(knotenId, knotenKoords, anzahlKnotenfreiheitsgrade, Raumdimension);
                                    FeModell.Knoten.Add(knotenId, node);
                                }
                                break;
                            case 3:
                                knotenPrefix = substrings[0];
                                koord0 = double.Parse(substrings[1]);
                                koord1 = double.Parse(substrings[2]);
                                for (var n = 0; n < offset.Length; n++)
                                {
                                    idY = n.ToString().PadLeft(2, '0');
                                    koords[1] = koord1 + offset[n];
                                    for (var m = 0; m < offset.Length; m++)
                                    {
                                        idX = m.ToString().PadLeft(2, '0');
                                        koords[0] = koord0 + offset[m];
                                        knotenId = knotenPrefix + idX + idY;
                                        knotenKoords = new[] { koords[0], koords[1] };
                                        var node = new Knoten(knotenId, knotenKoords, anzahlKnotenfreiheitsgrade, Raumdimension);
                                        FeModell.Knoten.Add(knotenId, node);
                                    }
                                }
                                break;
                            case 4:
                                knotenPrefix = substrings[0];
                                koord0 = double.Parse(substrings[1]);
                                koord1 = double.Parse(substrings[2]);
                                var koord2 = double.Parse(substrings[3]);
                                for (var n = 0; n < offset.Length; n++)
                                {
                                    var idZ = n.ToString().PadLeft(2, '0');
                                    var inkrement2 = koord2 + offset[n];
                                    for (var m = 0; m < offset.Length; m++)
                                    {
                                        idY = m.ToString().PadLeft(2, '0');
                                        var inkrement1 = koord1 + offset[m];
                                        for (var k = 0; k < offset.Length; k++)
                                        {
                                            koords = new double[3];
                                            koords[1] = inkrement1;
                                            koords[2] = inkrement2;
                                            idX = k.ToString().PadLeft(2, '0');
                                            koords[0] = koord0 + offset[k];
                                            knotenId = knotenPrefix + idX + idY + idZ;
                                            knotenKoords = new[] { koords[0], koords[1], koords[2] };
                                            var node = new Knoten(knotenId, knotenKoords, anzahlKnotenfreiheitsgrade, Raumdimension);
                                            FeModell.Knoten.Add(knotenId, node);
                                        }
                                    }
                                }
                                break;
                            default:
                                throw new ParseAusnahme(i + 1 + ": Variables Knotennetz");
                        }

                        i+=2;
                        if (zeilen[i] == string.Empty) break;
                    }
                    break;
                }
            }
        }
    }
}