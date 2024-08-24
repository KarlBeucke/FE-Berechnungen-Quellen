using FEBibliothek.Modell;
using FEBibliothek.Modell.abstrakte_Klassen;
using FEBibliothek.Werkzeuge;
using System;
using System.Windows.Media.Media3D;

namespace FE_Berechnungen.Elastizitätsberechnung.Modelldaten;

public class Element3D8 : AbstraktLinear3D8
{
    private static readonly double[] GCoord = [-1.0 / Math.Sqrt(5.0 / 3.0), 0.0, 1.0 / Math.Sqrt(5.0 / 3.0)];
    private static readonly double[] GWeight = [5.0 / 9.0, 8.0 / 9.0, 5.0 / 9.0]; // gaussian coordinates, weights
    private readonly double[,] b = new double[6, 24]; // strain-displacement transformation
    private readonly double[,] e = new double[6, 6]; // material matrix
    private readonly double[] elementDeformations = new double[24]; // at element nodes
    private readonly double[,] elementMatrix = new double[24, 24];
    private readonly FeModell modell;
    private AbstraktElement element;
    private double z0, z1, z2, g0, g1, g2;

    public Element3D8(string[] eKnotens, string eMaterialId, FeModell feModell)
    {
        modell = feModell;
        ElementFreiheitsgrade = 3;
        KnotenProElement = 8;
        KnotenIds = eKnotens;
        Knoten = new Knoten[KnotenProElement];
        ElementMaterialId = eMaterialId;
    }

    // ....Compute element matrix
    public override double[,] BerechneElementMatrix()
    {
        MatrizenAlgebra.Clear(elementMatrix);
        ComputeMaterial();
        for (var i = 0; i < GCoord.Length; i++)
        {
            z0 = GCoord[i];
            g0 = GWeight[i];
            for (var j = 0; j < GCoord.Length; j++)
            {
                z1 = GCoord[j];
                g1 = GWeight[j];
                for (var k = 0; k < GCoord.Length; k++)
                {
                    z2 = GCoord[k];
                    g2 = GWeight[k];
                    BerechneGeometrie(z0, z1, z2);
                    Sx = BerechneSx(z0, z1, z2);
                    ComputeStrainDisplacementTransformation();
                    // Ke = determinant*g0*g1*g2*BT*E*B
                    var temp = MatrizenAlgebra.MultTransposedMatrix(Determinant * g0 * g1 * g2, b, e);
                    MatrizenAlgebra.MultAddMatrix(elementMatrix, temp, b);
                }
            }
        }

        return elementMatrix;
    }

    // ....Compute mass Matrix
    public override double[] BerechneDiagonalMatrix()
    {
        throw new ModellAusnahme("\n*** Massenmatrix noch nicht implementiert in Element3D8");
    }

    // compute strain-displacement transformation matrix eps = B * u
    private void ComputeStrainDisplacementTransformation()
    {
        int i, j, k;
        for (i = 0; i < 3; i++)
            for (j = i, k = 0; k < 8; j += 3, k++)
                b[i, j] = Sx[k, i];
        for (i = 0, k = 0; i < 8; i++, k += 3) b[3, k] = Sx[i, 1];
        for (i = 0, k = 1; i < 8; i++, k += 3) b[3, k] = Sx[i, 0];
        for (i = 0, k = 1; i < 8; i++, k += 3) b[4, k] = Sx[i, 2];
        for (i = 0, k = 2; i < 8; i++, k += 3) b[4, k] = Sx[i, 1];
        for (i = 0, k = 0; i < 8; i++, k += 3) b[5, k] = Sx[i, 2];
        for (i = 0, k = 2; i < 8; i++, k += 3) b[5, k] = Sx[i, 0];
    }

    // compute material matrix for plane strain
    private void ComputeMaterial()
    {
        var emod = ElementMaterial.MaterialWerte[0];
        var nue = ElementMaterial.MaterialWerte[1];
        var factor = emod / (1.0 + nue); // E/(1+nue)

        e[0, 0] = e[1, 1] = e[2, 2] = factor * (1.0 - nue) / (1.0 - 2.0 * nue);
        e[3, 3] = e[4, 4] = e[5, 5] = factor * 0.5;
        e[0, 1] = e[0, 2] = e[1, 2] =
            e[1, 0] = e[2, 0] = e[2, 1] = factor * nue / (1.0 - 2.0 * nue);
    }

    // --- Elementverhalten

    // ....Berechne Elementspannungen: sigma = E * B * Ue (Elementverformungen)
    public override double[] BerechneZustandsvektor()
    {
        for (var i = 0; i < 8; i++)
        {
            var nodalDof = i * ElementFreiheitsgrade;
            elementDeformations[nodalDof] = Knoten[i].Knotenfreiheitsgrade[0];
            elementDeformations[nodalDof + 1] = Knoten[i].Knotenfreiheitsgrade[1];
            elementDeformations[nodalDof + 2] = Knoten[i].Knotenfreiheitsgrade[2];
        }

        var temp = MatrizenAlgebra.Mult(e, b);
        var elementStresses = MatrizenAlgebra.Mult(temp, elementDeformations);
        return elementStresses;
    }

    public override double[] BerechneElementZustand(double zet0, double zet1, double zet2)
    {
        var elementStresses = new double[6];
        return elementStresses;
    }

    public override void SetzElementSystemIndizes()
    {
        SystemIndizesElement = new int[KnotenProElement * ElementFreiheitsgrade];
        var counter = 0;
        for (var i = 0; i < KnotenProElement; i++)
            for (var j = 0; j < ElementFreiheitsgrade; j++)
                SystemIndizesElement[counter++] = Knoten[i].SystemIndizes[j];
    }

    public override Point3D BerechneSchwerpunkt3D()
    {
        if (!modell.Elemente.TryGetValue(ElementId, out element))
            throw new ModellAusnahme("\nElement3D8: " + ElementId + " nicht im Modell gefunden");
        element.SetzElementReferenzen(modell);
        return BerechneSchwerpunkt3D(element);
    }
}