namespace FE_Berechnungen.Elastizitätsberechnung.Modelldaten;

public class Material : AbstraktMaterial
{
    public Material(double eModul)
    {
        MaterialWerte = new double[2];
        MaterialWerte[0] = eModul;
    }
    public Material(double eModul, double poisson)
    {
        MaterialWerte = new double[2];
        MaterialWerte[0] = eModul;
        MaterialWerte[1] = poisson;
    }

    public Material(double eModul, double poisson, double schubModul)
    {
        MaterialWerte = new double[3];
        MaterialWerte[0] = eModul;
        MaterialWerte[1] = poisson;
        MaterialWerte[2] = schubModul;
    }
}