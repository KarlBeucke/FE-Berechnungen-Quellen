using FEBibliothek.Modell;
using FEBibliothek.Modell.abstrakte_Klassen;
using FEBibliothek.Werkzeuge;
using System.Windows;

namespace FE_Berechnungen.Tragwerksberechnung.Modelldaten;

public class Biegebalken : AbstraktBalken
{
    private readonly double[] _massenMatrix = new double[6];
    private readonly FeModell _modell;
    private AbstraktElement _element;

    private double _emodul, _masse, _fläche, _trägheitsmoment;
    private double[,] _steifigkeitsMatrix = new double[6, 6];
    protected AbstraktMaterial Material;
    protected Querschnitt Querschnitt;

    //private readonly double[] shapeFunction = new double[6];
    //private readonly double[] _lastVektor = new double[6];
    //private readonly double gaussPoint = 1.0 / Math.Sqrt(3.0);

    public Biegebalken(string[] eKnotenIds, string eMaterialId, string eQuerschnittId, FeModell feModell)
    {
        _modell = feModell;
        KnotenIds = eKnotenIds;
        ElementQuerschnittId = eQuerschnittId;
        ElementMaterialId = eMaterialId;
        ElementFreiheitsgrade = 3;
        KnotenProElement = 2;
        Knoten = new Knoten[2];
        ElementZustand = new double[6];
        ElementVerformungen = new double[6];
    }

    public override double[,] BerechneElementMatrix()
    {
        _steifigkeitsMatrix = BerechneLokaleMatrix();
        // transformiere lokale Matrix in globale Steifigkeitsmatrix
        _steifigkeitsMatrix = TransformMatrix(_steifigkeitsMatrix);
        return _steifigkeitsMatrix;
    }

    // berechne lokale Steifigkeitsmatrix
    private double[,] BerechneLokaleMatrix()
    {
        BerechneGeometrie();

        if (!_modell.Material.TryGetValue(ElementMaterialId, out var material)) return null;
        _emodul = E == 0 ? material.MaterialWerte[0] : E;
        if (!_modell.Querschnitt.TryGetValue(ElementQuerschnittId, out var querschnitt)) return null;
        _fläche = A == 0 ? querschnitt.QuerschnittsWerte[0] : A;
        _trägheitsmoment = I == 0 ? querschnitt.QuerschnittsWerte[1] : I;
        var h2 = _emodul * _trägheitsmoment; // EI
        var c1 = _emodul * _fläche / BalkenLänge; // EA/L
        var c2 = 12.0 * h2 / BalkenLänge / BalkenLänge / BalkenLänge;
        var c3 = 6.0 * h2 / BalkenLänge / BalkenLänge;
        var c4 = 4.0 * h2 / BalkenLänge;
        var c5 = 0.5 * c4;

        double[,] lokaleMatrix =
        {
            { c1, 0, 0, -c1, 0, 0 },
            { 0, c2, c3, 0, -c2, c3 },
            { 0, c3, c4, 0, -c3, c5 },
            { -c1, 0, 0, c1, 0, 0 },
            { 0, -c2, -c3, 0, c2, -c3 },
            { 0, c3, c5, 0, -c3, c4 }
        };
        return lokaleMatrix;
    }

    // berechne diagonal Massenmatrix
    public override double[] BerechneDiagonalMatrix()
    {
        if (ElementMaterial.MaterialWerte.Length < 3 && M == 0)
            throw new ModellAusnahme("\nBiegebalken " + ElementId + ", spezifische Masse noch nicht definiert");

        if (!_modell.Material.TryGetValue(ElementMaterialId, out var material)) return null;
        _masse = M == 0 ? material.MaterialWerte[2] : M;
        if (!_modell.Querschnitt.TryGetValue(ElementQuerschnittId, out var querschnitt)) return null;
        _fläche = A == 0 ? querschnitt.QuerschnittsWerte[0] : A;

        // Verschiebungen: Me = spezifische masse * fläche * 0.5*balkenLänge
        _massenMatrix[0] = _massenMatrix[1] = _massenMatrix[3] = _massenMatrix[4] = _masse * _fläche * BalkenLänge / 2;
        // Rotationsmassen = 0
        _massenMatrix[2] = _massenMatrix[5] = 0.0;
        return _massenMatrix;
    }

    //public double[] BerechneLastVektor(AbstraktLast ael, bool inElementCoordinateSystem)
    //{
    //    BerechneGeometrie();
    //    for (var i = 0; i < _lastVektor.Length; i++) { _lastVektor[i] = 0.0; }

    //    switch (ael)
    //    {
    //        case LinienLast ll:
    //            {
    //                double na, nb, qa, qb;
    //                if (!ll.IstInElementKoordinatenSystem())
    //                {
    //                    na = ll.Lastwerte[0] * Cos + ll.Lastwerte[0] * Sin;
    //                    nb = ll.Lastwerte[2] * Cos + ll.Lastwerte[2] * Sin;
    //                    qa = ll.Lastwerte[1] * -Sin + ll.Lastwerte[1] * Cos;
    //                    qb = ll.Lastwerte[3] * -Sin + ll.Lastwerte[3] * Cos;
    //                }
    //                else
    //                {
    //                    na = ll.Lastwerte[0];
    //                    nb = ll.Lastwerte[2];
    //                    qa = ll.Lastwerte[1];
    //                    qb = ll.Lastwerte[3];
    //                }

    //                _lastVektor[0] = na * 0.5 * BalkenLänge;
    //                _lastVektor[3] = nb * 0.5 * BalkenLänge;

    //                // konstante Linienlast
    //                if (Math.Abs(qa - qb) < double.Epsilon)
    //                {
    //                    _lastVektor[1] = _lastVektor[4] = qa * 0.5 * BalkenLänge;
    //                    _lastVektor[2] = qa * BalkenLänge * BalkenLänge / 12;
    //                    _lastVektor[5] = -qa * BalkenLänge * BalkenLänge / 12;
    //                }
    //                // Dreieckslast steigend von a nach b
    //                else if (Math.Abs(qa) < Math.Abs(qb))
    //                {
    //                    _lastVektor[1] = qa * 0.5 * BalkenLänge + (qb - qa) * 3 / 20 * BalkenLänge;
    //                    _lastVektor[4] = qa * 0.5 * BalkenLänge + (qb - qa) * 7 / 20 * BalkenLänge;
    //                    _lastVektor[2] = qa * BalkenLänge * BalkenLänge / 12 + (qb - qa) * BalkenLänge * BalkenLänge / 30;
    //                    _lastVektor[5] = -qa * BalkenLänge * BalkenLänge / 12 - (qb - qa) * BalkenLänge * BalkenLänge / 20;
    //                }
    //                // Dreieckslast fallend von a nach b
    //                else if (Math.Abs(qa) > Math.Abs(qb))
    //                {
    //                    _lastVektor[1] = qb * 0.5 * BalkenLänge + (qa - qb) * 7 / 20 * BalkenLänge;
    //                    _lastVektor[4] = qb * 0.5 * BalkenLänge + (qa - qb) * 3 / 20 * BalkenLänge;
    //                    _lastVektor[2] = qb * BalkenLänge * BalkenLänge / 12 + (qa - qb) * BalkenLänge * BalkenLänge / 20;
    //                    _lastVektor[5] = -qb * BalkenLänge * BalkenLänge / 12 - (qa - qb) * BalkenLänge * BalkenLänge / 30;
    //                }
    //                break;
    //            }

    //        case PunktLast pl:
    //            {
    //                double xLoad;
    //                double yLoad;

    //                if (!pl.IstInElementKoordinatenSystem())
    //                {
    //                    xLoad = pl.Lastwerte[0] * Cos + pl.Lastwerte[1] * Sin;
    //                    yLoad = pl.Lastwerte[0] * -Sin + pl.Lastwerte[1] * Cos;
    //                }
    //                else
    //                {
    //                    xLoad = pl.Lastwerte[0];
    //                    yLoad = pl.Lastwerte[1];
    //                }

    //                var a = pl.Offset * BalkenLänge;
    //                var b = BalkenLänge - a;
    //                _lastVektor[0] = xLoad / 2;
    //                _lastVektor[1] = yLoad * b * b / BalkenLänge / BalkenLänge / BalkenLänge * (BalkenLänge + 2 * a);
    //                _lastVektor[2] = yLoad * a * b * b / BalkenLänge / BalkenLänge;
    //                _lastVektor[3] = xLoad / 2;
    //                _lastVektor[4] = yLoad * a * a / BalkenLänge / BalkenLänge / BalkenLänge * (BalkenLänge + 2 * b);
    //                _lastVektor[5] = -yLoad * a * a * b / BalkenLänge / BalkenLänge;
    //                break;
    //            }
    //        default:
    //            throw new ModellAusnahme("Last " + ael + " wird in diesem Elementtyp nicht unterstützt ");
    //    }

    //    if (inElementCoordinateSystem) return _lastVektor;
    //    var tmpLastVektor = new double[6];
    //    Array.Copy(_lastVektor, tmpLastVektor, _lastVektor.Length);
    //    // transformiert den Lastvektor in das globale Koordinatensystem
    //    _lastVektor[0] = tmpLastVektor[0] * Cos + tmpLastVektor[1] * -Sin;
    //    _lastVektor[1] = tmpLastVektor[0] * Sin + tmpLastVektor[1] * Cos;
    //    _lastVektor[2] = tmpLastVektor[2];
    //    _lastVektor[3] = tmpLastVektor[3] * Cos + tmpLastVektor[4] * -Sin;
    //    _lastVektor[4] = tmpLastVektor[3] * Sin + tmpLastVektor[4] * Cos;
    //    _lastVektor[5] = tmpLastVektor[5];
    //    return _lastVektor;
    //}

    //private void GetShapeFunctionValues(double z)
    //{
    //    ComputeGeometry();
    //    if (z < 0 || z > 1)
    //        throw new ModellAusnahme("Biegebalken: Formfunktion ungültig : " + z + " liegt außerhalb des Elements");
    //    // Shape functions. 0 <= z <= 1
    //    shapeFunction[0] = 1 - z;                           //x translation - low node
    //    shapeFunction[1] = 2 * z * z * z - 3 * z * z + 1;   //y translation - low node
    //    shapeFunction[2] = length * z * (z - 1) * (z - 1);  //z rotation - low node
    //    shapeFunction[3] = z;                               //x translation - high node
    //    shapeFunction[4] = z * z * (3 - 2 * z);             //y translation - high node
    //    shapeFunction[5] = length * z * z * (z - 1);        //z rotation - high node
    //}

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

    // berechne Stabendkräfte
    public override double[] BerechneStabendkräfte()
    {
        var lokaleMatrix = BerechneLokaleMatrix();
        var vektor = BerechneZustandsvektor();

        // Beitrag der Knotendeformationen
        ElementZustand = MatrizenAlgebra.Mult(lokaleMatrix, vektor);

        // Beitrag der Balkenlasten
        foreach (var item in _modell.PunktLasten)
        {
            if (item.Value is not PunktLast punktLast) continue;
            if (punktLast.ElementId != ElementId) continue;
            vektor = punktLast.BerechneLokalenLastVektor();
            for (var i = 0; i < vektor.Length; i++) ElementZustand[i] -= vektor[i];
        }

        foreach (var item in _modell.ElementLasten)
        {
            if (item.Value is not LinienLast linienLast) continue;
            if (linienLast.ElementId != ElementId) continue;
            vektor = linienLast.BerechneLokalenLastVektor();
            for (var i = 0; i < vektor.Length; i++) ElementZustand[i] -= vektor[i];
        }

        ElementZustand[0] = -ElementZustand[0];
        ElementZustand[1] = -ElementZustand[1];
        ElementZustand[2] = -ElementZustand[2];
        return ElementZustand;
    }

    // berechne Verformungsvektor von Biegebalkenelementen
    public override double[] BerechneZustandsvektor()
    {
        BerechneGeometrie();
        const int ndof = 3;
        for (var i = 0; i < ndof; i++)
        {
            ElementVerformungen[i] = Knoten[0].Knotenfreiheitsgrade[i];
            ElementVerformungen[i + ndof] = Knoten[1].Knotenfreiheitsgrade[i];
        }

        // transformiere in das lokale Koordinatensystem
        var temp0 = RotationsMatrix[0, 0] * ElementVerformungen[0]
                    + RotationsMatrix[1, 0] * ElementVerformungen[1];

        var temp1 = RotationsMatrix[0, 1] * ElementVerformungen[0]
                    + RotationsMatrix[1, 1] * ElementVerformungen[1];
        ElementVerformungen[0] = temp0;
        ElementVerformungen[1] = temp1;

        temp0 = RotationsMatrix[0, 0] * ElementVerformungen[3]
                + RotationsMatrix[1, 0] * ElementVerformungen[4];
        temp1 = RotationsMatrix[0, 1] * ElementVerformungen[3]
                + RotationsMatrix[1, 1] * ElementVerformungen[4];
        ElementVerformungen[3] = temp0;
        ElementVerformungen[4] = temp1;

        return ElementVerformungen;
    }

    public override double[] BerechneElementZustand(double z0, double z1)
    {
        return ElementZustand;
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
            throw new ModellAusnahme("\nBiegebalken: " + ElementId + " nicht im Modell gefunden");
        return Schwerpunkt(_element);
    }
}