using FEBibliothek.Modell.abstrakte_Klassen;

namespace FE_Berechnungen.Elastizitätsberechnung.Modelldaten;

public class Material : AbstraktMaterial
{
    public Material(double emodul, double poisson)
    {
        MaterialWerte = new double[2];
        MaterialWerte[0] = emodul;
        MaterialWerte[1] = poisson;
    }

    public Material(double emodul, double poisson, double masse)
    {
        MaterialWerte = new double[3];
        MaterialWerte[0] = emodul;
        MaterialWerte[1] = poisson;
        MaterialWerte[2] = masse;
    }
}