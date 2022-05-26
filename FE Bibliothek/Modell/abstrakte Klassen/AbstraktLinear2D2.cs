using System;
using System.Windows;

namespace FEBibliothek.Modell.abstrakte_Klassen
{
    public abstract class AbstraktLinear2D2 : Abstrakt2D
    {
        public double balkenLänge;
        protected double sin, cos;
        protected double[,] rotationsMatrix = new double[2, 2];
        private double[] Sx { get; set; } = new double[4];

        //public double ComputeLength()
        //{
        //    var delx = Nodes[1].Coordinates[0] - Nodes[0].Coordinates[0];
        //    var dely = Nodes[1].Coordinates[1] - Nodes[0].Coordinates[1];
        //    return length = Math.Sqrt(delx * delx + dely * dely);
        //}

        protected void BerechneGeometrie()
        {
            var delx = Knoten[1].Koordinaten[0] - Knoten[0].Koordinaten[0];
            var dely = Knoten[1].Koordinaten[1] - Knoten[0].Koordinaten[1];
            //var angle = Math.Atan2(dely, delx);
            balkenLänge = Math.Sqrt(delx * delx + dely * dely);
            sin = dely / balkenLänge;
            cos = delx / balkenLänge;
            rotationsMatrix[0, 0] = cos; rotationsMatrix[1, 0] = sin;
            rotationsMatrix[0, 1] = -sin; rotationsMatrix[1, 1] = cos;
        }

        protected double[] BerechneSx()
        {
            Sx[0] = -cos; Sx[1] = -sin; Sx[2] = cos; Sx[3] = sin;
            return Sx;
        }
        public override void SetzElementSystemIndizes()
        {
            SystemIndizesElement = new int[KnotenProElement * ElementFreiheitsgrade];
            var counter = 0;
            for (var i = 0; i < KnotenProElement; i++)
            {
                for (var j = 0; j < ElementFreiheitsgrade; j++)
                    SystemIndizesElement[counter++] = Knoten[i].SystemIndizes[j];
            }
        }

        protected static Point Schwerpunkt(AbstraktElement element)
        {
            var cg = new Point();
            var nodes = element.Knoten;

            cg.X = nodes[0].Koordinaten[0];
            cg.Y = nodes[0].Koordinaten[1];

            cg.X += 0.5 * (nodes[1].Koordinaten[0] - cg.X);
            cg.Y += 0.5 * (nodes[1].Koordinaten[1] - cg.Y);

            return cg;
        }
    }
}
