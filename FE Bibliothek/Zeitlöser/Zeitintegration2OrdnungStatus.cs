using FEBibliothek.Gleichungslöser;
using FEBibliothek.Modell;
using FEBibliothek.Werkzeuge;

namespace FEBibliothek.Zeitlöser
{
    public class Zeitintegration2OrdnungStatus
    {
        private readonly int _dimension, _methode;
        private readonly double _dt, _parameter1, _parameter2;
        private readonly double[] _m;
        private readonly double[] _c;
        private readonly double[][] _k, _anregungsFunktion;
        private readonly int[] _profil;
        private readonly bool[] _status;
        public readonly double[][] Verformung, Geschwindigkeit;
        public double[][] Beschleunigung;

        public Zeitintegration2OrdnungStatus(Gleichungen systemGleichungen, double[] dämpfung,
            double dt, int methode, double parameter1, double parameter2, double[][] displ, double[][] veloc, double[][] anregung)
        {
            _m = systemGleichungen.DiagonalMatrix;
            _c = dämpfung;
            _k = systemGleichungen.Matrix;
            _profil = systemGleichungen.Profil;
            _status = systemGleichungen.Status;
            this._dt = dt;
            this._methode = methode;
            this._parameter1 = parameter1;
            this._parameter2 = parameter2;
            Verformung = displ;
            Geschwindigkeit = veloc;
            _anregungsFunktion = anregung;
            _dimension = _k.Length;
        }

        public Zeitintegration2OrdnungStatus(double[] masse, double[] dämpfung, double[][] steifigkeit,
            int[] profil, bool[] status,
            double dt, int methode, double parameter1, double parameter2, double[][] displ, double[][] veloc, double[][] anregung)
        {
            _m = masse;
            _c = dämpfung;
            _k = steifigkeit;
            this._profil = profil;
            this._status = status;
            this._dt = dt;
            this._methode = methode;
            this._parameter1 = parameter1;
            this._parameter2 = parameter2;
            Verformung = displ;
            Geschwindigkeit = veloc;
            _anregungsFunktion = anregung;
            _dimension = _k.Length;
        }

        public void Ausführen()
        {
            double alfa, beta, gamma, theta;
            if (_methode == 1) { alfa = 0; theta = 1; beta = _parameter1; gamma = _parameter2; }
            else if (_methode == 2) { beta = 1.0 / 6; gamma = 0.5; alfa = 0; theta = _parameter1; }
            else if (_methode == 3) { theta = 1; alfa = _parameter1; gamma = 0.5 - alfa; beta = 0.25 * (1 - alfa) * (1 - alfa); }
            else throw new BerechnungAusnahme("Zeitintegration2OrdnungStatus: ungültiger Identifikator für Methode eingegeben");

            var gammaDt = gamma * _dt;
            var betaDt2 = beta * _dt * _dt;
            var gammaDtTheta = gamma * _dt * theta;
            var dt1MGamma = _dt * (1 - gamma);
            var dt2MBetaDt2 = _dt * _dt / 2 - beta * _dt * _dt;
            var thetaDt = theta * _dt;
            var thetaDt1MGamma = theta * _dt * (1 - gamma);
            var theta2Dt2MBetaDt2 = theta * theta * dt2MBetaDt2;
            var betaDt2Theta2AlfaP1 = beta * _dt * _dt * theta * theta * (1 + alfa);

            var primal = new double[_dimension];
            var dual = new double[_dimension];
            var zeitschritte = Verformung.Length;
            Beschleunigung = new double[zeitschritte][];
            for (var i = 0; i < zeitschritte; i++)
                Beschleunigung[i] = new double[_dimension];

            // berechne Anfangsbeschleunigungen an freien Freiheitsgraden, für M[i]>0
            var rechteSeite = MatrizenAlgebra.Mult(_k, Verformung[0], _status, _profil);
            for (var i = 0; i < _dimension; i++)
            {
                // falls (status[i]) continue; ODER wenn M[i]=0 continue --> rechteSeite[i]=0
                if (_status[i] | _m[i] == 0) continue;
                rechteSeite[i] = (_anregungsFunktion[0][i] - _c[i] * Geschwindigkeit[0][i] - rechteSeite[i]) / _m[i];
                Beschleunigung[0][i] = rechteSeite[i];
            }

            // konstante Koeffizientenmatrix
            var cm = new double[_dimension][];
            for (var row = 0; row < _dimension; row++)
            {
                cm[row] = new double[row + 1 - _profil[row]];
                for (var col = 0; col <= (row - _profil[row]); col++)
                    cm[row][col] = betaDt2Theta2AlfaP1 * _k[row][col];
                cm[row][row - _profil[row]] += _m[row] + gammaDtTheta * _c[row];
            }

            var profillöserStatus = new ProfillöserStatus(
                                        cm, rechteSeite, primal, dual, _status, _profil);
            profillöserStatus.Dreieckszerlegung();

            for (var zähler = 1; zähler < zeitschritte; zähler++)
            {
                // berechne verformung(hut) und geschwindigkeit(hut) an n+1
                for (var i = 0; i < _dimension; i++)
                {
                    Verformung[zähler][i] = Verformung[zähler - 1][i]
                                               + thetaDt * Geschwindigkeit[0][i]
                                               + theta2Dt2MBetaDt2 * Beschleunigung[zähler - 1][i];
                    Geschwindigkeit[1][i] = Geschwindigkeit[0][i] + thetaDt1MGamma * Beschleunigung[zähler - 1][i];
                }

                // berechne neue RechteSeite
                for (var i = 0; i < _dimension; i++)
                    rechteSeite[i] = (1 + alfa) * Verformung[zähler][i] - alfa * Verformung[zähler - 1][i];
                rechteSeite = MatrizenAlgebra.Mult(_k, rechteSeite, _status, _profil);
                for (var i = 0; i < _dimension; i++)
                    if (!_status[i])
                        rechteSeite[i] = (1 - theta) * _anregungsFunktion[zähler - 1][i]
                                 + theta * _anregungsFunktion[zähler][i]
                                 - _c[i] * Geschwindigkeit[1][i] - rechteSeite[i];

                // Rückwärtseinsetzung
                profillöserStatus.SetzRechteSeite(rechteSeite);
                profillöserStatus.LösePrimal();

                // verformungen, geschwindigkeiten und beschleunigungen an nächstem Zeitschritt
                for (var i = 0; i < _dimension; i++)
                {
                    if (_status[i]) continue;
                    Beschleunigung[zähler][i] = Beschleunigung[zähler - 1][i]
                                                + (primal[i]
                                                - Beschleunigung[zähler - 1][i]) / theta;
                    Verformung[zähler][i] = Verformung[zähler - 1][i]
                                                + _dt * Geschwindigkeit[0][i]
                                                + dt2MBetaDt2 * Beschleunigung[zähler - 1][i]
                                                + betaDt2 * Beschleunigung[zähler][i];
                    Geschwindigkeit[0][i] = Geschwindigkeit[0][i]
                                                + dt1MGamma * Beschleunigung[zähler - 1][i]
                                                + gammaDt * Beschleunigung[zähler][i];
                }
            }
        }
    }
}
