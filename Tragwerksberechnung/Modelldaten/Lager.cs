using FEBibliothek.Modell;
using FEBibliothek.Modell.abstrakte_Klassen;
using System.Collections.Generic;

namespace FE_Berechnungen.Tragwerksberechnung.Modelldaten;

public class Lager : AbstraktRandbedingung
{
    public const int XFixed = 1,
        Yfixed = 2,
        Rfixed = 4,
        XYfixed = 3,
        XRfixed = 5,
        YRfixed = 6,
        XYRfixed = 7;

    public Lager(string knotenId, int lagerTyp, IReadOnlyList<double> pre, FeModell modell)
    {
        Typ = lagerTyp;
        if (!modell.Knoten.TryGetValue(knotenId, out _))
            throw new ModellAusnahme("\nLagerknoten " + knotenId + " nicht definiert");

        Vordefiniert = new double[pre.Count];
        Festgehalten = new bool[pre.Count];
        for (var i = 0; i < pre.Count; i++) Festgehalten[i] = false;
        KnotenId = knotenId;

        switch (lagerTyp)
        {
            case XFixed:
                Vordefiniert[0] = pre[0];
                Festgehalten[0] = true;
                break;
            case Yfixed:
                Vordefiniert[1] = pre[1];
                Festgehalten[1] = true;
                break;
            case Rfixed:
                Vordefiniert[2] = pre[2];
                Festgehalten[2] = true;
                break;
            case XYfixed:
                Vordefiniert[0] = pre[0];
                Festgehalten[0] = true;
                Vordefiniert[1] = pre[1];
                Festgehalten[1] = true;
                break;
            case XRfixed:
                Vordefiniert[0] = pre[0];
                Festgehalten[0] = true;
                Vordefiniert[2] = pre[2];
                Festgehalten[2] = true;
                break;
            case YRfixed:
                Vordefiniert[1] = pre[1];
                Festgehalten[1] = true;
                Vordefiniert[2] = pre[2];
                Festgehalten[2] = true;
                break;
            case XYRfixed:
                Vordefiniert[0] = pre[0];
                Festgehalten[0] = true;
                Vordefiniert[1] = pre[1];
                Festgehalten[1] = true;
                Vordefiniert[2] = pre[2];
                Festgehalten[2] = true;
                break;
        }
    }
}