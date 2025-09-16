using FEBibliothek.Gleichungslöser;
using FEBibliothek.Zeitlöser;
using Microsoft.Win32;
using System.IO;
using System.Linq;

namespace FEBibliothek.Modell
{
    public class Berechnung
    {
        private FeModell _modell;
        private Knoten _knoten;
        private AbstraktElement _element;
        private Gleichungen _systemGleichungen;
        private ProfillöserStatus _profilLöser;
        private int _dimension;
        private bool _zerlegt, _setzDimension, _profil, _diagonalMatrix;
        private readonly string _initialDirectory;

        public Berechnung(FeModell m)
        {
            _modell = m;
            if (_modell == null)
            {
                throw new BerechnungAusnahme("\nModelleingabedaten noch nicht eingelesen");
            }

            SetzSystemindizes();
            SetzReferenzen(m);
            FreieKnoten();
        }
        public Berechnung(FeModell m, string initialDirectory)
        {
            _modell = m;
            if (_modell == null)
            {
                throw new BerechnungAusnahme("\nModelleingabedaten noch nicht eingelesen");
            }
            _initialDirectory = initialDirectory;

            SetzSystemindizes();
            SetzReferenzen(m);
            FreieKnoten();
        }
        private void SetzSystemindizes()
        {
            var k = 0;
            foreach (var item in _modell.Knoten)
            {
                _knoten = item.Value;
                k = _knoten.SetzSystemIndizes(k);
            }
        }
        // Objekt Referenzen werden erst auf Basis der eindeutigen Identifikatoren ermittelt, d.h. unmittelbar vor Objekt Instantiierung
        // wenn, eine Berechnung gestartet wird, müssen folglich ALLE Objektreferenzen auf Basis der eindeutigen Identifikatoren ermittelt werden
        private void SetzReferenzen(FeModell m)
        {
            _modell = m;

            // Referenzen für Querschnittsverweise von 2D Elementen setzen
            foreach (var abstractElement in
                        from KeyValuePair<string, AbstraktElement> item in _modell.Elemente
                        where item.Value != null
                        where item.Value is Abstrakt2D
                        let element = item.Value
                        select element)
            {
                var element2D = (Abstrakt2D)abstractElement;
                element2D.SetzQuerschnittReferenzen(_modell);
            }
            // setzen aller notwendigen Elementreferenzen und der Systemindizes aller Elemente 
            foreach (var abstractElement in _modell.Elemente.Select(item => item.Value))
            {
                abstractElement.SetzElementReferenzen(_modell);
                abstractElement.SetzElementSystemIndizes();
            }
            // Lagerreferenzen
            foreach (var randbedingung in _modell.Randbedingungen.Select(item => item.Value))
            {
                randbedingung.SetzRandbedingungenReferenzen(_modell);
            }
            // Lastreferenzen
            foreach (var last in _modell.Lasten.Select(item => item.Value))
            {
                var knotenlast = (AbstraktKnotenlast)last;
                knotenlast.SetzReferenzen(_modell);
            }
            foreach (var last in _modell.ElementLasten.Select(item => item.Value))
            {
                switch (last)
                {
                    case AbstraktLinienlast linienlast:
                        linienlast.SetzLinienlastReferenzen(_modell);
                        break;
                    case not null:
                        last.SetzElementlastReferenzen(_modell);
                        break;
                    default:
                        _ = MessageBox.Show("Elementlast nicht gefunden", "Berechnung");
                        break;
                }
            }
            foreach (var last in _modell.PunktLasten.Select(item => item.Value))
            {
                last.SetzElementlastReferenzen(_modell);
            }
            // zeitabhängige Last- und Lagerreferenzen
            foreach (var zeitabhängigeKnotenlast in _modell.ZeitabhängigeKnotenLasten.Select(item => item.Value))
            {
                zeitabhängigeKnotenlast.SetzReferenzen(_modell);
            }
            foreach (var zeitabhängigeElementLast in _modell.ZeitabhängigeElementLasten.Select(item => item.Value))
            {
                zeitabhängigeElementLast.SetzElementlastReferenzen(_modell);
            }
            foreach (var zeitabhängigeRandbedingung in _modell.ZeitabhängigeRandbedingung.Select(item => item.Value))
            {
                zeitabhängigeRandbedingung.SetzRandbedingungenReferenzen(_modell);
            }
        }
        private void FreieKnoten()
        {
            // check alle Knoten, ob sie instabil sind, mit keinem Element verbunden
            foreach (var id in _modell.Knoten.Select(knoten => knoten.Key))
            {
                var frei = true;
                foreach (var element in _modell.Elemente)
                {
                    if (element.Value.KnotenIds.Any(t => t == id)) { frei = false; }
                    if (!frei) break;
                }
                if (frei) throw new BerechnungAusnahme("\nKnoten '" + id + "' ist instabil, wird durch kein Element genutzt");
            }
        }
        // bestimme Dimension der Systemmatrix
        private void BestimmeDimension()
        {
            _dimension = 0;
            foreach (var item in _modell.Knoten)
            {
                _dimension += item.Value.AnzahlKnotenfreiheitsgrade;
            }
            _systemGleichungen = new Gleichungen(_dimension);
            _setzDimension = true;
        }
        // berechne und löse die Matrix in Profilformat mit StatusVektor
        private void BestimmeProfil()
        {
            foreach (var item in _modell.Elemente)
            {
                _element = item.Value;
                _systemGleichungen.SetzProfil(_element.SystemIndizesElement);
            }
            _systemGleichungen.AllokiereMatrix();
            _profil = true;
        }
        public void BerechneSystemMatrix()
        {
            if (!_setzDimension) BestimmeDimension();
            if (!_profil) BestimmeProfil();
            // traversiere Elemente zur Bestimmung der Matrixkoeffizienten
            foreach (var item in _modell.Elemente)
            {
                _element = item.Value;
                var elementMatrix = _element.BerechneElementMatrix();
                _systemGleichungen.AddierMatrix(_element.SystemIndizesElement, elementMatrix);
            }
            SetzStatusVektor();
        }
        private void SetzStatusVektor()
        {
            // für alle festen Randbedingungen
            foreach (var item in _modell.Randbedingungen) StatusKnoten(item.Value);
        }
        private void StatusKnoten(AbstraktRandbedingung randbedingung)
        {
            var knotenId = randbedingung.KnotenId;

            if (_modell.Knoten.TryGetValue(knotenId, out _knoten))
            {
                _systemGleichungen.SetzProfil(_knoten.SystemIndizes);
                var vordefiniert = randbedingung.Vordefiniert;
                var festgehalten = randbedingung.Festgehalten;
                for (var i = 0; i < festgehalten.Length; i++)
                {
                    //if (festgehalten[2] && _knoten.SystemIndizes.Length < 3)
                    //    throw new BerechnungAusnahme("\nKnoten " + _knoten.Id +
                    //                                   " muß 3 Knotenfreiheitsgrade für Festeinspannung haben.");
                    if (festgehalten[i]) _systemGleichungen.SetzStatus(true, _knoten.SystemIndizes[i], vordefiniert[i]);
                }
            }
            else
                throw new BerechnungAusnahme("\nEndknoten " + knotenId + " ist nicht im Modell enthalten.");
        }
        private void NeuberechnungSystemMatrix()
        {
            // traversiere Element zur Bestimmung der Matrixkoeffizienten
            _systemGleichungen.InitialisiereMatrix();
            foreach (var item in _modell.Elemente)
            {
                _element = item.Value;
                var indizes = _element.SystemIndizesElement;
                var elementMatrix = _element.BerechneElementMatrix();
                _systemGleichungen.AddierMatrix(indizes, elementMatrix);
            }
        }
        public void BerechneSystemVektor()
        {
            int[] indizes;
            double[] lastVektor;

            // Knotenlasten
            foreach (var (_, knotenLast) in _modell.Lasten)
            {
                var knotenId = knotenLast.KnotenId;
                if (_modell.Knoten.TryGetValue(knotenId, out var lastKnoten))
                {
                    indizes = lastKnoten.SystemIndizes;
                    lastVektor = knotenLast.BerechneLastVektor();
                    _systemGleichungen.AddVektor(indizes, lastVektor);
                }
                else
                {
                    throw new BerechnungAusnahme("\nLastknoten " + knotenId + " ist nicht im Modell enthalten.");
                }
            }
            // Elementlasten: Linienlasten
            foreach (var (_, elementLast) in _modell.ElementLasten)
            {
                if (elementLast is AbstraktLinienlast linienLast)
                {
                    var start = linienLast.StartKnoten.SystemIndizes.Length;
                    var end = linienLast.EndKnoten.SystemIndizes.Length;
                    indizes = new int[start + end];
                    for (var i = 0; i < start; i++)
                        indizes[i] = linienLast.StartKnoten.SystemIndizes[i];
                    for (var i = 0; i < end; i++)
                        indizes[start + i] = linienLast.EndKnoten.SystemIndizes[i];
                    lastVektor = linienLast.BerechneLastVektor();
                    _systemGleichungen.AddVektor(indizes, lastVektor);
                }
                else
                {
                    var elementId = elementLast.ElementId;
                    if (_modell.Elemente.TryGetValue(elementId, out var element))
                    {
                        indizes = element.SystemIndizesElement;
                        lastVektor = elementLast.BerechneLastVektor();
                        _systemGleichungen.AddVektor(indizes, lastVektor);
                    }
                    else
                    {
                        throw new BerechnungAusnahme("Element " + elementId + " für Elementlast ist nicht im Modell enthalten");
                    }
                }
            }
            // Elementlasten: Punktlasten
            foreach (var (_, punktLast) in _modell.PunktLasten)
            {
                var elementId = punktLast.ElementId;
                if (_modell.Elemente.TryGetValue(elementId, out _element))
                {
                    punktLast.Element = _element;
                    indizes = _element.SystemIndizesElement;
                    lastVektor = punktLast.BerechneLastVektor();
                    _systemGleichungen.AddVektor(indizes, lastVektor);
                }
                else
                    throw new BerechnungAusnahme("\nElement " + elementId + " für Linienlasten ist nicht im Modell enthalten.");
            }
        }
        public void LöseGleichungen()
        {
            if (!_zerlegt)
            {
                _profilLöser = new ProfillöserStatus(
                    _systemGleichungen.Matrix, _systemGleichungen.Vektor,
                    _systemGleichungen.Primal, _systemGleichungen.Dual,
                    _systemGleichungen.Status, _systemGleichungen.Profil);
                _profilLöser.Dreieckszerlegung();
                _zerlegt = true;
            }
            _profilLöser.Lösung();
            // speichere System Unbekannte (primale Werte)
            foreach (var item in _modell.Knoten)
            {
                _knoten = item.Value;
                var index = _knoten.SystemIndizes;
                _knoten.Knotenfreiheitsgrade = new double[_knoten.AnzahlKnotenfreiheitsgrade];
                for (var i = 0; i < _knoten.Knotenfreiheitsgrade.Length; i++)
                    _knoten.Knotenfreiheitsgrade[i] = _systemGleichungen.Primal[index[i]];
            }
            // speichere duale Werte
            var reaktionen = _systemGleichungen.Dual;
            foreach (var randbedingung in _modell.Randbedingungen.Select(item => item.Value))
            {
                _knoten = randbedingung.Knoten;
                var index = _knoten.SystemIndizes;
                var reaktion = new double[_knoten.AnzahlKnotenfreiheitsgrade];
                for (var i = 0; i < reaktion.Length; i++)
                    reaktion[i] = reaktionen[index[i]];
                _knoten.Reaktionen = reaktion;
            }
        }

        // Eigenlösungen
        public void Eigenzustände()
        {
            var anzahlZustände = _modell.Eigenzustand.AnzahlZustände;
            // Anzahl der Eigenzustände muss ≤ sein der Anzahl der freien Systemfreiheitsgrade
            var anzahlFreieDof = 0;
            for (var i = 0; i < _dimension; i++)
                if (!_systemGleichungen.Status[i]) anzahlFreieDof += 1;
            if (_modell.Eigenzustand.AnzahlZustände > anzahlFreieDof)
                _modell.Eigenzustand.AnzahlZustände = anzahlFreieDof;

            var aMatrix = _systemGleichungen.Matrix;
            if (!_diagonalMatrix) BerechneDiagonalMatrix();
            var bDiag = _systemGleichungen.DiagonalMatrix;

            // allgemeine B-Matrix wird erweitert auf die gleiche Struktur wie A
            var bMatrix = new double[_dimension][];
            int zeile;
            for (zeile = 0; zeile < aMatrix.Length; zeile++)
            {
                bMatrix[zeile] = new double[aMatrix[zeile].Length];
                int spalte;
                for (spalte = 0; spalte < bMatrix[zeile].Length - 1; spalte++)
                    bMatrix[zeile][spalte] = 0;
                bMatrix[zeile][spalte] = bDiag[zeile];
            }

            if (!_modell.ZeitIntegration)
            {
                SetzZeitabhängigenStatusVektor();
            }

            if (!_zerlegt)
            {
                _profilLöser = new ProfillöserStatus(
                    _systemGleichungen.Matrix,
                    _systemGleichungen.Status, _systemGleichungen.Profil);
                _profilLöser.Dreieckszerlegung();
                _zerlegt = true;
            }

            var eigenLöser = new Eigenlöser(_systemGleichungen.Matrix, bMatrix,
                _systemGleichungen.Profil, _systemGleichungen.Status,
                anzahlZustände);
            eigenLöser.LöseEigenzustände();

            // speichere Eigenwerte und -vektoren
            var eigenwerte = new double[anzahlZustände];
            var eigenvektoren = new double[anzahlZustände][];
            for (var i = 0; i < anzahlZustände; i++)
            {

                eigenwerte[i] = eigenLöser.HolEigenwert(i);
                eigenvektoren[i] = eigenLöser.HolEigenvektor(i);
            }
            _modell.Eigenzustand.Eigenwerte = eigenwerte;
            _modell.Eigenzustand.Eigenvektoren = eigenvektoren;
            _modell.Eigen = true;
        }
        private void BerechneDiagonalMatrix()
        {
            // diagonale spezifische Wärme- bzw. Massenmatrix
            if (!_setzDimension) BestimmeDimension();

            // traversiere Elemente zur Ermittlung der Koeffizienten der Diagonalmatrix
            foreach (var item in _modell.Elemente)
            {
                var abstraktesElement = item.Value;
                var index = abstraktesElement.SystemIndizesElement;
                var elementMatrix = abstraktesElement.BerechneDiagonalMatrix();
                _systemGleichungen.AddDiagonalMatrix(index, elementMatrix);
            }

            // festgehaltene Freiheitsgrade liefern keine Beiträge zu Massenkräften
            foreach (var randbedingung in _modell.Randbedingungen)
            {
                var systemIndizes = randbedingung.Value.Knoten.SystemIndizes;
                for (var i = 0; i < randbedingung.Value.Festgehalten.Length; i++)
                {
                    if (randbedingung.Value.Festgehalten[i]) _systemGleichungen.DiagonalMatrix[systemIndizes[i]] = 0;
                }
            }
            _diagonalMatrix = true;
        }

        // Zeitintegration 1. Ordnung
        public void ZeitintegrationErsterOrdnung()
        {
            // berechne spezifische Wärme Matrix
            if (!_diagonalMatrix) BerechneDiagonalMatrix();
            _ = _systemGleichungen.DiagonalMatrix;


            var dt = _modell.Zeitintegration.Dt;
            if (dt == 0)
            {
                throw new BerechnungAusnahme("\nZeitschrittintervall nicht definiert.");
            }
            var tmax = _modell.Zeitintegration.Tmax;
            var alfa = _modell.Zeitintegration.Parameter1;
            var nZeitschritte = (int)(tmax / dt) + 1;
            var anregungsFunktion = new double[nZeitschritte][];
            for (var k = 0; k < nZeitschritte; k++)
                anregungsFunktion[k] = new double[_dimension];
            var temperatur = new double[nZeitschritte][];
            for (var i = 0; i < nZeitschritte; i++) temperatur[i] = new double[_dimension];

            SetzAnfangsbedingungenErsterOrdnung(temperatur);
            SetzZeitabhängigenStatusVektor();

            // berechne zeitabhängige Anregungsfunktion und Randbedingungen
            BerechneAnregungsfunktionErsterOrdnung(anregungsFunktion);
            BerechneRandbedingungenErsterOrdnung(temperatur);

            // Systemmatrix muss neu berechnet werden, falls Dreieckszerlegung gespeichert
            if (_zerlegt) { NeuberechnungSystemMatrix(); _zerlegt = false; }

            var zeitintegration = new Zeitintegration1OrdnungStatus(
                _systemGleichungen, anregungsFunktion, dt, alfa, temperatur);
            zeitintegration.Ausführung();

            // speichere Knotenzeitverläufe
            foreach (var item in _modell.Knoten)
            {
                _knoten = item.Value;
                var index = item.Value.SystemIndizes[0];
                _knoten.KnotenVariable = new double[1][];
                _knoten.KnotenVariable[0] = new double[nZeitschritte];
                _knoten.KnotenAbleitungen = new double[1][];
                _knoten.KnotenAbleitungen[0] = new double[nZeitschritte];

                // temperatur[nZeitschritte][index], KnotenVariable[index][nZeitschritte]
                for (var k = 0; k < nZeitschritte; k++)
                {
                    _knoten.KnotenVariable[0][k] = temperatur[k][index];
                    _knoten.KnotenAbleitungen[0][k] = zeitintegration.TemperaturGradient[k][index];
                }
            }
            _modell.ZeitIntegration = true;
        }
        private void SetzAnfangsbedingungenErsterOrdnung(double[][] temperatur)
        {
            // setz stationäre Lösung als Anfangsbedingungen
            if (_modell.Zeitintegration.VonStationär) { temperatur[0] = _systemGleichungen.Primal; }

            foreach (var anf in _modell.Zeitintegration.Anfangsbedingungen)
            {
                if (anf.KnotenId == "alle")
                {
                    for (var i = 0; i < _dimension; i++) temperatur[0][i] = anf.Werte[0];
                }
                else
                {
                    if (!_modell.Knoten.TryGetValue(anf.KnotenId, out var anfKnoten))
                    {
                        throw new BerechnungAusnahme("\nKnoten " + anf.KnotenId +
                                                     " für zeitabhängige Anfangsbedingung ist nicht im Modell enthalten.");
                    }

                    temperatur[0][anfKnoten.SystemIndizes[0]] = anf.Werte[0];
                }
            }
        }
        private void SetzZeitabhängigenStatusVektor()
        {
            // für alle zeitabhängigen Randbedingungen
            if (_modell == null) return;
            foreach (var randbedingung in
                _modell.ZeitabhängigeRandbedingung.Select(item => item.Value))
            {
                StatusKnoten(randbedingung);
            }
        }
        // zeitabhängige Knoten- und Elementlasten
        private void BerechneAnregungsfunktionErsterOrdnung(double[][] temperatur)
        {
            var last = new double[temperatur.Length];
            var nZeitschritte = last.Length;

            // finde zeitabhängige Knotenlasten
            foreach (var item in _modell.ZeitabhängigeKnotenLasten)
            {
                if (_modell.Knoten.TryGetValue(item.Value.KnotenId, out _knoten))
                {
                    var lastIndex = _knoten.SystemIndizes;

                    switch (item.Value.VariationsTyp)
                    {
                        case 0:
                            {
                                // Datei einlesen
                                var inputDirectory = _initialDirectory + @"\Beispiele\Wärmeberechnung\instationär\Anregungsdateien";
                                const int spalte = 0;
                                AusDatei(inputDirectory, spalte, last, _modell);
                                break;
                            }
                        case 1:
                            {
                                StückweiseLinear(item.Value.Intervall, last, _modell);
                                break;
                            }
                        case 2:
                            {
                                Periodisch(item.Value.Amplitude, item.Value.Frequenz, item.Value.PhasenWinkel, last, _modell);
                                break;
                            }
                    }
                    for (var k = 0; k < nZeitschritte; k++) temperatur[k][lastIndex[0]] = last[k];
                }
                else
                    throw new BerechnungAusnahme("\nKnoten " + item.Value.KnotenId + " für zeitabhängige Knotenlast ist nicht im Modell enthalten.");
            }

            // finde zeitabhängige Elementlasten
            foreach (var zeitabhängigeElementLast in _modell.ZeitabhängigeElementLasten.Select(item => item.Value))
            {
                if (!_modell.Elemente.TryGetValue(zeitabhängigeElementLast.ElementId, out var abstraktElement))
                {
                    throw new BerechnungAusnahme("\nzeitabhängige Elementlast '" + zeitabhängigeElementLast.ElementId + "' nicht definiert.");
                }

                var index = abstraktElement.SystemIndizesElement;
                var lastVektor = zeitabhängigeElementLast.BerechneLastVektor();
                _systemGleichungen.AddVektor(index, lastVektor);
                for (var k = 0; k < nZeitschritte; k++)
                    temperatur[k] = _systemGleichungen.Vektor;
            }
        }
        // zeitabhängige vordefinierte Randbedingungen
        private void BerechneRandbedingungenErsterOrdnung(double[][] temperatur)
        {
            var nZeitschritte = temperatur.Length;
            var vordefinierteTemperatur = new double[nZeitschritte];

            foreach (var item in _modell.ZeitabhängigeRandbedingung)
            {
                if (_modell.Knoten.TryGetValue(item.Value.KnotenId, out _knoten))
                {
                    var lastIndex = _knoten.SystemIndizes;

                    switch (item.Value.VariationsTyp)
                    {
                        case 0:
                            {
                                // Datei einlesen
                                _ = MessageBox.Show("Randbedingung " + item.Key + " Daten aus Datei", "Heat Transfer Analysis");

                                var inputDirectory = _initialDirectory + @"\Beispiele\Wärmeberechnung\instationär\Anregungsdateien";
                                const int spalte = 0;
                                AusDatei(inputDirectory, spalte, vordefinierteTemperatur, _modell);
                                break;
                            }
                        case 1:
                            {
                                // konstant
                                for (var k = 0; k < nZeitschritte; k++)
                                    vordefinierteTemperatur[k] = item.Value.KonstanteTemperatur;
                                break;
                            }
                        case 2:
                            {
                                Periodisch(item.Value.Amplitude, item.Value.Frequenz, item.Value.PhasenWinkel, vordefinierteTemperatur, _modell);
                                break;
                            }
                        case 3:
                            {
                                StückweiseLinear(item.Value.Intervall, vordefinierteTemperatur, _modell);
                                break;
                            }
                    }
                    StatusKnoten(item.Value);
                    for (var k = 0; k < nZeitschritte; k++)
                        temperatur[k][lastIndex[0]] = vordefinierteTemperatur[k];
                }
                else
                    throw new BerechnungAusnahme("\nKnoten " + item.Value.KnotenId + " für zeitabhängige Randbedingung ist nicht im Modell enthalten.");
            }
        }

        // Zeitintegration 2. Ordnung
        public void ZeitintegrationZweiterOrdnung()
        {
            var dt = _modell.Zeitintegration.Dt;
            if (dt == 0)
            {
                throw new BerechnungAusnahme("\nZeitschrittintervall nicht definiert");
            }
            var tmax = _modell.Zeitintegration.Tmax;
            var nZeitschritte = (int)(tmax / dt) + 1;
            var methode = _modell.Zeitintegration.Methode;
            var parameter1 = _modell.Zeitintegration.Parameter1;
            var parameter2 = _modell.Zeitintegration.Parameter2;
            var anregung = new double[nZeitschritte + 1][];
            for (var i = 0; i < (nZeitschritte + 1); i++) anregung[i] = new double[_dimension];
            // berechne diagonale Massenmatrix
            if (!_diagonalMatrix) BerechneDiagonalMatrix();

            // berechne diagonale Dämpfungsmatrix
            var dämpfungsmatrix = BerechneDämpfungsMatrix();

            // berechne zeitabhängige Anregungsfunktion und Randbedingungen
            BerechneAnregungsfunktionZweiterOrdnung(anregung);

            var verformung = new double[nZeitschritte][];
            for (var k = 0; k < nZeitschritte; k++) verformung[k] = new double[_dimension];
            var geschwindigkeit = new double[2][];
            for (var k = 0; k < 2; k++) geschwindigkeit[k] = new double[_dimension];

            SetzRandbedingungenZweiterOrdnung(verformung, geschwindigkeit);
            SetzDynamischenStatusVektor();

            if (_zerlegt)
            {
                NeuberechnungSystemMatrix();
                _zerlegt = false;
            }

            var zeitintegration = new Zeitintegration2OrdnungStatus(_systemGleichungen, dämpfungsmatrix,
                dt, methode, parameter1, parameter2,
                verformung, geschwindigkeit, anregung);
            zeitintegration.Ausführen();

            // speichere Knotenzeitverläufe
            foreach (var item2 in _modell.Knoten)
            {
                _knoten = item2.Value;
                var index = _knoten.SystemIndizes;
                var anzahlKnotenfreiheitsgrade = _knoten.AnzahlKnotenfreiheitsgrade;

                _knoten.KnotenVariable = new double[anzahlKnotenfreiheitsgrade][];
                for (var i = 0; i < anzahlKnotenfreiheitsgrade; i++) _knoten.KnotenVariable[i] = new double[nZeitschritte];
                _knoten.KnotenAbleitungen = new double[anzahlKnotenfreiheitsgrade][];
                for (var i = 0; i < anzahlKnotenfreiheitsgrade; i++) _knoten.KnotenAbleitungen[i] = new double[nZeitschritte];

                // verformung[nZeitschritte][index], geschwindigkeit[2][index], KnotenVariable[index][nZeitschritte]
                for (var i = 0; i < _knoten.AnzahlKnotenfreiheitsgrade; i++)
                {
                    if (_systemGleichungen.Status[index[i]]) continue;
                    for (var k = 0; k < nZeitschritte; k++)
                    {
                        _knoten.KnotenVariable[i][k] = zeitintegration.Verformung[k][index[i]];
                        _knoten.KnotenAbleitungen[i][k] = zeitintegration.Beschleunigung[k][index[i]];
                    }
                }
            }
            _modell.ZeitIntegration = true;
            _ = MessageBox.Show("Zeitverlaufsberechnung 2. Ordnung erfolgreich durchgeführt", "Zeitintegration2Ordnung");
        }
        private double[] BerechneDämpfungsMatrix()
        {
            // falls "Dämpfung" modale Dämpfungsmaße beinhaltet,
            // kann die Dämpfungsmatrix ermittelt werden über die Summe aller berücksichtigten
            // Eigenzustände (s. Clough & Penzien S. 198, 13-37
            // M*(SUM(((2*(xi)n*(omega)n )/(M)n))*phi(n)*(phi)nT)*M
            // wobei M die Massenmatrix ist, (xi)n das modale Dämpfungsmaß,
            // omega(n) eigenfrequenz, (M)n modale Massen und phi die Eigenvektoren
            var dämpfungsMatrix = new double[_dimension];
            if (_modell.Eigenzustand.DämpfungsRaten.Count == 0)
            {
                _ = MessageBox.Show("ungedämpftes System", "BerechneDämpfungsMatrix");
                return dämpfungsMatrix;
            }
            // Eigenberechnung wird für Ermittlung der modalen Dämpfungsmaße benötigt
            if (_modell.Eigen == false)
            {
                Eigenzustände();
                _modell.Eigen = true;
            }
            // modale Dämpfungsmaße werden aus eingelesenen DämpfungsRaten ermittelt
            var modaleDämpfung = new double[_modell.Eigenzustand.AnzahlZustände];
            for (var i = 0; i < _modell.Eigenzustand.DämpfungsRaten.Count; i++)
            {
                modaleDämpfung[i] = ((ModaleWerte)_modell.Eigenzustand.DämpfungsRaten[i]).Dämpfung;
            }
            // ist nur ein Dämpfungsmaß gegeben, werden ALLE Eigenzustände damit belegt
            if (_modell.Eigenzustand.DämpfungsRaten.Count == 1)
            {
                for (var i = 1; i < _modell.Eigenzustand.AnzahlZustände; i++)
                {
                    modaleDämpfung[i] = modaleDämpfung[0];
                }
            }

            double faktor = 0;
            for (var n = 0; n < _modell.Eigenzustand.AnzahlZustände; n++)
            {
                double phinPhinT = 0;
                for (var i = 0; i < _systemGleichungen.DiagonalMatrix.Length; i++)
                {
                    phinPhinT += _modell.Eigenzustand.Eigenvektoren[n][i] * _modell.Eigenzustand.Eigenvektoren[n][i];
                }

                var mn = _systemGleichungen.DiagonalMatrix.Select((t, i) => _modell.Eigenzustand.Eigenvektoren[n][i] * t * _modell.Eigenzustand.Eigenvektoren[n][i]).Sum();

                faktor += 2 * modaleDämpfung[n] * Math.Sqrt(_modell.Eigenzustand.Eigenwerte[n]) / 2 / Math.PI * phinPhinT / mn;
            }
            // diagonale Dämpfungsmatrix wird aus m*faktor*m ermittelt
            for (var i = 0; i < _systemGleichungen.DiagonalMatrix.Length; i++)
            {
                dämpfungsMatrix[i] = _systemGleichungen.DiagonalMatrix[i] * faktor * _systemGleichungen.DiagonalMatrix[i];
            }
            return dämpfungsMatrix;
        }
        private void SetzRandbedingungenZweiterOrdnung(double[][] displ, double[][] veloc)
        {
            // finde vordefinierte Anfangsbedingungen
            foreach (var anf in _modell.Zeitintegration.Anfangsbedingungen)
            {
                if (!_modell.Knoten.TryGetValue(anf.KnotenId, out var anfKnoten))
                    throw new BerechnungAusnahme("\nKnoten " + anf.KnotenId +
                                                 " für vordefinierte Anfangsbedingung ist nicht im Modell enthalten.");
                for (var i = 0; i < anf.Werte.Length / 2; i += 2)
                {
                    foreach (var knotenIndex in anfKnoten.SystemIndizes)
                    {
                        displ[i][knotenIndex] = anf.Werte[i];
                        veloc[i + 1][knotenIndex] = anf.Werte[i + 1];
                    }
                }
            }
        }
        private void SetzDynamischenStatusVektor()
        {
            // für alle zeitabhängigen Randbedingungen
            foreach (var randbedingung in
                _modell.ZeitabhängigeRandbedingung.Select(item => item.Value))
            {
                StatusKnoten(randbedingung);
            }
        }

        // zeitabhängige Knoteneinwirkungen
        private void BerechneAnregungsfunktionZweiterOrdnung(double[][] anregung)
        {
            // finde zeitabhängige Knoteneinwirkungen
            foreach (var item in _modell.ZeitabhängigeKnotenLasten)
            {
                var last = new double[anregung.Length];
                switch (item.Value.VariationsTyp)
                {
                    case 0:
                        {
                            var inputDirectory = _initialDirectory + @"\Beispiele\Tragwerksberechnung\Dynamik\Anregungsdateien";
                            const int col = -1; // ALLE Values in Datei
                                                // Ordinatenwerte im Zeitintervall dt aus Datei lesen
                            AusDatei(inputDirectory, col, last, _modell);
                            break;
                        }
                    case 1:
                        {
                            var intervall = item.Value.Intervall;
                            // lineare Interpolation der abschnittweise linearen Eingabedaten im Zeitintervall dt
                            StückweiseLinear(intervall, last, _modell);
                            break;
                        }
                    case 2:
                        {
                            var amplitude = item.Value.Amplitude;
                            var frequenz = 2 * Math.PI * item.Value.Frequenz;
                            var phasenWinkel = Math.PI / 180 * item.Value.PhasenWinkel;
                            // periodische Anregung mit Ausgabe "last" im Zeitintervall dt
                            Periodisch(amplitude, frequenz, phasenWinkel, last, _modell);
                            break;
                        }
                }

                if (item.Value.Bodenanregung)
                {
                    var knotenFreiheitsgrad = item.Value.KnotenFreiheitsgrad;

                    var masse = _systemGleichungen.DiagonalMatrix;
                    foreach (var index in _modell.Knoten.Select(item2 =>
                                 item2.Value.SystemIndizes).Where(index =>
                                 !_systemGleichungen.Status[index[knotenFreiheitsgrad]]))
                    {
                        for (var k = 0; k < anregung.Length; k++)
                            anregung[k][index[knotenFreiheitsgrad]] =
                                -masse[index[knotenFreiheitsgrad]] * last[k];
                    }
                }

                else
                {
                    if (!_modell.Knoten.TryGetValue(item.Value.KnotenId, out _knoten))
                    {
                        throw new BerechnungAusnahme("\nKnoten " + item.Value.KnotenId +
                                                     " für zeitabhängige Knotenlast ist nicht im Modell enthalten.");
                    }
                    var index = _knoten.SystemIndizes;
                    var knotenFreiheitsgrad = item.Value.KnotenFreiheitsgrad;

                    for (var k = 0; k < anregung.Length; k++)
                        for (var j = 0; j < anregung[0].Length; j++)
                            anregung[k][index[knotenFreiheitsgrad]] = last[k];
                }
            }
        }

        // zeitabhängige Eingabedaten

        // Werte aus einer Spalte/alle Spalten(-spalte) einer auszuwählenden Datei lesen
        // Lastfunktion in rufendem Programm initialisiert, Zeitschritt dt aus Modell oder Dateinamen 
        // dt wird ggf. angepasst auf Daten in Anregungsdatei
        public static void AusDatei(string inputDirectory, int spalte, double[] last, FeModell modell)
        {
            var datei = new OpenFileDialog
            {
                InitialDirectory = inputDirectory
            };
            if (datei.ShowDialog() != true) return;

            var pfad = datei.FileName;

            // Datei enthält nur Anregungswerte im VORGEGEBENEM ZEITSCHRITT dt
            // lies dt aus Dateinamen
            char[] delimiters = ['.'];
            var name = pfad.Split(delimiters);
            if (name.Length > 1)
            {
                var dt = double.Parse(name[1]);
                if (Math.Abs(dt - modell.Zeitintegration.Dt) > double.Epsilon)
                {
                    modell.Zeitintegration.Dt = dt;
                    _ = MessageBox.Show("Zeitschritt dt im Modell angepasst auf Daten in Anregungsdatei \ndt = "
                                        + modell.Zeitintegration.Dt, " Berechnung.AusDatei");
                }
            }

            string[] zeilen;
            delimiters = ['\t'];
            try
            {
                zeilen = File.ReadAllLines(pfad);
            }
            catch (IOException ex)
            {
                _ = MessageBox.Show(ex + " Anregungsfunktion konnte nicht aus Datei gelesen werden!!!", "Berechnung.AusDatei");
                return;
            }

            if (spalte < 0)
            {
                // lies alle Werte einer Datei
                var zähler = 0;
                foreach (var zeile in zeilen)
                {
                    var substrings = zeile.Split(delimiters);
                    // lies nur so viel Werte, wie in last[] gespeichert werden können
                    if ((zähler + substrings.Length) >= last.Length) break;
                    foreach (var wert in substrings)
                    {
                        last[zähler] = double.Parse(wert);
                        zähler++;
                    }
                }
            }
            else
            {
                // lies alle Werte einer bestimmten Spalte [0-n]
                var anzahl = last.Length;
                if (zeilen.Length < anzahl) anzahl = zeilen.Length;
                for (var i = 0; i < anzahl; i++)
                {
                    var zeile = zeilen[i].Split(delimiters);
                    last[i] = double.Parse(zeile[spalte]);
                }
            }
        }
        // alle Werte aus einer auszuwählenden Datei lesen
        // Zeitschritt dt und Maximalzeit tmax aus Modell oder Dateinamen
        // Lastfunktion wird neu initialisiert (AnzahlZeitschritte im Modell) und zurückgegeben
        // dt wird ggf. angepasst auf Daten in Anregungsdatei
        public static double[] AusDatei(string inputDirectory, FeModell modell)
        {
            var tmax = modell.Zeitintegration.Tmax;
            var nZeitschritte = (int)(tmax / modell.Zeitintegration.Dt) + 1;
            var last = new double[nZeitschritte];

            var datei = new OpenFileDialog
            {
                InitialDirectory = inputDirectory
            };

            if (datei.ShowDialog() != true) return null;
            var pfad = datei.FileName;

            // Anregungsfunktion[nZeitschritte]
            // Datei enthält nur Anregungswerte im VORGEGEBENEM ZEITSCHRITT dt
            // lies tmax und dt aus Dateinamen
            char[] delimiters = ['.'];
            var name = pfad.Split(delimiters);

            if (name.Length > 1)
            {
                var dt = double.Parse(name[1]);
                if (Math.Abs(dt - modell.Zeitintegration.Dt) > double.Epsilon)
                {
                    modell.Zeitintegration.Dt = dt;
                    _ = MessageBox.Show("Zeitschritt dt im Modell angepasst auf Daten in Anregungsdatei \ndt = "
                                        + modell.Zeitintegration.Dt, " Berechnung.AusDatei");
                }
            }

            delimiters = ['\t'];
            try
            {
                // lies alle Werte einer Datei
                var zeilen = File.ReadAllLines(pfad);
                var zähler = 0;
                foreach (var zeile in zeilen)
                {
                    var substrings = zeile.Split(delimiters);
                    foreach (var wert in substrings)
                    {
                        last[zähler] = double.Parse(wert);
                        zähler++;
                    }
                }
            }
            catch (IOException ex)
            {
                _ = MessageBox.Show(ex + " Anregungsfunktion konnte nicht aus Datei gelesen werden!!!", "Berechnung.AusDatei");
            }
            return last;
        }

        // Zeitschritt dt und Maximalzeit tmax aus Modell 
        // Werte aus Übergabeparameter Intervall (stückweise lineare Intervalle) berechnen
        // dimensionierte Lastfunktion als Übergabeparameter
        public static void StückweiseLinear(double[] intervall, double[] last, FeModell modell)
        {
            var nZeitschritte = (int)(modell.Zeitintegration.Tmax / modell.Zeitintegration.Dt) + 1;
            var zähler = 0;
            double endLast = 0;
            var startZeit = intervall[0];
            var startLast = intervall[1];
            last[zähler] = startLast;
            for (var j = 2; j < intervall.Length; j += 2)
            {
                var endZeit = intervall[j];
                endLast = intervall[j + 1];
                var schritteJeIntervall = (int)(Math.Round((endZeit - startZeit) / modell.Zeitintegration.Dt));
                var inkrement = (endLast - startLast) / schritteJeIntervall;
                for (var k = 1; k <= schritteJeIntervall; k++)
                {
                    zähler++;
                    if (zähler == nZeitschritte) return;
                    last[zähler] = last[zähler - 1] + inkrement;
                }
                startZeit = endZeit;
                startLast = endLast;
            }
            for (var k = zähler + 1; k < nZeitschritte; k++) last[k] = endLast;
        }

        // Zeitschritt dt und Maximalzeit tmax als Übergabeparameter,
        // Werte aus Übergabeparameter Intervall (stückweise lineare Intervalle) berechnen
        // Lastfunktion wird neu erzeugt und zurückgegeben
        public static double[] StückweiseLinear(double dt, double tmax, double[] intervall)
        {
            var nZeitschritte = (int)(tmax / dt) + 1;
            var last = new double[nZeitschritte];
            var startZeit = intervall[0];
            var startLast = intervall[1];

            var zähler = 0;
            last[zähler] = startLast;
            for (var j = 2; j < intervall.Length; j += 2)
            {
                var endZeit = intervall[j];
                var endLast = intervall[j + 1];
                var schritteJeIntervall = (int)(Math.Round((endZeit - startZeit) / dt));
                var inkrement = (endLast - startLast) / schritteJeIntervall;
                for (var k = 1; k <= schritteJeIntervall; k++)
                {
                    zähler++;
                    if (zähler == nZeitschritte) return null;
                    last[zähler] = last[zähler - 1] + inkrement;
                }
                startZeit = endZeit;
                startLast = endLast;
            }
            return last;
        }

        // Zeitschritt dt und Maximalzeit tmax aus Modell
        // Werte aus Übergabeparameter (Amplitude, Frequenz, Phasenwinkel) als periodische Anregung berechnen
        // dimensionierte Lastfunktion als Übergabeparameter
        public static void Periodisch(double amplitude, double frequenz, double phasenWinkel, double[] last, FeModell modell)
        {
            var tmax = modell.Zeitintegration.Tmax;
            var nZeitschritte = (int)(tmax / modell.Zeitintegration.Dt) + 1;
            double zeit = 0;
            for (var k = 0; k < nZeitschritte; k++)
            {
                last[k] = amplitude * Math.Sin(frequenz * zeit + phasenWinkel);
                zeit += modell.Zeitintegration.Dt;
            }
        }
        // Zeitschritt dt und Maximalzeit tmax als Übergabeparameter,
        // Werte aus Übergabeparameter (Amplitude, Frequenz, Phasenwinkel) als periodische Anregung berechnen
        // Lastfunktion wird neu erzeugt und zurückgegeben
        public static double[] Periodisch(double dt, double tmax, double amplitude, double frequenz, double phasenWinkel)
        {
            var nZeitschritte = (int)(tmax / dt) + 1;
            double zeit = 0;
            var last = new double[nZeitschritte];
            for (var k = 0; k < nZeitschritte; k++)
            {
                last[k] = amplitude * Math.Sin(frequenz * zeit + phasenWinkel);
                zeit += dt;
            }
            return last;
        }
    }
}