using FEBibliothek.Modell;
using FEBibliothek.Modell.abstrakte_Klassen;
using System.Collections.Generic;

namespace FE_Berechnungen.Tragwerksberechnung.Modelldaten;

public class Lager : AbstraktRandbedingung
{
    public const int XFixed = 1,
        YFixed = 2,
        RFixed = 4,
        XyFixed = 3,
        XrFixed = 5,
        YrFixed = 6,
        XyrFixed = 7;

    public Lager(string knotenId, int lagerTyp, IReadOnlyList<double> pre, FeModell modell)
    {
        Typ = lagerTyp;
        if (modell.Knoten.TryGetValue(knotenId, out _))
        {
        }
        else
        {
            throw new ModellAusnahme("\nLagerknoten " + knotenId + " nicht definiert");
        }

        Vordefiniert = new double[pre.Count];
        Festgehalten = new bool[pre.Count];
        for (var i = 0; i < pre.Count; i++) Festgehalten[i] = false;
        KnotenId = knotenId;

        if (lagerTyp == XFixed)
        {
            Vordefiniert[0] = pre[0];
            Festgehalten[0] = true;
        }

        if (lagerTyp == YFixed)
        {
            Vordefiniert[1] = pre[1];
            Festgehalten[1] = true;
        }

        if (lagerTyp == RFixed)
        {
            Vordefiniert[2] = pre[2];
            Festgehalten[2] = true;
        }

        if (lagerTyp == XyFixed)
        {
            Vordefiniert[0] = pre[0];
            Festgehalten[0] = true;
            Vordefiniert[1] = pre[1];
            Festgehalten[1] = true;
        }

        if (lagerTyp == XrFixed)
        {
            Vordefiniert[0] = pre[0];
            Festgehalten[0] = true;
            Vordefiniert[2] = pre[2];
            Festgehalten[2] = true;
        }

        if (lagerTyp == YrFixed)
        {
            Vordefiniert[1] = pre[1];
            Festgehalten[1] = true;
            Vordefiniert[2] = pre[2];
            Festgehalten[2] = true;
        }

        if (lagerTyp == XyrFixed)
        {
            Vordefiniert[0] = pre[0];
            Festgehalten[0] = true;
            Vordefiniert[1] = pre[1];
            Festgehalten[1] = true;
            Vordefiniert[2] = pre[2];
            Festgehalten[2] = true;
        }
    }
}