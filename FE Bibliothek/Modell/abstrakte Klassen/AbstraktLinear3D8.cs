using FEBibliothek.Werkzeuge;
using System;
using System.Windows.Media.Media3D;

namespace FEBibliothek.Modell.abstrakte_Klassen
{
    public abstract class AbstraktLinear3D8 : Abstrakt3D
    {
        private readonly double[,] xz = new double[3, 3];
        private readonly double[,] sz = new double[8, 3];
        protected double[,] Sx { get; set; } = new double[8, 3];

        protected void BerechneGeometrie(double z0, double z1, double z2)
        {
            sz[0, 0] = -(1 - z1) * (1 - z2); sz[0, 1] = -(1 - z0) * (1 - z2); sz[0, 2] = -(1 - z0) * (1 - z1);
            sz[1, 0] = (1 - z1) * (1 - z2); sz[1, 1] = -(1 + z0) * (1 - z2); sz[1, 2] = -(1 + z0) * (1 - z1);
            sz[2, 0] = (1 + z1) * (1 - z2); sz[2, 1] = (1 + z0) * (1 - z2); sz[2, 2] = -(1 + z0) * (1 + z1);
            sz[3, 0] = -(1 + z1) * (1 - z2); sz[3, 1] = (1 - z0) * (1 - z2); sz[3, 2] = -(1 - z0) * (1 + z1);
            sz[4, 0] = -(1 - z1) * (1 + z2); sz[4, 1] = -(1 - z0) * (1 + z2); sz[4, 2] = (1 - z0) * (1 - z1);
            sz[5, 0] = (1 - z1) * (1 + z2); sz[5, 1] = -(1 + z0) * (1 + z2); sz[5, 2] = (1 + z0) * (1 - z1);
            sz[6, 0] = (1 + z1) * (1 + z2); sz[6, 1] = (1 + z0) * (1 + z2); sz[6, 2] = (1 + z0) * (1 + z1);
            sz[7, 0] = -(1 + z1) * (1 + z2); sz[7, 1] = (1 - z0) * (1 + z2); sz[7, 2] = (1 - z0) * (1 + z1);
            for (var i = 0; i < 8; i++)
                for (var k = 0; k < 3; k++) sz[i, k] = 0.125 * sz[i, k];
            var n = new double[8];
            for (var i = 0; i < 3; i++)
            {
                for (var k = 0; k < 8; k++) n[k] = Knoten[k].Koordinaten[i];
                for (var j = 0; j < 3; j++)
                {
                    double temp = 0;
                    for (var k = 0; k < 8; k++) temp += n[k] * sz[k, j];
                    xz[i, j] = temp;
                }
            }
            var coor = new double[3, 8];
            for (var i = 0; i < 3; i++)
                for (var k = 0; k < 8; k++) coor[i, k] = Knoten[k].Koordinaten[i];
            Determinant = xz[0, 0] * (xz[1, 1] * xz[2, 2] - xz[1, 2] * xz[2, 1])
                         - xz[0, 1] * (xz[1, 0] * xz[2, 2] - xz[1, 2] * xz[2, 0])
                         + xz[0, 2] * (xz[1, 0] * xz[2, 1] - xz[1, 1] * xz[2, 0]);

            if (Math.Abs(Determinant) < double.Epsilon)
                throw new BerechnungAusnahme("Fläche = 0 in Element " + ElementId);
            if (Determinant < 0)
                throw new ModellAusnahme("negative Fläche in Element " + ElementId);
        }

        protected double[,] BerechneSx(double z0, double z1, double z2)
        {
            var zx = new double[3, 3];
            zx[0, 0] = (xz[1, 1] * xz[2, 2] - xz[1, 2] * xz[2, 1]) / Determinant;
            zx[1, 0] = -(xz[1, 0] * xz[2, 2] - xz[1, 2] * xz[2, 0]) / Determinant;
            zx[2, 0] = (xz[1, 0] * xz[2, 1] - xz[1, 1] * xz[2, 0]) / Determinant;

            zx[0, 1] = -(xz[0, 1] * xz[2, 2] - xz[0, 2] * xz[2, 1]) / Determinant;
            zx[1, 1] = (xz[0, 0] * xz[2, 2] - xz[0, 2] * xz[2, 0]) / Determinant;
            zx[2, 1] = -(xz[0, 0] * xz[2, 1] - xz[0, 1] * xz[2, 0]) / Determinant;

            zx[0, 2] = (xz[0, 1] * xz[1, 2] - xz[0, 2] * xz[1, 1]) / Determinant;
            zx[1, 2] = -(xz[0, 0] * xz[1, 2] - xz[0, 2] * xz[1, 0]) / Determinant;
            zx[2, 2] = (xz[0, 0] * xz[1, 1] - xz[0, 1] * xz[1, 0]) / Determinant;
            Sx = MatrizenAlgebra.Mult(sz, zx);
            return Sx;
        }
        protected static Point3D Schwerpunkt(AbstraktElement element)
        {
            var cg = new Point3D();
            var nodes = element.Knoten;
            cg.X = 0;
            cg.Y = 0;
            for (var i = 0; i < element.Knoten.Length; i++)
            {
                cg.X += nodes[i].Koordinaten[0];
                cg.Y += nodes[i].Koordinaten[1];
                cg.Z += nodes[i].Koordinaten[2];
            }
            cg.X /= 8.0;
            cg.Y /= 8.0;
            cg.Z /= 8.0;
            return cg;
        }
    }
}
