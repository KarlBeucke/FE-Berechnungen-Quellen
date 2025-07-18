using System.Globalization;
using static System.Console;

namespace FE_Berechnungen.input.Wärmeberechnung.instationär.Anregungsdateien.Schreib3DOFSinus.Schreib3DOFSinus
{
    public static class Schreib3DOFSinus
    {
        private static int _nSteps;
        private static int _dimension;
        private static double _tmax, _dt;
        private static double[][] _forceFunction;
        private static StreamWriter _writer;

        //public static void Main()
        public static void Schreib3DOF(string[] args)
        {
            Initialize();
        }

        private static void Initialize()
        {
            _dimension = 1;
            var delimiters = new[] { '\t', ' ' };

            WriteLine(@"Eingabe der Maximaldauer und des Zeitintervall:");
            // z.B. 400s und 0,99s Zeitintervall
            var line = ReadLine();
            if (line != null)
            {
                var substrings = line.Split(delimiters);
                _tmax = double.Parse(substrings[0]);
                _dt = double.Parse(substrings[1]);
            }

            _nSteps = (int)(_tmax / _dt);
            _forceFunction = new double[_nSteps + 1][];
            for (var i = 0; i < _nSteps + 1; i++)
                _forceFunction[i] = new double[_dimension];

            const string forceFile = "DreiDOFSinus.txt";
            _forceFunction = ForcingFunction();
            try
            {
                _writer = File.CreateText(forceFile);


                for (var k = 0; k < _nSteps + 1; k++)
                {
                    line = _forceFunction[k][0].ToString(CultureInfo.InvariantCulture);
                    for (var i = 1; i < _dimension; i++)
                    {
                        line += "\t" + _forceFunction[k][i];
                    }
                    _writer.WriteLine(line);
                }
            }
            finally
            {
                _writer?.Close();
            }
        }

        private static double[][] ForcingFunction()
        {
            for (var k = 0; k < _nSteps + 1; k++)
            {
                _forceFunction[k][0] = Math.Sin(0.03 * k * _dt);
                for (var i = 1; i < _dimension; i++)
                    _forceFunction[k][i] = 0;
            }
            return _forceFunction;
        }
    }
}
