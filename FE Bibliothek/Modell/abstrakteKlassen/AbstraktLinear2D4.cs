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

        private static double Polygonfläche(Point[] k)
        {
            double fläche = 0;
            var p = new Point[k.Length + 1];
            for (var i = 0; i < k.Length; i++) p[i] = k[i];
            p[k.Length] = p[0];
            for (var i = 0; i < p.Length - 1; i++)
            {
                fläche += p[i].X * p[i + 1].Y - p[i + 1].X * p[i].Y;
            }
            return 0.5 * fläche;
        }

        public static Point PolygonSchwerpunkt(Point[] k)
        {
            double xs = 0, ys = 0;
            var fläche = Polygonfläche(k);
            var p = new Point[k.Length + 1];
            for (var i = 0; i < k.Length; i++) p[i] = k[i];
            p[k.Length] = p[0];
            for (var i = 0; i < p.Length - 1; i++)
            {
                xs += (p[i].X + p[i + 1].X) * (p[i].X * p[i + 1].Y - p[i + 1].X * p[i].Y);
                ys += (p[i].Y + p[i + 1].Y) * (p[i].X * p[i + 1].Y - p[i + 1].X * p[i].Y);
            }
            var cg = new Point(xs / 6 / fläche, ys / 6 / fläche);
            return cg;
        }
    }
}
