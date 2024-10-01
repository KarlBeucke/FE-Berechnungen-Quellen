namespace FEBibliothek.Modell.abstrakteKlassen
{
    public abstract class AbstraktLinear2D4 : Abstrakt2D
    {
        private readonly double[,] _xz = new double[2, 2];
        protected double[,] Sx { get; set; } = new double[4, 2];

        public void BerechneGeometrie(double z0, double z1)
        {
            _xz[0, 0] = 0.25 * (-Knoten[0].Koordinaten[0] * (1 - z1)
                            + Knoten[1].Koordinaten[0] * (1 - z1)
                            + Knoten[2].Koordinaten[0] * (1 + z1)
                            - Knoten[3].Koordinaten[0] * (1 + z1));
            _xz[0, 1] = 0.25 * (-Knoten[0].Koordinaten[0] * (1 - z0)
                            - Knoten[1].Koordinaten[0] * (1 + z0)
                            + Knoten[2].Koordinaten[0] * (1 + z0)
                            + Knoten[3].Koordinaten[0] * (1 - z0));
            _xz[1, 0] = 0.25 * (-Knoten[0].Koordinaten[1] * (1 - z1)
                            + Knoten[1].Koordinaten[1] * (1 - z1)
                            + Knoten[2].Koordinaten[1] * (1 + z1)
                            - Knoten[3].Koordinaten[1] * (1 + z1));
            _xz[1, 1] = 0.25 * (-Knoten[0].Koordinaten[1] * (1 - z0)
                            - Knoten[1].Koordinaten[1] * (1 + z0)
                            + Knoten[2].Koordinaten[1] * (1 + z0)
                            + Knoten[3].Koordinaten[1] * (1 - z0));
            Determinant = _xz[0, 0] * _xz[1, 1] - _xz[0, 1] * _xz[1, 0];

            if (Math.Abs(Determinant) < double.Epsilon)
                throw new BerechnungAusnahme("\nFläche = 0 in Element " + ElementId);
            if (Determinant < 0)
                throw new BerechnungAusnahme("\nnegative Fläche in Element " + ElementId);
        }

        protected double[,] BerechneSx(double z0, double z1)
        {
            double fac = 0.25 / Determinant;
            Sx[0, 0] = fac * (-_xz[1, 1] * (1 - z1) + _xz[1, 0] * (1 - z0));
            Sx[1, 0] = fac * (_xz[1, 1] * (1 - z1) + _xz[1, 0] * (1 + z0));
            Sx[2, 0] = fac * (_xz[1, 1] * (1 + z1) - _xz[1, 0] * (1 + z0));
            Sx[3, 0] = fac * (-_xz[1, 1] * (1 + z1) - _xz[1, 0] * (1 - z0));
            Sx[0, 1] = fac * (_xz[0, 1] * (1 - z1) - _xz[0, 0] * (1 - z0));
            Sx[1, 1] = fac * (-_xz[0, 1] * (1 - z1) - _xz[0, 0] * (1 + z0));
            Sx[2, 1] = fac * (-_xz[0, 1] * (1 + z1) + _xz[0, 0] * (1 + z0));
            Sx[3, 1] = fac * (_xz[0, 1] * (1 + z1) + _xz[0, 0] * (1 - z0));
            return Sx;
        }

        public static double[] BerechneS(double z0, double z1)
        {
            var s = new double[4];
            s[0] = 0.25 * (1 - z0) * (1 - z1);
            s[1] = 0.25 * (1 + z0) * (1 - z1);
            s[2] = 0.25 * (1 + z0) * (1 + z1);
            s[3] = 0.25 * (1 - z0) * (1 + z1);
            return s;
        }
        protected static Point Schwerpunkt(AbstraktElement element)
        {
            var cg = new Point();
            var nodes = element.Knoten;
            cg.X = 0;
            for (var i = 0; i < element.Knoten.Length; i++)
            {
                cg.X += nodes[i].Koordinaten[0];
                cg.Y += nodes[i].Koordinaten[1];
            }
            cg.X /= 4.0;
            cg.Y /= 4.0;
            return cg;
        }
    }
}
