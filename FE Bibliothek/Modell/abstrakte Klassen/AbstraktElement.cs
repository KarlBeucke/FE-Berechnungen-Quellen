namespace FEBibliothek.Modell.abstrakte_Klassen
{
    public abstract class AbstraktElement
    {
        public double E { get; set; }
        public double Nue { get; set; }
        public double M { get; set; }
        public double A { get; set; }
        public double I { get; set; }
        public double Kxx { get; set; }
        public double Kyy { get; set; }
        public double Kphi { get; set; }
        public string ElementId { get; set; }
        public string[] KnotenIds { get; protected set; }
        public Knoten[] Knoten { get; protected set; }
        protected int ElementFreiheitsgrade { get; set; }
        public int KnotenProElement { get; set; }
        public int[] SystemIndizesElement { get; protected set; }
        public string ElementMaterialId { get; set; }
        public string ElementQuerschnittId { get; set; }
        public AbstraktMaterial ElementMaterial { get; set; }
        public int Typ { get; set; }
        public double[] ElementZustand { get; set; }
        public double[] ElementVerformungen { get; protected set; }
        public double Determinant { get; protected set; }
        public abstract double[,] BerechneElementMatrix();
        public abstract double[] BerechneDiagonalMatrix();
        public abstract void SetzElementSystemIndizes();
        public abstract double[] BerechneZustandsvektor();

        public void SetzElementReferenzen(FeModell modell)
        {
            for (var i = 0; i < KnotenProElement; i++)
            {
                if (modell.Knoten.TryGetValue(KnotenIds[i], out var node)) { Knoten[i] = node; }

                if (node != null) continue;
                throw new ModellAusnahme("\nElement mit ID = " + KnotenIds[i] + " ist nicht im Modell enthalten");
            }
            if (modell.Material.TryGetValue(ElementMaterialId, out var material)) { ElementMaterial = material; }

            if (material != null) return;
            {
                throw new ModellAusnahme("\nMaterial mit ID=" + ElementMaterialId + " ist nicht im Modell enthalten");
            }
        }
    }
}
