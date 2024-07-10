using System;
using FEBibliothek.Modell.abstrakte_Klassen;

namespace FE_Berechnungen.Wärmeberechnung.Modelldaten;

public class LinienLast : AbstraktLinienlast
{
    public LinienLast(string startKnotenId, string endKnotenId, double[] p)
    {
        StartKnotenId = startKnotenId;
        EndKnotenId = endKnotenId;
        Lastwerte = p;
    }

    public LinienLast(string id, string startKnotenId, string endKnotenId, double[] p)
    {
        LastId = id;
        StartKnotenId = startKnotenId;
        EndKnotenId = endKnotenId;
        Lastwerte = p;
    }

    // ....Compute concentrated node forces in local coordinate system....
    public override double[] BerechneLastVektor()
    {
        //Lastwerte = new double[2];
        var nStart = StartKnoten.Koordinaten;
        var nEnd = EndKnoten.Koordinaten;
        var vector = new double[2];
        var c1 = nEnd[0] - nStart[0];
        var c2 = nEnd[1] - nStart[1];
        var l = Math.Sqrt(c1 * c1 + c2 * c2) / 6.0;
        vector[0] = l * (2.0 * Lastwerte[0] + Lastwerte[1]);
        vector[1] = l * (2.0 * Lastwerte[1] + Lastwerte[0]);
        return vector;
    }
}