using FEBibliothek.Modell;
using FEBibliothek.Modell.abstrakte_Klassen;
using FEBibliothek.Werkzeuge;
using System;
using System.Windows;

namespace FE_Berechnungen.Tragwerksberechnung.Modelldaten
{
    public class Biegebalken : AbstraktBalken
    {
        protected AbstraktMaterial material;
        private AbstraktElement element;
        protected Querschnitt querschnitt;
        private readonly FEModell modell;

        private double[,] steifigkeitsMatrix = new double[6, 6];
        private readonly double[] massenMatrix = new double[6];

        //private readonly double[] shapeFunction = new double[6];
        private readonly double[] lastVektor = new double[6];
        //private readonly double gaussPoint = 1.0 / Math.Sqrt(3.0);

        // ... Konstruktor ........................................................
        public Biegebalken(string[] eKnotenIds, string eQuerschnittId, string eMaterialId, FEModell feModell)
        {
            modell = feModell;
            KnotenIds = eKnotenIds;
            ElementQuerschnittId = eQuerschnittId;
            ElementMaterialId = eMaterialId;
            ElementFreiheitsgrade = 3;
            KnotenProElement = 2;
            Knoten = new Knoten[2];
            ElementZustand = new double[6];
            ElementVerformungen = new double[6];
        }

        // ... berechne Elementmatrix ........................................
        public override double[,] BerechneElementMatrix()
        {
            steifigkeitsMatrix = BerechneLokaleMatrix();
            // ... transformiere lokale Matrix in globale Steifigkeitsmatrix ....
            steifigkeitsMatrix = TransformMatrix(steifigkeitsMatrix);
            return steifigkeitsMatrix;
        }

        // ... berechne lokale Steifigkeitsmatrix ...............................
        private double[,] BerechneLokaleMatrix()
        {
            BerechneGeometrie();
            var h2 = ElementMaterial.MaterialWerte[0] * ElementQuerschnitt.QuerschnittsWerte[1];          // EI
            var c1 = ElementMaterial.MaterialWerte[0] * ElementQuerschnitt.QuerschnittsWerte[0] / balkenLänge; // EA/L
            var c2 = (12.0 * h2) / balkenLänge / balkenLänge / balkenLänge;
            var c3 = (6.0 * h2) / balkenLänge / balkenLänge;
            var c4 = (4.0 * h2) / balkenLänge;
            var c5 = 0.5 * c4;

            double[,] lokaleMatrix = {{ c1,  0,  0, -c1,  0,  0},
                                     { 0,  c2,  c3,  0, -c2,  c3},
                                     { 0,  c3,  c4,  0, -c3,  c5},
                                     {-c1,  0,  0,  c1,  0,  0},
                                     { 0, -c2, -c3,  0,  c2, -c3},
                                     { 0,  c3,  c5,  0, -c3,  c4}};
            return lokaleMatrix;
        }

        // ....berechne diagonal Massenmatrix.................................
        public override double[] BerechneDiagonalMatrix()
        {
            if (ElementMaterial.MaterialWerte.Length < 3)
            {
                throw new ModellAusnahme("Biegebalken " + ElementId + ", spezifische Masse noch nicht definiert");
            }
            // Verschiebungen: Me = spezifische masse * fläche * 0.5*balkenLänge
            massenMatrix[0] = massenMatrix[1] = massenMatrix[3] = massenMatrix[4] =
                ElementMaterial.MaterialWerte[2] * ElementQuerschnitt.QuerschnittsWerte[0] * balkenLänge / 2;
            // Rotationsmassen = 0
            massenMatrix[2] = massenMatrix[5] = 0.0;
            return massenMatrix;
        }

        public double[] BerechneLastVektor(AbstraktLast ael, bool inElementCoordinateSystem)
        {
            BerechneGeometrie();
            for (var i = 0; i < lastVektor.Length; i++) { lastVektor[i] = 0.0; }

            switch (ael)
            {
                case LinienLast ll:
                    {
                        double na, nb, qa, qb;
                        if (!ll.IstInElementKoordinatenSystem())
                        {
                            na = ll.Lastwerte[0] * cos + ll.Lastwerte[0] * sin;
                            nb = ll.Lastwerte[2] * cos + ll.Lastwerte[2] * sin;
                            qa = ll.Lastwerte[1] * -sin + ll.Lastwerte[1] * cos;
                            qb = ll.Lastwerte[3] * -sin + ll.Lastwerte[3] * cos;
                        }
                        else
                        {
                            na = ll.Lastwerte[0];
                            nb = ll.Lastwerte[2];
                            qa = ll.Lastwerte[1];
                            qb = ll.Lastwerte[3];
                        }

                        lastVektor[0] = na * 0.5 * balkenLänge;
                        lastVektor[3] = nb * 0.5 * balkenLänge;

                        // konstante Linienlast
                        if (Math.Abs(qa - qb) < double.Epsilon)
                        {
                            lastVektor[1] = lastVektor[4] = qa * 0.5 * balkenLänge;
                            lastVektor[2] = qa * balkenLänge * balkenLänge / 12;
                            lastVektor[5] = -qa * balkenLänge * balkenLänge / 12;
                        }
                        // Dreieckslast steigend von a nach b
                        else if (Math.Abs(qa) < Math.Abs(qb))
                        {
                            lastVektor[1] = qa * 0.5 * balkenLänge + (qb - qa) * 3 / 20 * balkenLänge;
                            lastVektor[4] = qa * 0.5 * balkenLänge + (qb - qa) * 7 / 20 * balkenLänge;
                            lastVektor[2] = qa * balkenLänge * balkenLänge / 12 + (qb - qa) * balkenLänge * balkenLänge / 30;
                            lastVektor[5] = -qa * balkenLänge * balkenLänge / 12 - (qb - qa) * balkenLänge * balkenLänge / 20;
                        }
                        // Dreieckslast fallend von a nach b
                        else if (Math.Abs(qa) > Math.Abs(qb))
                        {
                            lastVektor[1] = qb * 0.5 * balkenLänge + (qa - qb) * 7 / 20 * balkenLänge;
                            lastVektor[4] = qb * 0.5 * balkenLänge + (qa - qb) * 3 / 20 * balkenLänge;
                            lastVektor[2] = qb * balkenLänge * balkenLänge / 12 + (qa - qb) * balkenLänge * balkenLänge / 20;
                            lastVektor[5] = -qb * balkenLänge * balkenLänge / 12 - (qa - qb) * balkenLänge * balkenLänge / 30;
                        }
                        break;
                    }

                case PunktLast pl:
                    {
                        double xLoad;
                        double yLoad;

                        if (!pl.IstInElementKoordinatenSystem())
                        {
                            xLoad = pl.Lastwerte[0] * cos + pl.Lastwerte[1] * sin;
                            yLoad = pl.Lastwerte[0] * -sin + pl.Lastwerte[1] * cos;
                        }
                        else
                        {
                            xLoad = pl.Lastwerte[0];
                            yLoad = pl.Lastwerte[1];
                        }

                        var a = pl.Offset * balkenLänge;
                        var b = balkenLänge - a;
                        lastVektor[0] = xLoad / 2;
                        lastVektor[1] = yLoad * b * b / balkenLänge / balkenLänge / balkenLänge * (balkenLänge + 2 * a);
                        lastVektor[2] = yLoad * a * b * b / balkenLänge / balkenLänge;
                        lastVektor[3] = xLoad / 2;
                        lastVektor[4] = yLoad * a * a / balkenLänge / balkenLänge / balkenLänge * (balkenLänge + 2 * b);
                        lastVektor[5] = -yLoad * a * a * b / balkenLänge / balkenLänge;
                        break;
                    }
                default:
                    throw new ModellAusnahme("Last " + ael + " wird in diesem Elementtyp nicht unterstützt ");
            }

            if (inElementCoordinateSystem) return lastVektor;
            var tmpLastVektor = new double[6];
            Array.Copy(lastVektor, tmpLastVektor, lastVektor.Length);
            // transforms the loadvector to the global coordinate system.
            lastVektor[0] = tmpLastVektor[0] * cos + tmpLastVektor[1] * -sin;
            lastVektor[1] = tmpLastVektor[0] * sin + tmpLastVektor[1] * cos;
            lastVektor[2] = tmpLastVektor[2];
            lastVektor[3] = tmpLastVektor[3] * cos + tmpLastVektor[4] * -sin;
            lastVektor[4] = tmpLastVektor[3] * sin + tmpLastVektor[4] * cos;
            lastVektor[5] = tmpLastVektor[5];
            return lastVektor;
        }

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

        // ... berechne Stabendkräfte ........................
        public override double[] BerechneStabendkräfte()
        {
            var lokaleMatrix = BerechneLokaleMatrix();
            var vektor = BerechneZustandsvektor();

            // contribution of the node deformations
            ElementZustand = MatrizenAlgebra.Mult(lokaleMatrix, vektor);

            // contribution of the beam loads
            foreach (var item in modell.PunktLasten)
            {
                if (!(item.Value is PunktLast punktLast)) continue;
                if (punktLast.ElementId != ElementId) continue;
                vektor = punktLast.BerechneLokalenLastVektor();
                for (var i = 0; i < vektor.Length; i++) ElementZustand[i] -= vektor[i];
            }

            foreach (var item in modell.ElementLasten)
            {
                if (!(item.Value is LinienLast linienLast)) continue;
                if (linienLast.ElementId != ElementId) continue;
                vektor = linienLast.BerechneLokalenLastVektor();
                for (var i = 0; i < vektor.Length; i++) ElementZustand[i] -= vektor[i];
            }
            ElementZustand[0] = -ElementZustand[0];
            ElementZustand[1] = -ElementZustand[1];
            ElementZustand[2] = -ElementZustand[2];
            return ElementZustand;
        }

        // ... berechne Verformungsvektor von Biegebalkenelementen .............
        public override double[] BerechneZustandsvektor()
        {
            BerechneGeometrie();
            const int ndof = 3;
            for (var i = 0; i < ndof; i++)
            {
                ElementVerformungen[i] = Knoten[0].Knotenfreiheitsgrade[i];
                ElementVerformungen[i + ndof] = Knoten[1].Knotenfreiheitsgrade[i];
            }
            // transformier in das lokale Koordinatensystem
            var temp0 = rotationsMatrix[0, 0] * ElementVerformungen[0]
                            + rotationsMatrix[1, 0] * ElementVerformungen[1];

            var temp1 = rotationsMatrix[0, 1] * ElementVerformungen[0]
                            + rotationsMatrix[1, 1] * ElementVerformungen[1];
            ElementVerformungen[0] = temp0;
            ElementVerformungen[1] = temp1;

            temp0 = rotationsMatrix[0, 0] * ElementVerformungen[3]
                  + rotationsMatrix[1, 0] * ElementVerformungen[4];
            temp1 = rotationsMatrix[0, 1] * ElementVerformungen[3]
                  + rotationsMatrix[1, 1] * ElementVerformungen[4];
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
            {
                for (var j = 0; j < ElementFreiheitsgrade; j++)
                    SystemIndizesElement[counter++] = Knoten[i].SystemIndizes[j];
            }
        }
        public override Point BerechneSchwerpunkt()
        {
            if (!modell.Elemente.TryGetValue(ElementId, out element))
            {
                throw new ModellAusnahme("Biegebalken: " + ElementId + " nicht im Modell gefunden");
            }
            return Schwerpunkt(element);
        }
    }
}