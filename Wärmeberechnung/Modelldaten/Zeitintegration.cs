using System.Collections.Generic;
using FEBibliothek.Modell.abstrakte_Klassen;

namespace FE_Berechnungen.Wärmeberechnung.Modelldaten;

public class Zeitintegration : AbstraktZeitintegration
{
    //private double[] initial;
    //private double[][] forceFunction;
    //public double[] Initial { get { return initial; } set { initial = value; } }
    //public double[][] ForceFunction { get { return forceFunction; } set { forceFunction = value; } }

    public Zeitintegration()
    {
    }

    public Zeitintegration(double tmax, double dt, double alfa)
    {
        Tmax = tmax;
        Dt = dt;
        Parameter1 = alfa;
        Anfangsbedingungen = new List<object>();
    }
}