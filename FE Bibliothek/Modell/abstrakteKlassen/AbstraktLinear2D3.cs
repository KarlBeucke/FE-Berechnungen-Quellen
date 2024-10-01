namespace FEBibliothek.Modell.abstrakteKlassen
{
    public abstract class AbstraktLinear2D3 : Abstrakt2D
    {
        protected readonly double[,] Xzu = new double[2, 2];   // dx = Xzu * dzu
        protected double[,] Sx { get; set; } = new double[3, 2];

        // ... berechne Geometrie ..........................................
        public void BerechneGeometrie()
        {
            Xzu[0, 0] = Knoten[0].Koordinaten[0] - Knoten[2].Koordinaten[0];
            Xzu[0, 1] = Knoten[1].Koordinaten[0] - Knoten[2].Koordinaten[0];
            Xzu[1, 0] = Knoten[0].Koordinaten[1] - Knoten[2].Koordinaten[1];
            Xzu[1, 1] = Knoten[1].Koordinaten[1] - Knoten[2].Koordinaten[1];
            Determinant = Xzu[0, 0] * Xzu[1, 1] - Xzu[0, 1] * Xzu[1, 0];

            if (Math.Abs(Determinant) < double.Epsilon)
                throw new ModellAusnahme("\nAbstractLinear2D3: *** Fehler!!! *** Fläche = 0 in Element " + ElementId);
            if (Determinant < 0)
                throw new ModellAusnahme("\nnegative Fläche in Element " + ElementId);

            Sx = BerechneSx();
        }

        private double[,] BerechneSx()
        {
            Sx[0, 0] = Xzu[1, 1] / Determinant; Sx[0, 1] = -Xzu[0, 1] / Determinant;
            Sx[1, 0] = -Xzu[1, 0] / Determinant; Sx[1, 1] = Xzu[0, 0] / Determinant;
            Sx[2, 0] = (Xzu[1, 0] - Xzu[1, 1]) / Determinant; Sx[2, 1] = (Xzu[0, 1] - Xzu[0, 0]) / Determinant;
            return Sx;
        }

        public static Point BerechneSchwerpunkt(AbstraktElement element)
        {
            var cg = new Point();
            var nodes = element.Knoten;
            cg.X = 0;
            for (var i = 0; i < element.Knoten.Length; i++)
            {
                cg.X += nodes[i].Koordinaten[0];
                cg.Y += nodes[i].Koordinaten[1];
            }
            cg.X /= 3.0;
            cg.Y /= 3.0;
            return cg;
        }
    }
}
