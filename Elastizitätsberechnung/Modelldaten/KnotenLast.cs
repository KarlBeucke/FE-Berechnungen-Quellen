using FEBibliothek.Modell.abstrakte_Klassen;
using System;

namespace FE_Berechnungen.Elastizitätsberechnung.Modelldaten;

public class KnotenLast : AbstraktLast
{
    // ... Constructor ........................................................
    public KnotenLast(String knotenId, double px, double py)
    {
        KnotenId = knotenId;
        Lastwerte = new double[2];
        Lastwerte[0] = px;
        Lastwerte[1] = py;
    }
    public KnotenLast(String knotenId, double px, double py, double pz)
    {
        KnotenId = knotenId;
        Lastwerte = new double[3];
        Lastwerte[0] = px;
        Lastwerte[1] = py;
        Lastwerte[2] = pz;
    }
    public override double[] BerechneLastVektor()
    {
        return Lastwerte;
    }
}