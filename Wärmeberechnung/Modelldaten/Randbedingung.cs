using FEBibliothek.Modell.abstrakte_Klassen;

namespace FE_Berechnungen.Wärmeberechnung.Modelldaten;

public class Randbedingung : AbstraktRandbedingung
{
    // ....Constructor....................................................
    public Randbedingung(string randbedingungId, string knotenId, double pre)
    {
        RandbedingungId = randbedingungId;
        KnotenId = knotenId;
        Vordefiniert = new double[1];
        Vordefiniert[0] = pre;
        Festgehalten = new bool[1];
        Festgehalten[0] = true;
    }
}