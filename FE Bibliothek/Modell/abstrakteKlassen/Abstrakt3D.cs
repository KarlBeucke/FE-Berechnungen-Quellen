﻿using System.Windows.Media.Media3D;

namespace FEBibliothek.Modell.abstrakteKlassen
{
    public abstract class Abstrakt3D : AbstraktElement
    {
        public abstract double[] BerechneElementZustand(double z0, double z1, double z2);
        public abstract Point3D BerechneSchwerpunkt3D();
    }
}
