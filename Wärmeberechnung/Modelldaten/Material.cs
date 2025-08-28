namespace FE_Berechnungen.Wärmeberechnung.Modelldaten;

public class Material : AbstraktMaterial
{
    public Material(string id, IReadOnlyList<double> conduct)
    {
        MaterialId = id;
        MaterialWerte = new double[conduct.Count];
        for (var i = 0; i < conduct.Count; i++) MaterialWerte[i] = conduct[i];
    }

    public Material(string id, IReadOnlyList<double> conduct, double rho)
    {
        MaterialId = id;
        MaterialWerte = new double[4];
        for (var i = 0; i < conduct.Count; i++) MaterialWerte[i] = conduct[i];
        MaterialWerte[3] = rho;
    }
}