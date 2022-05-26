using FEBibliothek.Modell.abstrakte_Klassen;

namespace FE_Berechnungen.Tragwerksberechnung.Modelldaten
{
    public class Material : AbstraktMaterial
    {
        public string Feder { get; }

        public Material(double emodulus, double poisson, double mass)
        {
            MaterialWerte = new double[3];
            MaterialWerte[0] = emodulus;
            MaterialWerte[1] = poisson;
            MaterialWerte[2] = mass;
        }
        public Material(double emodulus, double poisson)
        {
            MaterialWerte = new double[2];
            MaterialWerte[0] = emodulus;
            MaterialWerte[1] = poisson;
        }
        public Material(double emodulus)
        {
            MaterialWerte = new double[1];
            MaterialWerte[0] = emodulus;
        }
        public Material(string feder, double fkx, double fky, double fkphi)
        {
            Feder = feder;
            MaterialWerte = new double[3];
            MaterialWerte[0] = fkx;
            MaterialWerte[1] = fky;
            MaterialWerte[2] = fkphi;
        }
    }
}
