using FE_Berechnungen.Wärmeberechnung.Modelldaten;
using FEBibliothek.Modell;

namespace FE_Berechnungen.Wärmeberechnung.ModelldatenLesen
{
    public class MaterialParser
    {
        private FeModell modell;
        private string[] substrings;
        private string materialId;
        private Material material;
        private double[] leitfähigkeit;
        private double dichteLeitfähigkeit;

        public void ParseMaterials(string[] lines, FeModell feModell)
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
                    leitfähigkeit = new double[4];
                    switch (substrings.Length)
                    {
                        case 2:
                            //leitfähigkeit = new double[1];
                            leitfähigkeit[0] = double.Parse(substrings[1]);
                            material = new Material(materialId, leitfähigkeit);
                            break;
                        case 3:
                            //leitfähigkeit = new double[1];
                            leitfähigkeit[0] = double.Parse(substrings[1]);
                            dichteLeitfähigkeit = double.Parse(substrings[2]);
                            material = new Material(materialId, leitfähigkeit, dichteLeitfähigkeit);
                            break;
                        case 4:
                            //leitfähigkeit = new double[3];
                            leitfähigkeit[0] = double.Parse(substrings[1]);
                            leitfähigkeit[1] = double.Parse(substrings[2]);
                            leitfähigkeit[2] = double.Parse(substrings[3]);
                            material = new Material(materialId, leitfähigkeit);
                            break;
                        case 5:
                            //leitfähigkeit = new double[4];
                            leitfähigkeit[0] = double.Parse(substrings[1]);
                            leitfähigkeit[1] = double.Parse(substrings[2]);
                            leitfähigkeit[2] = double.Parse(substrings[3]);
                            dichteLeitfähigkeit = double.Parse(substrings[4]);
                            material = new Material(materialId, leitfähigkeit, dichteLeitfähigkeit);
                            break;
                        default:
                            throw new ParseAusnahme((i + 2) + ": Material, falsche Anzahl Parameter");
                    }
                    modell.Material.Add(materialId, material);
                    i++;
                } while (lines[i + 1].Length != 0);
                break;
            }
        }
    }
}
