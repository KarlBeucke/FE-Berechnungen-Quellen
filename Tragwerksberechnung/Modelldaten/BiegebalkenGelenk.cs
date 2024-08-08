using FEBibliothek.Modell;
using FEBibliothek.Modell.abstrakte_Klassen;
using FEBibliothek.Werkzeuge;
using System;
using System.Windows;

namespace FE_Berechnungen.Tragwerksberechnung.Modelldaten;

public class BiegebalkenGelenk : AbstraktBalken
{
    private const int Erster = 1;
    private const int Zweiter = 2;

    private readonly int[] _c;
    private readonly int[] _chigh = [0, 1, 2, 3, 4];

    // Identifikatoren der Freiheitsgrade
    private readonly int[] _clow = [0, 1, 3, 4, 5];

    private readonly double[,] _kcc = new double[5, 5];
    private readonly double[] _kcl = new double[5];
    private readonly double[,] _kclxkllxklc = new double[5, 5];
    private readonly double[] _klc = new double[5];
    private readonly double[] _kll = new double[1];
    private readonly double[] _kllxklc = new double[5];
    private readonly int[] _l;
    private readonly int[] _lhigh = [5];
    private readonly int[] _llow = [2];
    private readonly double[] _massenMatrix = new double[6];
    private readonly FeModell _modell;
    private readonly double[] _uc = new double[5];
    private AbstraktElement _element;
    private double _emodul, _masse, _fläche, _trägheitsmoment;

    // temporäre Variable für ein Gelenk
    private double _invkll;
    private double[] _kcxuc = new double[5];
    private double[,] _redSteifigkeitsMatrix = new double[5, 5];
    private double[,] _steifigkeitsMatrix = new double[6, 6];

    public BiegebalkenGelenk(string[] eKnotenIds, string eMaterialId, string eQuerschnittId, FeModell feModell, int typ)
    {
        _modell = feModell;
        ElementFreiheitsgrade = 3;
        KnotenIds = eKnotenIds;
        ElementMaterialId = eMaterialId;
        ElementQuerschnittId = eQuerschnittId;
        KnotenProElement = 2;
        Knoten = new Knoten[2];
        if (!_modell.Material.TryGetValue(eMaterialId, out var material)) return;
        _emodul = E == 0 ? material.MaterialWerte[0] : E;
        Typ = typ;
        ElementZustand = new double[6];
        ElementVerformungen = new double[6];

        Typ = typ;
        switch (Typ)
        {
            case Erster:
                _c = _clow;
                _l = _llow;
                break;
            case Zweiter:
                _c = _chigh;
                _l = _lhigh;
                break;
            default:
                throw new ModellAusnahme("\nBiegebalkenGelenk: Gelenktyp wurde nicht erkannt!");
        }
    }

    // berechne lokale Steifigkeit
    private double[,] BerechneLokaleSteifigkeitsmatrix()
    {
        BerechneGeometrie();

        if (!_modell.Material.TryGetValue(ElementMaterialId, out var material)) 
            throw new BerechnungAusnahme("Material Id" +
                                         " für Element "+ElementId+" nicht definiert");
        // ?: operator - the ternary conditional operator
        _emodul = E == 0 ? material.MaterialWerte[0] : E;
        if (!_modell.Querschnitt.TryGetValue(ElementQuerschnittId, out var querschnitt)) 
            throw new BerechnungAusnahme("Querschnitt Id" +
                                         " für Element "+ElementId+" nicht definiert");
        if (querschnitt.QuerschnittsWerte.Length < 2 && I == 0)
            throw new BerechnungAusnahme("Trägheitsmoment für Element "+ElementId+" nicht definiert");

        _fläche = A == 0 ? querschnitt.QuerschnittsWerte[0] : A;
        _trägheitsmoment = I == 0 ? querschnitt.QuerschnittsWerte[1] : I;
        var h2 = _emodul * _trägheitsmoment; // EI
        var c1 = _emodul * _fläche / BalkenLänge; // AE/L
        var c2 = 12.0 * h2 / BalkenLänge / BalkenLänge / BalkenLänge;
        var c3 = 6.0 * h2 / BalkenLänge / BalkenLänge;
        var c4 = 4.0 * h2 / BalkenLänge;
        var c5 = 0.5 * c4;

        double[,] lokaleSteifigkeitsmatrix =
        {
            { c1, 0, 0, -c1, 0, 0 },
            { 0, c2, c3, 0, -c2, c3 },
            { 0, c3, c4, 0, -c3, c5 },
            { -c1, 0, 0, c1, 0, 0 },
            { 0, -c2, -c3, 0, c2, -c3 },
            { 0, c3, c5, 0, -c3, c4 }
        };
        return lokaleSteifigkeitsmatrix;
    }

    public double[] BiegebalkenGelenkLastVektor(AbstraktElementLast ael, bool inElementCoordinateSystem)
    {
        var p = new double[6];
        var pc = new double[5];
        var pl = new double[1];
        var kcl = new double[5, 1];
        var kclxinvkll = new double[5, 1];
        var invkll = new double[1, 1];
        var kclxinvkllxpl = new double[5, 1];
        var lastVektor = new double[5];
        var superLastVektor = BerechneLastVektor(ael, inElementCoordinateSystem);
        //Array.Copy(superLoadVector, 0, p, 0, 6); //length 6, calculates the kcc, kcl ... matrices for this element
        Array.Copy(superLastVektor, p, 6);
        if (inElementCoordinateSystem) BerechneElementMatrix();

        if (Typ == Erster)
        {
            pc[0] = p[0];
            pc[1] = p[1];
            pc[2] = p[3];
            pc[3] = p[4];
            pc[4] = p[5];
            pl[0] = p[2];
        }
        else if (Typ == Zweiter)
        {
            pc[0] = p[0];
            pc[1] = p[1];
            pc[2] = p[2];
            pc[3] = p[3];
            pc[4] = p[4];
            pl[0] = p[5];
        }

        for (var k = 0; k < 5; k++) kclxinvkll[k, 0] = kcl[k, 0] * invkll[0, 0];
        for (var i = 0; i < 5; i++)
            for (var j = 0; j < 1; j++)
                kclxinvkllxpl[i, 0] += kclxinvkll[i, 0] * pl[j];
        for (var k = 0; k < 5; k++) kclxinvkllxpl[k, 0] = kclxinvkllxpl[k, 0] * -1;
        for (var k = 0; k < 5; k++) lastVektor[k] = pc[k] + kclxinvkllxpl[k, 0];
        return lastVektor;
    }

    private double[,] TransformMatrix(double[,] matrix)
    {
        var elementFreiheitsgrade = ElementFreiheitsgrade;
        for (var i = 0; i < matrix.GetLength(0); i += elementFreiheitsgrade)
            for (var k = 0; k < matrix.GetLength(0); k += elementFreiheitsgrade)
            {
                var m11 = matrix[i, k];
                var m12 = matrix[i, k + 1];
                var m13 = matrix[i, k + 2];

                var m21 = matrix[i + 1, k];
                var m22 = matrix[i + 1, k + 1];
                var m23 = matrix[i + 1, k + 2];

                var m31 = matrix[i + 2, k];
                var m32 = matrix[i + 2, k + 1];

                var e11 = RotationsMatrix[0, 0];
                var e12 = RotationsMatrix[0, 1];
                var e21 = RotationsMatrix[1, 0];
                var e22 = RotationsMatrix[1, 1];

                var h11 = e11 * m11 + e12 * m21;
                var h12 = e11 * m12 + e12 * m22;
                var h21 = e21 * m11 + e22 * m21;
                var h22 = e21 * m12 + e22 * m22;

                matrix[i, k] = h11 * e11 + h12 * e12;
                matrix[i, k + 1] = h11 * e21 + h12 * e22;
                matrix[i + 1, k] = h21 * e11 + h22 * e12;
                matrix[i + 1, k + 1] = h21 * e21 + h22 * e22;

                matrix[i, k + 2] = e11 * m13 + e12 * m23;
                matrix[i + 1, k + 2] = e21 * m13 + e22 * m23;
                matrix[i + 2, k] = m31 * e11 + m32 * e12;
                matrix[i + 2, k + 1] = m31 * e21 + m32 * e22;
            }

        return matrix;
    }

    public override double[] BerechneStabendkräfte()
    {
        var matrix = BerechneLokaleReduzierteMatrix();
        ElementVerformungen = BerechneZustandsvektor();

        // Beitrag der Knotenverformungen
        _kcxuc = MatrizenAlgebra.Mult(matrix, ElementVerformungen);

        // Beitrag der Balkenlasten
        foreach (var item in _modell.PunktLasten)
        {
            if (item.Value is not PunktLast punktLast) continue;
            ElementVerformungen = punktLast.BerechneLokalenLastVektor();
            for (var k = 0; k < 5; k++) _kcxuc[k] -= ElementVerformungen[k];
            break;
        }

        foreach (var item in _modell.ElementLasten)
        {
            if (item.Value is not LinienLast linienLast) continue;
            ElementVerformungen = linienLast.BerechneLokalenLastVektor();
            for (var k = 0; k < 5; k++) _kcxuc[k] -= ElementVerformungen[k];
            break;
        }

        switch (Typ)
        {
            case Erster:
                ElementZustand[0] = -_kcxuc[0];
                ElementZustand[1] = -_kcxuc[1];
                ElementZustand[2] = 0.0;
                ElementZustand[3] = _kcxuc[2];
                ElementZustand[4] = _kcxuc[3];
                ElementZustand[5] = _kcxuc[4];
                break;
            case Zweiter:
                ElementZustand[0] = -_kcxuc[0];
                ElementZustand[1] = -_kcxuc[1];
                ElementZustand[2] = -_kcxuc[2];
                ElementZustand[3] = _kcxuc[3];
                ElementZustand[4] = _kcxuc[4];
                ElementZustand[5] = 0.0;
                break;
        }

        return ElementZustand;
    }

    // berechne Verformungsvektor für Rahmenelemente
    public int[] HolSystemIndizes()
    {
        int[] indizes;
        if (Typ == Erster)
        {
            var reduced = new int[5];
            indizes = Knoten[0].SystemIndizes;
            reduced[0] = indizes[0];
            reduced[1] = indizes[1];
            indizes = Knoten[1].SystemIndizes;
            reduced[2] = indizes[0];
            reduced[3] = indizes[1];
            reduced[4] = indizes[2];
            return reduced;
        }

        if (Typ != Zweiter) throw new ModellAusnahme("\nBiegebalkenGelenk GetSystemIndices: ungültiger Gelenktyp");
        {
            var reduziert = new int[5];
            indizes = Knoten[0].SystemIndizes;
            reduziert[0] = indizes[0];
            reduziert[1] = indizes[1];
            reduziert[2] = indizes[2];
            indizes = Knoten[1].SystemIndizes;
            reduziert[3] = indizes[0];
            reduziert[4] = indizes[1];
            return reduziert;
        }
    }

    /**
     * |Kcc Klc|
     * |       |
     * |Kcl Kll|
     * 
     * | Kcc - Kcl*Kll^-1*klc |
     */

    // reduzierte Steifigkeitsmatrix
    private double[,] KondensierMatrix(double[,] ke)
    {
        MatrizenAlgebra.ExtractSubMatrix(ke, _kcc, _c);
        MatrizenAlgebra.ExtractSubMatrix(ke, _kcl, _c, _l);
        MatrizenAlgebra.ExtractSubMatrix(ke, _klc, _l, _c);
        MatrizenAlgebra.ExtractSubMatrix(ke, _kll, _l, _l);
        _invkll = 1 / _kll[0];
        for (var k = 0; k < 5; k++) _kllxklc[k] = _invkll * _klc[k];
        for (var i = 0; i < 5; i++)
            for (var j = 0; j < 5; j++)
                _kclxkllxklc[i, j] = _kcl[j] * _kllxklc[i];
        for (var i = 0; i < 5; i++)
            for (var j = 0; j < 5; j++)
                _redSteifigkeitsMatrix[i, j] = _kcc[i, j] - _kclxkllxklc[i, j];
        //MatrixAlgebra.Subtract(redStiffnessMatrix, kcc, kclxkllxklc);
        return _redSteifigkeitsMatrix;
    }

    private double[,] BerechneLokaleReduzierteMatrix()
    {
        return KondensierMatrix(BerechneLokaleSteifigkeitsmatrix());
    }

    public override double[,] BerechneElementMatrix()
    {
        _steifigkeitsMatrix = BerechneLokaleSteifigkeitsmatrix();
        // transform local matrix to compute global stiffness
        _steifigkeitsMatrix = TransformMatrix(_steifigkeitsMatrix);

        _redSteifigkeitsMatrix = KondensierMatrix(_steifigkeitsMatrix);
        return _redSteifigkeitsMatrix;
    }

    public override double[] BerechneDiagonalMatrix()
    {
        if (ElementMaterial.MaterialWerte.Length < 3 && M == 0)
            throw new ModellAusnahme("\nBiegebalkenGelenk " + ElementId + ", spezifische Masse noch nicht definiert");
        // Me = spezifische masse * fläche * 0.5*balkenlänge
        if (!_modell.Material.TryGetValue(ElementMaterialId, out var material)) return null;
        _masse = M == 0 ? material.MaterialWerte[2] : M;
        if (!_modell.Querschnitt.TryGetValue(ElementQuerschnittId, out var querschnitt)) return null;
        _fläche = A == 0 ? querschnitt.QuerschnittsWerte[0] : A;

        _massenMatrix[0] = _massenMatrix[1] = _massenMatrix[3] = _massenMatrix[4] = _masse * _fläche * BalkenLänge / 2;
        _massenMatrix[2] = _massenMatrix[5] = 1;
        return _massenMatrix;
    }

    public override void SetzElementSystemIndizes()
    {
        SystemIndizesElement = new int[5];
        var counter = 0;
        switch (Typ)
        {
            case Erster:
                {
                    for (var j = 0; j < ElementFreiheitsgrade - 1; j++)
                        SystemIndizesElement[counter++] = Knoten[0].SystemIndizes[j];
                    for (var j = 0; j < ElementFreiheitsgrade; j++)
                        SystemIndizesElement[counter++] = Knoten[1].SystemIndizes[j];
                    break;
                }
            case Zweiter:
                {
                    for (var j = 0; j < ElementFreiheitsgrade; j++)
                        SystemIndizesElement[counter++] = Knoten[0].SystemIndizes[j];
                    for (var j = 0; j < ElementFreiheitsgrade - 1; j++)
                        SystemIndizesElement[counter++] = Knoten[1].SystemIndizes[j];
                    break;
                }
            default:
                throw new ModellAusnahme("\nBiegebalkenGelenk SetSystemIndices: Gelenktyp wurde nicht erkannt!");
        }
    }

    public override double[] BerechneZustandsvektor()
    {
        BerechneGeometrie();
        switch (Typ)
        {
            case Erster:
                _uc[0] = Knoten[0].Knotenfreiheitsgrade[0] * Cos + Knoten[0].Knotenfreiheitsgrade[1] * Sin;
                _uc[1] = Knoten[0].Knotenfreiheitsgrade[0] * -Sin + Knoten[0].Knotenfreiheitsgrade[1] * Cos;
                _uc[2] = Knoten[1].Knotenfreiheitsgrade[0] * Cos + Knoten[1].Knotenfreiheitsgrade[1] * Sin;
                _uc[3] = Knoten[1].Knotenfreiheitsgrade[0] * -Sin + Knoten[1].Knotenfreiheitsgrade[1] * Cos;
                _uc[4] = Knoten[1].Knotenfreiheitsgrade[2];
                break;
            case Zweiter:
                _uc[0] = Knoten[0].Knotenfreiheitsgrade[0] * Cos + Knoten[0].Knotenfreiheitsgrade[1] * Sin;
                _uc[1] = Knoten[0].Knotenfreiheitsgrade[0] * -Sin + Knoten[0].Knotenfreiheitsgrade[1] * Cos;
                _uc[2] = Knoten[0].Knotenfreiheitsgrade[2];
                _uc[3] = Knoten[1].Knotenfreiheitsgrade[0] * Cos + Knoten[1].Knotenfreiheitsgrade[1] * Sin;
                _uc[4] = Knoten[1].Knotenfreiheitsgrade[0] * -Sin + Knoten[1].Knotenfreiheitsgrade[1] * Cos;
                break;
        }

        return _uc;
    }

    public override double[] BerechneElementZustand(double z0, double z1)
    {
        return _uc;
    }

    public override Point BerechneSchwerpunkt()
    {
        if (!_modell.Elemente.TryGetValue(ElementId, out _element))
            throw new ModellAusnahme("\nBiegebalkenGelenk: " + ElementId + " nicht im Modell gefunden");
        return Schwerpunkt(_element);
    }
}