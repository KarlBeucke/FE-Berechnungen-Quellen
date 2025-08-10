namespace FEBibliothek.Gleichungslöser
{
    //--------------------------------------------------------------------
    //  Class: ProfillöserStatus             lineares Gleichungssystem
    //--------------------------------------------------------------------
    //  Funktion:
    //
    //  Erzeugung und Lösung eines linearen Gleichungssystems
    //  mit symmetrischer Profilstruktur:
    //
    //      A * u = w + q
    //
    //  A   Systemmatrix mit vordefinierten Koeffizienten
    /// u   primal Lösungsvektor  (Vektor der Unbekannten)
    //  q   dual Lösungsvektor     (Vector der Reaktionen an Randbedingungen)
    //  w   Systemvektor with mit vordefinierten Koeffizienten (Lastvektor)
    //
    //  In jeder Zeile, ist entweder u[i] oder q[i] gegeben.
    //
    //-------------------------------------------------------------------
    //  METHODEN :
    //
    // public ProfileSolverStatus(double matrix [][], double vector [],
    // double primal[], double dual[],
    // boolean status[], int profile[])
    //
    // public ProfileSolverStatus(double matrix [][],
    // double primal[], double dual[],
    // boolean status[], int profile[])
    //
    //  public void SetRHS(double [] newVector)
    //  public void Decompose() throws Berechnungsausnahme
    //  public void Solve()
    //
    //-------------------------------------------------------------------

    public class ProfillöserStatus
    {
        private readonly bool[] _status;              // true  : primal vorgegeben
                                                      // false : dual   vorgegeben
        private readonly int[] _profil;               // Index der 1. spalte != 0
        private readonly double[][] _matrix;          // Systemmatrix A
        private double[] _vector;                     // Systemvektor w
        private readonly double[] _primal;            // primal Lösungsvektor
        private readonly double[] _dual;              // dual   Lösungsvektor
        private int _row, _column;
        private readonly int _dimension;

        // Erzeugung des Gleichungssystems
        public ProfillöserStatus(double[][] mat, double[] vec, double[] prim, double[] dua, bool[] stat, int[] prof)
        {
            _matrix = mat;
            _vector = vec;
            _primal = prim;
            _dual = dua;
            _status = stat;
            _profil = prof;
            _dimension = _matrix.Length;
        }
        // ohne vorgegebene Randbedingungen
        public ProfillöserStatus(double[][] mat, double[] vec, double[] prim, bool[] stat, int[] prof)
        {
            _matrix = mat;
            _vector = vec;
            _primal = prim;
            _status = stat;
            _profil = prof;
            _dimension = _matrix.Length;
        }
        // falls Matrix nur zerlegt werden soll
        public ProfillöserStatus(double[][] mat, bool[] stat, int[] prof)
        {
            _matrix = mat;
            _status = stat;
            _profil = prof;
            _dimension = _matrix.Length;
        }

        public void SetzRechteSeite(double[] newVector) { this._vector = newVector; }

        // Dreieckszerlegung der Systemmatrix
        public void Dreieckszerlegung()
        {
            // A[i][m] = A[i][m] - Sum(A[i][k]*A[k][m]) / A[k][k]
            for (_row = 0; _row < _dimension; _row++)
            {
                if (_status[_row]) continue;
                double sum;
                for (_column = _profil[_row]; _column < _row; _column++)
                {
                    if (_status[_column]) continue;
                    var start = Math.Max(_profil[_row], _profil[_column]);
                    sum = _matrix[_row][_column - _profil[_row]];
                    for (var m = start; m < _column; m++)
                    {
                        if (_status[m]) continue;
                        sum -= _matrix[_row][m - _profil[_row]] * _matrix[_column][m - _profil[_column]];
                    }
                    sum /= _matrix[_column][_column - _profil[_column]];
                    _matrix[_row][_column - _profil[_row]] = sum;
                }

                // A[i][i] = sqrt{(A[i][i] - Sum(A[i][m]*A[m][i])}
                sum = _matrix[_row][_row - _profil[_row]];
                for (var m = _profil[_row]; m < _row; m++)
                {
                    if (_status[m]) continue;
                    sum -= _matrix[_row][m - _profil[_row]] * _matrix[_row][m - _profil[_row]];
                }
                if (sum < double.Epsilon) throw new BerechnungAusnahme("\nGleichungslöser: Element <= 0 in Dreieckszerlegung von Zeile " + _row);
                _matrix[_row][_row - _profil[_row]] = Math.Sqrt(sum);
            }
        }

        // Lösung der Systemgleichungen
        // ersetze die vorgegebenen Werte in den Zeilen ohne
        // vorgegebene Primärvariable: u = c1 + y1 - A12 * x2
        public void Lösung()
        {
            LösePrimal();
            LösDual();
        }
        public void LösePrimal()
        {
            for (_row = 0; _row < _dimension; _row++)
            {
                if (_status[_row]) continue;
                _primal[_row] = _vector[_row];
                for (_column = _profil[_row]; _column < _row; _column++)
                {
                    if (!_status[_column]) continue;
                    _primal[_row] -= _matrix[_row][_column - _profil[_row]] * _primal[_column];
                }
            }

            for (_column = 0; _column < _dimension; _column++)
            {
                if (!_status[_column]) continue;
                for (_row = _profil[_column]; _row < _column; _row++)
                {
                    if (_status[_row]) continue;
                    _primal[_row] -= _matrix[_column][_row - _profil[_column]] * _primal[_column];
                }
            }

            // berechne Primärvariable: zeilenweise Vorwärtszerlegung
            for (_row = 0; _row < _dimension; _row++)
            {
                if (_status[_row]) continue;
                for (_column = _profil[_row]; _column < _row; _column++)
                {
                    if (_status[_column]) continue;
                    _primal[_row] -= _matrix[_row][_column - _profil[_row]] * _primal[_column];
                }
                _primal[_row] /= _matrix[_row][_row - _profil[_row]];
            }

            // berechne Primärvariable: zeilenweise Rückwärtszerlegung
            for (_column = _dimension - 1; _column >= 0; _column--)
            {
                if (_status[_column]) continue;
                _primal[_column] /= _matrix[_column][_column - _profil[_column]];
                for (_row = _profil[_column]; _row < _column; _row++)
                {
                    if (_status[_row]) continue;
                    _primal[_row] -= _matrix[_column][_row - _profil[_column]] * _primal[_column];
                }
            }
        }

        private void LösDual()
        {
            //  berechne die Dualvariablen: ersetze die Primärvariablen
            //  in den Zeilen mit den vorgegebenen Primärvariablen
            for (_row = 0; _row < _dimension; _row++)
            {
                if (!_status[_row]) continue;
                _dual[_row] = -_vector[_row];
                if (_row - _profil[_row] > _matrix[_row].Length - 1)
                    throw new BerechnungAusnahme("\nRandbedingung in Zeile '" + _row +
                                                 "' der Steifigkeitsmatrix nicht ermittelbar, falscher Anschluss");
                for (_column = _profil[_row]; _column <= _row; _column++)
                    _dual[_row] += _matrix[_row][_column - _profil[_row]] * _primal[_column];
            }

            for (_column = 0; _column < _dimension; _column++)
            {
                for (_row = _profil[_column]; _row < _column; _row++)
                {
                    if (!_status[_row]) continue;
                    _dual[_row] += _matrix[_column][_row - _profil[_column]] * _primal[_column];
                }
            }
        }
    }
}