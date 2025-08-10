namespace FE_Berechnungen.Elastizitätsberechnung.Modelldaten;

public class Lager : AbstraktRandbedingung
{
    public const int XFixed = 1, YFixed = 2, ZFixed = 4;
    private const int XyFixed = 3, XzFixed = 5, YzFixed = 6, XyzFixed = 7;

    protected double[] Deflection;

    public Lager(string knotenId, int supportTyp, double[] pre, int ndof)
    {
        Vordefiniert = pre;
        Typ = supportTyp;
        Festgehalten = new bool[ndof];
        for (var i = 0; i < ndof; i++) Festgehalten[i] = false;
        KnotenId = knotenId;
        SupportTyp(Typ, Vordefiniert, Festgehalten);
    }
    public Lager(string knotenId, string face, int supportTyp, double[] pre, int ndof)
    {
        Vordefiniert = pre;
        Typ = supportTyp;
        Festgehalten = new bool[ndof];
        for (var i = 0; i < ndof; i++) Festgehalten[i] = false;
        KnotenId = knotenId;
        Face = face;
        SupportTyp(Typ, Vordefiniert, Festgehalten);
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