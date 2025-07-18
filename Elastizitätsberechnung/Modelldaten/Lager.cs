namespace FE_Berechnungen.Elastizitätsberechnung.Modelldaten;

public class Lager : AbstraktRandbedingung
{
    public const int XFixed = 1, YFixed = 2, ZFixed = 4;
    private const int XyFixed = 3, XzFixed = 5, YzFixed = 6, XyzFixed = 7;

    protected double[] Deflection;

    private string _face;

    public Lager(string knotenId, int supportTyp, double[] pre, FeModell modell)
    {
        int ndof;
        Vordefiniert = pre;

        if (modell.Knoten.TryGetValue(knotenId, out var node))
            ndof = node.AnzahlKnotenfreiheitsgrade;
        else
            throw new ModellAusnahme("\nLagerknoten nicht definiert");
        Typ = supportTyp;
        Festgehalten = new bool[ndof];
        for (var i = 0; i < ndof; i++) Festgehalten[i] = false;
        KnotenId = knotenId;
        SupportTyp(Typ, Vordefiniert, Festgehalten);
    }
    public Lager(string knotenId, string face, int supportTyp, double[] pre, FeModell modell)
    {
        _face = face;
        int ndof;

        //switch (supportType)
        //{
        //    case 1:
        //        nodeId = "N00" + nodeId.Substring(3, 4);
        //        break;
        //    case 2:
        //        nodeId = nodeId.Substring(0, 3) + "00" + nodeId.Substring(5,2);
        //        break;
        //    case 3:
        //        nodeId = nodeId.Substring(0, 3) + nodeId.Substring(5, 2) + "00";
        //        break;
        //}
        if (modell.Knoten.TryGetValue(knotenId, out var node))
            ndof = node.AnzahlKnotenfreiheitsgrade;
        else
            throw new ModellAusnahme("\nLagerknoten nicht definiert");
        Typ = supportTyp;
        Vordefiniert = new double[ndof];
        var fest = new bool[ndof];
        for (var i = 0; i < ndof; i++) fest[i] = false;
        KnotenId = knotenId;
        SupportTyp(Typ, pre, fest);

    }

    private void SupportTyp(int supportTyp, double[] pre, bool[] fest)
    {
        switch (supportTyp)
        {
            case XFixed:
                Vordefiniert[0] = pre[0];
                fest[0] = true;
                break;
            case YFixed:
                Vordefiniert[1] = pre[1];
                fest[1] = true;
                break;
            case ZFixed:
                Vordefiniert[2] = pre[2];
                fest[2] = true;
                break;
            case XyFixed:
                Vordefiniert[0] = pre[0];
                fest[0] = true;
                Vordefiniert[1] = pre[1];
                fest[1] = true;
                break;
            case XzFixed:
                Vordefiniert[0] = pre[0];
                fest[0] = true;
                Vordefiniert[2] = pre[2];
                fest[2] = true;
                break;
            case YzFixed:
                Vordefiniert[1] = pre[1];
                fest[1] = true;
                Vordefiniert[2] = pre[2];
                fest[2] = true;
                break;
            case XyzFixed:
                Vordefiniert[0] = pre[0];
                fest[0] = true;
                Vordefiniert[1] = pre[1];
                fest[1] = true;
                Vordefiniert[2] = pre[2];
                fest[2] = true;
                break;
        }
    }
}