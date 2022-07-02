using FE_Berechnungen.Tragwerksberechnung.Modelldaten;
using FEBibliothek.Modell;

namespace FE_Berechnungen.Tragwerksberechnung.ModelldatenLesen
{
    internal class MaterialParser
    {
        private FEModell modell;
        private readonly char[] delimiters = { '\t' };
        private string[] substrings;
        private string materialId;
        private Material material;
        private double eModul;
        private double poisson;
        private double masse;
        private double kx, ky, kphi;

        public void ParseMaterials(string[] lines, FEModell feModell)
        {
            modell = feModell;

            for (var i = 0; i < lines.Length; i++)
            {
                if (lines[i] != "Material") continue;
                FeParser.EingabeGefunden += "\nMaterial";
                do
                {
                    substrings = lines[i + 1].Split(delimiters);
                    if (substrings.Length > 1 && substrings.Length < 6)
                    {
                        materialId = substrings[0];
                        switch (substrings.Length)
                        {
                            case 2:
                                eModul = double.Parse(substrings[1]);
                                material = new Material(eModul)
                                {
                                    MaterialId = materialId
                                };
                                modell.Material.Add(materialId, material);
                                break;
                            case 3:
                                eModul = double.Parse(substrings[1]);
                                poisson = double.Parse(substrings[2]);
                                material = new Material(eModul, poisson)
                                {
                                    MaterialId = materialId
                                };
                                modell.Material.Add(materialId, material);
                                break;
                            case 4:
                                eModul = double.Parse(substrings[1]);
                                poisson = double.Parse(substrings[2]);
                                masse = double.Parse(substrings[3]);
                                material = new Material(eModul, poisson, masse)
                                {
                                    MaterialId = materialId
                                };
                                modell.Material.Add(materialId, material);
                                break;
                            case 5:
                                {
                                    var feder = substrings[1];
                                    kx = double.Parse(substrings[2]);
                                    ky = double.Parse(substrings[3]);
                                    kphi = double.Parse(substrings[4]);
                                    material = new Material(feder, kx, ky, kphi)
                                    {
                                        MaterialId = materialId
                                    };
                                    modell.Material.Add(materialId, material);
                                    break;
                                }
                        }
                        i++;
                    }
                    else
                    {
                        throw new ParseAusnahme((i + 2) + ": Material " + materialId);
                    }
                } while (lines[i + 1].Length != 0);
                break;
            }
        }
    }
}