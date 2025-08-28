namespace FEBibliothek.Modell
{
    public class Gleichungen
    {
        private double[][] _matrix;          // Systemmatrix A
        private int _zeile;
        private readonly int _dimension;

        // Eigenschaften
        public double[][] Matrix
        {
            get
            {
                if (_matrix != null) return _matrix;

                var systemgleichungen = System.Windows.MessageBox.Show("Systemgleichungen wurden noch nicht berechnet");
                _ = systemgleichungen;
                return null;
            }
            set => _matrix = value;
        }

        public double[] DiagonalMatrix { get; set; }
        public double[] Primal { get; set; }
        public double[] Dual { get; set; }
        public double[] Vektor { get; set; }
        public bool[] Status { get; set; }
        public int[] Profil { get; set; }


        public Gleichungen(int n)
        {
            _dimension = n;
            Status = new bool[_dimension];
            Profil = new int[_dimension];
            Primal = new double[_dimension];
            Dual = new double[_dimension];
            Vektor = new double[_dimension];
            _matrix = new double[_dimension][];
            DiagonalMatrix = new double[_dimension];
            for (_zeile = 0; _zeile < _dimension; _zeile++) { Profil[_zeile] = _zeile; }
        }

        // Setz den Profilvektor für ein Element
        public void SetzProfil(int[] index)
        {
            foreach (var entry in index)
                foreach (var wert in index)
                    if (Profil[entry] > wert) Profil[entry] = wert;
        }
        // Setz den Statusvektor für einen Knoten.
        public void SetzStatus(bool status, int index, double value)
        {
            Status[index] = status;
            if (status) Primal[index] = value;
        }
        // Allokiere die Zeilenvektoren der Systemmatrix
        public void AllokiereMatrix()
        {
            for (_zeile = 0; _zeile < _dimension; _zeile++)
            {
                _matrix[_zeile] = new double[_zeile - Profil[_zeile] + 1];
            }
        }
        // initialisiere Systemmatrix
        public void InitialisiereMatrix()
        {
            foreach (var zeilenReferenz in _matrix)
                for (var col = 0; col < zeilenReferenz.Length; col++) zeilenReferenz[col] = 0;
        }

        // lese/schreibe einen Koeffizienten der Systemmatrix ......................
        public double HolWert(int i, int m) { return _matrix[i][m - Profil[i]]; }
        public void SetzWert(int i, int m, double value) { _matrix[i][m - Profil[i]] = value; }
        private void AddierWert(int i, int m, double value) { _matrix[i][m - Profil[i]] += value; }

        // addiereSubmatrix().
        public void AddierMatrix(int[] index, double[,] elementMatrix)
        {
            for (var k = 0; k < index.Length; k++)
            {
                for (var m = 0; m < index.Length; m++)
                {
                    if (index[m] <= index[k])
                        AddierWert(index[k], index[m], elementMatrix[k, m]);
                }
            }
        }
        // addier DiagonalSubmatrix()
        public void AddDiagonalMatrix(int[] index, double[] diagonalMatrix)
        {
            for (var k = 0; k < index.Length; k++)
                DiagonalMatrix[index[k]] += diagonalMatrix[k];
        }
        // addVector()
        public void AddVektor(int[] index, double[] subvektor)
        {
            if (subvektor.Length > index.Length)
                throw new BerechnungAusnahme("\"Zuweisung zu nicht vorhandenem Freiheitsgrad");
            for (var k = 0; k < subvektor.Length; k++)
                Vektor[index[k]] += subvektor[k];
        }
    }
}
