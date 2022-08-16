using FEBibliothek.Modell;
using FEBibliothek.Modell.abstrakte_Klassen;
using System;
using System.Windows;

namespace FE_Berechnungen.Wärmeberechnung.Modelldaten
{
    public class Element2D2 : AbstraktLinear2D2
    {
        private readonly FeModell modell;
        private AbstraktElement element;
        private Material material;
        private readonly double[,] elementMatrix;
        private readonly double[] specificHeatMatrix;
        public Element2D2(string[] eNodes, string eMaterialId, FeModell feModell)
        {
            if (feModell != null) modell = feModell ?? throw new ArgumentNullException(nameof(feModell));
            KnotenIds = eNodes ?? throw new ArgumentNullException(nameof(eNodes));
            ElementMaterialId = eMaterialId;
            ElementFreiheitsgrade = 1;
            KnotenProElement = 2;
            elementMatrix = new double[KnotenProElement, KnotenProElement];
            specificHeatMatrix = new double[KnotenProElement];
            Knoten = new Knoten[KnotenProElement];
        }
        public Element2D2(string id, string[] eNodes, string eMaterialId, FeModell feModell)
        {
            modell = feModell ?? throw new ArgumentNullException(nameof(feModell));
            ElementId = id ?? throw new ArgumentNullException(nameof(id));
            KnotenIds = eNodes ?? throw new ArgumentNullException(nameof(eNodes));
            ElementMaterialId = eMaterialId ?? throw new ArgumentNullException(nameof(eMaterialId));
            ElementFreiheitsgrade = 1;
            KnotenProElement = 2;
            elementMatrix = new double[KnotenProElement, KnotenProElement];
            specificHeatMatrix = new double[KnotenProElement];
            Knoten = new Knoten[KnotenProElement];
        }
        // ... compute element matrix ..................................
        public override double[,] BerechneElementMatrix()
        {
            if (modell.Material.TryGetValue(ElementMaterialId, out var abstractMaterial)) { }
            material = (Material)abstractMaterial;
            ElementMaterial = material ?? throw new ArgumentNullException(nameof(material));
            balkenLänge = Math.Abs(Knoten[1].Koordinaten[0] - Knoten[0].Koordinaten[0]);
            if (material == null) return elementMatrix;
            var factor = material.MaterialWerte[0] / balkenLänge;
            elementMatrix[0, 0] = elementMatrix[1, 1] = factor;
            elementMatrix[0, 1] = elementMatrix[1, 0] = -factor;
            return elementMatrix;
        }
        // ....Compute diagonal Specific Heat Matrix.................................
        public override double[] BerechneDiagonalMatrix()
        {
            balkenLänge = Math.Abs(Knoten[1].Koordinaten[0] - Knoten[0].Koordinaten[0]);
            // Me = specific heat * density * 0.5*length
            specificHeatMatrix[0] = specificHeatMatrix[1] = material.MaterialWerte[3] * balkenLänge / 2;
            return specificHeatMatrix;
        }
        public override double[] BerechneZustandsvektor()
        {
            var elementWärmeStatus = new double[2];             // in element
            return elementWärmeStatus;
        }
        public override double[] BerechneElementZustand(double z0, double z1)
        {
            var elementWärmeStatus = new double[2];             // in element
            return elementWärmeStatus;
        }
        public override Point BerechneSchwerpunkt()
        {
            if (!modell.Elemente.TryGetValue(ElementId, out element))
            {
                throw new ModellAusnahme("Element2D2: " + ElementId + " nicht im Modell gefunden");
            }
            element.SetzElementReferenzen(modell);
            return Schwerpunkt(element);
        }
    }
}