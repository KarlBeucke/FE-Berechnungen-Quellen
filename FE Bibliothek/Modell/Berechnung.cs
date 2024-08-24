using FEBibliothek.Gleichungslöser;
using FEBibliothek.Modell.abstrakte_Klassen;
using FEBibliothek.Zeitlöser;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;

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

        public Berechnung(FeModell m)
        {
            _modell = m;
            if (_modell == null)
            {
                throw new BerechnungAusnahme("\nModelleingabedaten noch nicht eingelesen");
            }
            // setz System Indizes
            var k = 0;
            foreach (var item in _modell.Knoten)
            {
                _knoten = item.Value;
                k = _knoten.SetzSystemIndizes(k);
            }
            SetzReferenzen(m);
            FreieKnoten();
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
                var linienlast = (AbstraktLinienlast)last;
                linienlast.SetzLinienlastReferenzen(_modell);
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
            // check alle Knoten, ob sie Steifigkeit durch ein Element erhalten
            foreach (var id in _modell.Knoten.Select(knoten => knoten.Key))
            {
                if (_modell.Elemente.Select((_, i)
                        => _modell.Elemente.ElementAt(i)).Any(element
                        => element.Value.KnotenIds[0] == id || element.Value.KnotenIds[1] == id)) continue;
                throw new BerechnungAusnahme("\nKnoten " + id + " ist instabil, wird durch kein Element genutzt");
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
                    if (festgehalten[i])
                        _systemGleichungen.SetzStatus(true, _knoten.SystemIndizes[i], vordefiniert[i]);
                }
            }
            else
            {
                throw new BerechnungAusnahme("\nEndknoten " + knotenId + " ist nicht im Modell enthalten.");
            }
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
            foreach (var item in _modell.ElementLasten)
            {
                var linienLast = (AbstraktLinienlast)item.Value;
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
                {
                    throw new BerechnungAusnahme("\nElement " + elementId + " für Linienlasten ist nicht im Modell enthalten.");
                }
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

        // Zeitintegration 1er Ordnung
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
            BerechneAnregungsfunktionErsterOrdnung(dt, anregungsFunktion);
            BerechneRandbedingungenErsterOrdnung(dt, temperatur);

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
        private void SetzAnfangsbedingungenErsterOrdnung(IList<double[]> temperatur)
        {
            // setz stationäre Lösung als Anfangsbedingungen
            if (_modell.Zeitintegration.VonStationär) { temperatur[0] = _systemGleichungen.Primal; }
            foreach (Knotenwerte anf in _modell.Zeitintegration.Anfangsbedingungen)
            {
                if (anf.KnotenId == "alle")
                {
                    for (var i = 0; i < _dimension; i++) temperatur[0][i] = anf.Werte[0];
                }
                else
                {
                    if (!_modell.Knoten.TryGetValue(anf.KnotenId, out var anfKnoten))
                    {
                        throw new BerechnungAusnahme("\nKnoten " + anf.KnotenId + " für zeitabhängige Anfangsbedingung ist nicht im Modell enthalten.");
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
        private void BerechneAnregungsfunktionErsterOrdnung(double dt, double[][] temperatur)
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
                                const string inputDirectory = @"\FE-Berechnungen\input\Wärmeberechnung\instationär\Anregungsdateien";
                                const int spalte = -1;
                                AusDatei(inputDirectory, spalte, last);
                                break;
                            }
                        case 1:
                            {
                                // stückweise linear
                                var interval = item.Value.Intervall;
                                StückweiseLinear(dt, interval, last);
                                break;
                            }
                        case 2:
                            {
                                // periodisch
                                var amplitude = item.Value.Amplitude;
                                var frequenz = item.Value.Frequenz;
                                var phasenWinkel = item.Value.PhasenWinkel;
                                Periodisch(dt, amplitude, frequenz, phasenWinkel, last);
                                break;
                            }
                    }
                    for (var k = 0; k < nZeitschritte; k++)
                        temperatur[k][lastIndex[0]] = last[k];
                }
                else
                {
                    throw new BerechnungAusnahme("\nKnoten " + item.Value.KnotenId + " für zeitabhängige Knotenlast ist nicht im Modell enthalten.");
                }
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
        private void BerechneRandbedingungenErsterOrdnung(double dt, double[][] temperatur)
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

                                const string inputDirectory = @"\FE-Berechnungen\input\Wärmeberechnung\instationär\Anregungsdateien";
                                const int spalte = 1;
                                AusDatei(inputDirectory, spalte, vordefinierteTemperatur);
                                break;
                            }
                        case 1:
                            {
                                // konstant
                                for (var k = 0; k < nZeitschritte; k++)
                                {
                                    vordefinierteTemperatur[k] = item.Value.KonstanteTemperatur;
                                }
                                break;
                            }
                        case 2:
                            {
                                // periodisch
                                var amplitude = item.Value.Amplitude;
                                var frequenz = item.Value.Frequenz;
                                var phasenWinkel = item.Value.PhasenWinkel;
                                Periodisch(dt, amplitude, frequenz, phasenWinkel, vordefinierteTemperatur);
                                break;
                            }
                        case 3:
                            {
                                // stückweise linear
                                var intervall = item.Value.Intervall;
                                StückweiseLinear(dt, intervall, vordefinierteTemperatur);
                                break;
                            }
                    }
                    StatusKnoten(item.Value);
                    for (var k = 0; k < nZeitschritte; k++)
                        temperatur[k][lastIndex[0]] = vordefinierteTemperatur[k];
                }
                else
                {
                    throw new BerechnungAusnahme("\nKnoten " + item.Value.KnotenId + " für zeitabhängige Randbedingung ist nicht im Modell enthalten.");
                }
            }
        }

        // 2nd order time integration
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
            BerechneAnregungsfunktionZweiterOrdnung(dt, anregung);

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
            for (var i = _modell.Eigenzustand.DämpfungsRaten.Count; i < _modell.Eigenzustand.AnzahlZustände; i++)
            {
                modaleDämpfung[i] = modaleDämpfung[0];
            }

            double faktor = 0;
            for (var n = 0; n < _modell.Eigenzustand.AnzahlZustände; n++)
            {
                double phinPhinT = 0;
                double mn = 0;
                for (var i = 0; i < _systemGleichungen.DiagonalMatrix.Length; i++)
                {
                    phinPhinT += _modell.Eigenzustand.Eigenvektoren[n][i] * _modell.Eigenzustand.Eigenvektoren[n][i];
                }

                for (var i = 0; i < _systemGleichungen.DiagonalMatrix.Length; i++)
                {
                    mn += _modell.Eigenzustand.Eigenvektoren[n][i] * _systemGleichungen.DiagonalMatrix[i] * _modell.Eigenzustand.Eigenvektoren[n][i];
                }

                faktor += 2 * modaleDämpfung[n] * Math.Sqrt(_modell.Eigenzustand.Eigenwerte[n]) / 2 / Math.PI * phinPhinT / mn;
            }
            // diagonale Dämpfungsmatrix wird aus m*faktor*m ermittelt
            for (var i = 0; i < _systemGleichungen.DiagonalMatrix.Length; i++)
            {
                dämpfungsMatrix[i] = _systemGleichungen.DiagonalMatrix[i] * faktor * _systemGleichungen.DiagonalMatrix[i];
            }
            return dämpfungsMatrix;
        }

        private void SetzRandbedingungenZweiterOrdnung(IReadOnlyList<double[]> displ, IReadOnlyList<double[]> veloc)
        {
            // finde vordefinierte Anfangsbedingungen
            foreach (Knotenwerte anf in _modell.Zeitintegration.Anfangsbedingungen)
            {
                if (!_modell.Knoten.TryGetValue(anf.KnotenId, out var anfKnoten))
                {
                    throw new BerechnungAusnahme("\nKnoten " + anf.KnotenId + " für vordefinierte Anfangsbedingung ist nicht im Modell enthalten.");
                }
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
        private void BerechneAnregungsfunktionZweiterOrdnung(double dt, IReadOnlyList<double[]> anregung)
        {
            // finde zeitabhängige Knoteneinwirkungen
            foreach (var item in _modell.ZeitabhängigeKnotenLasten)
            {
                var force = new double[anregung.Count];
                switch (item.Value.VariationsTyp)
                {
                    case 0:
                        {
                            const string inputDirectory = @"\FE-Berechnungen\input\Tragwerksberechnung\Dynamik\Anregungsdateien";
                            const int col = -1; // ALLE Values in Datei
                                                // Ordinatenwerte im Zeitintervall dt aus Datei lesen
                            AusDatei(inputDirectory, col, force);
                            break;
                        }
                    case 1:
                        {
                            var intervall = item.Value.Intervall;
                            // lineare Interpolation der abschnittweise linearen Eingabedaten im Zeitintervall dt
                            StückweiseLinear(dt, intervall, force);
                            break;
                        }
                    case 2:
                        {
                            var amplitude = item.Value.Amplitude;
                            var frequenz = 2 * Math.PI * item.Value.Frequenz;
                            var phasenWinkel = Math.PI / 180 * item.Value.PhasenWinkel;
                            // periodische Anregung mit Ausgabe "force" im Zeitintervall dt
                            Periodisch(dt, amplitude, frequenz, phasenWinkel, force);
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
                        for (var k = 0; k < anregung.Count; k++)
                            anregung[k][index[knotenFreiheitsgrad]] = -masse[index[knotenFreiheitsgrad]] * force[k];
                    }
                }

                else
                {
                    if (!_modell.Knoten.TryGetValue(item.Value.KnotenId, out _knoten))
                    {
                        throw new BerechnungAusnahme("\nKnoten " + item.Value.KnotenId + " für zeitabhängige Knotenlast ist nicht im Modell enthalten.");
                    }
                    var index = _knoten.SystemIndizes;
                    var knotenFreiheitsgrad = item.Value.KnotenFreiheitsgrad;

                    for (var k = 0; k < anregung.Count; k++)
                        for (var j = 0; j < anregung[0].Length; j++)
                            anregung[k][index[knotenFreiheitsgrad]] = force[k];
                }
            }
        }

        // zeitabhängige Eingabedaten
        public static void AusDatei(string inputDirectory, int spalte, IList<double> last)
        {
            string[] zeilen, substrings;
            var delimiters = new[] { '\t' };

            var datei = new OpenFileDialog
            {
                Filter = "All files (*.*)|*.*",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
            };
            datei.InitialDirectory += inputDirectory;

            if (datei.ShowDialog() != true)
                return;
            var pfad = datei.FileName;

            try
            {
                zeilen = File.ReadAllLines(pfad);
            }
            catch (IOException ex)
            {
                _ = MessageBox.Show(ex + " Anregungsfunktion konnte nicht aus Datei gelesen werden!!!", "Berechnung.AusDatei");
                return;
            }
            // Anregungsfunktion[timeSteps]
            // Datei enthält nur Anregungswerte im VORGEGEBENEM ZEITSCHRITT dt
            var werte = new List<double>();
            if (spalte < 0)
            {
                // lies alle Werte einer Datei
                foreach (var zeile in zeilen)
                {
                    substrings = zeile.Split(delimiters);
                    werte.AddRange(substrings.Select(double.Parse));
                }
            }
            else
            {
                // lies alle Werte einer bestimmten Spalte col [][0-n]
                foreach (var zeile in zeilen)
                {
                    substrings = zeile.Split(delimiters);
                    werte.Add(double.Parse(substrings[spalte - 1]));
                }
            }
            if (werte.Count <= last.Count)
            {
                for (var i = 0; i < werte.Count; i++) { last[i] = werte[i]; }
            }
            else
            {
                for (var i = 0; i < last.Count; i++) { last[i] = werte[i]; }
            }
        }
        public static List<double> AusDatei(string inputDirectory)
        {
            var delimiters = new[] { '\t' };
            var werte = new List<double>();

            var datei = new OpenFileDialog
            {
                Filter = "All files (*.*)|*.*",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
            };
            datei.InitialDirectory += inputDirectory;

            if (datei.ShowDialog() != true)
                return werte;
            var pfad = datei.FileName;

            try
            {
                var zeilen = File.ReadAllLines(pfad);
                foreach (var zeile in zeilen)
                {
                    var substrings = zeile.Split(delimiters);
                    werte.AddRange(substrings.Select(double.Parse));
                }
            }
            catch (IOException ex)
            {
                _ = MessageBox.Show(ex + " Anregungsfunktion konnte nicht aus Datei gelesen werden!!!", "Analysis FromFile");
                return werte;
            }
            return werte;
        }
        private static void StückweiseLinear(double dt, IReadOnlyList<double> intervall, IList<double> last)
        {
            int zähler = 0, nZeitschritte = last.Count;
            double endLast = 0;
            var startZeit = intervall[0];
            var startLast = intervall[1];
            last[zähler] = startLast;
            for (var j = 2; j < intervall.Count; j += 2)
            {
                var endZeit = intervall[j];
                endLast = intervall[j + 1];
                var schritteJeIntervall = (int)(Math.Round((endZeit - startZeit) / dt));
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
        private static void Periodisch(double dt, double amplitude, double frequenz, double phasenWinkel, double[] last)
        {
            var nZeitschritte = last.GetLength(0);
            double zeit = 0;
            for (var k = 0; k < nZeitschritte; k++)
            {
                last[k] = amplitude * Math.Sin(frequenz * zeit + phasenWinkel);
                zeit += dt;
            }
        }
    }
}