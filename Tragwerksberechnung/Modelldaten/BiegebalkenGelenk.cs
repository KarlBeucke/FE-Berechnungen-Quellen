using FEBibliothek.Modell;
using FEBibliothek.Modell.abstrakte_Klassen;
using FEBibliothek.Werkzeuge;
using System.Linq;
using System.Windows;

namespace FE_Berechnungen.Tragwerksberechnung.Modelldaten;

public class BiegebalkenGelenk : AbstraktBalken
{
    private readonly FeModell modell;
    private AbstraktElement element;
    protected Knoten knoten;
    private readonly double[] massenMatrix = new double[6];

    private readonly int erster = 1;
    private readonly int zweiter = 2;

    // temp Variable für ein Gelenk
    private double invkll;
    private double[,] steifigkeitsMatrix = new double[6, 6];
    private double[,] redSteifigkeitsMatrix = new double[5, 5];

    private readonly double[,] kcc = new double[5, 5];
    private readonly double[] klc = new double[5];
    private readonly double[] kcl = new double[5];
    private readonly double[] kll = new double[1];
    private readonly double[] kllxklc = new double[5];
    private readonly double[,] kclxkllxklc = new double[5, 5];
    private readonly double[] uc = new double[5];
    private double[] kcxuc = new double[5];

    // dof Identifikatoren
    private readonly int[] clow = { 0, 1, 3, 4, 5 };
    private readonly int[] llow = { 2 };
    private readonly int[] chigh = { 0, 1, 2, 3, 4 };
    private readonly int[] lhigh = { 5 };

    private readonly int[] c;
    private readonly int[] l;

    public BiegebalkenGelenk(string[] eKnotenIds, string eMaterialId, string eQuerschnittId, FeModell feModell, int typ)
    {
        modell = feModell;
        ElementFreiheitsgrade = 3;
        KnotenIds = eKnotenIds;
        ElementMaterialId = eMaterialId;
        ElementQuerschnittId = eQuerschnittId;
        KnotenProElement = 2;
        Knoten = new Knoten[2];
        Typ = typ;
        ElementZustand = new double[6];
        ElementVerformungen = new double[6];

        Typ = typ;
        if (Typ == erster)
        {
            c = clow;
            l = llow;
        }
        else if (Typ == zweiter)
        {
            c = chigh;
            l = lhigh;
        }
        else throw new ModellAusnahme("BiegebalkenGelenk: Gelenktyp wurde nicht erkannt!");
    }

    // ... berechne lokale Steigigkeit ................................
    private double[,] BerechneLokaleSteifigkeitsmatrix()
    {
        BerechneGeometrie();
        var h2 = ElementMaterial.MaterialWerte[0] * ElementQuerschnitt.QuerschnittsWerte[1];          // EI
        var c1 = ElementMaterial.MaterialWerte[0] * ElementQuerschnitt.QuerschnittsWerte[0] / balkenLänge; // AE/L
        var c2 = (12.0 * h2) / balkenLänge / balkenLänge / balkenLänge;
        var c3 = (6.0 * h2) / balkenLänge / balkenLänge;
        var c4 = (4.0 * h2) / balkenLänge;
        var c5 = 0.5 * c4;

        double[,] lokaleSteifigkeitsmatrix = {{ c1,  0,  0, -c1,  0,  0},
            { 0,  c2,  c3,  0, -c2,  c3},
            { 0,  c3,  c4,  0, -c3,  c5},
            {-c1,  0,  0,  c1,  0,  0},
            { 0, -c2, -c3,  0,  c2, -c3},
            { 0,  c3,  c5,  0, -c3,  c4}};
        return lokaleSteifigkeitsmatrix;
    }

    //private double[] ComputeLoadVector(AbstractElementLoad ael, bool inElementCoordinateSystem)
    //{
    //    var superLoadVector = ComputeLoadVector(ael, inElementCoordinateSystem);
    //    Array.Copy(superLoadVector, 0, p, 0, 6); //length 6, calculates the kcc, kcl ... matrices for this element

    //    if (inElementCoordinateSystem)
    //        ComputeLocalMatrix();
    //    else
    //        ComputeMatrix();
    //    if (Type == first)
    //    {
    //        pc[0] = p[0]; pc[1] = p[1]; pc[2] = p[3]; pc[3] = p[4]; pc[4] = p[5]; pl[0] = p[2];
    //    }
    //    else if (Type == second)
    //    {
    //        pc[0] = p[0]; pc[1] = p[1]; pc[2] = p[2]; pc[3] = p[3]; pc[4] = p[4]; pl[0] = p[5];
    //    }

    //    for (var k = 0; k < 5; k++) kclxinvkll[k] = kcl[k] * invkll;
    //    for (var i = 0; i < 5; i++)
    //        for (var j = 0; j < 5; j++) kclxinvkllxpl[i] += kclxinvkll[i] * pl[j];
    //    for (var k = 0; k < 5; k++) kclxinvkllxpl[k] = kclxinvkllxpl[k] * -1;
    //    for (var k = 0; k < 5; k++) loadVector[k] = pc[k] + kclxinvkllxpl[k];
    //    return loadVector;
    //}

    private double[,] TransformMatrix(double[,] matrix)
    {
        var elementFreiheitsgrade = ElementFreiheitsgrade;
        for (var i = 0; i < matrix.GetLength(0); i += elementFreiheitsgrade)
        {
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

                var e11 = rotationsMatrix[0, 0];
                var e12 = rotationsMatrix[0, 1];
                var e21 = rotationsMatrix[1, 0];
                var e22 = rotationsMatrix[1, 1];

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
        }
        return matrix;
    }

    public override double[] BerechneStabendkräfte()
    {
        var matrix = BerechneLokaleReduzierteMatrix();
        ElementVerformungen = BerechneZustandsvektor();

        // Beitrag der Knotenverformungen
        kcxuc = MatrizenAlgebra.Mult(matrix, ElementVerformungen);

        // Beitrag der Balkenlasten
        foreach (var last in modell.ElementLasten.Select(item => item.Value))
        {
            if (last is AbstraktElementLast linienLast)
            {
                var ll = (LinienLast)linienLast;
                ElementVerformungen = ll.BerechneLokalenLastVektor();
                for (var k = 0; k < 5; k++) kcxuc[k] -= ElementVerformungen[k];
            }

            if (!(last is AbstraktElementLast punktLast)) continue;
            {
                var last1 = (PunktLast)punktLast;
                ElementVerformungen = last1.BerechneLokalenLastVektor();
                for (var k = 0; k < 5; k++) kcxuc[k] -= ElementVerformungen[k];
            }
        }

        if (Typ == erster)
        {
            ElementZustand[0] = -kcxuc[0];
            ElementZustand[1] = -kcxuc[1];
            ElementZustand[2] = 0.0;
            ElementZustand[3] = kcxuc[2];
            ElementZustand[4] = kcxuc[3];
            ElementZustand[5] = kcxuc[4];
        }
        else if (Typ == zweiter)
        {
            ElementZustand[0] = -kcxuc[0];
            ElementZustand[1] = -kcxuc[1];
            ElementZustand[2] = -kcxuc[2];
            ElementZustand[3] = kcxuc[3];
            ElementZustand[4] = kcxuc[4];
            ElementZustand[5] = 0.0;
        }
        return ElementZustand;
    }

    // ... berechne Verformungsvektor für Rahmenelemente .............
    public int[] HolSystemIndizes()
    {
        int[] indizes;
        if (Typ == erster)
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

        if (Typ != zweiter) throw new ModellAusnahme("BiegebalkenGelenk GetSystemIndices: ungültiger Gelenktyp");
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
         *  |Kcc Klc|
         *  |       |
         *  |Kcl Kll|
         *
         *  | Kcc - Kcl*Kll^-1*klc |
         */
    private double[,] KondensierMatrix(double[,] ke)
    {
        MatrizenAlgebra.ExtractSubMatrix(ke, kcc, c);
        MatrizenAlgebra.ExtractSubMatrix(ke, kcl, c, l);
        MatrizenAlgebra.ExtractSubMatrix(ke, klc, l, c);
        MatrizenAlgebra.ExtractSubMatrix(ke, kll, l, l);
        invkll = 1 / kll[0];
        for (var k = 0; k < 5; k++) kllxklc[k] = invkll * klc[k];
        for (var i = 0; i < 5; i++)
            for (var j = 0; j < 5; j++) kclxkllxklc[i, j] = kcl[j] * kllxklc[i];
        for (var i = 0; i < 5; i++)
            for (var j = 0; j < 5; j++) redSteifigkeitsMatrix[i, j] = kcc[i, j] - kclxkllxklc[i, j];
        //MatrixAlgebra.Subtract(redStiffnessMatrix, kcc, kclxkllxklc);
        return redSteifigkeitsMatrix;
    }

    private double[,] BerechneLokaleReduzierteMatrix()
    {
        return KondensierMatrix(BerechneLokaleSteifigkeitsmatrix());
    }
    public override double[,] BerechneElementMatrix()
    {
        steifigkeitsMatrix = BerechneLokaleSteifigkeitsmatrix();
        // transform local matrix to compute global stiffness
        steifigkeitsMatrix = TransformMatrix(steifigkeitsMatrix);

        redSteifigkeitsMatrix = KondensierMatrix(steifigkeitsMatrix);
        return redSteifigkeitsMatrix;
    }
    public override double[] BerechneDiagonalMatrix()
    {
        if (ElementMaterial.MaterialWerte.Length < 3)
        {
            throw new ModellAusnahme("BiegebalkenGelenk " + ElementId + ", spezifische Masse noch nicht definiert");
        }
        // Me = speyifische masse * fläche * 0.5*balkenlänge
        massenMatrix[0] = massenMatrix[1] = massenMatrix[3] = massenMatrix[4] =
            ElementMaterial.MaterialWerte[2] * ElementQuerschnitt.QuerschnittsWerte[0] * balkenLänge / 2;
        massenMatrix[2] = massenMatrix[5] = 1;
        return massenMatrix;
    }
    public override void SetzElementSystemIndizes()
    {
        SystemIndizesElement = new int[5];
        var counter = 0;
        if (Typ == erster)
        {
            for (var j = 0; j < ElementFreiheitsgrade - 1; j++)
                SystemIndizesElement[counter++] = Knoten[0].SystemIndizes[j];
            for (var j = 0; j < ElementFreiheitsgrade; j++)
                SystemIndizesElement[counter++] = Knoten[1].SystemIndizes[j];
        }
        else if (Typ == zweiter)
        {
            for (var j = 0; j < ElementFreiheitsgrade; j++)
                SystemIndizesElement[counter++] = Knoten[0].SystemIndizes[j];
            for (var j = 0; j < ElementFreiheitsgrade - 1; j++)
                SystemIndizesElement[counter++] = Knoten[1].SystemIndizes[j];
        }
        else throw new ModellAusnahme("BiegebalkenGelenk SetSystemIndices: Gelenktyp wurde nicht erkannt!");
    }
    public override double[] BerechneZustandsvektor()
    {
        BerechneGeometrie();
        if (Typ == erster)
        {
            uc[0] = Knoten[0].Knotenfreiheitsgrade[0] * cos + Knoten[0].Knotenfreiheitsgrade[1] * sin;
            uc[1] = Knoten[0].Knotenfreiheitsgrade[0] * -sin + Knoten[0].Knotenfreiheitsgrade[1] * cos;
            uc[2] = Knoten[1].Knotenfreiheitsgrade[0] * cos + Knoten[1].Knotenfreiheitsgrade[1] * sin;
            uc[3] = Knoten[1].Knotenfreiheitsgrade[0] * -sin + Knoten[1].Knotenfreiheitsgrade[1] * cos;
            uc[4] = Knoten[1].Knotenfreiheitsgrade[2];
        }
        else if (Typ == zweiter)
        {
            uc[0] = Knoten[0].Knotenfreiheitsgrade[0] * cos + Knoten[0].Knotenfreiheitsgrade[1] * sin;
            uc[1] = Knoten[0].Knotenfreiheitsgrade[0] * -sin + Knoten[0].Knotenfreiheitsgrade[1] * cos;
            uc[2] = Knoten[0].Knotenfreiheitsgrade[2];
            uc[3] = Knoten[1].Knotenfreiheitsgrade[0] * cos + Knoten[1].Knotenfreiheitsgrade[1] * sin;
            uc[4] = Knoten[1].Knotenfreiheitsgrade[0] * -sin + Knoten[1].Knotenfreiheitsgrade[1] * cos;
        }
        return uc;
    }
    public override double[] BerechneElementZustand(double z0, double z1)
    {
        return uc;
    }

    public override Point BerechneSchwerpunkt()
    {
        if (!modell.Elemente.TryGetValue(ElementId, out element))
        {
            throw new ModellAusnahme("BiegebalkenGelenk: " + ElementId + " nicht im Modell gefunden");
        }
        return Schwerpunkt(element);
    }
}