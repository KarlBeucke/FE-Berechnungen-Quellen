namespace FE_Berechnungen.Tragwerksberechnung.Modelldaten;

public class LinienLast : AbstraktLinienlast
{
    // ... Constructors ........................................................
    public LinienLast(string elementId, double p1X, double p2X, double p1Y, double p2Y)
    {
        ElementId = elementId;
        Lastwerte = new double[4]; // 2 nodes, 2 dimensions
        Lastwerte[0] = p1X;
        Lastwerte[1] = p2X;
        Lastwerte[2] = p1Y;
        Lastwerte[3] = p2Y;
    }

    public LinienLast(string elementId, double p1X, double p2X, double p1Y, double p2Y, bool inElementKoordinatenSystem)
    {
        ElementId = elementId;
        Lastwerte = new double[4]; // 2 nodes, 2 dimensions
        Lastwerte[0] = p1X;
        Lastwerte[1] = p2X;
        Lastwerte[2] = p1Y;
        Lastwerte[3] = p2Y;
        InElementKoordinatenSystem = inElementKoordinatenSystem;
    }

    public override double[] BerechneLastVektor()
    {
        var balken = (Biegebalken)Element;
        // inElementCoordinateSystem is false
        return balken.BerechneLastVektor(this, false);
    }

    public double[] BerechneLokalenLastVektor()
    {
        var balken = (Biegebalken)Element;
        // inElementCoordinateSystem is true
        return balken.BerechneLastVektor(this, true);
    }

    // useful for GAUSS integration
    public double GetXIntensity(double z)
    {
        if (z < 0 || z > 1)
            throw new ModellAusnahme("\nLinienLast auf element:" + ElementId + "ausserhalb Koordinaten 0 <= z <= 1");
        return Lastwerte[0] * (1 - z) + Lastwerte[2] * z;
    }

    public double GetYIntensity(double z)
    {
        if (z < 0 || z > 1)
            throw new ModellAusnahme("\nLinienLast auf element:" + ElementId + "ausserhalb Koordinaten 0 <= z <= 1");
        return Lastwerte[1] * (1 - z) + Lastwerte[3] * z;
    }
}