using FE_Berechnungen.Elastizitätsberechnung.Modelldaten;

namespace FE_Berechnungen.Elastizitätsberechnung.ModelldatenLesen;

public class MaterialParser
{
    private double eModul;
    private Material material;
    private string materialId;
    private FeModell modell;
    private string[] substrings;

    public static double GModul { get; set; }
    public static double Poisson { get; set; }

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
                        throw new ParseAusnahme(i + 1 + ":\nMaterial erfordert 3 oder 4 Eingabeparameter");
                }

                material.MaterialId = materialId;
                modell.Material.Add(materialId, material);
                i++;
            } while (lines[i + 1].Length != 0);

            break;
        }
    }
}