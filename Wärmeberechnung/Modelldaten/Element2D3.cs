using FEBibliothek.Modell;
using FEBibliothek.Modell.abstrakte_Klassen;
using FEBibliothek.Werkzeuge;
using System.Windows;

namespace FE_Berechnungen.Wärmeberechnung.Modelldaten;

public class Element2D3 : AbstraktLinear2D3
{
    private AbstraktElement element;
    private double[,] elementMatrix = new double[3, 3];
    private Material material;
    private Knoten knoten;
    public double[] SpezifischeWärmeMatrix { get; }
    private readonly double[] elementTemperaturen = new double[3];   // at element nodes
    public FeModell Modell { get; }
    public Element2D3(string[] eKnotens, string eMaterialId, FeModell feModell)
    {
        Modell = feModell;
        ElementFreiheitsgrade = 1;
        KnotenProElement = 3;
        KnotenIds = eKnotens;
        Knoten = new Knoten[KnotenProElement];
        ElementMaterialId = eMaterialId;
        SpezifischeWärmeMatrix = new double[3];
    }
    public Element2D3(string id, string[] eKnotens, string eMaterialId, FeModell feModell)
    {
        Modell = feModell;
        ElementId = id;
        ElementFreiheitsgrade = 1;
        KnotenProElement = 3;
        KnotenIds = eKnotens;
        Knoten = new Knoten[KnotenProElement];
        ElementMaterialId = eMaterialId;
        SpezifischeWärmeMatrix = new double[3];
    }
    // ....Compute element Matrix.....................................
    public override double[,] BerechneElementMatrix()
    {
        BerechneGeometrie();
        if (Modell.Material.TryGetValue(ElementMaterialId, out var abstraktMaterial)) { }
        material = (Material)abstraktMaterial;
        ElementMaterial = material;
        if (material == null) return elementMatrix;
        var leitfähigkeit = material.MaterialWerte[0];
        // Ke = area*c*Sx*SxT
        elementMatrix = MatrizenAlgebra.RectMultMatrixTransposed(0.5 * Determinant * leitfähigkeit, Sx, Sx);

        return elementMatrix;
    }
    // ....berechne diagonale spezifische Wärme Matrix .................................
    public override double[] BerechneDiagonalMatrix()
    {
        BerechneGeometrie();
        // Me = dichte * leitfähigkeit * 0.5*determinante / 3    (area/3)
        SpezifischeWärmeMatrix[0] = material.MaterialWerte[3] * Determinant / 6;
        SpezifischeWärmeMatrix[1] = SpezifischeWärmeMatrix[0];
        if (SpezifischeWärmeMatrix.Length > 2) SpezifischeWärmeMatrix[2] = SpezifischeWärmeMatrix[0];
        return SpezifischeWärmeMatrix;
    }
    // ....Compute the heat state at the midpoint of the element......
    public override double[] BerechneZustandsvektor()
    {
        var elementZustand = new double[2];
        return elementZustand;
    }

    public override double[] BerechneElementZustand(double z0, double z1)
    {
        var elementWärmeStatus = new double[2];             // in element
        BerechneGeometrie();
        if (Modell.Material.TryGetValue(ElementMaterialId, out var abstractMaterial)) { }
        material = (Material)abstractMaterial;
        ElementMaterial = material;
        if (Modell.Elemente.TryGetValue(ElementId, out element))
        {
            for (var i = 0; i < element.Knoten.Length; i++)
            {
                if (Modell.Knoten.TryGetValue(element.KnotenIds[i], out knoten)) { }

                //Debug.Assert(node != null, nameof(node) + " != null");
                if (knoten != null) elementTemperaturen[i] = knoten.Knotenfreiheitsgrade[0];
            }

            if (material == null) return elementWärmeStatus;
            var leitfähigkeit = material.MaterialWerte[0];
            elementWärmeStatus = MatrizenAlgebra.MultTransposed(-leitfähigkeit, Sx, elementTemperaturen);
        }
        else
        {
            throw new ModellAusnahme("Element2D3: " + ElementId + " nicht im Modell gefunden");
        }
        return elementWärmeStatus;
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
        if (!Modell.Elemente.TryGetValue(ElementId, out element))
        {
            throw new ModellAusnahme("Element2D3: " + ElementId + " nicht im Modell gefunden");
        }
        element.SetzElementReferenzen(Modell);
        return BerechneSchwerpunkt(element);
    }
}