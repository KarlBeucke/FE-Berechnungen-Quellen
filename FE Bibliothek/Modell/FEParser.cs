namespace FEBibliothek.Modell
{
    public class FeParser
    {
        private string _knotenId, _knotenPrefix;
        private string[] _substrings;
        private readonly char[] _delimiters = { '\t' };
        private double[] _koords;
        private int _zähler;
        private double _xIntervall, _yIntervall, _zIntervall;
        private int _nKnotenX, _nKnotenY, _nKnotenZ;

        private string ModellId { get; set; }
        public FeModell FeModell { get; private set; }
        private int Raumdimension { get; set; }
        private int AnzahlKnotenfreiheitsgrade { get; set; }
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
                _substrings = zeilen[i + 1].Split(_delimiters);
                Raumdimension = int.Parse(_substrings[0]);
                AnzahlKnotenfreiheitsgrade = int.Parse(_substrings[1]);
                EingabeGefunden += "\nRaumdimension = " + Raumdimension + ", Anzahl Knotenfreiheitsgrade = " + AnzahlKnotenfreiheitsgrade;
                break;
            }
            FeModell = new FeModell(ModellId, Raumdimension, AnzahlKnotenfreiheitsgrade);
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
                        _substrings = zeilen[i + 1].Split(_delimiters);
                        Knoten knoten;
                        _koords = new double[3];
                        var dimension = FeModell.Raumdimension;
                        switch (_substrings.Length)
                        {
                            case 1:
                                AnzahlKnotenfreiheitsgrade = int.Parse(_substrings[0]);
                                break;
                            case 2:
                                _knotenId = _substrings[0];
                                _koords[0] = double.Parse(_substrings[1]);
                                knotenKoords = new[] { _koords[0] };
                                knoten = new Knoten(_knotenId, knotenKoords, AnzahlKnotenfreiheitsgrade, dimension);
                                FeModell.Knoten.Add(_knotenId, knoten);
                                break;
                            case 3:
                                _knotenId = _substrings[0];
                                _koords[0] = double.Parse(_substrings[1]);
                                _koords[1] = double.Parse(_substrings[2]);
                                knotenKoords = new[] { _koords[0], _koords[1] };
                                knoten = new Knoten(_knotenId, knotenKoords, AnzahlKnotenfreiheitsgrade, dimension);
                                FeModell.Knoten.Add(_knotenId, knoten);
                                break;
                            case 4:
                                _knotenId = _substrings[0];
                                _koords[0] = double.Parse(_substrings[1]);
                                _koords[1] = double.Parse(_substrings[2]);
                                _koords[2] = double.Parse(_substrings[3]);
                                knotenKoords = new[] { _koords[0], _koords[1], _koords[2] };
                                knoten = new Knoten(_knotenId, knotenKoords, AnzahlKnotenfreiheitsgrade, dimension);
                                FeModell.Knoten.Add(_knotenId, knoten);
                                break;
                            default:
                                throw new ParseAusnahme((i + 2) + ": Knoten " + _knotenId + " falsche Anzahl Parameter");
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
                        _substrings = zeilen[i].Split(_delimiters);
                        if (_substrings.Length == 1) _knotenPrefix = _substrings[0];
                        else
                            throw new ParseAusnahme(i + 2 + ": Knotengruppe falscher Prefix");
                        _zähler = 0;
                        while (zeilen[i + 1].Length > 1)
                        {
                            _substrings = zeilen[i + 1].Split(_delimiters);
                            knotenKoords = new double[_substrings.Length];
                            for (var k = 0; k < _substrings.Length; k++)
                                knotenKoords[k] = double.Parse(_substrings[k]);

                            _knotenId = _knotenPrefix + _zähler.ToString().PadLeft(2 * _substrings.Length, '0');
                            var node = new Knoten(_knotenId, knotenKoords, AnzahlKnotenfreiheitsgrade, Raumdimension);
                            FeModell.Knoten.Add(_knotenId, node);
                            _zähler++;
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
                        _substrings = zeilen[i].Split(_delimiters);
                        _knotenPrefix = _substrings[0];

                        switch (_substrings.Length)
                        {
                            //Äquidistantes Knotennetz in 1D
                            case 4:
                                EingabeGefunden += "\nÄquidistantes Knotennetz in 1D";
                                _koords[0] = double.Parse(_substrings[1]);
                                _xIntervall = double.Parse(_substrings[2]);
                                _nKnotenX = short.Parse(_substrings[3]);

                                for (var k = 0; k < _nKnotenX; k++)
                                {
                                    _knotenId = _knotenPrefix + k.ToString().PadLeft(2, '0');
                                    knotenKoords = new[] { _koords[0] };
                                    var node = new Knoten(_knotenId, knotenKoords, AnzahlKnotenfreiheitsgrade, Raumdimension);
                                    FeModell.Knoten.Add(_knotenId, node);
                                    _koords[0] += _xIntervall;
                                }

                                i++;
                                break;
                            //Äquidistantes Knotennetz in 2D
                            case 7:
                                EingabeGefunden += "\nÄquidistantes Knotennetz in 2D";
                                _koords = new double[3];
                                _koords[0] = double.Parse(_substrings[1]);
                                _xIntervall = double.Parse(_substrings[2]);
                                _nKnotenX = short.Parse(_substrings[3]);
                                _koords[1] = double.Parse(_substrings[4]);
                                _yIntervall = double.Parse(_substrings[5]);
                                _nKnotenY = short.Parse(_substrings[6]);

                                for (var k = 0; k < _nKnotenX; k++)
                                {
                                    var temp = _koords[0];
                                    var idY = k.ToString().PadLeft(2, '0');
                                    for (var l = 0; l < _nKnotenY; l++)
                                    {
                                        var idX = l.ToString().PadLeft(2, '0');
                                        _knotenId = _knotenPrefix + idX + idY;
                                        knotenKoords = new[] { _koords[0], _koords[1] };
                                        var node = new Knoten(_knotenId, knotenKoords, AnzahlKnotenfreiheitsgrade,
                                            Raumdimension);
                                        FeModell.Knoten.Add(_knotenId, node);
                                        _koords[0] += _xIntervall;
                                    }

                                    _koords[1] += _yIntervall;
                                    _koords[0] = temp;
                                }
                                i++;
                                break;
                            //Äquidistantes Knotennetz in 3D
                            case 10:
                                EingabeGefunden += "\nÄquidistantes Knotennetz in 3D";
                                _koords = new double[3];
                                _koords[0] = double.Parse(_substrings[1]);
                                _xIntervall = double.Parse(_substrings[2]);
                                _nKnotenX = short.Parse(_substrings[3]);
                                _koords[1] = double.Parse(_substrings[4]);
                                _yIntervall = double.Parse(_substrings[5]);
                                _nKnotenY = short.Parse(_substrings[6]);
                                _koords[2] = double.Parse(_substrings[7]);
                                _zIntervall = double.Parse(_substrings[8]);
                                _nKnotenZ = short.Parse(_substrings[9]);

                                for (var k = 0; k < _nKnotenZ; k++)
                                {
                                    var temp1 = _koords[1];
                                    var idZ = k.ToString().PadLeft(2, '0');
                                    for (var l = 0; l < _nKnotenY; l++)
                                    {
                                        var temp0 = _koords[0];
                                        var idY = l.ToString().PadLeft(2, '0');
                                        for (var m = 0; m < _nKnotenX; m++)
                                        {
                                            var idX = m.ToString().PadLeft(2, '0');
                                            _knotenId = _knotenPrefix + idX + idY + idZ;
                                            knotenKoords = new[] { _koords[0], _koords[1], _koords[2] };
                                            var node = new Knoten(_knotenId, knotenKoords, AnzahlKnotenfreiheitsgrade,
                                                Raumdimension);
                                            FeModell.Knoten.Add(_knotenId, node);
                                            _koords[0] += _xIntervall;
                                        }

                                        _koords[0] = temp0;
                                        _koords[1] += _yIntervall;
                                    }

                                    _koords[1] = temp1;
                                    _koords[2] += _zIntervall;
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
                        _substrings = zeilen[i].Split(_delimiters);
                        _koords = new double[3];

                        var offset = new double[_substrings.Length];
                        for (var k = 0; k < _substrings.Length; k++)
                            offset[k] = double.Parse(_substrings[k]);

                        _substrings = zeilen[i + 1].Split(_delimiters);
                        string idX, idY;
                        double koord0, koord1;
                        switch (_substrings.Length)
                        {
                            case 2:
                                _knotenPrefix = _substrings[0];
                                koord0 = double.Parse(_substrings[1]);
                                for (var n = 0; n < offset.Length; n++)
                                {
                                    _koords[0] = koord0 + offset[n];
                                    _knotenId = _knotenPrefix + n.ToString().PadLeft(2);
                                    knotenKoords = new[] { _koords[0] };
                                    var node = new Knoten(_knotenId, knotenKoords, AnzahlKnotenfreiheitsgrade, Raumdimension);
                                    FeModell.Knoten.Add(_knotenId, node);
                                }
                                break;
                            case 3:
                                _knotenPrefix = _substrings[0];
                                koord0 = double.Parse(_substrings[1]);
                                koord1 = double.Parse(_substrings[2]);
                                for (var n = 0; n < offset.Length; n++)
                                {
                                    idY = n.ToString().PadLeft(2, '0');
                                    _koords[1] = koord1 + offset[n];
                                    for (var m = 0; m < offset.Length; m++)
                                    {
                                        idX = m.ToString().PadLeft(2, '0');
                                        _koords[0] = koord0 + offset[m];
                                        _knotenId = _knotenPrefix + idX + idY;
                                        knotenKoords = new[] { _koords[0], _koords[1] };
                                        var node = new Knoten(_knotenId, knotenKoords, AnzahlKnotenfreiheitsgrade, Raumdimension);
                                        FeModell.Knoten.Add(_knotenId, node);
                                    }
                                }
                                break;
                            case 4:
                                _knotenPrefix = _substrings[0];
                                koord0 = double.Parse(_substrings[1]);
                                koord1 = double.Parse(_substrings[2]);
                                var koord2 = double.Parse(_substrings[3]);
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
                                            _koords = new double[3];
                                            _koords[1] = inkrement1;
                                            _koords[2] = inkrement2;
                                            idX = k.ToString().PadLeft(2, '0');
                                            _koords[0] = koord0 + offset[k];
                                            _knotenId = _knotenPrefix + idX + idY + idZ;
                                            knotenKoords = new[] { _koords[0], _koords[1], _koords[2] };
                                            var node = new Knoten(_knotenId, knotenKoords, AnzahlKnotenfreiheitsgrade, Raumdimension);
                                            FeModell.Knoten.Add(_knotenId, node);
                                        }
                                    }
                                }
                                break;
                            default:
                                throw new ParseAusnahme(i + 1 + ": Variables Knotennetz");
                        }

                        i += 2;
                        if (zeilen[i] == string.Empty) break;
                    }
                    break;
                }
            }
        }
    }
}