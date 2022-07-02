using FE_Berechnungen.Elastizitätsberechnung.Modelldaten;
using FEBibliothek.Modell;

namespace FE_Berechnungen.Elastizitätsberechnung.ModelldatenLesen
{
    public class MaterialParser
    {
        private FEModell modell;
        private string[] substrings;
        private string materialId;
        private Material material;
        private double eModul;

        public static double GModul { get; set; }
        public static double Poisson { get; set; }

        public void ParseMaterials(string[] lines, FEModell feModell)
        {
            modell = feModell;
            var delimiters = new[] { '\t' };

            for (var i = 0; i < lines.Length; i++)
            {
                if (lines[i] != "Material") continue;
                FeParser.EingabeGefunden += "\nMaterial";
                do
                {
                    substrings = lines[i + 1].Split(delimiters);
                    materialId = substrings[0];
                    switch (substrings.Length)
                    {
                        case 3:
                            eModul = double.Parse(substrings[1]);
                            Poisson = double.Parse(substrings[2]);
                            material = new Material(eModul, Poisson);
                            break;
                        case 4:
                            eModul = double.Parse(substrings[1]);
                            Poisson = double.Parse(substrings[2]);
                            GModul = double.Parse(substrings[3]);
                            material = new Material(eModul, Poisson, GModul);
                            break;
                        default:
                            throw new ParseAusnahme((i + 1) + ": Material erfordert 3 oder 4 Eingabeparameter");
                    }

                    material.MaterialId = materialId;
                    modell.Material.Add(materialId, material);
                    i++;
                } while (lines[i + 1].Length != 0);
                break;
            }
        }
    }
}
