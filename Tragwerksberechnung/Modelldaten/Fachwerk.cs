using System;
using System.Windows;
using FEBibliothek.Modell;
using FEBibliothek.Modell.abstrakte_Klassen;
using FEBibliothek.Werkzeuge;

namespace FE_Berechnungen.Tragwerksberechnung.Modelldaten;

public class Fachwerk : AbstraktBalken
{
    private static double[,] _stiffnessMatrix = new double[4, 4];

    private static readonly double[] MassMatrix = new double[4];
    private readonly FeModell _modell;
    private AbstraktElement _element;
    private double _emodul, _masse, _fläche;

    public Fachwerk(string[] eKnotens, string materialId, string querschnittId, FeModell feModel)
    {
        _modell = feModel;
        KnotenIds = eKnotens;
        ElementMaterialId = materialId;
        ElementQuerschnittId = querschnittId;
        ElementFreiheitsgrade = 2;
        KnotenProElement = 2;
        Knoten = new Knoten[2];
        ElementZustand = new double[2];
        ElementVerformungen = new double[2];
    }

    // berechne Elementmatrix
    public override double[,] BerechneElementMatrix()
    {
        BerechneGeometrie();

        if (!_modell.Material.TryGetValue(ElementMaterialId, out var material)) return null;
        _emodul = E == 0 ? material.MaterialWerte[0] : E;
        if (!_modell.Querschnitt.TryGetValue(ElementQuerschnittId, out var querschnitt)) return null;
        _fläche = A == 0 ? querschnitt.QuerschnittsWerte[0] : A;
        var factor = _emodul * _fläche / BalkenLänge;
        var sx = BerechneSx();
        _stiffnessMatrix = MatrizenAlgebra.MultTransposedRect(factor, sx);
        return _stiffnessMatrix;
    }

    // berechne diagonale Massenmatrix
    public override double[] BerechneDiagonalMatrix() //throws AlgebraicException
    {
        if (ElementMaterial.MaterialWerte.Length < 3 && M == 0)
            throw new ModellAusnahme("\nFachwerk " + ElementId + ", spezifische Masse noch nicht definiert");
        // Me = specific mass * area * 0.5*length
        if (!_modell.Material.TryGetValue(ElementMaterialId, out var material)) return null;
        _masse = M == 0 ? material.MaterialWerte[2] : M;
        if (!_modell.Querschnitt.TryGetValue(ElementQuerschnittId, out var querschnitt)) return null;
        _fläche = A == 0 ? querschnitt.QuerschnittsWerte[0] : A;

        MassMatrix[0] = MassMatrix[1] = MassMatrix[2] = MassMatrix[3] = _masse * _fläche * BalkenLänge / 2;
        return MassMatrix;
    }

    public static double[] ComputeLoadVector(AbstraktElementLast ael, bool inElementCoordinateSystem)
    {
        if (ael == null) throw new ArgumentNullException(nameof(ael));
        throw new ModellAusnahme("\nFachwerkelement kann keine interne Last aufnehmen! Benutze Biegebalken mit Gelenk");
    }

    // berechne Stabendkräfte eines Biegeelementes
    public override double[] BerechneStabendkräfte()
    {
        BerechneGeometrie();
        BerechneZustandsvektor();
        var c1 = ElementMaterial.MaterialWerte[0] * ElementQuerschnitt.QuerschnittsWerte[0] / BalkenLänge;
        ElementZustand[0] = c1 * (ElementVerformungen[0] - ElementVerformungen[1]);
        ElementZustand[1] = ElementZustand[0];
        return ElementZustand;
    }

    // berechne Verschiebungsvektor eines Biegeelementes
    public override double[] BerechneZustandsvektor()
    {
        // transform to the local coordinate system
        ElementVerformungen[0] = RotationsMatrix[0, 0] * Knoten[0].Knotenfreiheitsgrade[0]
                                 + RotationsMatrix[1, 0] * Knoten[0].Knotenfreiheitsgrade[1];
        ElementVerformungen[1] = RotationsMatrix[0, 0] * Knoten[1].Knotenfreiheitsgrade[0]
                                 + RotationsMatrix[1, 0] * Knoten[1].Knotenfreiheitsgrade[1];
        return ElementVerformungen;
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
        if (!_modell.Elemente.TryGetValue(ElementId, out _element))
            throw new ModellAusnahme("\nFachwerk: " + ElementId + " nicht im Modell gefunden");
        return Schwerpunkt(_element);
    }

    public override double[] BerechneElementZustand(double z0, double z1)
    {
        throw new NotImplementedException();
    }
}