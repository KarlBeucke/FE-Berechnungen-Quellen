using FE_Berechnungen.Tragwerksberechnung.Modelldaten;

namespace FE_Berechnungen.Tragwerksberechnung.ModelldatenLesen;

internal class MaterialParser
{
    private readonly char[] _delimiters = { '\t' };
    private double _eModul;
    private double _kx, _ky, _kphi;
    private double _masse;
    private Material _material;
    private string _materialId;
    private FeModell _modell;
    private double _poisson;
    private string[] _substrings;

    public void ParseMaterials(string[] lines, FeModell feModell)
    {
        _modell = feModell;

        for (var i = 0; i < lines.Length; i++)
        {
            if (lines[i] != "Material") continue;
            FeParser.EingabeGefunden += "\nMaterial";
            do
            {
                try
                {
                    _substrings = lines[i + 1].Split(_delimiters);
                    if (_substrings.Length is > 1 and < 6)
                    {
                        _materialId = _substrings[0];
                        switch (_substrings.Length)
                        {
                            case 2:
                                _eModul = double.Parse(_substrings[1]);
                                _material = new Material(_eModul)
                                {
                                    MaterialId = _materialId
                                };
                                _modell.Material.Add(_materialId, _material);
                                break;
                            case 3:
                                _eModul = double.Parse(_substrings[1]);
                                _poisson = double.Parse(_substrings[2]);
                                _material = new Material(_eModul, _poisson)
                                {
                                    MaterialId = _materialId
                                };
                                _modell.Material.Add(_materialId, _material);
                                break;
                            case 4:
                                _eModul = double.Parse(_substrings[1]);
                                _poisson = double.Parse(_substrings[2]);
                                _masse = double.Parse(_substrings[3]);
                                _material = new Material(_eModul, _poisson, _masse)
                                {
                                    MaterialId = _materialId
                                };
                                _modell.Material.Add(_materialId, _material);
                                break;
                            case 5:
                                {
                                    var feder = _substrings[1];
                                    _kx = double.Parse(_substrings[2]);
                                    _ky = double.Parse(_substrings[3]);
                                    _kphi = double.Parse(_substrings[4]);
                                    _material = new Material(true, _kx, _ky, _kphi)
                                    {
                                        MaterialId = _materialId
                                    };
                                    _modell.Material.Add(_materialId, _material);
                                    break;
                                }
                        }

                        i++;
                    }
                    else
                    {
                        throw new ParseAusnahme((i + 2) + ":\nMaterial " + _materialId);
                    }
                }
                catch (FormatException)
                {
                    throw new ParseAusnahme((i + 2) + ":\nMaterial, ungültiges Eingabeformat");
                }
            } while (lines[i + 1].Length != 0);

            break;
        }
    }
}