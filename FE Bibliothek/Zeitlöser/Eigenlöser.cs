using FEBibliothek.Gleichungslöser;
using FEBibliothek.Modell;
using FEBibliothek.Werkzeuge;
using System;

namespace FEBibliothek.Zeitlöser
{
    public class Eigenlöser
    {
        private const double RaleighFaktor = 1.0e-3;
        private const int SMax = 200;

        private readonly double[][] a;  // coefficients of matrix A
        private readonly double[][] b;  // coefficients of matrix B
        private readonly double[] y;    // y[s]  = A[-1] w[s-1]
        private readonly double[] w;    // w[s]  = m[s] z[s]
        private double[] z;             // z[s]  = B y[s]
        private double m2;              // m2[s] = 1 / (y[s,t] z[s])
        private double raleigh;         // r     = m2[s] y[s,t] w[s-1]
        private double deltaRaleigh;    // rNew - rOld

        private double[][] x;           // normalised eigenvectors x
        private double[][] p;           // w = B * x
        private double[] eigenwert;

        private readonly int[] profil; // row profile
        private int anzahlZustände;     // currently computed
        private readonly bool[] status; // true: displacement prescribed

        private int state, s, zeile;
        private readonly int dimension;

        //... Konstruktoren ......................................................
        public Eigenlöser(double[][] mA, double[][] mB,
                         int[] mProfil, bool[] mStatus, int mAnzahlZustände)
        {
            a = mA;
            b = mB;
            profil = mProfil;
            status = mStatus;
            anzahlZustände = mAnzahlZustände;

            dimension = a.Length;
            z = new double[dimension];
            w = new double[dimension];
            y = new double[dimension];
        }

        //... hole() .............................................................
        public double HolEigenwert(int index) { return eigenwert[index]; }
        public double[] HolEigenvektor(int index) { return x[index]; }

        //... löse Eigenzustände() ....................................................
        public void LöseEigenzustände()
        {
            // allokiere die Lösungsvektoren	
            x = new double[anzahlZustände][];
            p = new double[anzahlZustände][];
            for (var i = 0; i < anzahlZustände; i++)
            {
                x[i] = new double[dimension];
                p[i] = new double[dimension];
            }
            eigenwert = new double[anzahlZustände];

            // reduziere die Anzahl der Eigeenwerte auf die größtmögliche Anzahl
            var zähler = 0;
            for (zeile = 0; zeile < dimension; zeile++)
                if (status[zeile]) zähler++;
            if (anzahlZustände > dimension - zähler)
                anzahlZustände = dimension - zähler;

            var profilLöserStatus =
             new ProfillöserStatus(a, w, y, status, profil);

            // iteriere über die angegebene Zahl von Eigenzuständen
            for (state = 0; state < anzahlZustände; state++)
            {
                raleigh = 0;
                s = 0;
                // setz start vektor w0
                for (zeile = 0; zeile < dimension; zeile++)
                {
                    if (status[zeile]) w[zeile] = 0;
                    else w[zeile] = 1;
                }

                // start iteration für nächsten Eigenzustand
                double m;
                do
                {
                    // inkrementiere Iterationszähler
                    s++;
                    // test, ob Anzahl Iterationen ist größer als Smax
                    if (s > SMax)
                    {
                        throw new BerechnungAusnahme("Eigenlöser: zu viele Iterationen " + s);
                    }

                    // B-orthogonalisierung von w(s-1) in Bezug auf alle kleineren 
                    // Eigenvektoren x[0] bis x[state-1]
                    for (var i = 0; i < state; i++)
                    {
                        var c = 0.0;
                        // berechne c(i) und subtrahiere c(i)*p(i) von w
                        for (zeile = 0; zeile < dimension; zeile++)
                            if (!status[zeile]) c += w[zeile] * x[i][zeile];
                        for (zeile = 0; zeile < dimension; zeile++)
                            if (!status[zeile]) w[zeile] -= c * p[i][zeile];
                    }

                    // löse A * y(s) = w(s-1) for y(s)
                    profilLöserStatus.SetzRechteSeite(w);
                    profilLöserStatus.LösePrimal();

                    // berechne z(s) = B * y(s)
                    z = MatrizenAlgebra.Mult(b, y, status, profil);

                    // berechne m2 = 1 / (y[s] * z[s])
                    double sum = 0;
                    for (zeile = 0; zeile < dimension; zeile++)
                        if (!status[zeile]) sum += y[zeile] * z[zeile];
                    m2 = 1 / sum;


                    //berechne Rayleigh Quotient r = m2 * y(s)(T) * w(s-1)
                    // und die Differenz ( r(s) - r(s-1) )
                    sum = 0;
                    for (zeile = 0; zeile < dimension; zeile++)
                        if (!status[zeile]) sum += y[zeile] * w[zeile];
                    sum *= m2;
                    deltaRaleigh = sum - raleigh;
                    raleigh = sum;

                    //	berechne w(s) = m(s) * z(s)
                    m = Math.Sqrt(Math.Abs(m2));
                    for (zeile = 0; zeile < dimension; zeile++)
                        if (!status[zeile]) w[zeile] = m * z[zeile];

                    // fahre mit Iteration fort so lange wie die Veränderung des in Rayleigh Faktors (r(s)-r(s-1)
                    // größer ist als die Fehlerschranke
                } while (Math.Abs(deltaRaleigh) > Math.Abs(RaleighFaktor * raleigh));

                // speichere berechnete Eigenzustände und und Vektor p=w=Bx für B-orthogonalisierung
                eigenwert[state] = raleigh;
                for (zeile = 0; zeile < dimension; zeile++)
                {
                    x[state][zeile] = m * y[zeile];
                    p[state][zeile] = w[zeile];
                }
            }
        }
    }
}
