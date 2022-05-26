using System.Windows.Media.Media3D;

namespace FEBibliothek.Modell.abstrakte_Klassen
{
    public abstract class Abstrakt3D : AbstraktElement
    {
        public abstract double[] ComputeElementState(double z0, double z1, double z2);
        public abstract Point3D ComputeCenterOfGravity3D();
    }
}
