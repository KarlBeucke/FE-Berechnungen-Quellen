using FEBibliothek.Gleichungslöser;
using FEBibliothek.Modell;
using FEBibliothek.Werkzeuge;

namespace FEBibliothek.Zeitlöser
{
    public class Zeitintegration2OrdnungStatus
    {
        private readonly int dimension, methode;
        private readonly double dt, parameter1, parameter2;
        private readonly double[] m;
        private readonly double[] c;
        private readonly double[][] k, anregungsFunktion;
        private readonly int[] profil;
        private readonly bool[] status;
        public double[][] verformung, geschwindigkeit, beschleunigung;

        public Zeitintegration2OrdnungStatus(Gleichungen systemGleichungen, double[] dämpfung,
            double dt, int methode, double parameter1, double parameter2, double[][] displ, double[][] veloc, double[][] anregung)
        {
            m = systemGleichungen.DiagonalMatrix;
            c = dämpfung;
            k = systemGleichungen.Matrix;
            profil = systemGleichungen.Profil;
            status = systemGleichungen.Status;
            this.dt = dt;
            this.methode = methode;
            this.parameter1 = parameter1;
            this.parameter2 = parameter2;
            verformung = displ;
            geschwindigkeit = veloc;
            anregungsFunktion = anregung;
            dimension = k.Length;
        }

        public Zeitintegration2OrdnungStatus(double[] masse, double[] dämpfung, double[][] steifigkeit,
            int[] profil, bool[] status,
            double dt, int methode, double parameter1, double parameter2, double[][] displ, double[][] veloc, double[][] anregung)
        {
            m = masse;
            c = dämpfung;
            k = steifigkeit;
            this.profil = profil;
            this.status = status;
            this.dt = dt;
            this.methode = methode;
            this.parameter1 = parameter1;
            this.parameter2 = parameter2;
            verformung = displ;
            geschwindigkeit = veloc;
            anregungsFunktion = anregung;
            dimension = k.Length;
        }

        public void Ausführen()
        {
            double alfa, beta, gamma, theta;
            if (methode == 1) { alfa = 0; theta = 1; beta = parameter1; gamma = parameter2; }
            else if (methode == 2) { beta = 1.0 / 6; gamma = 0.5; alfa = 0; theta = parameter1; }
            else if (methode == 3) { theta = 1; alfa = parameter1; gamma = 0.5 - alfa; beta = 0.25 * (1 - alfa) * (1 - alfa); }
            else throw new BerechnungAusnahme("Zeitintegration2OrdnungStatus: ungültiger Identifikator für Methode eingegeben");

            var gammaDt = gamma * dt;
            var betaDt2 = beta * dt * dt;
            var gammaDtTheta = gamma * dt * theta;
            var dt1MGamma = dt * (1 - gamma);
            var dt2MBetaDt2 = dt * dt / 2 - beta * dt * dt;
            var thetaDt = theta * dt;
            var thetaDt1MGamma = theta * dt * (1 - gamma);
            var theta2Dt2MBetaDt2 = theta * theta * dt2MBetaDt2;
            var betaDt2Theta2AlfaP1 = beta * dt * dt * theta * theta * (1 + alfa);

            var primal = new double[dimension];
            var dual = new double[dimension];
            var zeitschritte = verformung.Length;
            beschleunigung = new double[zeitschritte][];
            for (var i = 0; i < zeitschritte; i++)
                beschleunigung[i] = new double[dimension];

            // berechne Anfangsbeschleunigungen an freien Freiheitsgraden, für M[i]>0
            var rechteSeite = MatrizenAlgebra.Mult(k, verformung[0], status, profil);
            for (var i = 0; i < dimension; i++)
            {
                // falls (status[i]) continue; ODER wenn M[i]=0 continue --> rechteSeite[i]=0
                if (status[i] | m[i] == 0) continue;
                rechteSeite[i] = (anregungsFunktion[0][i] - c[i] * geschwindigkeit[0][i] - rechteSeite[i]) / m[i];
                beschleunigung[0][i] = rechteSeite[i];
            }

            // konstante Koeffizientenmatrix
            var cm = new double[dimension][];
            for (var row = 0; row < dimension; row++)
            {
                cm[row] = new double[row + 1 - profil[row]];
                for (var col = 0; col <= (row - profil[row]); col++)
                    cm[row][col] = betaDt2Theta2AlfaP1 * k[row][col];
                cm[row][row - profil[row]] += m[row] + gammaDtTheta * c[row];
            }

            var profillöserStatus = new ProfillöserStatus(
                                        cm, rechteSeite, primal, dual, status, profil);
            profillöserStatus.Dreieckszerlegung();

            for (var zähler = 1; zähler < zeitschritte; zähler++)
            {
                // berechne verformung(hut) und geschwindigkeit(hut) an n+1
                for (var i = 0; i < dimension; i++)
                {
                    verformung[zähler][i] = verformung[zähler - 1][i]
                                               + thetaDt * geschwindigkeit[0][i]
                                               + theta2Dt2MBetaDt2 * beschleunigung[zähler - 1][i];
                    geschwindigkeit[1][i] = geschwindigkeit[0][i] + thetaDt1MGamma * beschleunigung[zähler - 1][i];
                }

                // berechne neue RechteSeite
                for (var i = 0; i < dimension; i++)
                    rechteSeite[i] = (1 + alfa) * verformung[zähler][i] - alfa * verformung[zähler - 1][i];
                rechteSeite = MatrizenAlgebra.Mult(k, rechteSeite, status, profil);
                for (var i = 0; i < dimension; i++)
                    if (!status[i])
                        rechteSeite[i] = (1 - theta) * anregungsFunktion[zähler - 1][i]
                                 + theta * anregungsFunktion[zähler][i]
                                 - c[i] * geschwindigkeit[1][i] - rechteSeite[i];

                // Rückwärtseinsetzung
                profillöserStatus.SetzRechteSeite(rechteSeite);
                profillöserStatus.LösePrimal();

                // verformungen, geschwindigkeiten und beschleunigungen an nächstem Zeitschritt
                for (var i = 0; i < dimension; i++)
                {
                    if (status[i]) continue;
                    beschleunigung[zähler][i] = beschleunigung[zähler - 1][i]
                                                + (primal[i]
                                                - beschleunigung[zähler - 1][i]) / theta;
                    verformung[zähler][i] = verformung[zähler - 1][i]
                                                + dt * geschwindigkeit[0][i]
                                                + dt2MBetaDt2 * beschleunigung[zähler - 1][i]
                                                + betaDt2 * beschleunigung[zähler][i];
                    geschwindigkeit[0][i] = geschwindigkeit[0][i]
                                                + dt1MGamma * beschleunigung[zähler - 1][i]
                                                + gammaDt * beschleunigung[zähler][i];
                }
            }
        }
    }
}
