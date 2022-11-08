using System.Windows;

namespace FEBibliothek.Modell.abstrakte_Klassen
{
    public abstract class AbstraktElement
    {
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
            for (int i = 0; i < KnotenProElement; i++)
            {
                if (modell.Knoten.TryGetValue(KnotenIds[i], out Knoten node)) { Knoten[i] = node; }

                if (node != null) continue;
                var message = "Element mit ID = " + KnotenIds[i] + " ist nicht im Modell enthalten";
                _ = MessageBox.Show(message, "AbstraktElement");
            }
            if (modell.Material.TryGetValue(ElementMaterialId, out AbstraktMaterial material)) { ElementMaterial = material; }
            if (material == null)
            {
                var message = "Material mit ID=" + ElementMaterialId + " ist nicht im Modell enthalten";
                _ = MessageBox.Show(message, "AbstraktElement");
            }
        }
    }
}
