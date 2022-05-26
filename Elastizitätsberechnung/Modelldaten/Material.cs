using FEBibliothek.Modell.abstrakte_Klassen;

namespace FE_Berechnungen.Elastizitätsberechnung.Modelldaten
{
    public class Material : AbstraktMaterial
    {
        public Material(double _emodulus, double poisson)
        {
            MaterialWerte = new double[2];
            MaterialWerte[0] = _emodulus;
            MaterialWerte[1] = poisson;
        }
        public Material(double _emodulus, double poisson, double mass)
        {
            MaterialWerte = new double[3];
            MaterialWerte[0] = _emodulus;
            MaterialWerte[1] = poisson;
            MaterialWerte[2] = mass;
        }
    }
}
