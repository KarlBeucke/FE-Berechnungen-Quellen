using System;
using FEBibliothek.Modell.abstrakte_Klassen;

namespace FE_Berechnungen.Tragwerksberechnung.Modelldaten;

public class ZeitabhängigeKnotenLast : AbstraktZeitabhängigeKnotenlast
{
    public ZeitabhängigeKnotenLast(string lastId, string knotenId, int knotenFreiheitsgrad,
        bool datei, bool boden)
    {
        LastId = lastId;
        KnotenId = knotenId;
        KnotenFreiheitsgrad = knotenFreiheitsgrad;
        Datei = datei;
        Bodenanregung = boden;
        VariationsTyp = 0;
    }

    public override double[] BerechneLastVektor()
    {
        throw new NotImplementedException();
    }
}