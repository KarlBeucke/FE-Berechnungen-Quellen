namespace FEBibliothek.Modell
{
    public class Gleichungen
    {
        // false : last vordefiniert
        private double[][] matrix;          // Systemmatrix A
        private int zeile;
        private readonly int dimension;

        // Eigenschaften
        public double[][] Matrix
        {
            get
            {
                if (matrix != null) return matrix;

                var systemgleichungen = System.Windows.MessageBox.Show("Systemgleichungen wurden noch nicht berechnet");
                _ = systemgleichungen;
                return null;
            }
            set => matrix = value;
        }

        public double[] DiagonalMatrix { get; set; }
        public double[] Primal { get; set; }
        public double[] Dual { get; set; }
        public double[] Vektor { get; set; }
        public bool[] Status { get; set; }
        public int[] Profil { get; set; }


        // ... Konstructor ........................................................
        public Gleichungen(int n)
        {
            dimension = n;
            Status = new bool[dimension];
            Profil = new int[dimension];
            Primal = new double[dimension];
            Dual = new double[dimension];
            Vektor = new double[dimension];
            matrix = new double[dimension][];
            DiagonalMatrix = new double[dimension];
            for (zeile = 0; zeile < dimension; zeile++) { Profil[zeile] = zeile; }
        }

        // ... Setz den Profilvektor für ein Element..............................
        public void SetzProfil(int[] index)
        {
            foreach (var entry in index)
                foreach (var wert in index)
                    if (Profil[entry] > wert) Profil[entry] = wert;
        }
        // ... Setz den Statusvektor für einen Knoten .................................
        public void SetzStatus(bool status, int index, double value)
        {
            Status[index] = status;
            if (status) Primal[index] = value;
        }
        // ... Allokiere die Zeilenvektoren der Systemmatrix .......................
        public void AllokiereMatrix()
        {
            for (zeile = 0; zeile < dimension; zeile++)
            {
                matrix[zeile] = new double[zeile - Profil[zeile] + 1];
            }
        }
        // ... initialisiere Systemmatrix ............................................
        public void InitialisiereMatrix()
        {
            foreach (var zeilenReferenz in matrix)
                for (var col = 0; col < zeilenReferenz.Length; col++) zeilenReferenz[col] = 0;
        }

        // ... lese/schreibe einen Koeffizienten der Systemmatrix ......................
        public double HolWert(int i, int m) { return matrix[i][m - Profil[i]]; }
        public void SetzWert(int i, int m, double value) { matrix[i][m - Profil[i]] = value; }
        public void AddierWert(int i, int m, double value) { matrix[i][m - Profil[i]] += value; }

        // ... addierSubmatrix() .....................................................
        public void AddierMatrix(int[] index, double[,] elementMatrix)
        {
            for (var k = 0; k < index.Length; k++)
            {
                for (int m = 0; m < index.Length; m++)
                {
                    if (index[m] <= index[k])
                        AddierWert(index[k], index[m], elementMatrix[k, m]);
                }
            }
        }
        // ... addier DiagonalSubmatrix() ...............................................
        public void AddDiagonalMatrix(int[] index, double[] elementMatrix)
        {
            for (var k = 0; k < index.Length; k++)
                DiagonalMatrix[index[k]] += elementMatrix[k];
        }
        // ... addVector() .....................................................
        public void AddVektor(int[] index, double[] subvector)
        {
            for (var k = 0; k < subvector.Length; k++)
                Vektor[index[k]] += subvector[k];
        }
    }
}
