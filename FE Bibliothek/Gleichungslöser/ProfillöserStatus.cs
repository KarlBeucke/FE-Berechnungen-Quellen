using FEBibliothek.Modell;
using System;

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
        private readonly bool[] status;              // true  : primal vorgegeben
                                                     // false : dual   vorgegeben
        private readonly int[] profil;               // Index der 1. spalte != 0
        private readonly double[][] matrix;          // Systemmatrix A
        private double[] vector;                     // Systemvektor w
        private readonly double[] primal;            // primal Lösungsvektor
        private readonly double[] dual;              // dual   Lösungsvektor
        private int row, column;
        private readonly int dimension;

        // Erzeugung des Gleichungssystems
        public ProfillöserStatus(double[][] mat, double[] vec, double[] prim, double[] dua, bool[] stat, int[] prof)
        {
            matrix = mat;
            vector = vec;
            primal = prim;
            dual = dua;
            status = stat;
            profil = prof;
            dimension = matrix.Length;
        }
        // ohne vorgegebene Randbedingungen
        public ProfillöserStatus(double[][] mat, double[] vec, double[] prim, bool[] stat, int[] prof)
        {
            matrix = mat;
            vector = vec;
            primal = prim;
            status = stat;
            profil = prof;
            dimension = matrix.Length;
        }
        // falls Matrix nur zerlegt werden soll
        public ProfillöserStatus(double[][] mat, bool[] stat, int[] prof)
        {
            matrix = mat;
            status = stat;
            profil = prof;
            dimension = matrix.Length;
        }

        public void SetzRechteSeite(double[] newVector) { this.vector = newVector; }

        // Dreieckszerlegung der Systemmatrix
        public void Dreieckszerlegung()
        {
            // A[i][m] = A[i][m] - Sum(A[i][k]*A[k][m]) / A[k][k]
            for (row = 0; row < dimension; row++)
            {
                if (status[row]) continue;
                double sum;
                for (column = profil[row]; column < row; column++)
                {
                    if (status[column]) continue;
                    var start = Math.Max(profil[row], profil[column]);
                    sum = matrix[row][column - profil[row]];
                    for (var m = start; m < column; m++)
                    {
                        if (status[m]) continue;
                        sum -= matrix[row][m - profil[row]] * matrix[column][m - profil[column]];
                    }
                    sum /= matrix[column][column - profil[column]];
                    matrix[row][column - profil[row]] = sum;
                }

                // A[i][i] = sqrt{(A[i][i] - Sum(A[i][m]*A[m][i])}
                sum = matrix[row][row - profil[row]];
                for (var m = profil[row]; m < row; m++)
                {
                    if (status[m]) continue;
                    sum -= matrix[row][m - profil[row]] * matrix[row][m - profil[row]];
                }
                if (sum < double.Epsilon) throw new BerechnungAusnahme("\nGleichungslöser: Element <= 0 in Dreieckszerlegung von Zeile " + row);
                matrix[row][row - profil[row]] = Math.Sqrt(sum);
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
            for (row = 0; row < dimension; row++)
            {
                if (status[row]) continue;
                primal[row] = vector[row];
                for (column = profil[row]; column < row; column++)
                {
                    if (!status[column]) continue;
                    primal[row] -= matrix[row][column - profil[row]] * primal[column];
                }
            }

            for (column = 0; column < dimension; column++)
            {
                if (!status[column]) continue;
                for (row = profil[column]; row < column; row++)
                {
                    if (status[row]) continue;
                    primal[row] -= matrix[column][row - profil[column]] * primal[column];
                }
            }

            // berechne Primärvariable: zeilenweise Vorwärtszerlegung
            for (row = 0; row < dimension; row++)
            {
                if (status[row]) continue;
                for (column = profil[row]; column < row; column++)
                {
                    if (status[column]) continue;
                    primal[row] -= matrix[row][column - profil[row]] * primal[column];
                }
                primal[row] /= matrix[row][row - profil[row]];
            }

            // berechne Primärvariable: zeilenweise Rückwärtszerlegung
            for (column = dimension - 1; column >= 0; column--)
            {
                if (status[column]) continue;
                primal[column] /= matrix[column][column - profil[column]];
                for (row = profil[column]; row < column; row++)
                {
                    if (status[row]) continue;
                    primal[row] -= matrix[column][row - profil[column]] * primal[column];
                }
            }
        }

        private void LösDual()
        {
            //  berechne die Dualvariablen: ersetze die Primärvariablen
            //  in den Zeilen mit den vorgegebenen Primärvariablen
            for (row = 0; row < dimension; row++)
            {
                if (!status[row]) continue;
                dual[row] = -vector[row];
                for (column = profil[row]; column <= row; column++)
                    dual[row] += matrix[row][column - profil[row]] * primal[column];
            }

            for (column = 0; column < dimension; column++)
            {
                for (row = profil[column]; row < column; row++)
                {
                    if (!status[row]) continue;
                    dual[row] += matrix[column][row - profil[column]] * primal[column];

                }
            }
        }
    }
}