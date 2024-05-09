using FEBibliothek.Modell;
using FEBibliothek.Modell.abstrakte_Klassen;
using FEBibliothek.Werkzeuge;
using System.Windows;

namespace FE_Berechnungen.Elastizitätsberechnung.Modelldaten;

public class Element2D3 : AbstraktLinear2D3
{
    private AbstraktElement _element;
    private double[,] _matrix = new double[6, 6];
    private readonly double[] _elementVerformungen = new double[6];// an Elementknoten

    private FeModell Modell { get; }

    private readonly double[,] _b = new double[3, 6];               // strain-displacement transformation
    private readonly double[,] _e = new double[3, 3];               // Materialmatrix

    public Element2D3(string[] eKnotens, string querschnittId, string eMaterialId, FeModell feModell)
    {
        Modell = feModell;
        ElementFreiheitsgrade = 2;
        KnotenProElement = 3;
        KnotenIds = eKnotens;
        Knoten = new Knoten[KnotenProElement];
        ElementQuerschnittId = querschnittId;
        ElementMaterialId = eMaterialId;
    }

    public override double[,] BerechneElementMatrix()
    {
        BerechneGeometrie();
        BerechneSpannungsDehnungsTransformation();
        ComputeMaterial();
        // Ke = 0.5*thickness*determinant*BT*E*B
        var temp = MatrizenAlgebra.MultTransposedMatrix(0.5 * ElementQuerschnitt.QuerschnittsWerte[0] * Determinant, _b, _e);
        _matrix = MatrizenAlgebra.Mult(temp, _b);
        return _matrix;
    }

    public override double[] BerechneDiagonalMatrix()
    {
        throw new ModellAusnahme("*** Mass Matrix noch nicht implementiert in Elastizität2D3");
    }

    private void BerechneSpannungsDehnungsTransformation()
    {
        _b[0, 0] = Xzu[1, 1]; _b[0, 1] = 0; _b[0, 2] = -Xzu[1, 0]; _b[0, 3] = 0; _b[0, 4] = Xzu[1, 0] - Xzu[1, 1]; _b[0, 5] = 0;
        _b[1, 0] = 0; _b[1, 1] = -Xzu[0, 1]; _b[1, 2] = 0; _b[1, 3] = Xzu[0, 0]; _b[1, 4] = 0; _b[1, 5] = Xzu[0, 1] - Xzu[0, 0];
        _b[2, 0] = -Xzu[0, 1]; _b[2, 1] = Xzu[1, 1]; _b[2, 2] = Xzu[0, 0]; _b[2, 3] = -Xzu[1, 0]; _b[2, 4] = Xzu[0, 1] - Xzu[0, 0]; _b[2, 5] = Xzu[1, 0] - Xzu[1, 1];
    }

    // berechne Materialmatrix für ebene Spannung
    private void ComputeMaterial()
    {
        var emod = ElementMaterial.MaterialWerte[0];
        var ratio = ElementMaterial.MaterialWerte[1];
        var factor = emod * (1.0 - ratio) / ((1.0 + ratio) * (1.0 - 2.0 * ratio));
        var coeff = ratio / (1.0 - ratio);

        _e[0, 0] = factor; _e[0, 1] = coeff * factor;
        _e[1, 0] = coeff * factor; _e[1, 1] = factor;
        _e[2, 2] = (1.0 - 2.0 * ratio) / 2.0 / (1.0 - ratio) * factor;
    }

    // --- Elementverhalten ----------------------------------

    // Berechne Elementspannungen: sigma = E∗B*Ue (Element Verformungen) ......
    public override double[] BerechneZustandsvektor()
    {
        for (var i = 0; i < KnotenProElement; i++)
        {
            var nodalDof = i * 2;
            _elementVerformungen[nodalDof] = Knoten[i].Knotenfreiheitsgrade[0];
            _elementVerformungen[nodalDof + 1] = Knoten[i].Knotenfreiheitsgrade[1];
        }
        var temp = MatrizenAlgebra.Mult(_e, _b);
        var elementSpannungen = MatrizenAlgebra.Mult(temp, _elementVerformungen);
        return elementSpannungen;
    }

    public override Point BerechneSchwerpunkt()
    {
        if (!Modell.Elemente.TryGetValue(ElementId, out _element))
        {
            throw new ModellAusnahme("Element2D3: " + ElementId + " nicht im Modell gefunden");
        }
        _element.SetzElementReferenzen(Modell);
        return BerechneSchwerpunkt(_element);
    }

    public override double[] BerechneElementZustand(double z0, double z1)
    {
        for (var i = 0; i < KnotenProElement; i++)
        {
            var nodalDof = i * 2;
            _elementVerformungen[nodalDof] = Knoten[i].Knotenfreiheitsgrade[0];
            _elementVerformungen[nodalDof + 1] = Knoten[i].Knotenfreiheitsgrade[1];
        }
        return _elementVerformungen;
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
}