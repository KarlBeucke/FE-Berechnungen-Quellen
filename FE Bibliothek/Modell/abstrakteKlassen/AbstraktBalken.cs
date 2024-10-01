namespace FEBibliothek.Modell.abstrakteKlassen
{
    public abstract class AbstraktBalken : AbstraktLinear2D2
    {
        public abstract double[] BerechneStabendkräfte();

        public double[] BerechneLastVektor(AbstraktLast ael, bool inElementCoordinateSystem)
        {
            BerechneGeometrie();
            var lastVektor = new double[6];
            for (var i = 0; i < lastVektor.Length; i++) { lastVektor[i] = 0.0; }

            switch (ael)
            {
                case AbstraktLinienlast ll:
                    {
                        double na, nb, qa, qb;
                        if (!ll.IstInElementKoordinatenSystem())
                        {
                            na = ll.Lastwerte[0] * Cos + ll.Lastwerte[0] * Sin;
                            nb = ll.Lastwerte[2] * Cos + ll.Lastwerte[2] * Sin;
                            qa = ll.Lastwerte[1] * -Sin + ll.Lastwerte[1] * Cos;
                            qb = ll.Lastwerte[3] * -Sin + ll.Lastwerte[3] * Cos;
                        }
                        else
                        {
                            na = ll.Lastwerte[0];
                            nb = ll.Lastwerte[2];
                            qa = ll.Lastwerte[1];
                            qb = ll.Lastwerte[3];
                        }

                        lastVektor[0] = na * 0.5 * BalkenLänge;
                        lastVektor[3] = nb * 0.5 * BalkenLänge;

                        // konstante Linienlast
                        if (Math.Abs(qa - qb) < double.Epsilon)
                        {
                            lastVektor[1] = lastVektor[4] = qa * 0.5 * BalkenLänge;
                            lastVektor[2] = qa * BalkenLänge * BalkenLänge / 12;
                            lastVektor[5] = -qa * BalkenLänge * BalkenLänge / 12;
                        }
                        // Dreieckslast steigend von a nach b
                        else if (Math.Abs(qa) < Math.Abs(qb))
                        {
                            lastVektor[1] = qa * 0.5 * BalkenLänge + (qb - qa) * 3 / 20 * BalkenLänge;
                            lastVektor[4] = qa * 0.5 * BalkenLänge + (qb - qa) * 7 / 20 * BalkenLänge;
                            lastVektor[2] = qa * BalkenLänge * BalkenLänge / 12 + (qb - qa) * BalkenLänge * BalkenLänge / 30;
                            lastVektor[5] = -qa * BalkenLänge * BalkenLänge / 12 - (qb - qa) * BalkenLänge * BalkenLänge / 20;
                        }
                        // Dreieckslast fallend von a nach b
                        else if (Math.Abs(qa) > Math.Abs(qb))
                        {
                            lastVektor[1] = qb * 0.5 * BalkenLänge + (qa - qb) * 7 / 20 * BalkenLänge;
                            lastVektor[4] = qb * 0.5 * BalkenLänge + (qa - qb) * 3 / 20 * BalkenLänge;
                            lastVektor[2] = qb * BalkenLänge * BalkenLänge / 12 + (qa - qb) * BalkenLänge * BalkenLänge / 20;
                            lastVektor[5] = -qb * BalkenLänge * BalkenLänge / 12 - (qa - qb) * BalkenLänge * BalkenLänge / 30;
                        }
                        break;
                    }

                case AbstraktElementLast pl:
                    {
                        double xLoad;
                        double yLoad;

                        if (!pl.IstInElementKoordinatenSystem())
                        {
                            xLoad = pl.Lastwerte[0] * Cos + pl.Lastwerte[1] * Sin;
                            yLoad = pl.Lastwerte[0] * -Sin + pl.Lastwerte[1] * Cos;
                        }
                        else
                        {
                            xLoad = pl.Lastwerte[0];
                            yLoad = pl.Lastwerte[1];
                        }

                        var a = pl.Offset * BalkenLänge;
                        var b = BalkenLänge - a;
                        lastVektor[0] = xLoad / 2;
                        lastVektor[1] = yLoad * b * b / BalkenLänge / BalkenLänge / BalkenLänge * (BalkenLänge + 2 * a);
                        lastVektor[2] = yLoad * a * b * b / BalkenLänge / BalkenLänge;
                        lastVektor[3] = xLoad / 2;
                        lastVektor[4] = yLoad * a * a / BalkenLänge / BalkenLänge / BalkenLänge * (BalkenLänge + 2 * b);
                        lastVektor[5] = -yLoad * a * a * b / BalkenLänge / BalkenLänge;
                        break;
                    }
                default:
                    throw new ModellAusnahme("\nLast " + ael + " wird in diesem Elementtyp nicht unterstützt ");
            }

            if (inElementCoordinateSystem) return lastVektor;
            var tmpLastVektor = new double[6];
            Array.Copy(lastVektor, tmpLastVektor, lastVektor.Length);
            // transformiert den Lastvektor in das globale Koordinatensystem
            lastVektor[0] = tmpLastVektor[0] * Cos + tmpLastVektor[1] * -Sin;
            lastVektor[1] = tmpLastVektor[0] * Sin + tmpLastVektor[1] * Cos;
            lastVektor[2] = tmpLastVektor[2];
            lastVektor[3] = tmpLastVektor[3] * Cos + tmpLastVektor[4] * -Sin;
            lastVektor[4] = tmpLastVektor[3] * Sin + tmpLastVektor[4] * Cos;
            lastVektor[5] = tmpLastVektor[5];
            return lastVektor;
        }
    }
}
