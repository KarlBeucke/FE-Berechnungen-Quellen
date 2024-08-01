using FEBibliothek.Modell;
using FEBibliothek.Modell.abstrakte_Klassen;
using System;
using System.Windows;

namespace FE_Berechnungen.Wärmeberechnung.Modelldaten;

public class Element2D2 : AbstraktLinear2D2
{
    private readonly double[,] _elementMatrix;
    private readonly FeModell _modell;
    private readonly double[] _specificHeatMatrix;
    private AbstraktElement _element;
    private Material _material;

    public Element2D2(string[] eNodes, string eMaterialId, FeModell feModell)
    {
        if (feModell != null) _modell = feModell;
        KnotenIds = eNodes ?? throw new ArgumentNullException(nameof(eNodes));
        ElementMaterialId = eMaterialId;
        ElementFreiheitsgrade = 1;
        KnotenProElement = 2;
        _elementMatrix = new double[KnotenProElement, KnotenProElement];
        _specificHeatMatrix = new double[KnotenProElement];
        Knoten = new Knoten[KnotenProElement];
    }

    public Element2D2(string id, string[] eNodes, string eMaterialId, FeModell feModell)
    {
        _modell = feModell ?? throw new ArgumentNullException(nameof(feModell));
        ElementId = id ?? throw new ArgumentNullException(nameof(id));
        KnotenIds = eNodes ?? throw new ArgumentNullException(nameof(eNodes));
        ElementMaterialId = eMaterialId ?? throw new ArgumentNullException(nameof(eMaterialId));
        ElementFreiheitsgrade = 1;
        KnotenProElement = 2;
        _elementMatrix = new double[KnotenProElement, KnotenProElement];
        _specificHeatMatrix = new double[KnotenProElement];
        Knoten = new Knoten[KnotenProElement];
    }

    // berechne Elementmatrix
    public override double[,] BerechneElementMatrix()
    {
        if (_modell.Material.TryGetValue(ElementMaterialId, out var abstractMaterial))
        {
        }

        _material = (Material)abstractMaterial;
        ElementMaterial = _material ?? throw new ArgumentNullException(nameof(_material));
        BalkenLänge = Math.Abs(Knoten[1].Koordinaten[0] - Knoten[0].Koordinaten[0]);
        if (_material == null) return _elementMatrix;
        var factor = _material.MaterialWerte[0] / BalkenLänge;
        _elementMatrix[0, 0] = _elementMatrix[1, 1] = factor;
        _elementMatrix[0, 1] = _elementMatrix[1, 0] = -factor;
        return _elementMatrix;
    }

    // berechne diagonale spezifische Wärmematrix
    public override double[] BerechneDiagonalMatrix()
    {
        BalkenLänge = Math.Abs(Knoten[1].Koordinaten[0] - Knoten[0].Koordinaten[0]);
        // Me = specific heat * density * 0.5*length
        _specificHeatMatrix[0] = _specificHeatMatrix[1] = _material.MaterialWerte[3] * BalkenLänge / 2;
        return _specificHeatMatrix;
    }

    public override double[] BerechneZustandsvektor()
    {
        var elementWärmeStatus = new double[2]; // in element
        return elementWärmeStatus;
    }

    public override double[] BerechneElementZustand(double z0, double z1)
    {
        var elementWärmeStatus = new double[2]; // in element
        return elementWärmeStatus;
    }

    public override Point BerechneSchwerpunkt()
    {
        if (!_modell.Elemente.TryGetValue(ElementId, out _element))
            throw new ModellAusnahme("\nElement2D2: " + ElementId + " nicht im Modell gefunden");
        _element.SetzElementReferenzen(_modell);
        return Schwerpunkt(_element);
    }
}