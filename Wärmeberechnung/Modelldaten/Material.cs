using System.Collections.Generic;
using FEBibliothek.Modell.abstrakte_Klassen;

namespace FE_Berechnungen.Wärmeberechnung.Modelldaten;

public class Material : AbstraktMaterial
{
    public Material(string id, IReadOnlyList<double> conduct)
    {
        MaterialId = id;
        MaterialWerte = new double[conduct.Count];
        for (var i = 0; i < conduct.Count; i++) MaterialWerte[i] = conduct[i];
    }

    public Material(string id, IReadOnlyList<double> conduct, double rhoC)
    {
        MaterialId = id;
        MaterialWerte = new double[4];
        for (var i = 0; i < conduct.Count; i++) MaterialWerte[i] = conduct[i];
        MaterialWerte[3] = rhoC;
    }
}