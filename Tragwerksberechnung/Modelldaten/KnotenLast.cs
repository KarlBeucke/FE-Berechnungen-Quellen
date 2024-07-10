using FEBibliothek.Modell.abstrakte_Klassen;

namespace FE_Berechnungen.Tragwerksberechnung.Modelldaten;

public class KnotenLast : AbstraktKnotenlast
{
    // ... Constructor ........................................................
    public KnotenLast(string knotenId, double[] p)
    {
        KnotenId = knotenId;
        Lastwerte = p;
    }

    public KnotenLast(string knotenId, double px, double py, double moment)
    {
        KnotenId = knotenId;
        Lastwerte = new double[3];
        Lastwerte[0] = px;
        Lastwerte[1] = py;
        Lastwerte[2] = moment;
    }

    public KnotenLast(string knotenId, double px, double py)
    {
        KnotenId = knotenId;
        Lastwerte = new double[2];
        Lastwerte[0] = px;
        Lastwerte[1] = py;
    }

    public override double[] BerechneLastVektor()
    {
        return Lastwerte;
    }
}