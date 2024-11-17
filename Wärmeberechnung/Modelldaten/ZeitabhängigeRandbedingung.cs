namespace FE_Berechnungen.Wärmeberechnung.Modelldaten;

public class ZeitabhängigeRandbedingung : AbstraktZeitabhängigeRandbedingung
{
    public ZeitabhängigeRandbedingung(string knotenId, bool datei)
    {
        KnotenId = knotenId;
        Festgehalten = new bool[1];
        Festgehalten[0] = true;
        Vordefiniert = new double[1];
        Datei = datei;
        VariationsTyp = 0;
    }

    public ZeitabhängigeRandbedingung(string knotenId, double konstanteTemperatur)
    {
        KnotenId = knotenId;
        Festgehalten = new bool[1];
        Festgehalten[0] = true;
        Vordefiniert = new double[1];
        KonstanteTemperatur = konstanteTemperatur;
        VariationsTyp = 3;
    }

    public ZeitabhängigeRandbedingung(string knotenId, double[] intervall)
    {
        KnotenId = knotenId;
        Festgehalten = new bool[1];
        Festgehalten[0] = true;
        Vordefiniert = new double[1];
        Intervall = intervall;
        VariationsTyp = 1;
    }

    public ZeitabhängigeRandbedingung(string knotenId,
        double amplitude, double frequenz, double phasenWinkel)
    {
        KnotenId = knotenId;
        Festgehalten = new bool[1];
        Vordefiniert = new double[1];
        Festgehalten[0] = true;
        Amplitude = amplitude;
        Frequenz = frequenz;
        PhasenWinkel = phasenWinkel;
        VariationsTyp = 2;
    }
}