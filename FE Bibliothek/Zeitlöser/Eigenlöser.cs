using FEBibliothek.Gleichungslöser;

namespace FEBibliothek.Zeitlöser
{
    public class Eigenlöser
    {
        // A*xi =  lambdai * B*xi
        private const double RaleighFaktor = 1.0e-3;
        private const int SMax = 200;

        private readonly double[][] _a;  // Koeffizienten der Matrix A
        private readonly double[][] _b;  // Koeffizienten der Matrix B
        private readonly double[] _y;    // y[s]  = A[-1] w[s-1]
        private readonly double[] _w;    // w[s]  = m[s] z[s]
        private double[] _z;             // z[s]  = B y[s]
        private double _m2;              // m2[s] = 1 / (y[s,t] z[s])
        private double _raleigh;         // r     = m2[s] y[s,t] w[s-1]
        private double _deltaRaleigh;    // rNew - rOld

        private double[][] _x;           // normalisierte Eigenvektoren x
        private double[][] _p;           // w = B * x
        private double[] _eigenwert;

        private readonly int[] _profil;  // Zeilenprofile
        private int _anzahlZustände;     // aktuell berechnet
        private readonly bool[] _status; // true: eingeprägte Verformungen

        private int _state, _s, _zeile;
        private readonly int _dimension;

        public Eigenlöser(double[][] mA, double[][] mB,
                         int[] mProfil, bool[] mStatus, int mAnzahlZustände)
        {
            _a = mA;
            _b = mB;
            _profil = mProfil;
            _status = mStatus;
            _anzahlZustände = mAnzahlZustände;

            _dimension = _a.Length;
            _z = new double[_dimension];
            _w = new double[_dimension];
            _y = new double[_dimension];
        }

        public double HolEigenwert(int index) { return _eigenwert[index]; }
        public double[] HolEigenvektor(int index) { return _x[index]; }

        // löse Eigenzustände()
        public void LöseEigenzustände()
        {
            // allokiere die Lösungsvektoren	
            _x = new double[_anzahlZustände][];
            _p = new double[_anzahlZustände][];
            for (var i = 0; i < _anzahlZustände; i++)
            {
                _x[i] = new double[_dimension];
                _p[i] = new double[_dimension];
            }
            _eigenwert = new double[_anzahlZustände];

            // reduziere die Anzahl der Eigenwerte auf die größtmögliche Anzahl
            var zähler = 0;
            for (_zeile = 0; _zeile < _dimension; _zeile++)
                if (_status[_zeile]) zähler++;
            if (_anzahlZustände > _dimension - zähler)
                _anzahlZustände = _dimension - zähler;

            var profilLöserStatus =
             new ProfillöserStatus(_a, _w, _y, _status, _profil);

            // iteriere über die angegebene Zahl von Eigenzuständen
            for (_state = 0; _state < _anzahlZustände; _state++)
            {
                _raleigh = 0;
                _s = 0;
                // setz start vektor w0
                for (_zeile = 0; _zeile < _dimension; _zeile++)
                {
                    if (_status[_zeile]) _w[_zeile] = 0;
                    else _w[_zeile] = 1;
                }

                // start iteration für nächsten Eigenzustand
                double m;
                do
                {
                    // inkrementiere Iterationszähler
                    _s++;
                    // test, ob Anzahl Iterationen ist größer als Smax
                    if (_s > SMax)
                    {
                        throw new BerechnungAusnahme("\nEigenlöser: zu viele Iterationen " + _s);
                    }

                    // B-Orthogonalisierung von w(s-1) in Bezug auf alle kleineren 
                    // Eigenvektoren x[0] bis x[state-1]
                    for (var i = 0; i < _state; i++)
                    {
                        var c = 0.0;
                        // berechne c(i) und subtrahiere c(i)*p(i) von w
                        for (_zeile = 0; _zeile < _dimension; _zeile++)
                            if (!_status[_zeile]) c += _w[_zeile] * _x[i][_zeile];
                        for (_zeile = 0; _zeile < _dimension; _zeile++)
                            if (!_status[_zeile]) _w[_zeile] -= c * _p[i][_zeile];
                    }

                    // löse A * y(s) = w(s-1) for y(s)
                    profilLöserStatus.SetzRechteSeite(_w);
                    profilLöserStatus.LösePrimal();

                    // berechne z(s) = B * y(s)
                    _z = MatrizenAlgebra.Mult(_b, _y, _status, _profil);

                    // berechne m2 = 1 / (y[s] * z[s])
                    double sum = 0;
                    for (_zeile = 0; _zeile < _dimension; _zeile++)
                        if (!_status[_zeile]) sum += _y[_zeile] * _z[_zeile];
                    _m2 = 1 / sum;


                    //berechne Rayleigh Quotient r = m2 * y(s)(T) * w(s-1)
                    // und die Differenz ( r(s) - r(s-1) )
                    sum = 0;
                    for (_zeile = 0; _zeile < _dimension; _zeile++)
                        if (!_status[_zeile]) sum += _y[_zeile] * _w[_zeile];
                    sum *= _m2;
                    _deltaRaleigh = sum - _raleigh;
                    _raleigh = sum;

                    //	berechne w(s) = m(s) * z(s)
                    m = Math.Sqrt(Math.Abs(_m2));
                    for (_zeile = 0; _zeile < _dimension; _zeile++)
                        if (!_status[_zeile]) _w[_zeile] = m * _z[_zeile];

                    // fahre mit Iteration fort so lange wie die Veränderung des in Rayleigh Faktors (r(s)-r(s-1)
                    // größer ist als die Fehlerschranke
                } while (Math.Abs(_deltaRaleigh) > Math.Abs(RaleighFaktor * _raleigh));

                // speichere berechnete Eigenzustände und Vektor p=w=Bx für B-orthogonalisierung
                _eigenwert[_state] = _raleigh;
                for (_zeile = 0; _zeile < _dimension; _zeile++)
                {
                    _x[_state][_zeile] = m * _y[_zeile];
                    _p[_state][_zeile] = _w[_zeile];
                }
            }
        }
    }
}
