using FEBibliothek.Modell;
using FEBibliothek.Modell.abstrakte_Klassen;
using System.Windows;

namespace FE_Berechnungen.Tragwerksberechnung.Modelldaten
{
    public class FederElement : Abstrakt2D
    {
        private readonly FEModell modell;
        private Knoten node;

        private readonly double[,] stiffnessMatrix = new double[3, 3];

        // ... Constructor ........................................................
        public FederElement(string[] springKnoten, string eMaterialId, FEModell feModel)
        {
            modell = feModel;
            KnotenIds = springKnoten;
            ElementMaterialId = eMaterialId;
            ElementFreiheitsgrade = 3;
            KnotenProElement = 1;
            Knoten = new Knoten[1];
        }

        // ... compute element matrix ..................................
        public override double[,] BerechneElementMatrix()
        {
            stiffnessMatrix[0, 0] = ElementMaterial.MaterialWerte[0];
            stiffnessMatrix[1, 1] = ElementMaterial.MaterialWerte[1];
            stiffnessMatrix[2, 2] = ElementMaterial.MaterialWerte[2];
            return stiffnessMatrix;
        }

        // ....Compute diagonal Spring Matrix.................................
        public override double[] BerechneDiagonalMatrix()
        {
            throw new ModellAusnahme("*** Massenmatrix nicht relevant für Federlager");
        }

        // ... compute forces of spring element........................
        public override double[] BerechneZustandsvektor()
        {
            ElementZustand = new double[3];
            ElementZustand[0] = ElementMaterial.MaterialWerte[0] * Knoten[0].Knotenfreiheitsgrade[0];
            ElementZustand[1] = ElementMaterial.MaterialWerte[1] * Knoten[0].Knotenfreiheitsgrade[1];
            ElementZustand[2] = ElementMaterial.MaterialWerte[2] * Knoten[0].Knotenfreiheitsgrade[2];
            return ElementZustand;
        }

        public override double[] BerechneElementZustand(double z0, double z1)
        {
            var springForces = new double[3];
            return springForces;
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
        public override Point BerechneSchwerpunkt()
        {
            var cg = new Point();

            if (!modell.Knoten.TryGetValue(KnotenIds[0], out node))
            {
                throw new ModellAusnahme("FederElement: " + ElementId + " nicht im Modell gefunden");
            }

            cg.X = node.Koordinaten[0];
            cg.Y = node.Koordinaten[1];
            return cg;
        }
    }
}
