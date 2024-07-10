using FEBibliothek.Modell.abstrakte_Klassen;

namespace FE_Berechnungen.Wärmeberechnung.Modelldaten;

public class ElementLast3 : AbstraktElementLast
{
    private int[] systemIndices;

    // ....Constructors...................................................
    public ElementLast3(string elementId, double[] p)
    {
        ElementId = elementId;
        Lastwerte = p;
        Lastwerte[0] = p[0];
        Lastwerte[1] = p[1];
        Lastwerte[2] = p[2];
    }

    public ElementLast3(string id, string elementId, double[] p)
    {
        LastId = id;
        ElementId = elementId;
        Lastwerte = p;
        Lastwerte[0] = p[0];
        Lastwerte[1] = p[1];
        Lastwerte[2] = p[2];
    }

    // ....Compute the element load vector.................................
    public override double[] BerechneLastVektor()
    {
        //Element.ComputeGeometry();
        var area = 0.5 * Element.Determinant;

        const double gaussWeight = 1.0 / 3.0;
        const double gc0 = 2.0 / 3.0;
        const double gc12 = 1.0 / 6.0;
        var vector = new double[3];
        var qp0 = gc0 * Lastwerte[0] + gc12 * Lastwerte[1] + gc12 * Lastwerte[2];
        var qp1 = gc12 * Lastwerte[0] + gc0 * Lastwerte[1] + gc12 * Lastwerte[2];
        var qp2 = gc12 * Lastwerte[0] + gc12 * Lastwerte[1] + gc0 * Lastwerte[2];
        vector[0] = (gc0 * qp0 + gc12 * qp1 + gc12 * qp2) * gaussWeight * area;
        vector[1] = (gc12 * qp0 + gc0 * qp1 + gc12 * qp2) * gaussWeight * area;
        vector[2] = (gc12 * qp0 + gc12 * qp1 + gc0 * qp2) * gaussWeight * area;
        return vector;
    }

    // ....Compute the element system indices .................................
    public int[] ComputeSystemIndices()
    {
        systemIndices = Element.SystemIndizesElement;
        return systemIndices;
    }
}