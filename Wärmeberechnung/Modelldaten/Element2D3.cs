using FEBibliothek.Modell;
using FEBibliothek.Modell.abstrakte_Klassen;
using FEBibliothek.Werkzeuge;
using System.Windows;

namespace FE_Berechnungen.Wärmeberechnung.Modelldaten;

public class Element2D3 : AbstraktLinear2D3
{
    private readonly double[] _elementTemperaturen = new double[3]; // an Elementknoten
    private AbstraktElement _element;
    private double[,] _elementMatrix = new double[3, 3];
    private Knoten _knoten;
    private Material _material;

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

    public double[] SpezifischeWärmeMatrix { get; }

    public FeModell Modell { get; }

    // ....Compute element Matrix.....................................
    public override double[,] BerechneElementMatrix()
    {
        BerechneGeometrie();
        if (Modell.Material.TryGetValue(ElementMaterialId, out var abstraktMaterial))
        {
        }

        _material = (Material)abstraktMaterial;
        ElementMaterial = _material;
        if (_material == null) return _elementMatrix;
        var leitfähigkeit = _material.MaterialWerte[0];
        // Ke = area*c*Sx*SxT
        _elementMatrix = MatrizenAlgebra.RectMultMatrixTransposed(0.5 * Determinant * leitfähigkeit, Sx, Sx);

        return _elementMatrix;
    }

    // ....berechne diagonale spezifische Wärme Matrix .................................
    public override double[] BerechneDiagonalMatrix()
    {
        BerechneGeometrie();
        // Me = dichte * leitfähigkeit * 0.5*determinante/3 (area/3)
        SpezifischeWärmeMatrix[0] = _material.MaterialWerte[3] * Determinant / 6;
        SpezifischeWärmeMatrix[1] = SpezifischeWärmeMatrix[0];
        if (SpezifischeWärmeMatrix.Length > 2) SpezifischeWärmeMatrix[2] = SpezifischeWärmeMatrix[0];
        return SpezifischeWärmeMatrix;
    }

    // berechne Wärmezustand an Mittelpunkten der Elemente
    public override double[] BerechneZustandsvektor()
    {
        var elementZustand = new double[2];
        return elementZustand;
    }

    public override double[] BerechneElementZustand(double z0, double z1)
    {
        var elementWärmeStatus = new double[2]; // in element
        BerechneGeometrie();
        if (Modell.Material.TryGetValue(ElementMaterialId, out var abstractMaterial))
        {
        }

        _material = (Material)abstractMaterial;
        ElementMaterial = _material;
        if (Modell.Elemente.TryGetValue(ElementId, out _element))
        {
            for (var i = 0; i < _element.Knoten.Length; i++)
            {
                if (Modell.Knoten.TryGetValue(_element.KnotenIds[i], out _knoten))
                {
                }

                if (_knoten != null) _elementTemperaturen[i] = _knoten.Knotenfreiheitsgrade[0];
            }

            if (_material == null) return elementWärmeStatus;
            var leitfähigkeit = _material.MaterialWerte[0];
            elementWärmeStatus = MatrizenAlgebra.MultTransposed(-leitfähigkeit, Sx, _elementTemperaturen);
        }
        else
        {
            throw new ModellAusnahme("\nElement2D3: " + ElementId + " nicht im Modell gefunden");
        }

        return elementWärmeStatus;
    }

    public override void SetzElementSystemIndizes()
    {
        SystemIndizesElement = new int[KnotenProElement * ElementFreiheitsgrade];
        var counter = 0;
        for (var i = 0; i < KnotenProElement; i++)
            for (var j = 0; j < ElementFreiheitsgrade; j++)
                SystemIndizesElement[counter++] = Knoten[i].SystemIndizes[j];
    }

    public override Point BerechneSchwerpunkt()
    {
        if (!Modell.Elemente.TryGetValue(ElementId, out _element))
            throw new ModellAusnahme("\nElement2D3: " + ElementId + " nicht im Modell gefunden");
        _element.SetzElementReferenzen(Modell);
        return BerechneSchwerpunkt(_element);
    }
}