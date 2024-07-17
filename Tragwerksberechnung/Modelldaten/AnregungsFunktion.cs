namespace FE_Berechnungen.Tragwerksberechnung.Modelldaten;

internal class AnregungsFunktion(double dt, int nSteps, int dimension)
{
    private readonly int _dimension = dimension;
    private readonly double _dt = dt;
    private readonly int _nSteps = nSteps;
    private double[][] _f;
    private double _zeit;

    public double[][] GetForce()
    {
        _f = new double[_nSteps + 1][];
        for (var i = 0; i < _nSteps + 1; i++) _f[i] = new double[_dimension];
        const double t1 = 0.8;

        for (var counter = 1; counter < _nSteps; counter++)
        {
            _zeit += _dt;
            double force;
            if ((_zeit >= 0) & (_zeit <= t1)) force = _zeit / t1;
            else if ((_zeit > t1) & (_zeit <= 2 * t1)) force = 2 - _zeit / t1;
            else if ((_zeit > 2 * t1) & (_zeit <= 4 * t1)) force = 1 - _zeit / (2 * t1);
            else if ((_zeit > 4 * t1) & (_zeit <= 6 * t1)) force = -3 + _zeit / (2 * t1);
            else if ((_zeit > 6 * t1) & (_zeit <= 7 * t1)) force = -6 + _zeit / t1;
            else if ((_zeit > 7 * t1) & (_zeit <= 8 * t1)) force = 8 - _zeit / t1;
            else force = 0;
            for (var i = 0; i < _dimension; i++)
                _f[counter][i] = force;
        }
        return _f;
    }
}