using FEBibliothek.Modell.abstrakte_Klassen;

namespace FE_Berechnungen.Tragwerksberechnung.Modelldaten;

public class PunktLast : AbstraktElementLast
{
    // constructor for point load .....
    public PunktLast(string elementId, double fx, double fy, double o)
    {
        ElementId = elementId;
        Lastwerte = new double[2];
        Lastwerte[0] = fx;
        Lastwerte[1] = fy;
        Offset = o;
    }

    // --- get global load vector ---------------------------------------------
    public override double[] BerechneLastVektor()
    {
        switch (Element)
        {
            case Biegebalken biegebalken:
                biegebalken = (Biegebalken)Element;
                return biegebalken.BerechneLastVektor(this, false);
            case BiegebalkenGelenk biegebalkenGelenk:
                biegebalkenGelenk = (BiegebalkenGelenk)Element;
                return biegebalkenGelenk.BiegebalkenGelenkLastVektor(this, false);
            default:
                return null;
        }
    }

    // get load vector
    public double[] BerechneLokalenLastVektor()
    {
        switch (Element)
        {
            case Biegebalken biegebalken:
                biegebalken = (Biegebalken)Element;
                return biegebalken.BerechneLastVektor(this, true);
            case BiegebalkenGelenk biegebalkenGelenk:
                biegebalkenGelenk = (BiegebalkenGelenk)Element;
                return biegebalkenGelenk.BiegebalkenGelenkLastVektor(this, true);
            default:
                return null;
        }
    }
}