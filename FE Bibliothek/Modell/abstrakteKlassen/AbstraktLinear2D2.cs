namespace FEBibliothek.Modell.abstrakteKlassen
{
    public abstract class AbstraktLinear2D2 : Abstrakt2D
    {
        public double BalkenLänge;
        protected double Sin, Cos;
        protected readonly double[,] RotationsMatrix = new double[2, 2];
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
            BalkenLänge = Math.Sqrt(delx * delx + dely * dely);
            Sin = dely / BalkenLänge;
            Cos = delx / BalkenLänge;
            RotationsMatrix[0, 0] = Cos; RotationsMatrix[1, 0] = Sin;
            RotationsMatrix[0, 1] = -Sin; RotationsMatrix[1, 1] = Cos;
        }

        protected double[] BerechneSx()
        {
            Sx[0] = -Cos; Sx[1] = -Sin; Sx[2] = Cos; Sx[3] = Sin;
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
