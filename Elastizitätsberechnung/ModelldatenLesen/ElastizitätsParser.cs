using FEBibliothek.Modell;

namespace FE_Berechnungen.Elastizitätsberechnung.ModelldatenLesen;

public class ElastizitätsParser : FeParser
{
    public static RandbedingungenParser ParseElastizitätsRandbedingungen;
    private FeModell modell;
    private ElementParser parseElastizitätsElemente;
    private LastParser parseElastizitätsLasten;
    private MaterialParser parseElastizitätsMaterial;

    // Eingabedaten für eine Elastizitätsberechnung aus Detei lesen
    public void ParseElastizität(string[] lines, FeModell feModell)
    {
        modell = feModell;
        parseElastizitätsElemente = new ElementParser();
        parseElastizitätsElemente.ParseElements(lines, modell);

        parseElastizitätsMaterial = new MaterialParser();
        parseElastizitätsMaterial.ParseMaterials(lines, modell);

        parseElastizitätsLasten = new LastParser();
        parseElastizitätsLasten.ParseLasten(lines, modell);

        ParseElastizitätsRandbedingungen = new RandbedingungenParser();
        ParseElastizitätsRandbedingungen.ParseRandbedingungen(lines, modell);
    }
}